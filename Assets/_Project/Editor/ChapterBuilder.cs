using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static BuilderUtil;

// Công cụ dựng scene chương (giai đoạn A blockout). Chạy lại được: mỗi bước kiểm tra đã có thì bỏ qua/ghi đè.
public static class ChapterBuilder
{
    private const string Scenes = "Assets/_Project/Scenes/";
    private const string Chapter1Scene = Scenes + "02_HangLinhMach.unity";
    private const string TemplateScene = Scenes + "Template_Chapter.unity";
    private const string Chapter2Scene = Scenes + "03_GiengTroiChimLac.unity";
    private const string Knowledge = "Assets/_Project/Data/Knowledge/";
    private const string C2Data = "Assets/_Project/Data/Chapters/C2/";

    // ================= UI Tri thức (dùng cho mọi chương) =================
    [MenuItem("Tools/Son Doong/Add Knowledge UI To Open Scene")]
    public static void AddKnowledgeUI()
    {
        GameObject session = FindInScene("GameSession");
        Transform canvas = FindInScene("Canvas_Gameplay").transform;
        WorldStateStore world = session.GetComponent<WorldStateStore>();
        InventoryService inventory = session.GetComponent<InventoryService>();

        RewardService rewards = session.GetComponent<RewardService>() ?? session.AddComponent<RewardService>();
        Set(rewards, "worldState", world);
        Set(rewards, "inventory", inventory);

        Transform story = canvas.Find("UI_StoryCard");
        Vector2 c = new Vector2(0.5f, 0.5f);

        if (canvas.Find("UI_Quiz") == null)
        {
            RectTransform holder = RT("UI_Quiz", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image panel = Img("Panel", holder, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.7f));
            Image win = Img("Window", panel.transform, c, c, Vector2.zero, new Vector2(1150, 720), new Color(0.07f, 0.07f, 0.09f, 0.97f));
            TMP_Text header = Txt("TXT_Header", win.transform, c, c, new Vector2(0, 310), new Vector2(1080, 50), 28, TextAlignmentOptions.Center, "Bệ Vấn Đáp");
            header.color = new Color(1f, 0.8f, 0.45f);
            TMP_Text question = Txt("TXT_Question", win.transform, c, c, new Vector2(0, 220), new Vector2(1060, 120), 32, TextAlignmentOptions.Center, "");
            Button[] options = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                options[i] = Btn($"BTN_Option{i}", win.transform, new Vector2(0, 100 - i * 82), new Vector2(1000, 70), "", 26);
                options[i].GetComponentInChildren<TMP_Text>().alignment = TextAlignmentOptions.Left;
                options[i].GetComponentInChildren<TMP_Text>().margin = new Vector4(24, 0, 24, 0);
            }
            TMP_Text explain = Txt("TXT_Explanation", win.transform, c, c, new Vector2(0, -250), new Vector2(1060, 110), 24, TextAlignmentOptions.Center, "");
            explain.color = new Color(1f, 0.9f, 0.6f);
            Button retry = Btn("BTN_Retry", win.transform, new Vector2(0, -320), new Vector2(240, 56), "Thử lại");
            QuizPanel quiz = holder.gameObject.AddComponent<QuizPanel>();
            Set(quiz, "worldState", world);
            Set(quiz, "panel", panel.gameObject);
            Set(quiz, "headerText", header);
            Set(quiz, "questionText", question);
            Set(quiz, "optionButtons", options);
            Set(quiz, "explanationText", explain);
            Set(quiz, "retryButton", retry);
            panel.gameObject.SetActive(false);
            if (story != null) holder.SetSiblingIndex(story.GetSiblingIndex());
        }

        if (canvas.Find("UI_Journal") == null)
        {
            RectTransform holder = RT("UI_Journal", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image panel = Img("Panel", holder, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.7f));
            Image win = Img("Window", panel.transform, c, c, Vector2.zero, new Vector2(1400, 800), new Color(0.08f, 0.07f, 0.06f, 0.97f));
            Txt("Title", win.transform, c, c, new Vector2(0, 360), new Vector2(1300, 50), 34, TextAlignmentOptions.Center, "Nhật ký  (J hoặc Esc để đóng)");
            TMP_Text objective = Txt("TXT_Objective", win.transform, c, c, new Vector2(-470, 150), new Vector2(380, 360), 24, TextAlignmentOptions.TopLeft, "");
            // Danh sách tri thức cuộn được.
            RectTransform view = RT("Viewport", win.transform, c, c, new Vector2(200, 10), new Vector2(880, 640));
            view.gameObject.AddComponent<RectMask2D>();
            Image viewBg = view.gameObject.AddComponent<Image>();
            viewBg.color = new Color(1, 1, 1, 0.03f);
            TMP_Text knowledge = Txt("TXT_Knowledge", view, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(-20, 600), 22, TextAlignmentOptions.TopLeft, "");
            knowledge.rectTransform.pivot = new Vector2(0.5f, 1f);
            knowledge.rectTransform.anchoredPosition = new Vector2(0, -8);
            ContentSizeFitter fit = knowledge.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            ScrollRect scroll = view.gameObject.AddComponent<ScrollRect>();
            scroll.content = knowledge.rectTransform;
            scroll.viewport = view;
            scroll.horizontal = false;
            scroll.scrollSensitivity = 30f;
            TMP_Text points = Txt("TXT_Points", win.transform, c, c, new Vector2(-470, -150), new Vector2(380, 120), 24, TextAlignmentOptions.TopLeft, "");
            points.color = new Color(1f, 0.85f, 0.5f);
            Button close = Btn("BTN_Close", win.transform, new Vector2(-470, -340), new Vector2(220, 56), "Đóng");
            JournalPanel journal = holder.gameObject.AddComponent<JournalPanel>();
            Set(journal, "worldState", world);
            Set(journal, "panel", panel.gameObject);
            Set(journal, "objectiveSource", FindInScene("TXT_QuestObjective").GetComponent<TMP_Text>());
            Set(journal, "objectiveText", objective);
            Set(journal, "knowledgeText", knowledge);
            Set(journal, "pointsText", points);
            Set(journal, "closeButton", close);
            panel.gameObject.SetActive(false);
            if (story != null) holder.SetSiblingIndex(story.GetSiblingIndex());
        }

        // Nhật ký liệt kê mọi bia đã có dữ liệu.
        List<LoreDefinition> lore = new List<LoreDefinition>();
        foreach (string g in AssetDatabase.FindAssets("t:LoreDefinition", new[] { "Assets/_Project/Data/Knowledge" }))
            lore.Add(AssetDatabase.LoadAssetAtPath<LoreDefinition>(AssetDatabase.GUIDToAssetPath(g)));
        lore.Sort((a, b) => string.CompareOrdinal(a.loreId, b.loreId));
        Set(canvas.Find("UI_Journal").GetComponent<JournalPanel>(), "allLore", lore.ToArray());

