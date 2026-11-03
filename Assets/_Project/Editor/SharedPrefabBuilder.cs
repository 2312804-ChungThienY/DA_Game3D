using TMPro;
using UnityEditor;
using UnityEngine;
using static BuilderUtil;

// Khuôn phòng dùng chung (spec phần 42, chapters/00 mục 4): prefab blockout cho mọi chương.
// Tham chiếu tới object trong scene (WorldState, SaveService, UI) được gán khi đặt instance.
public static class SharedPrefabBuilder
{
    public const string Folder = "Assets/_Project/Prefabs/Shared";

    [MenuItem("Tools/Son Doong/Build Shared Prefabs")]
    public static void BuildAll()
    {
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Shared");
        TransparentMat("M_BO_Mechanic", new Color(0.3f, 0.9f, 1f, 0.28f));

        int env = Layer("Environment"), inter = Layer("Interactable");

        // Bia văn tự: mặt đọc hướng -Z cục bộ.
        Save("PF_LoreStele", root =>
        {
            Prim(PrimitiveType.Cube, "Slab", root.transform, new Vector3(0, 1.1f, 0), new Vector3(1.4f, 2.2f, 0.3f), Mat("M_BO_Wall"), inter, true);
            GameObject content = new GameObject("RevealContent");
            content.transform.SetParent(root.transform, false);
            TextMeshPro text = WorldText("Text", content.transform, new Vector3(0, 1.15f, -0.17f), new Vector2(1.25f, 2f), "Văn tự", 1.4f, new Color(1f, 0.85f, 0.45f));
            GameObject point = new GameObject("RevealPoint");
            point.transform.SetParent(root.transform, false);
            point.transform.localPosition = new Vector3(0, 1.2f, -0.3f);
            LanternRevealSurface reveal = root.AddComponent<LanternRevealSurface>();
            Set(reveal, "revealContent", content);
            Set(reveal, "revealPoint", point.transform);
            LoreStele stele = root.AddComponent<LoreStele>();
            Set(stele, "reveal", reveal);
            Set(stele, "text3D", text);
        });

        Save("PF_QuizPedestal", root =>
        {
            Prim(PrimitiveType.Cube, "Base", root.transform, new Vector3(0, 0.5f, 0), new Vector3(1f, 1f, 1f), Mat("M_BO_Interact"), inter, true);
            GameObject closed = Prim(PrimitiveType.Sphere, "ClosedVisual", root.transform, new Vector3(0, 1.25f, 0), Vector3.one * 0.4f, Mat("M_BO_Mechanic"), 0, false);
            GameObject opened = Prim(PrimitiveType.Cylinder, "OpenedVisual", root.transform, new Vector3(0, 1.05f, 0), new Vector3(0.8f, 0.05f, 0.8f), Mat("M_BO_Relic"), 0, false);
            opened.SetActive(false);
            QuizPedestal p = root.AddComponent<QuizPedestal>();
            Set(p, "openedVisual", opened);
            Set(p, "closedVisual", closed);
        });

        Save("PF_WindColumn", root =>
        {
            root.layer = Layer("Ignore Raycast");
            CapsuleCollider c = root.AddComponent<CapsuleCollider>();
            c.isTrigger = true;
            c.radius = 1.2f;
            c.height = 6f;
            c.center = new Vector3(0, 3f, 0);
            GameObject visual = Prim(PrimitiveType.Cylinder, "Visual", root.transform, new Vector3(0, 3f, 0), new Vector3(2.2f, 3f, 2.2f), Mat("M_BO_Mechanic"), 0, false);
            WindColumn w = root.AddComponent<WindColumn>();
            Set(w, "visual", visual);
        });

        Save("PF_CheckpointAltar", root =>
        {
            Prim(PrimitiveType.Cube, "Altar", root.transform, new Vector3(0, 0.5f, 0), new Vector3(0.8f, 1f, 0.8f), Mat("M_BO_Interact"), inter, true);
            Prim(PrimitiveType.Sphere, "Flame", root.transform, new Vector3(0, 1.2f, 0), Vector3.one * 0.35f, Mat("M_BO_Relic"), 0, false);
            GameObject spawn = new GameObject("SpawnPoint");
            spawn.transform.SetParent(root.transform, false);
            spawn.transform.localPosition = new Vector3(1.5f, 0.2f, 0f);
            Set(root.AddComponent<CheckpointAltar>(), "spawnPoint", spawn.transform);
        });

        Save("PF_CheckpointTrigger", root =>
        {
            root.layer = Layer("Ignore Raycast");
            BoxCollider b = root.AddComponent<BoxCollider>();
            b.isTrigger = true;
            b.size = new Vector3(6f, 3f, 2f);
            b.center = new Vector3(0, 1.5f, 0);
            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            GameObject spawn = new GameObject("SpawnPoint");
            spawn.transform.SetParent(root.transform, false);
            spawn.transform.localPosition = new Vector3(0, 0.2f, 0);
            Set(root.AddComponent<CheckpointTrigger>(), "spawnPoint", spawn.transform);
        });

        Save("PF_KillZone", root =>
        {
            root.layer = Layer("Hazard");
            BoxCollider b = root.AddComponent<BoxCollider>();
            b.isTrigger = true;
            b.size = new Vector3(60f, 4f, 120f);
            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            root.AddComponent<KillZone>();
        });

        Save("PF_BoundaryWall", root =>
        {
            root.layer = env;
            BoxCollider b = root.AddComponent<BoxCollider>();
            b.size = new Vector3(1f, 6f, 1f);
            b.center = new Vector3(0, 3f, 0);
        });

        Save("PF_RequirementDoor", root =>
        {
            GameObject door = Prim(PrimitiveType.Cube, "Door", root.transform, new Vector3(0, 2f, 0), new Vector3(3f, 4f, 0.4f), Mat("M_BO_Interact"), env, true);
            Set(root.AddComponent<RequirementDoor>(), "door", door);
        });

        Save("PF_ChapterExit", root =>
        {
            GameObject portal = Prim(PrimitiveType.Cube, "Portal", root.transform, new Vector3(0, 1.5f, 0), new Vector3(1.6f, 3f, 0.4f), Mat("M_BO_Relic"), inter, true);
            Set(root.AddComponent<ChapterExit>(), "portalVisual", portal);
        });

        Save("PF_RelicAltar", root =>
        {
            Prim(PrimitiveType.Cube, "Base", root.transform, new Vector3(0, 0.5f, 0), new Vector3(2f, 1f, 2f), Mat("M_BO_Interact"), inter, true);
            GameObject restored = Prim(PrimitiveType.Cylinder, "RestoredVisual", root.transform, new Vector3(0, 1.3f, 0), new Vector3(1.4f, 0.3f, 1.4f), Mat("M_BO_Relic"), 0, false);
            restored.SetActive(false);
            GameObject glow = new GameObject("ReadyGlow");
            glow.transform.SetParent(root.transform, false);
            glow.transform.localPosition = new Vector3(0, 2.2f, 0);
            Light l = glow.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.7f, 0.3f);
            l.range = 6f;
            l.intensity = 4f;
            RelicAltar a = root.AddComponent<RelicAltar>();
            Set(a, "restoredVisual", restored);
            Set(a, "readyGlow", glow);
        });

