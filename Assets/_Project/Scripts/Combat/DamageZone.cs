using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    [SerializeField, Min(1f)] private float damage = 20f;
    [SerializeField, Min(0.1f)] private float interval = 1f;

    private readonly Dictionary<PlayerVitals, float> nextHitTimes =
        new Dictionary<PlayerVitals, float>();

    private void OnTriggerEnter(Collider other) => TryDamage(other);
    private void OnTriggerStay(Collider other) => TryDamage(other);

    private void TryDamage(Collider other)
    {
        if (!isActiveAndEnabled || !Application.isFocused || Time.timeScale == 0f)
            return;

        PlayerVitals target = other.GetComponentInParent<PlayerVitals>();
        if (target == null || target.IsDead)
            return;

        if (nextHitTimes.TryGetValue(target, out float nextTime) && Time.time < nextTime)
            return;

        if (target.TryTakeDamage(new DamageInfo(gameObject, damage, DamageTeam.Neutral)))
            nextHitTimes[target] = Time.time + interval;
    }

    private void OnDisable() => nextHitTimes.Clear();
}
