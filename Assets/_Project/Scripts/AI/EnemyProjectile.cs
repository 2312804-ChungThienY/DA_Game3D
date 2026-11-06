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
    private bool spent;
    private readonly Collider[] overlaps = new Collider[16];

    public Vector3 Velocity => velocity;

    public void Launch(Vector3 direction, float speed, float amount, GameObject source, string id)
    {
        if(!Finite(direction.x)||!Finite(direction.y)||!Finite(direction.z)||direction.sqrMagnitude<.000001f||!Finite(speed)||speed<=0||!Finite(amount)||amount<=0)
        {spent=true;Destroy(gameObject);return;}
        age=0;spent=false;
        velocity = direction.normalized * speed;
        damage = amount;
        owner = source;
        attackId = id;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (spent || dt <= 0f) return;

        age += dt;
        if (age > lifetime) { spent=true;Destroy(gameObject); return; }

        // SphereCast does not report colliders overlapping its initial sphere.
        int count=Physics.OverlapSphereNonAlloc(transform.position,radius,overlaps,hitMask,QueryTriggerInteraction.Ignore);
        if(count>0)
        {
            PlayerVitals target=null;bool blocked=false;
            for(int i=0;i<count;i++){var p=overlaps[i].GetComponentInParent<PlayerVitals>();if(p)target=p;else blocked=true;}
            Resolve(blocked?null:target);return;
        }

        Vector3 step = velocity * dt;
        if (Physics.SphereCast(transform.position, radius, step.normalized, out RaycastHit hit, step.magnitude, hitMask, QueryTriggerInteraction.Ignore))
        {
            Resolve(hit.collider.GetComponentInParent<PlayerVitals>());
            return;
        }
        transform.position += step;
    }
    static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
    void Resolve(PlayerVitals player)
    {
        spent=true;
        if(player)player.TryTakeDamage(new DamageInfo(owner,damage,DamageTeam.Enemy){AttackId=attackId});
        Destroy(gameObject);
    }
}
