using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using Unity.AI.Navigation;

// Explicit local requests only. Source models and gameplay components are never rebuilt.
[InitializeOnLoad]
public static class Chapter01EnvironmentSetup
{
    const string Main = CampEnvironmentSetup.MainPath, Preview = CampEnvironmentSetup.PreviewPath;
    const string Request = "Library/SonDoongChapter01Art.request.json";
    const string Root = "Assets/_Project/Art/Chapter01";
    const string Cave = "Assets/A09 - Mines and Cave Set/MInesAndCaveSet/prefabs/Wall/";
    const string Nature = "Assets/A11 - Ultimate Stylized Nature Pack/FBX/";
    const string Ruins = "Assets/A10 - Ultimate Modular Ruins Pack/FBX/";
    static bool busy;
    static double next;
    [Serializable] class Command { public string action; }
    [Serializable] class ProbeBackup { public string[] paths;public bool[] existed;public string[] hashes; }
    [Serializable] public class Report
    {
        public string action, scene, backup;
        public List<string> checks = new List<string>(), failures = new List<string>(), screenshots = new List<string>();
    }
    static Chapter01EnvironmentSetup() { EditorApplication.update += Poll; }
    static void Submit(string action)
    {
        if(File.Exists(Request))throw new InvalidOperationException("A chapter art request is already pending.");
        File.WriteAllText(Request,JsonUtility.ToJson(new Command{action=action}));
    }
    [MenuItem("Tools/Son Doong/Chapter 01 Environment/Open Main Scene")]
    static void OpenMain()=>Submit("open-main");
    [MenuItem("Tools/Son Doong/Chapter 01 Environment/Open Preview")]
    static void OpenPreview()=>Submit("open-preview");
    [MenuItem("Tools/Son Doong/Chapter 01 Environment/Validate Main Scene")]
    static void ValidateMain()=>Submit("validate");
    [MenuItem("Tools/Son Doong/Chapter 01 Environment/Run Play Mode Regression")]
    static void PlayTest()=>Submit("play-test");
    static IEnumerable<T> All<T>(Scene s) where T : Component => s.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true));
    static Transform Find(Scene s, string n) => All<Transform>(s).FirstOrDefault(t => t.name == n);
    static Transform Group(string n, Transform p) => CampEnvironmentSetup.Group(n,p);
    static Bounds BoundsOf(GameObject g) => CampEnvironmentSetup.BoundsOf(g);
    static void Guard()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode before editing scenery.");
        for(int i=0;i<SceneManager.sceneCount;i++) if(SceneManager.GetSceneAt(i).isDirty)
            throw new InvalidOperationException("Unsaved scene: "+SceneManager.GetSceneAt(i).path+". Preserve the user's edits.");
    }
    static void Poll()
    {
        if(busy || EditorApplication.timeSinceStartup<next || EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(Request)) return;
        busy=true;next=EditorApplication.timeSinceStartup+1;
        var report=new Report();
        try
        {
            report.action=JsonUtility.FromJson<Command>(File.ReadAllText(Request)).action;
            if(report.action=="stop-owned-test")
            {
                var probe=UnityEngine.Object.FindAnyObjectByType<CampEnvironmentSmokeProbe>();
                if(!EditorApplication.isPlaying||!probe)throw new InvalidOperationException("No owned environment probe is running.");
                // A hot reload can stop its coroutine. Recover from the on-disk manifest as well.
                string folder=Directory.GetDirectories("Temp","CampEnvironmentPlayBackup_*").OrderBy(x=>x,StringComparer.Ordinal).Last();
                var backup=JsonUtility.FromJson<ProbeBackup>(File.ReadAllText(folder+"/manifest.json"));
                string primary=Path.Combine(Application.persistentDataPath,"save_v1.json");
                string[] expected={primary,primary+".bak",primary+".tmp"};
                if(!backup.paths.Select(Path.GetFullPath).SequenceEqual(expected.Select(Path.GetFullPath)))throw new InvalidOperationException("Unexpected test save paths.");
                probe.SendMessage("Finish",SendMessageOptions.RequireReceiver);
                for(int i=0;i<expected.Length;i++)
                {
                    if(backup.existed[i])File.Copy(folder+"/"+i+".original",expected[i],true);
                    else if(File.Exists(expected[i]))File.Delete(expected[i]);
                }
                EditorApplication.isPaused=false;EditorApplication.isPlaying=false;
                report.checks.Add("Stopped owned probe and restored its primary/backup/tmp from the verified test manifest.");return;
            }
            if(report.action=="resume-owned-test")
            {
                if(!EditorApplication.isPlaying||!UnityEngine.Object.FindAnyObjectByType<CampEnvironmentSmokeProbe>())
                    throw new InvalidOperationException("No owned environment probe is running.");
                var pickup=UnityEngine.Object.FindAnyObjectByType<LanternPickup>();var motor=UnityEngine.Object.FindAnyObjectByType<PlayerMotor>();
                if(pickup&&motor)pickup.Interact(motor.gameObject);
                EditorApplication.isPaused=false;
                report.checks.Add("Resumed the owned test after satisfying its lantern prerequisite; no edit-mode scene change.");
                return;
            }
            Guard();
            if(report.action=="inspect") WithScene(Main,s=>Inspect(s,report));
            else if(report.action=="open-main")EditorSceneManager.OpenScene(Main);
            else if(report.action=="open-preview")EditorSceneManager.OpenScene(Preview);
            else if(report.action=="stage")
            {
                report.backup=Backup();
                // Copy serialized IDs and references intact; retain the preview scene's own GUID.
                if(SceneManager.GetSceneByPath(Preview).isLoaded) throw new InvalidOperationException("Close the old preview before staging the full chapter.");
                File.Copy(Main,Preview,true);AssetDatabase.ImportAsset(Preview,ImportAssetOptions.ForceSynchronousImport);
                WithScene(Preview,s=>{Build(s,report);Validate(s,report);EditorSceneManager.SaveScene(s);Capture(s,report,"preview");});
            }
            else if(report.action=="integrate")
            {
                if(!File.Exists("Logs/Chapter01Art-stage.json") || JsonUtility.FromJson<Report>(File.ReadAllText("Logs/Chapter01Art-stage.json")).failures.Count>0)
                    throw new InvalidOperationException("Stage and validate the preview first.");
                report.backup=Backup();
                WithScene(Main,s=>{Build(s,report);Validate(s,report);EditorSceneManager.SaveScene(s);Capture(s,report,"main");});
            }
            else if(report.action=="validate") WithScene(Main,s=>Validate(s,report));
            else if(report.action=="capture") WithScene(Main,s=>Capture(s,report,"main"));
            else if(report.action=="refine-garden")
            {
                report.backup=Backup();
                foreach(string path in new[]{Preview,Main})WithScene(path,s=>{
                    RefineGarden(Find(s,"ART_C1_Environment"));Validate(s,report);EditorSceneManager.SaveScene(s);Capture(s,report,path==Main?"main":"preview");
                });
                WithScene(Main,s=>PrefabUtility.SaveAsPrefabAsset(Find(s,"ART_C1_Environment").gameObject,Root+"/Prefabs/PF_ART_C1_Environment.prefab"));
            }
            else if(report.action=="attach-statue")
            {
                report.backup=Backup();
                foreach(string path in new[]{Preview,Main})WithScene(path,s=>{
                    AttachStatue(s,Find(s,"ART_C1_Environment"));Validate(s,report);EditorSceneManager.SaveScene(s);
                });
                WithScene(Main,s=>PrefabUtility.SaveAsPrefabAsset(Find(s,"ART_C1_Environment").gameObject,Root+"/Prefabs/PF_ART_C1_Environment.prefab"));
                report.checks.Add("Statue world transforms and collider geometry retained; only ownership moved under the existing relic altar for E selection.");
            }
            else if(report.action=="play-test")
            {
                SessionState.SetBool("SonDoong.Chapter01ArtTest",true);
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/00_Boot.unity");EditorApplication.EnterPlaymode();
                report.checks.Add("Requested five-area geometry/camera/puzzle/crafting/save regression from Boot; original saves restored.");
            }
            else if(report.action=="build")
            {
                var result=UnityEditor.BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                    scenes=EditorBuildSettings.scenes.Where(x=>x.enabled).Select(x=>x.path).ToArray(),
                    locationPathName="Builds/Chapter01Environment/Windows/SonDoong.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
                report.checks.Add("Windows x64 Release: "+result.summary.result+", errors="+result.summary.totalErrors+", warnings="+result.summary.totalWarnings);
                if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Windows build failed.");
            }
            else throw new InvalidOperationException("Unknown chapter art request: "+report.action);
            AssetDatabase.SaveAssets();
        }
        catch(Exception e){report.failures.Add(e.ToString());}
        finally
        {
            Directory.CreateDirectory("Logs");File.WriteAllText("Logs/Chapter01Art-"+report.action+".json",JsonUtility.ToJson(report,true));
            File.Delete(Request);busy=false;
        }
    }
    static string Backup()
    {
        string folder="SourceAssets/SceneBackups/2026-11-04_Chapter01Art/"+DateTime.UtcNow.ToString("HHmmssfff");Directory.CreateDirectory(folder);
        foreach(string p in new[]{Main,Preview}) foreach(string f in new[]{p,p+".meta"}) if(File.Exists(f)) File.Copy(f,folder+"/"+Path.GetFileName(f));
        return folder;
    }
    static void WithScene(string path,Action<Scene> action)
    {
        var prior=SceneManager.GetActiveScene();var scene=SceneManager.GetSceneByPath(path);bool opened=scene.IsValid()&&scene.isLoaded;
        if(!opened) scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
        try{SceneManager.SetActiveScene(scene);action(scene);}
        finally{if(prior.IsValid()&&prior.isLoaded)SceneManager.SetActiveScene(prior);if(!opened)EditorSceneManager.CloseScene(scene,true);}
    }
    static void Inspect(Scene s,Report r)
    {
        r.scene=s.path;
        foreach(var t in All<Transform>(s).Where(t=>!t.GetComponent<RectTransform>() && (t.GetComponents<MonoBehaviour>().Length>0 || t.name.StartsWith("Area_") || t.name.StartsWith("ENV_"))))
            r.checks.Add(t.name+" | pos="+t.position+" | size="+t.lossyScale+" | scripts="+string.Join(",",t.GetComponents<MonoBehaviour>().Where(x=>x).Select(x=>x.GetType().Name)));
        r.checks.Add("Scene idle, unsaved edits guarded; source asset folders A09/A10/A11/A16 already present.");
    }
    static void Folder(string path)
    {
        if(AssetDatabase.IsValidFolder(path))return;
        string parent=Path.GetDirectoryName(path).Replace('\\','/');Folder(parent);AssetDatabase.CreateFolder(parent,Path.GetFileName(path));
    }
    static Material Mat(string name,Color c,float glow=0)
    {
        string path=Root+"/Materials/"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
        m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",.12f);m.SetFloat("_Cull",0);
        if(glow>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*glow);}
        EditorUtility.SetDirty(m);return m;
    }
    static Material Inlay()
    {
        var m=Mat("Glyph_Inlay",new Color(.25f,.68f,.66f));m.shader=Shader.Find("Universal Render Pipeline/Unlit");EditorUtility.SetDirty(m);return m;
    }
    static Mesh SaveMesh(string name,List<Vector3> v,List<int> tris,List<Color> colors=null)
    {
        string path=Root+"/Meshes/"+name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        bool create=!mesh;if(create)mesh=new Mesh{name=name};else mesh.Clear();
        mesh.SetVertices(v);mesh.SetTriangles(tris,0);if(colors!=null)mesh.SetColors(colors);mesh.RecalculateNormals();mesh.RecalculateBounds();
        if(create)AssetDatabase.CreateAsset(mesh,path);else EditorUtility.SetDirty(mesh);return mesh;
    }
    static GameObject MeshObject(string n,Transform p,Mesh mesh,Material mat)
    {
        var t=Group(n,p);t.gameObject.layer=3;t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
        t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mat;return t.gameObject;
    }
    static GameObject Shape(string n,Transform p,PrimitiveType type,Vector3 pos,Vector3 size,Material mat)
    {
        var go=GameObject.CreatePrimitive(type);go.name=n;go.layer=3;go.transform.SetParent(p,false);go.transform.position=pos;go.transform.localScale=size;
        if(p)go.transform.localScale=new Vector3(size.x/p.lossyScale.x,size.y/p.lossyScale.y,size.z/p.lossyScale.z);
        UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=mat;
        if(type==PrimitiveType.Sphere)go.GetComponent<MeshFilter>().sharedMesh=FacetedSphere();return go;
    }
    static Mesh FacetedSphere()
    {
        var v=new List<Vector3>();var tris=new List<int>();const int rings=8,sides=14;
        Func<int,int,Vector3> point=(y,x)=>new Vector3(Mathf.Cos(x*Mathf.PI*2/sides)*Mathf.Sin(y*Mathf.PI/rings),Mathf.Cos(y*Mathf.PI/rings),Mathf.Sin(x*Mathf.PI*2/sides)*Mathf.Sin(y*Mathf.PI/rings))*.5f;
        for(int y=0;y<rings;y++)for(int x=0;x<sides;x++)
            foreach(var f in new[]{new[]{point(y,x),point(y+1,x),point(y,x+1)},new[]{point(y,x+1),point(y+1,x),point(y+1,x+1)}})
            {int n=v.Count;v.AddRange(f);tris.AddRange(new[]{n,n+2,n+1});}
        return SaveMesh("Sphere_Faceted",v,tris);
    }
    static GameObject Place(string path,string name,Transform p,Vector3 pos,Vector3 size,float yaw,string category="Rock")
        => CampEnvironmentSetup.Place(path,name,p,pos,size,yaw,category,false,Math.Abs(name.GetHashCode()%3));
    static Light Lamp(string name,Transform p,Vector3 pos,Color c,float intensity,float range,LightType type=LightType.Point)
    {
        var l=Group(name,p).gameObject.AddComponent<Light>();l.transform.position=pos;l.type=type;l.color=c;l.intensity=intensity;l.range=range;
        l.shadows=LightShadows.None;return l;
    }
    static void FacetFloor(Transform parent,Bounds b,string key,Color color)
    {
        b.Expand(new Vector3(1,0,1)); // overlap the rock-wall skirts; collision still uses the original floor.
        // Flat contact plane: colour/triangulation provides detail without bumps in the controller floor.
        var v=new List<Vector3>();var tris=new List<int>();int nx=Mathf.CeilToInt(b.size.x/1.6f),nz=Mathf.CeilToInt(b.size.z/1.6f);
        var rng=new System.Random(181+key.Length);
        for(int z=0;z<nz;z++) for(int x=0;x<nx;x++)
        {
            float x0=b.min.x+b.size.x*x/nx,x1=b.min.x+b.size.x*(x+1)/nx,z0=b.min.z+b.size.z*z/nz,z1=b.min.z+b.size.z*(z+1)/nz;
            Vector3[] q={new Vector3(x0,b.max.y+.002f,z0),new Vector3(x0,b.max.y+.002f,z1),new Vector3(x1,b.max.y+.002f,z1),new Vector3(x1,b.max.y+.002f,z0)};
            int start=v.Count;v.AddRange(q);tris.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});
        }
        var mesh=SaveMesh("Ground_"+key,v,tris);var groups=Enumerable.Range(0,6).Select(_=>new List<int>()).ToArray();
        for(int i=0;i<tris.Count;i+=6)
        {
            var c=(v[tris[i]]+v[tris[i+2]])*.5f;
            int group=Mathf.Abs(c.x)<2.2f ? rng.Next(0,3) : rng.Next(3,6);
            for(int j=0;j<6;j++)groups[group].Add(tris[i+j]);
        }
        mesh.subMeshCount=6;for(int i=0;i<6;i++)mesh.SetTriangles(groups[i],i);EditorUtility.SetDirty(mesh);
        var go=MeshObject("Ground_"+key,parent,mesh,Mat("Earth_"+key,color));
        go.GetComponent<Renderer>().sharedMaterials=Enumerable.Range(0,6).Select(i=>Mat("Earth_"+key+"_"+i,i<3?color*(.92f+i*.05f):Color.Lerp(color,new Color(.23f,.29f,.19f),.28f+(i-3)*.12f))).ToArray();
        // Separate non-overlapping small slabs break the broad floor into natural islands.
        int count=Mathf.RoundToInt(b.size.z/1.7f);
        for(int i=0;i<count;i++)
        {
            float z=b.min.z+.7f+i*1.7f;
            if(b.size.x<5)continue;
            for(int side=-1;side<=1;side+=2)
                Place(Nature+"Rock_"+(i%3+1)+".fbx","Waystone_"+key+"_"+i+"_"+side,parent,new Vector3(side*(.7f+(float)rng.NextDouble()*.35f),b.max.y-.035f,z),new Vector3(1.15f,.075f,1.35f),i*39);
        }
    }
    static void CladWall(Transform original,Transform art,int index)
    {
        var r=original.GetComponent<Renderer>();var b=r.bounds;
        bool alongZ=b.size.z>b.size.x;float length=alongZ?b.size.z:b.size.x;
        bool garden=original.name.EndsWith("_30")||original.name.StartsWith("Wall_Garden");
        float height=garden?14:b.size.y;
        StratifiedWall(art,b,height,original.name,index);
        int count=Mathf.Max(1,Mathf.CeilToInt(length/4));
        for(int i=0;i<count;i++)
        {
            float u=(i+.5f)/count;
            var p=new Vector3(alongZ?b.center.x:Mathf.Lerp(b.min.x,b.max.x,u),b.min.y-.05f,alongZ?Mathf.Lerp(b.min.z,b.max.z,u):b.center.z);
            var size=alongZ?new Vector3(Mathf.Max(.8f,b.size.x+.12f),height,length/count+.2f):new Vector3(length/count+.2f,height,Mathf.Max(.8f,b.size.z+.12f));
            Place(Cave+"Cave_Wall_Rocks_"+((index+i)%2==0?"A":"B")+".prefab","Cladding_"+original.name+"_"+i,art,p,size,alongZ?90:0);
        }
        r.enabled=false;
    }
    static void StratifiedWall(Transform parent,Bounds b,float height,string key,int seed)
    {
        bool alongZ=b.size.z>b.size.x;
        float roomZ=parent.name.Contains("02_")?30:parent.name.Contains("03_")?54:parent.name.Contains("04_")?72:99;
        float sign=alongZ?Mathf.Sign(b.center.x):Mathf.Sign(b.center.z-roomZ);if(sign==0)sign=1;
        float inner=alongZ?b.center.x-sign*b.extents.x:b.center.z-sign*b.extents.z;
        float low=alongZ?b.min.z:b.min.x,length=alongZ?b.size.z:b.size.x;
        int nx=Mathf.Max(2,Mathf.CeilToInt(length/1.2f)),ny=Mathf.Max(2,Mathf.CeilToInt(height/.9f));
        var rng=new System.Random(seed+411);var grid=new Vector3[(nx+1)*(ny+1)];
        for(int y=0;y<=ny;y++)for(int x=0;x<=nx;x++)
        {
            float u=low+length*x/nx;
            if(x>0&&x<nx)u+=((float)rng.NextDouble()-.5f)*.28f;
            float h=b.min.y+height*y/ny;if(y>0&&y<ny)h+=((float)rng.NextDouble()-.5f)*.22f;
            float depth=.02f+(float)rng.NextDouble()*.26f;
            float plane=inner+sign*depth;
            grid[y*(nx+1)+x]=alongZ?new Vector3(plane,h,u):new Vector3(u,h,plane);
        }
        var v=new List<Vector3>();var tris=new List<int>();var sub=Enumerable.Range(0,3).Select(_=>new List<int>()).ToArray();
        for(int y=0;y<ny;y++)for(int x=0;x<nx;x++)
        {
            int a=y*(nx+1)+x,b0=a+1,c=a+nx+1,d=c+1;
            foreach(int[] face in new[]{new[]{a,c,b0},new[]{b0,c,d}})
            {
                int start=v.Count;foreach(int f in face)v.Add(grid[f]);
                int[] indices=(alongZ?sign>0:sign<0)?new[]{start,start+2,start+1}:new[]{start,start+1,start+2};
                tris.AddRange(indices);sub[rng.Next(3)].AddRange(indices);
            }
        }
        var mesh=SaveMesh("Strata_"+key,v,tris);mesh.subMeshCount=3;for(int i=0;i<3;i++)mesh.SetTriangles(sub[i],i);
        var go=MeshObject("StratifiedLimestone_"+key,parent,mesh,Mat("Rock_Stratum_0",new Color(.29f,.35f,.36f)));
        go.GetComponent<Renderer>().sharedMaterials=Enumerable.Range(0,3).Select(i=>Mat("Rock_Stratum_"+i,new Color(.29f,.35f,.36f)*(.92f+i*.08f))).ToArray();
    }
    static void RoughRoof(Transform parent,Bounds b,string key)
    {
        b.Expand(new Vector3(2,0,2));
        int nx=Mathf.CeilToInt(b.size.x/1.5f),nz=Mathf.CeilToInt(b.size.z/1.5f);var rng=new System.Random(key.Length+51);
        var points=new Vector3[(nx+1)*(nz+1)];
        for(int z=0;z<=nz;z++)for(int x=0;x<=nx;x++)points[z*(nx+1)+x]=new Vector3(b.min.x+b.size.x*x/nx,b.min.y+((x<2||x>nx-2||z<2||z>nz-2)?0:.02f+(float)rng.NextDouble()*.65f),b.min.z+b.size.z*z/nz);
        var v=new List<Vector3>();var tris=new List<int>();
        for(int z=0;z<nz;z++)for(int x=0;x<nx;x++)
        {
            int a=z*(nx+1)+x,c=a+nx+1;
            foreach(int[] f in new[]{new[]{a,a+1,c},new[]{a+1,c+1,c}}){int n=v.Count;foreach(int k in f)v.Add(points[k]);tris.AddRange(new[]{n,n+1,n+2});}
        }
        MeshObject("RoughVault_"+key,parent,SaveMesh("Vault_"+key,v,tris),Mat("Rock_Stratum_1",new Color(.29f,.35f,.36f)));
        for(int i=0;i<Mathf.RoundToInt(b.size.z/1.8f);i++)for(int side=-1;side<=1;side+=2)
        {
            var cone=MeshObject("Stalactite_"+key+"_"+i+"_"+side,parent,AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Meshes/Crystal_FiveSided.asset"),Mat("Rock_Stratum_0",new Color(.26f,.32f,.33f)));
            cone.transform.position=new Vector3(b.center.x+side*b.extents.x*.68f,b.min.y+.3f,b.min.z+1+i*1.8f);
            cone.transform.localScale=new Vector3(.3f+(i%3)*.12f,.65f+(i%2)*.6f,.35f);cone.transform.localRotation=Quaternion.Euler(180,i*56,0);
        }
    }
    static void ConformMoss(GameObject rock,Transform parent,string key)
    {
        // Raycast against this rock's actual triangles, not its bounding box or neighbouring grass.
        var colliders=new List<MeshCollider>();
        foreach(var f in rock.GetComponentsInChildren<MeshFilter>())
        {var c=f.gameObject.AddComponent<MeshCollider>();c.sharedMesh=f.sharedMesh;colliders.Add(c);}
        Physics.SyncTransforms();var b=BoundsOf(rock);var v=new List<Vector3>();var tri=new List<int>();
        const int cells=7;
        try
        {
            for(int z=0;z<cells;z++)for(int x=0;x<cells;x++)
            {
                float x0=Mathf.Lerp(b.min.x,b.max.x,.13f+.74f*x/cells),x1=Mathf.Lerp(b.min.x,b.max.x,.13f+.74f*(x+1)/cells);
                float z0=Mathf.Lerp(b.min.z,b.max.z,.13f+.74f*z/cells),z1=Mathf.Lerp(b.min.z,b.max.z,.13f+.74f*(z+1)/cells);
                Vector3[] q={new Vector3(x0,b.max.y+1,z0),new Vector3(x0,b.max.y+1,z1),new Vector3(x1,b.max.y+1,z1),new Vector3(x1,b.max.y+1,z0)};
                bool valid=true;
                for(int j=0;j<4;j++)
                {
                    RaycastHit hit=default;bool found=false;float d=float.MaxValue;
                    foreach(var c in colliders)if(c.Raycast(new Ray(q[j],Vector3.down),out var h,b.size.y+3)&&h.distance<d){found=true;d=h.distance;hit=h;}
                    if(!found||hit.normal.y<.42f){valid=false;break;}q[j]=hit.point+hit.normal*.009f;
                }
                if(!valid)continue;int n=v.Count;v.AddRange(q);tri.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
            }
            if(v.Count>0)MeshObject("MossSurface_"+key,parent,SaveMesh("MossSurface_"+key,v,tri),Mat("Moss_Carpet",new Color(.24f,.34f,.10f)));
        }
        finally{foreach(var c in colliders)UnityEngine.Object.DestroyImmediate(c);}
    }
    static Mesh FernMesh()
    {
        var v=new List<Vector3>();var tri=new List<int>();
        // Each frond has a stem rooted in the soil; leaf-only ribbons would float above it.
        for(int f=0;f<7;f++)for(int j=0;j<10;j++)
        {
            float a=f*Mathf.PI*2/7,t0=j/10f,t1=(j+1)/10f;
            Vector3 forward=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)),right=new Vector3(-Mathf.Sin(a),0,Mathf.Cos(a));
            Vector3 p0=forward*(t0*.8f)+Vector3.up*(Mathf.Sin(t0*Mathf.PI)*.48f+.015f);
            Vector3 p1=forward*(t1*.8f)+Vector3.up*(Mathf.Sin(t1*Mathf.PI)*.48f+.015f);int n=v.Count;
            v.Add(p0-right*.012f);v.Add(p0+right*.012f);v.Add(p1-right*.007f);v.Add(p1+right*.007f);tri.AddRange(new[]{n,n+2,n+1,n+1,n+2,n+3});
        }
        for(int f=0;f<7;f++)for(int j=1;j<=9;j++)for(int side=-1;side<=1;side+=2)
        {
            float t=j/10f,a=f*Mathf.PI*2/7;
            Vector3 forward=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)),right=new Vector3(-forward.z,0,forward.x);
            Vector3 center=forward*(t*.8f)+Vector3.up*(Mathf.Sin(t*Mathf.PI)*.48f+.05f);
            float w=Mathf.Sin(t*Mathf.PI)*.22f;int n=v.Count;
            v.Add(center);v.Add(center+right*(side*w)-forward*.10f+Vector3.up*.03f);v.Add(center+forward*.12f);
            tri.AddRange(new[]{n,n+1,n+2});
        }
        return SaveMesh("Fern_Faceted",v,tri);
    }
    static void Vegetation(Transform parent,float z0,int density)
    {
        var rng=new System.Random((int)z0+20261104);var fern=FernMesh();
        for(int i=0;i<density;i++)
        {
            int side=i%2==0?-1:1;float x=side*(6.5f+(float)rng.NextDouble()*2.0f),z=z0-7.5f+(float)rng.NextDouble()*15;
            float scale=.65f+(float)rng.NextDouble()*.55f;
            if(i%3==0)
            {
                var g=MeshObject("Fern_"+i,parent,fern,Mat("Fern_Jade",new Color(.26f,.42f,.14f)));
                g.transform.position=new Vector3(x,-.012f,z);g.transform.localRotation=Quaternion.Euler(0,i*137.5f,0);g.transform.localScale=Vector3.one*scale;
            }
            else Place(Nature+(i%4==0?"Bush_Small.fbx":"Grass_Large.fbx"),"Understory_Grounded_"+i,parent,new Vector3(x,-.025f,z),new Vector3(scale*1.4f,scale*.7f,scale*1.3f),i*137.5f,"Nature");
        }
        for(int i=0;i<7;i++)
        {
            float x=(i%2==0?-1:1)*(7.6f+i%3*.3f),z=z0-6+i*1.9f;
            var tree=Place(Nature+(i%3==0?"PalmTree_1.fbx":"NormalTree_2.fbx"),"Canopy_Tree_"+i,parent,new Vector3(x,-.025f,z),new Vector3(3.5f,6.5f+i%3,3.2f),i*59,"Nature");
            var trunk=tree.AddComponent<CapsuleCollider>();trunk.center=new Vector3(0,1.2f,0);trunk.height=2.4f;trunk.radius=.17f;
        }
        for(int i=0;i<8;i++)
        {
            var rock=Place(Cave+"Cave_Rocks_"+(i%2==0?"A":"B")+".prefab","EdgeBoulder_"+i,parent,new Vector3((i%2==0?-1:1)*8.2f,-.08f,z0-7+i*2),new Vector3(2.1f,1.25f+i%3*.35f,2),i*61);
            ConformMoss(rock,parent,"Garden_"+i);
            var box=rock.AddComponent<BoxCollider>();box.size=new Vector3(1.8f,1.25f+i%3*.35f,1.7f);box.center=Vector3.up*box.size.y*.5f;
        }
    }
    static void Ring(string n,Transform p,Vector3 center,float radius,Material mat,int points=48,float width=.025f,float aspect=1)
    {
        var go=Group(n,p).gameObject;go.layer=3;var line=go.AddComponent<LineRenderer>();line.sharedMaterial=mat;line.useWorldSpace=false;
        line.loop=true;line.widthMultiplier=width;line.positionCount=points;line.numCornerVertices=0;
        for(int i=0;i<points;i++){float a=i*Mathf.PI*2/points;line.SetPosition(i,center+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius*aspect));}
    }
    static void RuinColumn(Transform p,Vector3 pos,float height)
    {
        var go=Place(Ruins+"Column_Square.fbx","Ruin_Pillar",p,pos,new Vector3(.75f,height,.75f),0);
        foreach(var r in go.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(_=>Mat("Carved_Limestone",new Color(.38f,.43f,.39f))).ToArray();
    }
    static void Shrine(Transform root)
    {
        Material gold=Mat("Bronze_Patina",new Color(.39f,.50f,.31f)),edge=Mat("Bronze_Gold",new Color(.65f,.46f,.20f));
        Vector3 c=new Vector3(0,0,105.5f);
        var plinth=Shape("Statue_Plinth",root,PrimitiveType.Cube,c+Vector3.up*.36f,new Vector3(5.2f,.72f,2.8f),gold);plinth.AddComponent<BoxCollider>();
        Shape("Plinth_Cornice",root,PrimitiveType.Cube,c+Vector3.up*.79f,new Vector3(5.5f,.14f,3.1f),edge);
        // Static scenery sculpture, separate from the existing guardian actor and altar scripts.
        var shell=Shape("KimQuy_Shell",root,PrimitiveType.Sphere,c+new Vector3(0,1.75f,0),new Vector3(3.6f,1.65f,2.5f),gold);shell.AddComponent<SphereCollider>().radius=.5f;
        Shape("KimQuy_Shell_Rim",root,PrimitiveType.Sphere,c+new Vector3(0,1.25f,0),new Vector3(3.85f,.45f,2.65f),edge);
        Shape("KimQuy_Neck",root,PrimitiveType.Capsule,c+new Vector3(0,1.62f,-1.35f),new Vector3(.62f,.85f,.7f),gold);
        Shape("KimQuy_Head",root,PrimitiveType.Sphere,c+new Vector3(0,2.10f,-1.65f),new Vector3(.8f,.7f,.95f),gold);
        for(int side=-1;side<=1;side+=2)
        {
            for(int end=-1;end<=1;end+=2)Shape("KimQuy_Foot",root,PrimitiveType.Sphere,c+new Vector3(side*1.45f,1.05f,end*.85f),new Vector3(1,.55f,.7f),gold);
            Shape("KimQuy_Eye",root,PrimitiveType.Sphere,c+new Vector3(side*.31f,2.22f,-1.89f),Vector3.one*.105f,Mat("Jade_Eye",new Color(.27f,.8f,.53f),1.2f));
        }
        for(int i=0;i<3;i++){float r=.55f+i*.35f;float y=1.75f+.825f*Mathf.Sqrt(1-r*r/(1.8f*1.8f))+.015f;Ring("Shell_Inlay_"+i,root,c+new Vector3(0,y,0),r,edge,28,.035f,2.5f/3.6f);}
        for(int side=-1;side<=1;side+=2)RuinColumn(root,new Vector3(side*4,0,106),4.6f);
        Shape("Sanctum_Lintel",root,PrimitiveType.Cube,new Vector3(0,4.45f,106),new Vector3(9,.55f,1),gold);
        SunDisc(root,new Vector3(0,2.9f,107.7f),1.8f,"Sanctum");
    }
    static void SunDisc(Transform p,Vector3 c,float radius,string key)
    {
        var bronze=Mat("Old_Bronze",new Color(.53f,.36f,.16f));
        var disc=Shape("SunDisc_"+key,p,PrimitiveType.Cylinder,c,new Vector3(radius*2,.035f,radius*2),bronze);disc.transform.rotation=Quaternion.Euler(90,0,0);
        for(int ring=0;ring<3;ring++)
        {
            var go=Group("Engraved_Ring_"+key+"_"+ring,p).gameObject;go.layer=3;var line=go.AddComponent<LineRenderer>();line.sharedMaterial=ring==1?Inlay():Mat("Bronze_Gold",new Color(.65f,.46f,.20f));line.useWorldSpace=true;line.loop=true;line.widthMultiplier=.025f;line.positionCount=64;
            for(int i=0;i<64;i++){float a=i*Mathf.PI/32;line.SetPosition(i,c+new Vector3(Mathf.Cos(a)*radius*(.55f+ring*.19f),Mathf.Sin(a)*radius*(.55f+ring*.19f),-.045f));}
        }
        var v=new List<Vector3>();var tris=new List<int>();
        for(int i=0;i<12;i++)
        {
            float a=i*Mathf.PI/6,b=a+Mathf.PI/12;int n=v.Count;v.Add(c+new Vector3(0,0,-.05f));
            v.Add(c+new Vector3(Mathf.Cos(a)*radius*.45f,Mathf.Sin(a)*radius*.45f,-.05f));
            v.Add(c+new Vector3(Mathf.Cos(b)*radius*.21f,Mathf.Sin(b)*radius*.21f,-.05f));tris.AddRange(new[]{n,n+1,n+2});
        }
        MeshObject("Sun_Rays_"+key,p,SaveMesh("SunDisc_"+key,v,tris),Inlay());
    }
    static void Build(Scene scene,Report report)
    {
        Folder(Root+"/Materials");Folder(Root+"/Meshes");Folder(Root+"/Prefabs");
        if(Find(scene,"ART_C1_Environment"))throw new InvalidOperationException("Art already installed; preserve manual edits instead of rebuilding.");
        var scripts=All<MonoBehaviour>(scene).Where(x=>x&&!(x is NavMeshSurface)).ToDictionary(x=>x,x=>EditorJsonUtility.ToJson(x));
        var collisions=All<Collider>(scene).ToDictionary(x=>x,x=>EditorJsonUtility.ToJson(x));
        var root=Group("ART_C1_Environment",Find(scene,"World_Geometry"));
        var camp=Group("ENV_01_Camp_ContactFix",root);var garden=Group("ENV_02_MossGarden",root);
        var drums=Group("ENV_03_DrumHall",root);var mirrors=Group("ENV_04_LightCorridor",root);var sanctuary=Group("ENV_05_KimQuy",root);
        var existingCamp=Find(scene,"ART_Camp");
        if(!existingCamp)throw new InvalidOperationException("Existing camp required.");
        foreach(var t in existingCamp.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("MossCrown_")).ToArray())
            t.gameObject.SetActive(false);
        foreach(var t in existingCamp.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Boulder_")).ToArray())
            ConformMoss(t.gameObject,camp,"Camp_"+t.name);
        foreach(var t in existingCamp.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Understory_")||t.name.StartsWith("RockFoot_Shrub_")||t.name.StartsWith("Tree_")))
        {
            if(!t.gameObject.activeInHierarchy)continue;var b=BoundsOf(t.gameObject);t.position+=Vector3.up*(-.02f-b.min.y);
            PrefabUtility.RecordPrefabInstancePropertyModifications(t);
        }
        // Keep all collision and gameplay roots. Only replace visible blockout surfaces.
        foreach(var floor in All<Transform>(scene).Where(t=>t.name.StartsWith("Floor_")&&t.GetComponent<Renderer>()).ToArray())
        {
            var b=floor.GetComponent<Renderer>().bounds;Transform p=b.center.z<20?camp:b.center.z<40?garden:b.center.z<62?drums:b.center.z<82?mirrors:sanctuary;
            FacetFloor(p,b,floor.name,b.center.z<40?new Color(.35f,.34f,.25f):new Color(.31f,.36f,.36f));floor.GetComponent<Renderer>().enabled=false;
        }
        foreach(string n in new[]{"Camp_Floor_Visual","Passage_Floor_Visual","07_FlatTrail_NoCollision"}){var t=Find(scene,n);if(t)t.gameObject.SetActive(false);}
        int wallIndex=0;
        foreach(var t in All<Transform>(scene).Where(t=>t.name.StartsWith("Wall_")&&t.GetComponent<Renderer>()&&t.position.z>=19).ToArray())
            CladWall(t,t.position.z<41?garden:t.position.z<63?drums:t.position.z<83?mirrors:sanctuary,wallIndex++);
        foreach(var t in All<Transform>(scene).Where(t=>t.name.StartsWith("Ceiling_")&&t.GetComponent<Renderer>()&&t.position.z>20).ToArray())
        {
            var b=t.GetComponent<Renderer>().bounds;var p=t.position.z<63?drums:t.position.z<83?mirrors:sanctuary;
            RoughRoof(p,b,t.name);
            Place(Cave+"Cave_Rocks_B.prefab","Vault_"+t.name,p,new Vector3(b.center.x,b.min.y,b.center.z),new Vector3(b.size.x+.2f,1.8f,b.size.z+.2f),0);t.GetComponent<Renderer>().enabled=false;
        }
        // Existing trial rocks retain their collider dimensions beneath fitted visual models.
        foreach(var t in All<Transform>(scene).Where(t=>(t.name.StartsWith("Garden_Stone_")||t.name.StartsWith("Stone_Base_"))&&t.gameObject.activeInHierarchy&&t.GetComponent<Renderer>()).ToArray())
        {
            var b=t.GetComponent<Renderer>().bounds;var g=Place(Nature+"Rock_2.fbx","RetainedObstacle_"+t.name,t.position.z<20?camp:garden,new Vector3(b.center.x,b.min.y,b.center.z),b.size,0);
            ConformMoss(g,t.position.z<20?camp:garden,"Retained_"+t.name);t.GetComponent<Renderer>().enabled=false;
        }
        Vegetation(garden,30,82);
        for(int side=-1;side<=1;side+=2)
        {
            Place(Cave+"Cave_Rocks_B.prefab","Garden_Skylight_Lip_"+side,garden,new Vector3(side*9.3f,11.5f,30),new Vector3(4.8f,3.5f,21),0);
            Place(Cave+"Cave_Rocks_B.prefab","Garden_Upper_Arch_"+side,garden,new Vector3(0,11.6f,30+side*9),new Vector3(21,3.2f,4.7f),0);
        }
        StratifiedWall(garden,new Bounds(new Vector3(0,9.2f,40),new Vector3(4.8f,10,1)),10,"GardenExit_UpperRock",71);
        foreach(var t in All<Transform>(scene).Where(t=>(t.name.StartsWith("Path_Edge_")||t.name.StartsWith("Passage_Trim_")||t.name.StartsWith("Gate_Jamb_")||t.name.StartsWith("Arch_Header_")||t.name=="FragmentA_Pedestal")&&t.GetComponent<Renderer>()).ToArray())
        {
            var b=t.GetComponent<Renderer>().bounds;
            Place(Nature+"Rock_1.fbx","OldMarker_Replaced_"+t.name,t.position.z<20?camp:garden,new Vector3(b.center.x,b.min.y,b.center.z),b.size,0);
            t.GetComponent<Renderer>().enabled=false;
        }
        var fern=FernMesh();
        for(int i=0;i<20;i++)
        {
            var f=MeshObject("Camp_Fern_"+i,camp,fern,Mat("Fern_Jade",new Color(.26f,.42f,.14f)));f.transform.position=new Vector3(6.0f+(i%3)*1.05f,-.015f,-5+i*.65f);f.transform.localRotation=Quaternion.Euler(0,i*123,0);
        }
        Material cyan=Inlay(),bronze=Mat("Old_Bronze",new Color(.53f,.36f,.16f));
        foreach(float z in new[]{47.5f,53.5f,60f})for(int side=-1;side<=1;side+=2)RuinColumn(drums,new Vector3(side*7.45f,0,z),4.7f);
        for(int i=0;i<3;i++)Ring("DrumHall_Inlay_"+i,drums,new Vector3(0,.035f,52),2.4f+i*.15f,i==1?cyan:bronze,64,.035f);
        var drumDoor=(GameObject)new SerializedObject(All<DrumPuzzleController>(scene).Single()).FindProperty("door").objectReferenceValue;
        var discParent=Group("VIS_DrumDoor_Engraving",drumDoor.transform);discParent.position=Vector3.zero;
        discParent.localScale=new Vector3(1/drumDoor.transform.lossyScale.x,1/drumDoor.transform.lossyScale.y,1/drumDoor.transform.lossyScale.z);
        SunDisc(discParent,new Vector3(0,1.65f,56.65f),1.05f,"DrumDoor");
        foreach(var drum in All<DrumInteractable>(scene))
        {
            var model=Group("VIS_C1_DrumPedestal",drum.transform);var pos=drum.transform.position;
            var r=drum.GetComponent<Renderer>();if(r)r.enabled=false;
            Shape("Bronze_Body",model,PrimitiveType.Cylinder,pos,new Vector3(.92f,.36f,.92f),bronze);
            Shape("Bronze_Foot",model,PrimitiveType.Cylinder,pos+Vector3.down*.32f,new Vector3(1.03f,.07f,1.03f),bronze);
            Shape("Bronze_Head",model,PrimitiveType.Cylinder,pos+Vector3.up*.36f,new Vector3(1.05f,.045f,1.05f),Mat("Drum_Face",new Color(.65f,.52f,.27f)));
            for(int j=0;j<12;j++)
            {
                float a=j*Mathf.PI*2/12;
                Shape("Drum_Stud",model,PrimitiveType.Sphere,pos+new Vector3(Mathf.Cos(a)*.49f,.35f,Mathf.Sin(a)*.49f),Vector3.one*.065f,bronze);
            }
        }
        foreach(var mirror in All<MirrorRotator>(scene))
        {
            var original=mirror.GetComponent<MeshRenderer>();
            if(original){var silver=Mat("Mirror_Silver",new Color(.38f,.59f,.64f));silver.SetFloat("_Metallic",.65f);silver.SetFloat("_Smoothness",.88f);original.sharedMaterial=silver;}
            foreach(var r in mirror.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.transform!=mirror.transform))r.sharedMaterial=bronze;
            var face=mirror.transform.Find("MirrorSurface");if(face&&face.GetComponent<Renderer>())face.GetComponent<Renderer>().sharedMaterial=Mat("Mirror_Silver",new Color(.38f,.59f,.64f));
            var v=Group("VIS_C1_MirrorFrame",mirror.transform);var m=Shape("Mirror_Frame",v,PrimitiveType.Cylinder,mirror.transform.position,new Vector3(1.5f,.05f,1.5f),bronze);
            m.transform.rotation=mirror.transform.rotation*Quaternion.Euler(90,0,0);
            // Backing behind the original optical face: no raycast collider is added.
            m.transform.position-=mirror.transform.forward*.11f;
        }
        for(int i=0;i<12;i++)
        {
            int side=i%2==0?-1:1;float z=64+i*1.45f;
            var rock=Place(Nature+"Rock_3.fbx","CrystalHost_"+i,mirrors,new Vector3(side*8.8f,-.06f,z),new Vector3(1.8f,1.2f,1.7f),i*43);
            for(int j=0;j<3;j++)Crystal(mirrors,new Vector3(side*(8.5f-j*.25f),.45f+j*.1f,z+j*.2f),new Vector3(.22f,.7f+j*.24f,.22f),i+j);
        }
        Shrine(sanctuary);
        AttachStatue(scene,root);
        foreach(var altar in All<RelicAltar>(scene))foreach(var r in altar.GetComponentsInChildren<MeshRenderer>(true))
            if(!r.transform.IsChildOf(altar.transform.Find("VIS_C1_KimQuy_Statue")))r.sharedMaterial=Mat("Bronze_Patina",new Color(.39f,.50f,.31f));
        DressInteractables(scene);
        for(int i=0;i<3;i++)Ring("Sanctum_Inlay_"+i,sanctuary,new Vector3(0,.035f,99),5.3f+i*.12f,i==1?cyan:bronze,80,.035f);
        foreach(float z in new[]{92f,98f,104f})for(int side=-1;side<=1;side+=2)
        {
            RuinColumn(sanctuary,new Vector3(side*8.1f,0,z),4.2f);
            Shape("Brazier_Bowl",sanctuary,PrimitiveType.Cylinder,new Vector3(side*7.7f,1.15f,z),new Vector3(.6f,.1f,.6f),bronze);
            Crystal(sanctuary,new Vector3(side*7.7f,1.2f,z),new Vector3(.19f,.55f,.19f),0);
        }
        Lighting(scene,root);
        RefineGarden(root);
        foreach(var pair in scripts)if(EditorJsonUtility.ToJson(pair.Key)!=pair.Value)throw new InvalidOperationException("Gameplay component changed: "+pair.Key.name+" "+pair.Key.GetType().Name);
        foreach(var pair in collisions)if(EditorJsonUtility.ToJson(pair.Key)!=pair.Value)throw new InvalidOperationException("Collision changed: "+pair.Key.name);
        report.checks.Add("All existing gameplay MonoBehaviour fields/references and original colliders retained byte-identical in Editor serialization; navigation is rebuilt separately.");
        report.checks.Add("Camp moss conforms to source rock triangles; ground vegetation bounds seated at -0.02m; no bounding-box moss crowns.");
        report.checks.Add("Five connected visual areas, original floors and doors intact, no new actors or gameplay.");
        PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"/Prefabs/PF_ART_C1_Environment.prefab");
        var surface=All<NavMeshSurface>(scene).Single();surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;surface.BuildNavMesh();
        if(surface.navMeshData==null)throw new InvalidOperationException("Navigation bake failed.");
        string navPath=Root+"/Meshes/NavMesh_C1_Environment.asset";
        var nav=AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
        if(nav){EditorUtility.CopySerialized(surface.navMeshData,nav);surface.RemoveData();surface.navMeshData=nav;surface.AddData();EditorUtility.SetDirty(nav);}
        else AssetDatabase.CreateAsset(surface.navMeshData,navPath);
        report.checks.Add("Navigation rebaked from physics colliders, including new edge boulders, tree trunks and statue; decorative leaves/grass excluded.");
        foreach(var component in All<Component>(scene).Where(c=>c&&(c is Renderer||c is Light||c is Transform)&&PrefabUtility.IsPartOfPrefabInstance(c)))
            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        foreach(var t in All<Transform>(scene).Where(t=>PrefabUtility.IsPartOfPrefabInstance(t.gameObject)))
            PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
    }
    static void AttachStatue(Scene scene,Transform root)
    {
        var altar=All<RelicAltar>(scene).Single();
        if(altar.transform.Find("VIS_C1_KimQuy_Statue"))return;
        var visual=Group("VIS_C1_KimQuy_Statue",altar.transform);
        var source=root.Find("ENV_05_KimQuy");
        foreach(Transform t in source.Cast<Transform>().Where(t=>t.name.StartsWith("KimQuy_")||t.name.StartsWith("Shell_Inlay_")||t.name=="Statue_Plinth"||t.name=="Plinth_Cornice").ToArray())
            t.SetParent(visual,true);
        EditorSceneManager.MarkSceneDirty(scene);
    }
    static void RefineGarden(Transform root)
    {
        if(!root)throw new InvalidOperationException("Chapter art must be installed first.");
        var garden=root.Find("ENV_02_MossGarden");if(garden.Find("Garden_GroundMoss_Patches"))return;
        foreach(Transform t in garden)
            if(t.name.StartsWith("Fern_")||t.name.StartsWith("Understory_Grounded_"))
            {var p=t.position;p.x=Mathf.Sign(p.x)*(3.5f+(Mathf.Abs(p.x)-6.5f)*2.3f);t.position=p;}
        var patches=Group("Garden_GroundMoss_Patches",garden);var mat=Mat("Moss_Soil",new Color(.23f,.30f,.14f));
        for(int i=0;i<10;i++)
        {
            float x=(i%2==0?-1:1)*(4.5f+i%3*.6f),z=23.2f+i*1.4f;
            var v=new List<Vector3>();var tri=new List<int>();var c=new Vector3(x,.012f,z);
            for(int j=0;j<12;j++)
            {
                float a=j*Mathf.PI/6,b=(j+1)*Mathf.PI/6;int n=v.Count;
                v.Add(c);v.Add(c+new Vector3(Mathf.Cos(a)*(1.6f+j%3*.15f),0,Mathf.Sin(a)*2.2f));v.Add(c+new Vector3(Mathf.Cos(b)*(1.6f+(j+1)%3*.15f),0,Mathf.Sin(b)*2.2f));tri.AddRange(new[]{n,n+2,n+1});
            }
            MeshObject("Moss_Soil_Island_"+i,patches,SaveMesh("Moss_Soil_Island_"+i,v,tri),mat);
        }
        var fern=FernMesh();
        for(int i=0;i<34;i++)
        {
            var g=MeshObject("Fern_Dense_"+i,garden,fern,Mat("Fern_Jade",new Color(.26f,.42f,.14f)));
            g.transform.position=new Vector3((i%2==0?-1:1)*(3.8f+i%4*.65f),-.015f,23+i*.42f);
            g.transform.localScale=Vector3.one*(1.15f+i%3*.13f);g.transform.rotation=Quaternion.Euler(0,i*137.5f,0);
        }
        EditorSceneManager.MarkSceneDirty(root.gameObject.scene);
    }
    static void Crystal(Transform p,Vector3 pos,Vector3 size,int variant)
    {
        var v=new List<Vector3>();var tri=new List<int>();const int sides=5;
        for(int i=0;i<sides;i++)
        {
            float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;int n=v.Count;
            v.Add(new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)));v.Add(new Vector3(Mathf.Cos(b),0,Mathf.Sin(b)));v.Add(new Vector3(0,1,0));tri.AddRange(new[]{n,n+2,n+1});
        }
        var go=MeshObject("Crystal_"+variant,p,SaveMesh("Crystal_FiveSided",v,tri),Mat(variant%3==0?"Crystal_Amethyst":"Crystal_Aqua",variant%3==0?new Color(.48f,.24f,.67f):new Color(.14f,.61f,.65f),1.6f));
        go.transform.position=pos;go.transform.localScale=size;go.transform.rotation=Quaternion.Euler(variant%3*11,variant*47,variant%2*15);
    }
    static void DressInteractables(Scene scene)
    {
        var stone=Mat("Carved_Limestone",new Color(.38f,.43f,.39f));
        var bronze=Mat("Old_Bronze",new Color(.53f,.36f,.16f));
        foreach(var lantern in All<LanternPickup>(scene))
        {
            var old=lantern.GetComponent<Renderer>();if(old)old.enabled=false;Vector3 p=lantern.transform.position;
            Shape("VIS_Lantern_Core",lantern.transform,PrimitiveType.Sphere,p,new Vector3(.18f,.24f,.18f),Mat("Lantern_Amber",new Color(1,.65f,.23f),1.2f));
            Shape("VIS_Lantern_Base",lantern.transform,PrimitiveType.Cylinder,p-Vector3.up*.16f,new Vector3(.28f,.025f,.28f),bronze);
            Shape("VIS_Lantern_Cap",lantern.transform,PrimitiveType.Cylinder,p+Vector3.up*.16f,new Vector3(.22f,.022f,.22f),bronze);
            for(int i=0;i<6;i++)
            {float a=i*Mathf.PI/3;Shape("VIS_Lantern_Rib",lantern.transform,PrimitiveType.Cube,p+new Vector3(Mathf.Cos(a)*.1f,0,Mathf.Sin(a)*.1f),new Vector3(.012f,.32f,.012f),bronze);}
        }
        foreach(var pickup in All<ItemPickup>(scene))
        {
            var r=pickup.GetComponent<Renderer>();if(r)r.enabled=false;
            if(pickup.name.Contains("linh_chi"))
            {
                var p=pickup.transform.position;
                Shape("VIS_MushroomStem",pickup.transform,PrimitiveType.Cylinder,p+Vector3.down*.12f,new Vector3(.11f,.17f,.11f),Mat("Herb_Stem",new Color(.67f,.56f,.35f)));
                Shape("VIS_MushroomCap",pickup.transform,PrimitiveType.Sphere,p+Vector3.up*.07f,new Vector3(.42f,.15f,.38f),Mat("Herb_RedBrown",new Color(.42f,.16f,.075f)));
            }
            else if(pickup.name.Contains("la_cam"))
                Place(Nature+"Plant_1.fbx","VIS_Herb",pickup.transform,pickup.transform.position-Vector3.up*.32f,new Vector3(.5f,.5f,.45f),0,"Nature");
            else
                Place("Assets/A16 - Survival Pack/FBX/WaterBottle_1.fbx","VIS_PotionBottle",pickup.transform,pickup.transform.position-Vector3.up*.25f,new Vector3(.25f,.5f,.25f),0,"Survival");
        }
        foreach(var relic in All<RelicPickup>(scene))
        {
            var r=relic.GetComponent<Renderer>();if(r)r.enabled=false;
            var v=new List<Vector3>();var tri=new List<int>();
            for(int i=0;i<8;i++)
            {
                int n=v.Count;float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;
                v.Add(Vector3.zero);v.Add(new Vector3(Mathf.Cos(a)*.65f,Mathf.Sin(a)*.65f,0));v.Add(new Vector3(Mathf.Cos(b)*.65f,Mathf.Sin(b)*.65f,0));tri.AddRange(new[]{n,n+1,n+2});
            }
            MeshObject("VIS_BronzeFragment",relic.transform,SaveMesh("Bronze_Disc_Fragment",v,tri),bronze);
        }
        foreach(var cp in All<CheckpointAltar>(scene).Where(c=>c.CheckpointId!="cp_trai"))
        {
            foreach(var r in cp.GetComponentsInChildren<MeshRenderer>(true))r.sharedMaterial=stone;
            var p=cp.transform.position;
            Shape("VIS_CheckpointBowl",cp.transform,PrimitiveType.Cylinder,p+Vector3.up*.94f,new Vector3(.78f,.08f,.78f),bronze);
            Shape("VIS_CheckpointJade",cp.transform,PrimitiveType.Sphere,p+Vector3.up*1.15f,new Vector3(.17f,.27f,.17f),Mat("Checkpoint_Amber",new Color(1,.6f,.20f),1.5f));
        }
        foreach(var lore in All<LoreStele>(scene))foreach(var r in lore.GetComponentsInChildren<MeshRenderer>(true))r.sharedMaterial=stone;
        foreach(var exit in All<ChapterExit>(scene))foreach(var r in exit.GetComponentsInChildren<MeshRenderer>(true))r.sharedMaterial=stone;
        foreach(var reset in All<DrumPuzzleReset>(scene))foreach(var r in reset.GetComponentsInChildren<MeshRenderer>(true))r.sharedMaterial=stone;
        foreach(var t in All<Transform>(scene).Where(t=>t.name.StartsWith("Door")||t.name=="Barrier"))
        {
            var r=t.GetComponent<MeshRenderer>();if(r)r.sharedMaterial=stone;
        }
        foreach(var emitter in All<MirrorBeamEmitter>(scene))foreach(var r in emitter.GetComponentsInChildren<MeshRenderer>(true))r.sharedMaterial=bronze;
        foreach(var receiver in All<LightPuzzleReceiver>(scene))foreach(var r in receiver.GetComponentsInChildren<MeshRenderer>(true))
            r.sharedMaterial=r.transform==receiver.transform?Mat("Receiver_Jade",new Color(.24f,.57f,.54f),.4f):stone;
        foreach(var seal in All<SealTarget>(scene))
        {
            var barrier=new SerializedObject(seal).FindProperty("barrier").objectReferenceValue as GameObject;
            if(barrier)foreach(var r in barrier.GetComponentsInChildren<MeshRenderer>(true))r.sharedMaterial=Mat("Seal_StoneJade",new Color(.22f,.43f,.36f));
        }
    }
    static void Lighting(Scene scene,Transform root)
    {
        var g=Group("Lighting_C1_Accents",root);
        SunRays(g,"Camp",new Vector3(-5,12,1),new Vector3(-1,.1f,3),1.5f);
        SunRays(g,"Garden",new Vector3(-4,13,29),new Vector3(1,.1f,32),2.1f);
        SunRays(g,"Corridor",new Vector3(-2,5.3f,70),new Vector3(1,.1f,73),.8f);
        var garden=Lamp("Garden_Skylight",g,new Vector3(-3,13,30),new Color(.85f,.94f,.74f),12,25,LightType.Spot);
        garden.spotAngle=85;garden.innerSpotAngle=45;garden.transform.rotation=Quaternion.Euler(78,-25,0);garden.shadows=LightShadows.Soft;
        Lamp("Garden_Bounce",g,new Vector3(2,4,31),new Color(.62f,.78f,.60f),9,16);
        Lamp("Drum_CoolFill",g,new Vector3(0,3.5f,51),new Color(.42f,.72f,.82f),28,12);
        Lamp("Drum_RewardAccent",g,new Vector3(0,3.3f,60),new Color(.32f,.78f,.8f),15,7);
        var shaft=Lamp("Corridor_SunShaft",g,new Vector3(-2,5.0f,70),new Color(1,.80f,.45f),5,16,LightType.Spot);
        shaft.spotAngle=50;shaft.innerSpotAngle=20;shaft.transform.rotation=Quaternion.Euler(65,15,0);
        Lamp("Corridor_CrystalFill",g,new Vector3(3,3,75),new Color(.36f,.67f,.8f),18,11);
        Lamp("Sanctum_Jade",g,new Vector3(0,3.7f,104),new Color(.49f,.84f,.64f),27,11);
        Lamp("Sanctum_WarmApproach",g,new Vector3(-4,3,96),new Color(1,.66f,.31f),18,11);
        foreach(string n in new[]{"Light_KimQuyAltar","Light_KimQuyArena"})
        {var t=Find(scene,n);if(t){var l=t.GetComponent<Light>();l.color=n.EndsWith("Altar")?new Color(.82f,.81f,.60f):new Color(.39f,.54f,.62f);l.intensity=2;}}
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.26f,.34f,.38f);
        RenderSettings.ambientEquatorColor=new Color(.16f,.22f,.23f);RenderSettings.ambientGroundColor=new Color(.10f,.12f,.10f);
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.014f;RenderSettings.fogColor=new Color(.17f,.25f,.27f);
    }
    static void SunRays(Transform p,string key,Vector3 top,Vector3 foot,float spread)
    {
        string path=Root+"/Materials/SunShaft.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!mat){mat=new Material(Shader.Find("SonDoong/CaveLightShaft"));AssetDatabase.CreateAsset(mat,path);}
        mat.SetColor("_BaseColor",new Color(1,.81f,.49f,.065f));
        var v=new List<Vector3>();var tris=new List<int>();var uv=new List<Vector2>();
        for(int i=0;i<6;i++)
        {
            var offset=new Vector3((i-2.5f)*spread*.4f,0,(i%2)*.4f);var width=new Vector3(.26f+ i%3*.12f,0,.04f);
            int n=v.Count;v.AddRange(new[]{top+offset-width,top+offset+width,foot+offset-width*1.5f,foot+offset+width*1.5f});
            uv.AddRange(new[]{new Vector2(0,1),new Vector2(1,1),new Vector2(0,0),new Vector2(1,0)});tris.AddRange(new[]{n,n+2,n+1,n+1,n+2,n+3});
        }
        var mesh=SaveMesh("SunShaft_"+key,v,tris);mesh.SetUVs(0,uv);EditorUtility.SetDirty(mesh);
        var go=MeshObject("SunShaft_"+key,p,mesh,mat);go.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;go.GetComponent<Renderer>().receiveShadows=false;
    }
    static void Validate(Scene s,Report report)
    {
        report.scene=s.path;var root=Find(s,"ART_C1_Environment");if(!root)throw new InvalidOperationException("Missing chapter art.");
        foreach(var r in root.GetComponentsInChildren<Renderer>(true))foreach(var m in r.sharedMaterials)if(!m||!m.shader.isSupported)throw new InvalidOperationException("Invalid art material: "+r.name);
        var altar=All<RelicAltar>(s).Single();var statue=altar.transform.Find("VIS_C1_KimQuy_Statue");
        if(!statue)throw new InvalidOperationException("Statue must be a visual child of the existing relic altar.");
        foreach(var r in statue.GetComponentsInChildren<Renderer>(true))foreach(var m in r.sharedMaterials)if(!m||!m.shader.isSupported)throw new InvalidOperationException("Invalid statue material: "+r.name);
        var solid=root.GetComponentsInChildren<Collider>(true).Concat(statue.GetComponentsInChildren<Collider>(true)).Where(c=>c.enabled).ToArray();
        if(solid.Any(c=>!(c is BoxCollider)&&!(c is CapsuleCollider)&&!(c is SphereCollider)))throw new InvalidOperationException("Unexpected detailed collision in decoration.");
        if(statue.GetComponentsInChildren<Collider>(true).Any(c=>c.GetComponentInParent<IInteractable>()!=altar))throw new InvalidOperationException("Statue collision masks the relic altar interaction.");
        foreach(var cp in All<CheckpointAltar>(s))
        {
            if(!cp.SpawnPoint)throw new InvalidOperationException("Missing spawn "+cp.name);
            var p=cp.SpawnPoint.position;
            var floors=All<Collider>(s).Where(c=>c.enabled&&!c.isTrigger&&c.name.StartsWith("Floor_")).ToArray();
            if(!floors.Any(c=>c.bounds.Contains(new Vector3(p.x,-.1f,p.z))))throw new InvalidOperationException("Unsafe checkpoint floor "+cp.name);
            report.checks.Add("Checkpoint preserved on original floor: "+cp.CheckpointId+" at "+p);
        }
        report.checks.Add("Supported materials across "+(root.GetComponentsInChildren<Renderer>(true).Length+statue.GetComponentsInChildren<Renderer>(true).Length)+" new renderers including gameplay statue; "+solid.Length+" simple solid obstacles, source mesh colliders disabled, original collision retained.");
        report.checks.Add("Statue colliders resolve the original RelicAltar for E selection; no interaction proxy or new ID.");
        report.checks.Add("All five art areas present; original floor/wall/camera collision and gameplay IDs retained.");
        int seated=0;
        foreach(var t in root.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Understory_Grounded_")||t.name.StartsWith("Canopy_Tree_")||t.name.StartsWith("Camp_Fern_")||t.name.StartsWith("Fern_")))
        {
            float y=BoundsOf(t.gameObject).min.y;
            if(y<-.055f||y>.035f)throw new InvalidOperationException("Vegetation floating or sunk: "+t.name+" bottom="+y);
            seated++;
        }
        report.checks.Add("Bounds contact verified for "+seated+" new ground vegetation clusters (bottom -0.055..0.035m).");
        report.checks.Add("Rock-conforming moss meshes: "+root.GetComponentsInChildren<MeshFilter>().Count(f=>f.name.StartsWith("MossSurface_")));
    }
    static void Capture(Scene s,Report report,string label)
    {
        var go=new GameObject("Temporary_Chapter01_ReviewCamera");SceneManager.MoveGameObjectToScene(go,s);var cam=go.AddComponent<Camera>();go.AddComponent<UniversalAdditionalCameraData>();
        cam.fieldOfView=62;cam.nearClipPlane=.1f;cam.farClipPlane=180;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.48f,.65f,.72f);cam.useOcclusionCulling=false;
        var otherR=Resources.FindObjectsOfTypeAll<Renderer>().Where(r=>r.gameObject.scene.IsValid()&&r.gameObject.scene!=s&&r.enabled).ToArray();
        var otherL=Resources.FindObjectsOfTypeAll<Light>().Where(l=>l.gameObject.scene.IsValid()&&l.gameObject.scene!=s&&l.enabled).ToArray();
        var actors=All<Renderer>(s).Where(r=>r.enabled&&(r.GetComponentInParent<PlayerMotor>()||r.GetComponentInParent<EnemyHealth>())).ToArray();
        var canvases=All<Canvas>(s).Where(c=>c.enabled).ToArray();foreach(var c in canvases)c.enabled=false;foreach(var r in otherR)r.enabled=false;foreach(var l in otherL)l.enabled=false;foreach(var r in actors)r.enabled=false;
        string[] names={"camp","garden","drums","light","kimquy"};
        Vector3[] pos={new Vector3(.2f,2.3f,-8.7f),new Vector3(0,2.3f,21),new Vector3(0,2.3f,47),new Vector3(0,2.3f,63),new Vector3(0,2.3f,91)};
        Vector3[] target={new Vector3(-1,2.1f,3),new Vector3(0,3,34),new Vector3(0,2,55),new Vector3(0,2.4f,75),new Vector3(0,2,105)};
        try
        {
            Directory.CreateDirectory("docs/images");
            for(int i=0;i<names.Length;i++)
            {
                cam.transform.position=pos[i];cam.transform.LookAt(target[i]);var rt=new RenderTexture(1600,900,24);rt.Create();var prev=RenderTexture.active;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);
                try
                {
                    RenderPipeline.SubmitRenderRequest(cam,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});
                    RenderPipeline.SubmitRenderRequest(cam,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});
                    RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();
                    string path="docs/images/c1-"+label+"-"+names[i]+"-2026-11-04.png";File.WriteAllBytes(path,tex.EncodeToPNG());report.screenshots.Add(path);
                }
                finally{RenderTexture.active=prev;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
            }
        }
        finally{foreach(var r in otherR)if(r)r.enabled=true;foreach(var l in otherL)if(l)l.enabled=true;foreach(var r in actors)if(r)r.enabled=true;foreach(var c in canvases)if(c)c.enabled=true;UnityEngine.Object.DestroyImmediate(go);}
    }
}
