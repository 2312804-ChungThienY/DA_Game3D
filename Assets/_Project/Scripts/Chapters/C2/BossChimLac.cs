using System.Collections;
using UnityEngine;

// Trùm Chim Lạc nhiễm Hắc Sương (C2 mục 3). Đậu luân phiên trên 3 cột; chiếu đèn 1,5 s khi đang đậu thì choáng
// rơi xuống sàn; quạt gió đẩy An ra mép; lao theo vệt báo 1 giây. ≤ 100 máu: đậu ngắn hơn và lao 2 lần.
public class BossChimLac : BossBase
{
    public enum ChimState { Waiting, Perched, Gust, DiveTelegraph, Dive, Landed, Stunned, FlyToPerch, Dead }

    [SerializeField] private Transform[] perchTops = new Transform[3];
    [SerializeField] private Transform arenaCenter;
    [SerializeField] private float floorY = 18f;
    [SerializeField] private ChimLacFocusReceiver focus;
    [SerializeField] private Transform diveTelegraph;
    [SerializeField] private GameObject gustTelegraph;
    [SerializeField] private LayerMask playerMask = 1 << 6;

    [Header("Nhịp đòn")]
    [SerializeField] private float perchTime = 3.5f;
    [SerializeField] private float perchTimeEnraged = 2.5f;
    [SerializeField] private float enrageHealth = 100f;
    [SerializeField] private float gustTelegraphTime = 0.8f;
    [SerializeField] private float gustTime = 2f;
    [SerializeField] private float gustPush = 4f;
    [SerializeField] private float diveTelegraphTime = 1f;
    [SerializeField] private float diveTime = 0.4f;
    [SerializeField] private float diveDamage = 20f;
    [SerializeField] private float diveRadius = 1f;
    [SerializeField] private float landedTime = 1.5f;
    [SerializeField] private float stunTime = 4f;
    [SerializeField] private float stunDamageCap = 60f;
    [SerializeField] private float flyTime = 1f;

    private readonly Collider[] hits = new Collider[4];
    private int perchIndex;
    private int diveCount;

    public ChimState State { get; private set; } = ChimState.Waiting;
    public int PerchIndex => perchIndex;
    public bool Enraged => health.Current <= enrageHealth;

    protected override void Awake()
    {
        base.Awake();
        if (perchTops == null || perchTops.Length < 2 || arenaCenter == null || focus == null || diveTelegraph == null)
        {
            Debug.LogError($"{name}: BossChimLac thiếu cột đậu, tâm đấu trường, Focus hoặc telegraph.", this);
            enabled = false;
            return;
        }
        diveTelegraph.gameObject.SetActive(false);
        if (gustTelegraph != null) gustTelegraph.SetActive(false);
        StunDamageMultiplier = 1.5f; // Choáng được tính như đang bị làm yếu.
    }

    protected override IEnumerator RunAI()
    {
        perchIndex = 0;
        yield return FlyTo(PerchPoint(perchIndex), flyTime);

        while (true)
        {
            // Đậu: nhận Focus; đủ 1,5 s thì choáng.
            State = ChimState.Perched;
            focus.ResetProgress();
            float t = 0f, dur = Enraged ? perchTimeEnraged : perchTime;
            bool stunned = false;
            while (t < dur)
            {
                if (focus.StunTriggered) { stunned = true; break; }
                if (CanAct) t += Time.deltaTime;
                yield return null;
            }

            if (stunned)
            {
                yield return Stun();
                continue;
            }

            if (Random.value < 0.5f) yield return Gust();
            yield return DiveAt();
            if (Enraged) yield return DiveAt();

            State = ChimState.Landed;
            yield return Wait(landedTime);

            perchIndex = NextPerch();
            yield return FlyTo(PerchPoint(perchIndex), flyTime);
        }
    }

