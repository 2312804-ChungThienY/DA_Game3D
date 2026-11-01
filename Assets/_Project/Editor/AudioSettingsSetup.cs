using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[InitializeOnLoad]
public static class AudioSettingsSetup
{
    const string RequestFile="Library/SonDoongAudioSetup.request.json";
    const string MixerPath="Assets/_Project/Audio/Mixers/MainAudioMixer.mixer";
    static double next;
    static bool running;
    const BindingFlags All=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    [Serializable] public class Request { public string action="inspect"; }
    [Serializable] public class Result { public string action; public List<string> checks=new List<string>(); public List<string> failures=new List<string>(); }
    static AudioSettingsSetup() { EditorApplication.update+=Poll; }
    static void Poll()
    {
        if(running||EditorApplication.timeSinceStartup<next)return;
        next=EditorApplication.timeSinceStartup+.5;
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(RequestFile))return;
        var result=new Result();running=true;
        try
        {
            result.action=JsonUtility.FromJson<Request>(File.ReadAllText(RequestFile)).action;
            if(EditorApplication.isPlaying) { EditorApplication.ExitPlaymode();throw new InvalidOperationException("Exited Play Mode; repeat the request after the editor is ready."); }
            if(result.action=="install")Install(result);
            else if(result.action=="validate")Validate(result);
            else if(result.action=="build-report")ReportMessages(UnityEditor.Build.Reporting.BuildReport.GetLatestReport(),result);
            else if(result.action=="build-player")BuildPlayer(result);
            else if(result.action=="preview"||result.action=="preview-game")Preview(result);
            else if(result.action=="finish")FinishConfiguration(result);
            else
            {
                result.checks.Add("Editor playing="+EditorApplication.isPlaying);
                for(int i=0;i<SceneManager.sceneCount;i++){var s=SceneManager.GetSceneAt(i);result.checks.Add(s.path+" dirty="+s.isDirty);}
                var t=MixerType();result.checks.Add("Mixer API="+t.AssemblyQualifiedName);
                foreach(var m in t.GetMethods(All).Where(m=>m.Name=="CreateNewGroup"||m.Name=="CreateMixerControllerAtPath"))result.checks.Add(m.ToString());
            }
        }
        catch(Exception e){result.failures.Add(e.ToString());}
        finally{Directory.CreateDirectory("Logs");File.WriteAllText("Logs/AudioSettingsSetup.json",JsonUtility.ToJson(result,true));File.Delete(RequestFile);running=false;}
        Debug.Log("Audio settings setup: "+result.action+", failures="+result.failures.Count);
    }
    static Type MixerType()
    {
        var type=Type.GetType("UnityEditor.Audio.AudioMixerController, UnityEditor.AudioModule");
        if(type==null)type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.Audio.AudioMixerController")).FirstOrDefault(t=>t!=null);
        if(type==null)throw new InvalidOperationException("Unity Editor AudioMixer API unavailable.");
        return type;
    }
    static object Call(object target,string name,params object[] args)
    {
        var method=target.GetType().GetMethods(All).FirstOrDefault(m=>m.Name==name&&m.GetParameters().Length==args.Length);
        if(method==null)throw new MissingMethodException(target.GetType().Name,name);
        return method.Invoke(target,args);
    }
    static void Folder(string path)
    {
        if(AssetDatabase.IsValidFolder(path))return;
        string parent=Path.GetDirectoryName(path).Replace('\\','/');Folder(parent);AssetDatabase.CreateFolder(parent,Path.GetFileName(path));
    }
    static AudioMixer Mixer(Result result)
    {
        Folder("Assets/_Project/Audio/Mixers");
        var mixer=AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
        if(mixer==null)mixer=(AudioMixer)MixerType().GetMethod("CreateMixerControllerAtPath",All).Invoke(null,new object[]{MixerPath});
        var type=mixer.GetType();var master=type.GetProperty("masterGroup",All).GetValue(mixer);
        foreach(string name in new[]{"Music","SFX","UI"})
        {
            var group=mixer.FindMatchingGroups(name).FirstOrDefault(g=>g.name==name);
            if(group==null)
            {
                group=(AudioMixerGroup)Call(mixer,"CreateNewGroup",name,false);
                var parent=name=="UI"?(object)mixer.FindMatchingGroups("SFX").First(g=>g.name=="SFX"):master;
                Call(mixer,"AddChildToParent",group,parent);
            }
        }
        var property=type.GetProperty("exposedParameters",All);var element=property.PropertyType.GetElementType();
        Array parameters=Array.CreateInstance(element,3);int n=0;
        foreach(string name in new[]{"Master","Music","SFX"})
        {
            object group=name=="Master"?master:mixer.FindMatchingGroups(name).First(g=>g.name==name);
            object guid=Call(group,"GetGUIDForVolume");object item=Activator.CreateInstance(element);
            element.GetField("guid",All).SetValue(item,guid);element.GetField("name",All).SetValue(item,name=="SFX"?"SFXVolume":name+"Volume");parameters.SetValue(item,n++);
            object snapshot=type.GetProperty("TargetSnapshot",All).GetValue(mixer);
            Call(group,"SetValueForVolume",mixer,snapshot,name=="Music"?AudioSettingsService.ToDecibels(.7f):0f);
            EditorUtility.SetDirty((UnityEngine.Object)group);
        }
        property.SetValue(mixer,parameters);EditorUtility.SetDirty(mixer);AssetDatabase.SaveAssets();
        result.checks.Add("Shared mixer: Master -> Music, SFX -> UI; three exposed volumes");return mixer;
    }
    static IEnumerable<T> Components<T>(Scene scene) where T:Component =>scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<T>(true));
    static void Set(UnityEngine.Object obj,string name,UnityEngine.Object value)
    {
        var so=new SerializedObject(obj);var property=so.FindProperty(name);if(property==null)throw new InvalidOperationException(obj.name+" missing field "+name);
        property.objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();
    }
    static T Field<T>(UnityEngine.Object obj,string name) where T:UnityEngine.Object =>(T)obj.GetType().GetField(name,All).GetValue(obj);
    static RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size)
    {
        var go=new GameObject(name,typeof(RectTransform));go.layer=LayerMask.NameToLayer("UI");var rt=go.GetComponent<RectTransform>();rt.SetParent(parent,false);
        rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.anchoredPosition=pos;rt.sizeDelta=size;return rt;
    }
    static TextMeshProUGUI Text(string name,string text,Transform parent,Vector2 pos,Vector2 size,TMP_FontAsset font,int fontSize,TextAlignmentOptions alignment=TextAlignmentOptions.Center)
    {
        var rt=Rect(name,parent,pos,size);var label=rt.gameObject.AddComponent<TextMeshProUGUI>();label.font=font;label.text=text;label.fontSize=fontSize;label.alignment=alignment;label.color=new Color(.95f,.96f,.92f);label.raycastTarget=false;return label;
    }
    static Button Button(string name,string text,Transform parent,Vector2 pos,TMP_FontAsset font,UnityAction action)
    {
        var rt=Rect(name,parent,pos,new Vector2(250,58));var image=rt.gameObject.AddComponent<Image>();image.color=new Color(.17f,.38f,.4f);
        var button=rt.gameObject.AddComponent<Button>();button.targetGraphic=image;UnityEventTools.AddPersistentListener(button.onClick,action);
        Text("Label",text,rt,Vector2.zero,new Vector2(240,50),font,26);return button;
    }
    static Slider Slider(string name,Transform parent,Vector2 pos)
    {
        var root=Rect(name,parent,pos,new Vector2(380,38));var slider=root.gameObject.AddComponent<Slider>();
        var background=Rect("Background",root,Vector2.zero,new Vector2(380,12));background.gameObject.AddComponent<Image>().color=new Color(.13f,.17f,.2f);
        var fillArea=Rect("Fill Area",root,Vector2.zero,new Vector2(356,12));var fill=Rect("Fill",fillArea,Vector2.zero,Vector2.zero);
        fill.anchorMin=Vector2.zero;fill.anchorMax=Vector2.one;fill.gameObject.AddComponent<Image>().color=new Color(.36f,.68f,.57f);
        var handleArea=Rect("Handle Slide Area",root,Vector2.zero,new Vector2(356,38));var handle=Rect("Handle",handleArea,Vector2.zero,new Vector2(26,38));
        var graphic=handle.gameObject.AddComponent<Image>();graphic.color=new Color(.94f,.79f,.5f);
        slider.fillRect=fill;slider.handleRect=handle;slider.targetGraphic=graphic;slider.minValue=0;slider.maxValue=1;slider.wholeNumbers=false;return slider;
    }
    static TMP_FontAsset FontFor(Scene scene)
    {
        var existing=Components<TMP_Text>(scene).Select(t=>t.font).FirstOrDefault(f=>f!=null);
        if(existing==null)existing=TMP_Settings.defaultFontAsset;
        if(existing==null)throw new InvalidOperationException("No TMP font configured in "+scene.name);
        const string chars="Âm thanh Tổng Nhạc Hiệu ứng Mặc định Đóng Tiếp tục Về menu";
        if(existing.HasCharacters(chars,out uint[] missing,true))return existing;
        Folder("Assets/_Project/UI/Fonts");const string path="Assets/_Project/UI/Fonts/AudioSettings_Vietnamese.asset";
        var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if(font==null)
        {
            var sourceFont=AssetDatabase.LoadAssetAtPath<Font>("Assets/A31 - Noto Sans/static/NotoSans-Regular.ttf") ?? existing.sourceFontFile ?? AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
            if(sourceFont==null)throw new InvalidOperationException("No font source available for Vietnamese labels.");
            font=TMP_FontAsset.CreateFontAsset(sourceFont,48,8,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
            font.name="AudioSettings_Vietnamese";AssetDatabase.CreateAsset(font,path);
            AssetDatabase.AddObjectToAsset(font.material,font);
            foreach(var atlas in font.atlasTextures)AssetDatabase.AddObjectToAsset(atlas,font);
        }
        string glyphs=chars+new string(Enumerable.Range(32,95).Select(c=>(char)c).ToArray());
        if(!font.HasCharacters(glyphs,out uint[] absent))
        {
            var fontData=new SerializedObject(font);
            var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/A31 - Noto Sans/static/NotoSans-Regular.ttf");
            fontData.FindProperty("m_SourceFontFilePath").stringValue=Path.GetFullPath(AssetDatabase.GetAssetPath(source));
            fontData.FindProperty("m_SourceFontFile").objectReferenceValue=source;
            fontData.ApplyModifiedPropertiesWithoutUndo();
            font.atlasPopulationMode=AtlasPopulationMode.Dynamic;
            if(!font.TryAddCharacters(glyphs,out string unavailable))throw new InvalidOperationException("Vietnamese glyphs unavailable: "+unavailable);
        }
        font.atlasPopulationMode=AtlasPopulationMode.Static;

        var cleanup=new SerializedObject(font);cleanup.FindProperty("m_SourceFontFilePath").stringValue="";cleanup.FindProperty("m_ClearDynamicDataOnBuild").boolValue=false;cleanup.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(font);return font;
    }
    static GameObject Panel(Transform parent,AudioSettingsService service,TMP_FontAsset font,UnityAction close)
    {
        var root=Rect("Panel_AudioSettings",parent,Vector2.zero,Vector2.zero);root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;
        root.gameObject.AddComponent<Image>().color=new Color(.025f,.04f,.055f,.97f);
        var content=Rect("Content",root,Vector2.zero,new Vector2(800,580));content.gameObject.AddComponent<Image>().color=new Color(.055f,.095f,.115f,.98f);
        Text("Title","Âm thanh",content,new Vector2(0,225),new Vector2(700,60),font,38);
        var controller=root.gameObject.AddComponent<AudioSettingsPanel>();Set(controller,"settings",service);
        string[] fields={"master","music","sfx"};string[] labels={"Tổng","Nhạc","Hiệu ứng"};
        for(int i=0;i<3;i++)
        {
            float y=110-i*105;Text("Label_"+fields[i],labels[i],content,new Vector2(-275,y),new Vector2(170,50),font,27,TextAlignmentOptions.Left);
            var slider=Slider("Slider_"+fields[i],content,new Vector2(35,y));slider.SetValueWithoutNotify(i==1?.7f:1f);Set(controller,fields[i]+"Slider",slider);
            var value=Text("Value_"+fields[i],i==1?"70%":"100%",content,new Vector2(305,y),new Vector2(100,50),font,26,TextAlignmentOptions.Right);Set(controller,fields[i]+"Value",value);
        }
        Button("BTN_AudioDefaults","Mặc định",content,new Vector2(-150,-225),font,controller.ResetDefaults);
        Button("BTN_AudioClose","Đóng",content,new Vector2(150,-225),font,close);
        root.gameObject.SetActive(false);return root.gameObject;
    }
    static AudioMixerGroup Group(AudioMixer mixer,string name)=>mixer.FindMatchingGroups(name).First(g=>g.name==name);
    static AudioSettingsService Service(Scene scene,AudioMixer mixer,Result result)
    {
        var service=Components<AudioSettingsService>(scene).SingleOrDefault();
        if(service==null){var obj=new GameObject("SceneAudio");SceneManager.MoveGameObjectToScene(obj,scene);service=obj.AddComponent<AudioSettingsService>();}
        Set(service,"mixer",mixer);var music=new List<AudioSource>();
        foreach(var source in Components<AudioSource>(scene))
        {
            bool isMusic=source.name.IndexOf("music",StringComparison.OrdinalIgnoreCase)>=0||source.name.IndexOf("bgm",StringComparison.OrdinalIgnoreCase)>=0||(source.loop&&source.spatialBlend<.01f);
            bool ui=source.GetComponentInParent<Canvas>()!=null;
            source.outputAudioMixerGroup=Group(mixer,isMusic?"Music":ui?"UI":"SFX");
            if(ui)source.ignoreListenerPause=true;
            if(isMusic){source.playOnAwake=false;source.loop=true;source.volume=1;source.spatialBlend=0;music.Add(source);}
            PrefabUtility.RecordPrefabInstancePropertyModifications(source);
        }
        var so=new SerializedObject(service);var array=so.FindProperty("startAfterApply");array.arraySize=music.Count;
        for(int i=0;i<music.Count;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=music[i];so.ApplyModifiedPropertiesWithoutUndo();
        result.checks.Add(scene.name+": SceneAudio active, routed audio sources="+Components<AudioSource>(scene).Count()+", music="+music.Count);return service;
    }
    static void EditScene(string path,AudioMixer mixer,Result result)
    {
        Scene previous=SceneManager.GetActiveScene();Scene scene=SceneManager.GetSceneByPath(path);bool opened=scene.IsValid()&&scene.isLoaded;
        if(!opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
        try
        {
            SceneManager.SetActiveScene(scene);
            if(scene.isDirty){Directory.CreateDirectory("Temp/AudioSettingsBackup_20261102/EditorScenes");EditorSceneManager.SaveScene(scene,"Temp/AudioSettingsBackup_20261102/EditorScenes/"+scene.name+".unity",true);}
            var font=FontFor(scene);var service=Service(scene,mixer,result);
            if(scene.name=="01_MainMenu")
            {
                var menu=Components<MainMenuController>(scene).Single();var existing=Components<Transform>(scene).FirstOrDefault(t=>t.name=="Panel_AudioSettings");
                if(existing!=null)UnityEngine.Object.DestroyImmediate(existing.gameObject);
                var oldSettings=Components<Button>(scene).FirstOrDefault(b=>b.name=="BTN_Settings");if(oldSettings!=null)UnityEngine.Object.DestroyImmediate(oldSettings.gameObject);
                Transform canvas=Field<Button>(menu,"newGameButton").GetComponentInParent<Canvas>().transform;
                var panel=Panel(canvas,service,font,menu.CloseAudioSettings);Set(menu,"audioSettingsPanel",panel);
                var button=Button("BTN_Settings","Âm thanh",canvas,new Vector2(0,-110),font,menu.OpenAudioSettings);button.GetComponent<RectTransform>().sizeDelta=new Vector2(330,62);Set(menu,"settingsButton",button);
                Field<Button>(menu,"newGameButton").GetComponent<RectTransform>().anchoredPosition=new Vector2(0,70);
                Field<Button>(menu,"continueButton").GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-20);
                Field<Button>(menu,"quitButton").GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-200);
                Field<TMP_Text>(menu,"statusText").rectTransform.anchoredPosition=new Vector2(0,-290);
                panel.transform.SetAsLastSibling();
            }
            else
            {
                var pause=Components<PauseMenuController>(scene).Single();var pausePanel=Field<GameObject>(pause,"pausePanel");
                var oldPanel=pausePanel.transform.Find("Panel_AudioSettings");if(oldPanel!=null)UnityEngine.Object.DestroyImmediate(oldPanel.gameObject);
                var oldButton=pausePanel.transform.Find("BTN_PauseAudio");if(oldButton!=null)UnityEngine.Object.DestroyImmediate(oldButton.gameObject);
                var panel=Panel(pausePanel.transform,service,font,pause.CloseAudioSettings);Set(pause,"audioSettingsPanel",panel);
                Button("BTN_PauseAudio","Âm thanh",pausePanel.transform,new Vector2(0,-50),font,pause.OpenAudioSettings);
                foreach(var button in pausePanel.GetComponentsInChildren<Button>(true))
                {
                    if(button.name=="BTN_Resume")button.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,40);
                    if(button.name=="BTN_ReturnMenu")button.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-140);
                }
                var status=Field<TMP_Text>(pause,"statusText");if(status!=null)status.rectTransform.anchoredPosition=new Vector2(0,-220);
                panel.transform.SetAsLastSibling();pausePanel.SetActive(false);
                foreach(var motor in Components<MonoBehaviour>(scene).Where(c=>c!=null&&c.GetType().Name=="ThirdPersonController"))Set(motor,"SfxOutput",Group(mixer,"SFX"));
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);result.checks.Add("Saved "+path);
        }
        finally{if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);if(!opened)EditorSceneManager.CloseScene(scene,true);}
    }
    static void Preview(Result result)
    {
        var paths=result.action=="preview-game"?new[]{"Assets/_Project/Scenes/02_HangLinhMach.unity"}:new[]{"Assets/_Project/Scenes/01_MainMenu.unity"};
        foreach(string path in paths)
        {
            Scene previous=SceneManager.GetActiveScene();Scene source=SceneManager.GetSceneByPath(path);bool opened=source.IsValid()&&source.isLoaded;
            if(!opened)source=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            Scene temporary=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            RenderTexture target=null;Texture2D texture=null;var prior=RenderTexture.active;
            try
            {
                SceneManager.SetActiveScene(temporary);
                var cameraObject=new GameObject("PreviewCamera");var camera=cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                camera.transform.position=new Vector3(0,0,-10);camera.orthographic=true;camera.orthographicSize=5;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.02f,.03f,.04f);camera.cullingMask=1<<30;
                target=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);target.Create();camera.targetTexture=target;
                var canvasObject=new GameObject("PreviewCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
                var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
                canvasObject.GetComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;canvas.scaleFactor=1280f/1920;
                var sourcePanel=Components<AudioSettingsPanel>(source).Single();
                var clone=UnityEngine.Object.Instantiate(sourcePanel.gameObject,canvas.transform,false);clone.SetActive(true);
                foreach(var transform in canvasObject.GetComponentsInChildren<Transform>(true))transform.gameObject.layer=30;
                foreach(var label in clone.GetComponentsInChildren<TMP_Text>(true))label.ForceMeshUpdate(true);
                Canvas.ForceUpdateCanvases();
                foreach(var label in clone.GetComponentsInChildren<TMP_Text>(true))label.ForceMeshUpdate(true,true);
                Canvas.ForceUpdateCanvases();
                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=target});
                RenderTexture.active=target;texture=new Texture2D(1280,720,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();
                string file="Logs/AudioSettings_"+source.name+".png";File.WriteAllBytes(file,texture.EncodeToPNG());result.checks.Add("Preview: "+file);
            }
            finally
            {
                RenderTexture.active=prior;if(texture!=null)UnityEngine.Object.DestroyImmediate(texture);
                if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}
                if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(temporary,true);if(!opened)EditorSceneManager.CloseScene(source,true);
            }
        }
    }
    static void Validate(Result result)
    {
        var mixer=AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
        if(mixer==null)throw new InvalidOperationException("Missing shared mixer");
        foreach(string path in new[]{"Assets/_Project/Scenes/01_MainMenu.unity","Assets/_Project/Scenes/02_HangLinhMach.unity"})
        {
            Scene scene=SceneManager.GetSceneByPath(path);bool opened=scene.IsValid()&&scene.isLoaded;
            if(!opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try
            {
                var services=Components<AudioSettingsService>(scene).ToArray();
                if(services.Length!=1||Field<AudioMixer>(services[0],"mixer")!=mixer||!services[0].gameObject.activeSelf)throw new InvalidOperationException("SceneAudio configuration invalid: "+path);
                var panel=Components<AudioSettingsPanel>(scene).Single();
                if(panel.gameObject.activeSelf||Field<AudioSettingsService>(panel,"settings")!=services[0])throw new InvalidOperationException("Panel state/service invalid: "+path);
                foreach(string name in new[]{"master","music","sfx"})
                {
                    var slider=Field<Slider>(panel,name+"Slider");
                    if(slider==null||slider.minValue!=0||slider.maxValue!=1||slider.wholeNumbers||slider.onValueChanged.GetPersistentEventCount()!=0)throw new InvalidOperationException("Slider wiring invalid: "+path+"/"+name);
                    if(Field<TMP_Text>(panel,name+"Value")==null)throw new InvalidOperationException("Missing percentage label");
                }
                foreach(var source in Components<AudioSource>(scene))if(source.outputAudioMixerGroup==null||source.outputAudioMixerGroup.audioMixer!=mixer)throw new InvalidOperationException("Unrouted audio source: "+source.name);
                foreach(var button in Components<Button>(scene).Where(b=>b.name=="BTN_Settings"||b.name=="BTN_PauseAudio"||b.name=="BTN_AudioClose"||b.name=="BTN_AudioDefaults"))
                    if(button.onClick.GetPersistentEventCount()!=1||button.onClick.GetPersistentTarget(0)==null)throw new InvalidOperationException("Button callback invalid: "+button.name);
                result.checks.Add(scene.name+": unique active service, shared mixer, inactive wired panel, three sliders, percentage labels and single button callbacks validated.");
            }
            finally{if(!opened)EditorSceneManager.CloseScene(scene,true);}
        }
        var expected=new[]{"MasterVolume","MusicVolume","SFXVolume"};
        var values=(Array)mixer.GetType().GetProperty("exposedParameters",All).GetValue(mixer);
        var names=values.Cast<object>().Select(v=>(string)v.GetType().GetField("name",All).GetValue(v)).ToArray();
        if(!expected.All(names.Contains))throw new InvalidOperationException("Exposed mixer names incorrect");
        result.checks.Add("Mixer exposed names validated; SaveData version remains "+new SaveData().version);
    }
    static void FinishConfiguration(Result result)
    {
        foreach(string path in new[]{"Assets/_Project/Scenes/01_MainMenu.unity","Assets/_Project/Scenes/02_HangLinhMach.unity"})
        {
            Scene scene=SceneManager.GetSceneByPath(path);bool opened=scene.IsValid()&&scene.isLoaded;
            if(!opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try
            {
                var panel=Components<AudioSettingsPanel>(scene).Single();
                Field<Slider>(panel,"masterSlider").SetValueWithoutNotify(1);
                Field<Slider>(panel,"musicSlider").SetValueWithoutNotify(.7f);
                Field<Slider>(panel,"sfxSlider").SetValueWithoutNotify(1);
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            finally{if(!opened)EditorSceneManager.CloseScene(scene,true);}
        }
        // The Input System build hook temporarily inserts its default actions.
        // The clean pre-task PlayerSettings had no preloaded assets.
        var preloads=PlayerSettings.GetPreloadedAssets();
        if(preloads.Length==1&&AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(preloads[0]))=="052faaac586de48259a63d0c4782560b")
            PlayerSettings.SetPreloadedAssets(new UnityEngine.Object[0]);
        AssetDatabase.SaveAssets();Validate(result);
    }
    static void ReportMessages(UnityEditor.Build.Reporting.BuildReport report,Result result)
    {
        if(report==null)throw new InvalidOperationException("No build report");
        foreach(var step in report.steps)foreach(var message in step.messages)
            if(message.type==LogType.Error||message.type==LogType.Exception)result.checks.Add(step.name+": "+message.content);
    }
    static void BuildPlayer(Result result)
    {
        Validate(result);
        Directory.CreateDirectory("Builds/AudioSettingsValidation");
        var originalPreloads=PlayerSettings.GetPreloadedAssets();
        UnityEditor.Build.Reporting.BuildReport report;
        try
        {
            report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
                locationPathName="Builds/AudioSettingsValidation/SonDoong.exe",target=BuildTarget.StandaloneWindows64,
                options=BuildOptions.Development});
        }
        finally{PlayerSettings.SetPreloadedAssets(originalPreloads);AssetDatabase.SaveAssets();}
        ReportMessages(report,result);
        result.checks.Add("Windows build: "+report.summary.result+", errors="+report.summary.totalErrors+", warnings="+report.summary.totalWarnings);
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Windows build failed");
    }
    static void Install(Result result)
    {
        var mixer=Mixer(result);
        string playerPath=AssetDatabase.GUIDToAssetPath("eba7c58098edbfb4d8d8f9141634f044");
        var player=PrefabUtility.LoadPrefabContents(playerPath);
        try{foreach(var motor in player.GetComponentsInChildren<MonoBehaviour>(true).Where(c=>c!=null&&c.GetType().Name=="ThirdPersonController"))Set(motor,"SfxOutput",Group(mixer,"SFX"));PrefabUtility.SaveAsPrefabAsset(player,playerPath);}
        finally{PrefabUtility.UnloadPrefabContents(player);}
        foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/_Project/Prefabs"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(asset.GetComponentsInChildren<AudioSource>(true).Length==0)continue;
            var root=PrefabUtility.LoadPrefabContents(path);
            try{foreach(var source in root.GetComponentsInChildren<AudioSource>(true)){source.outputAudioMixerGroup=Group(mixer,"SFX");PrefabUtility.RecordPrefabInstancePropertyModifications(source);}PrefabUtility.SaveAsPrefabAsset(root,path);}
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        EditScene("Assets/_Project/Scenes/01_MainMenu.unity",mixer,result);
        EditScene("Assets/_Project/Scenes/02_HangLinhMach.unity",mixer,result);
        AssetDatabase.SaveAssets();result.checks.Add("SaveData unchanged; no gameplay save operation invoked by installer.");
    }
}
