using UnityEngine;

public sealed class DoorSoundCue:MonoBehaviour
{
    bool started;
    void Start()=>started=true;
    void OnDisable(){if(started&&Application.isPlaying&&AppRoot.Instance&&AppRoot.Instance.Session.State==GameSession.SessionState.Playing)SoundscapeRuntime.Play(SoundCue.Door,transform.position,.7f);}
}