        Save("PF_ItemPickup", root =>
        {
            root.layer = inter;
            GameObject v = Prim(PrimitiveType.Cube, "Visual", root.transform, Vector3.zero, Vector3.one * 0.45f, Mat("M_BO_Herb"), inter, false);
            BoxCollider b = root.AddComponent<BoxCollider>();
            b.size = Vector3.one * 0.9f;
            root.AddComponent<ItemPickup>();
        });

        Save("PF_AbilityPickup", root =>
        {
            root.layer = inter;
            Prim(PrimitiveType.Cube, "Feather", root.transform, Vector3.zero, new Vector3(0.15f, 0.9f, 0.35f), Mat("M_BO_Relic"), inter, false);
            BoxCollider b = root.AddComponent<BoxCollider>();
            b.size = new Vector3(0.8f, 1.2f, 0.8f);
            root.AddComponent<AbilityPickup>();
        });

        Save("PF_SequenceBell", root =>
        {
            Prim(PrimitiveType.Cylinder, "Post", root.transform, new Vector3(0, 0.6f, 0), new Vector3(0.12f, 0.6f, 0.12f), Mat("M_BO_Wall"), 0, false);
            Prim(PrimitiveType.Sphere, "Bell", root.transform, new Vector3(0, 1.35f, 0), Vector3.one * 0.6f, Mat("M_BO_Interact"), inter, true);
            GameObject hit = Prim(PrimitiveType.Sphere, "HitIndicator", root.transform, new Vector3(0, 1.35f, 0), Vector3.one * 0.85f, Mat("M_BO_Mechanic"), 0, false);
            SequenceInputNode node = root.AddComponent<SequenceInputNode>();
            Set(node, "hitIndicator", hit);
        });

