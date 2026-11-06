using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Chapter01CompletionArt;

public static class Chapter01ActorsSetup
{
    const string Ghost="Assets/A08 - Ultimate Monsters/Flying/FBX/Ghost.fbx";
    const string Golem="Assets/Kevin Iglesias/Characters/Humanoid Giant/Models/GiantModel01_Flat - Golem_Flat.fbx";
    static T[] All<T>(Scene s)where T:Component=>s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
    public static void Install(Scene scene,Action<string> check)
    {
        Folder(Art+"/Animations");
        foreach(var health in All<EnemyHealth>(scene))
        {
            if(health.transform.Find("VIS_ImportedEnemy"))continue;
            var brain=health.GetComponent<EnemyBrain>();var boss=health.GetComponent<StoneGuardian>();if(!brain&&!boss)continue;
            var warnings=new List<Transform>();
            foreach(var owner in new UnityEngine.Object[]{brain,boss})if(owner)
            {
                var so=new SerializedObject(owner);foreach(string field in new[]{"telegraph","sweepTelegraph","slamTelegraph"})
                {var p=so.FindProperty(field);if(p!=null&&p.objectReferenceValue is GameObject go)warnings.Add(go.transform);}
            }
            foreach(var rr in health.GetComponentsInChildren<Renderer>(true))if(!warnings.Any(t=>rr.transform==t||rr.transform.IsChildOf(t))){rr.enabled=false;if(PrefabUtility.IsPartOfPrefabInstance(rr))PrefabUtility.RecordPrefabInstancePropertyModifications(rr);}
            var model=Model(boss?Golem:Ghost,"VIS_ImportedEnemy",health.transform,health.transform.position+(boss?Vector3.zero:Vector3.up*.08f),boss?3.35f:1.9f);
            foreach(var rr in model.GetComponentsInChildren<Renderer>(true))
            {
                rr.gameObject.layer=LayerMask.NameToLayer("Enemy");
                rr.sharedMaterials=rr.sharedMaterials.Select(m=>Convert(m,boss!=null)).ToArray();
                if(rr is SkinnedMeshRenderer skin)skin.updateWhenOffscreen=true;
                PrefabUtility.RecordPrefabInstancePropertyModifications(rr);PrefabUtility.RecordPrefabInstancePropertyModifications(rr.gameObject);
            }
            var anim=model.GetComponentInChildren<Animator>(true);if(!anim)anim=model.AddComponent<Animator>();
            anim.applyRootMotion=false;anim.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            if(boss){anim.avatar=null;anim.runtimeAnimatorController=BuildGolemController(anim,health.transform,check);}
            else anim.runtimeAnimatorController=BuildGhostController();
            var idle=anim.runtimeAnimatorController.animationClips.FirstOrDefault(c=>c.name.Contains("Idle"));if(idle)idle.SampleAnimation(anim.gameObject,0);
            if(PrefabUtility.IsPartOfPrefabInstance(anim))PrefabUtility.RecordPrefabInstancePropertyModifications(anim);
            var visual=model.AddComponent<EnemyPresentation>();BuilderUtil.Set(visual,"health",health);BuilderUtil.Set(visual,"brain",brain);BuilderUtil.Set(visual,"guardian",boss);BuilderUtil.Set(visual,"animator",anim);BuilderUtil.Set(visual,"surfaces",model.GetComponentsInChildren<Renderer>(true));
            var receiver=health.GetComponent<EnemyLanternReceiver>();if(receiver)BuilderUtil.Set(receiver,"tintRenderer",model.GetComponentInChildren<Renderer>(true));
            check(health.name+" uses "+(boss?"Kevin Iglesias skinned Golem with six project bone clips":"A08 animated Ghost clips")+"; original AI, health, focus hitboxes and root retained.");
        }
        foreach(var t in All<Transform>(scene))if(PrefabUtility.IsPartOfPrefabInstance(t))PrefabUtility.RecordPrefabInstancePropertyModifications(t);
    }
    static Material Convert(Material original,bool golem)
    {
        string name=(golem?"Golem_":"Shadow_")+(original?original.name:"Body");Folder(Art+"/Materials");string path=Art+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
        Color c=original?(original.HasProperty("_BaseColor")?original.GetColor("_BaseColor"):original.HasProperty("_Color")?original.color:Color.white):Color.white;
        if(original&&original.mainTexture)m.SetTexture("_BaseMap",original.mainTexture);
        m.SetColor("_BaseColor",c*(golem?new Color(.73f,.79f,.74f):new Color(.24f,.27f,.37f)));m.SetFloat("_Smoothness",.12f);m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",Color.black);EditorUtility.SetDirty(m);return m;
    }
    static AnimatorController Controller(string name,Dictionary<string,AnimationClip> clips)
    {
        string path=Art+"/Animations/"+name+".controller";var c=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if(c)return c;c=AnimatorController.CreateAnimatorControllerAtPath(path);var sm=c.layers[0].stateMachine;
        foreach(var p in clips){var state=sm.AddState(p.Key);state.motion=p.Value;if(p.Key=="Idle")sm.defaultState=state;}
        EditorUtility.SetDirty(c);return c;
    }
    static AnimatorController BuildGhostController()
    {
        var clips=AssetDatabase.LoadAllAssetsAtPath(Ghost).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
        AnimationClip Clip(string suffix)
        {
            string path=Art+"/Animations/Shadow_"+suffix+".anim";var copy=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);if(copy)return copy;
            copy=UnityEngine.Object.Instantiate(clips.Single(c=>c.name.EndsWith("|"+suffix)));copy.name="Shadow_"+suffix;copy.events=Array.Empty<AnimationEvent>();var settings=AnimationUtility.GetAnimationClipSettings(copy);settings.loopTime=suffix=="Flying_Idle"||suffix=="Fast_Flying";AnimationUtility.SetAnimationClipSettings(copy,settings);AssetDatabase.CreateAsset(copy,path);return copy;
        }
        return Controller("AC_HacAnh",new Dictionary<string,AnimationClip>{{"Idle",Clip("Flying_Idle")},{"Walk",Clip("Fast_Flying")},{"Attack",Clip("Punch")},{"Hurt",Clip("HitReact")},{"Death",Clip("Death")}});
    }
    static AnimatorController BuildGolemController(Animator animator,Transform actor,Action<string> check)
    {
        string existing=Art+"/Animations/AC_Guardian.controller";var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(existing);if(controller)return controller;
        var names=new[]{"B-hips","B-spine","B-head","B-upperArm.L","B-upperArm.R","B-forearm.L","B-forearm.R","B-thigh.L","B-thigh.R","B-shin.L","B-shin.R"};
        var transforms=animator.GetComponentsInChildren<Transform>(true);var bones=names.ToDictionary(n=>n,n=>transforms.Single(t=>t.name==n));
        var original=bones.ToDictionary(p=>p.Key,p=>p.Value.localRotation);var positions=bones.ToDictionary(p=>p.Key,p=>p.Value.localPosition);
        void Reset(){foreach(var b in bones){b.Value.localRotation=original[b.Key];b.Value.localPosition=positions[b.Key];}}
        void Aim(string upper,string lower,Vector3 upperDir,Vector3 lowerDir)
        {
            var a=bones[upper];var b=bones[lower];var hand=b.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name.StartsWith("B-hand"));
            a.rotation=Quaternion.FromToRotation(b.position-a.position,actor.TransformDirection(upperDir))*a.rotation;
            if(hand)b.rotation=Quaternion.FromToRotation(hand.position-b.position,actor.TransformDirection(lowerDir))*b.rotation;
        }
        Reset();Aim("B-upperArm.L","B-forearm.L",new Vector3(-.22f,-1,0),new Vector3(-.05f,-1,.15f));Aim("B-upperArm.R","B-forearm.R",new Vector3(.22f,-1,0),new Vector3(.05f,-1,.15f));
        var ready=bones.ToDictionary(p=>p.Key,p=>p.Value.localRotation);var clips=new Dictionary<string,AnimationClip>();
        foreach(string state in new[]{"Idle","Walk","Sweep","Slam","Hurt","Death"})
        {
            float duration=state=="Sweep"?2.2f:state=="Slam"?2.95f:state=="Hurt"?.35f:state=="Death"?1f:state=="Walk"?1.05f:2f;
            var curves=bones.Keys.ToDictionary(n=>n,_=>new[]{new AnimationCurve(),new AnimationCurve(),new AnimationCurve(),new AnimationCurve()});var hp=new[]{new AnimationCurve(),new AnimationCurve(),new AnimationCurve()};
            int samples=Mathf.CeilToInt(duration*30);
            for(int sample=0;sample<=samples;sample++)
            {
                float t=duration*sample/samples;foreach(var b in bones){b.Value.localRotation=ready[b.Key];b.Value.localPosition=positions[b.Key];}
                float phase=t/duration;
                if(state=="Idle"){bones["B-spine"].localRotation*=Quaternion.Euler(Mathf.Sin(phase*Mathf.PI*2)*1.5f,0,0);}
                if(state=="Walk")
                {
                    float gait=Mathf.Sin(phase*Mathf.PI*2);bones["B-thigh.L"].localRotation*=Quaternion.Euler(gait*25,0,0);bones["B-thigh.R"].localRotation*=Quaternion.Euler(-gait*25,0,0);
                    bones["B-shin.L"].localRotation*=Quaternion.Euler(Mathf.Max(0,-gait)*28,0,0);bones["B-shin.R"].localRotation*=Quaternion.Euler(Mathf.Max(0,gait)*28,0,0);
                    bones["B-upperArm.L"].localRotation*=Quaternion.Euler(-gait*16,0,0);bones["B-upperArm.R"].localRotation*=Quaternion.Euler(gait*16,0,0);
                }
                if(state=="Sweep")
                {
                    float wind=Mathf.Clamp01(t/.8f),strike=Mathf.Clamp01((t-.8f)/.2f),recover=Mathf.Clamp01((t-1)/1.2f);
                    Vector3 target=Vector3.Lerp(new Vector3(.22f,-1,0),new Vector3(.9f,.15f,-.5f),wind);target=Vector3.Lerp(target,new Vector3(-.7f,-.1f,.8f),strike);target=Vector3.Lerp(target,new Vector3(.22f,-1,0),recover);
                    Aim("B-upperArm.R","B-forearm.R",target,Vector3.Lerp(new Vector3(.3f,-.15f,.8f),new Vector3(-.5f,-.2f,.8f),strike));bones["B-spine"].localRotation*=Quaternion.Euler(0,Mathf.Lerp(-25*wind,25,strike)*(1-recover),0);
                }
                if(state=="Slam")
                {
                    float raise=Mathf.Clamp01(t/1.2f),strike=Mathf.Clamp01((t-1.2f)/.22f),recover=Mathf.Clamp01((t-1.55f)/1.4f);
                    Vector3 arm=Vector3.Lerp(new Vector3(.22f,-1,0),new Vector3(.3f,1,-.15f),raise);arm=Vector3.Lerp(arm,new Vector3(.25f,-.5f,.8f),strike);arm=Vector3.Lerp(arm,new Vector3(.22f,-1,0),recover);
                    Aim("B-upperArm.L","B-forearm.L",new Vector3(-arm.x,arm.y,arm.z),new Vector3(-.1f,arm.y,arm.z+.1f));Aim("B-upperArm.R","B-forearm.R",arm,new Vector3(.1f,arm.y,arm.z+.1f));bones["B-spine"].localRotation*=Quaternion.Euler(42*strike*(1-recover),0,0);bones["B-hips"].localPosition-=Vector3.up*(.15f*strike*(1-recover));
                }
                if(state=="Hurt")bones["B-spine"].localRotation*=Quaternion.Euler(-16*Mathf.Sin(phase*Mathf.PI),0,0);
                if(state=="Death"){bones["B-hips"].localPosition-=Vector3.up*(phase*.75f);bones["B-spine"].localRotation*=Quaternion.Euler(phase*70,0,phase*12);}
                foreach(var b in bones){var q=b.Value.localRotation;curves[b.Key][0].AddKey(t,q.x);curves[b.Key][1].AddKey(t,q.y);curves[b.Key][2].AddKey(t,q.z);curves[b.Key][3].AddKey(t,q.w);}
                var pos=bones["B-hips"].localPosition;hp[0].AddKey(t,pos.x);hp[1].AddKey(t,pos.y);hp[2].AddKey(t,pos.z);
            }
            var clip=new AnimationClip{name="Guardian_"+state,frameRate=30};foreach(var b in bones)for(int channel=0;channel<4;channel++)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(AnimationUtility.CalculateTransformPath(b.Value,animator.transform),typeof(Transform),"m_LocalRotation."+"xyzw"[channel]),curves[b.Key][channel]);
            for(int channel=0;channel<3;channel++)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(AnimationUtility.CalculateTransformPath(bones["B-hips"],animator.transform),typeof(Transform),"m_LocalPosition."+"xyz"[channel]),hp[channel]);
            clip.EnsureQuaternionContinuity();var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=state=="Idle"||state=="Walk";AnimationUtility.SetAnimationClipSettings(clip,settings);AssetDatabase.CreateAsset(clip,Art+"/Animations/Guardian_"+state+".anim");clips[state]=clip;
        }
        Reset();check("Authored six bone-rig clips for imported Golem: idle/walk/sweep/slam/hurt/death; root motion disabled.");return Controller("AC_Guardian",clips);
    }
}
