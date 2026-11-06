using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Chapter01CompletionArt;

public static class Chapter01PropSetup
{
    static T[] All<T>(Scene s)where T:Component=>s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
    static T Ref<T>(UnityEngine.Object o,string key)where T:UnityEngine.Object{var p=new SerializedObject(o).FindProperty(key);return p!=null?p.objectReferenceValue as T:null;}
    public static void Install(Scene scene,Action<string> check)
    {
        foreach(var camera in All<Unity.Cinemachine.CinemachineCamera>(scene))if(!camera.GetComponent<CaveCameraCollisionGuard>())camera.gameObject.AddComponent<CaveCameraCollisionGuard>();
        int shells=0;
        foreach(var collider in All<MeshCollider>(scene).Where(c=>c.name.StartsWith("Continuous_Limestone_Cliffs_")&&c.sharedMesh).ToArray())
        {
            if(collider.transform.Find("Collision_Backface"))continue;string name="CollisionBack_"+collider.sharedMesh.name;string path=Art+"/Meshes/"+name+".asset";
            var reversed=AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(path);
            if(!reversed)
            {
                reversed=UnityEngine.Object.Instantiate(collider.sharedMesh);reversed.name=name;
                for(int sub=0;sub<reversed.subMeshCount;sub++){var triangles=reversed.GetTriangles(sub);for(int i=0;i<triangles.Length;i+=3){int a=triangles[i];triangles[i]=triangles[i+1];triangles[i+1]=a;}reversed.SetTriangles(triangles,sub);}
                reversed.normals=reversed.normals.Select(n=>-n).ToArray();AssetDatabase.CreateAsset(reversed,path);
            }
            var back=Group("Collision_Backface",collider.transform).gameObject;back.layer=collider.gameObject.layer;back.AddComponent<MeshCollider>().sharedMesh=reversed;shells++;
        }
        check("Added reverse-facing collision shells to "+shells+" thin limestone walls, so camera queries also hit their exterior faces; original render meshes and physics settings retained.");
        var gold=Material("Lantern_Bronze",new Color(.57f,.37f,.13f));var petal=Material("Lantern_Petal",new Color(1,.66f,.27f),.65f);var core=Material("Lantern_Heart",new Color(1,.86f,.47f),1.3f);var jade=Material("Seal_Jade_Glow",new Color(.22f,.75f,.62f),.8f);
        foreach(var motor in All<PlayerMotor>(scene))
        {
            var occlusion=motor.GetComponent<CameraBodyOcclusion>()??motor.gameObject.AddComponent<CameraBodyOcclusion>();BuilderUtil.Set(occlusion,"view",All<Camera>(scene).First(c=>c.CompareTag("MainCamera")));
            var lantern=motor.GetComponent<LanternController>();var visual=Ref<GameObject>(lantern,"lanternVisual");if(visual&&!visual.transform.Find("VIS_LotusLantern"))Lotus(visual.transform,motor.transform,gold,petal,core,.30f);
        }
        foreach(var pickup in All<LanternPickup>(scene))if(!pickup.transform.Find("VIS_LotusLantern"))Lotus(pickup.transform,null,gold,petal,core,.34f);
        var doorObjects=new HashSet<GameObject>();
        foreach(var t in All<Transform>(scene).Where(t=>t.gameObject.activeInHierarchy&&(t.name.StartsWith("Door")||t.name=="Gate_ArenaLock")&&t.GetComponent<Collider>()))doorObjects.Add(t.gameObject);
        foreach(var seal in All<SealTarget>(scene)){var barrier=Ref<GameObject>(seal,"barrier");if(barrier)doorObjects.Add(barrier);}
        foreach(var door in doorObjects)
        {
            if(door.transform.Find("VIS_SealedDoor_FBX"))continue;var col=door.GetComponent<Collider>()??door.GetComponentInChildren<Collider>();if(!col)continue;var bounds=col.bounds;
            foreach(var rr in door.GetComponentsInChildren<Renderer>(true)){rr.enabled=false;if(PrefabUtility.IsPartOfPrefabInstance(rr))PrefabUtility.RecordPrefabInstancePropertyModifications(rr);}
            var root=Model("Assets/A10 - Ultimate Modular Ruins Pack/FBX/Doors_RoundArch.fbx","VIS_SealedDoor_FBX",null,new Vector3(bounds.center.x,bounds.min.y,bounds.center.z),bounds.size.y,0,Rock);
            var b=CampEnvironmentSetup.BoundsOf(root);var scale=root.transform.localScale;scale.x*=bounds.size.x/Mathf.Max(.01f,b.size.x);root.transform.localScale=scale;root.transform.SetParent(door.transform,true);
            var sigil=Group("Seal_Sigil",root.transform);sigil.position=new Vector3(bounds.center.x,bounds.center.y,bounds.min.z-.13f);
            var line=sigil.gameObject.AddComponent<LineRenderer>();line.sharedMaterial=jade;line.useWorldSpace=false;line.widthMultiplier=.028f;line.loop=true;line.positionCount=48;
            float radius=Mathf.Min(bounds.size.x*.31f,.9f);for(int i=0;i<48;i++){float a=i*Mathf.PI/24;line.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0));}
            for(int i=0;i<6;i++)
            {
                var mark=Group("Rune_"+i,sigil);mark.localPosition=Vector3.zero;var rune=mark.gameObject.AddComponent<LineRenderer>();rune.sharedMaterial=jade;rune.useWorldSpace=false;rune.widthMultiplier=.023f;rune.positionCount=3;
                float a=i*Mathf.PI/3;var d=new Vector3(Mathf.Sin(a),Mathf.Cos(a),0);var r=new Vector3(d.y,-d.x,0);rune.SetPositions(new[]{d*radius*.25f,d*radius*.62f+r*.10f,d*radius*.82f});
            }
            if(!door.GetComponent<DoorSoundCue>())door.AddComponent<DoorSoundCue>();check("FBX sealed door and jade sigil attached below existing barrier: "+door.name);
        }
        foreach(var receiver in All<LightPuzzleReceiver>(scene))
        {
            if(receiver.transform.Find("VIS_ReceiverCrystal"))continue;var rr=receiver.GetComponent<Renderer>();if(rr)rr.enabled=false;
            Model("Assets/A17 - Ultimate RPG Pack/FBX/Crystal3.fbx","VIS_ReceiverCrystal",receiver.transform,receiver.transform.position-Vector3.up*.35f,.72f,15,jade);
        }
        foreach(var item in All<ItemPickup>(scene))
        {
            if(item.transform.Find("VIS_Item_FBX"))continue;var so=new SerializedObject(item);var id=so.FindProperty("itemId")?.stringValue??"";
            if(item.name=="Pickup_linh_chi_01")item.transform.position=new Vector3(-5.5f,.35f,4.8f);
            foreach(var rr in item.GetComponentsInChildren<Renderer>(true))rr.enabled=false;
            string source=id.Contains("thuoc")?"Assets/A17 - Ultimate RPG Pack/FBX/Potion1_Filled.fbx":id.Contains("linh_chi")?"Assets/A19 - Ultimate Crops Pack/FBX/Mushroom_2.fbx":"Assets/A11 - Ultimate Stylized Nature Pack/FBX/Plant_2.fbx";
            Model(source,"VIS_Item_FBX",item.transform,new Vector3(item.transform.position.x,.025f,item.transform.position.z),.48f,0);
        }
        foreach(var t in All<Transform>(scene))if(PrefabUtility.IsPartOfPrefabInstance(t))PrefabUtility.RecordPrefabInstancePropertyModifications(t);
        foreach(var rr in All<Renderer>(scene).Where(r=>PrefabUtility.IsPartOfPrefabInstance(r)))PrefabUtility.RecordPrefabInstancePropertyModifications(rr);
        foreach(var box in All<BoxCollider>(scene).Where(b=>b.name.StartsWith("Collision_")&&b.GetComponentsInParent<Transform>().Any(t=>t.name=="ENV_C1_ClosedJunctions"))){var size=box.size;size.z=3.2f;box.size=size;}
        Refine(scene,check);Bake(scene);check("Lotus lantern replaces orange sphere in the hand and on pickup; no changes to An's model/rig, input, focus range or energy. FBX herbs/potions/crystal installed; junction collider thickness and navigation updated.");
    }
    static void Refine(Scene scene,Action<string> check)
    {
        foreach(var light in All<Light>(scene).Where(l=>l.name=="LanternLight"&&l.GetComponentInParent<PlayerMotor>()))
        {light.intensity=.35f;light.range=5;light.color=new Color(1,.75f,.45f);if(PrefabUtility.IsPartOfPrefabInstance(light))PrefabUtility.RecordPrefabInstancePropertyModifications(light);}
        foreach(var t in All<Transform>(scene).Where(t=>t.name=="VIS_LotusLantern"))
        {
            var rr=t.GetComponentsInChildren<Renderer>();foreach(var renderer in rr)
            {
                foreach(var m in renderer.sharedMaterials.Where(m=>m))
                {
                    if(m.name=="Lantern_Petal"){m.SetColor("_BaseColor",new Color(.9f,.45f,.15f));m.SetColor("_EmissionColor",new Color(.22f,.07f,.01f));}
                    if(m.name=="Lantern_Heart"){m.SetColor("_BaseColor",new Color(1,.73f,.32f));m.SetColor("_EmissionColor",new Color(.8f,.38f,.07f));}EditorUtility.SetDirty(m);
                }
            }
        }
        foreach(var boss in All<StoneGuardian>(scene))
        {
            var visual=boss.transform.Find("VIS_ImportedEnemy");if(!visual)continue;
            foreach(var rr in visual.GetComponentsInChildren<Renderer>(true))foreach(var m in rr.sharedMaterials.Where(m=>m))
            {m.SetTexture("_BaseMap",null);m.SetColor("_BaseColor",new Color(.38f,.44f,.45f));m.SetFloat("_Smoothness",.18f);EditorUtility.SetDirty(m);}
        }
        var world=All<Transform>(scene).Single(t=>t.name=="World_Geometry");
        foreach(var mf in world.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.gameObject.activeInHierarchy&&f.sharedMesh&&f.sharedMesh.name=="Crystal_FiveSided").ToArray())
        {
            if(mf.transform.Find("VIS_Crystal_FBX"))continue;var rr=mf.GetComponent<Renderer>();var b=rr.bounds;rr.enabled=false;
            bool purple=rr.sharedMaterial&&rr.sharedMaterial.name.Contains("Amethyst");var mat=Material(purple?"Crystal_Amethyst_FBX":"Crystal_Aqua_FBX",purple?new Color(.40f,.23f,.59f):new Color(.12f,.48f,.46f),.22f);
            Model("Assets/A17 - Ultimate RPG Pack/FBX/Crystal3.fbx","VIS_Crystal_FBX",mf.transform,b.min+new Vector3(b.extents.x,0,b.extents.z),b.size.y,mf.transform.eulerAngles.y,mat);
        }
        foreach(var relic in All<RelicPickup>(scene))
        {
            if(relic.transform.Find("VIS_DrumFragment"))continue;
            foreach(var rr in relic.GetComponentsInChildren<Renderer>(true))rr.enabled=false;
            var root=Group("VIS_DrumFragment");root.position=relic.transform.position;root.rotation=Quaternion.Euler(70,10,0);
            var v=new List<Vector3>();var tri=new List<int>();const int steps=12;
            for(int i=0;i<=steps;i++){float a=(-.8f+i*1.9f/steps);for(int ring=0;ring<4;ring++){float r=ring%2==0?.16f:.42f;v.Add(new Vector3(Mathf.Sin(a)*r,ring<2?-.025f:.025f,Mathf.Cos(a)*r));}}
            for(int i=0;i<steps;i++){int n=i*4,k=n+4;tri.AddRange(new[]{n,n+1,k,n+1,k+1,k,n+2,k+2,n+3,n+3,k+2,k+3,n,k,n+2,n+2,k,k+2,n+1,n+3,k+1,n+3,k+3,k+1});}
            tri.AddRange(new[]{0,2,1,1,2,3,steps*4,steps*4+1,steps*4+2,steps*4+1,steps*4+3,steps*4+2});
            Surface("Bronze_Arc",root,Mesh("Drum_Fragment_Arc",v,tri),Material("Relic_Aged_Bronze",new Color(.64f,.41f,.15f),.08f));
            var stroke=Group("Engraved_Ring",root).gameObject.AddComponent<LineRenderer>();stroke.useWorldSpace=false;stroke.sharedMaterial=Material("Relic_Engraving",new Color(.38f,.76f,.61f),.2f);stroke.widthMultiplier=.012f;stroke.positionCount=steps+1;
            for(int i=0;i<=steps;i++){float a=-.8f+i*1.9f/steps;stroke.SetPosition(i,new Vector3(Mathf.Sin(a)*.31f,.031f,Mathf.Cos(a)*.31f));}root.SetParent(relic.transform,true);
        }
        foreach(var cp in All<CheckpointAltar>(scene).Where(c=>c.CheckpointId!="cp_trai"))
        {
            if(cp.transform.Find("VIS_CheckpointCrystal"))continue;
            foreach(var mf in cp.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.sharedMesh&&f.sharedMesh.name=="Sphere"))mf.GetComponent<Renderer>().enabled=false;
            var col=cp.GetComponent<Collider>();var b=col?col.bounds:new Bounds(cp.transform.position,Vector3.one);
            Model("Assets/A17 - Ultimate RPG Pack/FBX/Crystal2.fbx","VIS_CheckpointCrystal",cp.transform,new Vector3(cp.transform.position.x,b.max.y-.08f,cp.transform.position.z),.38f,20,Material("Checkpoint_Jade",new Color(.18f,.63f,.46f),.3f));
        }
        foreach(var meshFilter in world.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.sharedMesh&&f.sharedMesh.name.StartsWith("Paved_")&&!f.sharedMesh.name.EndsWith("_Beveled")))
        {
            string key=meshFilter.sharedMesh.name+"_Beveled";string path=Art+"/Meshes/"+key+".asset";var existing=AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(path);if(existing){meshFilter.sharedMesh=existing;continue;}
            var src=meshFilter.sharedMesh.vertices;if(src.Length%9!=0)continue;var v=new List<Vector3>();var tri=new List<int>();
            for(int stone=0;stone<src.Length/9;stone++)
            {
                int start=stone*9,n=v.Count;Vector3 c=src[start];
                for(int j=0;j<8;j++){var edge=src[start+1+j];edge.y=.042f;v.Add(edge);edge=c+(edge-c)*.91f;edge.y=.078f;v.Add(edge);}
                v.Add(new Vector3(c.x,.08f,c.z));
                for(int j=0;j<8;j++){int a=n+j*2,b=n+(j+1)%8*2;tri.AddRange(new[]{a,b,a+1,b,b+1,a+1,n+16,a+1,b+1});}
            }
            meshFilter.sharedMesh=Mesh(key,v,tri);
        }
        foreach(var t in All<Transform>(scene))if(PrefabUtility.IsPartOfPrefabInstance(t))PrefabUtility.RecordPrefabInstancePropertyModifications(t);
        foreach(var rr in All<Renderer>(scene).Where(r=>PrefabUtility.IsPartOfPrefabInstance(r)))PrefabUtility.RecordPrefabInstancePropertyModifications(rr);
        FixPaverNormals(check);check("Refined lotus exposure, stone guardian palette, FBX cave crystals, bronze relic fragments, checkpoint crystals and beveled path stones; original IDs and colliders retained.");
    }
    public static void FixPaverNormals(Action<string> check)
    {
        int fixedMeshes=0;foreach(var guid in AssetDatabase.FindAssets("t:Mesh",new[]{Art+"/Meshes"}))
        {
            var mesh=AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(AssetDatabase.GUIDToAssetPath(guid));if(!mesh||!mesh.name.EndsWith("_Beveled"))continue;
            if(mesh.normals.Average(n=>n.y)>=0)continue;var triangles=mesh.triangles;for(int i=0;i<triangles.Length;i+=3){int n=triangles[i];triangles[i]=triangles[i+1];triangles[i+1]=n;}mesh.triangles=triangles;mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);fixedMeshes++;
        }AssetDatabase.SaveAssets();check("Corrected upward stone/bevel normals: "+fixedMeshes+" project meshes; geometry, IDs and collision unchanged.");
    }
    static void Lotus(Transform parent,Transform actor,Material bronze,Material petals,Material heart,float size)
    {
        foreach(var rr in parent.GetComponentsInChildren<Renderer>(true)){rr.enabled=false;if(PrefabUtility.IsPartOfPrefabInstance(rr))PrefabUtility.RecordPrefabInstancePropertyModifications(rr);}
        var root=Group("VIS_LotusLantern");root.position=parent.position;root.localScale=Vector3.one*size;var v=new List<Vector3>();var tri=new List<int>();
        for(int petal=0;petal<12;petal++)
        {
            float a=petal*Mathf.PI/6;var f=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));var r=new Vector3(f.z,0,-f.x);int n=v.Count;
            v.Add(new Vector3(0,-.48f,0));v.Add(f*.68f-r*.28f+Vector3.up*-.25f);v.Add(f*(petal%2==0?.74f:.64f)+Vector3.up*.38f);v.Add(f*.68f+r*.28f+Vector3.up*-.25f);v.Add(f*.36f+Vector3.up*-.1f);
            tri.AddRange(new[]{n,n+1,n+4,n+1,n+2,n+4,n+2,n+3,n+4,n+3,n,n+4});
        }
        Surface("Lotus_Petals",root,Mesh("Lotus_Petals",v,tri),petals);
        var glow=GameObject.CreatePrimitive(PrimitiveType.Sphere);glow.name="Lotus_Heart";UnityEngine.Object.DestroyImmediate(glow.GetComponent<Collider>());glow.transform.SetParent(root,false);glow.transform.localScale=Vector3.one*.42f;glow.GetComponent<Renderer>().sharedMaterial=heart;
        var handle=Group("Bronze_Handle",root).gameObject.AddComponent<LineRenderer>();handle.sharedMaterial=bronze;handle.useWorldSpace=false;handle.widthMultiplier=.045f;handle.loop=false;handle.positionCount=25;
        for(int i=0;i<25;i++){float a=i*Mathf.PI/24;handle.SetPosition(i,new Vector3(Mathf.Cos(a)*.42f,.38f+Mathf.Sin(a)*.65f,0));}
        root.SetParent(parent,true);if(actor){var upright=root.gameObject.AddComponent<LotusLanternVisual>();BuilderUtil.Set(upright,"actor",actor);}
    }
}
