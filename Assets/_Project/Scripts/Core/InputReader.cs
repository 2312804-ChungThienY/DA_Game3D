using UnityEngine;
using UnityEngine.InputSystem;

// Nơi duy nhất đọc input. Dùng bộ action dùng chung của dự án (PlayerControls).
public class InputReader : MonoBehaviour
{
    private InputActionMap gameplay;
    private InputActionMap ui;

    public InputAction Move { get; private set; }
    public InputAction Look { get; private set; }
    public InputAction Jump { get; private set; }
    public InputAction Sprint { get; private set; }
    public InputAction Dodge { get; private set; }
    public InputAction Attack { get; private set; }
    public InputAction ToggleLantern { get; private set; }
    public InputAction Focus { get; private set; }
    public InputAction Interact { get; private set; }
    public InputAction UsePotion { get; private set; }
    public InputAction LockOn { get; private set; }
    public InputAction Inventory { get; private set; }
    public InputAction Journal { get; private set; }
    public InputAction Pause { get; private set; }
    public InputAction DebugLoad { get; private set; }
    public InputAction UICancel { get; private set; }

    public bool IsReady => gameplay != null && ui != null;
    // Cờ riêng: bảng UI có thể bật lẻ một action (ví dụ I để đóng túi) mà gameplay vẫn coi là tắt.
    public bool GameplayEnabled { get; private set; }

    public Vector2 MoveValue => GameplayEnabled ? Move.ReadValue<Vector2>() : Vector2.zero;
    public Vector2 LookValue => GameplayEnabled ? Look.ReadValue<Vector2>() : Vector2.zero;

    private void Awake()
    {
        InputActionAsset asset = InputSystem.actions;
        if (asset == null)
        {
            Debug.LogError("InputReader: chưa đặt PlayerControls làm bộ action dùng chung.", this);
            enabled = false;
            return;
        }

        gameplay = asset.FindActionMap("Gameplay", false);
        ui = asset.FindActionMap("UI", false);
        if (gameplay == null || ui == null)
        {
            Debug.LogError($"InputReader: {asset.name} thiếu map Gameplay hoặc UI.", this);
            gameplay = null;
            ui = null;
            enabled = false;
            return;
        }

        Move = gameplay.FindAction("Move", true);
        Look = gameplay.FindAction("Look", true);
        Jump = gameplay.FindAction("Jump", true);
        Sprint = gameplay.FindAction("Sprint", true);
        Dodge = gameplay.FindAction("Dodge", true);
        Attack = gameplay.FindAction("Attack", true);
        ToggleLantern = gameplay.FindAction("ToggleLantern", true);
        Focus = gameplay.FindAction("Focus", true);
        Interact = gameplay.FindAction("Interact", true);
        UsePotion = gameplay.FindAction("UsePotion", true);
        LockOn = gameplay.FindAction("LockOn", true);
        Inventory = gameplay.FindAction("Inventory", true);
        Journal = gameplay.FindAction("Journal", true);
        Pause = gameplay.FindAction("Pause", true);
        DebugLoad = gameplay.FindAction("DebugLoad", true);
        UICancel = ui.FindAction("Cancel", true);

        // UI luôn bật để menu nhận chuột; Gameplay chỉ bật khi phiên đang chơi.
        ui.Enable();
        gameplay.Disable();
    }

    // Tắt map sẽ xóa giá trị đang giữ (Move, Focus, Attack...).
    public void SetGameplayEnabled(bool on)
    {
        if (gameplay == null) return;

        GameplayEnabled = on;
        if (on) gameplay.Enable();
        else gameplay.Disable();
    }
}
