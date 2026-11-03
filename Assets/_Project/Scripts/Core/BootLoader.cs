using UnityEngine;
using UnityEngine.SceneManagement;

// Scene 00_Boot: AppRoot đã được tạo trước khi scene nạp; chỉ cần chuyển sang menu.
public class BootLoader : MonoBehaviour
{
    private void Start() => SceneManager.LoadScene(GameSession.MainMenuScene);
}
