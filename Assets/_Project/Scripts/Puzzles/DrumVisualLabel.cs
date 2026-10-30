using TMPro;
using UnityEngine;

// Placeholder 3D labels for the isolated mechanics scene.
public class DrumVisualLabel : MonoBehaviour
{
    [SerializeField] private string label = "A";
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.1f, 0f);
    [SerializeField] private Vector3 localEulerAngles = new Vector3(-90f, 0f, 0f);
    [SerializeField] private float textScale = 0.25f;
    [SerializeField] private float fontSize = 4f;
    [SerializeField] private float textWidth = 2f;

    private void Awake()
    {
        GameObject textObject = new GameObject($"TXT_{label}", typeof(RectTransform), typeof(MeshRenderer));
        textObject.transform.SetParent(transform, false);
        textObject.transform.localPosition = localOffset;
        textObject.transform.localRotation = Quaternion.Euler(localEulerAngles);
        textObject.transform.localScale = Vector3.one * textScale;
        TextMeshPro text = textObject.AddComponent<TextMeshPro>();
        text.text = label;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.enableAutoSizing = false;
        text.rectTransform.sizeDelta = new Vector2(textWidth, 1f);
    }
}
