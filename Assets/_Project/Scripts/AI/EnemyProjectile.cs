using UnityEngine;

// Đạn chậm của địch (spec chapters/00): kinematic, mỗi frame SphereCast từ vị trí cũ tới mới.
// Chạm Environment thì hủy; chạm An thì gây sát thương một lần rồi hủy; tự hủy sau thời hạn.
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float radius = 0.3f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private LayerMask hitMask = (1 << 6) | (1 << 3);

    private Vector3 velocity;
    private float damage;
    private GameObject owner;
    private string attackId;
    private float age;

    public Vector3 Velocity => velocity;

    public void Launch(Vector3 direction, float speed, float amount, GameObject source, string id)
    {
        velocity = direction.normalized * speed;
        damage = amount;
        owner = source;
        attackId = id;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        age += dt;
        if (age > lifetime) { Destroy(gameObject); return; }

        Vector3 step = velocity * dt;
        if (Physics.SphereCast(transform.position, radius, step.normalized, out RaycastHit hit, step.magnitude, hitMask, QueryTriggerInteraction.Ignore))
        {
            PlayerVitals player = hit.collider.GetComponentInParent<PlayerVitals>();
            if (player != null)
                player.TryTakeDamage(new DamageInfo(owner, damage, DamageTeam.Enemy) { AttackId = attackId });
            Destroy(gameObject);
            return;
        }
        transform.position += step;
    }
}
