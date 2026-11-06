using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

// Scene authoring only: all traversal colliders, gameplay roots and persistent references stay in place.
public static class Chapter02EnvironmentArt
{
    public const string Art="Assets/_Project/Art/Chapter02Environment";
    const string Cave="Assets/A09 - Mines and Cave Set/MInesAndCaveSet/prefabs/Wall/";
    const string Nature="Assets/A11 - Ultimate Stylized Nature Pack/FBX/";
    const string Ruins="Assets/A10 - Ultimate Modular Ruins Pack/FBX/";
    const string C1="Assets/_Project/Art/Chapter01Expanded/Materials/";
    static Material rock,stone,bronze,leaf,wood,slate,lightStone;
    static T[] All<T>(Scene s)where T:Component=>s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
    static T Ref<T>(UnityEngine.Object o,string f)where T:UnityEngine.Object=>Chapter01RevisionSetup.Ref<T>(o,f);
    static Transform Group(string n,Transform p,Vector3 position=default){var g=new GameObject(n);g.layer=2;g.transform.SetParent(p,false);g.transform.position=position;return g.transform;}
    static Material Mat(string n,Color c,float metal=0)
    {
        string p=Art+"/Materials/"+n+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);
        if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}
        m.color=c;m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",.2f);m.SetFloat("_Cull",0);EditorUtility.SetDirty(m);return m;
    }
    static Transform Shape(string n,Transform p,PrimitiveType type,Vector3 pos,Vector3 size,Material mat)
    {
        var g=GameObject.CreatePrimitive(type);g.name=n;g.layer=2;g.transform.SetParent(p,false);g.transform.position=pos;g.transform.localScale=size;
        UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=mat;return g.transform;
    }
    static GameObject Model(string path,string n,Transform p,Vector3 foot,float h,float yaw=0,Material mat=null)
    {
        var g=Chapter01CompletionArt.Model(path,n,p,foot,h,yaw,mat);
        if(!mat)foreach(var rr in g.GetComponentsInChildren<Renderer>(true))rr.sharedMaterials=rr.sharedMaterials.Select(m=>
        {
            var known=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Camp/M_A11_"+m.name+".mat");
            if(known)return known;
            var copy=Mat("Source_"+m.name.Replace(' ','_'),m.HasProperty("_Color")?m.color:new Color(.4f,.6f,.25f));if(m.mainTexture)copy.SetTexture("_BaseMap",m.mainTexture);return copy;
        }).ToArray();
        return g;
    }
    static GameObject Fit(string path,string n,Transform p,Bounds box,Material mat)
    {
        var g=Model(path,n,p,new Vector3(box.center.x,box.min.y,box.center.z),box.size.y,0,mat);
        var b=CampEnvironmentSetup.BoundsOf(g);var fit=g.transform.GetChild(0);var sc=fit.localScale;sc.x*=box.size.x/Mathf.Max(.001f,b.size.x);sc.z*=box.size.z/Mathf.Max(.001f,b.size.z);fit.localScale=sc;
        b=CampEnvironmentSetup.BoundsOf(g);fit.position+=box.center-b.center;return g;
    }
    static void Hide(Transform p){foreach(var rr in p.GetComponentsInChildren<MeshRenderer>(true))rr.enabled=false;}
    static Transform ReplaceVisual(Transform p)
    {
        var old=p.Find("VIS_C2_Art");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
        Hide(p);return Group("VIS_C2_Art",p,p.position);
    }
    static void Mark(Transform p,Vector3 point,float size=1)
    {
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Chapter02OpeningSetup.Art+"/Feather_LowPoly.asset");
        var g=Group("Feather_Inlay",p,point);g.localRotation=Quaternion.Euler(90,0,0);g.localScale=Vector3.one*size;g.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;g.gameObject.AddComponent<MeshRenderer>().sharedMaterial=bronze;
    }
    public static void Install(Scene s)
    {
        Chapter01CompletionArt.Folder(Art+"/Materials");Chapter01CompletionArt.Folder(Art+"/Meshes");
        rock=AssetDatabase.LoadAssetAtPath<Material>(C1+"Limestone_Moss.mat");stone=Mat("Weathered_Stone",new Color(.37f,.43f,.40f));lightStone=Mat("Sunlit_Stone",new Color(.55f,.59f,.49f));bronze=Mat("Feather_Bronze",new Color(.66f,.46f,.18f),.45f);leaf=AssetDatabase.LoadAssetAtPath<Material>(C1+"Fern_Leaf.mat");wood=AssetDatabase.LoadAssetAtPath<Material>(C1+"Source_M_CampWood.mat");slate=AssetDatabase.LoadAssetAtPath<Material>(C1+"Sanctum_Roof_Slate.mat");
        var world=All<Transform>(s).Single(t=>t.name=="World_Geometry");
        var previous=world.Find("ART_C2_Environment");if(previous)UnityEngine.Object.DestroyImmediate(previous.gameObject);
        var root=Group("ART_C2_Environment",world);
        Geometry(s,world,root);Gardens(root);Architecture(s,root);Interactables(s);Bird(s);Atmosphere(s,root);Audio();UI(s);
        var exclusion=Type.GetType("UnityEngine.Rendering.DisallowGPUDrivenRendering, Unity.RenderPipelines.GPUDriven.Runtime",true);
        foreach(var g in s.GetRootGameObjects().Where(g=>g.GetComponentsInChildren<MeshRenderer>(true).Length>0))
        {var c=g.GetComponent(exclusion)??g.AddComponent(exclusion);exclusion.GetProperty("applyToChildrenRecursively").SetValue(c,true);}
    }
    static void Geometry(Scene s,Transform world,Transform root)
    {
        // A closed rock surface keeps every wall opaque; UVs use metres instead of stretched prefab bounds.
        foreach(var rr in world.GetComponentsInChildren<MeshRenderer>(true).Where(r=>!r.transform.IsChildOf(root)&&r.GetComponent<BoxCollider>()&&r.name.StartsWith("Wall_")).ToArray())
        {
            rr.enabled=true;rr.sharedMaterial=rock;SkinBox(rr);
        }
        foreach(var rr in world.GetComponentsInChildren<MeshRenderer>(true).Where(r=>!r.transform.IsChildOf(root)&&r.GetComponent<Collider>()&&!r.name.StartsWith("Wall_")).ToArray())
        {
            string n=rr.name;
            if(n.StartsWith("Floor_")||n.StartsWith("Bridge_")||n.StartsWith("Plat_")||n.StartsWith("Ledge_"))rr.sharedMaterial=n.StartsWith("Ledge_")||n.StartsWith("Plat_")?rock:AssetDatabase.LoadAssetAtPath<Material>(C1+(n=="Floor_P1"?"Moss_Soil.mat":"Worn_Path_Stone.mat"));
            if((n.StartsWith("Ledge_")||n.StartsWith("Plat_")||n.StartsWith("Floor_")||n.StartsWith("Bridge_"))&&rr.GetComponent<BoxCollider>())SkinBox(rr);
            if(n.StartsWith("Rail_")||n.StartsWith("Perch_")||n.StartsWith("ArenaWall"))rr.sharedMaterial=rock;
        }
        // Weathered paving stays flush with floors, including both bridge gaps.
        var rng=new System.Random(117);
        foreach(var rr in world.GetComponentsInChildren<MeshRenderer>(true).Where(r=>!r.transform.IsChildOf(root)&&(r.name.StartsWith("Floor_")||r.name.StartsWith("Bridge_")||r.name.StartsWith("Plat_")||r.name.StartsWith("Ledge_"))&&r.GetComponent<Collider>()).ToArray())
        {
            var b=rr.GetComponent<Collider>().bounds;
            if(b.size.y>6)continue;
            int count=Mathf.Min(12,Mathf.CeilToInt(b.size.x*b.size.z/8));
            for(int i=0;i<count;i++)
            {
                float x=Mathf.Lerp(b.min.x+.25f,b.max.x-.25f,(float)rng.NextDouble()),z=Mathf.Lerp(b.min.z+.25f,b.max.z-.25f,(float)rng.NextDouble());
                if(rr.name=="Floor_P5"&&new Vector2(x-b.center.x,z-b.center.z).magnitude>8.2f)continue;
                var tile=Model(Ruins+"Floor_Standard_Half.fbx","Worn_Paving",root,new Vector3(x,b.max.y+.004f,z),.018f,(float)rng.NextDouble()*20, i%3==0?lightStone:stone);
                var bb=CampEnvironmentSetup.BoundsOf(tile);var f=tile.transform.GetChild(0);var sc=f.localScale;sc.x*=1.0f/Mathf.Max(.001f,bb.size.x);sc.z*=.8f/Mathf.Max(.001f,bb.size.z);f.localScale=sc;
            }
        }
        // Outer cliffs enclose distant views around the exposed bridge without closing playable gaps.
        for(int side=-1;side<=1;side+=2)for(int j=0;j<8;j++)
        {
            var p=new Vector3(5+side*(18+j%2*3),-6,36+j*7);
            Fit(Cave+"Cave_Wall_Rocks_"+(j%2==0?"A":"B")+".prefab","Distant_Cliff",root,new Bounds(p+Vector3.up*19,new Vector3(8,38,9)),rock);
        }
        for(int i=0;i<11;i++)
        {
            float a=i*Mathf.PI*2/11;var p=new Vector3(5+Mathf.Cos(a)*15,-2,82+Mathf.Sin(a)*14);
            Fit(Cave+"Cave_Rocks_B.prefab","Nest_Outer_Cliff",root,new Bounds(p+Vector3.up*12,new Vector3(5,24,5)),rock);
        }
        // Full-volume imported rock clusters break the flat outline at safe room edges.
        foreach(var p in new[]{new Vector3(-7.4f,-.1f,-3),new Vector3(7.5f,-.1f,6),new Vector3(-.5f,4.9f,18),new Vector3(11.5f,12.9f,34)})
            Fit(Cave+"Cave_Rocks_B.prefab","Limestone_Buttress",root,new Bounds(p+Vector3.up*1.9f,new Vector3(1.2f,3.8f,1.4f)),rock);
    }
    static void SkinBox(MeshRenderer renderer)
    {
        var filter=renderer.GetComponent<MeshFilter>();var box=renderer.GetComponent<BoxCollider>();if(!filter||!box)return;
        string file=Art+"/Meshes/RockSkin_"+renderer.name+"_"+renderer.transform.parent.name+".asset";
        var verts=new List<Vector3>();var triangles=new List<int>();var uv=new List<Vector2>();
        var size=box.bounds.size;
        foreach(var normal in new[]{Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back})
        {
            Vector3 u=Mathf.Abs(normal.y)>.5f?Vector3.right:Vector3.up,v=Vector3.Cross(normal,u);
            float width=Vector3.Scale(u,size).magnitude,height=Vector3.Scale(v,size).magnitude;
            int nx=Mathf.Clamp(Mathf.CeilToInt(width/1.4f),1,32),ny=Mathf.Clamp(Mathf.CeilToInt(height/1.4f),1,32);
            Func<int,int,Vector3> point=(x,y)=>
            {
                float rough=(x==0||x==nx||y==0||y==ny||normal.y>.5f)?0:Mathf.Abs(Mathf.Sin(x*2.7f+y*1.9f))*.07f/Mathf.Max(.2f,Vector3.Scale(normal,size).magnitude);
                return box.center+Vector3.Scale(normal*(.5f-rough)+u*((float)x/nx-.5f)+v*((float)y/ny-.5f),box.size);
            };
            for(int y=0;y<ny;y++)for(int x=0;x<nx;x++)
            {
                Vector3[] p={point(x,y),point(x+1,y),point(x,y+1),point(x+1,y),point(x+1,y+1),point(x,y+1)};
                Vector2[] t={new Vector2(x,y),new Vector2(x+1,y),new Vector2(x,y+1),new Vector2(x+1,y),new Vector2(x+1,y+1),new Vector2(x,y+1)};
                for(int i=0;i<6;i++){triangles.Add(verts.Count);verts.Add(p[i]);uv.Add(new Vector2(t[i].x*width/nx*.22f,t[i].y*height/ny*.22f));}
            }
        }
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(file);if(!mesh){mesh=new Mesh{name="Closed_Limestone_"+renderer.name};AssetDatabase.CreateAsset(mesh,file);}else mesh.Clear();
        mesh.SetVertices(verts);mesh.SetTriangles(triangles,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);filter.sharedMesh=mesh;
    }
    static void Gardens(Transform root)
    {
        Vector3[] patches={new Vector3(-6.6f,0,-1),new Vector3(-6.6f,0,5),new Vector3(6.6f,0,-5),new Vector3(6.9f,0,0),new Vector3(-3,0,6),new Vector3(0,5,12),new Vector3(10,5,17),new Vector3(.1f,13,25),new Vector3(11,13,35),new Vector3(1.2f,13,36),new Vector3(1.5f,18,66),new Vector3(8.7f,18,70),new Vector3(4,18,90),new Vector3(10.8f,18,87)};
        var rng=new System.Random(338);
        for(int i=0;i<patches.Length;i++)
        {
            var p=patches[i];Model(Nature+"Rock_"+(i%5+1)+".fbx","Moss_Boulder_Cluster",root,p-Vector3.up*.04f,.5f+i%3*.18f,i*47,rock);
            Model(Nature+"Bush_Small.fbx","Bush_At_Rock_Foot",root,p+new Vector3(.4f,0,.2f),.55f,i*35,leaf);
            for(int k=0;k<4;k++){var q=p+new Vector3((float)rng.NextDouble()*1.1f-.55f,-.015f,(float)rng.NextDouble()*1.1f-.55f);Model(Nature+"Grass_Small.fbx","Grounded_Grass",root,q,.23f+k*.04f,k*73,leaf);}
            if(i%3==0)Model(Nature+"PalmTree_1.fbx","Skylight_Tree",root,p,3.8f+i%2,i*43);
        }
        for(int i=0;i<7;i++)Model(Nature+"PalmTree_"+(i%3+1)+".fbx","Distant_Canopy",root,new Vector3(i%2==0?-7:17,5,42+i*6),7+i%3,i*50);
        for(int i=0;i<9;i++)Model(Nature+"Rock_"+(i%5+1)+".fbx","Well_Rim_Rock",root,new Vector3(-7+i*1.8f,20,-8.1f),1.2f,i*63,rock);
        foreach(float x in new[]{-8.2f,8.2f})foreach(float z in new[]{-8.1f,8.1f})
        {
            Fit(Cave+"Cave_Pillar_B.prefab","Well_Limestone_Rib",root,new Bounds(new Vector3(x,10,z),new Vector3(2.3f,20,2.3f)),rock);
            Model(Nature+"PalmTree_2.fbx","Canopy_Above_Well",root,new Vector3(x,20,z),4.5f,z*12);
        }
    }
    static void Architecture(Scene s,Transform root)
    {
        foreach(var house in All<Transform>(s).Where(t=>t.name.StartsWith("House_")&&t.parent&&t.parent.name=="Room_P3_NhaSan"))House(house,root);
        // Name-based fallback keeps the actual authored house roots independent of builder spelling.
        foreach(var house in All<Transform>(s).Where(t=>t.name.StartsWith("House_")&&t.Find("Floor")&&!root.Find("Roof_"+t.name)))House(house,root);
        foreach(var perch in All<Transform>(s).Where(t=>t.name.StartsWith("Perch_")&&t.GetComponent<BoxCollider>()))
        {
            var b=perch.GetComponent<BoxCollider>().bounds;perch.GetComponent<MeshRenderer>().enabled=false;
            Fit(Ruins+"Column_Round.fbx","Carved_Stone_Perch",root,new Bounds(b.center,new Vector3(1.5f,b.size.y,1.5f)),stone);Mark(root,new Vector3(b.center.x,b.max.y+.012f,b.center.z),1);
        }
        for(int i=0;i<3;i++)
        {
            float z=new[]{43f,52.5f,60.5f}[i];foreach(float x in new[]{3.12f,6.88f})
            {Model(Ruins+"Column_Round_Short.fbx","Bridge_Rail_Post",root,new Vector3(x,18,z),.85f,0,stone);}
        }
        foreach(var n in All<Transform>(s).Where(t=>t.name=="Wall_ArenaGate_Top"))Mark(root,n.position+new Vector3(0,0,-.25f),2);
        foreach(var mark in All<Transform>(s).Where(t=>t.name=="Landing_And_Aisle_Mark"))
        {if(mark.GetComponent<MeshRenderer>())mark.GetComponent<MeshRenderer>().enabled=false;Mark(root,mark.position+Vector3.up*.008f,.45f);}
    }
    static void House(Transform house,Transform root)
    {
        var h=house.position;var roof=Group("Roof_"+house.name,root,h);
        foreach(var rr in house.GetComponentsInChildren<MeshRenderer>(true))rr.sharedMaterial=stone;
        var verts=new List<Vector3>();var tri=new List<int>();
        for(int z=0;z<2;z++)for(int x=0;x<7;x++)
        {float u=(x-3)/3f;verts.Add(new Vector3(u*1.9f,3.1f+(1-Mathf.Abs(u))+.45f*Mathf.Pow(Mathf.Abs(u),6),z==0?-1.9f:1.9f));}
        for(int x=0;x<6;x++)tri.AddRange(new[]{x,x+7,x+1,x+1,x+7,x+8});
        string path=Art+"/Meshes/Curved_House_Roof.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(!mesh){mesh=new Mesh{name="Curved_House_Roof"};AssetDatabase.CreateAsset(mesh,path);}mesh.Clear();mesh.SetVertices(verts);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
        roof.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;roof.gameObject.AddComponent<MeshRenderer>().sharedMaterial=slate;
        for(int side=-1;side<=1;side+=2){Shape("Roof_Eave",roof,PrimitiveType.Cube,h+new Vector3(side*1.85f,3.8f,0),new Vector3(.13f,.14f,4),bronze);for(int end=-1;end<=1;end+=2)Model(Ruins+"Column_Round.fbx","Stilt_Column",roof,h+new Vector3(side*1.3f,1.5f,end*1.3f),1.5f,0,stone);}
        Mark(roof,h+new Vector3(0,1.52f,-.7f),.7f);
    }
    static void Interactables(Scene s)
    {
        foreach(var pickup in All<ItemPickup>(s))
        {
            string id=new SerializedObject(pickup).FindProperty("itemId").stringValue;var vis=ReplaceVisual(pickup.transform);var p=pickup.transform.position-Vector3.up*.3f;
            if(id==InventoryService.LaCam)Model(Nature+"Grass_Large.fbx","Medicinal_Leaves",vis,p,.42f,0,leaf);
            else if(id==InventoryService.ThuocMau)Model("Assets/A17 - Ultimate RPG Pack/FBX/Potion1_Filled.fbx","Health_Potion",vis,p,.35f);
            else
            {
                var cap=Mat("Mushroom_Cap",new Color(.42f,.18f,.12f));var stem=Mat("Mushroom_Stem",new Color(.78f,.71f,.50f));
                for(int i=0;i<3;i++){var q=p+new Vector3((i-1)*.14f,0,i%2*.10f);Shape("LinhChi_Stem",vis,PrimitiveType.Cylinder,q+Vector3.up*.12f,new Vector3(.06f,.12f,.06f),stem);Shape("LinhChi_Cap",vis,PrimitiveType.Sphere,q+Vector3.up*.25f,new Vector3(.24f,.1f,.20f),cap);}
            }
        }
        foreach(var gate in All<RequirementDoor>(s))
        {
            var door=Ref<GameObject>(gate,"door");var bounds=door.GetComponent<Collider>().bounds;var art=ReplaceVisual(door.transform);
            Fit("Assets/_Project/Art/Chapter01Revision/Models/Portal_A22.fbx","Carved_Arena_Gate",art,bounds,stone);
        }
        foreach(var cp in All<CheckpointAltar>(s))
        {
            var vis=ReplaceVisual(cp.transform);var p=cp.transform.position;
            Model("Assets/_Project/Art/Chapter01Revision/Models/Pedestal_A24.fbx","Checkpoint_Carved_Stone",vis,p,.75f,0,stone);
            Shape("Resting_Lantern_Bowl",vis,PrimitiveType.Sphere,p+Vector3.up*.82f,new Vector3(.5f,.13f,.5f),bronze);Mark(vis,p+Vector3.up*.92f,.5f);
            if(cp.CheckpointId=="c2_cp_start")
            {
                var fire=new GameObject("Checkpoint_Flame");fire.transform.SetParent(vis,false);fire.transform.position=p+Vector3.up*.86f;var ps=fire.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var m=ps.main;m.startLifetime=.7f;m.startSpeed=.55f;m.startSize=new ParticleSystem.MinMaxCurve(.10f,.2f);m.startColor=new Color(1,.40f,.08f,.8f);m.maxParticles=32;var e=ps.emission;e.rateOverTime=16;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.radius=.10f;shape.angle=5;shape.rotation=new Vector3(-90,0,0);ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Chapter01Revision/Materials/Fog_URP.mat");ps.Play();
                var light=fire.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.48f,.16f);light.intensity=3;light.range=4;
            }
        }
        foreach(var pedestal in All<QuizPedestal>(s))
        {
            var vis=ReplaceVisual(pedestal.transform);var p=pedestal.transform.position;
            Model("Assets/_Project/Art/Chapter01Revision/Models/Pedestal_A24.fbx","Question_Altar",vis,p,1.05f,0,stone);
            Mark(vis,p+Vector3.up*1.07f,.75f);
        }
        foreach(var stele in All<LoreStele>(s))
        {
            var vis=ReplaceVisual(stele.transform);var p=stele.transform.position;
            Model(Cave+"Cave_Pillar_A.prefab","Lore_Stone",vis,p,1.75f,stele.transform.eulerAngles.y,stone);
            var col=stele.GetComponentInChildren<Collider>();if(col){var b=col.bounds;var slab=Shape("Inscribed_Face",vis,PrimitiveType.Cube,b.center,new Vector3(.72f,1.3f,.12f),lightStone);slab.rotation=stele.transform.rotation;}
        }
        foreach(var bell in All<SequenceInputNode>(s))
        {
            Ref<GameObject>(bell,"hitIndicator").SetActive(false);
            var body=bell.transform.Find("Bell");body.GetComponent<MeshRenderer>().sharedMaterial=bronze;body.GetComponent<MeshFilter>().sharedMesh=BellMesh();
            bell.transform.Find("Post").GetComponent<MeshRenderer>().enabled=false;
            var oldFrame=bell.transform.Find("VIS_C2_BellFrame");if(oldFrame)UnityEngine.Object.DestroyImmediate(oldFrame.gameObject);var frame=Group("VIS_C2_BellFrame",bell.transform,bell.transform.position);var bp=bell.transform.position;
            foreach(float x in new[]{-.72f,.72f})Shape("Bronze_Hanger",frame,PrimitiveType.Cylinder,bp+new Vector3(x,.95f,0),new Vector3(.12f,.95f,.12f),wood);
            Shape("Crossbeam",frame,PrimitiveType.Cube,bp+Vector3.up*1.98f,new Vector3(1.7f,.16f,.18f),wood);
            Shape("Bell_Suspension",frame,PrimitiveType.Cylinder,bp+Vector3.up*1.75f,new Vector3(.035f,.15f,.035f),bronze);
        }
        var altar=All<RelicAltar>(s).Single();var restored=Ref<GameObject>(altar,"restoredVisual");
        foreach(var rr in altar.GetComponentsInChildren<MeshRenderer>(true))if(!rr.transform.IsChildOf(restored.transform))rr.sharedMaterial=stone;
        var paltar=altar.transform.position;var old=altar.transform.Find("VIS_C2_AltarBase");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);var baseArt=Group("VIS_C2_AltarBase",altar.transform,paltar);
        Model("Assets/_Project/Art/Chapter01Revision/Models/Pedestal_A24.fbx","Restoration_Pedestal",baseArt,paltar,1.1f,0,stone);
        var relic=ReplaceVisual(restored.transform);var center=paltar+Vector3.up*1.2f;Shape("Restored_Sun",relic,PrimitiveType.Cylinder,center,new Vector3(1.1f,.055f,1.1f),bronze);
        for(int i=0;i<12;i++){float a=i*30*Mathf.Deg2Rad;Mark(relic,center+new Vector3(Mathf.Cos(a)*.65f,.06f,Mathf.Sin(a)*.65f),.55f);var f=relic.GetChild(relic.childCount-1);f.rotation=Quaternion.Euler(90,-i*30,0);}
        var exit=All<ChapterExit>(s).Single();var portal=Ref<GameObject>(exit,"portalVisual");var ep=ReplaceVisual(portal.transform);
        Fit("Assets/_Project/Art/Chapter01Revision/Models/Portal_A22.fbx","Wind_Gateway",ep,new Bounds(portal.transform.position+Vector3.up*1.6f,new Vector3(2.8f,3.2f,.6f)),stone);
    }
    static Mesh BellMesh()
    {
        string path=Art+"/Meshes/Bronze_Wind_Bell.asset";var m=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(m)return m;
        var v=new List<Vector3>();var t=new List<int>();float[] radii={.48f,.39f,.30f,.24f,.18f};
        for(int r=0;r<radii.Length;r++)for(int i=0;i<16;i++){float a=i*Mathf.PI/8;v.Add(new Vector3(Mathf.Cos(a)*radii[r],-.45f+r*.225f,Mathf.Sin(a)*radii[r]));}
        for(int r=0;r<4;r++)for(int i=0;i<16;i++){int a=r*16+i,b=r*16+(i+1)%16;t.AddRange(new[]{a,b,a+16,b,b+16,a+16});}
        m=new Mesh{name="Bronze_Wind_Bell",vertices=v.ToArray(),triangles=t.ToArray()};m.RecalculateNormals();m.RecalculateBounds();AssetDatabase.CreateAsset(m,path);return m;
    }
    static void Bird(Scene s)
    {
        var boss=All<BossChimLac>(s).Single();var model=boss.transform.Find("VIS_ChimLac_Combat");
        if(!model)return;
        // Retain the animated wing/neck pivots and replace the faceted proxy silhouette with shaped plumage.
        foreach(var r in model.GetComponentsInChildren<MeshRenderer>(true))if(r.name=="Body"||r.name=="Head"||r.name=="Neck_Length")r.sharedMaterial=Mat("ChimLac_Jade",new Color(.09f,.32f,.29f),.12f);
        var neck=model.Find("Neck");var prior=neck.Find("Crest");if(prior)UnityEngine.Object.DestroyImmediate(prior.gameObject);var crest=Group("Crest",neck,neck.position);
        for(int i=0;i<3;i++){Mark(crest,neck.TransformPoint(new Vector3((i-1)*.11f,1.17f,.1f)),.5f);crest.GetChild(i).localRotation=Quaternion.Euler(-25,(i-1)*15,0);}
        var body=model.Find("Body");body.localScale=new Vector3(.8f,.75f,1.3f);
        var spirit=All<QuizSequence>(s).Single();var visual=Ref<GameObject>(spirit,"visual");var old=visual.transform.Find("VIS_C2_Spirit");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
        if(visual.GetComponent<MeshRenderer>())visual.GetComponent<MeshRenderer>().enabled=false;
        var clone=UnityEngine.Object.Instantiate(model.gameObject,visual.transform);clone.name="VIS_C2_Spirit";UnityEngine.Object.DestroyImmediate(clone.GetComponent<ChimLacPresentation>());clone.transform.localPosition=new Vector3(0,-1.1f,0);clone.transform.localScale=Vector3.one*.65f;clone.transform.localRotation=Quaternion.Euler(0,180,0);clone.transform.Find("Stun_Stars").gameObject.SetActive(false);
        clone.transform.Find("Left_Wing").localRotation=Quaternion.Euler(0,15,48);clone.transform.Find("Right_Wing").localRotation=Quaternion.Euler(0,-15,-48);
    }
    static void Atmosphere(Scene s,Transform root)
    {
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0085f;RenderSettings.fogColor=new Color(.40f,.56f,.55f);
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.55f,.64f,.65f);RenderSettings.ambientEquatorColor=new Color(.35f,.44f,.40f);RenderSettings.ambientGroundColor=new Color(.18f,.22f,.19f);RenderSettings.ambientIntensity=1;
        var sun=All<Light>(s).FirstOrDefault(l=>l.type==LightType.Directional);
        if(!sun)sun=Group("C2_Skylight_Sun",root).gameObject.AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(1,.91f,.72f);sun.intensity=1.5f;sun.transform.rotation=Quaternion.Euler(58,-28,0);sun.shadows=LightShadows.Soft;sun.shadowStrength=.75f;RenderSettings.sun=sun;
        foreach(var l in All<Light>(s).Where(l=>l.type==LightType.Point&&l.name!="Checkpoint_Flame")){l.shadows=LightShadows.None;l.intensity=Mathf.Min(l.intensity,2);l.color=new Color(.89f,.94f,.80f);}
        foreach(var w in All<WindColumn>(s))
        {
            var v=Ref<GameObject>(w,"visual");var prior=v.transform.Find("VIS_C2_WindParticles");if(prior)UnityEngine.Object.DestroyImmediate(prior.gameObject);
            string wp=Art+"/Materials/Wind_Readable.mat";var wm=AssetDatabase.LoadAssetAtPath<Material>(wp);if(!wm){wm=new Material(v.GetComponent<Renderer>().sharedMaterial);AssetDatabase.CreateAsset(wm,wp);}wm.color=new Color(.35f,.85f,.78f,.065f);EditorUtility.SetDirty(wm);v.GetComponent<Renderer>().sharedMaterial=wm;
            var g=new GameObject("VIS_C2_WindParticles");g.transform.SetParent(v.transform,false);g.transform.position=new Vector3(w.transform.position.x,w.transform.position.y+.1f,w.transform.position.z);g.transform.localScale=new Vector3(1/v.transform.lossyScale.x,1/v.transform.lossyScale.y,1/v.transform.lossyScale.z);
            var ps=g.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=ps.main;main.startLifetime=Mathf.Max(1,(w.HoverTopY-w.transform.position.y)/2);main.startSpeed=2;main.startSize=new ParticleSystem.MinMaxCurve(.025f,.07f);main.startColor=new Color(.7f,.95f,.9f,.35f);main.maxParticles=80;main.simulationSpace=ParticleSystemSimulationSpace.World;
            var em=ps.emission;em.rateOverTime=8;var sh=ps.shape;sh.shapeType=ParticleSystemShapeType.Circle;sh.radius=.7f;sh.rotation=new Vector3(-90,0,0);
            var r=ps.GetComponent<ParticleSystemRenderer>();r.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Chapter01Revision/Materials/Fog_URP.mat");r.shadowCastingMode=ShadowCastingMode.Off;ps.Play();
        }
    }
    static void Audio()
    {
        var original=AssetDatabase.LoadAssetAtPath<SoundPalette>("Assets/_Project/Resources/C1SoundPalette.asset");const string path="Assets/_Project/Resources/C2SoundPalette.asset";var p=AssetDatabase.LoadAssetAtPath<SoundPalette>(path);
        if(!p){p=ScriptableObject.CreateInstance<SoundPalette>();AssetDatabase.CreateAsset(p,path);}EditorUtility.CopySerialized(original,p);p.name="C2SoundPalette";
        Func<string,AudioClip> clip=n=>{string f="Assets/_Project/Audio/C2/"+n+".wav";AssetDatabase.ImportAsset(f);var imp=(AudioImporter)AssetImporter.GetAtPath(f);var settings=imp.defaultSampleSettings;settings.loadType=n.StartsWith("Music")?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;imp.defaultSampleSettings=settings;imp.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<AudioClip>(f);};
        p.explorationMusic=clip("Music_Skylight");p.battleMusic=clip("Music_Arena");p.wind=p.ambience=clip("Wind_Through_Well");p.bell=clip("Bell_Bronze");p.bird=clip("ChimLac_Call");EditorUtility.SetDirty(p);
    }
    static void UI(Scene s)
    {
        var canvas=All<Canvas>(s).Single(c=>c.name=="Canvas_Gameplay");
        SkinHud(s,canvas.transform);
        var lantern=All<LanternController>(s).Single();var hint=Ref<TMP_Text>(lantern,"energyText");Rect(hint.transform,canvas.transform,new Vector2(1,0),new Vector2(1,0),new Vector2(-32,35),new Vector2(550,35));hint.fontSize=19;hint.alignment=TextAlignmentOptions.Right;
        foreach(var inventory in All<InventoryPanel>(s))
        {BuilderUtil.Set(inventory,"normalColor",new Color(.055f,.15f,.13f));BuilderUtil.Set(inventory,"selectedColor",new Color(.36f,.32f,.14f));var panel=Ref<GameObject>(inventory,"panel");if(panel.GetComponent<Image>())panel.GetComponent<Image>().color=new Color(.02f,.055f,.05f,.96f);Ref<TMP_Text>(inventory,"keyItemsText").fontSize=20;}
        foreach(var t in canvas.GetComponentsInChildren<TMP_Text>(true))if(t.name=="Objective"||t.name=="ObjectiveText")t.fontSize=22;
        var prior=canvas.transform.Find("C2_ZoneBanner");if(prior)UnityEngine.Object.DestroyImmediate(prior.gameObject);
        var g=new GameObject("C2_ZoneBanner",typeof(RectTransform),typeof(CanvasGroup),typeof(Image));var rt=(RectTransform)g.transform;rt.SetParent(canvas.transform,false);rt.anchorMin=rt.anchorMax=new Vector2(.5f,1);rt.pivot=new Vector2(.5f,1);rt.anchoredPosition=new Vector2(0,-105);rt.sizeDelta=new Vector2(680,50);g.GetComponent<Image>().color=new Color(.025f,.08f,.075f,.88f);g.GetComponent<Image>().raycastTarget=false;var cg=g.GetComponent<CanvasGroup>();cg.alpha=0;cg.blocksRaycasts=false;cg.interactable=false;
        var label=new GameObject("ZoneName",typeof(RectTransform),typeof(TextMeshProUGUI));label.transform.SetParent(g.transform,false);var lr=(RectTransform)label.transform;lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=lr.offsetMax=Vector2.zero;var txt=label.GetComponent<TMP_Text>();txt.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BuilderUtil.FontPath);txt.fontSize=23;txt.color=new Color(.92f,.86f,.64f);txt.alignment=TextAlignmentOptions.Center;txt.raycastTarget=false;
        var old=All<Chapter02Atmosphere>(s).FirstOrDefault();if(old)UnityEngine.Object.DestroyImmediate(old);
        var a=canvas.gameObject.AddComponent<Chapter02Atmosphere>();BuilderUtil.Set(a,"player",All<PlayerMotor>(s).Single());BuilderUtil.Set(a,"boss",All<BossChimLac>(s).Single());BuilderUtil.Set(a,"banner",cg);BuilderUtil.Set(a,"zoneText",txt);
        BuilderUtil.Set(a,"worldState",All<WorldStateStore>(s).Single());
        var questFrame=canvas.transform.Find("C2_HUD_Theme/Quest_Card");var progress=questFrame.Find("Chapter_Progress");if(!progress){progress=new GameObject("Chapter_Progress",typeof(RectTransform),typeof(TextMeshProUGUI)).transform;Rect(progress,questFrame,new Vector2(0,0),new Vector2(0,0),new Vector2(20,9),new Vector2(420,30));}
        var pt=progress.GetComponent<TMP_Text>();pt.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BuilderUtil.FontPath);pt.fontSize=18;pt.color=new Color(.8f,.76f,.53f);pt.raycastTarget=false;BuilderUtil.Set(a,"progressText",pt);
        var winds=All<WindColumn>(s);var sources=new List<AudioSource>();var palette=AssetDatabase.LoadAssetAtPath<SoundPalette>("Assets/_Project/Resources/C2SoundPalette.asset");
        foreach(var w in winds){var source=w.GetComponent<AudioSource>();if(!source)source=w.gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=true;source.clip=palette.wind;source.outputAudioMixerGroup=palette.sfx;source.spatialBlend=1;source.volume=.16f;source.minDistance=1;source.maxDistance=8;source.rolloffMode=AudioRolloffMode.Linear;sources.Add(source);}
        BuilderUtil.Set(a,"winds",winds);BuilderUtil.Set(a,"windSources",sources.ToArray());
    }
    static RectTransform Rect(Transform t,Transform parent,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 size)
    {var r=(RectTransform)t;r.SetParent(parent,false);r.localScale=Vector3.one;r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;r.anchoredPosition=pos;r.sizeDelta=size;return r;}
    static Transform Panel(string n,Transform p,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 size)
    {
        var g=new GameObject(n,typeof(RectTransform),typeof(Image));Rect(g.transform,p,anchor,pivot,pos,size);g.GetComponent<Image>().color=new Color(.025f,.065f,.06f,.91f);g.GetComponent<Image>().raycastTarget=false;
        var outline=g.AddComponent<Outline>();outline.effectColor=new Color(.58f,.47f,.26f,.85f);outline.effectDistance=new Vector2(1,-1);return g.transform;
    }
    static void SkinHud(Scene s,Transform canvas)
    {
        if(canvas.Find("C2_HUD_Theme"))return;
        var g=new GameObject("C2_HUD_Theme",typeof(RectTransform));var root=(RectTransform)g.transform;root.SetParent(canvas,false);root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;root.SetAsFirstSibling();
        var hud=All<HudBars>(s).Single();var vitals=Panel("Resource_Card",root,new Vector2(0,1),new Vector2(0,1),new Vector2(32,-28),new Vector2(330,176));
        var oldLantern=Ref<GameObject>(hud,"lanternGroup");string[] fills={"healthFill","staminaFill","lanternFill"},labels={"healthLabel","staminaLabel","lanternLabel"};Color[] colors={new Color(.72f,.22f,.22f),new Color(.27f,.61f,.43f),new Color(.85f,.64f,.29f)};
        for(int i=0;i<3;i++)
        {
            var fill=Ref<RectTransform>(hud,fills[i]);var label=Ref<TMP_Text>(hud,labels[i]);var oldParent=fill.parent;
            var row=Panel("Resource_"+i,vitals,new Vector2(0,1),new Vector2(0,1),new Vector2(16,-10-i*52),new Vector2(298,48));row.GetComponent<Image>().color=Color.clear;
            Rect(label.transform,row,new Vector2(0,1),new Vector2(0,1),Vector2.zero,new Vector2(298,26));label.fontSize=18;label.color=new Color(.93f,.93f,.84f);label.alignment=TextAlignmentOptions.Left;
            var track=Panel("Track",row,Vector2.zero,Vector2.zero,new Vector2(0,4),new Vector2(298,10));track.GetComponent<Image>().color=new Color(.12f,.17f,.15f);
            fill.SetParent(track,false);fill.anchorMin=Vector2.zero;fill.anchorMax=Vector2.one;fill.offsetMin=fill.offsetMax=Vector2.zero;fill.GetComponent<Image>().color=colors[i];
            if(oldParent!=canvas&&oldParent!=hud.transform)oldParent.gameObject.SetActive(false);
            if(i==2)BuilderUtil.Set(hud,"lanternGroup",row.gameObject);
        }
        if(oldLantern)oldLantern.SetActive(false);
        var potion=Ref<TMP_Text>(hud,"potionText");var quick=Panel("Potion_Quickslot",root,Vector2.zero,Vector2.zero,new Vector2(32,30),new Vector2(285,56));Rect(potion.transform,quick,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(14,0),new Vector2(260,35));potion.fontSize=21;
        var quest=All<ChapterQuestTracker>(s).Single();var objective=Ref<TMP_Text>(quest,"objectiveText");var frame=Panel("Quest_Card",root,Vector2.one,Vector2.one,new Vector2(-32,-28),new Vector2(455,176));Rect(objective.transform,frame,new Vector2(0,1),new Vector2(0,1),new Vector2(20,-18),new Vector2(415,140));objective.fontSize=21;objective.alignment=TextAlignmentOptions.Left;
        var prompt=Ref<TMP_Text>(All<InteractionScanner>(s).Single(),"promptText");Rect(prompt.transform,root,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,146),new Vector2(1000,55));prompt.fontSize=24;prompt.color=new Color(.97f,.91f,.73f);prompt.alignment=TextAlignmentOptions.Center;
        var shadow=prompt.gameObject.GetComponent<Shadow>();if(!shadow)shadow=prompt.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.95f);shadow.effectDistance=new Vector2(1,-2);
        foreach(var b in All<Button>(s)){var image=b.GetComponent<Image>();if(image)image.color=new Color(.07f,.20f,.18f,.98f);var c=b.colors;c.highlightedColor=new Color(.73f,.9f,.76f);c.pressedColor=new Color(.52f,.73f,.60f);b.colors=c;}
        foreach(var scale in All<CanvasScaler>(s)){scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1920,1080);scale.matchWidthOrHeight=.5f;}
    }
}
