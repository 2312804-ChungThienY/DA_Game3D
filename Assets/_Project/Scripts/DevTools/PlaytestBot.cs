#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Bot kiểm thử tự động (chỉ có trong Editor). Điều khiển An bằng input giả lập của Input System
// giống người chơi: đi theo waypoint, ngắm bằng camera, nhấn E, giữ chuột phải, chém, chạy né.
// Gắn bằng PlaytestBot.Run(scenario) khi đang Play; đọc kết quả qua PlaytestBot.Report.
public partial class PlaytestBot : MonoBehaviour
{
    public static string Report => log.ToString();
    public static string ReportPath => System.IO.Path.Combine(Application.temporaryCachePath, "playtest_report.txt");
    public static bool Finished { get; private set; }
    public static bool Failed { get; private set; }

    private static readonly StringBuilder log = new StringBuilder();
    private readonly HashSet<Key> heldKeys = new HashSet<Key>();
    private bool leftMouse, rightMouse, middleMouse;
    private float startTime;

    public static PlaytestBot Run(string scenario)
    {
        log.Clear();
        Finished = false;
        Failed = false;
        Application.runInBackground = true;
        GameObject go = new GameObject("PlaytestBot");
        DontDestroyOnLoad(go);
        PlaytestBot bot = go.AddComponent<PlaytestBot>();
        bot.StartCoroutine(bot.Main(scenario));
        return bot;
    }

    private IEnumerator Main(string scenario)
    {
        startTime = Time.realtimeSinceStartup;
        IEnumerator routine = scenario switch
        {
            "c1" => Chapter1(),
            "c2" => Chapter2(),
            "c1c2" => Chapter1Then2(),
            "c2s" => ContinueThenChapter2(),
            _ => null
        };
        if (routine == null) { Fail($"Không có kịch bản {scenario}"); yield break; }

        yield return Guard(routine);
        ReleaseAll();
        Log(Failed ? "KẾT THÚC: CÓ LỖI" : "KẾT THÚC: ĐẠT");
        Finished = true;
        System.IO.File.WriteAllText(ReportPath, Report);
        Destroy(gameObject, 0.5f);
    }

    // Chạy coroutine lồng nhau, dừng ngay khi có Fail.
    private IEnumerator Guard(IEnumerator routine)
    {
        Stack<IEnumerator> stack = new Stack<IEnumerator>();
        stack.Push(routine);
        while (stack.Count > 0 && !Failed)
        {
            IEnumerator top = stack.Peek();
            bool moved;
            try { moved = top.MoveNext(); }
            catch (Exception e) { Fail($"Ngoại lệ: {e.Message}\n{e.StackTrace}"); yield break; }
            if (!moved) { stack.Pop(); continue; }
            if (top.Current is IEnumerator nested) stack.Push(nested);
            else yield return top.Current;
        }
    }

    // ---------- Ghi nhận ----------
    private void Log(string message)
    {
        log.Append($"[{Time.realtimeSinceStartup - startTime,6:0.0}s] {message}\n");
        try { System.IO.File.WriteAllText(ReportPath, Report); } catch (Exception) { }
    }

    private void Fail(string message)
    {
        Failed = true;
        Log("LỖI: " + message);
    }

    private bool Check(bool condition, string message)
    {
        if (condition) Log("OK  " + message);
        else Fail(message);
        return condition;
    }

