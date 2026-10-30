"""Run with the installed Blender Python. Stage repairs before replacing assets.

FBX repair edits only polygon topology and its mapped attributes, preserving
object IDs, transforms, original vertex coordinates, material links and names.
Original files and Unity metadata are backed up outside Assets.
"""
import array
import collections
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import sys

import bpy
import bmesh
from io_scene_fbx import parse_fbx, encode_bin

ROOT = Path.cwd().resolve()
STAGE = ROOT / 'Temp/ImportRepairStage'
BACKUP = ROOT / 'SourceAssets/ImportFixBackups/2026-10-30'
ARGS = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
APPLY = '--apply' in ARGS
report = []

def child(node, name):
    return next((n for n in node.elems if n.id == name), None)

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def serialize(node):
    out = encode_bin.FBXElem(node.id)
    writers = {
        'B':'add_bool','C':'add_char','Z':'add_int8','Y':'add_int16',
        'I':'add_int32','L':'add_int64','F':'add_float32','D':'add_float64',
        'R':'add_bytes','S':'add_string','i':'add_int32_array',
        'l':'add_int64_array','f':'add_float32_array','d':'add_float64_array',
        'b':'add_bool_array','c':'add_byte_array',
    }
    for typ, prop in zip(node.props_type, node.props):
        getattr(out, writers[chr(typ)])(prop)
    out.elems = [serialize(n) for n in node.elems]
    return out

def remap_array(node, mapping, count):
    values = node.props[0]
    assert len(values) % count == 0, (node.id, len(values), count)
    stride = len(values) // count
    node.props[0] = array.array(values.typecode,
        (values[i*stride+j] for i in mapping for j in range(stride)))

def triangles(vertices, faces):
    mesh = bpy.data.meshes.new('__ImportRepair')
    mesh.from_pydata(vertices, [], faces)
    mesh.calc_loop_triangles()
    assert list(v.vertex_index for v in mesh.loops) == [v for f in faces for v in f]
    result = [(tuple(t.loops), t.polygon_index) for t in mesh.loop_triangles]
    assert len(result) == sum(len(f)-2 for f in faces)
    bpy.data.meshes.remove(mesh)
    return result

