using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(10000)]
public sealed class CameraBodyOcclusion:MonoBehaviour
{
    [SerializeField] Camera view;
    SkinnedMeshRenderer[] body;ShadowCastingMode[] defaults;bool hidden;
    void Awake(){body=GetComponentsInChildren<SkinnedMeshRenderer>(true);defaults=new ShadowCastingMode[body.Length];for(int i=0;i<body.Length;i++)defaults[i]=body[i].shadowCastingMode;}
    void OnEnable()=>Application.onBeforeRender+=Refresh;
    void LateUpdate()=>Refresh();
    void OnDisable(){Application.onBeforeRender-=Refresh;Restore();}
    void Refresh()
    {
        if(!view||body==null)return;var eye=transform.position+Vector3.up*1.35f;float d=Vector3.Distance(view.transform.position,eye);
        bool next=hidden?d<1.1f:d<.85f;if(next==hidden)return;hidden=next;
        for(int i=0;i<body.Length;i++)if(body[i])body[i].shadowCastingMode=hidden?ShadowCastingMode.ShadowsOnly:defaults[i];
    }
    void Restore(){if(body==null)return;for(int i=0;i<body.Length;i++)if(body[i])body[i].shadowCastingMode=defaults[i];hidden=false;}
}
