using UnityEngine;

public class HoopScore : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Basketball"))
        {
            // Đổi tag ngay lập tức để quả bóng này không thể ăn điểm 2 lần (nếu nó vướng lưới nảy nảy)
            other.tag = "Untagged";

            // Báo cho manager xử lý: cộng điểm + phát tiếng + hủy bóng + spawn bóng mới
            if (BasketballManager.Instance != null)
                BasketballManager.Instance.OnBallScored(other.gameObject, transform.position);
        }
    }
}