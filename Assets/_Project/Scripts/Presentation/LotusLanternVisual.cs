using UnityEngine;

public sealed class LotusLanternVisual:MonoBehaviour
{
    [SerializeField] Transform actor;
    void LateUpdate()
    {
        if(!actor||!transform.parent)return;
        transform.position=transform.parent.position-Vector3.up*.22f;
        transform.rotation=Quaternion.Euler(0,actor.eulerAngles.y,0);
        var scale=transform.parent.lossyScale;
        transform.localScale=new Vector3(.22f/Mathf.Max(.001f,Mathf.Abs(scale.x)),.22f/Mathf.Max(.001f,Mathf.Abs(scale.y)),.22f/Mathf.Max(.001f,Mathf.Abs(scale.z)));
    }
}
