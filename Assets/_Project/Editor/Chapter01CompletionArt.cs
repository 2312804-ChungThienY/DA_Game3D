using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;

public static class Chapter01CompletionArt
{
    public const string Art="Assets/_Project/Art/Chapter01Completion";
    const string Cave="Assets/A09 - Mines and Cave Set/MInesAndCaveSet/prefabs/Wall/";
    public static void Folder(string path){if(AssetDatabase.IsValidFolder(path))return;var p=System.IO.Path.GetDirectoryName(path).Replace('\\','/');Folder(p);AssetDatabase.CreateFolder(p,System.IO.Path.GetFileName(path));}
    public static Transform Group(string name,Transform parent=null){var g=new GameObject(name);g.transform.SetParent(parent,false);return g.transform;}
    public static Material Rock=>AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Chapter01Expanded/Materials/Limestone_Moss.mat");
    public static Material Material(string name,Color color,float emission=0)
    {
        Folder(Art+"/Materials");var path=Art+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
        m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.12f);m.SetFloat("_Cull",0);
        if(emission>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*emission);}EditorUtility.SetDirty(m);return m;
    }
    public static Mesh Mesh(string name,List<Vector3> v,List<int> t,List<Vector2> uv=null)
    {
        Folder(Art+"/Meshes");string path=Art+"/Meshes/"+name+".asset";var m=AssetDatabase.LoadAssetAtPath<Mesh>(path);bool fresh=!m;
        if(fresh)m=new Mesh{name=name};else m.Clear();if(v.Count>65000)m.indexFormat=IndexFormat.UInt32;
        m.SetVertices(v);m.SetTriangles(t,0);m.SetUVs(0,uv??v.Select(q=>new Vector2(q.x,q.z)*.22f).ToList());m.RecalculateNormals();m.RecalculateBounds();
        if(fresh)AssetDatabase.CreateAsset(m,path);else EditorUtility.SetDirty(m);return m;
    }
    public static GameObject Surface(string name,Transform parent,Mesh mesh,Material material,bool collider=false)
    {
        var g=Group(name,parent).gameObject;g.layer=3;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=material;
        if(collider)g.AddComponent<MeshCollider>().sharedMesh=mesh;return g;
    }
    public static GameObject Model(string source,string name,Transform parent,Vector3 position,float height,float yaw=0,Material material=null)
    {
        var outer=Group(name);outer.position=position;var fit=Group("Fit",outer);var asset=AssetDatabase.LoadAssetAtPath<GameObject>(source);if(!asset)throw new InvalidOperationException("Missing FBX/prefab: "+source);
        var g=(GameObject)PrefabUtility.InstantiatePrefab(asset,fit);g.transform.localRotation=Quaternion.Euler(0,yaw,0)*g.transform.localRotation;
        PrefabUtility.RecordPrefabInstancePropertyModifications(g.transform);
        var b=CampEnvironmentSetup.BoundsOf(g);fit.localScale=Vector3.one*height/Mathf.Max(.01f,b.size.y);b=CampEnvironmentSetup.BoundsOf(g);fit.position-=new Vector3(b.center.x-position.x,b.min.y-position.y,b.center.z-position.z);
        foreach(var c in g.GetComponentsInChildren<Collider>(true)){c.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(c);}
        foreach(var rb in g.GetComponentsInChildren<Rigidbody>(true)){rb.isKinematic=true;rb.detectCollisions=false;PrefabUtility.RecordPrefabInstancePropertyModifications(rb);}
        foreach(var rr in g.GetComponentsInChildren<Renderer>(true)){rr.gameObject.layer=3;if(material)rr.sharedMaterials=rr.sharedMaterials.Select(_=>material).ToArray();PrefabUtility.RecordPrefabInstancePropertyModifications(rr);PrefabUtility.RecordPrefabInstancePropertyModifications(rr.gameObject);}
        outer.SetParent(parent,true);return outer.gameObject;
    }
    static void Mass(Transform parent,string key,float xmin,float xmax,float z,float baseY,float top)
    {
        var g=Group("Collision_"+key,parent).gameObject;g.layer=3;var box=g.AddComponent<BoxCollider>();box.center=new Vector3((xmin+xmax)*.5f,(baseY+top)*.5f,z);box.size=new Vector3(xmax-xmin,top-baseY,2.6f);
        var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();int nx=Mathf.CeilToInt((xmax-xmin)/1.1f),ny=Mathf.CeilToInt((top-baseY)/1.3f);
        for(int side=-1;side<=1;side+=2)
        {
            int offset=v.Count;
            for(int y=0;y<=ny;y++)for(int x=0;x<=nx;x++)
            {
                float px=Mathf.Lerp(xmin,xmax,(float)x/nx),py=Mathf.Lerp(baseY,top,(float)y/ny);
                float dz=1.3f+((x==0||x==nx||y==0||y==ny)?0:Mathf.Sin(x*2.17f+y*1.61f)*.24f);
                v.Add(new Vector3(px,py,z+side*dz));uv.Add(new Vector2(px*.19f,py*.21f));
            }
            for(int y=0;y<ny;y++)for(int x=0;x<nx;x++)
            {int n=offset+y*(nx+1)+x;t.AddRange(side<0?new[]{n,n+nx+1,n+1,n+1,n+nx+1,n+nx+2}:new[]{n,n+1,n+nx+1,n+1,n+nx+2,n+nx+1});}
        }
        Surface("Limestone_"+key,parent,Mesh(key,v,t,uv),Rock);
    }
    static void Vestibule(Transform parent,List<Vector3> points,string key)
    {
        const int profile=17;var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();
        for(int i=0;i<points.Count;i++)
        {
            var direction=(points[Mathf.Min(i+1,points.Count-1)]-points[Mathf.Max(0,i-1)]).normalized;var right=new Vector3(direction.z,0,-direction.x);
            for(int j=0;j<profile;j++)
            {
                float a=j*Mathf.PI/(profile-1);v.Add(points[i]+right*(-Mathf.Cos(a)*5.7f)+Vector3.up*(Mathf.Sin(a)*10.7f-.07f));uv.Add(new Vector2(i*.27f,j*.23f));
                if(i>0&&j<profile-1){int n=(i-1)*profile+j,b=n+profile;t.AddRange(new[]{n,n+1,b,n+1,b+1,b});}
            }
        }
        Surface("Closed_Neck_Vault_"+key,parent,Mesh("ClosedNeck_"+key,v,t,uv),Rock,true);
        v=new List<Vector3>();t=new List<int>();
        for(int i=0;i<points.Count;i++)
        {
            var d=(points[Mathf.Min(i+1,points.Count-1)]-points[Mathf.Max(0,i-1)]).normalized;var right=new Vector3(d.z,0,-d.x);
            v.Add(points[i]-right*5.8f-Vector3.up*.06f);v.Add(points[i]+right*5.8f-Vector3.up*.06f);
            if(i>0){int n=i*2;t.AddRange(new[]{n-2,n,n-1,n-1,n,n+1});}
        }
        Surface("Neck_Safety_Floor_"+key,parent,Mesh("NeckFloor_"+key,v,t),AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Chapter01Expanded/Materials/Cave_Soil.mat"),true);
    }
    public static void RepairHull(Scene scene,Action<string> check)
    {
        var world=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Single(t=>t.name=="World_Geometry");
        if(world.Find("ENV_C1_ClosedJunctions"))throw new InvalidOperationException("Junction repair already installed; preserve scene changes.");
        var root=Group("ENV_C1_ClosedJunctions",world);
        Vector2[] sizes={new Vector2(18,18),new Vector2(24,24),new Vector2(20,19),new Vector2(24,21),new Vector2(23,22)};
        float[] heights={22,25,16,19,21};
        for(int link=0;link<4;link++)for(int end=0;end<2;end++)
        {
            int room=link+end;var center=Chapter01ExpandedLayout.Centers[room];float sign=end==0?1:-1;
            float z=center.z+sign*(sizes[room].y-4.5f);string key=link+"_"+end;
            var p=Group("Vestibule_"+key,root);
            Mass(p,key+"_Left",center.x-sizes[room].x*1.4f,center.x-4.3f,z,-.12f,heights[room]+1);
            Mass(p,key+"_Right",center.x+4.3f,center.x+sizes[room].x*1.4f,z,-.12f,heights[room]+1);
            Mass(p,key+"_Crown",center.x-4.3f,center.x+4.3f,z,6.0f,heights[room]+1);
            var route=Chapter01ExpandedLayout.SampleLink(link);List<Vector3> points;
            if(end==0){points=new List<Vector3>{new Vector3(center.x,0,z-3),new Vector3(center.x,0,z)};points.AddRange(route.Take(13));}
            else{points=route.Skip(Mathf.Max(0,route.Count-13)).ToList();points.Add(new Vector3(center.x,0,z));points.Add(new Vector3(center.x,0,z+3));}
            Vestibule(p,points,key);
            for(int side=-1;side<=1;side+=2)Model(Cave+"Cave_Rocks_B.prefab","Rock_Arch_Foot_"+side,p,new Vector3(center.x+side*8.7f,-.12f,z-sign*1.6f),8.8f,side*71,Rock);
            check("Closed full-height vestibule "+key+" at "+new Vector3(center.x,0,z)+"; overlapping vault/floor, 8.6m opening, FBX rock feet.");
        }
        Bake(scene);check("Eight sealed room/link junctions; skylights inside rooms retained; original gameplay roots/IDs/spawns untouched.");
    }
    public static void Bake(Scene scene)
    {
        Physics.SyncTransforms();var nav=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<NavMeshSurface>(true)).Single();nav.BuildNavMesh();
        var path="Assets/_Project/Art/Chapter01Expanded/Meshes/NavMesh_ExpandedC1.asset";var data=AssetDatabase.LoadAssetAtPath<UnityEngine.AI.NavMeshData>(path);EditorUtility.CopySerialized(nav.navMeshData,data);nav.RemoveData();nav.navMeshData=data;nav.AddData();EditorUtility.SetDirty(data);
    }
}
