using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.XR.CoreUtils;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;

    [Header("Player References")]
    public GameObject xrOrigin;

    [Header("Teleport Points")]
    public Transform grenadeSpawnPoint;
    public Transform shootingSpawnPoint;

    [Header("Scene Names")]
    public string finalSceneName = "Scene_KetThuc";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Nếu lỡ kéo prefab từ Project vào Inspector, tự tìm XR Origin trong scene đang chạy.
        if (xrOrigin == null || !xrOrigin.scene.IsValid() || !xrOrigin.scene.isLoaded)
        {
            XROrigin sceneXROrigin = FindObjectOfType<XROrigin>();
            if (sceneXROrigin != null)
            {
                xrOrigin = sceneXROrigin.gameObject;
            }
        }
    }

    void Start()
    {
        // Tăng thời gian chờ lên 0.5s để XR Device Simulator khởi động xong hoàn toàn
        StartCoroutine(TeleportRoutine(0.5f, grenadeSpawnPoint));
    }

    // Tách riêng quy trình dịch chuyển thành một Coroutine để quản lý thời gian
    private IEnumerator TeleportRoutine(float initialDelay, Transform targetDestination)
    {
        // 1. Chờ delay ban đầu (nếu có)
        if (initialDelay > 0)
        {
            yield return new WaitForSeconds(initialDelay);
        }

        if (xrOrigin != null && targetDestination != null)
        {
            CharacterController cc = xrOrigin.GetComponent<CharacterController>();

            // Cảnh báo sớm để dễ debug khi điểm spawn đặt đúng bằng vị trí hiện tại.
            if (Vector3.Distance(xrOrigin.transform.position, targetDestination.position) < 0.01f)
            {
                Debug.LogWarning("[GameFlowManager] Teleport target trung với vị trí hiện tại, nên bạn sẽ thấy như không dịch chuyển.");
            }

            // 2. Tắt Character Controller
            if (cc != null) cc.enabled = false;

            // 3. BẮT BUỘC: Chờ hết 1 frame để Unity ghi nhận việc tắt CC
            yield return new WaitForEndOfFrame();

            // 4. Tiến hành bế nhân vật đi
            xrOrigin.transform.position = targetDestination.position;
            xrOrigin.transform.rotation = targetDestination.rotation;

            // 5. BẮT BUỘC: Ép Unity đồng bộ vị trí vật lý ngay lập tức
            Physics.SyncTransforms();

            // 6. Bật lại Character Controller
            if (cc != null) cc.enabled = true;
        }
        else
        {
            Debug.LogWarning("[GameFlowManager] Thiếu tham chiếu xrOrigin hoặc targetDestination, không thể teleport.");
        }
    }

    // Các hàm gọi từ nút bấm UI giờ sẽ dùng chung quy trình an toàn ở trên
    public void TeleportToGrenade()
    {
        StartCoroutine(TeleportRoutine(0f, grenadeSpawnPoint));
    }

    public void NextToShootingGame()
    {
        StartCoroutine(TeleportRoutine(0f, shootingSpawnPoint));
    }

    public void LoadFinalScene()
    {
        SceneManager.LoadScene(finalSceneName);
    }
}