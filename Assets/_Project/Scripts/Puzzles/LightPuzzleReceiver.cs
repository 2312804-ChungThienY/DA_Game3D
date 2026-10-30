using TMPro;
using UnityEngine;

public class LightPuzzleReceiver : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float requiredTime = 1f;
    [SerializeField] private GameObject door;
    [SerializeField] private GameObject rewardDisplay;
    [SerializeField] private TMP_Text statusText;

    private float progress;

    public bool IsSolved { get; private set; }

    private void Awake()
    {
        if (door == null || rewardDisplay == null || statusText == null ||
            door == gameObject || rewardDisplay == gameObject)
        {
            Debug.LogError($"{name}: Thiếu cửa, bệ thưởng hoặc UI của câu đố gương.", this);
            enabled = false;
            return;
        }

        door.SetActive(true);
        rewardDisplay.SetActive(false);
        RefreshUI();
    }

    public void TickLight(bool receivingLight, float deltaTime)
    {
        if (!isActiveAndEnabled || IsSolved) return;

        progress = receivingLight && deltaTime > 0f
            ? Mathf.Min(requiredTime, progress + deltaTime)
            : 0f;

        if (progress >= requiredTime)
        {
            IsSolved = true;
            door.SetActive(false);
            rewardDisplay.SetActive(true);
            Debug.Log("Đã giải câu đố gương pz_guong_01 trong scene thử.", this);
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        statusText.text = IsSolved
            ? "Đã giải câu đố gương!"
            : $"Ngọc nhận sáng: {progress:0.0}/{requiredTime:0.0}";
    }
}