        EditorSceneManager.MarkSceneDirty(session.scene);
    }

    // ================= Chương 1: bia, cổng chương 2, lời kết =================
    [MenuItem("Tools/Son Doong/Chapter 1: Add Knowledge Content")]
    public static void Chapter1Content()
    {
        EditorSceneManager.OpenScene(Chapter1Scene);
        AddKnowledgeUI();
        GameObject session = FindInScene("GameSession");
        WorldStateStore world = session.GetComponent<WorldStateStore>();
        StoryCardPanel story = FindInScene("UI_StoryCard").GetComponent<StoryCardPanel>();

        Transform parent = FindInScene("Gameplay").transform;
        Transform steles = parent.Find("LoreSteles") ?? Empty("LoreSteles", parent, Vector3.zero).transform;
        PlaceStele("Lore_C1_SonDoong", steles, new Vector3(3.6f, 0f, 4.2f), new Vector3(0f, 0f, 0f), "c1_lore_1", world, story);
        PlaceStele("Lore_C1_DongSon", steles, new Vector3(6.8f, 0f, 47.6f), new Vector3(2f, 0f, 47.6f), "c1_lore_2", world, story);
        PlaceStele("Lore_C1_KimQuy", steles, new Vector3(-7.6f, 0f, 102.5f), new Vector3(0f, 0f, 102.5f), "c1_lore_3", world, story);

        // Lời kết và cổng sang chương 2 lấy giá trị mới trong code.
        RelicAltar altar = FindInScene("Altar_TrongDong").GetComponent<RelicAltar>();
        GameObject tmp = new GameObject("tmp");
        RelicAltar fresh = tmp.AddComponent<RelicAltar>();
        SerializedObject src = new SerializedObject(fresh), dst = new SerializedObject(altar);
        foreach (string p in new[] { "endBody", "nextScene", "nextCheckpoint" }) dst.CopyFromSerializedProperty(src.FindProperty(p));
        dst.ApplyModifiedPropertiesWithoutUndo();
        Object.DestroyImmediate(tmp);
        Set(FindInScene("ChapterExit_C1").GetComponent<ChapterExit>(), "storyPanel", story);

        EditorSceneManager.SaveScene(session.scene);
        Debug.Log("ChapterBuilder: đã thêm UI Tri thức, 3 bia và cập nhật lời kết chương 1.");
    }

    private static GameObject PlaceStele(string name, Transform parent, Vector3 pos, Vector3 faceTowards, string loreId, WorldStateStore world, StoryCardPanel story)
    {
        Transform existing = parent.Find(name);
        if (existing != null) Object.DestroyImmediate(existing.gameObject);
        Vector3 away = pos - faceTowards;
        float yaw = Mathf.Atan2(away.x, away.z) * Mathf.Rad2Deg;
        GameObject go = SharedPrefabBuilder.Place("PF_LoreStele", parent, pos, yaw, name);
        LoreStele stele = go.GetComponent<LoreStele>();
        Set(stele, "worldState", world);
        Set(stele, "lore", AssetDatabase.LoadAssetAtPath<LoreDefinition>($"{Knowledge}LR_{loreId}.asset"));
        Set(stele, "readPanel", story);
        return go;
    }

    // ================= Khuôn scene chương =================
    [MenuItem("Tools/Son Doong/Create Template_Chapter From 02")]
    public static void CreateTemplate()
    {
        AssetDatabase.DeleteAsset(TemplateScene);
        AssetDatabase.CopyAsset(Chapter1Scene, TemplateScene);
        var scene = EditorSceneManager.OpenScene(TemplateScene);

        Transform geo = FindInScene("World_Geometry").transform;
        for (int i = geo.childCount - 1; i >= 0; i--)
            if (geo.GetChild(i).name != "KillZone") Object.DestroyImmediate(geo.GetChild(i).gameObject);
        NavMeshSurface surface = geo.GetComponent<NavMeshSurface>();
        if (surface != null) surface.navMeshData = null;

        Transform gameplay = FindInScene("Gameplay").transform;
        for (int i = gameplay.childCount - 1; i >= 0; i--)
        {
            Transform t = gameplay.GetChild(i);
            if (t.name == "PrototypeRig") continue;
            if (t.name == "Checkpoint_CampSpawn") { t.name = "Spawn_Default"; continue; }
            Object.DestroyImmediate(t.gameObject);
        }

        Transform lighting = FindInScene("Lighting").transform;
        for (int i = lighting.childCount - 1; i >= 0; i--)
        {
            string n = lighting.GetChild(i).name;
            if (n != "Directional Light" && n != "Global Volume") Object.DestroyImmediate(lighting.GetChild(i).gameObject);
        }

        Transform canvas = FindInScene("Canvas_Gameplay").transform;
        foreach (string n in new[] { "TXT_SealProgress", "TXT_DrumPuzzleStatus", "TXT_MirrorStatus" })
        {
            Transform t = canvas.Find(n);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        GameObject session = FindInScene("GameSession");
        Object.DestroyImmediate(session.GetComponent<QuestTracker>());
        ChapterQuestTracker quest = session.AddComponent<ChapterQuestTracker>();
        Set(quest, "worldState", session.GetComponent<WorldStateStore>());
        Set(quest, "objectiveText", FindInScene("TXT_QuestObjective").GetComponent<TMP_Text>());
        SaveService save = session.GetComponent<SaveService>();
        foreach (string p in new[] { "campSeal", "lanternPickup", "drumAPickup", "drumBPickup", "drumCPickup", "mirrorReceiver" }) Set(save, p, null);
        Set(save, "checkpoints", new Object[0]);
        Set(save, "campSpawn", FindInScene("Spawn_Default").transform);

        EditorSceneManager.SaveScene(scene);
        Debug.Log("ChapterBuilder: đã tạo Template_Chapter.unity.");
    }

    // ================= Chương 2: Giếng Trời Chim Lạc =================
    [MenuItem("Tools/Son Doong/Build Chapter 2 Scene")]
    public static void BuildChapter2()
    {
        AssetDatabase.DeleteAsset(Chapter2Scene);
        AssetDatabase.CopyAsset(TemplateScene, Chapter2Scene);
        var scene = EditorSceneManager.OpenScene(Chapter2Scene);

        GameObject session = FindInScene("GameSession");
        WorldStateStore world = session.GetComponent<WorldStateStore>();
        InventoryService inventory = session.GetComponent<InventoryService>();
        RewardService rewards = session.GetComponent<RewardService>();
        SaveService save = session.GetComponent<SaveService>();
        QuizPanel quiz = FindInScene("UI_Quiz").GetComponent<QuizPanel>();
        StoryCardPanel story = FindInScene("UI_StoryCard").GetComponent<StoryCardPanel>();
        CraftingPanel crafting = FindInScene("UI_Crafting").GetComponent<CraftingPanel>();

        Transform geo = FindInScene("World_Geometry").transform;
        Transform gameplay = FindInScene("Gameplay").transform;
        Transform lighting = FindInScene("Lighting").transform;
        int env = Layer("Environment");
        Material floor = Mat("M_BO_Floor"), wall = Mat("M_BO_Wall"), boss = Mat("M_BO_Boss"), relic = Mat("M_BO_Relic"), tele = Mat("M_BO_Telegraph");

        // ---- An bắt đầu tại Spawn_C2 ----
        Transform spawn = FindInScene("Spawn_Default").transform;
        spawn.position = new Vector3(0, 0.2f, -6f);
        spawn.rotation = Quaternion.identity;
        Transform an = FindInScene("PF_An_Prototype").transform;
        an.SetPositionAndRotation(spawn.position, Quaternion.identity);

        // ---- P1 Đáy Giếng Gió ----
        Transform p1 = Empty("Room_P1_DayGieng", geo, Vector3.zero).transform;
        BoxMinMax("Floor_P1", p1, new Vector3(-8, -1, -8), new Vector3(8, 0, 8), floor, env);
        BoxMinMax("Wall_P1_West", p1, new Vector3(-8.5f, 0, -8.5f), new Vector3(-8, 20, 8.5f), wall, env);
        BoxMinMax("Wall_P1_East", p1, new Vector3(8, 0, -8.5f), new Vector3(8.5f, 20, 8.5f), wall, env);
        BoxMinMax("Wall_P1_South", p1, new Vector3(-8.5f, 0, -8.5f), new Vector3(8.5f, 20, -8), wall, env);
        BoxMinMax("Wall_P1_NorthLeft", p1, new Vector3(-8.5f, 0, 8), new Vector3(3.5f, 20, 8.5f), wall, env);
        BoxMinMax("Wall_P1_NorthRight", p1, new Vector3(6.5f, 0, 8), new Vector3(8.5f, 20, 8.5f), wall, env);
        BoxMinMax("Wall_P1_NorthUnder", p1, new Vector3(3.5f, 0, 8), new Vector3(6.5f, 5, 8.5f), wall, env);
        BoxMinMax("Wall_P1_NorthOver", p1, new Vector3(3.5f, 9, 8), new Vector3(6.5f, 20, 8.5f), wall, env);
        BoxMinMax("Ledge_P1", p1, new Vector3(2, 0, 5), new Vector3(8, 5, 8), wall, env);
        BoxMinMax("Floor_P1_P2", p1, new Vector3(3.5f, 4, 8), new Vector3(6.5f, 5, 10), floor, env);
        PointLight("Light_P1_Sun", lighting, new Vector3(0, 12, 0), new Color(1f, 0.95f, 0.75f), 22, 5);

        Place("PF_WindColumn", gameplay, new Vector3(5f, 0f, 3.5f), "WindColumn_P1_Tut", world, 0f, 6f, true, "", "");
        GameObject campfire = SharedPrefabBuilder.Place("PF_CheckpointAltar", gameplay, new Vector3(-5f, 0f, -5f), 0f, "CP_C2_Start");
        ConfigureCheckpoint(campfire, save, "c2_cp_start", "lửa trại đáy giếng", new Vector3(-3.5f, 0.2f, -5f), world, null, crafting);
        GameObject feather = SharedPrefabBuilder.Place("PF_AbilityPickup", gameplay, new Vector3(0f, 1f, -2f), 0f, "Pickup_LongChimLac");
        Set(feather.GetComponent<AbilityPickup>(), "worldState", world);
        Set(feather.GetComponent<AbilityPickup>(), "storyPanel", story);
        Set(feather.GetComponent<AbilityPickup>(), "card", AssetDatabase.LoadAssetAtPath<StoryCard>(C2Data + "SC_C2_LongChimLac.asset"));
        Transform steles = Empty("LoreSteles", gameplay, Vector3.zero).transform;
        PlaceStele("Lore_C2_P1", steles, new Vector3(-7.4f, 0f, 2f), new Vector3(0f, 0f, 2f), "c2_lore_p1", world, story);
        Pedestal("QuizPedestal_C2_P1", gameplay, new Vector3(6f, 5f, 7f), "c2_ped_p1", "c2_qz_p1", world, inventory, rewards, quiz, (RewardType.Fragment, "c2_frag_1", 1));
        ItemPickup("Pickup_c2_la_cam_01", gameplay, new Vector3(-6f, 0.35f, 4f), "c2_pickup_la_cam_01", InventoryService.LaCam, world, inventory);
        ItemPickup("Pickup_c2_linh_chi_01", gameplay, new Vector3(-3f, 0.35f, 6f), "c2_pickup_linh_chi_01", InventoryService.LinhChi, world, inventory);
        TextMeshPro sign = WorldText("Sign_Tutorial", gameplay, new Vector3(0f, 2.6f, -7.9f), new Vector2(7f, 2.5f),
            "Quà Kim Quy\nCtrl + hướng: lộn né  •  Lộn rồi chém: đâm tới\nChuột giữa: khóa mục tiêu  •  Space khi lơ lửng: nhảy", 0.5f, new Color(1f, 0.9f, 0.6f));
        sign.transform.rotation = Quaternion.Euler(0, 180, 0);

        // ---- P2 Vách Gió ----
        Transform p2 = Empty("Room_P2_VachGio", geo, Vector3.zero).transform;
        BoxMinMax("Floor_P2", p2, new Vector3(-1, 4, 10), new Vector3(11, 5, 22), floor, env);
        BoxMinMax("Wall_P2_West", p2, new Vector3(-1.5f, 0, 9.5f), new Vector3(-1, 23, 22.5f), wall, env);
        BoxMinMax("Wall_P2_East", p2, new Vector3(11, 0, 9.5f), new Vector3(11.5f, 23, 22.5f), wall, env);
        BoxMinMax("Wall_P2_SouthLeft", p2, new Vector3(-1.5f, 0, 9.5f), new Vector3(3.5f, 23, 10), wall, env);
        BoxMinMax("Wall_P2_SouthRight", p2, new Vector3(6.5f, 0, 9.5f), new Vector3(11.5f, 23, 10), wall, env);
        BoxMinMax("Wall_P2_SouthOver", p2, new Vector3(3.5f, 9, 9.5f), new Vector3(6.5f, 23, 10), wall, env);
        BoxMinMax("Wall_P2_NorthLeft", p2, new Vector3(-1.5f, 0, 22), new Vector3(7.75f, 23, 22.5f), wall, env);
        BoxMinMax("Wall_P2_NorthRight", p2, new Vector3(10.75f, 0, 22), new Vector3(11.5f, 23, 22.5f), wall, env);
        BoxMinMax("Wall_P2_NorthUnder", p2, new Vector3(7.75f, 0, 22), new Vector3(10.75f, 13, 22.5f), wall, env);
        BoxMinMax("Wall_P2_NorthOver", p2, new Vector3(7.75f, 17, 22), new Vector3(10.75f, 23, 22.5f), wall, env);
        BoxMinMax("Plat_P2_A", p2, new Vector3(1, 5, 12), new Vector3(6, 9, 17), wall, env);
        BoxMinMax("Plat_P2_B", p2, new Vector3(7, 5, 15), new Vector3(11, 13, 19), wall, env);
        BoxMinMax("Floor_P2_P3", p2, new Vector3(7.75f, 12, 19), new Vector3(10.75f, 13, 24.5f), floor, env);
        PointLight("Light_P2", lighting, new Vector3(5, 16, 16), new Color(1f, 0.95f, 0.8f), 18, 4);
        Place("PF_WindColumn", gameplay, new Vector3(7.6f, 5f, 12.6f), "WindColumn_P2_1", world, 5f, 10f, true, "", "");
        Place("PF_WindColumn", gameplay, new Vector3(5.4f, 9f, 16.2f), "WindColumn_P2_2", world, 9f, 14f, true, "", "");
        PlaceStele("Lore_C2_P2", steles, new Vector3(1.8f, 9f, 14.5f), new Vector3(6f, 9f, 14.5f), "c2_lore_p2", world, story);
        Pedestal("QuizPedestal_C2_P2", gameplay, new Vector3(9.5f, 13f, 17.8f), "c2_ped_p2", "c2_qz_p2", world, inventory, rewards, quiz, (RewardType.Fragment, "c2_frag_2", 1));

        // ---- P3 Nhà Sàn Đá Treo ----
        Transform p3 = Empty("Room_P3_NhaSan", geo, Vector3.zero).transform;
        BoxMinMax("Floor_P3", p3, new Vector3(-2, 12, 24), new Vector3(12, 13, 38), floor, env);
        BoxMinMax("Wall_P3_West", p3, new Vector3(-2.5f, 0, 23.5f), new Vector3(-2, 31, 38.5f), wall, env);
        BoxMinMax("Wall_P3_East", p3, new Vector3(12, 0, 23.5f), new Vector3(12.5f, 31, 38.5f), wall, env);
        BoxMinMax("Wall_P3_SouthLeft", p3, new Vector3(-2.5f, 0, 23.5f), new Vector3(7.75f, 31, 24), wall, env);
        BoxMinMax("Wall_P3_SouthRight", p3, new Vector3(10.75f, 0, 23.5f), new Vector3(12.5f, 31, 24), wall, env);
        BoxMinMax("Wall_P3_SouthOver", p3, new Vector3(7.75f, 17, 23.5f), new Vector3(10.75f, 31, 24), wall, env);
        BoxMinMax("Wall_P3_NorthLeft", p3, new Vector3(-2.5f, 0, 38), new Vector3(3, 31, 38.5f), wall, env);
        BoxMinMax("Wall_P3_NorthRight", p3, new Vector3(7, 0, 38), new Vector3(12.5f, 31, 38.5f), wall, env);
        BoxMinMax("Wall_P3_NorthUnder", p3, new Vector3(3, 0, 38), new Vector3(7, 17, 38.5f), wall, env);
        BoxMinMax("Wall_P3_NorthOver", p3, new Vector3(3, 22, 38), new Vector3(7, 31, 38.5f), wall, env);
        BoxMinMax("Ledge_P3", p3, new Vector3(2, 13, 35), new Vector3(8, 18, 38), wall, env);
        PointLight("Light_P3", lighting, new Vector3(5, 20, 30), new Color(1f, 0.85f, 0.6f), 20, 4);

        Vector3[] houses = { new Vector3(0.5f, 13f, 29f), new Vector3(9.5f, 13f, 29.5f), new Vector3(0.5f, 13f, 34.5f) };
        SequencePuzzleDefinition bells = AssetDatabase.LoadAssetAtPath<SequencePuzzleDefinition>(C2Data + "SQ_C2_Bells.asset");
        GameObject puzzleRoot = Empty("Puzzle_C2_Bells", gameplay, new Vector3(5f, 13f, 30f));
        TextMeshPro bellStatus = WorldText("Status", puzzleRoot.transform, new Vector3(0, 4.5f, 2f), new Vector2(4f, 1f), "0/3", 1f, Color.white);
        SequencePuzzleController seq = puzzleRoot.AddComponent<SequencePuzzleController>();
        Set(seq, "worldState", world);
        Set(seq, "definition", bells);
        Set(seq, "statusText", bellStatus);
        for (int i = 0; i < 3; i++)
        {
            Vector3 h = houses[i];
            Transform house = Empty($"House_{i + 1}", p3, h).transform;
            Box("Floor", house, h + new Vector3(0, 1.35f, 0), new Vector3(3f, 0.3f, 3f), wall, env);
            foreach (Vector2 o in new[] { new Vector2(-1.3f, -1.3f), new Vector2(1.3f, -1.3f), new Vector2(-1.3f, 1.3f), new Vector2(1.3f, 1.3f) })
                Box("Post", house, h + new Vector3(o.x, 0.6f, o.y), new Vector3(0.3f, 1.2f, 0.3f), wall, env);
            GameObject bell = SharedPrefabBuilder.Place("PF_SequenceBell", puzzleRoot.transform, h + new Vector3(0, 1.5f, 0), 0f, $"Bell_{i + 1}");
            SequenceInputNode node = bell.GetComponent<SequenceInputNode>();
            Set(node, "controller", seq);
            Set(node, "nodeId", $"bell_{i + 1}");
            Set(node, "prompt", $"Gõ chuông chim số {i + 1}");
            TextMeshPro label = WorldText("Label", bell.transform, new Vector3(0, 2.3f, 0), new Vector2(1.5f, 0.6f), $"Chim {i + 1}", 0.8f, new Color(1f, 0.9f, 0.5f));
            label.gameObject.AddComponent<FaceCameraBillboard>();
        }
        // Phù điêu thứ tự: chỉ hiện khi rọi đèn.
        GameObject relief = Empty("Relief_C2_Order", gameplay, new Vector3(5f, 13f, 34.9f));
        Box("Slab", relief.transform, new Vector3(5f, 15.5f, 34.95f), new Vector3(4f, 2f, 0.1f), Mat("M_BO_Wall"), 0, false);
        GameObject reliefContent = new GameObject("RevealContent");
        reliefContent.transform.SetParent(relief.transform, false);
        TextMeshPro reliefText = WorldText("Text", reliefContent.transform, new Vector3(0f, 2.5f, -0.12f), new Vector2(3.8f, 1.8f), "Chim 2  →  Chim 3  →  Chim 1", 1f, new Color(1f, 0.85f, 0.4f));
        reliefText.transform.position = new Vector3(5f, 15.5f, 34.85f);
        GameObject reliefPoint = Empty("RevealPoint", relief.transform, new Vector3(5f, 15.5f, 34.7f));
        LanternRevealSurface reliefReveal = relief.AddComponent<LanternRevealSurface>();
        Set(reliefReveal, "revealContent", reliefContent);
        Set(reliefReveal, "revealPoint", reliefPoint.transform);
        Set(reliefReveal, "revealRange", 6f);

        GameObject finalColumn = Place("PF_WindColumn", gameplay, new Vector3(5f, 13f, 32.8f), "WindColumn_P3_Final", world, 13f, 19f, false, "c2_wind_p3_final", "");
        AddListener(seq, "onSolved", finalColumn.GetComponent<WindColumn>().Activate);
        GameObject cpMid = SharedPrefabBuilder.Place("PF_CheckpointAltar", gameplay, new Vector3(6.2f, 13f, 25.4f), 0f, "CP_C2_Mid");
        ConfigureCheckpoint(cpMid, save, "c2_cp_mid", "bàn thờ nhà sàn", new Vector3(9.2f, 13.2f, 26f), world, null, null);
        PlaceStele("Lore_C2_P3", steles, new Vector3(11.4f, 13f, 31f), new Vector3(5f, 13f, 31f), "c2_lore_p3", world, story);
        Pedestal("QuizPedestal_C2_P3", gameplay, new Vector3(6.6f, 18f, 36.6f), "c2_ped_p3", "c2_qz_p3", world, inventory, rewards, quiz, (RewardType.Fragment, "c2_frag_3", 1));
        ItemPickup("Pickup_c2_linh_chi_02", gameplay, new Vector3(10.5f, 13.35f, 36.5f), "c2_pickup_linh_chi_02", InventoryService.LinhChi, world, inventory);

        // ---- P4 Cầu Mây Gãy ----
        Transform p4 = Empty("Room_P4_CauMay", geo, Vector3.zero).transform;
        float[][] segments = { new[] { 38f, 48f }, new[] { 49.5f, 56f }, new[] { 57.5f, 64f } };
        for (int i = 0; i < segments.Length; i++)
        {
            float z0 = segments[i][0], z1 = segments[i][1];
            BoxMinMax($"Bridge_{i + 1}", p4, new Vector3(3, 17, z0), new Vector3(7, 18, z1), floor, env);
            bool ledgeSegment = i == 1;
            if (!ledgeSegment)
            {
                BoxMinMax($"Rail_L_{i + 1}", p4, new Vector3(3, 18, z0), new Vector3(3.2f, 18.6f, z1), wall, env);
                BoxMinMax($"Rail_R_{i + 1}", p4, new Vector3(6.8f, 18, z0), new Vector3(7, 18.6f, z1), wall, env);
            }
            else
            {
                // Chừa lối lên cột gió ở đoạn có gờ.
                BoxMinMax("Rail_L_2a", p4, new Vector3(3, 18, z0), new Vector3(3.2f, 18.6f, 50f), wall, env);
                BoxMinMax("Rail_L_2b", p4, new Vector3(3, 18, 53f), new Vector3(3.2f, 18.6f, z1), wall, env);
                BoxMinMax("Rail_R_2a", p4, new Vector3(6.8f, 18, z0), new Vector3(7, 18.6f, 50f), wall, env);
                BoxMinMax("Rail_R_2b", p4, new Vector3(6.8f, 18, 53f), new Vector3(7, 18.6f, z1), wall, env);
            }
        }
        Boundary(p4, new Vector3(0.5f, 18, 38), new Vector3(1f, 24, 50));
        Boundary(p4, new Vector3(0.5f, 18, 53), new Vector3(1f, 24, 73));
        Boundary(p4, new Vector3(9f, 18, 38), new Vector3(9.5f, 24, 50));
        Boundary(p4, new Vector3(9f, 18, 53), new Vector3(9.5f, 24, 73));
        BoxMinMax("Ledge_P4_L", p4, new Vector3(-2, 15, 50), new Vector3(1, 21, 53), wall, env);
        BoxMinMax("Ledge_P4_R", p4, new Vector3(9, 15, 50), new Vector3(12, 21, 53), wall, env);
        Boundary(p4, new Vector3(-2.5f, 21, 49.5f), new Vector3(-2f, 24, 53.5f));
        Boundary(p4, new Vector3(-2.5f, 21, 49.5f), new Vector3(1f, 24, 50f));
        Boundary(p4, new Vector3(-2.5f, 21, 53f), new Vector3(1f, 24, 53.5f));
        Boundary(p4, new Vector3(12f, 21, 49.5f), new Vector3(12.5f, 24, 53.5f));
        Boundary(p4, new Vector3(9f, 21, 49.5f), new Vector3(12.5f, 24, 50f));
        Boundary(p4, new Vector3(9f, 21, 53f), new Vector3(12.5f, 24, 53.5f));
        Place("PF_WindColumn", gameplay, new Vector3(2.4f, 18f, 51.5f), "WindColumn_P4_L", world, 18f, 22f, true, "", "");
        Place("PF_WindColumn", gameplay, new Vector3(7.6f, 18f, 51.5f), "WindColumn_P4_R", world, 18f, 22f, true, "", "");
        BoxMinMax("Floor_P4_Yard", p4, new Vector3(1, 17, 64), new Vector3(9, 18, 73.5f), floor, env);
        PointLight("Light_P4_Bridge", lighting, new Vector3(5, 23, 51), new Color(0.9f, 0.9f, 1f), 18, 3);
        PointLight("Light_P4_Yard", lighting, new Vector3(5, 22, 68), new Color(1f, 0.9f, 0.7f), 12, 3);

        Transform enemies = Empty("Enemies", gameplay, Vector3.zero).transform;
        Thrower("HacAnhNem_L", enemies, new Vector3(-0.5f, 21f, 51.5f));
        Thrower("HacAnhNem_R", enemies, new Vector3(10.5f, 21f, 51.5f));
        Shade("HacAnh_C2_1", enemies, new Vector3(5f, 18f, 41f), new Vector3(5f, 18f, 40f), new Vector3(5f, 18f, 47f));
        // Đoạn cầu 2 (không đặt ở đoạn 3: quá gần mốc c2_cp_arena, hồi sinh là bị đánh ngay).
        Shade("HacAnh_C2_2", enemies, new Vector3(5f, 18f, 53.5f), new Vector3(5f, 18f, 50.5f), new Vector3(5f, 18f, 55.2f));

        PlaceStele("Lore_C2_P4", steles, new Vector3(1.8f, 18f, 67f), new Vector3(5f, 18f, 67f), "c2_lore_p4", world, story);
        Pedestal("QuizPedestal_C2_P4", gameplay, new Vector3(7.8f, 18f, 67f), "c2_ped_p4", "c2_qz_p4", world, inventory, rewards, quiz,
            (RewardType.BossTool, "c2_tool_long_gio_lon", 1), (RewardType.Item, InventoryService.ThuocMau, 2));
        GameObject cpArena = SharedPrefabBuilder.Place("PF_CheckpointTrigger", gameplay, new Vector3(5f, 18f, 65f), 0f, "CP_C2_Arena");
        Set(cpArena.GetComponent<CheckpointTrigger>(), "saveService", save);
        Set(cpArena.GetComponent<CheckpointTrigger>(), "checkpointId", "c2_cp_arena");
        cpArena.transform.Find("SpawnPoint").position = new Vector3(5f, 18.2f, 66.5f);

        // Cổng đấu trường (Z = 72).
        BoxMinMax("Wall_ArenaGate_L", p4, new Vector3(1, 18, 71.8f), new Vector3(3.5f, 23, 72.2f), wall, env);
        BoxMinMax("Wall_ArenaGate_R", p4, new Vector3(6.5f, 18, 71.8f), new Vector3(9, 23, 72.2f), wall, env);
        BoxMinMax("Wall_ArenaGate_Top", p4, new Vector3(3.5f, 22, 71.8f), new Vector3(6.5f, 23, 72.2f), wall, env);
        GameObject door = SharedPrefabBuilder.Place("PF_RequirementDoor", gameplay, new Vector3(5f, 18f, 72f), 0f, "Door_C2_Arena");
        RequirementDoor rd = door.GetComponent<RequirementDoor>();
        Set(rd, "worldState", world);
        Set(rd, "doorId", "c2_door_arena");
        Set(rd, "doorName", "cổng Đỉnh Tổ Chim");
        Set(rd, "saveService", save);
        Set(rd, "saveCheckpointId", "c2_cp_arena");
        SetRequirements(rd, "requirements", (StateSet.Fragments, "c2_frag_1", "mảnh 1"), (StateSet.Fragments, "c2_frag_2", "mảnh 2"),
            (StateSet.Fragments, "c2_frag_3", "mảnh 3"), (StateSet.BossTools, "c2_tool_long_gio_lon", "Lông Gió Lớn"));
        GameObject lockGate = Box("Gate_C2_Back", gameplay, new Vector3(5f, 20f, 72.6f), new Vector3(3f, 4f, 0.4f), boss, env);
        lockGate.SetActive(false);

        // ---- P5 Đỉnh Tổ Chim ----
        Transform p5 = Empty("Room_P5_DinhTo", geo, Vector3.zero).transform;
        Vector3 center = new Vector3(5f, 18f, 82f);
        GameObject arenaFloor = Prim(PrimitiveType.Cylinder, "Floor_P5", p5, Vector3.zero, new Vector3(18f, 0.5f, 18f), floor, env, false);
        arenaFloor.transform.position = center + Vector3.down * 0.5f;
        arenaFloor.AddComponent<MeshCollider>();
        arenaFloor.isStatic = true;
        for (int i = 0; i < 24; i++)
        {
            float a = i * 15f;
            if (Mathf.Abs(Mathf.DeltaAngle(a, 0f)) < 10f || Mathf.Abs(Mathf.DeltaAngle(a, 180f)) < 10f || Mathf.Abs(Mathf.DeltaAngle(a, 270f)) < 12f) continue;
            float rad = a * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad));
            GameObject rim = Box($"Rim_{i}", p5, center + dir * 9.25f + Vector3.up * 0.5f, new Vector3(0.5f, 1f, 2.5f), wall, env);
            rim.transform.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, dir));
            rim.transform.rotation = Quaternion.LookRotation(new Vector3(-dir.z, 0, dir.x));
            GameObject bound = new GameObject($"Boundary_{i}");
            bound.transform.SetParent(p5, false);
            bound.transform.SetPositionAndRotation(center + dir * 9.6f + Vector3.up * 3f, rim.transform.rotation);
            bound.layer = env;
            BoxCollider bc = bound.AddComponent<BoxCollider>();
            bc.size = new Vector3(0.4f, 6f, 2.6f);
        }
        Transform[] perchTops = new Transform[3];
        float[] perchAngles = { 90f, 210f, 330f };
        for (int i = 0; i < 3; i++)
        {
            float rad = perchAngles[i] * Mathf.Deg2Rad;
            Vector3 pos = center + new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * 7f;
            Box($"Perch_{i + 1}", p5, pos + Vector3.up * 3f, new Vector3(1.5f, 6f, 1.5f), wall, env);
            perchTops[i] = Empty($"PerchTop_{i + 1}", p5, pos + Vector3.up * 6f).transform;
        }
        PointLight("Light_P5_Sun", lighting, center + Vector3.up * 10f, new Color(1f, 0.92f, 0.7f), 24, 5);
        Place("PF_WindColumn", gameplay, center, "WindColumn_Arena", world, 18f, 24.5f, true, "", "c2_tool_long_gio_lon");

        // Trùm Chim Lạc.
        GameObject chim = new GameObject("Boss_ChimLac");
        chim.transform.SetParent(gameplay, false);
        chim.transform.position = perchTops[0].position + Vector3.up * 0.6f;
        chim.layer = Layer("Enemy");
        CapsuleCollider cc = chim.AddComponent<CapsuleCollider>();
        cc.radius = 0.9f;
        cc.height = 2.2f;
        cc.center = new Vector3(0, 0.6f, 0);
        GameObject chimBody = Prim(PrimitiveType.Cube, "Body", chim.transform, new Vector3(0, 0.6f, 0), new Vector3(1.5f, 1f, 2f), boss, 0, false);
        Prim(PrimitiveType.Cube, "Wing_L", chim.transform, new Vector3(-1.6f, 0.8f, 0), new Vector3(1.8f, 0.1f, 1.2f), boss, 0, false);
        Prim(PrimitiveType.Cube, "Wing_R", chim.transform, new Vector3(1.6f, 0.8f, 0), new Vector3(1.8f, 0.1f, 1.2f), boss, 0, false);
        Prim(PrimitiveType.Cube, "Beak", chim.transform, new Vector3(0, 0.8f, 1.3f), new Vector3(0.3f, 0.3f, 0.8f), relic, 0, false);
        GameObject chimAim = Empty("AimPoint", chim.transform, chim.transform.position + Vector3.up * 0.7f);
        GameObject focusBox = Empty("FocusHitbox", chim.transform, chim.transform.position + Vector3.up * 0.7f);
        focusBox.layer = Layer("LanternTarget");
        focusBox.AddComponent<SphereCollider>().radius = 1.3f;
        ChimLacFocusReceiver focus = focusBox.AddComponent<ChimLacFocusReceiver>();
        GameObject gust = Prim(PrimitiveType.Sphere, "GustTelegraph", chim.transform, new Vector3(0, 0.6f, 0), Vector3.one * 5f, Mat("M_BO_Mechanic"), 0, false);
        GameObject dive = Box("DiveTelegraph_C2", gameplay, center, Vector3.one, tele, 0, false);
        EnemyHealth chimHealth = chim.AddComponent<EnemyHealth>();
        Set(chimHealth, "definition", AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Project/Data/Enemies/ED_ChimLac.asset"));
        BossChimLac chimBoss = chim.AddComponent<BossChimLac>();
        Set(chimBoss, "worldState", world);
        Set(chimBoss, "bossId", "c2_boss_chim_lac");
        Set(chimBoss, "displayName", "Chim Lạc");
        Set(chimBoss, "saveOnDefeat", save);
        Set(chimBoss, "defeatCheckpointId", "c2_cp_arena");
        Transform canvas = FindInScene("Canvas_Gameplay").transform;
        Transform bar = canvas.Find("BossBar");
        Set(chimBoss, "bossBar", bar.gameObject);
        Set(chimBoss, "bossBarFill", bar.Find("Fill").GetComponent<RectTransform>());
        Set(chimBoss, "bossBarLabel", bar.Find("Label").GetComponent<TMP_Text>());
        Set(chimBoss, "perchTops", perchTops);
        Set(chimBoss, "arenaCenter", Empty("ArenaCenter_C2", p5, center).transform);
        Set(chimBoss, "focus", focus);
        Set(chimBoss, "diveTelegraph", dive.transform);
        Set(chimBoss, "gustTelegraph", gust);
        Set(focus, "boss", chimBoss);
        Set(focus, "aimPoint", chimAim.transform);

        GameObject arenaTrigger = Empty("ArenaGate_C2", gameplay, new Vector3(5f, 19.5f, 75f));
        arenaTrigger.layer = Layer("Ignore Raycast");
        BoxCollider at = arenaTrigger.AddComponent<BoxCollider>();
        at.isTrigger = true;
        at.size = new Vector3(6f, 3f, 2f);
        Rigidbody arb = arenaTrigger.AddComponent<Rigidbody>();
        arb.isKinematic = true;
        arb.useGravity = false;
        ArenaGate gateComp = arenaTrigger.AddComponent<ArenaGate>();
        Set(gateComp, "worldState", world);
        Set(gateComp, "boss", chimBoss);
        Set(gateComp, "gateBehind", lockGate);
        Set(gateComp, "bossId", "c2_boss_chim_lac");

        GameObject altar = SharedPrefabBuilder.Place("PF_RelicAltar", gameplay, new Vector3(5f, 18f, 89f), 0f, "RelicAltar_C2");
        RelicAltar ra = altar.GetComponent<RelicAltar>();
        Set(ra, "worldState", world);
        Set(ra, "saveService", save);
        Set(ra, "storyPanel", story);
        Set(ra, "fragmentIds", new[] { "c2_frag_1", "c2_frag_2", "c2_frag_3" });
        Set(ra, "bossId", "c2_boss_chim_lac");
        Set(ra, "bossName", "Chim Lạc");
        Set(ra, "restoredId", "c2_relic_restored");
        Set(ra, "relicName", "Mặt Trời Lông Chim");
        Set(ra, "grantAbility", "");
        Set(ra, "checkpointId", "c2_cp_arena");
        Set(ra, "endsChapter", false);

        GameObject linhThan = Empty("LinhThan_C2", gameplay, new Vector3(9f, 18f, 86.5f));
        GameObject linhVisual = Prim(PrimitiveType.Cube, "Visual", linhThan.transform, new Vector3(0, 1.2f, 0), new Vector3(1.2f, 1.2f, 1.6f), relic, Layer("Interactable"), true);
        QuizSequence qs = linhThan.AddComponent<QuizSequence>();
        Set(qs, "worldState", world);
        Set(qs, "quizPanel", quiz);
        Set(qs, "storyPanel", story);
        Set(qs, "sequenceId", "c2_linhthan_done");
        Set(qs, "requiredPuzzleId", "c2_relic_restored");
        Set(qs, "speakerName", "Chim Lạc");
        Set(qs, "quizzes", new Object[]
        {
            AssetDatabase.LoadAssetAtPath<QuizDefinition>(Knowledge + "QZ_c2_qz_lt1.asset"),
            AssetDatabase.LoadAssetAtPath<QuizDefinition>(Knowledge + "QZ_c2_qz_lt2.asset"),
            AssetDatabase.LoadAssetAtPath<QuizDefinition>(Knowledge + "QZ_c2_qz_lt3.asset")
        });
        Set(qs, "completionCard", AssetDatabase.LoadAssetAtPath<StoryCard>(C2Data + "SC_C2_Awaken.asset"));
        Set(qs, "visual", linhVisual);
        Set(qs, "saveService", save);
        Set(qs, "saveCheckpointId", "c2_cp_arena");

        GameObject exit = SharedPrefabBuilder.Place("PF_ChapterExit", gameplay, new Vector3(-1.4f, 18f, 82f), 90f, "ChapterExit_C2");
        ChapterExit ce = exit.GetComponent<ChapterExit>();
        Set(ce, "worldState", world);
        Set(ce, "requiredPuzzleId", "c2_linhthan_done");
        Set(ce, "nextScene", "04_ThanhDaSonTinh");
        Set(ce, "nextCheckpointId", "c3_cp_start");
        Set(ce, "label", "Đi theo cổng gió xuống Thành Đá (chương 3)");
        Set(ce, "storyPanel", story);
        Set(ce, "endTitle", "Chương 2 hoàn thành – Giếng Trời Chim Lạc");
        Set(ce, "endBody", "Chim Lạc đã thức tỉnh, giếng trời không còn bị Hắc Sương bịt kín.\n\nChương 3 Thành Đá Sơn Tinh đang được xây dựng. Tiến trình đã được lưu; chọn Tiếp tục ở menu để quay lại đỉnh tổ.");
        PlaceStele("Lore_C2_P5", steles, center + new Vector3(6.6f, 0f, 3.8f), center, "c2_lore_p5", world, story);

        // ---- KillZone, nhiệm vụ, NavMesh ----
        GameObject kill = FindInScene("KillZone");
        kill.transform.position = new Vector3(5f, -10f, 40f);
        kill.GetComponent<BoxCollider>().size = new Vector3(60f, 4f, 120f);

        ChapterQuestTracker quest = session.GetComponent<ChapterQuestTracker>();
        Set(quest, "doneText", "Chương 2 hoàn thành.\nĐi qua cổng gió phía tây đỉnh tổ.");
        SetQuestSteps(quest,
            ("Nhận Lông Chim Lạc ở giữa đáy giếng.", StateSet.Abilities, "ability_wind"),
            ("Bước vào cột gió lên gờ,\ntrả lời Bệ Vấn Đáp lấy mảnh 1.", StateSet.Fragments, "c2_frag_1"),
            ("Vượt Vách Gió bằng hai cột gió,\nlấy mảnh 2 trên bệ cao.", StateSet.Fragments, "c2_frag_2"),
            ("Rọi đèn đọc phù điêu,\ngõ chuông gió theo thứ tự.", StateSet.Puzzles, "c2_pz_chuong_gio"),
            ("Lên gờ bằng cột gió cuối, lấy mảnh 3.", StateSet.Fragments, "c2_frag_3"),
            ("Qua Cầu Mây Gãy, nhận Lông Gió Lớn.", StateSet.BossTools, "c2_tool_long_gio_lon"),
            ("Giải thoát Chim Lạc: chờ chim đậu,\nlên cột gió giữa và rọi đèn.", StateSet.Bosses, "c2_boss_chim_lac"),
            ("Phục dựng Mặt Trời Lông Chim tại bệ.", StateSet.Puzzles, "c2_relic_restored"),
            ("Trả lời Chim Lạc.", StateSet.Puzzles, "c2_linhthan_done"));

        NavMeshSurface surface = geo.GetComponent<NavMeshSurface>();
        surface.BuildNavMesh();
        string navPath = Scenes + "03_GiengTroiChimLac_NavMesh.asset";
        AssetDatabase.DeleteAsset(navPath);
        AssetDatabase.CreateAsset(surface.navMeshData, navPath);

        EditorSceneManager.SaveScene(scene);
        AddToBuild(Chapter2Scene);
        Debug.Log("ChapterBuilder: đã dựng 03_GiengTroiChimLac.");
    }

    // ---------- Trợ giúp ----------
    private static GameObject Place(string prefab, Transform parent, Vector3 pos, string name, WorldStateStore world,
        float floorY, float hoverTop, bool startsActive, string columnId, string bossTool)
    {
        GameObject go = SharedPrefabBuilder.Place(prefab, parent, pos, 0f, name);
        WindColumn w = go.GetComponent<WindColumn>();
        Set(w, "worldState", world);
        Set(w, "hoverTopY", hoverTop);
        Set(w, "startsActive", startsActive);
        Set(w, "columnId", columnId);
        Set(w, "requiredBossTool", bossTool);
        float height = hoverTop - floorY + 1f;
        CapsuleCollider c = go.GetComponent<CapsuleCollider>();
        c.height = height;
        c.center = new Vector3(0, height * 0.5f, 0);
        Transform v = go.transform.Find("Visual");
        v.localPosition = new Vector3(0, height * 0.5f, 0);
        v.localScale = new Vector3(2.2f, height * 0.5f, 2.2f);
        return go;
    }

    private static void ConfigureCheckpoint(GameObject go, SaveService save, string id, string displayName, Vector3 spawnPos,
        WorldStateStore world, string requiredBoss, CraftingPanel crafting)
    {
        CheckpointAltar cp = go.GetComponent<CheckpointAltar>();
        Set(cp, "saveService", save);
        Set(cp, "checkpointId", id);
        Set(cp, "displayName", displayName);
        Set(cp, "worldState", world);
        Set(cp, "requiredBossId", requiredBoss ?? "");
        Set(cp, "craftingPanel", crafting);
        Transform sp = go.transform.Find("SpawnPoint");
        sp.position = spawnPos;
        sp.rotation = Quaternion.identity;
    }

    private static void Pedestal(string name, Transform parent, Vector3 pos, string pedestalId, string quizId, WorldStateStore world,
        InventoryService inventory, RewardService rewards, QuizPanel quiz, params (RewardType, string, int)[] items)
    {
        GameObject go = SharedPrefabBuilder.Place("PF_QuizPedestal", parent, pos, 0f, name);
        QuizPedestal p = go.GetComponent<QuizPedestal>();
        Set(p, "worldState", world);
        Set(p, "inventory", inventory);
        Set(p, "rewardService", rewards);
        Set(p, "quizPanel", quiz);
        Set(p, "pedestalId", pedestalId);
        Set(p, "quiz", AssetDatabase.LoadAssetAtPath<QuizDefinition>($"{Knowledge}QZ_{quizId}.asset"));
        SetRewards(p, items);
    }

    private static void ItemPickup(string name, Transform parent, Vector3 pos, string pickupId, string itemId, WorldStateStore world, InventoryService inventory)
    {
        GameObject go = SharedPrefabBuilder.Place("PF_ItemPickup", parent, pos, 0f, name);
        ItemPickup ip = go.GetComponent<ItemPickup>();
        Set(ip, "worldState", world);
        Set(ip, "inventory", inventory);
        Set(ip, "pickupId", pickupId);
        Set(ip, "itemId", itemId);
        go.transform.Find("Visual").GetComponent<Renderer>().sharedMaterial =
            Mat(itemId == InventoryService.LinhChi ? "M_BO_Mushroom" : itemId == InventoryService.ThuocMau ? "M_BO_Potion" : "M_BO_Herb");
    }

    private static void Boundary(Transform parent, Vector3 min, Vector3 max)
    {
        GameObject b = new GameObject("Boundary");
        b.transform.SetParent(parent, false);
        b.transform.position = (min + max) * 0.5f;
        b.layer = Layer("Environment");
        b.AddComponent<BoxCollider>().size = max - min;
    }

    private static void Thrower(string name, Transform parent, Vector3 pos)
    {
        GameObject go = SharedPrefabBuilder.Place("PF_HacAnhNem", parent, pos, 180f, name);
    }

    private static void Shade(string name, Transform parent, Vector3 pos, Vector3 patrolA, Vector3 patrolB)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/PF_HacAnh.prefab");
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.name = name;
        go.transform.position = pos;
        Transform a = Empty(name + "_PatrolA", parent, patrolA).transform;
        Transform b = Empty(name + "_PatrolB", parent, patrolB).transform;
        Set(go.GetComponent<EnemyBrain>(), "patrolPoints", new Object[] { a, b });
    }

    private static void SetQuestSteps(ChapterQuestTracker quest, params (string text, StateSet set, string id)[] steps)
    {
        SerializedObject so = new SerializedObject(quest);
        SerializedProperty list = so.FindProperty("steps");
        list.arraySize = steps.Length;
        for (int i = 0; i < steps.Length; i++)
        {
            SerializedProperty step = list.GetArrayElementAtIndex(i);
            step.FindPropertyRelative("text").stringValue = steps[i].text;
            SerializedProperty reqs = step.FindPropertyRelative("requirements");
            reqs.arraySize = 1;
            SerializedProperty r = reqs.GetArrayElementAtIndex(0);
            r.FindPropertyRelative("set").enumValueIndex = (int)steps[i].set;
            r.FindPropertyRelative("id").stringValue = steps[i].id;
            r.FindPropertyRelative("displayName").stringValue = steps[i].id;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddToBuild(string path)
    {
        List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (list.Exists(s => s.path == path)) return;
        list.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = list.ToArray();
    }
}
