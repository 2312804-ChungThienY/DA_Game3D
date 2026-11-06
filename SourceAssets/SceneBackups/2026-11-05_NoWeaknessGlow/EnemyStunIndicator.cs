using UnityEngine;

public sealed class EnemyStunIndicator:MonoBehaviour
{
    [SerializeField] EnemyLanternReceiver receiver;
    [SerializeField] Transform indicator;
    [SerializeField] Transform[] stars;
    [SerializeField] float height=2.2f;
    Camera view;
    void Start(){view=Camera.main;if(indicator)indicator.gameObject.SetActive(false);}
    void LateUpdate()
    {
        if(!receiver||!indicator)return;bool show=receiver.IsStunned;
        if(indicator.gameObject.activeSelf!=show)indicator.gameObject.SetActive(show);if(!show)return;
        indicator.position=receiver.transform.position+Vector3.up*height;
        var scale=receiver.transform.lossyScale;indicator.localScale=new Vector3(.85f/Mathf.Max(.01f,Mathf.Abs(scale.x)),.85f/Mathf.Max(.01f,Mathf.Abs(scale.y)),.85f/Mathf.Max(.01f,Mathf.Abs(scale.z)));
        if(view)
        {
            var point=view.WorldToViewportPoint(indicator.position);
            if(point.z>0)
            {
                bool clipped=point.x<.07f||point.x>.93f||point.y<.18f||point.y>.78f;
                point.x=Mathf.Clamp(point.x,.07f,.93f);point.y=Mathf.Clamp(point.y,.18f,.78f);
                // Clamping a tall enemy's overhead marker downward can put it
                // inside its head. Bring it in front of the body along the same
                // view ray, retaining depth tests against the environment.
                if(clipped)point.z=Mathf.Max(view.nearClipPlane+.25f,point.z-(receiver.GetComponent<StoneGuardian>()?1.8f:.85f));
                indicator.position=view.ViewportToWorldPoint(point);
            }
            indicator.rotation=Quaternion.LookRotation(view.transform.position-indicator.position,Vector3.up);
        }
        for(int i=0;i<stars.Length;i++)if(stars[i]){float phase=Time.time*3.2f+i*Mathf.PI*2/Mathf.Max(1,stars.Length);stars[i].localPosition=new Vector3(Mathf.Cos(phase)*.33f,Mathf.Sin(phase)*.12f,0);stars[i].localRotation=Quaternion.Euler(0,0,Time.time*45+i*25);}
    }
}