def repair_binary_fbx(source, dest):
    tree, version = parse_fbx.parse(str(source))
    geometries = child(tree, b'Objects').elems
    stats = []
    for geo in geometries:
        if geo.id != b'Geometry' or geo.props[-1] != b'Mesh':
            continue
        vertex_node = child(geo, b'Vertices')
        polygon_node = child(geo, b'PolygonVertexIndex')
        vertices = list(zip(*[iter(vertex_node.props[0])]*3))
        flat, faces, face = [], [], []
        for value in polygon_node.props[0]:
            vertex = value if value >= 0 else ~value
            flat.append(vertex); face.append(vertex)
            if value < 0:
                faces.append(face); face = []
        assert not face
        tris = triangles(vertices, faces)
        loops = [i for t, _ in tris for i in t]
        polys = [p for _, p in tris]

        # Rebuild the optional edge table and retain per-edge properties.
        old_edges = child(geo, b'Edges')
        old_edge_lookup = {}
        next_loop = {}
        offset = 0
        for f in faces:
            for i in range(len(f)): next_loop[offset+i] = offset+(i+1)%len(f)
            offset += len(f)
        if old_edges:
            for edge_id, loop in enumerate(old_edges.props[0]):
                old_edge_lookup[tuple(sorted((flat[loop], flat[next_loop[loop]])))] = edge_id
        new_flat = [flat[i] for i in loops]
        new_edges, edge_pairs, seen = [], [], set()
        for i in range(0,len(new_flat),3):
            for j in range(3):
                pair = tuple(sorted((new_flat[i+j], new_flat[i+(j+1)%3])))
                if pair not in seen:
                    seen.add(pair); new_edges.append(i+j); edge_pairs.append(pair)

        for layer in geo.elems:
            if not layer.id.startswith(b'LayerElement'): continue
            mapping_node = child(layer,b'MappingInformationType')
            reference_node = child(layer,b'ReferenceInformationType')
            if not mapping_node: continue
            mapping = mapping_node.props[0]
            reference = reference_node.props[0] if reference_node else b'Direct'
            if mapping in (b'ByVertice', b'ByVertex', b'ByControlPoint', b'AllSame', b'None'):
                continue
            assert mapping in (b'ByPolygonVertex',b'ByPolygon',b'ByEdge'), (layer.id,mapping)
            for data in layer.elems:
                if not data.props or not isinstance(data.props[0],array.array): continue
                # With IndexToDirect, only the index array is topology-mapped.
                if reference in (b'IndexToDirect',b'Index') and not (data.id.endswith(b'Index') or data.id.endswith(b'Indices') or data.id == b'Materials'):
                    continue
                if mapping == b'ByPolygonVertex': remap_array(data,loops,len(flat))
                elif mapping == b'ByPolygon': remap_array(data,polys,len(faces))
                else:
                    assert old_edges and len(data.props[0]) == len(old_edges.props[0])
                    old_values = data.props[0]
                    data.props[0] = array.array(old_values.typecode,
                        (old_values[old_edge_lookup[p]] if p in old_edge_lookup else 1 for p in edge_pairs))
        polygon_node.props[0] = array.array(polygon_node.props[0].typecode, (v if i%3 != 2 else ~v for i,v in enumerate(new_flat)))
        if old_edges: old_edges.props[0] = array.array(old_edges.props[0].typecode,new_edges)
        stats.append(dict(mesh=geo.props[1].decode(errors='replace'),vertices=len(vertices),before_faces=len(faces),triangles=len(tris)))
    encode_bin.write(str(dest),serialize(tree),version)
    check,_ = parse_fbx.parse(str(dest))
    # All original vertex buffers, IDs and transforms remain untouched.
    for a,b in zip(child(tree,b'Objects').elems,child(check,b'Objects').elems):
        assert a.props == b.props
        if a.id == b'Geometry':
            assert child(a,b'Vertices').props == child(b,b'Vertices').props
    return stats

def repair_blend(source,dest):
    bpy.ops.wm.open_mainfile(filepath=str(source))
    result=[]
    for mesh in bpy.data.meshes:
        before=[tuple(v.co) for v in mesh.vertices]
        bm=bmesh.new(); bm.from_mesh(mesh)
        before_faces=len(bm.faces)
        bmesh.ops.triangulate(bm,faces=list(bm.faces),quad_method='BEAUTY',ngon_method='BEAUTY')
        bm.to_mesh(mesh); bm.free(); mesh.update()
        assert all(len(p.vertices)==3 for p in mesh.polygons)
        assert before == [tuple(v.co) for v in mesh.vertices]
        result.append(dict(mesh=mesh.name,vertices=len(before),before_faces=before_faces,triangles=len(mesh.polygons)))
    bpy.context.preferences.filepaths.save_version=0
    if hasattr(bpy.context.preferences.filepaths,'use_save_preview_images'):
        bpy.context.preferences.filepaths.use_save_preview_images=False
    bpy.ops.wm.save_as_mainfile(filepath=str(dest),check_existing=False)
    return result

def repair_obj(source,dest):
    lines=source.read_text(encoding='utf-8-sig').splitlines(keepends=True)
    vertices=[]; faces=[]; tokens=[]; face_lines=[]
    for n,line in enumerate(lines):
        if line.startswith('v '): vertices.append(tuple(map(float,line.split()[1:4])))
        elif line.startswith('f '):
            parts=line.split()[1:]; tokens.append(parts); face_lines.append(n)
            face=[]
            for token in parts:
                i=int(token.split('/')[0]); face.append(i-1 if i>0 else len(vertices)+i)
            faces.append(face)
    tris=triangles(vertices,faces)
    global_tokens=[t for ts in tokens for t in ts]
    by_polygon=collections.defaultdict(list)
    for loops,p in tris: by_polygon[p].append('f '+' '.join(global_tokens[i] for i in loops)+'\n')
    for p,n in enumerate(face_lines): lines[n]=''.join(by_polygon[p])
    dest.write_text(''.join(lines),encoding='utf-8')
    return dict(vertices=len(vertices),before_faces=len(faces),triangles=len(tris))

