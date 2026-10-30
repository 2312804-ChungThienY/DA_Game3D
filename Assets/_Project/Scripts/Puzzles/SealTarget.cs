using TMPro;
using UnityEngine;

public class SealTarget : MonoBehaviour, ILanternTarget
{
    [Header("References")]
    [SerializeField] private Transform aimPoint;
    [SerializeField] private GameObject barrier;
    [SerializeField] private TMP_Text progressText;

    [Header("Rules")]
    [SerializeField] private float requiredTime = 1.5f;
    [SerializeField] private float graceTime = 0.25f;

    private float progress;
    private float lastFocusTime = float.NegativeInfinity;
    private bool solved;

    public Transform AimPoint => aimPoint;
    public bool IsSolved => solved;

    public bool CanReceiveFocus =>
        isActiveAndEnabled && !solved && aimPoint != null;

    private void Awake()
    {
        if (aimPoint == null ||
            barrier == null ||
            barrier == gameObject)
        {
            Debug.LogError(
                $"{name}: Gán AimPoint và Barrier là object con.",
                this);

            enabled = false;
            return;
        }

        RefreshUI();
    }

    private void Update()
    {
        if (solved || Time.timeScale == 0f)
        {
            return;
        }

        if (progress > 0f &&
            Time.time - lastFocusTime > graceTime)
        {
            progress = 0f;
            RefreshUI();
        }
    }

    public void ReceiveFocus(float deltaTime)
    {
        if (!CanReceiveFocus || deltaTime <= 0f)
        {
            return;
        }

        // Nếu đã mất chiếu quá lâu, bắt đầu lại.
        if (Time.time - lastFocusTime > graceTime)
        {
            progress = 0f;
        }

        lastFocusTime = Time.time;

        progress = Mathf.Min(
            requiredTime,
            progress + deltaTime);

        if (progress >= requiredTime)
        {
            Solve();
        }

        RefreshUI();
    }

    private void Solve()
    {
        if (solved)
        {
            return;
        }

        // Ghi trạng thái trước khi mở đường.
        solved = true;

        barrier.SetActive(false);

        Debug.Log(
            $"{name}: Đã xóa phong ấn.",
            this);
    }

    public void ApplySavedState(bool savedSolved)
    {
        solved = savedSolved;
        progress = 0f;
        lastFocusTime = float.NegativeInfinity;
        barrier.SetActive(!solved);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (progressText == null)
        {
            return;
        }

        progressText.text = solved
            ? "Da xoa phong an"
            : $"Phong an: {progress:0.0}/{requiredTime:0.0} giay";
    }
}
