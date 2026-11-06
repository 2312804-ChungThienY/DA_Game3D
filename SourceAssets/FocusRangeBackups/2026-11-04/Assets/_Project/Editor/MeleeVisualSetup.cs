using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class MeleeVisualSetup
{
    const string RequestPath="Library/SonDoongMeleeVisual.request.json";
    const string PrefabPath="Assets/_Project/Prefabs/Player/PF_An_Prototype.prefab";
    const string KnifePath="Assets/A16 - Survival Pack/FBX/Knife.fbx";
    const string ControllerPath="Assets/_Project/Animations/AC_An.controller";
    [Serializable] class Request { public string action; }
    [Serializable] public class Report
    {
        public string action;
        public List<string> checks=new List<string>();
        public List<string> failures=new List<string>();
        public List<string> images=new List<string>();
    }
    static double next;
    static MeleeVisualSetup(){EditorApplication.update+=Poll;}
    static void Poll()
    {
        if(EditorApplication.timeSinceStartup<next||EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(RequestPath))return;
        next=EditorApplication.timeSinceStartup+1;var report=new Report();
        try
        {
            report.action=JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath)).action;
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before editing animation.");
            if(report.action=="inspect")Inspect(report);
            else if(report.action=="install")Install(report);
            else if(report.action=="capture")Capture(report);
            else if(report.action=="validate")Validate(report);
            else throw new InvalidOperationException("Unknown melee action");
        }
        catch(Exception e){report.failures.Add(e.ToString());}
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/MeleeVisual-"+report.action+".json",JsonUtility.ToJson(report,true));File.Delete(RequestPath);
    }
    static void Inspect(Report report)
    {
        var root=PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            var animator=root.GetComponent<Animator>();animator.Rebind();animator.Update(0);
            report.checks.Add("Avatar valid="+animator.avatar.isValid+" human="+animator.avatar.isHuman);
            var human=new HumanPose();using(var hp=new HumanPoseHandler(animator.avatar,root.transform))hp.GetHumanPose(ref human);
            report.checks.Add("humanScale="+animator.humanScale+", bodyPosition="+human.bodyPosition);
            report.checks.Add("Neutral out-of-range muscles: "+string.Join(", ",HumanTrait.MuscleName.Select((name,i)=>new{name,value=human.muscles[i]}).Where(p=>Mathf.Abs(p.value)>.95f).Select(p=>p.name+"="+p.value)));
            foreach(var id in new[]{HumanBodyBones.Hips,HumanBodyBones.Chest,HumanBodyBones.RightUpperArm,HumanBodyBones.RightLowerArm,HumanBodyBones.RightHand,HumanBodyBones.RightMiddleProximal})
            {var bone=animator.GetBoneTransform(id);report.checks.Add(id+": "+bone.name+" position="+bone.position+" right="+bone.right+" up="+bone.up+" forward="+bone.forward);}
            foreach(var mesh in AssetDatabase.LoadAllAssetsAtPath(KnifePath).OfType<Mesh>())report.checks.Add("Knife mesh "+mesh.name+" bounds="+mesh.bounds);
            using(var handler=new HumanPoseHandler(animator.avatar,root.transform))
            {
                var before=new HumanPose();handler.GetHumanPose(ref before);
                var hand=animator.GetBoneTransform(HumanBodyBones.RightUpperArm);hand.localRotation*=Quaternion.Euler(0,0,35);
                var after=new HumanPose();handler.GetHumanPose(ref after);
                float difference=before.muscles.Zip(after.muscles,(a,b)=>Mathf.Abs(a-b)).Max();
                report.checks.Add("Transform-to-human muscle readback delta="+difference);
                if(difference<.01f)throw new InvalidOperationException("Humanoid muscle readback did not detect transformed arm.");
            }
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    static void Validate(Report report)
    {
        var root=PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            var animator=root.GetComponent<Animator>();animator.Rebind();animator.Update(0);
            float restFoot=Bone(animator,HumanBodyBones.LeftFoot).position.y;
            report.checks.Add("Native locomotion left-foot height="+restFoot);
            foreach(string name in new[]{"AN_Slash1","AN_Slash2","AN_Lunge"})
            {
                var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/_Project/Animations/"+name+".anim");
                if(Mathf.Abs(clip.length-.65f)>.001f)throw new InvalidOperationException(name+" duration mismatch");
                var bindings=AnimationUtility.GetCurveBindings(clip);
                if(bindings.Count(b=>b.propertyName.StartsWith("RootT.")||b.propertyName.StartsWith("RootQ."))!=7)throw new InvalidOperationException(name+" lacks body pose");
                Vector3 initial=Vector3.zero;float motion=0;
                foreach(float time in new[]{0f,.11f,.21f,.31f,.60f,.65f})
                {
                    animator.Rebind();clip.SampleAnimation(root,time);
                    var hand=Bone(animator,HumanBodyBones.RightHand);var grip=hand.Find("KnifeGrip");float foot=Bone(animator,HumanBodyBones.LeftFoot).position.y;
                    if(time==0)initial=hand.position;motion=Mathf.Max(motion,Vector3.Distance(initial,hand.position));
                    report.checks.Add(name+" @"+time+": foot="+foot+", baseline delta="+(foot-restFoot)+", grip distance="+Vector3.Distance(hand.position,grip.position));
                    if(Mathf.Abs(foot-restFoot)>.08f)throw new InvalidOperationException(name+" moved feet vertically more than .08m");
                    if(Vector3.Distance(hand.position,grip.position)>.08f)throw new InvalidOperationException("Knife detached from hand");
                }
                if(motion<.2f)throw new InvalidOperationException(name+" wrist is not animated");
                report.checks.Add(name+" validated body/foot/palm/wrist travel="+motion);
            }
            var ac=AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);var melee=ac.layers.Single(l=>l.name=="Melee");
            if(melee.stateMachine.states.Count(s=>s.state.motion is AnimationClip)!=3)throw new InvalidOperationException("Melee must have 3 distinct motions");
            if(root.GetComponentInChildren<TrailRenderer>(true)==null)throw new InvalidOperationException("Blade trail missing");
            report.checks.Add("Three distinct melee motions, hand-held knife and blade trail are configured; root motion off="+!animator.applyRootMotion);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    sealed class Bind
    {
        public Transform transform;public Vector3 position;public Quaternion rotation;
        public Bind(Transform t){transform=t;position=t.localPosition;rotation=t.localRotation;}
        public void Restore(){transform.localPosition=position;transform.localRotation=rotation;}
    }
    static Transform Bone(Animator a,HumanBodyBones id)=>a.GetBoneTransform(id);
    static void Solve(Transform upper,Transform lower,Transform tip,Vector3 target,Vector3 hint)
    {
        Vector3 origin=upper.position;float a=Vector3.Distance(origin,lower.position),b=Vector3.Distance(lower.position,tip.position);
        Vector3 direction=(target-origin).normalized;float d=Mathf.Clamp(Vector3.Distance(origin,target),Mathf.Abs(a-b)+.001f,a+b-.002f);
        Vector3 bend=Vector3.ProjectOnPlane(hint-origin,direction).normalized;
        float along=(a*a+d*d-b*b)/(2*d);float height=Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
        Vector3 elbow=origin+direction*along+bend*height;
        upper.rotation=Quaternion.FromToRotation(lower.position-origin,elbow-origin)*upper.rotation;
        lower.rotation=Quaternion.FromToRotation(tip.position-lower.position,target-lower.position)*lower.rotation;
    }
    struct KeyPose
    {
        public float time,twist,lean;public Vector3 wrist,blade;
        public KeyPose(float t,Vector3 w,Vector3 b,float turn,float l=0){time=t;wrist=w;blade=b;twist=turn;lean=l;}
    }
    static KeyPose PoseAt(KeyPose[] keys,float time)
    {
        int i=0;while(i<keys.Length-2&&keys[i+1].time<time)i++;
        float t=Mathf.SmoothStep(0,1,Mathf.InverseLerp(keys[i].time,keys[i+1].time,time));
        return new KeyPose(time,Vector3.Lerp(keys[i].wrist,keys[i+1].wrist,t),Vector3.Slerp(keys[i].blade,keys[i+1].blade,t),Mathf.Lerp(keys[i].twist,keys[i+1].twist,t),Mathf.Lerp(keys[i].lean,keys[i+1].lean,t));
    }
    static AnimationClip Bake(GameObject root,string name,int kind,Report report)
    {
        var animator=root.GetComponent<Animator>();animator.Rebind();animator.Update(0);
        Motion reference=((AnimatorController)animator.runtimeAnimatorController).layers[0].stateMachine.defaultState.motion;
        while(reference is BlendTree tree)reference=tree.children.OrderBy(child=>child.threshold).First().motion;
        var idle=reference as AnimationClip;
        if(idle==null)throw new InvalidOperationException("Valid native idle reference required for foot/body pose.");
        var binds=root.GetComponentsInChildren<Transform>(true).Select(t=>new Bind(t)).ToArray();
        var hand=Bone(animator,HumanBodyBones.RightHand);var finger=Bone(animator,HumanBodyBones.RightMiddleProximal);
        Vector3 rest=root.transform.InverseTransformPoint(hand.position);
        Vector3 restBlade=root.transform.InverseTransformDirection((finger.position-hand.position).normalized);
        Quaternion gripLocal=Quaternion.Inverse(hand.rotation)*Quaternion.LookRotation(root.transform.TransformDirection(restBlade),root.transform.forward);
        KeyPose[] keys=kind==0?new[]{
            new KeyPose(0,rest,restBlade,0),new KeyPose(.11f,new Vector3(.58f,1.52f,.08f),new Vector3(.15f,.05f,1),-22,2),
            new KeyPose(.21f,new Vector3(.27f,1.41f,.43f),new Vector3(-.5f,-.15f,1),8,8),
            new KeyPose(.31f,new Vector3(-.10f,1.32f,.32f),new Vector3(-.85f,-.15f,.55f),28,5),new KeyPose(.65f,rest,restBlade,0)}:
            kind==1?new[]{
            new KeyPose(0,rest,restBlade,0),new KeyPose(.11f,new Vector3(-.08f,1.42f,.25f),new Vector3(.5f,.05f,.8f),24,3),
            new KeyPose(.21f,new Vector3(.28f,1.44f,.43f),new Vector3(.65f,-.05f,1),-8,7),
            new KeyPose(.31f,new Vector3(.59f,1.46f,.12f),new Vector3(.9f,.05f,.35f),-25,3),new KeyPose(.65f,rest,restBlade,0)}:
            new[]{new KeyPose(0,rest,restBlade,0),new KeyPose(.11f,new Vector3(.43f,1.40f,.04f),Vector3.forward,-15,2),
            new KeyPose(.20f,new Vector3(.21f,1.41f,.48f),Vector3.forward,5,10),new KeyPose(.30f,new Vector3(.21f,1.39f,.50f),Vector3.forward,8,8),new KeyPose(.65f,rest,restBlade,0)};
        var curves=HumanTrait.MuscleName.Select(_=>new AnimationCurve()).ToArray();
        using(var handler=new HumanPoseHandler(animator.avatar,root.transform))
        {
            var pose=new HumanPose();handler.GetHumanPose(ref pose);float[] neutral=(float[])pose.muscles.Clone();
            for(int frame=0;frame<=39;frame++)
            {
                float time=frame/60f;foreach(var bind in binds)bind.Restore();var key=PoseAt(keys,time);
                var spine=Bone(animator,HumanBodyBones.Spine);var chest=Bone(animator,HumanBodyBones.Chest);
                spine.rotation=Quaternion.AngleAxis(key.lean,root.transform.right)*Quaternion.AngleAxis(key.twist*.4f,root.transform.up)*spine.rotation;
                chest.rotation=Quaternion.AngleAxis(key.twist*.6f,root.transform.up)*chest.rotation;
                Solve(Bone(animator,HumanBodyBones.RightUpperArm),Bone(animator,HumanBodyBones.RightLowerArm),hand,
                    root.transform.TransformPoint(key.wrist),root.transform.TransformPoint(new Vector3(.95f,1.2f,.05f)));
                hand.rotation=Quaternion.LookRotation(root.transform.TransformDirection(key.blade.normalized),root.transform.up)*Quaternion.Inverse(gripLocal);
                // Blend in from and back to the sampled locomotion stance without snapping wrists.
                float weight=Mathf.Min(Mathf.Clamp01(time/.075f),Mathf.Clamp01((.65f-time)/.12f));
                handler.GetHumanPose(ref pose);
                for(int m=0;m<curves.Length;m++)
                {
                    float value=Mathf.Lerp(neutral[m],pose.muscles[m],weight);
                    if(HumanTrait.MuscleName[m].StartsWith("Right ")&&HumanTrait.MuscleName[m].Contains("Stretched"))value=Mathf.Lerp(value,-.55f,weight);
                    curves[m].AddKey(time,value);
                }
            }
        }
        foreach(var bind in binds)bind.Restore();
        string path="Assets/_Project/Animations/"+name+".anim";var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if(clip==null){clip=new AnimationClip{name=name};AssetDatabase.CreateAsset(clip,path);}else clip.ClearCurves();
        clip.frameRate=60;
        // Native humanoid clips include body and foot-goal channels that are not
        // interchangeable with HumanPose.bodyPosition. Preserve that proven stance.
        foreach(var binding in AnimationUtility.GetCurveBindings(idle))
        {
            var curve=AnimationUtility.GetEditorCurve(idle,binding);
            AnimationUtility.SetEditorCurve(clip,binding,AnimationCurve.Constant(0,.65f,curve.Evaluate(0)));
        }
        for(int m=0;m<curves.Length;m++)
        {
            string muscle=HumanTrait.MuscleName[m];
            if(muscle.StartsWith("Right Shoulder")||muscle.StartsWith("Right Arm")||muscle.StartsWith("Right Forearm")||muscle.StartsWith("Right Hand")||
               muscle.StartsWith("Spine ")||muscle.StartsWith("Chest ")||muscle.StartsWith("UpperChest "))
                AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Animator),muscle),curves[m]);
        }
        var settings=AnimationUtility.GetAnimationClipSettings(idle);settings.startTime=0;settings.stopTime=.65f;settings.loopTime=false;AnimationUtility.SetAnimationClipSettings(clip,settings);EditorUtility.SetDirty(clip);
        report.checks.Add("Baked "+name+": 40 humanoid arm/torso samples / .65s; native "+idle.name+" body and feet retained, actor root motion off.");return clip;
    }
    static void Install(Report report)
    {
        for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
            if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save open scenes first.");
        var root=PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            var clips=new[]{Bake(root,"AN_Slash1",0,report),Bake(root,"AN_Slash2",1,report),Bake(root,"AN_Lunge",2,report)};
            var ac=AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            var layers=ac.layers.ToList();var existing=layers.FirstOrDefault(l=>l.name=="Melee");
            var machine=existing!=null?existing.stateMachine:new AnimatorStateMachine{name="Melee",hideFlags=HideFlags.HideInHierarchy};
            if(existing==null)AssetDatabase.AddObjectToAsset(machine,ac);
            foreach(var child in machine.states)machine.RemoveState(child.state);
            foreach(var transition in machine.anyStateTransitions)machine.RemoveAnyStateTransition(transition);
            var empty=machine.AddState("Empty");machine.defaultState=empty;
            for(int i=0;i<clips.Length;i++)
            {
                var state=machine.AddState(i==0?"Slash1":i==1?"Slash2":"Lunge");state.motion=clips[i];state.writeDefaultValues=false;
                var exit=state.AddTransition(empty);exit.hasExitTime=true;exit.exitTime=1;exit.hasFixedDuration=true;exit.duration=.08f;
            }
            layers.RemoveAll(l=>l.name=="Melee");int index=layers.FindIndex(l=>l.name=="FullBody");
            layers.Insert(index<0?layers.Count:index,new AnimatorControllerLayer{name="Melee",stateMachine=machine,defaultWeight=0,blendingMode=AnimatorLayerBlendingMode.Override});
            ac.layers=layers.ToArray();EditorUtility.SetDirty(ac);
            AddGripLayer(ac);
            // Stop the old floating cube; the blade trail is handled by PlayerMeleeVisual.
            var combat=new SerializedObject(root.GetComponent<CombatController>());combat.FindProperty("slashVisual").objectReferenceValue=null;combat.ApplyModifiedPropertiesWithoutUndo();
            var old=root.transform.Find("SlashFX");if(old!=null)old.gameObject.SetActive(false);
            EquipKnife(root,report);
            PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);AssetDatabase.SaveAssets();
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    [MenuItem("Tools/Son Doong/Rebuild Melee Visuals")]
    public static void Rebuild()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
        var report=new Report{action="install"};Install(report);
        File.WriteAllText("Logs/MeleeVisual-install.json",JsonUtility.ToJson(report,true));
        Debug.Log("Rebuilt melee clips and hand-held knife in place; controller/prefab GUIDs retained.");
    }
    static void AddGripLayer(AnimatorController ac)
    {
        const string clipPath="Assets/_Project/Animations/AN_KnifeGrip.anim";
        var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if(clip==null){clip=new AnimationClip{name="AN_KnifeGrip"};AssetDatabase.CreateAsset(clip,clipPath);}else clip.ClearCurves();
        foreach(string name in HumanTrait.MuscleName.Where(n=>n.StartsWith("Right ")&&n.Contains("Stretched")))
        {
            float curl=name.Contains("Thumb")?-.25f:-.55f;
            AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Animator),name),AnimationCurve.Constant(0,1,curl));
        }
        var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,settings);
        const string maskPath="Assets/_Project/Animations/AM_An_KnifeGrip.mask";
        var mask=AssetDatabase.LoadAssetAtPath<AvatarMask>(maskPath);if(mask==null){mask=new AvatarMask();AssetDatabase.CreateAsset(mask,maskPath);}
        for(int i=0;i<(int)AvatarMaskBodyPart.LastBodyPart;i++)mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i,i==(int)AvatarMaskBodyPart.RightFingers);
        var layers=ac.layers.ToList();var layer=layers.FirstOrDefault(l=>l.name=="KnifeGrip");
        if(layer==null)
        {
            var machine=new AnimatorStateMachine{name="KnifeGrip",hideFlags=HideFlags.HideInHierarchy};AssetDatabase.AddObjectToAsset(machine,ac);
            layer=new AnimatorControllerLayer{name="KnifeGrip",stateMachine=machine,defaultWeight=1,avatarMask=mask,blendingMode=AnimatorLayerBlendingMode.Override};
            layers.Add(layer);
        }
        var state=layer.stateMachine.states.FirstOrDefault().state;if(state==null)state=layer.stateMachine.AddState("Hold");
        state.motion=clip;state.writeDefaultValues=false;layer.stateMachine.defaultState=state;layer.avatarMask=mask;layer.defaultWeight=1;
        ac.layers=layers.ToArray();EditorUtility.SetDirty(mask);EditorUtility.SetDirty(clip);EditorUtility.SetDirty(ac);
    }
    static void EquipKnife(GameObject root,Report report)
    {
        var animator=root.GetComponent<Animator>();animator.Rebind();animator.Update(0);
        var hand=Bone(animator,HumanBodyBones.RightHand);var finger=Bone(animator,HumanBodyBones.RightMiddleProximal);
        Transform old=hand.Find("KnifeGrip");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var grip=new GameObject("KnifeGrip").transform;grip.SetParent(hand,false);
        Vector3 forward=(finger.position-hand.position).normalized;
        grip.position=hand.position+forward*.045f;
        grip.rotation=Quaternion.LookRotation(forward,root.transform.forward);
        var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(KnifePath),root.scene);
        model.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);model.transform.localScale=Vector3.one;
        var renderers=model.GetComponentsInChildren<Renderer>();Bounds bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
        Vector3 size=bounds.size;int axis=size.y>size.x?1:0;if(size.z>size[axis])axis=2;
        if(size[axis]<.000001f)throw new InvalidOperationException("Knife bounds are empty.");
        Vector3 major=axis==0?Vector3.right:axis==1?Vector3.up:Vector3.forward;
        var normalized=new GameObject("KnifeModel").transform;normalized.SetParent(grip,false);
        Quaternion rotation=Quaternion.FromToRotation(major,Vector3.forward);float scale=.34f/size[axis];
        model.transform.SetParent(normalized,false);model.transform.localScale=Vector3.one*scale;model.transform.localRotation=rotation;
        model.transform.localPosition=Vector3.forward*.09f-rotation*(bounds.center*scale);
        foreach(var t in model.GetComponentsInChildren<Transform>(true))t.gameObject.layer=2;
        foreach(var collider in model.GetComponentsInChildren<Collider>())UnityEngine.Object.DestroyImmediate(collider);
        var tip=new GameObject("BladeTrail").transform;tip.SetParent(grip,false);tip.localPosition=Vector3.forward*.25f;
        var trail=tip.gameObject.AddComponent<TrailRenderer>();trail.time=.10f;trail.minVertexDistance=.012f;trail.widthMultiplier=.035f;trail.emitting=false;trail.autodestruct=false;
        trail.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;trail.receiveShadows=false;
        string materialPath="Assets/_Project/Materials/M_KnifeTrail.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));AssetDatabase.CreateAsset(material,materialPath);}
        material.SetColor("_BaseColor",new Color(1,.62f,.16f,.65f));material.SetFloat("_Surface",1);material.SetFloat("_Blend",0);material.SetFloat("_ZWrite",0);material.SetFloat("_SrcBlend",5);material.SetFloat("_DstBlend",10);material.renderQueue=3000;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.SetFloat("_Cull",0);EditorUtility.SetDirty(material);
        var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(new Color(1,.7f,.25f),0),new GradientColorKey(new Color(1,.45f,.08f),1)},new[]{new GradientAlphaKey(.75f,0),new GradientAlphaKey(0,1)});trail.colorGradient=gradient;trail.sharedMaterial=material;
        var visual=root.GetComponent<PlayerMeleeVisual>();if(visual==null)visual=root.AddComponent<PlayerMeleeVisual>();
        var config=new SerializedObject(visual);config.FindProperty("bladeTrail").objectReferenceValue=trail;config.FindProperty("knifeGrip").objectReferenceValue=grip;config.ApplyModifiedPropertiesWithoutUndo();
        report.checks.Add("Equipped A16 Knife on RightHand; normalized length=.34m, imported bounds="+bounds.size.ToString("F6")+", major axis="+axis);
    }
    static void Capture(Report report)
    {
        var scene=EditorSceneManager.NewPreviewScene();GameObject root=null;
        try
        {
            root=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath),scene);
            root.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);var animator=root.GetComponent<Animator>();animator.Rebind();animator.Update(0);
            var cameraGo=new GameObject("MeleePoseCamera");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraGo,scene);var cam=cameraGo.AddComponent<Camera>();cam.scene=scene;
            cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.12f,.16f,.20f);cam.fieldOfView=38;
            cameraGo.transform.position=new Vector3(2.8f,1.9f,3.3f);cameraGo.transform.LookAt(new Vector3(0,1.05f,.1f));
            var lightGo=new GameObject("MeleePoseKey");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(lightGo,scene);var light=lightGo.AddComponent<Light>();light.type=LightType.Directional;light.intensity=2.2f;lightGo.transform.rotation=Quaternion.Euler(40,-30,0);
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(floor,scene);floor.transform.position=Vector3.down*.05f;floor.transform.localScale=new Vector3(6,.1f,6);
            var floorMat=new Material(Shader.Find("Universal Render Pipeline/Lit"));floorMat.color=new Color(.22f,.27f,.28f);floor.GetComponent<Renderer>().sharedMaterial=floorMat;
            foreach(string name in new[]{"AN_Slash1","AN_Slash2","AN_Lunge"})
            {
                var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/_Project/Animations/"+name+".anim");
                var sheet=new Texture2D(400*5,500,TextureFormat.RGB24,false);int frame=0;
                foreach(float time in new[]{0f,.11f,.21f,.31f,.60f})
                {
                    animator.Rebind();clip.SampleAnimation(root,time);
                    var hand=Bone(animator,HumanBodyBones.RightHand);var grip=hand.Find("KnifeGrip");
                    report.checks.Add(name+" @"+time+" hips="+Bone(animator,HumanBodyBones.Hips).position+", wrist="+hand.position+", grip="+grip.position+", gripDistance="+Vector3.Distance(hand.position,grip.position));
                    var baked=new List<GameObject>();var bakedMeshes=new List<Mesh>();
                    var skins=root.GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach(var skin in skins)
                    {
                        var mesh=new Mesh();skin.BakeMesh(mesh);bakedMeshes.Add(mesh);
                        var frozen=new GameObject("PoseSkin");frozen.transform.SetParent(skin.transform,false);
                        frozen.AddComponent<MeshFilter>().sharedMesh=mesh;frozen.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;
                        skin.enabled=false;baked.Add(frozen);
                    }
                    var rt=new RenderTexture(400,500,24);rt.Create();var before=RenderTexture.active;
                    try
                    {
                        UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(cam,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=rt});
                        RenderTexture.active=rt;var texture=new Texture2D(400,500,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,400,500),0,0);texture.Apply();sheet.SetPixels(frame*400,0,400,500,texture.GetPixels());UnityEngine.Object.DestroyImmediate(texture);
                    }
                    finally
                    {
                        RenderTexture.active=before;rt.Release();UnityEngine.Object.DestroyImmediate(rt);
                        foreach(var frozen in baked)UnityEngine.Object.DestroyImmediate(frozen);
                        foreach(var mesh in bakedMeshes)UnityEngine.Object.DestroyImmediate(mesh);
                        foreach(var skin in skins)skin.enabled=true;
                    }
                    frame++;
                }
                sheet.Apply();Directory.CreateDirectory("docs/images");string path="docs/images/melee-"+name+"-2026-11-04.png";File.WriteAllBytes(path,sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);report.images.Add(path);
            }
            UnityEngine.Object.DestroyImmediate(floorMat);
        }
        finally{EditorSceneManager.ClosePreviewScene(scene);}
    }
}
