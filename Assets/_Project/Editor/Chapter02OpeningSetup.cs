using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;

// Stage 1 only: story continuity and the first-room reading/visual cues.
[InitializeOnLoad]
public static class Chapter02OpeningSetup
{
    const string Request = "Library/SonDoongC2Opening.request.json";
    public const string Art = "Assets/_Project/Art/Chapter02Opening";
    [Serializable] class Command { public string action; }
    [Serializable] class Report { public string action; public List<string> checks = new List<string>(), failures = new List<string>(), notes = new List<string>(); }
    static bool busy;
    static Chapter02OpeningSetup() => EditorApplication.update += Poll;
    static T[] All<T>(Scene s) where T : Component => s.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();
    static void Guard()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
        for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Unsaved user scene: " + SceneManager.GetSceneAt(i).path);
    }
    static void Poll()
    {
        if (busy || EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(Request)) return;
        busy = true; var r = new Report();
        try
        {
            r.action = JsonUtility.FromJson<Command>(File.ReadAllText(Request)).action; Guard();
            if (r.action == "inspect-entry")
            {
                var setup=EditorSceneManager.GetSceneManagerSetup();
                try
                {
                    var s=EditorSceneManager.OpenScene(Chapter01RevisionSetup.Main);
                    var exit=All<ChapterExit>(s).Single();
                    Chapter01RevisionSetup.Ref<GameObject>(exit,"portalVisual").SetActive(true);Physics.SyncTransforms();
                    foreach(var c in exit.GetComponentsInChildren<Collider>())r.notes.Add(c.name+": layer="+c.gameObject.layer+", active="+c.gameObject.activeInHierarchy+", bounds="+c.bounds);
                    var target=exit.GetComponentsInChildren<Collider>().First(c=>c.enabled&&c.gameObject.layer==8).bounds.center;
                    foreach(var dir in new[]{Vector3.back,Vector3.left,Vector3.right,Vector3.forward})
                    {
                        var p=target+dir*1.8f;
                        bool floorFound=Physics.Raycast(p+Vector3.up*2,Vector3.down,out var floor,4,1<<3,QueryTriggerInteraction.Ignore);
                        p=floor.point+Vector3.up*.08f;
                        var blockers=Physics.OverlapCapsule(p+Vector3.up*.4f,p+Vector3.up*1.4f,.28f,1<<3,QueryTriggerInteraction.Ignore);
                        var eye=p+Vector3.up*1.45f;var d=target-eye;
                        Physics.Raycast(eye,d.normalized,out var hit,4,(1<<3)|(1<<8),QueryTriggerInteraction.Ignore);
                        r.notes.Add("Approach "+dir+": floor="+floorFound+" "+floor.collider?.name+", point="+p+", blockers="+string.Join(",",blockers.Select(c=>c.name))+", aimHit="+hit.collider?.name+", target="+target);
                    }
                }
                finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
            else if (r.action == "apply" || r.action == "validate")
            {
                var setup = EditorSceneManager.GetSceneManagerSetup();
                try
                {
                    foreach (string path in new[] { Chapter01RevisionSetup.Main, Chapter01RevisionSetup.Preview, Chapter02MechanicsAudit.ScenePath })
                    {
                        var s = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                        if (r.action == "apply")
                        {
                            if (path == Chapter02MechanicsAudit.ScenePath) Opening(s); else Continuity(s);
                            foreach (var c in All<Component>(s)) if (c && PrefabUtility.IsPartOfPrefabInstance(c)) PrefabUtility.RecordPrefabInstancePropertyModifications(c);
                            EditorSceneManager.SaveScene(s);
                        }
                        Validate(s, r);
                    }
                    if (r.action == "apply") AssetDatabase.SaveAssets();
                }
                finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            }
            else if (r.action == "test")
            {
                SessionState.SetBool("SonDoong.C2OpeningTest", true);
                EditorSceneManager.OpenScene(Chapter01RevisionSetup.Main);
                EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();
                EditorApplication.isPaused = false; EditorApplication.EnterPlaymode();
                r.checks.Add("Opt-in C1-to-C2 regression requested; personal saves protected before fixtures.");
            }
            else throw new InvalidOperationException(r.action);
        }
        catch (Exception e) { r.failures.Add(e.ToString()); }
        finally { Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/C2Opening-" + r.action + ".json", JsonUtility.ToJson(r, true)); File.Delete(Request); busy = false; }
    }
    static Transform Group(string name, Transform parent)
    { var g = new GameObject(name); g.transform.SetParent(parent, false); return g.transform; }
    static void Continuity(Scene s)
    {
        var altar = All<RelicAltar>(s).Single();
        BuilderUtil.Set(altar, "endBody", "Ba mảnh Trống Đồng khớp lại. Tiếng trống ngân qua lòng hang, linh mạch dưới Đàn Kim Quy sáng trở lại và Hắc Sương lùi xa.\n\nKim Quy trao cho An <b>Phúc Kim Quy</b>: Ctrl trái + hướng để lộn né, lộn rồi chém để đâm tới. Kim Quy dặn: “Khi gặp trống bốn vành, vành của ta vang cuối cùng.”\n\nMột luồng gió từ khe đá đưa xuống chiếc lông vàng. An đi theo luồng gió tới <b>Giếng Trời Chim Lạc</b>; trên cao, một linh mạch khác vẫn đang bị Hắc Sương che phủ.\n\nTiến trình đã được lưu tại bệ Kim Quy. Túi đồ, Lồng Đèn Sen, Phúc Kim Quy và những ghi chép sẽ theo An sang chương tiếp theo.");
        var exit = All<ChapterExit>(s).Single(); BuilderUtil.Set(exit, "label", "Theo luồng gió đến Giếng Trời Chim Lạc");
        // The expanded cave's rear buttresses enclosed the old (-7,0,307) portal.
        // Keep its script/visual references and move the whole portal into the court.
        exit.transform.position = new Vector3(-6,0,300);
    }
    static Material Material(string name, Color color)
    {
        string path = Art + "/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
        m.color = color; m.SetFloat("_Smoothness", .25f); m.SetFloat("_Cull", 0); EditorUtility.SetDirty(m); return m;
    }
    static GameObject Shape(PrimitiveType type, string name, Transform parent, Vector3 local, Vector3 size, Material m)
    {
        var g = GameObject.CreatePrimitive(type); g.name = name; g.layer = 2;
        g.transform.SetParent(parent, false); g.transform.localPosition = local; g.transform.localScale = size;
        UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>()); g.GetComponent<Renderer>().sharedMaterial = m; return g;
    }
    static Mesh FeatherMesh()
    {
        string path = Art + "/Feather_LowPoly.asset"; var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path); if (mesh) return mesh;
        var outline = new[] { new Vector2(-.04f,-.45f), new Vector2(-.23f,-.15f), new Vector2(-.25f,.12f), new Vector2(-.12f,.4f), new Vector2(.02f,.55f), new Vector2(.17f,.32f), new Vector2(.24f,.08f), new Vector2(.16f,-.2f), new Vector2(.04f,-.45f) };
        var vertices = new List<Vector3> { new Vector3(0,.04f,.055f) }; var triangles = new List<int>();
        vertices.AddRange(outline.Select(p => new Vector3(p.x,p.y,0)));
        for (int i = 0; i < outline.Length; i++) { triangles.Add(0); triangles.Add(i+1); triangles.Add((i+1)%outline.Length+1); }
        mesh = new Mesh { name = "Feather_LowPoly", vertices = vertices.ToArray(), triangles = triangles.ToArray() }; mesh.RecalculateNormals(); mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh, path); return mesh;
    }
    static void Opening(Scene s)
    {
        Chapter01RevisionSetup.Folder(Art);
        var world = All<Transform>(s).Single(t => t.name == "World_Geometry");
        var old = world.Find("ART_C2_Opening"); if (old) UnityEngine.Object.DestroyImmediate(old.gameObject);
        var root = Group("ART_C2_Opening", world);
        var start = All<CheckpointAltar>(s).Single(c=>c.CheckpointId=="c2_cp_start");
        var spawn = Chapter01RevisionSetup.Ref<Transform>(All<SaveService>(s).Single(),"campSpawn");
        start.SpawnPoint.SetPositionAndRotation(spawn.position,Quaternion.identity);
        Chapter01GuidePedestalSetup.AddGuide(s, root, WorldIds.GuideWindC2, "Guide_C2_Wind", new Vector3(-1.8f,0,-3.4f));
        var gold = Material("Feather_Gold", new Color(.94f,.63f,.19f));
        var ivory = Material("Feather_Spine", new Color(1,.88f,.57f));
        var teal = Material("Wind_Waymark", new Color(.25f,.64f,.61f));
        var pickup = All<AbilityPickup>(s).Single();
        pickup.transform.position = new Vector3(1.6f,1.1f,-2.4f);
        var oldVisual = pickup.transform.Find("VIS_WindFeather"); if (oldVisual) UnityEngine.Object.DestroyImmediate(oldVisual.gameObject);
        foreach(var renderer in pickup.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
        var visual = Group("VIS_WindFeather", pickup.transform);
        var scale = pickup.transform.lossyScale; visual.localScale = new Vector3(1/scale.x,1/scale.y,1/scale.z); visual.localRotation = Quaternion.Euler(8,25,-15);
        var vane = Group("Golden_Vane", visual).gameObject; vane.layer = 2;
        vane.AddComponent<MeshFilter>().sharedMesh = FeatherMesh(); vane.AddComponent<MeshRenderer>().sharedMaterial = gold;
        var spine = Shape(PrimitiveType.Cylinder,"Ivory_Spine",visual,new Vector3(0,0,.04f),new Vector3(.035f,.53f,.035f),ivory);
        spine.transform.localRotation = Quaternion.Euler(0,0,-3);
        var box=pickup.GetComponentsInChildren<BoxCollider>(true).First(c=>c.gameObject.layer==8);
        var bounds=Chapter01GuidePedestalSetup.GeometryBounds(visual.gameObject);bounds.Expand(.08f);
        var localBounds=new Bounds(box.transform.InverseTransformPoint(bounds.center),Vector3.zero);
        for(int i=0;i<8;i++)localBounds.Encapsulate(box.transform.InverseTransformPoint(bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1))));
        box.center=localBounds.center;box.size=localBounds.size;
        foreach (var q in new[] { new Vector3(0,.018f,-4.6f),new Vector3(0,.018f,-3.4f),new Vector3(1.8f,.018f,-.5f),new Vector3(3.6f,.018f,1.2f) })
        {
            var marker = Shape(PrimitiveType.Cube,"Inlaid_Wind_Mark",root,q,new Vector3(.18f,.018f,.65f),teal);
            marker.transform.rotation = Quaternion.Euler(0,q.z< -2?0:45,0);
        }
        var light = Group("Feather_Warm_Pool",root).gameObject.AddComponent<Light>(); light.type = LightType.Point; light.transform.position = new Vector3(1.6f,2.8f,-2.4f); light.color = new Color(1,.78f,.47f); light.intensity = 2; light.range = 6; light.shadows = LightShadows.None;
        var card = Chapter01RevisionSetup.Ref<StoryCard>(pickup,"card");
        card.body = "<b>Lông Chim Lạc — Cột Gió</b>\n\nChiếc lông vàng đáp lại ánh sáng Lồng Đèn Sen. An đã nhận năng lực gió; đây là bước đầu để lên đỉnh giếng và tìm ba mảnh Mặt Trời Lông Chim, chưa phải lúc Chim Lạc được giải thoát.\n\n<b>Thử ngay ở cột gió bên phải</b>\nBước vào vệt gió xanh để được nâng lên. Đợi lơ lửng gần đỉnh, dùng WASD và nhấn Space để nhảy sang gờ đá. Ra khỏi luồng gió, An sẽ rơi bình thường.\n\nTrên gờ có bia và Bệ Vấn Đáp. Bật đèn thường bằng F, đọc bia rồi trả lời để nhận mảnh đầu tiên.\n\nPhúc Kim Quy vẫn còn: Ctrl trái + hướng để lộn né, tốn 25 thể lực. Hãy ghé lửa trại bên trái và nhấn E để lưu năng lực vừa nhận; giấy “Theo luồng gió” có thể đọc lại trong Nhật ký (J).";
        EditorUtility.SetDirty(card);
        var tracker = All<ChapterQuestTracker>(s).Single(); var so = new SerializedObject(tracker); var steps = so.FindProperty("steps");
        steps.GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue = "Nhận Lông Chim Lạc (E).\nTheo khoảng sáng trước mặt.";
        steps.GetArrayElementAtIndex(1).FindPropertyRelative("text").stringValue = "Dùng cột gió bên phải để lên gờ.\nĐọc bia và nhận mảnh đầu tiên.";
        so.ApplyModifiedPropertiesWithoutUndo();
        Chapter01UISetup.ReadingPanels(s);
        Chapter01RevisionUI.RefineDetails(s,_=>{});
        foreach(var scaler in All<UnityEngine.UI.CanvasScaler>(s)) { scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f; }
        // Keep C2's own NavMesh asset and GUID; never call the C1-specific baker.
        var nav = All<NavMeshSurface>(s).Single(); var existing = nav.navMeshData;
        nav.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders; nav.layerMask = 1<<3; nav.BuildNavMesh();
        var generated = nav.navMeshData;
        if (existing && generated != existing) { EditorUtility.CopySerialized(generated,existing); nav.RemoveData(); nav.navMeshData = existing; nav.AddData(); EditorUtility.SetDirty(existing); UnityEngine.Object.DestroyImmediate(generated); }
    }
    static void Validate(Scene s, Report r)
    {
        Action<bool,string> check = (ok,text) => { if (ok) r.checks.Add(s.name+": "+text); else r.failures.Add(s.name+": "+text); };
        check(All<Transform>(s).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject))==0,"No missing scripts");
        if (s.name != GameSession.SecondChapterScene)
        {
            var exit = All<ChapterExit>(s).Single(); check(Chapter01RevisionSetup.Ref<WorldStateStore>(exit,"worldState"),"Exit keeps world reference");
            var so = new SerializedObject(exit); check(so.FindProperty("nextScene").stringValue==GameSession.SecondChapterScene && so.FindProperty("nextCheckpointId").stringValue=="c2_cp_start","Exit targets C2 start");
            check(All<PuzzleCluePickup>(s).Length==PuzzleCluePickup.Chapter1ClueIds.Count,"C1 keeps five papers");
            var visual=Chapter01RevisionSetup.Ref<GameObject>(exit,"portalVisual");bool active=visual.activeSelf;visual.SetActive(true);Physics.SyncTransforms();
            var target=exit.GetComponentsInChildren<Collider>().First(c=>c.enabled&&c.gameObject.layer==8).bounds.center;
            var p=exit.transform.position+Vector3.back*1.8f+Vector3.up*.08f;
            check(!Physics.CheckCapsule(p+Vector3.up*.4f,p+Vector3.up*1.4f,.28f,1<<3,QueryTriggerInteraction.Ignore),"Exit court approach has capsule clearance");
            var d=target-(p+Vector3.up*1.45f);Physics.Raycast(p+Vector3.up*1.45f,d.normalized,out var hit,4,(1<<3)|(1<<8),QueryTriggerInteraction.Ignore);
            check(hit.collider&&hit.collider.GetComponentInParent<ChapterExit>()==exit,"Exit court approach has unobstructed interaction ray");
            var finalCheckpoint=All<CheckpointAltar>(s).Single(c=>c.CheckpointId==SaveService.KimQuyCheckpointId);var path=new UnityEngine.AI.NavMeshPath();
            check(UnityEngine.AI.NavMesh.CalculatePath(finalCheckpoint.SpawnPoint.position,p,UnityEngine.AI.NavMesh.AllAreas,path)&&path.status==UnityEngine.AI.NavMeshPathStatus.PathComplete,"Final checkpoint connects to exit court on existing NavMesh");
            visual.SetActive(active);
            return;
        }
        var note = All<PuzzleCluePickup>(s).SingleOrDefault(n=>n.ClueId==WorldIds.GuideWindC2); check(note,"One optional arrival guide");
        check(All<AbilityPickup>(s).Single().transform.Find("VIS_WindFeather"),"Feather visual keeps original ability root");
        check(All<AbilityPickup>(s).Single().GetComponentsInChildren<Renderer>(true).Where(r=>!r.transform.IsChildOf(All<AbilityPickup>(s).Single().transform.Find("VIS_WindFeather"))).All(r=>!r.enabled),"Original blockout feather renderers are hidden");
        var cp = All<CheckpointAltar>(s).Single(c=>c.CheckpointId=="c2_cp_start"); Physics.SyncTransforms();
        check(Physics.Raycast(cp.SpawnPoint.position+Vector3.up*.2f,Vector3.down,1,1<<3),"C2 start spawn has floor");
        check(All<NavMeshSurface>(s).Single().navMeshData && AssetDatabase.GetAssetPath(All<NavMeshSurface>(s).Single().navMeshData).Contains("03_GiengTroiChimLac"),"C2 NavMesh retains its own asset");
        check(All<ChapterQuestTracker>(s).Single().CurrentObjective.Contains("Lông Chim Lạc"),"Opening HUD points to feather");
    }
}
