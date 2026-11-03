using UnityEngine;

// Vùng chết dưới map (layer Hazard): An rơi vào thì chết và hồi sinh ở mốc lưu.
[RequireComponent(typeof(Collider))]
public class KillZone : MonoBehaviour
{
    private void Awake()
    {
        Collider zone = GetComponent<Collider>();
        if (!zone.isTrigger)
        {
            Debug.LogError($"{name}: Collider của KillZone phải là trigger.", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActiveAndEnabled) return;

        PlayerVitals vitals = other.GetComponentInParent<PlayerVitals>();
        if (vitals != null) vitals.Kill();
    }
}
