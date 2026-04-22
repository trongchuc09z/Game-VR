using UnityEngine;

public class FallDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem vật rơi xuống có phải là thùng không
        if (other.CompareTag("Barrel"))
        {
            // Cộng điểm
            MiniGameManager.Instance.AddScore(1);

            // Đổi tag để tránh tính điểm 2 lần nếu nó nảy lên rớt xuống
            other.tag = "Untagged";

            // Xóa thùng sau 2 giây cho đỡ nặng máy
            Destroy(other.gameObject, 2f);
        }
    }
}