    // ---------- Input giả lập ----------
    private void PushKeyboard()
    {
        Key[] keys = new Key[heldKeys.Count];
        heldKeys.CopyTo(keys);
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(keys));
    }

    private void PushMouse()
    {
        MouseState state = new MouseState();
        if (leftMouse) state = state.WithButton(MouseButton.Left, true);
        if (rightMouse) state = state.WithButton(MouseButton.Right, true);
        if (middleMouse) state = state.WithButton(MouseButton.Middle, true);
        InputSystem.QueueStateEvent(Mouse.current, state);
    }

    private void Hold(Key key) { if (heldKeys.Add(key)) PushKeyboard(); }
    private void Release(Key key) { if (heldKeys.Remove(key)) PushKeyboard(); }

    private void ReleaseAll()
    {
        heldKeys.Clear();
        leftMouse = rightMouse = middleMouse = false;
        if (Keyboard.current != null) PushKeyboard();
        if (Mouse.current != null) PushMouse();
    }

    private IEnumerator Tap(Key key)
    {
        Hold(key);
        yield return null;
        yield return null;
        Release(key);
        yield return null;
    }

    private IEnumerator Click(MouseButton button)
    {
        SetMouse(button, true);
        yield return null;
        yield return null;
        SetMouse(button, false);
        yield return null;
    }

    private void SetMouse(MouseButton button, bool down)
    {
        if (button == MouseButton.Left) leftMouse = down;
        else if (button == MouseButton.Right) rightMouse = down;
        else middleMouse = down;
        PushMouse();
    }

    // ---------- Truy cập scene ----------
    private static GameSession Session => AppRoot.Instance.Session;
    private static T Find<T>() where T : UnityEngine.Object => FindAnyObjectByType<T>(FindObjectsInactive.Include);
    private static GameObject Player => FindAnyObjectByType<PlayerMotor>().gameObject;
    private static PlayerVitals Vitals => Player.GetComponent<PlayerVitals>();
    private static PlayerCameraController Cam => Player.GetComponent<PlayerCameraController>();

    private static Transform FindByName(string name)
    {
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include))
            if (t.name == name) return t;
        return null;
    }

    private static string Prompt
    {
        get
        {
            GameObject go = GameObject.Find("TXT_InteractionPrompt");
            return go != null ? go.GetComponent<TMPro.TMP_Text>().text : string.Empty;
        }
    }

    private IEnumerator WaitUntil(Func<bool> condition, float timeout, string what)
    {
        float end = Time.realtimeSinceStartup + timeout;
        while (!condition())
        {
            if (Time.realtimeSinceStartup > end) { Fail($"Hết giờ chờ: {what}"); yield break; }
            yield return null;
        }
    }

    private IEnumerator WaitSeconds(float seconds)
    {
        float end = Time.realtimeSinceStartup + seconds;
        while (Time.realtimeSinceStartup < end) yield return null;
    }

    private IEnumerator WaitPlaying(float timeout = 15f)
    {
        yield return WaitUntil(() => AppRoot.Instance != null && Session.State == GameSession.SessionState.Playing &&
                                     FindAnyObjectByType<PlayerMotor>() != null, timeout, "trạng thái Playing");
        yield return WaitSeconds(0.3f);
    }

    // ---------- Di chuyển và ngắm ----------
    private void FaceYaw(Vector3 from, Vector3 to, float pitch = 10f)
    {
        Vector3 d = to - from;
        d.y = 0f;
        if (d.sqrMagnitude < 0.0001f) return;
        Cam.SetAngles(Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg, pitch);
    }

    // Đi thẳng tới điểm (chỉ xét XZ). Giữ W, lái camera theo hướng đích mỗi frame.
    private IEnumerator WalkTo(Vector3 target, float tolerance = 0.5f, float timeout = 20f, bool sprint = false)
    {
        float end = Time.realtimeSinceStartup + timeout;
        if (sprint) Hold(Key.LeftShift);
        Hold(Key.W);
        Vector3 lastPos = Player.transform.position;
        float stuckTimer = 0f;
        int sidesteps = 0;
        int startFrame = Time.frameCount;
        while (true)
        {
            Vector3 p = Player.transform.position;
            Vector3 d = target - p;
            d.y = 0f;
            if (d.magnitude <= tolerance) break;
            if (Time.realtimeSinceStartup > end) { Release(Key.W); Release(Key.LeftShift); Fail($"Không tới được {target} (đang ở {p}; {Time.frameCount - startFrame} frame trong {timeout:0} s, timeScale {Time.timeScale}, bước ngang {sidesteps})"); yield break; }
            if (Session.State != GameSession.SessionState.Playing) { Release(Key.W); Release(Key.LeftShift); Fail($"Phiên chuyển {Session.State} khi đang đi tới {target}"); yield break; }
            FaceYaw(p, target);
            stuckTimer = (p - lastPos).sqrMagnitude < 0.0001f ? stuckTimer + Time.unscaledDeltaTime : 0f;
            if (stuckTimer > 0.5f && sidesteps < 6)
            {
                // Vướng vật cản: bước ngang (luân phiên trái/phải) rồi đi tiếp.
                Key side = sidesteps % 2 == 0 ? Key.D : Key.A;
                sidesteps++;
                Hold(side);
                yield return WaitSeconds(0.45f + 0.15f * sidesteps);
                Release(side);
                stuckTimer = 0f;
            }
            if (stuckTimer > 2.5f) { Release(Key.W); Release(Key.LeftShift); Fail($"Kẹt tại {p} khi đi tới {target}"); yield break; }
            lastPos = Player.transform.position;
            yield return Reflexes();
            yield return null;
        }
        Release(Key.W);
        Release(Key.LeftShift);
        yield return WaitSeconds(0.25f);
    }

    private IEnumerator WalkPath(params Vector3[] points)
    {
        foreach (Vector3 p in points) yield return WalkTo(p);
    }

    // Lái camera để tia giữa màn hình chạm điểm đích; lặp tới khi tia đi sát điểm (camera có độ trễ).
    private IEnumerator Aim(Vector3 point)
    {
        int stable = 0;
        for (int i = 0; i < 120 && stable < 6; i++)
        {
            Transform cam = Camera.main.transform;
            Vector3 v = point - cam.position;
            float horiz = new Vector2(v.x, v.z).magnitude;
            float yaw = Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;
            float pitch = Mathf.Atan2(-v.y, horiz) * Mathf.Rad2Deg;
            Cam.SetAngles(yaw, pitch);
            float miss = Vector3.Cross(cam.forward, v.normalized).magnitude * v.magnitude;
            stable = miss < 0.15f ? stable + 1 : 0;
            yield return null;
        }
    }

    // Ngắm, kiểm tra lời nhắc có chứa từ khóa, rồi nhấn E.
    private IEnumerator Interact(Vector3 point, string expectedPrompt)
    {
        yield return Aim(point);
        yield return WaitUntil(() => Prompt.Length > 0 && Prompt.Contains(expectedPrompt), 2f, $"lời nhắc \"{expectedPrompt}\" (đang: \"{Prompt}\")");
        if (Failed) yield break;
        yield return Tap(Key.E);
        yield return WaitSeconds(0.2f);
    }

    // ---------- Chiến đấu ----------
    private IEnumerator FightBrain(EnemyBrain enemy, float timeout = 40f)
    {
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        float end = Time.realtimeSinceStartup + timeout;
        float hpStart = Vitals.CurrentHealth;
        while (enemy != null && !health.IsDead)
        {
            if (Time.realtimeSinceStartup > end) { Fail($"Đánh {enemy.name} quá lâu"); yield break; }
            if (Vitals.IsDead) { Fail($"An chết khi đánh {enemy.name}"); yield break; }
            Vector3 p = Player.transform.position;
            Vector3 e = enemy.transform.position;
            float dist = Vector3.Distance(new Vector3(p.x, 0, p.z), new Vector3(e.x, 0, e.z));
            if (dist > 1.4f)
            {
                FaceYaw(p, e);
                Hold(Key.W);
                yield return null;
                continue;
            }
            Release(Key.W);
            FaceYaw(p, e);
            yield return Click(MouseButton.Left);
            yield return WaitSeconds(0.7f);
            yield return Reflexes();
        }
        Release(Key.W);
        Log($"Hạ {enemy.name}: máu An {hpStart:0} → {Vitals.CurrentHealth:0}");
        yield return WaitSeconds(1.2f);
    }

    // Phản xạ như người chơi: lộn né đạn đang bay tới, uống thuốc khi máu thấp.
    private IEnumerator Reflexes()
    {
        yield return UsePotionIfLow(40f);
        DodgeController dodge = Player.GetComponent<DodgeController>();
        if (dodge == null || dodge.IsDodging || Player.GetComponent<PlayerStamina>().CurrentStamina < 25f ||
            !Find<WorldStateStore>().Has(StateSet.Abilities, WorldIds.MovementKit)) yield break;
        Vector3 p = Player.transform.position + Vector3.up;
        foreach (EnemyProjectile proj in FindObjectsByType<EnemyProjectile>())
        {
            Vector3 to = p - proj.transform.position;
            if (to.magnitude > 4f || Vector3.Dot(to.normalized, proj.Velocity.normalized) < 0.8f) continue;
            Vector3 side = Vector3.Cross(Vector3.up, proj.Velocity).normalized;
            bool heldW = heldKeys.Contains(Key.W);
            FaceYaw(Player.transform.position, Player.transform.position + side);
            Hold(Key.W);
            yield return null;
            yield return Tap(Key.LeftCtrl);
            if (!heldW) Release(Key.W);
            dodgedProjectiles++;
            yield return WaitSeconds(0.45f);
            yield break;
        }
    }

    private int dodgedProjectiles;

    private IEnumerator UsePotionIfLow(float threshold)
    {
        InventoryService inv = Find<InventoryService>();
        if (Vitals.CurrentHealth < threshold && inv != null && inv.Count(InventoryService.ThuocMau) > 0)
        {
            float before = Vitals.CurrentHealth;
            yield return Tap(Key.Q);
            Log($"Dùng thuốc: {before:0} → {Vitals.CurrentHealth:0}");
        }
    }
}
#endif
