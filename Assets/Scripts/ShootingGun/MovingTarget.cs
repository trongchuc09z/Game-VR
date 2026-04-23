using UnityEngine;
using System.Collections;

public class MovingTarget : MonoBehaviour
{
    public float speed = 2f;
    public float moveRange = 3f; // Khoảng cách di chuyển sang 2 bên

    [Header("Audio Settings")]
    public AudioClip hitClip; // Tiếng kim loại boong boong khi trúng đạn

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
        if (isHit) return;

        // Di chuyển ngang
        transform.Translate(Vector3.right * moveDirection * speed * Time.deltaTime);

        // Đảo chiều nếu đi quá giới hạn
        if (Vector3.Distance(startPos, transform.position) >= moveRange)
        {
            // Ép nó nhích lùi lại vào trong vùng an toàn một chút để không bị kẹt
            transform.position = Vector3.MoveTowards(transform.position, startPos, 0.05f);

            // Sau đó mới đảo chiều
            moveDirection *= -1f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
            HitByBullet(other.gameObject);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Bullet"))
            HitByBullet(other.gameObject);
    }

    public void HitByBullet(GameObject bullet = null)
    {
        if (isHit) return;

        isHit = true;
        ShootingManager.Instance.AddScore();
        if (bullet != null) Destroy(bullet); // Xóa viên đạn nếu có

        // Phát tiếng bắn trúng cho cả trường hợp trúng bằng hitscan hoặc va chạm vật lý.
        if (hitClip != null) AudioSource.PlayClipAtPoint(hitClip, transform.position);

        StartCoroutine(FallAndRespawn());
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