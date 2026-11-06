import bpy, os, json

ROOT=os.path.abspath(os.path.join(os.path.dirname(__file__),'../..'))
OUT=os.path.join(ROOT,'Assets/_Project/Art/Chapter01Revision/Models')
os.makedirs(OUT,exist_ok=True)
report=[]
for prefix,name,budget in [('A22 -','Portal_A22',22000),('A23 -','Mirror_A23',12000),('A24 -','Pedestal_A24',10000)]:
    source_dir=next(os.path.join(ROOT,'Assets',n) for n in os.listdir(os.path.join(ROOT,'Assets')) if n.startswith(prefix) and os.path.isdir(os.path.join(ROOT,'Assets',n)))
    source=next(os.path.join(p,n) for p,ds,fs in os.walk(source_dir) for n in fs if n.lower().endswith(('.fbx','.glb','.obj')))
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    if source.endswith('.glb'):bpy.ops.import_scene.gltf(filepath=source)
    elif source.endswith('.obj'):bpy.ops.wm.obj_import(filepath=source)
    else:bpy.ops.import_scene.fbx(filepath=source)
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
    before=sum(len(o.data.polygons) for o in meshes)
    for o in meshes:
        bpy.context.view_layer.objects.active=o
        bpy.ops.object.select_all(action='DESELECT');o.select_set(True)
        tris=sum(len(p.vertices)-2 for p in o.data.polygons)
        if tris>budget:
            mod=o.modifiers.new('Project_LOD','DECIMATE');mod.ratio=budget/tris
            bpy.ops.object.modifier_apply(modifier=mod.name)
        for p in o.data.polygons:p.use_smooth=False
    # Keep model/material UVs; project materials are assigned by Unity. Source files stay untouched.
    bpy.ops.object.select_all(action='DESELECT')
    for o in meshes:o.select_set(True)
    dest=os.path.join(OUT,name+'.fbx')
    bpy.ops.export_scene.fbx(filepath=dest,use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False,path_mode='AUTO',use_mesh_modifiers=True)
    report.append({'source':os.path.relpath(source,ROOT),'output':os.path.relpath(dest,ROOT),'source_polygons':before,'output_triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in meshes)})
with open(os.path.join(ROOT,'Logs/C1Revision-model-processing.json'),'w',encoding='utf-8') as f:json.dump(report,f,indent=2,ensure_ascii=False)
print('C1_REVISION_MODELS_DONE',json.dumps(report,ensure_ascii=True))
