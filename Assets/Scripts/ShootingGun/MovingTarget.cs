using UnityEngine;
using System.Collections;

public class MovingTarget : MonoBehaviour
{
    public float speed = 2f;
    public float moveRange = 3f; // Khoảng cách di chuyển sang 2 bên

    private Vector3 startPos;
    private float moveDirection;
    private bool isHit = false;

    void Start()
    {
        startPos = transform.position;
        // Random hướng xuất phát (Trái hoặc Phải)
        moveDirection = Random.Range(0, 2) == 0 ? 1f : -1f;
    }

    void Update()
    {
        if (isHit) return; // Bị bắn trúng thì ngừng di chuyển

        // Di chuyển ngang
        transform.Translate(Vector3.right * moveDirection * speed * Time.deltaTime);

        // Đảo chiều nếu đi quá giới hạn
        if (Vector3.Distance(startPos, transform.position) >= moveRange)
        {
            moveDirection *= -1f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isHit) return;

        // Nếu vật chạm vào có Tag là "Bullet"
        if (other.CompareTag("Bullet"))
        {
            isHit = true;
            ShootingManager.Instance.AddScore();
            Destroy(other.gameObject); // Xóa viên đạn
            StartCoroutine(FallAndRespawn());
        }
    }

    IEnumerator FallAndRespawn()
    {
        // Hiệu ứng đổ ngửa ra sau (Xoay 90 độ trục X)
        float t = 0;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = transform.rotation * Quaternion.Euler(90f, 0, 0);

        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f); // Nằm chết 0.5s

        ShootingManager.Instance.SpawnTarget(); // Gọi manager sinh bia mới
        Destroy(gameObject); // Tự hủy
    }
}