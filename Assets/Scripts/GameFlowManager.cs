using UnityEngine;
using UnityEngine.SceneManagement; // Bắt buộc để chuyển Scene

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;

    [Header("Player References")]
    public GameObject xrOrigin; // Kéo XR Origin (VR) vào đây

    [Header("Teleport Points")]
    public Transform grenadeSpawnPoint; // Điểm spawn màn lựu đạn
    public Transform shootingSpawnPoint; // Điểm dịch chuyển màn bắn súng

    [Header("Scene Names")]
    public string finalSceneName = "Scene_KetThuc"; // Tên scene tiếp theo sau khi win bắn súng

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Khi Scene mới bắt đầu, tự động đưa người chơi về vị trí màn lựu đạn
        TeleportToGrenade();
    }

    // 1. Hàm dịch chuyển về màn lựu đạn (Giai đoạn bắt đầu)
    public void TeleportToGrenade()
    {
        if (xrOrigin != null && grenadeSpawnPoint != null)
        {
            xrOrigin.transform.position = grenadeSpawnPoint.position;
            xrOrigin.transform.rotation = grenadeSpawnPoint.rotation;
        }
    }

    // 2. Hàm dịch chuyển sang màn bắn súng (Gắn vào nút Next của màn lựu đạn)
    public void NextToShootingGame()
    {
        if (xrOrigin != null && shootingSpawnPoint != null)
        {
            xrOrigin.transform.position = shootingSpawnPoint.position;
            xrOrigin.transform.rotation = shootingSpawnPoint.rotation;
        }
    }

    // 3. Hàm chuyển sang Scene tiếp theo (Gắn vào nút Next của màn bắn súng)
    public void LoadFinalScene()
    {
        // Đảm bảo bạn đã thêm Scene vào Build Settings
        SceneManager.LoadScene(finalSceneName);
    }
}