using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class UIButtonSound:MonoBehaviour,IPointerEnterHandler,IPointerClickHandler,ISubmitHandler
{
    bool Active=>!GetComponent<Selectable>()||GetComponent<Selectable>().IsInteractable();
    public void OnPointerEnter(PointerEventData e){if(Active)SoundscapeRuntime.Play(SoundCue.Hover);}
    public void OnPointerClick(PointerEventData e){if(Active)SoundscapeRuntime.Play(SoundCue.Click);}
    public void OnSubmit(BaseEventData e){if(Active)SoundscapeRuntime.Play(SoundCue.Click);}
}
