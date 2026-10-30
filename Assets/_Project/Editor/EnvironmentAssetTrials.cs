using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

[InitializeOnLoad]
public static class EnvironmentAssetTrials
{
    private const string RequestPath = "Library/SonDoongEnvironmentTrial.request.json";
    private static double nextCheck;
    private static bool running;
    private const string Cave = "Assets/A09 - Mines and Cave Set/MInesAndCaveSet/prefabs/";
    private const string Nature = "Assets/A11 - Ultimate Stylized Nature Pack/FBX/";
    private static readonly string[] Sources = {
        Cave + "Ground/Ground_Mines_A.prefab", Cave + "Wall/Cave_Rocks_A.prefab",
        Cave + "Wall/Cave_Wall_Rocks_A.prefab", Nature + "NormalTree_1.fbx",
        Nature + "Bush_Small.fbx", Nature + "Grass_Small.fbx"
    };

    [Serializable] private class Request { public string action = ""; }
    [Serializable] public class Sample
    {
        public string source;
        public Vector3 size;
        public int colliders;
        public List<string> materials = new List<string>();
        public string prefab;
        public string collisionPolicy;
        public bool walkAroundPassed;
        public bool blockingPassed;
    }
    [Serializable] public class Report
    {
        public string action;
        public List<Sample> samples = new List<Sample>();
        public List<string> failures = new List<string>();
        public string scene;
        public string validation;
        public string screenshot;
    }