        GameObject projectile = Save("PF_EnemyProjectile", root =>
        {
            Prim(PrimitiveType.Sphere, "Orb", root.transform, Vector3.zero, Vector3.one * 0.6f, Mat("M_BO_Boss"), 0, false);
            root.AddComponent<EnemyProjectile>();
        });

        Save("PF_HacAnhNem", root =>
        {
            root.layer = Layer("Enemy");
            CapsuleCollider cap = root.AddComponent<CapsuleCollider>();
            cap.radius = 0.4f;
            cap.height = 1.8f;
            cap.center = new Vector3(0, 0.9f, 0);
            GameObject body = Prim(PrimitiveType.Cube, "Body", root.transform, new Vector3(0, 0.9f, 0), new Vector3(0.8f, 1.8f, 0.6f), Mat("M_BO_Enemy"), 0, false);
            Prim(PrimitiveType.Cube, "Face", root.transform, new Vector3(0, 1.5f, 0.32f), new Vector3(0.4f, 0.12f, 0.05f), Mat("M_BO_Relic"), 0, false);
            GameObject hand = new GameObject("Hand");
            hand.transform.SetParent(root.transform, false);
            hand.transform.localPosition = new Vector3(0.5f, 1.5f, 0.4f);
            GameObject aim = new GameObject("AimPoint");
            aim.transform.SetParent(root.transform, false);
            aim.transform.localPosition = new Vector3(0, 1.2f, 0);
            GameObject hitbox = new GameObject("LanternHitbox");
            hitbox.transform.SetParent(root.transform, false);
            hitbox.layer = Layer("LanternTarget");
            CapsuleCollider hc = hitbox.AddComponent<CapsuleCollider>();
            hc.radius = 0.5f;
            hc.height = 1.9f;
            hc.center = new Vector3(0, 0.95f, 0);
            GameObject line = Prim(PrimitiveType.Cube, "AimTelegraph", root.transform, Vector3.zero, Vector3.one, Mat("M_BO_Telegraph"), 0, false);
            GameObject melee = Prim(PrimitiveType.Cube, "MeleeTelegraph", root.transform, new Vector3(0, 0.03f, 1.1f), new Vector3(1.2f, 0.03f, 1.6f), Mat("M_BO_Telegraph"), 0, false);
            EnemyDefinition def = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Project/Data/Enemies/ED_HacAnhNem.asset");
            Set(root.AddComponent<EnemyHealth>(), "definition", def);
            EnemyLanternReceiver r = root.AddComponent<EnemyLanternReceiver>();
            Set(r, "definition", def);
            Set(r, "aimPoint", aim.transform);
            Set(r, "tintRenderer", body.GetComponent<Renderer>());
            ThrowerBrain brain = root.AddComponent<ThrowerBrain>();
            Set(brain, "hand", hand.transform);
            Set(brain, "projectilePrefab", projectile.GetComponent<EnemyProjectile>());
            Set(brain, "aimTelegraph", line.transform);
            Set(brain, "meleeTelegraph", melee);
        });

        AssetDatabase.SaveAssets();
        Debug.Log("SharedPrefabBuilder: đã tạo prefab dùng chung trong " + Folder);
    }

    public static GameObject Load(string name) => AssetDatabase.LoadAssetAtPath<GameObject>($"{Folder}/{name}.prefab");

    // Đặt instance prefab vào scene tại vị trí/hướng cho trước.
    public static GameObject Place(string prefab, Transform parent, Vector3 position, float yaw = 0f, string name = null)
    {
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(Load(prefab), parent);
        go.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
        if (name != null) go.name = name;
        return go;
    }

    private static GameObject Save(string name, System.Action<GameObject> build)
    {
        GameObject root = new GameObject(name);
        try
        {
            build(root);
            return PrefabUtility.SaveAsPrefabAsset(root, $"{Folder}/{name}.prefab");
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }
}
