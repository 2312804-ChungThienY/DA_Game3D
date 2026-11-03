using TMPro;
using UnityEngine;

// Nhãn ký hiệu trống (blockout): một mặt trên và một mặt hướng về lối vào.
// Kích thước tính theo mét thật, không phụ thuộc scale của object cha.
public class DrumVisualLabel : MonoBehaviour
{
    [SerializeField] private string label = "A";
    [SerializeField] private float worldTopOffset = 0.45f;
    [SerializeField] private float worldFrontDistance = 1.1f;
    [SerializeField] private Vector3 frontDirection = new Vector3(0f, 0f, -1f);
    [SerializeField] private float letterHeight = 0.8f;
    [SerializeField] private bool addTopLabel = true;
    [SerializeField] private bool addFrontLabel = true;

    private void Awake()
    {
        if (addTopLabel)
        {
            CreateLabel("Top", transform.position + Vector3.up * worldTopOffset,
                Quaternion.Euler(90f, 0f, 0f));
        }

        if (addFrontLabel && frontDirection.sqrMagnitude > 0.001f)
        {
            Vector3 facing = frontDirection.normalized;
            CreateLabel("Front", transform.position + facing * worldFrontDistance + Vector3.up * 0.2f,
                Quaternion.LookRotation(-facing, Vector3.up));
        }
    }

    private void CreateLabel(string suffix, Vector3 worldPosition, Quaternion worldRotation)
    {
        GameObject textObject = new GameObject($"TXT_{label}_{suffix}", typeof(RectTransform), typeof(MeshRenderer));
        textObject.transform.SetParent(transform, true);
        textObject.transform.SetPositionAndRotation(worldPosition, worldRotation);

        // Bù scale của cha để chữ luôn cao letterHeight mét.
        Vector3 parentScale = transform.lossyScale;
        textObject.transform.localScale = new Vector3(
            1f / Mathf.Max(0.0001f, parentScale.x),
            1f / Mathf.Max(0.0001f, parentScale.y),
            1f / Mathf.Max(0.0001f, parentScale.z));

        TextMeshPro text = textObject.AddComponent<TextMeshPro>();
        text.text = label;
        text.fontSize = letterHeight * 10f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.outlineWidth = 0.2f;
        text.outlineColor = Color.black;
        text.enableAutoSizing = false;
        text.rectTransform.sizeDelta = new Vector2(letterHeight * 2f, letterHeight * 1.2f);
    }
}