    static EnvironmentAssetTrials() { EditorApplication.update += Poll; }
    private static void Poll()
    {
        if (running || EditorApplication.timeSinceStartup < nextCheck) return;
        nextCheck = EditorApplication.timeSinceStartup + 1;
        if (!File.Exists(RequestPath) || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        running = true;
        var report = new Report();
        try
        {
            report.action = JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath)).action;
            if (report.action == "inspect") Inspect(report);
            else if (report.action == "refine") Refine(report);
            else Build(report);
        }
        catch (Exception error) { report.failures.Add(error.ToString()); }
        finally
        {
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/EnvironmentAssetTrials.json", JsonUtility.ToJson(report, true));
            File.Delete(RequestPath);
            running = false;
        }
        Debug.Log("Environment asset trial: " + report.action + ", failures=" + report.failures.Count);
    }

    private static Bounds BoundsOf(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) throw new InvalidOperationException("No renderer: " + obj.name);
        Bounds bounds = renderers[0].bounds;
        foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        return bounds;
    }

    private static void Inspect(Report report)
    {
        foreach (string path in Sources)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) { report.failures.Add("Missing source: " + path); continue; }
            var sample = new Sample { source = path, size = BoundsOf(asset).size,
                colliders = asset.GetComponentsInChildren<Collider>(true).Length };
            foreach (var renderer in asset.GetComponentsInChildren<Renderer>(true))
                foreach (var material in renderer.sharedMaterials)
                    sample.materials.Add(material == null ? "MISSING" : material.name + " | " + material.shader.name
                        + " | texture=" + (material.mainTexture == null ? "None" : AssetDatabase.GetAssetPath(material.mainTexture)));
            report.samples.Add(sample);
        }
    }

    private static void Build(Report report)
    {
        const string scenePath = "Assets/_Project/Scenes/93_TestEnvironment.unity";
        if (File.Exists(scenePath)) throw new InvalidOperationException("Trial scene already exists; preserve it and inspect before rebuilding.");
        EnsureFolder("Assets/_Project/Prefabs/Environment/A09_Trials");
        EnsureFolder("Assets/_Project/Prefabs/Environment/A11_Trials");
        EnsureFolder("Assets/_Project/Materials/EnvironmentTrials");
        Scene previous = SceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            var container = new GameObject("Environment_Asset_Trials");
            var geometry = Group("World_Geometry", container.transform);
            var a09 = Group("Batch_01_A09_Cave", geometry);
            var a11 = Group("Batch_02_A11_Nature", geometry);
            var gameplay = Group("Gameplay", container.transform);
            var cameras = Group("Cameras", container.transform);
            var lighting = Group("Lighting", container.transform);
            Group("UI", container.transform);
            var groundMat = SimpleMaterial("M_Trial_Ground", new Color(.32f, .29f, .22f));
            var borderMat = SimpleMaterial("M_Trial_Border", new Color(.25f, .30f, .33f));
            var routeMat = SimpleMaterial("M_Trial_Route", new Color(.12f, .48f, .42f));
            Block("Safe_Floor_32x28m", geometry, new Vector3(0,-.5f,0), new Vector3(32,1,28), groundMat);
            Block("Boundary_West", geometry,new Vector3(-16,1.5f,0),new Vector3(.5f,3,28),borderMat);
            Block("Boundary_East", geometry,new Vector3(16,1.5f,0),new Vector3(.5f,3,28),borderMat);
            Block("Boundary_North", geometry,new Vector3(0,1.5f,14),new Vector3(32,3,.5f),borderMat);
            Block("Boundary_South", geometry,new Vector3(0,1.5f,-14),new Vector3(32,3,.5f),borderMat);
            var route = Block("Main_Walkway_3m",geometry,new Vector3(0,.006f,0),new Vector3(3,.012f,24),routeMat);
            UnityEngine.Object.DestroyImmediate(route.GetComponent<Collider>());

            string[] names = { "PF_A09_Ground_Trial", "PF_A09_Rock_Trial", "PF_A09_Wall_Trial",
                "PF_A11_NormalTree_Trial", "PF_A11_Bush_Trial", "PF_A11_Grass_Trial" };
            Vector3[] positions = { new Vector3(-8,.08f,-4), new Vector3(-11,0,5), new Vector3(-4,0,6),
                new Vector3(6,0,6), new Vector3(11,0,0), new Vector3(6,0,-5) };
            float[] targets = { 6, 2.2f, 3.8f, 4.8f, .8f, .35f };
            var instances = new List<GameObject>();
            for (int i = 0; i < Sources.Length; i++)
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Sources[i]);
                if (asset == null) throw new InvalidOperationException("Missing source: " + Sources[i]);
                var wrapper = new GameObject(names[i]);
                var visual = (GameObject)PrefabUtility.InstantiatePrefab(asset, scene);
                visual.transform.SetParent(wrapper.transform, false);
                visual.transform.localPosition = Vector3.zero;
                Bounds bounds = BoundsOf(visual);
                float measured = i >= 3 || i == 2 ? bounds.size.y : Mathf.Max(bounds.size.x, bounds.size.z);
                if (measured < .001f) throw new InvalidOperationException("Invalid source bounds: " + Sources[i]);
                visual.transform.localScale *= targets[i] / measured;
                bounds = BoundsOf(visual);
                visual.transform.position += new Vector3(-bounds.center.x, -(i == 0 ? bounds.max.y : bounds.min.y), -bounds.center.z);
                foreach (var transform in wrapper.GetComponentsInChildren<Transform>(true))
                    transform.gameObject.layer = LayerMask.NameToLayer("Environment");
                if (i >= 3) ApplyNatureMaterials(visual);
                else ApplyCaveMaterials(visual);
                if (i == 3)
                {
                    var trunk = wrapper.AddComponent<CapsuleCollider>();
                    trunk.radius = .18f;
                    trunk.height = 2.2f;
                    Vector3 pivot = wrapper.transform.InverseTransformPoint(visual.GetComponentInChildren<MeshFilter>().transform.position);
                    trunk.center = new Vector3(pivot.x, 1.1f, pivot.z);
                }
                else if (i >= 4)
                {
                    foreach (var collider in wrapper.GetComponentsInChildren<Collider>(true))
                        UnityEngine.Object.DestroyImmediate(collider);
                }
                else if (wrapper.GetComponentsInChildren<Collider>(true).Length == 0)
                {
                    foreach (var filter in wrapper.GetComponentsInChildren<MeshFilter>(true))
                        filter.gameObject.AddComponent<MeshCollider>().sharedMesh = filter.sharedMesh;
                }
                foreach (var transform in visual.GetComponentsInChildren<Transform>(true))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(transform);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(transform.gameObject);
                }
                string folder = i < 3 ? "A09_Trials" : "A11_Trials";
                string prefabPath = "Assets/_Project/Prefabs/Environment/" + folder + "/" + names[i] + ".prefab";
                PrefabUtility.SaveAsPrefabAssetAndConnect(wrapper,prefabPath,InteractionMode.AutomatedAction);
                wrapper.transform.SetParent(i < 3 ? a09 : a11, false);
                wrapper.transform.localPosition = positions[i];
                instances.Add(wrapper);
                var sample = new Sample { source = Sources[i], prefab = prefabPath, size = BoundsOf(wrapper).size,
                    colliders = wrapper.GetComponentsInChildren<Collider>(true).Length,
                    collisionPolicy = i == 3 ? "Trunk capsule only" : i >= 4 ? "Decorative foliage: no collider" : "Source geometry collider" };
                foreach (var renderer in wrapper.GetComponentsInChildren<Renderer>(true))
                    foreach (var material in renderer.sharedMaterials)
                    {
                        sample.materials.Add(material == null ? "MISSING" : material.name + " | " + material.shader.name);
                        if (material == null || material.shader == null || !material.shader.isSupported
                            || material.shader.name == "Hidden/InternalErrorShader")
                            report.failures.Add("Invalid trial material: " + names[i]);
                    }
                report.samples.Add(sample);
                Label(names[i].Replace("PF_", ""), geometry, positions[i] + new Vector3(0, .08f, -2), .12f);
            }
            Label("01  A09 - CAVE",geometry,new Vector3(-8,2.8f,-11),.18f);
            Label("02  A11 - NATURE",geometry,new Vector3(8,2.8f,-11),.18f);
            Label("A10: Dien Trong later / A12: alternative only",geometry,new Vector3(0,3,-13),.10f);
            var reference = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            reference.name = "Height_Reference_1_8m";
            reference.transform.SetParent(geometry, false);
            reference.transform.localPosition = new Vector3(2,.9f,-9);
            reference.transform.localScale = new Vector3(.6f,.9f,.6f);
            reference.GetComponent<Renderer>().sharedMaterial = SimpleMaterial("M_Trial_Reference",new Color(.9f,.62f,.2f));
            UnityEngine.Object.DestroyImmediate(reference.GetComponent<Collider>());
            Label("1.8 m",geometry,new Vector3(2,2,-9),.12f);

            var lightObject = new GameObject("Directional Light");
            lightObject.transform.SetParent(lighting,false);
            lightObject.transform.rotation = Quaternion.Euler(45,-30,0);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional; light.intensity = 1.6f; light.shadows = LightShadows.Soft;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.6f,.68f,.75f);
            RenderSettings.ambientEquatorColor = new Color(.4f,.44f,.4f);
            RenderSettings.ambientGroundColor = new Color(.25f,.24f,.2f);

            var main = (GameObject)PrefabUtility.InstantiatePrefab(LoadPrefab("f6d148d888ffbf54b9afe9936dfaec1f"),scene);
            main.transform.SetParent(cameras,false);
            var player = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Player/PF_An_Prototype.prefab"),scene);
            player.name = "An_EnvironmentTrial";
            player.transform.SetParent(gameplay,false);
            var spawn = new Vector3(0,.2f,-10);
            player.transform.localPosition = spawn;
            var cameraTarget = Array.Find(player.GetComponentsInChildren<Transform>(true),t => t.name == "PlayerCameraRoot");
            if (cameraTarget == null) throw new InvalidOperationException("Missing PlayerCameraRoot");
            var follow = (GameObject)PrefabUtility.InstantiatePrefab(LoadPrefab("a1a802ecaf6775746bb2a929fb554ad8"),scene);
            follow.transform.SetParent(cameras,false);
            var camera = follow.GetComponent<CinemachineCamera>();
            camera.Follow = cameraTarget;
            var target = camera.Target; target.CustomLookAtTarget = false; camera.Target = target;
            var thirdPerson = follow.GetComponent<CinemachineThirdPersonFollow>();
            var obstacles = thirdPerson.AvoidObstacles; obstacles.Enabled = true;
            obstacles.CollisionFilter = LayerMask.GetMask("Environment"); thirdPerson.AvoidObstacles = obstacles;
            var input = player.GetComponent<PlayerInput>();
            if (input != null) { input.camera = main.GetComponent<Camera>(); PrefabUtility.RecordPrefabInstancePropertyModifications(input); }
            foreach (var component in player.GetComponents<MonoBehaviour>())
            {
                var serialized = new SerializedObject(component);
                var mask = serialized.FindProperty("GroundLayers");
                if (mask != null) { mask.intValue = LayerMask.GetMask("Environment"); serialized.ApplyModifiedPropertiesWithoutUndo(); }
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(camera);
            PrefabUtility.RecordPrefabInstancePropertyModifications(thirdPerson);
            ValidateWalking(container,player,instances,report);
            player.transform.localPosition = spawn;
            player.transform.localRotation = Quaternion.identity;
            report.screenshot = CaptureOverview(container);
            EditorSceneManager.SaveScene(scene,scenePath);
            AssetDatabase.SaveAssets();
            report.scene = scenePath;
            report.validation = "Editor CharacterController collision probes; visual Play Mode review is still required before promotion to the main level.";
        }
        finally
        {
            if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            EditorSceneManager.CloseScene(scene,true);
        }
    }

    private static GameObject LoadPrefab(string guid)
    {
        string path=AssetDatabase.GUIDToAssetPath(guid);
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if(asset==null)throw new InvalidOperationException("Missing prefab GUID: "+guid);
        return asset;
    }

    private static void Refine(Report report)
    {
        var previousReport=JsonUtility.FromJson<Report>(File.ReadAllText("Logs/EnvironmentAssetTrials_Build.json"));
        report.samples=previousReport.samples;report.scene=previousReport.scene;report.validation=previousReport.validation;
        for(int i=0;i<report.samples.Count;i++)
        {
            string path=report.samples[i].prefab;
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                if(i<3)ApplyCaveMaterials(root);else ApplyNatureMaterials(root);
                PrefabUtility.SaveAsPrefabAsset(root,path);
                report.samples[i].materials.Clear();
                foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
                    foreach(var material in renderer.sharedMaterials)
                        report.samples[i].materials.Add(material.name+" | "+material.shader.name);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
        Scene previous=SceneManager.GetActiveScene();
        Scene scene=EditorSceneManager.OpenScene(report.scene,OpenSceneMode.Additive);
        try
        {
            SceneManager.SetActiveScene(scene);
            var root=Array.Find(scene.GetRootGameObjects(),o=>o.name=="Environment_Asset_Trials");
            report.screenshot=CaptureOverview(root);
        }
        finally
        {
            if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);
            EditorSceneManager.CloseScene(scene,true);
        }
    }

    private static void ApplyCaveMaterials(GameObject visual)
    {
        foreach(var renderer in visual.GetComponentsInChildren<Renderer>(true))
        {
            var materials=renderer.sharedMaterials;
            for(int i=0;i<materials.Length;i++)
            {
                var original=materials[i];
                string name=original.name.StartsWith("M_A09_")?original.name:"M_A09_"+original.name;
                Texture texture=original.HasProperty("_ColorTexture")?original.GetTexture("_ColorTexture"):original.GetTexture("_BaseMap");
                var material=SimpleMaterial(name,new Color(.82f,.85f,.8f,1));
                material.SetTexture("_BaseMap",texture);
                material.SetFloat("_Cull",0);
                materials[i]=material;EditorUtility.SetDirty(material);
            }
            renderer.sharedMaterials=materials;
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        }
    }

    private static string CaptureOverview(GameObject container)
    {
        Vector3 offset=new Vector3(10000,0,10000);
        var cameraObject=new GameObject("Temporary_Overview_Camera");
        SceneManager.MoveGameObjectToScene(cameraObject,container.scene);
        var camera=cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        var target=new RenderTexture(1280,900,24,RenderTextureFormat.ARGB32);
        var previous=RenderTexture.active;
        Texture2D texture=null;
        try
        {
            container.transform.position=offset;
            foreach(var renderer in container.GetComponentsInChildren<MeshRenderer>(true))
            {
                bool active=renderer.enabled;renderer.enabled=false;renderer.enabled=active;
            }
            camera.transform.position=offset+new Vector3(23,25,-29);
            camera.transform.LookAt(offset+new Vector3(0,1,0));
            camera.orthographic=true;camera.orthographicSize=19;
            camera.nearClipPlane=.1f;camera.farClipPlane=150;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.09f,.12f,.15f);
            target.Create();
            UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,
                new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest { destination=target });
            RenderTexture.active=target;
            texture=new Texture2D(1280,900,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,1280,900),0,0);texture.Apply();
            Directory.CreateDirectory("Logs");
            const string path="Logs/EnvironmentTrialOverview.png";
            File.WriteAllBytes(path,texture.EncodeToPNG());
            return path;
        }
        finally
        {
            RenderTexture.active=previous;
            container.transform.position=Vector3.zero;
            if(texture!=null)UnityEngine.Object.DestroyImmediate(texture);
            target.Release();UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(cameraObject);
        }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\','/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent,Path.GetFileName(path));
    }
    private static Transform Group(string name, Transform parent)
    {
        var obj = new GameObject(name); obj.transform.SetParent(parent,false); return obj.transform;
    }
    private static Material SimpleMaterial(string name, Color color)
    {
        string path = "Assets/_Project/Materials/EnvironmentTrials/"+name+".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material,path);
        }
        material.SetColor("_BaseColor",color); material.SetFloat("_Smoothness",.15f);
        EditorUtility.SetDirty(material); return material;
    }
    private static void ApplyNatureMaterials(GameObject visual)
    {
        const string textures = "Assets/A11 - Ultimate Stylized Nature Pack/Textures/";
        foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null) throw new InvalidOperationException("Missing nature source material");
                string original = materials[i].name;
                if(original.StartsWith("M_A11_"))original=original.Substring(6);
                bool bark = original.IndexOf("Bark",StringComparison.OrdinalIgnoreCase) >= 0;
                var material = SimpleMaterial("M_A11_"+original,bark ? new Color(.75f,.66f,.52f) : new Color(.43f,.58f,.35f));
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(textures+original+".png");
                if (texture == null) throw new InvalidOperationException("Missing nature base texture: "+original);
                material.SetTexture("_BaseMap",texture);
                if (!bark)
                {
                    material.SetFloat("_AlphaClip",1); material.SetFloat("_Cutoff",.45f); material.SetFloat("_Cull",0);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.SetOverrideTag("RenderType","TransparentCutout");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                }
                materials[i] = material; EditorUtility.SetDirty(material);
            }
            renderer.sharedMaterials = materials;
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        }
    }
    private static GameObject Block(string name,Transform parent,Vector3 position,Vector3 scale,Material material)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name = name;
        obj.transform.SetParent(parent,false); obj.transform.localPosition = position; obj.transform.localScale = scale;
        obj.layer = LayerMask.NameToLayer("Environment"); obj.GetComponent<Renderer>().sharedMaterial = material; return obj;
    }
    private static void Label(string text, Transform parent, Vector3 position,float size)
    {
        var obj = new GameObject("Label_"+text); obj.transform.SetParent(parent,false); obj.transform.localPosition = position;
        var label = obj.AddComponent<TextMesh>(); label.text = text; label.characterSize = size; label.fontSize = 48;
        label.anchor = TextAnchor.MiddleCenter; label.color = Color.white;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        obj.GetComponent<MeshRenderer>().sharedMaterial = label.font.material;
    }

    private static void ValidateWalking(GameObject container,GameObject player,List<GameObject> samples,Report report)
    {
        // Offset the temporary trial scene so the user's other open scenes cannot
        // influence these collision probes. No physics simulation of those scenes.
        Vector3 offset = new Vector3(10000,0,10000);
        var behaviours = player.GetComponentsInChildren<MonoBehaviour>(true);
        var enabled = new bool[behaviours.Length];
        for (int i=0;i<behaviours.Length;i++) { enabled[i]=behaviours[i].enabled; behaviours[i].enabled=false; }
        var controller = player.GetComponent<CharacterController>();
        if (controller == null) throw new InvalidOperationException("Missing player CharacterController");
        container.transform.position = offset;
        Physics.SyncTransforms();
        try
        {
            for (int i=0;i<samples.Count;i++)
            {
                Bounds bounds = BoundsOf(samples[i]);
                float margin = controller.radius + .65f;
                Vector3[] corners = {
                    new Vector3(bounds.min.x-margin,.08f,bounds.min.z-margin),
                    new Vector3(bounds.max.x+margin,.08f,bounds.min.z-margin),
                    new Vector3(bounds.max.x+margin,.08f,bounds.max.z+margin),
                    new Vector3(bounds.min.x-margin,.08f,bounds.max.z+margin)
                };
                Teleport(controller,corners[0]); bool walked = true;
                for (int edge=0;edge<4;edge++)
                {
                    Vector3 destination=corners[(edge+1)%4];
                    Vector3 horizontal=destination-player.transform.position;horizontal.y=0;
                    int steps=Mathf.CeilToInt(horizontal.magnitude/.08f);
                    Vector3 delta=horizontal/Mathf.Max(1,steps);
                    for (int step=0;step<steps;step++) controller.Move(delta+Vector3.down*.03f);
                    Vector3 error=player.transform.position-destination;error.y=0;
                    if(error.magnitude>.15f) walked=false;
                }
                report.samples[i].walkAroundPassed=walked;
                if(!walked)report.failures.Add("Walk-around route blocked: "+samples[i].name);
                if(i>=1 && i<=3)
                {
                    Vector3 start=new Vector3(bounds.center.x,.08f,bounds.min.z-1.5f);
                    // Test the tree's trunk, not the much wider leaf canopy.
                    if(i==3)
                    {
                        Bounds trunk=samples[i].GetComponent<CapsuleCollider>().bounds;
                        start=new Vector3(trunk.center.x,.08f,trunk.min.z-1.5f);
                    }
                    Teleport(controller,start);bool hit=false;
                    int steps=Mathf.CeilToInt((bounds.size.z+3)/.08f);
                    for(int step=0;step<steps;step++)
                        hit |= (controller.Move(new Vector3(0,-.03f,.08f)) & CollisionFlags.Sides)!=0;
                    report.samples[i].blockingPassed=hit;
                    if(!hit)report.failures.Add("Solid collision not detected: "+samples[i].name);
                }
                else if(i>=4)
                {
                    Vector3 start=new Vector3(bounds.center.x,.08f,bounds.min.z-.6f);
                    Teleport(controller,start);int steps=Mathf.CeilToInt((bounds.size.z+1.2f)/.08f);
                    bool blocked=false;
                    for(int step=0;step<steps;step++)blocked|=(controller.Move(new Vector3(0,-.03f,.08f))&CollisionFlags.Sides)!=0;
                    report.samples[i].blockingPassed=!blocked;
                    if(blocked)report.failures.Add("Decorative foliage blocks player: "+samples[i].name);
                }
                else
                {
                    bool supported=false;
                    for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++)
                    {
                        Vector3 origin=new Vector3(bounds.center.x+x,bounds.max.y+2,bounds.center.z+z);
                        if(Physics.Raycast(origin,Vector3.down,out RaycastHit hit,5,LayerMask.GetMask("Environment")))
                            supported|=hit.collider.transform.IsChildOf(samples[i].transform);
                    }
                    report.samples[i].blockingPassed=supported;
                    if(!supported)report.failures.Add("Ground surface collider not detected");
                }
            }
        }
        finally
        {
            container.transform.position=Vector3.zero;
            for(int i=0;i<behaviours.Length;i++)behaviours[i].enabled=enabled[i];
            Physics.SyncTransforms();
        }
    }
    private static void Teleport(CharacterController controller,Vector3 position)
    {
        controller.enabled=false;controller.transform.position=position;controller.enabled=true;Physics.SyncTransforms();
        for(int i=0;i<8;i++)controller.Move(Vector3.down*.03f);
    }
}
