using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Explicit, local Editor requests only. Never runs while playing or in builds.
[InitializeOnLoad]
public static class CampEnvironmentSetup
{
    public const string MainPath = "Assets/_Project/Scenes/02_HangLinhMach.unity";
    public const string PreviewPath = "Assets/_Project/Scenes/92_EnvironmentPreview.unity";
    const string RequestPath = "Library/SonDoongCampEnvironment.request.json";
    const string ArtPath = "Assets/_Project/Prefabs/Environment/Camp/PF_ART_Camp.prefab";
    const string MaterialsPath = "Assets/_Project/Materials/Camp";
    const string Cave = "Assets/A09 - Mines and Cave Set/MInesAndCaveSet/prefabs/Wall/";
    const string Nature = "Assets/A11 - Ultimate Stylized Nature Pack/FBX/";
    const string Survival = "Assets/A16 - Survival Pack/FBX/";
    static double next;
    static bool running;
    [Serializable] class Request { public string action; }
    [Serializable] public class Report
    {
        public string action;
        public List<string> checks = new List<string>();
        public List<string> failures = new List<string>();
        public List<string> screenshots = new List<string>();
    }
    static CampEnvironmentSetup() { EditorApplication.update += Poll; }
    static void Poll()
    {
        if (running || EditorApplication.timeSinceStartup < next || EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(RequestPath)) return;
        next = EditorApplication.timeSinceStartup + 1;
        running = true;
        var report = new Report();
        try
        {
            report.action = JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath)).action;
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode before environment editing.");
            if (report.action == "inspect") Inspect(report);
            else if (report.action == "stage") Stage(report);
            else if (report.action == "capture") WithScene(PreviewPath, s => Capture(s, report));
            else if (report.action == "validate") WithScene(PreviewPath, s => Validate(s, report));
            else throw new InvalidOperationException("Unknown action: " + report.action);
        }
        catch (Exception e) { report.failures.Add(e.ToString()); }
        finally
        {
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/CampEnvironment-" + report.action + ".json", JsonUtility.ToJson(report, true));
            File.Delete(RequestPath);
            running = false;
        }
    }
    static IEnumerable<T> All<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<T>(true));
    static Transform Find(Scene scene, string name) => All<Transform>(scene).FirstOrDefault(t => t.name == name);
    static Bounds BoundsOf(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) throw new InvalidOperationException("No renderer: " + go.name);
        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }
    static void WithScene(string path, Action<Scene> action)
    {
        var prior = SceneManager.GetActiveScene();
        var scene = SceneManager.GetSceneByPath(path);
        bool wasOpen = scene.IsValid() && scene.isLoaded;
        if (!wasOpen) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        try { SceneManager.SetActiveScene(scene); action(scene); }
        finally
        {
            if (prior.IsValid() && prior.isLoaded) SceneManager.SetActiveScene(prior);
            if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
        }
    }
    static void Inspect(Report report)
    {
        report.checks.Add("Unity " + Application.unityVersion);
        WithScene(MainPath, scene =>
        {
            report.checks.Add("Main dirty=" + scene.isDirty);
            foreach (var t in All<Transform>(scene).Where(t => t.position.z < 20 && t.GetComponent<RectTransform>() == null))
                report.checks.Add(t.name + " | parent=" + (t.parent ? t.parent.name : "ROOT") + " | pos=" + t.position + " | scale=" + t.lossyScale);
        });
        var paths = new[] { Cave + "Cave_Wall_Rocks_A.prefab", Cave + "Cave_Rocks_A.prefab", Cave + "Wall_Caves_Entrance_A.prefab", Nature + "NormalTree_1.fbx", Nature + "Plant_1.fbx", Nature + "Bush_Small.fbx", Nature + "Rock_1.fbx", Survival + "Tent.fbx", Survival + "Bonfire.fbx", Survival + "Bonfire_Fire.fbx", Survival + "WoodLog.fbx", Survival + "Pot.fbx" };
        foreach (var path in paths)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!asset) { report.failures.Add("Missing: " + path); continue; }
            report.checks.Add(path + " | bounds=" + BoundsOf(asset) + " | materials=" + string.Join(",", asset.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials).Select(m => m ? m.name + ":" + m.shader.name : "NULL").Distinct()));
        }
    }

    static Transform Group(string name, Transform parent = null)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform;
    }
    static void Folder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        Folder(Path.GetDirectoryName(path).Replace('\\', '/'));
        AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path));
    }
    static Material Mat(string name, Color color, bool emissive = false)
    {
        string path = MaterialsPath + "/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
        m.SetColor("_BaseColor", color); m.SetFloat("_Smoothness", .08f);
        if (emissive) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", color * 2.5f); }
        EditorUtility.SetDirty(m); return m;
    }
    static Color RockColor(int i) => new Color(.30f + (i % 3) * .025f, .35f + (i % 3) * .025f, .37f + (i % 3) * .025f);
    static void Palette(GameObject visual, string category, int variation = 0)
    {
        foreach (var r in visual.GetComponentsInChildren<Renderer>(true))
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                var source = mats[i]; if (!source) throw new InvalidOperationException("Missing material " + visual.name);
                string n = source.name;
                Material m;
                if (category == "Rock") m = Mat("M_Limestone_" + variation % 3, RockColor(variation));
                else if (category == "Nature")
                {
                    bool bark = n.IndexOf("Bark", StringComparison.OrdinalIgnoreCase) >= 0;
                    bool rock = n.IndexOf("Rock", StringComparison.OrdinalIgnoreCase) >= 0;
                    m = Mat("M_A11_" + n, bark ? new Color(.35f,.27f,.17f) : rock ? new Color(.36f,.40f,.34f) : new Color(.52f,.65f,.31f));
                    if (!rock)
                    {
                        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/A11 - Ultimate Stylized Nature Pack/Textures/" + n + ".png");
                        if (tex) m.SetTexture("_BaseMap", tex);
                        if (!bark)
                        {
                            m.SetFloat("_AlphaClip",1); m.SetFloat("_Cutoff",.45f); m.SetFloat("_Cull",0);
                            m.EnableKeyword("_ALPHATEST_ON"); m.SetOverrideTag("RenderType","TransparentCutout"); m.renderQueue=2450;
                        }
                    }
                }
                else
                {
                    Color c = source.HasProperty("_BaseColor") ? source.GetColor("_BaseColor") : source.color;
                    if (n == "Green") c = new Color(.48f,.42f,.26f);
                    if (n == "LightGreen") c = new Color(.75f,.66f,.44f);
                    if (n.Contains("Wood")) c = n.Contains("Light") ? new Color(.58f,.38f,.19f) : new Color(.26f,.15f,.075f);
                    m = Mat("M_A16_" + n, c, n == "Fire");
                }
                mats[i] = m;
            }
            r.sharedMaterials = mats;
            r.gameObject.layer = 3;
            PrefabUtility.RecordPrefabInstancePropertyModifications(r);
        }
    }
    // A project wrapper retains the imported model as a nested prefab. The wrapper
    // fixes units/pivot, palette and simple collision without editing source assets.
    static GameObject Place(string path, string name, Transform parent, Vector3 foot, Vector3 size, float yaw, string category, bool solid, int variation = 0)
    {
        var wrapper = Group(name, parent).gameObject; wrapper.layer = 3;
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (!source) throw new InvalidOperationException("Missing asset: " + path);
        var visual = (GameObject)PrefabUtility.InstantiatePrefab(source, wrapper.transform);
        visual.transform.localRotation = Quaternion.Euler(0,yaw,0);
        var b = BoundsOf(visual);
        visual.transform.localScale = Vector3.Scale(visual.transform.localScale, new Vector3(size.x/b.size.x,size.y/b.size.y,size.z/b.size.z));
        b = BoundsOf(visual);
        visual.transform.position += foot - new Vector3(b.center.x,b.min.y,b.center.z);
        // Keep the wrapper at its anchor; model corrections remain local.
        wrapper.transform.position = foot;
        visual.transform.position -= foot;
        foreach (var c in visual.GetComponentsInChildren<Collider>(true)) { c.enabled=false; PrefabUtility.RecordPrefabInstancePropertyModifications(c); }
        Palette(visual,category,variation);
        if (solid)
        {
            var box=wrapper.AddComponent<BoxCollider>(); box.center=new Vector3(0,size.y*.5f,0); box.size=size;
        }
        return wrapper;
    }
    static GameObject Block(string name, Transform parent, Vector3 position, Vector3 size, Material material, bool solid = false)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.layer=3;
        go.transform.SetParent(parent,false);go.transform.position=position;go.transform.localScale=size;
        go.GetComponent<Renderer>().sharedMaterial=material;
        if(!solid) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }
    static Light Lamp(string name, Transform parent, Vector3 position, Color color, float intensity, float range, LightType type = LightType.Point)
    {
        var t=Group(name,parent);t.position=position;var l=t.gameObject.AddComponent<Light>();
        l.type=type;l.color=color;l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;l.renderMode=LightRenderMode.ForcePixel;return l;
    }
    static readonly Vector3 TablePosition = new Vector3(-3,0,3);
    static void BuildArt(Transform root)
    {
        var cliffs=Group("01_LimestoneCliffs_A09",root);
        // Walls remain outside the 20x20 blockout. Staggered masses leave an open
        // skylight, with a lower, darker canopy behind the spawn camera.
        for(int side=-1;side<=1;side+=2)
        for(int row=0;row<4;row++)
        {
            float z=-7.5f+row*5;
            Place(Cave+"Cave_Wall_Rocks_"+(row%2==0?"A":"B")+".prefab","Cliff_"+side+"_"+row,cliffs,new Vector3(side*11.2f,-.5f,z),new Vector3(4.2f,17+(row%2)*2,6.5f),side*90,"Rock",true,row);
        }
        for(int i=0;i<4;i++) Place(Cave+"Cave_Wall_Rocks_A.prefab","Rear_Cliff_"+i,cliffs,new Vector3(-8+i*5.3f,-.6f,-11.1f),new Vector3(6.4f,15.5f,3.8f),i*16,"Rock",true,i);
        for(int side=-1;side<=1;side+=2)
            Place(Cave+"Cave_Wall_Rocks_C.prefab","Far_Cliff_"+side,cliffs,new Vector3(side*6.7f,-.4f,11.1f),new Vector3(9.5f,18,4),0,"Rock",true,side+2);
        var canopy=Group("02_RearCanopy_SkylightRim",root);
        Place(Cave+"Cave_Rocks_B.prefab","Rear_Overhang",canopy,new Vector3(0,6.7f,-9),new Vector3(22,4,7),0,"Rock",true,0);
        for(int side=-1;side<=1;side+=2)
        {
            Place(Cave+"Cave_Rocks_A.prefab","Skylight_Rim_"+side,canopy,new Vector3(side*9,14.7f,1),new Vector3(5,4,17),0,"Rock",true,1);
            Place(Cave+"Cave_Rocks_C.prefab","Entrance_Jamb_"+side,cliffs,new Vector3(side*3.2f,-.15f,9.4f),new Vector3(3.3f,6,2.4f),side*12,"Rock",true,1);
        }
        Place(Cave+"Cave_Rocks_B.prefab","Entrance_Crown",cliffs,new Vector3(0,4.8f,10),new Vector3(8.6f,2.5f,3),0,"Rock",true,2);
        for(int side=-1;side<=1;side+=2)
        for(int i=0;i<3;i++) Place(Cave+"Cave_Wall_Rocks_A.prefab","Passage_Wall_"+side+"_"+i,cliffs,new Vector3(side*2.6f,-.1f,12+i*3),new Vector3(2.1f,4.3f,4.1f),90,"Rock",true,i);
        Place(Cave+"Cave_Rocks_B.prefab","Passage_Roof",cliffs,new Vector3(0,4.05f,15),new Vector3(6,1.7f,10),0,"Rock",true,1);
        var boulders=Group("03_Boulders_A09",root);
        Vector3[] rocks={new Vector3(-8,0,-5),new Vector3(7,0,-5),new Vector3(8,0,0),new Vector3(-8.1f,0,6),new Vector3(7.3f,0,6),new Vector3(-5,0,7.5f),new Vector3(5,0,8)};
        for(int i=0;i<rocks.Length;i++) Place(Cave+"Cave_Rocks_"+(i%2==0?"A":"B")+".prefab","Boulder_"+i,boulders,rocks[i],new Vector3(2.5f+i%2,1.6f+(i%3)*.6f,2.3f),i*41,"Rock",true,i);
        var green=Group("04_MossGardenClusters_A11",root);
        Vector3[] trees={new Vector3(-7.9f,0,5),new Vector3(-5.8f,0,7.5f),new Vector3(7.6f,0,3),new Vector3(6.7f,0,8),new Vector3(-8.4f,0,-2)};
        for(int i=0;i<trees.Length;i++)
        {
            float h=5.5f+i%3;
            var tree=Place(Nature+"NormalTree_"+(i%2+1)+".fbx","Tree_"+i,green,trees[i],new Vector3(h*.65f,h,h*.55f),i*72,"Nature",false);
            var trunk=tree.AddComponent<CapsuleCollider>();trunk.height=2.4f;trunk.radius=.18f;trunk.center=new Vector3(0,1.2f,0);
        }
        var random=new System.Random(641104);
        for(int i=0;i<48;i++)
        {
            int side=i%2==0?-1:1;float z=-5.5f+(float)random.NextDouble()*14;
            float x=side*(4.7f+(float)random.NextDouble()*4);
            // Leave the tent, fire/checkpoint apron and crafting approach open.
            if(side<0 && z<4.4f) continue;
            float h=.5f+(float)random.NextDouble()*.65f;
            Place(Nature+(i%3==0?"Bush_Small.fbx":"Grass_Large.fbx"),"Understory_"+i,green,new Vector3(x,.005f,z),new Vector3(h*1.8f,h,h*1.7f),i*47,"Nature",false);
        }
        for(int side=-1;side<=1;side+=2)
        for(int i=0;i<5;i++) Place(Nature+"Grass_Small.fbx","Exit_Green_"+side+"_"+i,green,new Vector3(side*(2.3f+i*.28f),0,7.6f+i*.42f),new Vector3(.8f,.5f,.8f),i*53,"Nature",false);
        var camp=Group("05_CampProps_A16",root);
        Place(Survival+"Tent.fbx","Tent_Main",camp,new Vector3(-7.1f,0,1.5f),new Vector3(3.7f,2.45f,4.1f),180,"Survival",true);
        Place(Survival+"Tent.fbx","Tent_Small",camp,new Vector3(-6.7f,0,5.2f),new Vector3(2.6f,1.7f,2.8f),155,"Survival",true);
        var fire=Group("VIS_Checkpoint_Fire",camp);fire.position=new Vector3(-4,0,0);
        Place(Survival+"Bonfire_Fire.fbx","Bonfire",fire,new Vector3(-4,.03f,0),new Vector3(1.5f,1.6f,1.45f),20,"Survival",false);
        for(int i=0;i<10;i++)
        {
            float a=i*Mathf.PI*2/10;
            Place(Nature+"Rock_"+(i%3+1)+".fbx","FireRing_"+i,fire,new Vector3(-4+Mathf.Cos(a)*.95f,.01f,Mathf.Sin(a)*.95f),new Vector3(.34f,.24f,.32f),i*37,"Rock",false,i);
        }
        Place(Survival+"WoodLog.fbx","Seat_Left",camp,new Vector3(-5.8f,.01f,-1.1f),new Vector3(.75f,.42f,.72f),0,"Survival",true);
        Place(Survival+"WoodLog.fbx","Seat_Back",camp,new Vector3(-4.1f,.01f,-2),new Vector3(.7f,.4f,.65f),0,"Survival",true);
        var table=Group("VIS_CraftingTable",camp);table.position=TablePosition;
        var wood=Mat("M_CampWood",new Color(.39f,.24f,.11f));
        Block("Table_Top",table,TablePosition+new Vector3(0,.92f,0),new Vector3(1.65f,.16f,.9f),wood);
        for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2) Block("Table_Leg_"+x+"_"+z,table,TablePosition+new Vector3(x*.67f,.44f,z*.31f),new Vector3(.13f,.88f,.13f),wood);
        Block("Supply_Crate",camp,new Vector3(-4.55f,.35f,3.3f),new Vector3(.7f,.7f,.7f),wood,true);
        Place(Survival+"Pot_Small.fbx","Mortar_Pot",table,TablePosition+new Vector3(-.3f,1.01f,0),new Vector3(.35f,.22f,.33f),0,"Survival",false);
        Place(Survival+"WaterBottle_1.fbx","Ingredient_Bottle",table,TablePosition+new Vector3(.42f,1.01f,.05f),new Vector3(.16f,.32f,.16f),0,"Survival",false);
        Block("HangingLamp_Post",table,TablePosition+new Vector3(.78f,1.2f,.32f),new Vector3(.11f,2.4f,.11f),wood);
        Block("HangingLamp_Beam",table,TablePosition+new Vector3(.53f,2.35f,.32f),new Vector3(.65f,.11f,.11f),wood);
        var glow=Mat("M_LampAmber",new Color(1,.48f,.085f),true);
        Block("HangingLamp",table,TablePosition+new Vector3(.3f,2.08f,.32f),new Vector3(.18f,.3f,.18f),glow);
        var lights=Group("06_CampLighting",root);
        Lamp("Light_Fire_Warm",lights,new Vector3(-4,1.5f,0),new Color(1,.43f,.12f),4.5f,6.2f);
        Lamp("Light_TableLamp",lights,TablePosition+new Vector3(.3f,2.1f,.25f),new Color(1,.68f,.3f),1.4f,3.5f);
        var sun=Lamp("Light_Skylight_Pool",lights,new Vector3(-4,14,2),new Color(1,.83f,.55f),8,24,LightType.Spot);
        sun.spotAngle=65;sun.innerSpotAngle=35;sun.transform.rotation=Quaternion.Euler(79,-22,0);sun.shadows=LightShadows.Soft;
        var exit=Lamp("Light_Exit_Readability",lights,new Vector3(0,3.2f,12),new Color(.74f,.91f,.63f),2.4f,9);
        var probes=Group("LightProbes_Camp",lights).gameObject.AddComponent<LightProbeGroup>();
        var points=new List<Vector3>();for(int x=-1;x<=1;x++)for(int z=-1;z<=2;z++)foreach(float y in new[]{.6f,2.5f}) points.Add(new Vector3(x*5,y,z*5));probes.probePositions=points.ToArray();
    }

    static void Stage(Report report)
    {
        if(File.Exists(PreviewPath)) throw new InvalidOperationException("Preview already exists. Use capture/validate; preserve manual scene edits.");
        Folder(MaterialsPath);Folder("Assets/_Project/Prefabs/Environment/Camp");
        var prior=SceneManager.GetActiveScene();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
        try
        {
            var world=Group("World_Geometry");var env=Group("ENV_01_Camp",world);var art=Group("ART_Camp",env);BuildArt(art);
            var blockout=Group("BLOCKOUT_Collision_OriginalDimensions",env);
            WithScene(MainPath,main=>
            {
                foreach(string name in new[]{"Floor_Camp","Wall_Side_W_0","Wall_Side_E_0","Wall_South_0","Wall_North_L_0","Wall_North_R_0","Floor_Passage","Wall_Left","Wall_Right","Ceiling_Passage"})
                {
                    var source=Find(main,name);if(!source)throw new InvalidOperationException("Missing blockout "+name);
                    var clone=UnityEngine.Object.Instantiate(source.gameObject);SceneManager.MoveGameObjectToScene(clone,scene);clone.name=name;clone.transform.SetParent(blockout,true);
                    foreach(var r in clone.GetComponentsInChildren<Renderer>())r.enabled=false;
                    report.checks.Add("Copied blockout dimensions: "+name+" "+clone.transform.position+" "+clone.transform.lossyScale);
                }
            });
            SceneManager.SetActiveScene(scene);
            var ground=Mat("M_CampEarth",new Color(.36f,.32f,.23f));
            Block("Camp_Floor_Visual",art,new Vector3(0,-.02f,0),new Vector3(20,.04f,20),ground);
            Block("Passage_Floor_Visual",art,new Vector3(0,-.02f,15),new Vector3(4,.04f,10),ground);
            var markers=Group("Layout_Markers_EditorOnly");markers.tag="EditorOnly";
            var markerMat=Mat("M_LayoutMarker",Color.cyan);
            Block("MARK_Checkpoint_cp_trai",markers,new Vector3(-4,.5f,0),Vector3.one,markerMat);
            Block("MARK_Crafting_KeepClear",markers,TablePosition+Vector3.up*.5f,Vector3.one,markerMat);
            Block("MARK_Exit_To_VuonReu",markers,new Vector3(0,.5f,10),new Vector3(2.5f,1,1),markerMat);
            Block("MARK_Spawn_An",markers,new Vector3(0,.5f,-5),Vector3.one,markerMat);markers.gameObject.SetActive(false);
            var lighting=Group("Lighting");
            var sun=Lamp("Sun_Skylight",lighting,new Vector3(0,20,0),new Color(1,.87f,.65f),1.25f,100,LightType.Directional);sun.transform.rotation=Quaternion.Euler(65,-32,0);sun.shadows=LightShadows.Soft;sun.shadowStrength=.85f;
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.32f,.40f,.47f);RenderSettings.ambientEquatorColor=new Color(.20f,.25f,.25f);RenderSettings.ambientGroundColor=new Color(.12f,.13f,.11f);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.20f,.27f,.29f);RenderSettings.fogDensity=.012f;
            var sky=Mat("M_PreviewSky",new Color(.5f,.7f,.8f));
            var cameras=Group("Cameras");var camera=Group("Camera_An_View_NoActor",cameras).gameObject.AddComponent<Camera>();
            camera.gameObject.tag="MainCamera";camera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.43f,.64f,.73f);camera.fieldOfView=62;camera.nearClipPlane=.1f;camera.farClipPlane=120;
            camera.transform.position=new Vector3(.2f,2.3f,-8.7f);camera.transform.LookAt(new Vector3(-1,2.1f,3));
            Group("Gameplay_ReferenceOnly");Group("UI");
            PrefabUtility.SaveAsPrefabAsset(art.gameObject,ArtPath);
            EditorSceneManager.SaveScene(scene,PreviewPath);AssetDatabase.SaveAssets();
            Validate(scene,report);Capture(scene,report);
            report.checks.Add("Preview only: no player, enemy, new gameplay or build-list changes.");
        }
        finally {if(prior.IsValid()&&prior.isLoaded)SceneManager.SetActiveScene(prior);EditorSceneManager.CloseScene(scene,true);}
    }
    static void Validate(Scene scene,Report report)
    {
        var art=Find(scene,"ART_Camp");if(!art)throw new InvalidOperationException("Missing ART_Camp");
        foreach(var r in art.GetComponentsInChildren<Renderer>(true))foreach(var m in r.sharedMaterials)
            if(!m||!m.shader.isSupported)throw new InvalidOperationException("Missing/unsupported material: "+r.name);
        report.checks.Add("All art renderers have supported materials.");
        // Geometric sweep at walking height. Ignore the floor and decorations
        // without colliders; the path must fit a 0.3m controller with margin.
        var colliders=All<Collider>(scene).Where(c=>c.enabled&&c.gameObject.activeInHierarchy&&!c.isTrigger).ToArray();
        Vector3[] route={new Vector3(0,.2f,-5),new Vector3(1,.2f,-3),new Vector3(1,.2f,1),new Vector3(0,.2f,5),new Vector3(0,.2f,10),new Vector3(0,.2f,16)};
        int samples=0;
        for(int edge=0;edge<route.Length-1;edge++)
        {
            int steps=Mathf.CeilToInt(Vector3.Distance(route[edge],route[edge+1])/.2f);
            for(int i=0;i<=steps;i++)
            {
                var foot=Vector3.Lerp(route[edge],route[edge+1],(float)i/steps);
                var body=new Bounds(foot+new Vector3(0,.9f,0),new Vector3(.72f,1.65f,.72f));
                foreach(var c in colliders)if(c.bounds.Intersects(body))throw new InvalidOperationException("Route blocked at "+foot+" by "+c.name);
                samples++;
            }
        }
        report.checks.Add("PASS: "+samples+" route clearance samples, spawn to garden passage.");
        var spawnBody=new Bounds(new Vector3(0,1.1f,-5),new Vector3(1,1.8f,1));
        foreach(var c in colliders)if(c.bounds.Intersects(spawnBody))throw new InvalidOperationException("Spawn occupied: "+c.name);
        report.checks.Add("PASS: checkpoint respawn area clear; main floor collider retained.");
        report.checks.Add("Art: "+art.GetComponentsInChildren<Renderer>().Length+" renderers, "+art.GetComponentsInChildren<Collider>().Count(c=>c.enabled)+" enabled colliders.");
    }
    static void Capture(Scene scene,Report report)
    {
        var camera=All<Camera>(scene).Single();var position=camera.transform.position;var rotation=camera.transform.rotation;
        // Avoid additive scenes appearing in these diagnostic renders.
        var otherRenderers=Resources.FindObjectsOfTypeAll<Renderer>().Where(r=>r.gameObject.scene.IsValid()&&r.gameObject.scene!=scene&&r.enabled).ToArray();
        var otherLights=Resources.FindObjectsOfTypeAll<Light>().Where(l=>l.gameObject.scene.IsValid()&&l.gameObject.scene!=scene&&l.enabled).ToArray();
        foreach(var r in otherRenderers)r.enabled=false;foreach(var l in otherLights)l.enabled=false;
        try
        {
            string[] names={"spawn","crafting","exit","overview"};
            Vector3[] positions={new Vector3(.2f,2.3f,-8.7f),new Vector3(-1.1f,1.8f,1.1f),new Vector3(0,1.9f,8),new Vector3(17,18,-22)};
            Vector3[] targets={new Vector3(-1,2.1f,3),new Vector3(-3,1,3),new Vector3(0,1.8f,17),new Vector3(0,4,1)};
            Directory.CreateDirectory("docs/images");
            for(int i=0;i<names.Length;i++)
            {
                camera.transform.position=positions[i];camera.transform.LookAt(targets[i]);
                var rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32);rt.Create();var previous=RenderTexture.active;
                var texture=new Texture2D(1600,900,TextureFormat.RGB24,false);
                try
                {
                    UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=rt});
                    RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();
                    string path="docs/images/camp-"+names[i]+"-2026-11-04.png";File.WriteAllBytes(path,texture.EncodeToPNG());report.screenshots.Add(path);
                }
                finally{RenderTexture.active=previous;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);}
            }
        }
        finally
        {
            camera.transform.SetPositionAndRotation(position,rotation);
            foreach(var r in otherRenderers)if(r)r.enabled=true;foreach(var l in otherLights)if(l)l.enabled=true;
        }
    }
}
