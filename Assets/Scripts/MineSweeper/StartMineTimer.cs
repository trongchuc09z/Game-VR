using UnityEngine;
public class StartMineTimer : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("XR Origin") || other.name.Contains("VR Player"))
        {
            MineManager.Instance.StartGame();
            Destroy(gameObject); // Xóa cổng đi để chỉ chạy 1 lần
        }
    }
}