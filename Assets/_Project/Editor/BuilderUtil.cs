using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Hàm dựng blockout dùng chung cho các công cụ Editor (spec chapters/00: khối cơ bản, vật liệu M_BO_*).
public static class BuilderUtil
{
    public const string MaterialFolder = "Assets/_Project/Materials/Blockout/";
    public const string FontPath = "Assets/A31 - Noto Sans/NotoSans-VariableFont_wdth,wght SDF.asset";

    public static int Layer(string name) => LayerMask.NameToLayer(name);

    public static Material Mat(string name)
    {
        Material m = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + name + ".mat");
        if (m == null) throw new Exception($"Thiếu vật liệu {name}");
        return m;
    }

    // Vật liệu trong suốt tạo từ M_BO_Telegraph (đã cấu hình transparent) với màu khác.
    public static Material TransparentMat(string name, Color color)
    {
        string path = MaterialFolder + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Mat("M_BO_Telegraph"));
            AssetDatabase.CreateAsset(m, path);
        }
        m.SetColor("_BaseColor", color);
        EditorUtility.SetDirty(m);
        return m;
    }

    public static GameObject Empty(string name, Transform parent, Vector3 worldPos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        return go;
    }

    // Hộp theo tâm/kích thước thế giới (scale = size).
    public static GameObject Box(string name, Transform parent, Vector3 center, Vector3 size, Material mat, int layer, bool collider = true)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, true);
        go.transform.position = center;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = size;
        go.layer = layer;
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        if (!collider) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        go.isStatic = collider && layer == Layer("Environment");
        return go;
    }

    // Hộp theo khoảng min..max.
    public static GameObject BoxMinMax(string name, Transform parent, Vector3 min, Vector3 max, Material mat, int layer, bool collider = true) =>
        Box(name, parent, (min + max) * 0.5f, max - min, mat, layer, collider);

    public static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, int layer, bool collider)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        go.layer = layer;
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        if (!collider) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    public static Light PointLight(string name, Transform parent, Vector3 pos, Color color, float range, float intensity)
    {
        GameObject go = Empty(name, parent, pos);
        Light l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.range = range;
        l.intensity = intensity;
        l.shadows = LightShadows.None;
        return l;
    }

    public static TextMeshPro WorldText(string name, Transform parent, Vector3 localPos, Vector2 size, string text, float fontSize, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        TextMeshPro t = go.AddComponent<TextMeshPro>();
        t.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        t.rectTransform.sizeDelta = size;
        t.text = text;
        t.fontSize = fontSize;
        t.enableAutoSizing = true;
        t.fontSizeMin = fontSize * 0.4f;
        t.fontSizeMax = fontSize;
        t.alignment = TextAlignmentOptions.Center;
        t.color = color;
        return t;
    }

    // ---------- SerializedObject ----------
    public static void Set(UnityEngine.Object target, string property, object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(property);
        if (p == null) throw new Exception($"{target.GetType().Name} không có trường {property}");
        Assign(p, value);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Assign(SerializedProperty p, object value)
    {
        switch (value)
        {
            case null: p.objectReferenceValue = null; break;
            case UnityEngine.Object o: p.objectReferenceValue = o; break;
            case string s: p.stringValue = s; break;
            case bool b: p.boolValue = b; break;
            case int i: if (p.propertyType == SerializedPropertyType.Enum) p.enumValueIndex = i; else p.intValue = i; break;
            case float f: p.floatValue = f; break;
            case Vector3 v: p.vector3Value = v; break;
            case Enum e: p.enumValueIndex = Convert.ToInt32(e); break;
            case Array arr:
                p.arraySize = arr.Length;
                for (int k = 0; k < arr.Length; k++) Assign(p.GetArrayElementAtIndex(k), arr.GetValue(k));
                break;
            default: throw new Exception($"Kiểu chưa hỗ trợ: {value.GetType()}");
        }
    }

    public static void SetRequirements(UnityEngine.Object target, string property, params (StateSet set, string id, string name)[] items)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty list = so.FindProperty(property);
        list.arraySize = items.Length;
        for (int i = 0; i < items.Length; i++)
        {
            SerializedProperty e = list.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("set").enumValueIndex = (int)items[i].set;
            e.FindPropertyRelative("id").stringValue = items[i].id;
            e.FindPropertyRelative("displayName").stringValue = items[i].name;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetRewards(UnityEngine.Object target, params (RewardType type, string id, int qty)[] items)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty list = so.FindProperty("rewards");
        list.arraySize = items.Length;
        for (int i = 0; i < items.Length; i++)
        {
            SerializedProperty e = list.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("type").enumValueIndex = (int)items[i].type;
            e.FindPropertyRelative("id").stringValue = items[i].id;
            e.FindPropertyRelative("quantity").intValue = items[i].qty;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Nối UnityEvent (trường private) tới một hàm không tham số.
    public static void AddListener(UnityEngine.Object owner, string eventField, UnityAction action)
    {
        FieldInfo f = owner.GetType().GetField(eventField, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        UnityEvent ev = (UnityEvent)f.GetValue(owner);
        if (ev == null) { ev = new UnityEvent(); f.SetValue(owner, ev); }
        UnityEventTools.AddPersistentListener(ev, action);
        EditorUtility.SetDirty(owner);
    }

    // ---------- UI ----------
    public static RectTransform RT(string name, Transform parent, Vector2 amin, Vector2 amax, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = Layer("UI");
        RectTransform r = (RectTransform)go.transform;
        r.SetParent(parent, false);
        r.anchorMin = amin;
        r.anchorMax = amax;
        r.pivot = new Vector2((amin.x + amax.x) / 2f, (amin.y + amax.y) / 2f);
        r.anchoredPosition = pos;
        r.sizeDelta = size;
        return r;
    }

    public static Image Img(string name, Transform parent, Vector2 amin, Vector2 amax, Vector2 pos, Vector2 size, Color color)
    {
        Image im = RT(name, parent, amin, amax, pos, size).gameObject.AddComponent<Image>();
        im.color = color;
        return im;
    }

    public static TextMeshProUGUI Txt(string name, Transform parent, Vector2 amin, Vector2 amax, Vector2 pos, Vector2 size,
        float fontSize, TextAlignmentOptions align, string text)
    {
        TextMeshProUGUI t = RT(name, parent, amin, amax, pos, size).gameObject.AddComponent<TextMeshProUGUI>();
        t.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        t.fontSize = fontSize;
        t.alignment = align;
        t.text = text;
        t.color = Color.white;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.Normal;
        return t;
    }

    public static Button Btn(string name, Transform parent, Vector2 pos, Vector2 size, string label, float fontSize = 26)
    {
        Image im = Img(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size, new Color(0.25f, 0.2f, 0.12f, 0.95f));
        Button b = im.gameObject.AddComponent<Button>();
        b.targetGraphic = im;
        Txt("Label", im.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, fontSize, TextAlignmentOptions.Center, label);
        return b;
    }

    public static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        foreach (Transform c in root)
        {
            Transform r = FindDeep(c, name);
            if (r != null) return r;
        }
        return null;
    }

    public static GameObject FindInScene(string name)
    {
        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Transform t = FindDeep(root.transform, name);
            if (t != null) return t.gameObject;
        }
        return null;
    }

    public static List<T> FindAll<T>() where T : Component
    {
        List<T> list = new List<T>();
        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            list.AddRange(root.GetComponentsInChildren<T>(true));
        return list;
    }
}
