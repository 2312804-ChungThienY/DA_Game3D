using UnityEngine;

public sealed class CampFireAnimation:MonoBehaviour
{
    [SerializeField] Light fireLight;
    float intensity;
    void Awake(){if(fireLight)intensity=fireLight.intensity;}
    void Update(){if(fireLight)fireLight.intensity=intensity*(.87f+.17f*Mathf.PerlinNoise(Time.time*7.5f,1.4f)+.09f*Mathf.Sin(Time.time*13));}
    void OnDisable(){if(fireLight)fireLight.intensity=intensity;}
}
