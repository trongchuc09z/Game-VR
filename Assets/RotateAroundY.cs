using UnityEngine;

public class RotateAroundY : MonoBehaviour
{
    // Tốc độ xoay (độ/giây). Có thể chỉnh sửa trực tiếp trong Inspector.
    public float rotationSpeed = 50f;

    void Update()
    {
        // Xoay quanh trục Y (trục Up) liên tục mỗi khung hình
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}