    private IEnumerator Gust()
    {
        State = ChimState.Gust;
        if (gustTelegraph != null) gustTelegraph.SetActive(true);
        yield return Wait(gustTelegraphTime);
        float t = 0f;
        while (t < gustTime)
        {
            if (CanAct)
            {
                t += Time.deltaTime;
                // An trong cột gió đang bật thì không bị đẩy.
                if (!WindColumn.PlayerInActiveColumn && playerMotor != null)
                {
                    Vector3 away = PlayerPosition - transform.position;
                    away.y = 0f;
                    if (away.sqrMagnitude > 0.01f) playerMotor.AddExternalVelocity(away.normalized * gustPush);
                }
            }
            yield return null;
        }
        if (gustTelegraph != null) gustTelegraph.SetActive(false);
    }

    private IEnumerator DiveAt()
    {
        State = ChimState.DiveTelegraph;
        Vector3 start = transform.position;
        Vector3 target = PlayerPosition;
        target.y = floorY + 1f;
        ShowDiveLine(start, target);
        yield return Wait(diveTelegraphTime);
        diveTelegraph.gameObject.SetActive(false);

        State = ChimState.Dive;
        diveCount++;
        string attackId = $"chimlac_dive_{diveCount}";
        bool hit = false;
        float t = 0f;
        Vector3 previous = start;
        while (t < diveTime)
        {
            if (CanAct)
            {
                t += Time.deltaTime;
                Vector3 next = Vector3.Lerp(start, target, Mathf.Clamp01(t / diveTime));
                FaceTowards(next - previous);
                transform.position = next;
                if (!hit) hit = TryHit(previous, next, attackId);
                previous = next;
            }
            yield return null;
        }
    }

    private IEnumerator Stun()
    {
        State = ChimState.Stunned;
        Vector3 perch = PerchPoint(perchIndex);
        Vector3 toCenter = arenaCenter.position - perch;
        toCenter.y = 0f;
        Vector3 ground = perch + toCenter.normalized * 1.5f;
        ground.y = floorY;
        BeginStunWindow(stunTime, stunDamageCap);
        yield return FlyTo(ground, 0.5f, false);
        yield return Wait(stunTime - 0.5f);
        perchIndex = NextPerch();
        yield return FlyTo(PerchPoint(perchIndex), flyTime);
    }

    // Bay bất tử tới điểm (FlyToPerch theo spec).
    private IEnumerator FlyTo(Vector3 destination, float duration, bool invulnerable = true)
    {
        if (invulnerable) State = ChimState.FlyToPerch;
        Invulnerable = invulnerable;
        Vector3 from = transform.position;
        float t = 0f;
        while (t < duration)
        {
            if (CanAct) t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            Vector3 p = Vector3.Lerp(from, destination, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 2f;
            FaceTowards(p - transform.position);
            transform.position = p;
            yield return null;
        }
        transform.position = destination;
        Invulnerable = false;
    }

    private bool TryHit(Vector3 from, Vector3 to, string attackId)
    {
        int count = Physics.OverlapCapsuleNonAlloc(from, to, diveRadius, hits, playerMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            PlayerVitals target = hits[i].GetComponentInParent<PlayerVitals>();
            if (target == null || target != player) continue;
            target.TryTakeDamage(new DamageInfo(gameObject, diveDamage, DamageTeam.Enemy) { AttackId = attackId });
            return true;
        }
        return false;
    }

    private void ShowDiveLine(Vector3 from, Vector3 to)
    {
        Vector3 d = to - from;
        diveTelegraph.position = from + d * 0.5f;
        diveTelegraph.rotation = Quaternion.LookRotation(d);
        diveTelegraph.localScale = new Vector3(2f, 0.05f, d.magnitude);
        diveTelegraph.gameObject.SetActive(true);
    }

    private Vector3 PerchPoint(int index) => perchTops[index].position + Vector3.up * 0.6f;

    private int NextPerch()
    {
        int next = Random.Range(0, perchTops.Length - 1);
        return next >= perchIndex ? next + 1 : next;
    }

    private void FaceTowards(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(direction);
    }

    protected override void OnDefeated()
    {
        State = ChimState.Dead;
        diveTelegraph.gameObject.SetActive(false);
        if (gustTelegraph != null) gustTelegraph.SetActive(false);
    }
}