def unique_names(source,dest):
    text=source.read_text(encoding='utf-8-sig')
    seen=collections.Counter(); changes=[]
    def name_once(name):
        seen[name]+=1
        new=name if seen[name]==1 else f'{name}_{seen[name]:02}'
        if name!=new:changes.append([name,new])
        return new
    if source.suffix=='.fbx':
        text=re.sub(r'(Model:\s*\d+,\s*"Model::)([^"\r\n]+)(",\s*"Mesh")',lambda m:m[1]+name_once(m[2])+m[3],text)
        names={m[1]:m[2] for m in re.finditer(r'Model:\s*(\d+),\s*"Model::([^"\r\n]+)",\s*"Mesh"',text)}
        links={m[1]:m[2] for m in re.finditer(r'C:\s*"OO",\s*(\d+),\s*(\d+)',text)}
        def geometry_name(m):
            name=names.get(links.get(m[1],''),f'Mesh_{m[1]}')
            return f'Geometry: {m[1]}, "Geometry::{name}", "Mesh"'
        text=re.sub(r'Geometry:\s*(\d+),\s*"Geometry::[^"\r\n]*",\s*"Mesh"',geometry_name,text)
    else:
        text=re.sub(r'(<node\b[^>]*\bname=")([^"]+)(")',lambda m:m[1]+name_once(m[2])+m[3],text)
        # Geometry names must also be explicit because Unity creates Mesh subassets.
        node_names={}
        for m in re.finditer(r'<node\b[^>]*name="([^"]+)"[^>]*>(.*?)</node>',text,re.S):
            for geom in re.findall(r'<instance_geometry\s+url="#([^"]+)"',m[2]):node_names[geom]=m[1]
        def geometry_name(m):
            attrs=m[1]; ident=re.search(r'\bid="([^"]+)"',attrs)[1]
            name=node_names.get(ident,f'Mesh_{ident}')
            attrs=re.sub(r'\s+name="[^"]*"','',attrs)
            return '<geometry'+attrs+f' name="{name}">'
        text=re.sub(r'<geometry\b([^>]*)>',geometry_name,text)
    dest.write_text(text,encoding='utf-8')
    return dict(renamed=changes)

paths=json.loads((Path(__file__).resolve().parent/'warning_model_paths.json').read_text())
dupes=json.loads((Path(__file__).resolve().parent/'duplicate_model_paths.json').read_text())
if '--one' in ARGS: paths=paths[:1]; dupes=[]
for relative in paths+dupes:
    source=(ROOT/relative).resolve()
    assert source.is_relative_to(ROOT/'Assets')
    dest=STAGE/relative; dest.parent.mkdir(parents=True,exist_ok=True)
    if relative in dupes: stats=unique_names(source,dest)
    elif source.suffix=='.fbx': stats=repair_binary_fbx(source,dest)
    elif source.suffix=='.blend': stats=repair_blend(source,dest)
    else: stats=repair_obj(source,dest)
    report.append(dict(path=relative,before_sha256=digest(source),after_sha256=digest(dest),stats=stats))
    print('STAGED',relative,flush=True)

if APPLY:
    for entry in report:
        relative=entry['path']; source=ROOT/relative; dest=STAGE/relative
        assert digest(source)==entry['before_sha256'],f'Source changed while repairing: {relative}'
        backup=BACKUP/relative; backup.parent.mkdir(parents=True,exist_ok=True)
        assert not backup.exists(),f'Backup already exists; inspect before overwriting {backup}'
        shutil.copy2(source,backup)
        meta=Path(str(source)+'.meta')
        if meta.exists():shutil.copy2(meta,Path(str(backup)+'.meta'))
        os.replace(dest,source)
    (BACKUP/'repair_manifest.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    print('APPLIED',len(report),'files; originals and metadata preserved in',BACKUP,flush=True)
(ROOT/'Temp/import_repair_report.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
