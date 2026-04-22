using UnityEngine;

public class HideOnApproach : MonoBehaviour
{
    public Transform playerCamera; // Kéo Main Camera của XR Origin vào đây
    public float hideDistance = 2f; // Khoảng cách (mét) để chữ biến mất

    void Update()
    {
        // Nếu player lại gần hơn hideDistance -> tự động tắt object này
        if (playerCamera != null)
        {
            float dist = Vector3.Distance(transform.position, playerCamera.position);
            if (dist <= hideDistance)
            {
                gameObject.SetActive(false);
            }
        }
    }
}