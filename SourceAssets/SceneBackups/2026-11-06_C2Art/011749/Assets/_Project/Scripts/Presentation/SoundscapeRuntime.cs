using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SoundCue{Click,Hover,Pickup,Success,Hit,Stone,Door,Drum,Ghost}

// Presentation only: audio never advances puzzles, awards items or writes a save.
public sealed class SoundscapeRuntime:MonoBehaviour
{
    public static SoundscapeRuntime Instance{get;private set;}
    public SoundPalette Palette{get;private set;}
    public AudioSource MusicA{get;private set;}
    public AudioSource MusicB{get;private set;}
    public AudioSource Ambience{get;private set;}
    public AudioSource Ui{get;private set;}
    WorldStateStore world;int relics,puzzles,bosses,seals;PlayerMotor player;
    float fade=1;bool targetB;AudioClip wanted;float nextCombatCheck;bool combat;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetStatics()=>Instance=null;
    void Awake()
    {
        Instance=this;Palette=Resources.Load<SoundPalette>("C1SoundPalette");
        if(!Palette)return;
        MusicA=Source("Music_Exploration",Palette.music,true,true);MusicB=Source("Music_Crossfade",Palette.music,true,true);
        Ambience=Source("Ambience_Cave",Palette.sfx,true,false);Ambience.clip=Palette.ambience;Ambience.volume=.23f;
        Ui=Source("UI_Feedback",Palette.ui,false,true);Ui.volume=.7f;
        SceneManager.sceneLoaded+=OnScene;AppRoot.Instance.Session.StateChanged+=OnState;
    }
    AudioSource Source(string name,UnityEngine.Audio.AudioMixerGroup group,bool loop,bool ignorePause)
    {
        var g=new GameObject(name);g.transform.SetParent(transform,false);var s=g.AddComponent<AudioSource>();s.playOnAwake=false;s.loop=loop;s.outputAudioMixerGroup=group;s.ignoreListenerPause=ignorePause;s.spatialBlend=0;return s;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded-=OnScene;if(world)world.Changed-=OnWorld;
        if(AppRoot.Instance&&AppRoot.Instance.Session)AppRoot.Instance.Session.StateChanged-=OnState;
        if(Instance==this)Instance=null;AudioListener.pause=false;
    }
    void OnScene(Scene scene,LoadSceneMode mode)
    {
        StopAllCoroutines();
        if(world)world.Changed-=OnWorld;world=null;player=null;combat=false;wanted=null;
        StartCoroutine(BindScene());
    }
    IEnumerator BindScene()
    {
        yield return null;yield return null;yield return new WaitForSecondsRealtime(.12f);
        world=FindAnyObjectByType<WorldStateStore>();player=FindAnyObjectByType<PlayerMotor>();
        if(world){ReadCounts();world.Changed+=OnWorld;}
        if(Palette&&Ambience){if(player&&!Ambience.isPlaying)Ambience.Play();else if(!player)Ambience.Stop();}
        SelectMusic();
    }
    int CompletedPuzzles()=>(world.HasSolvedPuzzle(WorldStateStore.DrumPuzzleId)?1:0)+(world.HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId)?1:0);
    void ReadCounts(){relics=world.RelicCount;puzzles=CompletedPuzzles();bosses=world.Count(StateSet.Bosses);seals=world.Count(StateSet.Seals);}
    void OnWorld()
    {
        if(world.RelicCount>relics)Play(SoundCue.Pickup);
        if(CompletedPuzzles()>puzzles)Play(SoundCue.Success);
        if(world.Count(StateSet.Bosses)>bosses)Play(SoundCue.Success);
        if(world.Count(StateSet.Seals)>seals)Play(SoundCue.Door);
        ReadCounts();
    }
    void OnState(GameSession.SessionState state)
    {
        AudioListener.pause=state==GameSession.SessionState.Paused||state==GameSession.SessionState.InMenu||state==GameSession.SessionState.Dead||state==GameSession.SessionState.ChapterComplete;
    }
    void SelectMusic()
    {
        if(!Palette)return;
        var next=!player?Palette.menuMusic:combat?Palette.battleMusic:Palette.explorationMusic;
        if(next==wanted)return;wanted=next;targetB=!targetB;fade=0;
        var incoming=targetB?MusicB:MusicA;incoming.clip=next;incoming.volume=0;if(next)incoming.Play();
    }
    void Update()
    {
        if(!Palette)return;
        if(Time.unscaledTime>=nextCombatCheck)
        {
            nextCombatCheck=Time.unscaledTime+.3f;combat=false;
            if(player)foreach(var enemy in EnemyHealth.Active)
            {
                if(!enemy||enemy.IsDead||(enemy.transform.position-player.transform.position).sqrMagnitude>196)continue;
                var boss=enemy.GetComponent<StoneGuardian>();var brain=enemy.GetComponent<EnemyBrain>();
                if((boss&&boss.State!=StoneGuardian.GuardianState.Dormant)||(brain&&(brain.State==EnemyState.Chase||brain.State==EnemyState.Attack))){combat=true;break;}
            }
            SelectMusic();
        }
        fade=Mathf.MoveTowards(fade,1,Time.unscaledDeltaTime/.95f);
        float level=AudioListener.pause?.20f:.36f;var incoming=targetB?MusicB:MusicA;var outgoing=targetB?MusicA:MusicB;
        incoming.volume=fade*level;outgoing.volume=(1-fade)*level;
        if(fade>=1&&outgoing.isPlaying)outgoing.Stop();
    }
    public static void Play(SoundCue cue,Vector3? position=null,float gain=1,float pitch=1)
    {
        var s=Instance;if(!s||!s.Palette)return;var p=s.Palette;
        AudioClip clip=cue switch{SoundCue.Click=>p.click,SoundCue.Hover=>p.hover,SoundCue.Pickup=>p.pickup,SoundCue.Success=>p.success,SoundCue.Hit=>p.hit,SoundCue.Stone=>p.stone,SoundCue.Door=>p.door,SoundCue.Drum=>p.drum,SoundCue.Ghost=>p.ghost,_=>null};
        if(!clip)return;
        if(!position.HasValue){if(s.Ui)s.Ui.PlayOneShot(clip,Mathf.Clamp01(gain)*(cue==SoundCue.Hover?.16f:.4f));return;}
        var g=new GameObject("WorldAudio_"+cue);g.transform.position=position.Value;var a=g.AddComponent<AudioSource>();a.outputAudioMixerGroup=p.sfx;a.playOnAwake=false;a.clip=clip;a.volume=Mathf.Clamp01(gain)*.55f;a.pitch=Mathf.Clamp(pitch,.6f,1.6f);a.spatialBlend=1;a.minDistance=1.5f;a.maxDistance=22;a.rolloffMode=AudioRolloffMode.Linear;a.Play();Destroy(g,clip.length/a.pitch+.2f);
    }
}
