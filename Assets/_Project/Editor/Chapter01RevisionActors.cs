using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.SceneManagement;
using static Chapter01RevisionSetup;

public static class Chapter01RevisionActors
{
    public static void Install(Scene scene,Action<string> check)
    {
        Chapter01RevisionSetup.Folder(Art+"/Animations");
        foreach(var health in All<EnemyHealth>(scene))
        {
            var visual=health.transform.Find("VIS_ImportedEnemy");if(!visual)continue;
            float old=Quaternion.Angle(health.transform.rotation,visual.rotation);visual.localRotation=Quaternion.identity;
            var animator=visual.GetComponentInChildren<Animator>();bool boss=health.GetComponent<StoneGuardian>();
            var receiver=health.GetComponent<EnemyLanternReceiver>();BuilderUtil.Set(receiver,"enableStun",true);BuilderUtil.Set(receiver,"stunDuration",boss?2f:4f);BuilderUtil.Set(receiver,"stunRecoveryImmunity",3f);
            if(boss)GuardianClips(animator,health.transform,check);else GhostStun(animator,health.transform,check);
            var idle=animator.runtimeAnimatorController.animationClips.FirstOrDefault(c=>c.name.Contains("Idle"));if(idle)idle.SampleAnimation(animator.gameObject,0);
            if(!health.GetComponent<EnemyStunIndicator>())
            {
                var root=Chapter01CompletionArt.Group("VIS_StunStars",health.transform);var material=Chapter01CompletionArt.Material("Stun_Stars",new Color(1,.76f,.22f),.8f);var points=new List<Vector3>{Vector3.zero};var tri=new List<int>();for(int i=0;i<10;i++){float a=i*Mathf.PI/5;float r=i%2==0?.13f:.055f;points.Add(new Vector3(Mathf.Sin(a)*r,Mathf.Cos(a)*r,0));}for(int i=0;i<10;i++)tri.AddRange(new[]{0,1+i,1+(i+1)%10});
                var mesh=Chapter01CompletionArt.Mesh("Stun_Star",points,tri);var stars=new List<Transform>();
                for(int i=0;i<3;i++){var star=Chapter01CompletionArt.Surface("Star_"+i,root,mesh,material);star.layer=2;stars.Add(star.transform);}
                var indicator=health.gameObject.AddComponent<EnemyStunIndicator>();BuilderUtil.Set(indicator,"receiver",receiver);BuilderUtil.Set(indicator,"indicator",root);BuilderUtil.Set(indicator,"stars",stars.ToArray());BuilderUtil.Set(indicator,"height",boss?3.7f:2.25f);root.gameObject.SetActive(false);
            }
            if(PrefabUtility.IsPartOfPrefabInstance(visual))PrefabUtility.RecordPrefabInstancePropertyModifications(visual);
            check(health.name+": removed visual yaw offset "+old.ToString("F1")+" degrees, aligned local +Z; stun "+(boss?2:4)+"s with 3s immunity, bowed rig and orbiting stars.");
        }
    }
    static AnimationClip SaveClip(string name,AnimationClip clip)
    {
        string path=Art+"/Animations/"+name+".anim";var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);clip.name=name;
        if(existing){EditorUtility.CopySerialized(clip,existing);UnityEngine.Object.DestroyImmediate(clip);EditorUtility.SetDirty(existing);return existing;}AssetDatabase.CreateAsset(clip,path);return clip;
    }
    static void State(Animator animator,string name,AnimationClip clip,float speed=1)
    {
        var controller=(AnimatorController)animator.runtimeAnimatorController;var sm=controller.layers[0].stateMachine;var state=sm.states.Select(s=>s.state).FirstOrDefault(s=>s.name==name)??sm.AddState(name);state.motion=clip;state.speed=speed;EditorUtility.SetDirty(controller);
    }
    static void GhostStun(Animator animator,Transform actor,Action<string> check)
    {
        var clips=animator.runtimeAnimatorController.animationClips;var idle=clips.First(c=>c.name.Contains("Flying_Idle"));var head=animator.GetComponentsInChildren<Transform>().Single(t=>t.name=="Head");var torso=animator.GetComponentsInChildren<Transform>().Single(t=>t.name=="Torso");
        var clip=UnityEngine.Object.Instantiate(idle);clip.events=Array.Empty<AnimationEvent>();var targets=new[]{torso,head};var curves=targets.ToDictionary(t=>t,_=>new[]{new AnimationCurve(),new AnimationCurve(),new AnimationCurve(),new AnimationCurve()});
        for(int i=0;i<=35;i++)
        {
            float t=i*idle.length/35;idle.SampleAnimation(animator.gameObject,t);torso.rotation=Quaternion.AngleAxis(18,actor.right)*torso.rotation;head.rotation=Quaternion.AngleAxis(28,actor.right)*head.rotation;
            foreach(var bone in targets){var q=bone.localRotation;for(int j=0;j<4;j++)curves[bone][j].AddKey(t,j==0?q.x:j==1?q.y:j==2?q.z:q.w);}
        }
        foreach(var bone in targets)for(int j=0;j<4;j++)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(AnimationUtility.CalculateTransformPath(bone,animator.transform),typeof(Transform),"m_LocalRotation."+"xyzw"[j]),curves[bone][j]);
        clip.EnsureQuaternionContinuity();var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,settings);State(animator,"Stunned",SaveClip("Shadow_Stunned",clip));
        var punch=clips.First(c=>c.name.Contains("Punch"));var hands=animator.GetComponentsInChildren<Transform>().Where(t=>t.name=="Index1.L"||t.name=="Index1.R").ToArray();float peak=-100,peakTime=.4f;
        for(int i=0;i<=60;i++){float t=punch.length*i/60;punch.SampleAnimation(animator.gameObject,t);float d=hands.Max(h=>Vector3.Dot(h.position-actor.position,actor.forward));if(d>peak){peak=d;peakTime=t;}}
        State(animator,"Attack",punch,Mathf.Clamp(peakTime/.4f,.65f,2.5f));idle.SampleAnimation(animator.gameObject,0);check("Ghost bow clip authored on Torso/Head; punch peak aligned to native 0.4s damage telegraph.");
    }
    static void GuardianClips(Animator animator,Transform actor,Action<string> check)
    {
        var names=new[]{"B-hips","B-spine","B-head","B-upperArm.L","B-upperArm.R","B-forearm.L","B-forearm.R","B-thigh.L","B-thigh.R","B-shin.L","B-shin.R"};var bones=names.ToDictionary(n=>n,n=>animator.GetComponentsInChildren<Transform>().Single(t=>t.name==n));
        var idle=animator.runtimeAnimatorController.animationClips.First(c=>c.name.Contains("Idle"));idle.SampleAnimation(animator.gameObject,0);
        void Aim(string upper,string lower,Vector3 upperDir,Vector3 lowerDir){var a=bones[upper];var b=bones[lower];var hand=b.GetComponentsInChildren<Transform>().First(t=>t.name.StartsWith("B-hand"));a.rotation=Quaternion.FromToRotation(b.position-a.position,actor.TransformDirection(upperDir))*a.rotation;b.rotation=Quaternion.FromToRotation(hand.position-b.position,actor.TransformDirection(lowerDir))*b.rotation;}
        Aim("B-upperArm.L","B-forearm.L",new Vector3(-.22f,-1,0),new Vector3(-.05f,-1,.15f));Aim("B-upperArm.R","B-forearm.R",new Vector3(.22f,-1,0),new Vector3(.05f,-1,.15f));
        var ready=bones.ToDictionary(p=>p.Key,p=>p.Value.localRotation);var positions=bones.ToDictionary(p=>p.Key,p=>p.Value.localPosition);
        foreach(string state in new[]{"Idle","Walk","Sweep","Slam","Hurt","Death","Stunned"})
        {
            float duration=state=="Sweep"?2.2f:state=="Slam"?2.95f:state=="Hurt"?.35f:state=="Death"?1:state=="Walk"?1.05f:2;
            var curves=names.ToDictionary(n=>n,_=>new[]{new AnimationCurve(),new AnimationCurve(),new AnimationCurve(),new AnimationCurve()});var hip=new[]{new AnimationCurve(),new AnimationCurve(),new AnimationCurve()};int samples=Mathf.CeilToInt(duration*30);
            for(int sample=0;sample<=samples;sample++)
            {
                float t=duration*sample/samples,phase=t/duration;foreach(var b in bones){b.Value.localRotation=ready[b.Key];b.Value.localPosition=positions[b.Key];}
                if(state=="Idle")bones["B-spine"].localRotation*=Quaternion.Euler(Mathf.Sin(phase*Mathf.PI*2)*1.5f,0,0);
                if(state=="Walk"){float gait=Mathf.Sin(phase*Mathf.PI*2);bones["B-thigh.L"].localRotation*=Quaternion.Euler(gait*25,0,0);bones["B-thigh.R"].localRotation*=Quaternion.Euler(-gait*25,0,0);bones["B-shin.L"].localRotation*=Quaternion.Euler(Mathf.Max(0,-gait)*28,0,0);bones["B-shin.R"].localRotation*=Quaternion.Euler(Mathf.Max(0,gait)*28,0,0);bones["B-upperArm.L"].localRotation*=Quaternion.Euler(-gait*16,0,0);bones["B-upperArm.R"].localRotation*=Quaternion.Euler(gait*16,0,0);}
                if(state=="Sweep"){float wind=Mathf.Clamp01(t/.8f),strike=Mathf.Clamp01((t-.8f)/.2f),recover=Mathf.Clamp01((t-1)/1.2f);var target=Vector3.Lerp(new Vector3(.22f,-1,0),new Vector3(.9f,.15f,-.5f),wind);target=Vector3.Lerp(target,new Vector3(-.7f,-.1f,.8f),strike);target=Vector3.Lerp(target,new Vector3(.22f,-1,0),recover);Aim("B-upperArm.R","B-forearm.R",target,Vector3.Lerp(new Vector3(.3f,-.15f,.8f),new Vector3(-.5f,-.2f,.8f),strike));bones["B-spine"].localRotation*=Quaternion.Euler(0,Mathf.Lerp(-25*wind,25,strike)*(1-recover),0);}
                if(state=="Slam"){float raise=Mathf.Clamp01(t/1.2f),strike=Mathf.Clamp01((t-1.2f)/.22f),recover=Mathf.Clamp01((t-1.55f)/1.4f);var arm=Vector3.Lerp(new Vector3(.22f,-1,0),new Vector3(.3f,1,-.15f),raise);arm=Vector3.Lerp(arm,new Vector3(.25f,-.5f,.8f),strike);arm=Vector3.Lerp(arm,new Vector3(.22f,-1,0),recover);Aim("B-upperArm.L","B-forearm.L",new Vector3(-arm.x,arm.y,arm.z),new Vector3(-.1f,arm.y,arm.z+.1f));Aim("B-upperArm.R","B-forearm.R",arm,new Vector3(.1f,arm.y,arm.z+.1f));bones["B-spine"].localRotation*=Quaternion.Euler(42*strike*(1-recover),0,0);bones["B-hips"].localPosition-=Vector3.up*(.15f*strike*(1-recover));}
                if(state=="Hurt")bones["B-spine"].localRotation*=Quaternion.Euler(-16*Mathf.Sin(phase*Mathf.PI),0,0);
                if(state=="Death"){bones["B-hips"].localPosition-=Vector3.up*(phase*.75f);bones["B-spine"].localRotation*=Quaternion.Euler(phase*70,0,phase*12);}
                if(state=="Stunned"){bones["B-spine"].rotation=Quaternion.AngleAxis(32+Mathf.Sin(phase*Mathf.PI*2)*2,actor.right)*bones["B-spine"].rotation;bones["B-head"].rotation=Quaternion.AngleAxis(24,actor.right)*bones["B-head"].rotation;bones["B-hips"].localPosition-=Vector3.up*.07f;}
                foreach(var b in bones){var q=b.Value.localRotation;for(int j=0;j<4;j++)curves[b.Key][j].AddKey(t,j==0?q.x:j==1?q.y:j==2?q.z:q.w);}var pos=bones["B-hips"].localPosition;hip[0].AddKey(t,pos.x);hip[1].AddKey(t,pos.y);hip[2].AddKey(t,pos.z);
            }
            var clip=new AnimationClip{frameRate=30};foreach(var b in bones)for(int j=0;j<4;j++)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(AnimationUtility.CalculateTransformPath(b.Value,animator.transform),typeof(Transform),"m_LocalRotation."+"xyzw"[j]),curves[b.Key][j]);for(int j=0;j<3;j++)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(AnimationUtility.CalculateTransformPath(bones["B-hips"],animator.transform),typeof(Transform),"m_LocalPosition."+"xyz"[j]),hip[j]);clip.EnsureQuaternionContinuity();var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=state=="Idle"||state=="Walk"||state=="Stunned";AnimationUtility.SetAnimationClipSettings(clip,settings);State(animator,state,SaveClip("Guardian_"+state,clip));
        }
        check("Guardian seven rig clips rebuilt with corrected forward basis and held bowed stun; FSM still owns damage.");
    }
}
