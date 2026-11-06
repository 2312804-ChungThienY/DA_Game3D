using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;

// Scene authoring only. The gameplay objects retain their IDs and references.
[InitializeOnLoad]
public static class Chapter01ExpandedLayout
{
    public const string Main="Assets/_Project/Scenes/02_HangLinhMach.unity";
    public const string Preview="Assets/_Project/Scenes/92_EnvironmentPreview.unity";
    const string RequestPath="Library/SonDoongExpandedC1.request.json";
    const string Art="Assets/_Project/Art/Chapter01Expanded";
    const string Nature="Assets/A11 - Ultimate Stylized Nature Pack/FBX/";
    const string Cave="Assets/A09 - Mines and Cave Set/MInesAndCaveSet/prefabs/Wall/";
    const string Ruins="Assets/A10 - Ultimate Modular Ruins Pack/FBX/";
    const string Survival="Assets/A16 - Survival Pack/FBX/";
    static bool busy;static double next;
    [Serializable] class Command { public string action; }
    [Serializable] public class Report
    {public string action,backup,scene;public List<string> checks=new List<string>(),failures=new List<string>(),screenshots=new List<string>();}
    public static readonly Vector3[] Centers={new Vector3(0,0,0),new Vector3(18,0,70),new Vector3(-14,0,145),new Vector3(25,0,215),new Vector3(0,0,290)};
    static readonly Vector2[] Radii={new Vector2(18,18),new Vector2(24,24),new Vector2(20,19),new Vector2(24,21),new Vector2(23,22)};
    static readonly string[] Names={"01_TraiGiengTroi","02_VuonReu","03_DienTrong","04_HanhLangQuang","05_DanKimQuy"};
    public static readonly Vector3[][] Links={
        new[]{new Vector3(0,0,16),new Vector3(0,0,23),new Vector3(-4,0,30),new Vector3(1,0,39),new Vector3(13,0,43),new Vector3(18,0,48)},
        new[]{new Vector3(18,0,92),new Vector3(18,0,99),new Vector3(25,0,106),new Vector3(18,0,116),new Vector3(-6,0,120),new Vector3(-14,0,128)},
        new[]{new Vector3(-14,0,162),new Vector3(-14,0,170),new Vector3(-21,0,177),new Vector3(-8,0,186),new Vector3(17,0,188),new Vector3(25,0,196)},
        new[]{new Vector3(25,0,234),new Vector3(25,0,241),new Vector3(33,0,249),new Vector3(24,0,258),new Vector3(8,0,263),new Vector3(0,0,270)}
    };
    static Chapter01ExpandedLayout(){EditorApplication.update+=Poll;}
    static void Submit(string action){if(File.Exists(RequestPath))throw new InvalidOperationException("An expansion request is pending.");File.WriteAllText(RequestPath,JsonUtility.ToJson(new Command{action=action}));}
    [MenuItem("Tools/Son Doong/Expanded Chapter 01/Open Main")]
    static void OpenMain()=>Submit("open-main");
    [MenuItem("Tools/Son Doong/Expanded Chapter 01/Validate")]
    static void CheckMain()=>Submit("validate");
    [MenuItem("Tools/Son Doong/Expanded Chapter 01/Run Play Test")]
    static void Test()=>Submit("play-test");
    static void Guard(){if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Unsaved user scene: "+SceneManager.GetSceneAt(i).path);}
    static IEnumerable<T> All<T>(Scene s) where T:Component=>s.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<T>(true));
    static Transform Find(Scene s,string n)=>All<Transform>(s).FirstOrDefault(x=>x.name==n);
    static Transform Group(string n,Transform p=null){var g=new GameObject(n);g.transform.SetParent(p,false);return g.transform;}
    static void WithScene(string path,Action<Scene> action)
    {
        bool same=SceneManager.sceneCount==1&&SceneManager.GetActiveScene().path==path;
        var setup=EditorSceneManager.GetSceneManagerSetup();var s=same?SceneManager.GetActiveScene():EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
        try{action(s);}finally{if(!same)EditorSceneManager.RestoreSceneManagerSetup(setup);}
    }
    static void Poll()
    {
        if(busy||EditorApplication.timeSinceStartup<next||EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(RequestPath))return;
        busy=true;next=EditorApplication.timeSinceStartup+1;var r=new Report();
        try
        {
            r.action=JsonUtility.FromJson<Command>(File.ReadAllText(RequestPath)).action;Guard();
            if(r.action=="inspect")WithScene(Main,s=>Inspect(s,r));
            else if(r.action=="stage")
            {
                WithScene(Main,s=>{if(Find(s,"ENV_C1_ExpandedWorld"))throw new InvalidOperationException("Main is already expanded; do not overwrite the preview from it.");});
                r.backup=Backup();if(SceneManager.GetSceneByPath(Preview).isLoaded)throw new InvalidOperationException("Close preview before rebuilding it.");
                File.Copy(Main,Preview,true);AssetDatabase.ImportAsset(Preview,ImportAssetOptions.ForceSynchronousImport);
                WithScene(Preview,s=>{Build(s,r);Validate(s,r);EditorSceneManager.SaveScene(s);Capture(s,r,"preview");});
            }
            else if(r.action=="apply")
            {
                var staged=JsonUtility.FromJson<Report>(File.ReadAllText("Logs/ExpandedC1-stage.json"));if(staged.failures.Count>0)throw new InvalidOperationException("Preview must pass first.");
                r.backup=Backup();WithScene(Main,s=>{Build(s,r);Validate(s,r);EditorSceneManager.SaveScene(s);Capture(s,r,"main");});
            }
            else if(r.action=="repair-entrances")
            {
                r.backup=Backup();foreach(var path in new[]{Main,Preview})WithScene(path,s=>
                {RepairEntrances(s);Validate(s,r);EditorSceneManager.SaveScene(s);Capture(s,r,path==Main?"main":"preview");});
            }
            else if(r.action=="tune-relief")
            {
                r.backup=Backup();foreach(var path in new[]{Main,Preview})WithScene(path,s=>
                {TuneRelief(s);Validate(s,r);EditorSceneManager.SaveScene(s);Capture(s,r,path==Main?"main":"preview");});
            }
            else if(r.action=="finish-visuals")
            {
                r.backup=Backup();foreach(var path in new[]{Main,Preview})WithScene(path,s=>
                {FinishVisuals(s);Validate(s,r);EditorSceneManager.SaveScene(s);Capture(s,r,path==Main?"main":"preview");});
            }
            else if(r.action=="repair-gating")
            {
                r.backup=Backup();foreach(var path in new[]{Main,Preview})WithScene(path,s=>
                {RepairGating(s);Validate(s,r);EditorSceneManager.SaveScene(s);Capture(s,r,path==Main?"main":"preview");});
            }
            else if(r.action=="inspect-blockers")WithScene(Main,s=>
            {
                Physics.SyncTransforms();for(int i=0;i<4;i++)foreach(var q in SampleLink(i))
                foreach(var c in Physics.OverlapCapsule(q+Vector3.up*.4f,q+Vector3.up*1.4f,.32f,1<<3,QueryTriggerInteraction.Ignore))
                {if(c.name.StartsWith("Walkable_")||c.name.StartsWith("LinkFloor_"))continue;r.checks.Add("Link "+i+" "+q+" | "+c.name+" | parent="+c.transform.parent?.name+" | "+c.bounds);}
                foreach(var q in new[]{new Vector3(-14,0,159.2f),new Vector3(-17,0,156)})
                foreach(var c in Physics.OverlapCapsule(q+Vector3.up*.4f,q+Vector3.up*1.4f,.45f,~0,QueryTriggerInteraction.Ignore))
                r.checks.Add("Interior "+q+" | "+c.name+" | parent="+c.transform.parent?.name+" | "+c.bounds);
                if(Physics.CapsuleCast(new Vector3(-17,.4f,157),new Vector3(-17,1.4f,157),.32f,new Vector3(3,0,5).normalized,out var obstruction,6,~0,QueryTriggerInteraction.Ignore))r.checks.Add("Interior sweep | "+obstruction.collider.name+" | "+obstruction.point+" | "+obstruction.collider.bounds);
            });
            else if(r.action=="polish-paths")
            {
                r.backup=Backup();foreach(var path in new[]{Main,Preview})WithScene(path,s=>
                {PolishPaths(s);Validate(s,r);EditorSceneManager.SaveScene(s);Capture(s,r,path==Main?"main":"preview");});
            }
            else if(r.action=="repair-safety")
            {
                r.backup=Backup();foreach(var path in new[]{Main,Preview})WithScene(path,s=>
                {
                    var kill=All<KillZone>(s).Single();kill.transform.SetParent(Find(s,"Gameplay"),true);kill.gameObject.SetActive(true);
                    kill.transform.position=new Vector3(0,-14,150);kill.GetComponent<BoxCollider>().size=new Vector3(180,2,370);
                    Validate(s,r);EditorSceneManager.SaveScene(s);
                });
            }
            else if(r.action=="validate")WithScene(Main,s=>Validate(s,r));
            else if(r.action=="capture")WithScene(Main,s=>Capture(s,r,"main"));
            else if(r.action=="open-main")EditorSceneManager.OpenScene(Main);
            else if(r.action=="play-test"||r.action=="play-geometry-test")
            {SessionState.SetBool("SonDoong.ExpandedC1Test",true);SessionState.SetBool("SonDoong.ExpandedC1GeometryOnly",r.action=="play-geometry-test");EditorSceneManager.OpenScene("Assets/_Project/Scenes/00_Boot.unity");EditorApplication.EnterPlaymode();r.checks.Add("Requested expansion compatibility test from Boot; original saves backed up and restored.");}
            else if(r.action=="build")
            {
                var b=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(x=>x.enabled).Select(x=>x.path).ToArray(),locationPathName="Builds/Chapter01Expanded/Windows/SonDoong.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
                r.checks.Add("Windows x64 Release: "+b.summary.result+", errors="+b.summary.totalErrors+", warnings="+b.summary.totalWarnings);
                if(b.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Build failed.");
            }
            else throw new InvalidOperationException("Unknown request: "+r.action);
            AssetDatabase.SaveAssets();
        }
        catch(Exception e){r.failures.Add(e.ToString());}
        finally{Directory.CreateDirectory("Logs");File.WriteAllText("Logs/ExpandedC1-"+r.action+".json",JsonUtility.ToJson(r,true));File.Delete(RequestPath);busy=false;}
    }
    static string Backup()
    {
        var p="SourceAssets/SceneBackups/2026-11-05_ExpandedC1/"+DateTime.UtcNow.ToString("HHmmssfff");Directory.CreateDirectory(p);
        foreach(var s in new[]{Main,Preview})foreach(var f in new[]{s,s+".meta"})if(File.Exists(f))File.Copy(f,p+"/"+Path.GetFileName(f));return p;
    }
    static void Folder(string p){if(AssetDatabase.IsValidFolder(p))return;string parent=Path.GetDirectoryName(p).Replace('\\','/');Folder(parent);AssetDatabase.CreateFolder(parent,Path.GetFileName(p));}
    static void Inspect(Scene s,Report r)
    {
        foreach(var t in All<Transform>(s).Where(t=>t.GetComponent<MonoBehaviour>()&&!t.GetComponent<RectTransform>()))
            if(!t.GetComponentInParent<Canvas>())r.checks.Add(t.name+" | parent="+(t.parent?t.parent.name:"ROOT")+" | "+t.position+" | "+string.Join(",",t.GetComponents<MonoBehaviour>().Where(x=>x).Select(x=>x.GetType().Name)));
        foreach(string p in new[]{"Assets/A20 - Trống đồng/source/filetex.fbx",Ruins+"BridgeSection.fbx",Nature+"Plant_1.fbx"})
        {var g=AssetDatabase.LoadAssetAtPath<GameObject>(p);r.checks.Add(p+" | "+(g?CampEnvironmentSetup.BoundsOf(g).ToString():"MISSING"));}
    }
    static Material Mat(string name,Color color,Texture2D texture=null,float emission=0,float tiling=1)
    {
        string path=Art+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
        m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.14f);m.SetFloat("_Cull",0);
        if(texture){m.SetTexture("_BaseMap",texture);m.SetTextureScale("_BaseMap",Vector2.one*tiling);}
        if(emission>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*emission);}
        EditorUtility.SetDirty(m);return m;
    }
    static Texture2D Texture(string source,string name,bool normal=false)
    {
        string dest=Art+"/Textures/"+name+Path.GetExtension(source);
        if(!File.Exists(dest))AssetDatabase.CopyAsset(source,dest);
        var imp=AssetImporter.GetAtPath(dest) as TextureImporter;
        if(imp&&(imp.maxTextureSize!=1024||normal&&imp.textureType!=TextureImporterType.NormalMap))
        {imp.maxTextureSize=1024;if(normal)imp.textureType=TextureImporterType.NormalMap;imp.SaveAndReimport();}
        return AssetDatabase.LoadAssetAtPath<Texture2D>(dest);
    }
    static Material rock,earth,pathStone,moss,bronze,jade;
    static void Palette()
    {
        // A13 is a UV atlas with black padding, unsuitable for repeating cave surfaces.
        var rockTex=Texture("Assets/A14 - Mossy Rock/textures/mossy_rock_diff_4k.jpg","MossyRock_Tileable_1K");
        var soil=Texture("Assets/A15 - Ground 037/Ground037_2K-PNG_Color.png","Soil_1K");
        rock=Mat("Limestone_Moss",new Color(.72f,.79f,.83f),rockTex);
        earth=Mat("Cave_Soil",new Color(.70f,.68f,.53f),soil);
        pathStone=Mat("Worn_Path_Stone",new Color(.91f,.90f,.79f),rockTex);
        moss=Mat("Moss_Soil",new Color(.29f,.38f,.17f),soil);
        bronze=Mat("Aged_Bronze",new Color(.53f,.40f,.22f));bronze.SetFloat("_Metallic",.55f);bronze.SetFloat("_Smoothness",.3f);
        jade=Mat("Jade_Glyph",new Color(.25f,.62f,.56f),null,.65f);
    }
    static Mesh MeshAsset(string name,List<Vector3> v,List<int> tris,List<Vector2> uv=null)
    {
        string path=Art+"/Meshes/"+name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);bool fresh=!mesh;
        if(fresh)mesh=new Mesh{name=name};else mesh.Clear();if(v.Count>65000)mesh.indexFormat=IndexFormat.UInt32;
        mesh.SetVertices(v);mesh.SetTriangles(tris,0);mesh.SetUVs(0,uv??v.Select(x=>new Vector2(x.x,x.z)*.24f).ToList());mesh.RecalculateNormals();mesh.RecalculateBounds();
        if(fresh)AssetDatabase.CreateAsset(mesh,path);else EditorUtility.SetDirty(mesh);return mesh;
    }
    static GameObject MeshGo(string name,Transform parent,Mesh mesh,Material material,bool solid=false)
    {
        var g=Group(name,parent).gameObject;g.layer=3;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=material;
        if(solid)g.AddComponent<MeshCollider>().sharedMesh=mesh;return g;
    }
    static GameObject Box(string n,Transform p,Vector3 position,Vector3 size,Material mat,bool solid=true)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.layer=3;g.transform.SetParent(p,false);g.transform.position=position;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;
        if(!solid)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static GameObject Model(string source,string name,Transform p,Vector3 pos,float height,float yaw,Material material=null,bool solid=false)
    {
        var outer=Group(name,p);outer.position=pos;outer.gameObject.layer=3;
        var fit=Group("Fit",outer);var src=AssetDatabase.LoadAssetAtPath<GameObject>(source);if(!src)throw new InvalidOperationException("Missing model "+source);
        var g=(GameObject)PrefabUtility.InstantiatePrefab(src,fit);g.transform.localRotation=Quaternion.Euler(0,yaw,0)*g.transform.localRotation;
        PrefabUtility.RecordPrefabInstancePropertyModifications(g.transform);
        var b=CampEnvironmentSetup.BoundsOf(g);float scale=height/Mathf.Max(.01f,b.size.y);fit.localScale=Vector3.one*scale;
        b=CampEnvironmentSetup.BoundsOf(g);fit.position-=new Vector3(b.center.x-pos.x,b.min.y-pos.y,b.center.z-pos.z);
        foreach(var c in g.GetComponentsInChildren<Collider>(true)){c.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(c);}
        foreach(var r in g.GetComponentsInChildren<Renderer>(true))
        {
            r.gameObject.layer=3;
            if(material)r.sharedMaterials=r.sharedMaterials.Select(_=>material).ToArray();
            else r.sharedMaterials=r.sharedMaterials.Select(SourceMaterial).ToArray();
            PrefabUtility.RecordPrefabInstancePropertyModifications(r);
        }
        if(solid){b=CampEnvironmentSetup.BoundsOf(outer.gameObject);var box=outer.gameObject.AddComponent<BoxCollider>();box.center=outer.InverseTransformPoint(b.center);box.size=new Vector3(b.size.x*.7f,b.size.y*.9f,b.size.z*.7f);}
        return outer.gameObject;
    }
    static Material SourceMaterial(Material source)
    {
        if(!source)return rock;
        var known=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Camp/M_A11_"+source.name+".mat");if(known)return known;
        known=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Camp/M_A16_"+source.name+".mat");if(known)return known;
        string clean=new string(source.name.Select(c=>char.IsLetterOrDigit(c)?c:'_').ToArray());
        Color c=source.HasProperty("_BaseColor")?source.GetColor("_BaseColor"):source.HasProperty("_Color")?source.color:new Color(.45f,.38f,.25f);
        var m=Mat("Source_"+clean,c);if(source.mainTexture)m.SetTexture("_BaseMap",source.mainTexture);return m;
    }
    static bool GameplayOwner(Transform t)=>t.GetComponents<MonoBehaviour>().Any(x=>x is KillZone||x is PlayerMotor||x is EnemyHealth||x is CheckpointAltar||x is LanternPickup||x is RelicPickup||x is ItemPickup||x is LoreStele||x is SealTarget||x is DrumPuzzleController||x is MirrorRotator||x is LightPuzzleReceiver||x is MirrorBeamEmitter||x is RequirementDoor||x is ArenaGate||x is RelicAltar||x is ChapterExit||x is CampCraftingTable||x is DrumPuzzleReset);
    static Vector3 Delta(Vector3 pos)
    {
        if(pos.z<20)return Vector3.zero;
        if(pos.z<40)return Centers[1]-new Vector3(0,0,30);
        if(pos.z<62)return Centers[2]-new Vector3(0,0,54);
        if(pos.z<83)return Centers[3]-new Vector3(0,0,72);
        return Centers[4]-new Vector3(0,0,99);
    }
    static void Relocate(Scene s,Report r)
    {
        var original=All<Transform>(s).ToDictionary(t=>t,t=>t.position);var world=Find(s,"World_Geometry");var gameplay=Find(s,"Gameplay");
        var owned=original.Keys.Where(GameplayOwner).ToArray();
        // Preserve functional roots that used to live inside the blockout geometry.
        foreach(var t in owned.Where(t=>t.IsChildOf(world)&&!owned.Any(a=>a!=t&&t.IsChildOf(a))).ToArray())
        {t.SetParent(gameplay,true);}
        foreach(var t in original.Keys.OrderBy(Depth))
        {
            if(t.GetComponentInParent<Canvas>()||t.name=="World_Geometry"||t==gameplay)continue;
            var owner=t;while(owner&&!owned.Contains(owner))owner=owner.parent;
            if(owner)
            {
                Vector3 d=Delta(original[owner]);
                if(owner.name=="CP_TruocDan")d=new Vector3(8,0,265)-original[owner];
                if(owner.GetComponent<RelicAltar>())d=new Vector3(0,0,307)-original[owner];
                if(owner.name=="CP_KimQuy")d=new Vector3(7,0,306)-original[owner];
                if(owner.GetComponent<ChapterExit>())d=new Vector3(-7,0,307)-original[owner];
                t.position=original[t]+d;
                var line=t.GetComponent<LineRenderer>();if(line&&line.useWorldSpace){var points=new Vector3[line.positionCount];line.GetPositions(points);for(int i=0;i<points.Length;i++)points[i]+=d;line.SetPositions(points);}
            }
            else if(t.IsChildOf(gameplay)||t.name.StartsWith("Patrol_"))t.position=original[t]+Delta(original[t]);
        }
        // The seal moves to a purpose-built throat on the first winding passage.
        var seal=All<SealTarget>(s).Single();MoveRoot(seal.transform,new Vector3(0,0,20));
        var barrier=(GameObject)new SerializedObject(seal).FindProperty("barrier").objectReferenceValue;
        var sb=barrier.GetComponent<Collider>();if(sb is BoxCollider box){box.size=new Vector3(5.7f/barrier.transform.lossyScale.x,box.size.y,box.size.z);}
        var guardian=All<StoneGuardian>(s).Single();var so=new SerializedObject(guardian);
        so.FindProperty("arenaMin").vector3Value=new Vector3(-20,0,270);so.FindProperty("arenaMax").vector3Value=new Vector3(20,0,309);so.ApplyModifiedPropertiesWithoutUndo();
        var requirement=All<RequirementDoor>(s).Single();MoveRoot(requirement.transform,new Vector3(0,0,270));
        var lockGate=Find(s,"Gate_ArenaLock");if(lockGate)MoveRoot(lockGate,new Vector3(0,lockGate.position.y,270));
        var trigger=All<ArenaGate>(s).Single();MoveRoot(trigger.transform,new Vector3(0,1.5f,276));
        foreach(var point in All<Transform>(s).Where(t=>t.name.StartsWith("Patrol_")&&!owned.Any(o=>t.IsChildOf(o))))point.position=original[point]+Delta(original[point]);
        var kill=Find(s,"KillZone");if(kill){kill.position=new Vector3(0,-14,150);var b=kill.GetComponent<BoxCollider>();if(b)b.size=new Vector3(180,2,370);}
        r.checks.Add("Existing gameplay moved by room translations; puzzle optical distances, references and IDs retained. Boss arena bounds and kill volume updated to match the larger level.");
    }
    static int Depth(Transform t){int i=0;while(t.parent){i++;t=t.parent;}return i;}
    static void MoveRoot(Transform t,Vector3 pos)
    {Vector3 d=pos-t.position;t.position=pos;foreach(var l in t.GetComponentsInChildren<LineRenderer>(true).Where(l=>l.useWorldSpace)){var v=new Vector3[l.positionCount];l.GetPositions(v);for(int i=0;i<v.Length;i++)v[i]+=d;l.SetPositions(v);}}
    static void Build(Scene s,Report r)
    {
        if(Find(s,"ENV_C1_ExpandedWorld"))throw new InvalidOperationException("Expanded layout already exists; preserve user changes.");
        Folder(Art+"/Materials");Folder(Art+"/Meshes");Folder(Art+"/Textures");Palette();
        var scripts=All<MonoBehaviour>(s).Where(x=>x&&!(x is NavMeshSurface)&&!(x is StoneGuardian)).ToDictionary(x=>x,x=>EditorJsonUtility.ToJson(x));
        Relocate(s,r);
        var world=Find(s,"World_Geometry");
        foreach(Transform child in world)child.gameObject.SetActive(false);
        // Disable original local art lights; new lighting follows the expanded spaces.
        foreach(var l in All<Light>(s).Where(l=>!l.GetComponentInParent<PlayerMotor>()))l.enabled=false;
        var root=Group("ENV_C1_ExpandedWorld",world);var rooms=new Transform[5];
        for(int i=0;i<5;i++){rooms[i]=Group("ENV_"+Names[i],root);Room(rooms[i],i);Dress(rooms[i],i);}
        for(int i=0;i<4;i++)Passage(Group("LINK_"+(i+1)+"_"+Names[i]+"_to_"+Names[i+1],root),i);
        var guides=Group("Authoring_RouteGuides",root);guides.tag="EditorOnly";
        for(int i=0;i<4;i++){var guide=Group("TEST_ROUTE_"+i,guides);int j=0;foreach(var q in SampleLink(i)){var t=Group("Point_"+j++,guide);t.position=q;}}guides.gameObject.SetActive(false);
        RestoreCampProps(s,rooms[0]);GameplayArchitecture(s,rooms);FinishInteractableStone(s);LightScene(s,root);
        var nav=All<NavMeshSurface>(s).Single();nav.useGeometry=NavMeshCollectGeometry.PhysicsColliders;nav.BuildNavMesh();
        var navPath=Art+"/Meshes/NavMesh_ExpandedC1.asset";var old=AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
        if(old){EditorUtility.CopySerialized(nav.navMeshData,old);nav.RemoveData();nav.navMeshData=old;nav.AddData();EditorUtility.SetDirty(old);}else AssetDatabase.CreateAsset(nav.navMeshData,navPath);
        RepairEntrances(s);RepairGating(s);PolishPaths(s);FinishVisuals(s);TuneRelief(s);
        foreach(var pair in scripts)if(EditorJsonUtility.ToJson(pair.Key)!=pair.Value)throw new InvalidOperationException("Unexpected gameplay setting/ref change: "+pair.Key.name+" "+pair.Key.GetType().Name);
        foreach(var t in All<Transform>(s))if(PrefabUtility.IsPartOfPrefabInstance(t)){PrefabUtility.RecordPrefabInstancePropertyModifications(t);PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);}
        foreach(var c in All<Component>(s).Where(c=>c&&(c is Renderer||c is Collider||c is Light)&&PrefabUtility.IsPartOfPrefabInstance(c)))PrefabUtility.RecordPrefabInstancePropertyModifications(c);
        EditorSceneManager.MarkSceneDirty(s);r.checks.Add("Five expanded organic chambers and four curved walkable links; old scenery archived inactive, source asset files unchanged.");
        r.checks.Add("Gameplay serialized fields unchanged except explicitly adjusted guardian arena bounds; optical puzzle layout retained.");
    }
    static float Wobble(float a)=>1+.10f*Mathf.Sin(3*a)+.055f*Mathf.Sin(5*a)+.035f*Mathf.Sin(7*a);
    static Vector3 Rim(int room,float a,float radius=1)
    {var p=Centers[room];float r=Wobble(a)*radius;return p+new Vector3(Mathf.Sin(a)*Radii[room].x*r,0,Mathf.Cos(a)*Radii[room].y*r);}
    static void Room(Transform p,int room)
    {
        const int segments=96,bands=10;var v=new List<Vector3>();var tri=new List<int>();
        v.Add(Centers[room]);
        for(int r=1;r<=bands;r++)for(int j=0;j<segments;j++)
        {var q=Rim(room,j*Mathf.PI*2/segments,(float)r/bands);q.y=r>8?(r-8)*.06f*Mathf.Pow(Mathf.Sin(j*1.77f),2):0;v.Add(q);}
        for(int j=0;j<segments;j++)tri.AddRange(new[]{0,1+j,1+(j+1)%segments});
        for(int r=1;r<bands;r++)for(int j=0;j<segments;j++)
        {int a=1+(r-1)*segments+j,b=1+(r-1)*segments+(j+1)%segments,c=a+segments,d=b+segments;tri.AddRange(new[]{a,c,b,b,c,d});}
        MeshGo("Walkable_NaturalFloor_"+room,p,MeshAsset("RoomFloor_"+room,v,tri),room==1?moss:earth,true);
        v=new List<Vector3>();tri=new List<int>();var uv=new List<Vector2>();
        float height=room==0?22:room==1?25:room==2?16:room==3?19:21;
        const int tiers=8;
        for(int y=0;y<=tiers;y++)for(int j=0;j<=segments;j++)
        {
            float a=j*Mathf.PI*2/segments;float radius=1+.025f*y/tiers+(y==0?0:.035f*Mathf.Sin(3*a+y*.7f)+.025f*Mathf.Cos(7*a-y*.6f));
            var q=Rim(room,a,radius);q.y=height*y/tiers+((y==0||y==tiers)?0:Mathf.Sin(5*a+y)*.45f);
            v.Add(q);uv.Add(new Vector2(j*.55f,q.y*.23f));
        }
        for(int y=0;y<tiers;y++)for(int j=0;j<segments;j++)
        {
            float a=(j+.5f)*Mathf.PI*2/segments;var q=Rim(room,a);
            bool north=room<4&&q.z>Centers[room].z+Radii[room].y*.9f&&Mathf.Abs(q.x-Centers[room].x)<3.6f;
            bool south=room>0&&q.z<Centers[room].z-Radii[room].y*.9f&&Mathf.Abs(q.x-Centers[room].x)<(room==4?7.6f:3.6f);
            if(y<2&&(north||south))continue;
            int n=y*(segments+1)+j;tri.AddRange(new[]{n,n+segments+1,n+1,n+1,n+segments+1,n+segments+2});
        }
        MeshGo("Continuous_Limestone_Cliffs_"+room,p,MeshAsset("ChamberWall_"+room,v,tri,uv),rock,true);
        // Annular rock canopy creates a real skylight instead of an open box.
        v=new List<Vector3>();tri=new List<int>();uv=new List<Vector2>();
        float inner=room<2?.48f:room==3?.20f:.10f;
        for(int ring=0;ring<5;ring++)for(int j=0;j<=segments;j++)
        {
            float a=j*Mathf.PI*2/segments,t=ring/4f;var q=Rim(room,a,Mathf.Lerp(1.08f,inner,t));
            q.y=height-2.2f*Mathf.Sin(t*Mathf.PI)+Mathf.Sin(6*a)*.6f;v.Add(q);uv.Add(new Vector2(q.x,q.z)*.2f);
        }
        for(int ring=0;ring<4;ring++)for(int j=0;j<segments;j++)
        {int n=ring*(segments+1)+j;tri.AddRange(new[]{n,n+segments+1,n+1,n+1,n+segments+1,n+segments+2});}
        MeshGo("CaveVault_Skylight_"+room,p,MeshAsset("ChamberRoof_"+room,v,tri,uv),rock,true);
        for(int i=0;i<32;i++)
        {
            float a=i*Mathf.PI*2/32+.15f;var q=Rim(room,a,.99f);
            if(Mathf.Abs(q.x-Centers[room].x)<5&&Mathf.Abs(q.z-Centers[room].z)>Radii[room].y*.82f)continue;
            Model(Cave+"Cave_Rocks_"+(i%2==0?"A":"B")+".prefab","Cliff_Buttress_"+i,p,q-Vector3.up*.18f,5.5f+i%4*2.2f,i*79,rock,true);
            if(i%3==0)
            {
                var crown=Model(Cave+"Cave_Rocks_B.prefab","Vault_Tooth_"+i,p,q+Vector3.up*(height-3),4.5f+i%3,i*71,rock);
                crown.transform.rotation=Quaternion.Euler(170,i*71,0);
            }
        }
        // Ground trail sweeps into each chamber, keeping a readable central corridor.
        var trail=new List<Vector3>();for(float z=-Radii[room].y+1;z<=Radii[room].y-1;z+=2)trail.Add(Centers[room]+new Vector3(Mathf.Sin(z*.13f)*(room==1?2.2f:.45f),.025f,z));
        Ribbon(p,"ChamberTrail_"+room,trail,3.8f,pathStone,false,.025f);
    }
    public static List<Vector3> SampleLink(int index)
    {
        var nodes=Links[index];var result=new List<Vector3>();
        for(int i=0;i<nodes.Length-1;i++)
        {
            Vector3 a=nodes[Mathf.Max(0,i-1)],b=nodes[i],c=nodes[i+1],d=nodes[Mathf.Min(nodes.Length-1,i+2)];
            int count=Mathf.CeilToInt(Vector3.Distance(b,c)/1.25f);
            for(int j=0;j<count;j++)
            {float t=(float)j/count;var q=.5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t);q.y=0;result.Add(q);}
        }
        result.Add(nodes.Last());return result;
    }
    static Vector3 Right(List<Vector3> points,int i)
    {var forward=(points[Mathf.Min(i+1,points.Count-1)]-points[Mathf.Max(0,i-1)]).normalized;return new Vector3(forward.z,0,-forward.x);}
    static void Ribbon(Transform p,string key,List<Vector3> points,float width,Material mat,bool solid,float y=0)
    {
        var v=new List<Vector3>();var tri=new List<int>();var uv=new List<Vector2>();float distance=0;
        for(int i=0;i<points.Count;i++)
        {
            if(i>0)distance+=Vector3.Distance(points[i-1],points[i]);var side=Right(points,i)*width*.5f;
            v.Add(points[i]-side+Vector3.up*y);v.Add(points[i]+side+Vector3.up*y);uv.Add(new Vector2(0,distance*.25f));uv.Add(new Vector2(width*.25f,distance*.25f));
            if(i>0){int n=i*2;tri.AddRange(new[]{n-2,n,n-1,n-1,n,n+1});}
        }
        MeshGo(key,p,MeshAsset(key,v,tri,uv),mat,solid);
    }
    static void Passage(Transform p,int index)
    {
        var points=SampleLink(index);Ribbon(p,"LinkFloor_"+index,points,10.5f,earth,true);Ribbon(p,"LinkPaving_"+index,points,4.5f,pathStone,false,.018f);
        var v=new List<Vector3>();var tri=new List<int>();var uv=new List<Vector2>();
        const int profile=11;
        for(int i=0;i<points.Count;i++)for(int j=0;j<profile;j++)
        {
            float a=j*Mathf.PI/(profile-1);float side=-Mathf.Cos(a)*(4.5f+.3f*Mathf.Sin(i*.65f+j));
            float h=Mathf.Sin(a)*(9.5f+Mathf.Sin(i*.24f)*1.7f);
            var q=points[i]+Right(points,i)*side+Vector3.up*h;v.Add(q);uv.Add(new Vector2(i*.34f,j*.65f));
        }
        for(int i=1;i<points.Count;i++)for(int j=0;j<profile-1;j++)
        {
            // Short breaks overhead frame daylight at the midpoint; sides stay closed.
            if((index==0||index==3)&&i>points.Count*.38f&&i<points.Count*.52f&&j>=4&&j<=6)continue;
            int a=(i-1)*profile+j,b=a+profile;tri.AddRange(new[]{a,a+1,b,a+1,b+1,b});
        }
        MeshGo("Winding_CaveShell_"+index,p,MeshAsset("LinkVault_"+index,v,tri,uv),rock,true);
        for(int i=2;i<points.Count-2;i+=3)
        {
            var side=Right(points,i);float yaw=Mathf.Atan2(side.x,side.z)*Mathf.Rad2Deg;
            for(int s=-1;s<=1;s+=2)
            {
                var q=points[i]+side*s*4.25f;
                Model(Cave+"Cave_Rocks_A.prefab","Passage_Bank_"+i+"_"+s,p,q-Vector3.up*.05f,1.1f+(i%3)*.25f,yaw+i*33,rock,true);
                if(i%2==0)Fern(p,q-side*s*.65f,.9f,"PassageFern_"+index+"_"+i+"_"+s);
            }
            if(i%6==2)
            {var marker=Model(Ruins+"Column_Round_Short.fbx","Route_Marker_"+i,p,points[i]-side*3.0f,1.1f,yaw,rock);Lamp("Marker_Light_"+i,p,marker.transform.position+Vector3.up*1.2f,new Color(1,.70f,.35f),4,5);}
        }
        // Wide, quiet mid-passage light pools make the transitions readable.
        var middle=points[points.Count/2];Lamp("Passage_Bounce_"+index,p,middle+Vector3.up*5,new Color(.49f,.66f,.65f),18,16);
        for(int i=5;i<points.Count-4;i+=10)Slabs(p,points[i],Right(points,i),index*100+i,5);
    }
    static void Slabs(Transform p,Vector3 center,Vector3 right,int seed,int count)
    {
        var rng=new System.Random(seed+390);
        for(int j=0;j<count;j++)
        {
            var c=center+right*((j-(count-1)*.5f)*.7f);c.y=.032f;var v=new List<Vector3>();var tri=new List<int>();
            int sides=7;for(int i=0;i<sides;i++)
            {int n=v.Count;float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;v.Add(c);v.Add(c+new Vector3(Mathf.Cos(a)*(.35f+(float)rng.NextDouble()*.12f),0,Mathf.Sin(a)*.62f));v.Add(c+new Vector3(Mathf.Cos(b)*.42f,0,Mathf.Sin(b)*.62f));tri.AddRange(new[]{n,n+2,n+1});}
            MeshGo("Handlaid_Slab_"+seed+"_"+j,p,MeshAsset("Slab_"+seed+"_"+j,v,tri),pathStone);
        }
    }
    static void Fern(Transform p,Vector3 pos,float scale,string name)
    {
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"/Meshes/Fern_Broad.asset");
        if(!mesh)
        {
            var v=new List<Vector3>();var tri=new List<int>();
            for(int f=0;f<8;f++)for(int j=1;j<=7;j++)for(int side=-1;side<=1;side+=2)
            {
                float a=f*Mathf.PI/4,t=j/8f;var forward=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var right=new Vector3(-forward.z,0,forward.x);
                var c=forward*(t*.85f)+Vector3.up*(Mathf.Sin(t*Mathf.PI*.88f)*.72f+.03f);
                float w=Mathf.Sin(t*Mathf.PI)*.24f;int n=v.Count;
                v.Add(c-forward*.065f);v.Add(c+right*side*w-forward*.06f+Vector3.up*.09f);v.Add(c+right*side*w*.7f+forward*.08f+Vector3.up*.07f);v.Add(c+forward*.14f);
                tri.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
            }
            mesh=MeshAsset("Fern_Broad",v,tri);
        }
        var g=MeshGo(name,p,mesh,Mat("Fern_Leaf",new Color(.36f,.50f,.20f)));g.transform.position=pos;g.transform.localScale=Vector3.one*scale;g.transform.rotation=Quaternion.Euler(0,(pos.x+pos.z)*37,0);
        g.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
    }
    static void Dress(Transform p,int room)
    {
        var rng=new System.Random(room+81127);int plants=room==1?170:room==0?80:room==3?36:30;
        for(int i=0;i<plants;i++)
        {
            float a=(float)rng.NextDouble()*Mathf.PI*2;float rad=.42f+(float)rng.NextDouble()*.46f;var q=Rim(room,a,rad);
            // Keep approach axes, optical area and fighting bowl clear.
            if(Mathf.Abs(q.x-Centers[room].x)<4.8f)continue;
            if(room==0&&q.x<1&&q.z<7)continue;if(room==2&&Mathf.Abs(q.x-Centers[room].x)<10)continue;
            if(room==4&&rad<.75f)continue;
            if(i%3!=0)Fern(p,q,1.1f+(float)rng.NextDouble()*.8f,"Understory_Fern_"+room+"_"+i);
            else Model(Nature+(i%2==0?"Bush_Small.fbx":"Plant_2.fbx"),"Understory_Clump_"+room+"_"+i,p,q-Vector3.up*.015f,.6f+(float)rng.NextDouble()*.5f,i*137.5f);
        }
        if(room<2)for(int i=0;i<(room==1?17:9);i++)
        {
            float a=(i+.35f)*Mathf.PI*2/(room==1?17:9);var q=Rim(room,a,.73f);
            if(Mathf.Abs(q.x-Centers[room].x)<5)continue;
            var tree=Model(Nature+(i%3==0?"PalmTree_2.fbx":"NormalTree_"+(i%2+1)+".fbx"),"Canopy_"+room+"_"+i,p,q-Vector3.up*.02f,7+i%3*1.6f,i*71);
            var c=tree.AddComponent<CapsuleCollider>();c.radius=.2f;c.height=3;c.center=Vector3.up*1.5f;
        }
        if(room>=2)for(int i=0;i<8;i++)
        {float a=(i+.4f)*Mathf.PI*2/8;var q=Rim(room,a,.83f);if(Mathf.Abs(q.x-Centers[room].x)<5)continue;Model(Ruins+"Column_Square.fbx","Ancient_Pillar_"+i,p,q,5.5f,i*45,rock,true);}
        if(room==3||room==4)for(int i=0;i<24;i++)
        {
            var q=Rim(room,i*Mathf.PI/12,.78f);if(Mathf.Abs(q.x-Centers[room].x)<5)continue;
            var g=Model(Nature+"Rock_3.fbx","Crystal_Rock_"+i,p,q,1.1f+i%3*.3f,i*57,rock);
            Crystal(p,q+new Vector3(.2f,.7f,0),.7f+i%3*.3f,i);
        }
        for(int i=0;i<18;i++)
        {
            var q=Centers[room]+new Vector3((i%2==0?-1:1)*(.7f+i%3*.3f),0,-Radii[room].y+2+i*2);
            if(q.z>Centers[room].z+Radii[room].y-1)break;
            if(room==2&&q.z>Centers[room].z+1&&q.z<Centers[room].z+5)continue;
            Slabs(p,q,Vector3.right,room*1000+i,3);
        }
        if(room==1||room==0)
        {
            for(int cluster=0;cluster<(room==1?4:2);cluster++)
            {
                var c=Centers[room]+new Vector3((cluster%2==0?-1:1)*(9+cluster*.55f),0,-3+cluster*5.5f);
                if(room==0)c=Centers[room]+new Vector3(12+cluster*1.5f,0,cluster==0?-1:10);
                Model(Cave+"Cave_Rocks_B.prefab","Inner_Outcrop_"+cluster,p,c-Vector3.up*.09f,3.1f+cluster%2,cluster*68,rock,true);
                for(int j=0;j<12;j++){float a=j*Mathf.PI/6;Fern(p,c+new Vector3(Mathf.Cos(a)*2.4f,0,Mathf.Sin(a)*2),1.35f+j%3*.18f,"Outcrop_Fern_"+cluster+"_"+j);}
            }
        }
    }
    static void Crystal(Transform p,Vector3 pos,float height,int variant)
    {
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/_Project/Art/Chapter01/Meshes/Crystal_FiveSided.asset");
        var g=MeshGo("Crystal_"+variant,p,mesh,Mat(variant%4==0?"Amethyst":"Aqua_Crystal",variant%4==0?new Color(.43f,.27f,.63f):new Color(.20f,.63f,.62f),null,1.1f));
        g.transform.position=pos;g.transform.localScale=new Vector3(height*.24f,height,height*.24f);g.transform.rotation=Quaternion.Euler(variant%3*8,variant*37,0);
    }
    static void RestoreCampProps(Scene s,Transform camp)
    {
        var source=Find(s,"05_CampProps_A16");
        if(source)
        {
            var copy=UnityEngine.Object.Instantiate(source.gameObject,camp);copy.name="Camp_Supplies_ExistingA16";copy.SetActive(true);
            foreach(var t in copy.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="VIS_Checkpoint_Fire"||t.name=="VIS_CraftingTable"))t.gameObject.SetActive(false);
        }
        Model(Survival+"Tent.fbx","Expedition_Tent_Outer",camp,new Vector3(-11,0,7),3.2f,140);
        Model(Ruins+"Crate.fbx","Supply_Crate",camp,new Vector3(-9,0,-2),.75f,12,SourceMaterial(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Camp/M_CampWood.mat")),true);
        Model(Ruins+"Barrel.fbx","Water_Barrel",camp,new Vector3(-10,0,-1),1.1f,20,null,true);
        Model(Ruins+"Cart.fbx","Supply_Cart",camp,new Vector3(-13,0,3),1.8f,10,null,true);
        for(int i=0;i<4;i++)Model(Survival+"WoodLog.fbx","Stored_Timber_"+i,camp,new Vector3(-10+i*.5f,0,9),.5f,i*29,null,true);
    }
    static void RockPartition(Transform p,string key,float xmin,float xmax,float z,float height,float baseY=0)
    {
        var box=Box("Collision_"+key,p,new Vector3((xmin+xmax)*.5f,baseY+height*.5f,z),new Vector3(xmax-xmin,height,1.6f),rock);
        box.GetComponent<Renderer>().enabled=false;
        var v=new List<Vector3>();var tri=new List<int>();var uv=new List<Vector2>();
        int nx=Mathf.CeilToInt((xmax-xmin)/1.25f),ny=Mathf.CeilToInt(height/1.2f);
        for(int side=-1;side<=1;side+=2)
        {
            int offset=v.Count;
            for(int y=0;y<=ny;y++)for(int x=0;x<=nx;x++)
            {
                float px=Mathf.Lerp(xmin,xmax,(float)x/nx),py=baseY+height*y/ny;
                float depth=.82f+((x==0||x==nx||y==0||y==ny)?0:Mathf.Sin(x*1.7f+y*2.8f)*.22f);
                v.Add(new Vector3(px,py,z+side*depth));uv.Add(new Vector2(px*.24f,py*.24f));
            }
            for(int y=0;y<ny;y++)for(int x=0;x<nx;x++)
            {int n=offset+y*(nx+1)+x;tri.AddRange(side<0?new[]{n,n+nx+1,n+1,n+1,n+nx+1,n+nx+2}:new[]{n,n+1,n+nx+1,n+1,n+nx+2,n+nx+1});}
        }
        MeshGo("CarvedRock_"+key,p,MeshAsset("Partition_"+key,v,tri,uv),rock);
    }
    static void Portal(Transform p,Vector3 center,string key,float gap=3)
    {
        RockPartition(p,key+"_Left",center.x-7,center.x-gap*.5f,center.z,8.5f);
        RockPartition(p,key+"_Right",center.x+gap*.5f,center.x+7,center.z,8.5f);
        RockPartition(p,key+"_Crown",center.x-gap*.5f,center.x+gap*.5f,center.z,4.5f,4);
        Model(Ruins+"Arch_Round.fbx","Ancient_Arch_"+key,p,center-Vector3.forward*.92f,5.8f,0,rock);
    }
    static void GameplayArchitecture(Scene s,Transform[] rooms)
    {
        Portal(rooms[0],new Vector3(0,0,20),"CampSeal",5.6f);
        // Full-width partitions preserve puzzle gating despite the larger chambers.
        foreach(var x in new[]{new {room=2,z=148f},new {room=3,z=223f}})
        {
            var c=Centers[x.room];float radius=Radii[x.room].x+1;
            RockPartition(rooms[x.room],"Puzzle_"+x.room+"_Left",c.x-radius,c.x-1.5f,x.z,9);
            RockPartition(rooms[x.room],"Puzzle_"+x.room+"_Right",c.x+1.5f,c.x+radius,x.z,9);
            RockPartition(rooms[x.room],"Puzzle_"+x.room+"_Top",c.x-1.5f,c.x+1.5f,x.z,5.5f,3.5f);
            Model(Ruins+"Arch_Round.fbx","Puzzle_Arch_"+x.room,rooms[x.room],new Vector3(c.x,0,x.z-1),5.5f,0,rock);
        }
        Portal(rooms[4],new Vector3(0,0,270),"KimQuyGate",3.5f);
        // Supporting the pre-arena checkpoint with an open alcove and a light cue.
        Model(Ruins+"Column_Round_Short.fbx","Checkpoint_Waystone",rooms[4],new Vector3(10,0,264),1.8f,12,rock);
        Lamp("Checkpoint_SafePool",rooms[4],new Vector3(8,3,264),new Color(1,.72f,.39f),14,9);
        foreach(var t in All<Transform>(s).Where(t=>t.name.StartsWith("Door")||t.name=="Barrier"))
            foreach(var rr in t.GetComponents<Renderer>())rr.sharedMaterial=rock;
        foreach(var cp in All<CheckpointAltar>(s))
            if(!cp.gameObject.activeInHierarchy)throw new InvalidOperationException("Checkpoint disabled by scenery archive: "+cp.CheckpointId);
        // Retain the prior interactive visuals under their script owners. The old baked world-space disc is decorative only.
        foreach(var line in All<LineRenderer>(s).Where(l=>l.name.Contains("Engraved_Ring")&&l.GetComponentInParent<DrumPuzzleController>()))line.enabled=false;
        foreach(var drum in All<DrumInteractable>(s))
        {
            var pos=drum.transform.position;
            // Twin stone supports frame each small playable drum without altering its collider.
            Model(Ruins+"Column_Round_Short.fbx","Drum_Platform_Accent",rooms[2],pos+new Vector3(.85f,-.02f,.35f),.55f,0,rock);
        }
        var altar=All<RelicAltar>(s).Single();
        var restored=(GameObject)new SerializedObject(altar).FindProperty("restoredVisual").objectReferenceValue;
        var bronzeTexture=Texture("Assets/A20 - Trống đồng/textures/trong_dong_Base_color_1001.png","DongSon_Base_1K");
        var drumMat=Mat("DongSon_Bronze",new Color(.86f,.78f,.57f),bronzeTexture);drumMat.SetFloat("_Metallic",.6f);drumMat.SetFloat("_Smoothness",.35f);
        if(restored)
        {
            var parent=Group("VIS_DongSon_Restored",restored.transform);parent.position=Vector3.zero;parent.localScale=new Vector3(1/restored.transform.lossyScale.x,1/restored.transform.lossyScale.y,1/restored.transform.lossyScale.z);
            Model("Assets/A20 - Trống đồng/source/filetex.fbx","Restored_DongSon_Drum",parent,altar.transform.position+new Vector3(0,1.1f,0),1.35f,0,drumMat);
            foreach(var old in restored.GetComponents<Renderer>())old.enabled=false;
        }
        // An oversized relief is a background focal point, not another interactable reward.
        var hero=Model("Assets/A20 - Trống đồng/source/filetex.fbx","DongSon_StoneRelief",rooms[2],Centers[2]+new Vector3(0,8f,15),3.4f,0,drumMat);
        hero.transform.rotation=Quaternion.Euler(-70,0,0); // face the chamber entry
        for(int i=-1;i<=1;i+=2)
        {
            Model(Ruins+"Column_Square.fbx","Sanctum_Pillar_"+i,rooms[4],new Vector3(i*10,0,308),8.3f,0,rock,true);
            Model(Ruins+"Column_Square.fbx","DrumHall_RewardPillar_"+i,rooms[2],Centers[2]+new Vector3(i*7,0,12),7,0,rock,true);
        }
        // A backlit stepped roof silhouette fills the enlarged sanctum behind the gameplay altar.
        for(int i=0;i<3;i++)Box("Sanctum_Cornice_"+i,rooms[4],new Vector3(0,7.8f+i*.42f,308),new Vector3(18-i*1.6f,.36f,2.5f-i*.35f),rock,false);
        Ring(rooms[2],Centers[2]+Vector3.up*.04f,5.8f,"Drum_Floor_Inlay");Ring(rooms[4],Centers[4]+Vector3.up*.04f,12.5f,"Sanctum_Inlay");
    }
    static void TuneRelief(Scene s)
    {
        Palette();var root=Find(s,"ENV_C1_ExpandedWorld");
        var normal=Texture("Assets/A20 - Trống đồng/textures/trong_dong_Normal_OpenGL_1001.png","DongSon_Normal_1K",true);
        var mat=AssetDatabase.LoadAssetAtPath<Material>(Art+"/Materials/DongSon_Bronze.mat");mat.SetTexture("_BumpMap",normal);mat.EnableKeyword("_NORMALMAP");mat.SetFloat("_BumpScale",.8f);mat.SetFloat("_Metallic",.15f);mat.SetFloat("_Smoothness",.08f);EditorUtility.SetDirty(mat);
        var light=root.Find("Handlaid_Paths/DongSon_Relief_Accent").GetComponent<Light>();light.intensity=4;light.range=10;light.color=new Color(.98f,.87f,.64f);
        var hero=root.Find("ENV_03_DienTrong/DongSon_StoneRelief");
        if(!root.Find("DongSon_WallMount"))
        {
            var p=Group("DongSon_WallMount",root);var b=CampEnvironmentSetup.BoundsOf(hero.gameObject);float wall=147.1f;
            Box("Carved_Backplate",p,new Vector3(b.center.x,b.center.y,wall),new Vector3(b.size.x+.45f,b.size.y+.45f,.26f),rock,false);
            for(int side=-1;side<=1;side+=2)Box("Stone_Corbel_"+side,p,new Vector3(b.center.x+side*b.size.x*.28f,b.min.y-.15f,(wall+b.center.z)*.5f),new Vector3(.38f,.32f,Mathf.Max(.6f,wall-b.min.z)),rock,false);
        }
    }
    static void FinishVisuals(Scene s)
    {
        Palette();var root=Find(s,"ENV_C1_ExpandedWorld");if(root.Find("VIS_KimQuy_CurvedRoof"))throw new InvalidOperationException("Final focal scenery already present.");
        var hero=root.Find("ENV_03_DienTrong/DongSon_StoneRelief");hero.position=Centers[2]+new Vector3(0,6.2f,1.6f);
        root.Find("Handlaid_Paths/DongSon_Relief_Accent").position=Centers[2]+new Vector3(0,7.5f,-2);
        var p=Group("VIS_KimQuy_CurvedRoof",root);var v=new List<Vector3>();var tri=new List<int>();
        const int nx=20,nz=12;
        for(int z=0;z<=nz;z++)for(int x=0;x<=nx;x++)
        {
            float px=Mathf.Lerp(-11.5f,11.5f,(float)x/nx),pz=Mathf.Lerp(-3.3f,3.3f,(float)z/nz);
            v.Add(new Vector3(px,RoofHeight(px,pz),308+pz));
        }
        for(int z=0;z<nz;z++)for(int x=0;x<nx;x++){int n=z*(nx+1)+x;tri.AddRange(new[]{n,n+nx+1,n+1,n+1,n+nx+1,n+nx+2});}
        MeshGo("MossGreen_CurvedTileRoof",p,MeshAsset("Sanctum_CurvedRoof",v,tri),Mat("Sanctum_Roof_Slate",new Color(.19f,.30f,.25f)));
        foreach(float z in new[]{-3.3f,0,3.3f})
        {
            var g=Group("Bronze_Roof_Edge_"+z,p).gameObject;var line=g.AddComponent<LineRenderer>();line.sharedMaterial=bronze;line.useWorldSpace=true;line.positionCount=nx+1;line.widthMultiplier=z==0?.13f:.075f;
            for(int i=0;i<=nx;i++){float x=Mathf.Lerp(-11.5f,11.5f,(float)i/nx);line.SetPosition(i,new Vector3(x,RoofHeight(x,z)+.05f,308+z));}
        }
        Lamp("Sanctum_Architecture_Fill",p,new Vector3(0,7,302),new Color(.91f,.78f,.49f),25,17);
    }
    static float RoofHeight(float x,float z)=>8.7f+1.65f*(1-Mathf.Pow(Mathf.Abs(z)/3.3f,.7f))+.5f*Mathf.Pow(Mathf.Abs(x)/11.5f,6)+.5f*Mathf.Pow(Mathf.Abs(z)/3.3f,8);
    static void RepairGating(Scene s)
    {
        Palette();var root=Find(s,"ENV_C1_ExpandedWorld");
        foreach(var item in new[]{new {room=2,z=148f,key="Puzzle_2",gap=3f},new {room=3,z=223f,key="Puzzle_3",gap=3f},new {room=4,z=270f,key="KimQuyGate",gap=3.5f}})
        {
            var p=root.Find("ENV_"+Names[item.room]);float center=Centers[item.room].x;Physics.SyncTransforms();
            var door=All<Collider>(s).Where(c=>c.enabled&&c.gameObject.activeInHierarchy&&c.name.StartsWith("Door")&&Mathf.Abs(c.bounds.center.z-item.z)<1).OrderBy(c=>Mathf.Abs(c.bounds.center.x-center)).FirstOrDefault();
            if(!door)throw new InvalidOperationException("Missing physical door for "+item.key);
            if(door.bounds.size.x<3.4f)
            {
                var scale=door.transform.localScale;scale.x*=3.4f/door.bounds.size.x;door.transform.localScale=scale;
                if(PrefabUtility.IsPartOfPrefabInstance(door.transform))PrefabUtility.RecordPrefabInstancePropertyModifications(door.transform);
                Physics.SyncTransforms();
            }
            float leftEnd=Mathf.Max(center-item.gap*.5f,door.bounds.min.x+.025f),rightStart=Mathf.Min(center+item.gap*.5f,door.bounds.max.x-.025f);
            if(rightStart-leftEnd<1.4f)throw new InvalidOperationException("Door opening is too narrow for "+item.key);
            foreach(var side in new[]{"Left","Right"})foreach(var prefix in new[]{"Collision_","CarvedRock_"})
            {var t=p.Find(prefix+item.key+"_"+side);if(t)UnityEngine.Object.DestroyImmediate(t.gameObject);}
            RockPartition(p,item.key+"_Left",center-Radii[item.room].x*1.35f,leftEnd,item.z,9);
            RockPartition(p,item.key+"_Right",rightStart,center+Radii[item.room].x*1.35f,item.z,9);
        }
        var nav=All<NavMeshSurface>(s).Single();nav.BuildNavMesh();var data=AssetDatabase.LoadAssetAtPath<NavMeshData>(Art+"/Meshes/NavMesh_ExpandedC1.asset");EditorUtility.CopySerialized(nav.navMeshData,data);nav.RemoveData();nav.navMeshData=data;nav.AddData();EditorUtility.SetDirty(data);
    }
    static void RepairEntrances(Scene s)
    {
        var root=Find(s,"ENV_C1_ExpandedWorld");
        var drum=root.Find("ENV_03_DienTrong/Cliff_Buttress_14");drum.position=new Vector3(-1.5f,drum.position.y,124);
        var sanctum=root.Find("ENV_05_DanKimQuy/Cliff_Buttress_14");sanctum.position=new Vector3(14,sanctum.position.y,268);
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"/Meshes/ChamberWall_4.asset");var tri=new List<int>();
        const int segments=96;
        for(int y=0;y<8;y++)for(int j=0;j<segments;j++)
        {
            float a=(j+.5f)*Mathf.PI*2/segments;var q=Rim(4,a);
            bool entrance=q.z<Centers[4].z-Radii[4].y*.9f&&Mathf.Abs(q.x-Centers[4].x)<7.6f;
            if(y<2&&entrance)continue;int n=y*(segments+1)+j;tri.AddRange(new[]{n,n+segments+1,n+1,n+1,n+segments+1,n+segments+2});
        }
        mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
        var shell=root.Find("ENV_05_DanKimQuy/Continuous_Limestone_Cliffs_4").GetComponent<MeshCollider>();shell.sharedMesh=null;shell.sharedMesh=mesh;
        Physics.SyncTransforms();var nav=All<NavMeshSurface>(s).Single();nav.BuildNavMesh();
        var data=AssetDatabase.LoadAssetAtPath<NavMeshData>(Art+"/Meshes/NavMesh_ExpandedC1.asset");EditorUtility.CopySerialized(nav.navMeshData,data);nav.RemoveData();nav.navMeshData=data;nav.AddData();EditorUtility.SetDirty(data);
    }
    static void PolishPaths(Scene s)
    {
        Palette();
        var root=Find(s,"ENV_C1_ExpandedWorld");if(root.Find("Handlaid_Paths"))throw new InvalidOperationException("Paths already polished; preserve scene edits.");
        var p=Group("Handlaid_Paths",root);
        for(int room=0;room<5;room++)
        {
            var points=new List<Vector3>();for(float z=-Radii[room].y+1;z<=Radii[room].y-1;z+=1.2f)points.Add(Centers[room]+new Vector3(Mathf.Sin(z*.13f)*(room==1?2.2f:.45f),0,z));
            Paving(p,points,"Room_"+room,3.8f);
        }
        for(int link=0;link<4;link++)Paving(p,SampleLink(link),"Link_"+link,4.5f);
        for(int room=2;room<4;room++)for(int i=0;i<4;i++)
        {
            var q=Centers[room]+new Vector3(i%2==0?-12:12,0,-7+i/2*13);
            Model(Cave+"Cave_Rocks_B.prefab","Side_Outcrop_"+room+"_"+i,p,q-Vector3.up*.06f,3.0f+i%2,i*71,rock,true);
            Model(Ruins+"Column_Round_Short.fbx","Broken_Side_Column_"+room+"_"+i,p,q+new Vector3(1.7f,0,.2f),2.2f,i*30,rock);
            Crystal(p,q+new Vector3(-1.5f,.05f,.3f),1.4f,i);
            Lamp("Side_Crystal_Bounce_"+room+"_"+i,p,q+new Vector3(0,2,-1),room==2?new Color(.28f,.63f,.69f):new Color(.48f,.39f,.75f),10,8);
        }
        Lamp("DongSon_Relief_Accent",p,Centers[2]+new Vector3(0,10,11),new Color(1,.77f,.4f),55,12);
        Lamp("DrumHall_Entry_Fill",p,Centers[2]+new Vector3(0,5,-6),new Color(.55f,.72f,.75f),22,18);
        Lamp("MirrorHall_Entry_Fill",p,Centers[3]+new Vector3(0,5,-8),new Color(.52f,.68f,.79f),22,18);
        // A repeating planar texture needs a real front UV on the beveled marker meshes.
        var marker=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"/Meshes/Carved_InteractableStone.asset");
        if(marker){marker.uv=marker.vertices.Select(q=>new Vector2(q.x+.5f,q.y+.5f)).ToArray();EditorUtility.SetDirty(marker);}
        var nav=All<NavMeshSurface>(s).Single();nav.BuildNavMesh();
        var data=AssetDatabase.LoadAssetAtPath<NavMeshData>(Art+"/Meshes/NavMesh_ExpandedC1.asset");
        EditorUtility.CopySerialized(nav.navMeshData,data);nav.RemoveData();nav.navMeshData=data;nav.AddData();EditorUtility.SetDirty(data);
    }
    static void Paving(Transform p,List<Vector3> points,string key,float width)
    {
        var rng=new System.Random(key.Aggregate(17,(hash,c)=>unchecked(hash*31+c)));
        for(int variant=0;variant<4;variant++)
        {
            var v=new List<Vector3>();var tri=new List<int>();
            for(int i=1;i<points.Count-1;i++)for(int row=0;row<5;row++)
            {
                if((i*3+row)%4!=variant)continue;var right=Right(points,i);var forward=Vector3.Cross(right,Vector3.up);
                float cell=width/5;var c=points[i]+right*((row-2)*cell)+forward*((row%2)*.12f);c.y=.052f;
                int n=v.Count;v.Add(c);
                for(int j=0;j<8;j++){float a=j*Mathf.PI/4;v.Add(c+right*(Mathf.Sin(a)*(cell*.46f+(float)rng.NextDouble()*.025f))+forward*(Mathf.Cos(a)*.55f));}
                for(int j=0;j<8;j++)tri.AddRange(new[]{n,n+1+j,n+1+(j+1)%8});
            }
            var color=Color.Lerp(new Color(.38f,.42f,.39f),new Color(.58f,.59f,.50f),variant/3f);
            MeshGo("Flagstones_"+key+"_"+variant,p,MeshAsset("Paved_"+key+"_"+variant,v,tri),Mat("Paving_Limestone_"+variant,color));
        }
    }
    static void FinishInteractableStone(Scene s)
    {
        var v=new List<Vector3>();var tri=new List<int>();
        var rim=new[]{new Vector2(-.35f,-.5f),new Vector2(.35f,-.5f),new Vector2(.5f,-.35f),new Vector2(.5f,.35f),new Vector2(.35f,.5f),new Vector2(-.35f,.5f),new Vector2(-.5f,.35f),new Vector2(-.5f,-.35f)};
        for(int y=0;y<2;y++)for(int i=0;i<8;i++)v.Add(new Vector3(rim[i].x*(y==1?.94f:1),y-.5f,rim[i].y));
        v.Add(new Vector3(0,-.5f,0));v.Add(new Vector3(0,.5f,0));
        for(int i=0;i<8;i++){int j=(i+1)%8;tri.AddRange(new[]{i,j,i+8,j,j+8,i+8,16,j,i,17,i+8,j+8});}
        var mesh=MeshAsset("Carved_InteractableStone",v,tri);
        foreach(var f in All<MeshFilter>(s).Where(f=>f.gameObject.activeInHierarchy&&f.sharedMesh&&f.sharedMesh.name=="Cube"))
        {
            var cp=f.GetComponentInParent<CheckpointAltar>();
            if(!(f.name=="LanternPedestal"||f.GetComponentInParent<LoreStele>()||(cp&&cp.CheckpointId!="cp_trai")))continue;
            f.sharedMesh=mesh;var rr=f.GetComponent<Renderer>();if(rr)rr.sharedMaterial=rock;
        }
    }
    static void Ring(Transform p,Vector3 c,float radius,string name)
    {
        var g=Group(name,p).gameObject;var l=g.AddComponent<LineRenderer>();l.sharedMaterial=jade;l.useWorldSpace=true;l.loop=true;l.widthMultiplier=.045f;l.positionCount=96;
        for(int i=0;i<96;i++){float a=i*Mathf.PI*2/96;l.SetPosition(i,c+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));}
    }
    static Light Lamp(string n,Transform p,Vector3 pos,Color color,float intensity,float range,LightType type=LightType.Point)
    {
        var l=Group(n,p).gameObject.AddComponent<Light>();l.transform.position=pos;l.type=type;l.color=color;l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;return l;
    }
    static void LightScene(Scene s,Transform root)
    {
        var p=Group("Lighting_ExpandedC1",root);var sun=Lamp("Sun_OpenSkylights",p,new Vector3(0,40,0),new Color(1,.88f,.68f),1.25f,500,LightType.Directional);
        sun.transform.rotation=Quaternion.Euler(68,-35,0);sun.shadows=LightShadows.Soft;
        Color[] colors={new Color(1,.81f,.53f),new Color(.68f,.84f,.62f),new Color(.33f,.65f,.78f),new Color(.46f,.61f,.79f),new Color(.51f,.77f,.65f)};
        for(int i=0;i<5;i++)
        {
            var l=Lamp("Chamber_Key_"+i,p,Centers[i]+new Vector3(-3,i<2?17:11,1),colors[i],i<2?65:100,40,LightType.Spot);
            l.transform.rotation=Quaternion.Euler(83,-20,0);l.spotAngle=100;l.innerSpotAngle=60;l.shadows=LightShadows.Soft;
            Lamp("Chamber_Bounce_"+i,p,Centers[i]+new Vector3(4,4,4),colors[i],25,27);
            Lamp("NextPath_Cue_"+i,p,Centers[i]+new Vector3(0,3,Radii[i].y-3),new Color(.78f,.84f,.63f),9,11);
        }
        Lamp("Camp_Fire",p,new Vector3(-4,1.5f,0),new Color(1,.43f,.14f),13,10);
        Lamp("Camp_CraftingLamp",p,new Vector3(-3,2,3),new Color(1,.71f,.31f),5,6);
        Lamp("KimQuy_Altar",p,new Vector3(0,5.5f,304),new Color(.65f,.87f,.66f),50,17);
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.33f,.42f,.46f);RenderSettings.ambientEquatorColor=new Color(.23f,.29f,.28f);RenderSettings.ambientGroundColor=new Color(.13f,.16f,.12f);
        var sh=new SphericalHarmonicsL2();sh.AddAmbientLight(new Color(.30f,.34f,.32f));RenderSettings.ambientProbe=sh;
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.25f,.34f,.35f);RenderSettings.fogDensity=.005f;
    }
    static void Validate(Scene s,Report r)
    {
        r.scene=s.path;Physics.SyncTransforms();var root=Find(s,"ENV_C1_ExpandedWorld");if(!root)throw new InvalidOperationException("Expanded world missing.");
        if(!All<KillZone>(s).Single().gameObject.activeInHierarchy)throw new InvalidOperationException("Safety kill volume must remain active.");
        foreach(var rr in root.GetComponentsInChildren<Renderer>(true))foreach(var m in rr.sharedMaterials)if(!m||!m.shader.isSupported)throw new InvalidOperationException("Unsupported material: "+rr.name);
        var collider=root.GetComponentsInChildren<Collider>().Where(c=>c.enabled&&!c.isTrigger).ToArray();int samples=0;
        for(int route=0;route<4;route++)
        {
            var points=SampleLink(route);
            foreach(var q in points)
            {
                bool floor=collider.Any(c=>c.name.StartsWith("LinkFloor_")&&c.Raycast(new Ray(q+Vector3.up*2,Vector3.down),out var hit,3));
                if(!floor)throw new InvalidOperationException("Gap in connecting path "+route+" at "+q);samples++;
                foreach(var c in Physics.OverlapCapsule(q+Vector3.up*.4f,q+Vector3.up*1.4f,.32f,1<<3,QueryTriggerInteraction.Ignore))
                {
                    if(c.name.StartsWith("Walkable_")||c.name.StartsWith("LinkFloor_"))continue;
                    if(route==3&&q.z>269&&(c.name=="Door"||c.name=="Gate_ArenaLock"))continue;
                    throw new InvalidOperationException("Scenery blocks link "+route+" at "+q+": "+c.name);
                }
            }
        }
        int sections=0;
        foreach(var gate in new[]{new {room=2,z=148f,key="Puzzle_2"},new {room=3,z=223f,key="Puzzle_3"},new {room=4,z=270f,key="KimQuyGate"}})
        for(float x=Centers[gate.room].x-Radii[gate.room].x*1.25f;x<Centers[gate.room].x+Radii[gate.room].x*1.25f;x+=.65f)
        {
            var q=new Vector3(x,0,gate.z);
            bool before=collider.Any(c=>c.name.StartsWith("Walkable_")&&c.Raycast(new Ray(q-Vector3.forward+Vector3.up*2,Vector3.down),out _,3));
            bool after=collider.Any(c=>c.name.StartsWith("Walkable_")&&c.Raycast(new Ray(q+Vector3.forward+Vector3.up*2,Vector3.down),out _,3));
            if(!before||!after)continue;
            bool blocked=Physics.RaycastAll(q-Vector3.forward*2+Vector3.up*1.2f,Vector3.forward,4,~0,QueryTriggerInteraction.Ignore).Any(h=>h.collider.name.StartsWith("Collision_"+gate.key)||h.collider.name.StartsWith("Door")||h.collider.name=="Gate_ArenaLock");
            if(!blocked)throw new InvalidOperationException("Closed gate can be bypassed: "+gate.key+" at "+q);sections++;
        }
        r.checks.Add("Closed puzzle/arena partitions span "+sections+" walkable cross-sections; no side bypass at sampled points.");
        foreach(var cp in All<CheckpointAltar>(s))
        {
            if(!cp.gameObject.activeInHierarchy||!cp.SpawnPoint)throw new InvalidOperationException("Disabled checkpoint "+cp.name);
            bool floor=collider.Any(c=>c.name.StartsWith("Walkable_")||c.name.StartsWith("LinkFloor_"));
            floor=collider.Any(c=>(c.name.StartsWith("Walkable_")||c.name.StartsWith("LinkFloor_"))&&c.Raycast(new Ray(cp.SpawnPoint.position+Vector3.up,Vector3.down),out var h,2));
            if(!floor)throw new InvalidOperationException("Checkpoint has no safe floor "+cp.CheckpointId+" "+cp.SpawnPoint.position);
            r.checks.Add("Checkpoint "+cp.CheckpointId+" retained at "+cp.SpawnPoint.position);
        }
        var mirrors=All<MirrorRotator>(s).ToArray();if(mirrors.Length!=2||Mathf.Abs(Vector3.Distance(mirrors[0].transform.position,mirrors[1].transform.position)-5)> .02f)throw new InvalidOperationException("Optical puzzle geometry changed.");
        r.checks.Add("Four curved links continuous at "+samples+" centerline samples; five chambers and checkpoints active.");
        r.checks.Add("Two mirrors retain 5m spacing, gameplay IDs and original script refs preserved; navigation rebaked.");
        r.checks.Add(root.GetComponentsInChildren<Renderer>().Length+" active world renderers; materials supported. No baked-GI/FPS claim.");
    }
    static void Capture(Scene s,Report r,string version)
    {
        var g=new GameObject("Temporary_ExpandedC1_Camera");var cam=g.AddComponent<Camera>();g.AddComponent<UniversalAdditionalCameraData>();cam.fieldOfView=63;cam.nearClipPlane=.1f;cam.farClipPlane=600;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.58f,.72f,.77f);
        var hidden=All<Renderer>(s).Where(x=>x.enabled&&(x.GetComponentInParent<PlayerMotor>()||x.GetComponentInParent<EnemyHealth>())).ToArray();foreach(var rr in hidden)rr.enabled=false;
        var canvases=All<Canvas>(s).Where(x=>x.enabled).ToArray();foreach(var c in canvases)c.enabled=false;
        var pos=new[]{new Vector3(.2f,2.4f,-9),Centers[1]+new Vector3(-2,2.4f,-18),Centers[2]+new Vector3(0,2.4f,-14),Centers[3]+new Vector3(0,2.4f,-15),Centers[4]+new Vector3(0,2.4f,-14),new Vector3(115,250,-5)};
        var targets=new[]{new Vector3(-2,2.0f,5),Centers[1]+new Vector3(0,3.5f,5),Centers[2]+new Vector3(0,3,8),Centers[3]+new Vector3(1,3,6),new Vector3(0,3,307),new Vector3(3,0,150)};
        var names=new[]{"camp","garden","drums","light","kimquy","overview"};
        bool fog=RenderSettings.fog;
        try
        {
            for(int i=0;i<pos.Length+4;i++)
            {
                RenderSettings.fog=i==5?false:fog;
                string name;
                if(i<pos.Length){cam.transform.position=pos[i];cam.transform.LookAt(targets[i]);name=names[i];}
                else{int link=i-pos.Length;var list=SampleLink(link);int a=list.Count/3;cam.transform.position=list[a]+Vector3.up*2.1f;cam.transform.LookAt(list[Mathf.Min(a+9,list.Count-1)]+Vector3.up*1.6f);name="link"+(link+1);}
                var rt=new RenderTexture(1600,900,24);rt.Create();var old=RenderTexture.active;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);
                try
                {
                    RenderPipeline.SubmitRenderRequest(cam,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});RenderPipeline.SubmitRenderRequest(cam,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});
                    RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();string path="docs/images/c1-expanded-"+version+"-"+name+"-2026-11-05.png";File.WriteAllBytes(path,tex.EncodeToPNG());r.screenshots.Add(path);
                }
                finally{RenderTexture.active=old;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
            }
        }
        finally{RenderSettings.fog=fog;foreach(var rr in hidden)if(rr)rr.enabled=true;foreach(var c in canvases)if(c)c.enabled=true;UnityEngine.Object.DestroyImmediate(g);}
    }
}
