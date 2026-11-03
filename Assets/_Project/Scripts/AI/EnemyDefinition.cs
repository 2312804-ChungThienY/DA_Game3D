using UnityEngine;

// Chỉ số thiết kế của một loại địch (giá trị mặc định = Hắc Ảnh, phần 14–15).
[CreateAssetMenu(menuName = "Son Doong/Enemy Definition", fileName = "ED_NewEnemy")]
public class EnemyDefinition : ScriptableObject
{
    public string enemyName = "Hắc Ảnh";
    public float maxHealth = 60f;
    public float damage = 15f;
    public float walkSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public float detectionRadius = 10f;
    public float viewAngle = 120f;
    public float attackRange = 1.7f;
    public float attackCooldown = 1.6f;
    public float attackTelegraph = 0.4f;
    public float attackActive = 0.15f;
    public float leashDistance = 18f;
    public float loseSightTime = 3f;
    public float searchTime = 2f;
    public float weakenFocusTime = 1f;
    public float weakenDuration = 4f;
    public float weakenMultiplier = 1.5f;
}
