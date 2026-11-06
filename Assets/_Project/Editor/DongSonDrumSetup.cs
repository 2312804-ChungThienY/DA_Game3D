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
using static Chapter01RevisionSetup;

// Explicit, narrowly scoped authoring. Never rebuilds the chapter or its navigation.
[InitializeOnLoad]
public static class DongSonDrumSetup
{
    public const string Art="Assets/_Project/Art/DongSonDrum";
    public const string ModelPath=Art+"/Source/source/TrongDongDongSonArt.fbx";
    public const string MaterialPath=Art+"/Materials/DongSon_EngravedBronze.mat";
    const string Request="Library/SonDoongDongSonDrums.request.json";
    const string TexPrefix=Art+"/Source/textures/TrongDongDongSonArt_DefaultMaterial_";
    static bool busy;
    [Serializable]class Command{public string action;}
    [Serializable]class Report{public string action;public List<string> checks=new List<string>(),failures=new List<string>(),notes=new List<string>();}
    static DongSonDrumSetup()=>EditorApplication.update+=Poll;
    static void Poll()
    {
        if(busy||EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(Request))return;
        busy=true;var r=new Report();
        try
        {
            r.action=JsonUtility.FromJson<Command>(File.ReadAllText(Request)).action;
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before authoring.");
            for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Unsaved user scene: "+SceneManager.GetSceneAt(i).path);
            if(r.action=="test")
            {
                SessionState.SetBool("SonDoong.DongSonDrumsTest",true);
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/00_Boot.unity");
                EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();
                EditorApplication.isPaused=false;EditorApplication.EnterPlaymode();
            }
            else
            {
                if(r.action=="prepare")Prepare(r);
                var setup=EditorSceneManager.GetSceneManagerSetup();
                try
                {
                    foreach(var path in r.action=="apply"?new[]{Preview,Main}:new[]{Main})
                    {
                        if(r.action=="apply")Backup(path,r);
                        var scene=EditorSceneManager.OpenScene(path);
                        if(r.action=="apply")Apply(scene,r);
                        Inspect(scene,r);
                        if(r.action=="apply"){if(r.failures.Count>0)throw new InvalidOperationException("Drum validation failed; scene not saved.");EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();}
                        if(r.action=="render"||r.action=="prepare"||r.action=="apply")Capture(scene,r);
                    }
                }
                finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
        }
        catch(Exception e){r.failures.Add(e.ToString());}
        finally{Directory.CreateDirectory("Logs");File.WriteAllText("Logs/DongSonDrums-"+r.action+".json",JsonUtility.ToJson(r,true));File.Delete(Request);busy=false;}
    }
    static void Backup(string path,Report r)
    {
        const string dir="SourceAssets/SceneBackups/2026-11-06_DongSonDrums";Directory.CreateDirectory(dir);
        foreach(var p in new[]{path,path+".meta"}){string dst=dir+"/"+Path.GetFileName(p);if(!File.Exists(dst))File.Copy(p,dst);}
        r.notes.Add("Original scene/metadata backup: "+dir+"/"+Path.GetFileName(path));
    }
    static Texture2D ImportTexture(string suffix,bool linear=false,bool readable=false,bool normal=false)
    {
        string path=TexPrefix+suffix+".png";
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var imp=(TextureImporter)AssetImporter.GetAtPath(path);
        if(!imp)throw new InvalidOperationException("Texture not imported: "+path);
        imp.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;
        imp.sRGBTexture=!linear&&!normal;imp.isReadable=readable;imp.mipmapEnabled=true;
        imp.maxTextureSize=2048;imp.anisoLevel=16;imp.filterMode=FilterMode.Trilinear;
        imp.textureCompression=TextureImporterCompression.CompressedHQ;
        var pc=imp.GetPlatformTextureSettings("Standalone");pc.overridden=true;pc.maxTextureSize=2048;
        pc.format=normal?TextureImporterFormat.BC5:TextureImporterFormat.BC7;pc.compressionQuality=100;imp.SetPlatformTextureSettings(pc);
        imp.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static void Prepare(Report r)
    {
        Folder(Art+"/Materials");Folder(Art+"/Textures");
        var model=(ModelImporter)AssetImporter.GetAtPath(ModelPath);
        model.importAnimation=false;model.addCollider=false;model.meshCompression=ModelImporterMeshCompression.Off;
        model.importTangents=ModelImporterTangents.CalculateMikk;model.materialImportMode=ModelImporterMaterialImportMode.None;
        model.SaveAndReimport();
        var baseMap=ImportTexture("BaseCo");var normal=ImportTexture("Normal",true,false,true);
        var metal=ImportTexture("Metall",true,true);var rough=ImportTexture("Roughn",true,true);
        var pixels=metal.GetPixels32();var roughPixels=rough.GetPixels32();
        if(pixels.Length!=roughPixels.Length)throw new InvalidOperationException("PBR maps have different dimensions.");
        for(int i=0;i<pixels.Length;i++)pixels[i]=new Color32(pixels[i].r,0,0,(byte)(255-roughPixels[i].r));
        var packed=new Texture2D(metal.width,metal.height,TextureFormat.RGBA32,false,true);
        packed.SetPixels32(pixels);packed.Apply();string packedPath=Art+"/Textures/Metallic_Smoothness.png";
        File.WriteAllBytes(packedPath,packed.EncodeToPNG());UnityEngine.Object.DestroyImmediate(packed);
        AssetDatabase.ImportAsset(packedPath,ImportAssetOptions.ForceSynchronousImport);
        var pi=(TextureImporter)AssetImporter.GetAtPath(packedPath);pi.sRGBTexture=false;pi.mipmapEnabled=true;
        pi.maxTextureSize=2048;pi.anisoLevel=16;pi.filterMode=FilterMode.Trilinear;
        var ps=pi.GetPlatformTextureSettings("Standalone");ps.overridden=true;ps.maxTextureSize=2048;ps.format=TextureImporterFormat.BC7;ps.compressionQuality=100;pi.SetPlatformTextureSettings(ps);pi.SaveAndReimport();
        ImportTexture("Metall",true);ImportTexture("Roughn",true);ImportTexture("Height",true);
        var mat=AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,MaterialPath);}
        mat.SetColor("_BaseColor",Color.white);mat.SetTexture("_BaseMap",baseMap);mat.SetTexture("_BumpMap",normal);mat.SetFloat("_BumpScale",.65f);
        mat.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(packedPath));
        mat.SetFloat("_Metallic",1);mat.SetFloat("_Smoothness",.68f);mat.SetFloat("_SmoothnessTextureChannel",0);
        mat.EnableKeyword("_NORMALMAP");mat.EnableKeyword("_METALLICSPECGLOSSMAP");mat.DisableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
        mat.SetFloat("_Cull",2);EditorUtility.SetDirty(mat);AssetDatabase.SaveAssets();
        r.checks.Add("Original 2K base/normal/metal/rough maps retained; linear metallic R and inverted roughness A packed for URP, BC7/BC5, mipmaps and anisotropy16; no geometry decimation.");
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        foreach(var m in asset.GetComponentsInChildren<MeshFilter>(true))r.notes.Add("SOURCE "+m.name+": "+m.sharedMesh.vertexCount+" vertices, "+m.sharedMesh.triangles.Length/3+" triangles; local bounds="+m.sharedMesh.bounds+", transform="+m.transform.localToWorldMatrix);
    }
    static void Apply(Scene s,Report r)
    {
        var mat=AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);if(!mat)throw new InvalidOperationException("Prepare the drum PBR material first.");
        foreach(var drum in All<DrumInteractable>(s))
        {
            var old=drum.transform.Find("VIS_DongSon_Playable");if(!old)throw new InvalidOperationException("Expected existing visual under "+drum.name);
            // Only replace the model subtree. Component refs and collision stay on the original root.
            UnityEngine.Object.DestroyImmediate(old.gameObject);
            var visual=Chapter01CompletionArt.Model(ModelPath,"VIS_DongSon_Playable",drum.transform,new Vector3(drum.transform.position.x,.12f,drum.transform.position.z),1.12f,0,mat);
            foreach(var rr in visual.GetComponentsInChildren<Renderer>(true))rr.gameObject.layer=2;
            var note=(DrumNote)new SerializedObject(drum).FindProperty("note").enumValueIndex;
            var plaque=All<Transform>(s).Single(t=>t.name=="Emblem_Plaque_"+note);
            var symbol=All<Transform>(s).Single(t=>t.name=="Symbol_"+note);
            plaque.position=new Vector3(drum.transform.position.x-.95f,.61f,drum.transform.position.z-.85f);plaque.localScale=new Vector3(.50f,.38f,.07f);
            symbol.position=plaque.position+Vector3.back*.04f;symbol.localScale=Vector3.one*.68f;
            var post=plaque.parent.Find("Reading_Sign_Post_"+note);
            if(!post)
            {
                var rod=GameObject.CreatePrimitive(PrimitiveType.Cylinder);rod.name="Reading_Sign_Post_"+note;post=rod.transform;
                post.SetParent(plaque.parent,true);UnityEngine.Object.DestroyImmediate(rod.GetComponent<Collider>());rod.layer=2;
                rod.GetComponent<Renderer>().sharedMaterial=plaque.GetComponent<Renderer>().sharedMaterial;
            }
            post.position=new Vector3(plaque.position.x,.285f,plaque.position.z+.02f);post.localScale=new Vector3(.055f,.165f,.055f);
            var lightRoot=drum.transform.Find("ART_Drum_ReadingLight");
            if(!lightRoot){lightRoot=new GameObject("ART_Drum_ReadingLight").transform;lightRoot.SetParent(drum.transform,true);lightRoot.gameObject.AddComponent<Light>();}
            lightRoot.position=drum.transform.position+new Vector3(-.5f,2.7f,-2.1f);lightRoot.LookAt(new Vector3(drum.transform.position.x,.9f,drum.transform.position.z));
            var light=lightRoot.GetComponent<Light>();light.type=LightType.Spot;light.color=new Color(1,.87f,.66f);light.intensity=22;light.range=6;light.spotAngle=64;light.innerSpotAngle=38;light.shadows=LightShadows.None;
            r.checks.Add(s.name+": replaced only FBX visual under "+drum.name+"; root position/rotation/scale, note, puzzle/audio/indicator refs and collider preserved.");
        }
        foreach(var c in All<Component>(s))if(c&&PrefabUtility.IsPartOfPrefabInstance(c))PrefabUtility.RecordPrefabInstancePropertyModifications(c);
    }
    static void Inspect(Scene s,Report r)
    {
        var drums=All<DrumInteractable>(s);r.notes.Add(s.name+": drums="+drums.Length);
        if(drums.Length!=3)r.failures.Add(s.name+": expected three drum interactables.");
        foreach(var drum in drums)
        {
            var visual=drum.transform.Find("VIS_DongSon_Playable");
            var so=new SerializedObject(drum);r.notes.Add(drum.name+" pos="+drum.transform.position+" rotation="+drum.transform.eulerAngles+" scale="+drum.transform.lossyScale+" note="+so.FindProperty("note").enumValueIndex+" visual="+(visual?Chapter01GuidePedestalSetup.GeometryBounds(visual.gameObject).ToString():"missing"));
            foreach(var c in drum.GetComponents<Collider>())r.notes.Add("  collider "+c.GetType().Name+" "+c.bounds);
            if(!visual){r.failures.Add("Missing visual: "+drum.name);continue;}
            foreach(var rr in visual.GetComponentsInChildren<Renderer>(true))
            {
                if(rr.sharedMaterials.Any(m=>!m||!m.shader||!m.shader.isSupported))r.failures.Add("Unsupported material: "+rr.name);
            }
            if(r.action=="apply"||r.action=="render")
            {
                if(visual.GetComponentsInChildren<MeshFilter>(true).Any(m=>AssetDatabase.GetAssetPath(m.sharedMesh)!=ModelPath))r.failures.Add("Wrong source model: "+drum.name);
                if(visual.GetComponentsInChildren<Collider>(true).Any(c=>c.enabled))r.failures.Add("Added decoration collider: "+drum.name);
                if(visual.GetComponentsInChildren<Renderer>(true).Any(rr=>rr.sharedMaterials.Any(m=>AssetDatabase.GetAssetPath(m)!=MaterialPath)))r.failures.Add("PBR material not assigned: "+drum.name);
                if(!Ref<DrumPuzzleController>(drum,"puzzle")||!drum.GetComponent<Collider>())r.failures.Add("Gameplay contract missing: "+drum.name);
                else r.checks.Add(s.name+": "+drum.name+" uses supplied FBX/URP PBR; no visual collider; existing puzzle and interaction collider valid.");
            }
        }
        foreach(var t in All<Transform>(s).Where(t=>t.name.StartsWith("Drum_Emblem")))r.notes.Add("EMBLEM "+t.name+" "+t.position);
        if(All<Transform>(s).Any(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0))r.failures.Add(s.name+": missing scripts.");
    }
    static void Shot(Camera cam,Vector3 from,Vector3 to,string path)
    {
        cam.transform.position=from;cam.transform.LookAt(to);var rt=new RenderTexture(1600,1000,24);rt.Create();var previous=RenderTexture.active;var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);
        try{RenderPipeline.SubmitRenderRequest(cam,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();Directory.CreateDirectory("docs/images");File.WriteAllBytes(path,tex.EncodeToPNG());}
        finally{RenderTexture.active=previous;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
    }
    static void Capture(Scene s,Report r)
    {
        var drums=All<DrumInteractable>(s);var center=drums.Select(d=>d.transform.position).Aggregate(Vector3.zero,(a,b)=>a+b)/3;
        var go=new GameObject("Drum_Inspection_Camera");var cam=go.AddComponent<Camera>();cam.nearClipPlane=.05f;cam.farClipPlane=200;cam.fieldOfView=55;cam.clearFlags=CameraClearFlags.Skybox;
        try
        {
            string prefix="docs/images/dongson-"+s.name+"-";
            Shot(cam,center+new Vector3(0,1.65f,-5.4f),center+Vector3.up*.65f,prefix+"approach-2026-11-06.png");
            var d=drums.OrderBy(x=>x.transform.position.x).First();var b=Chapter01GuidePedestalSetup.GeometryBounds(d.transform.Find("VIS_DongSon_Playable").gameObject);
            Shot(cam,b.center+new Vector3(.85f,1.9f,-1.9f),b.center,prefix+"detail-2026-11-06.png");
            r.notes.Add("Rendered approach and engraved-face detail: "+prefix);
        }
        finally{UnityEngine.Object.DestroyImmediate(go);}
    }
}
