using UnityEngine;

public class BulletShooter : MonoBehaviour
{
    [Header("Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float bulletLifetime = 5f; // seconds before bullet is destroyed

    [Header("Audio")]
    public AudioClip clip;
    private AudioSource source;

    private void Start()
    {
        // Khởi tạo AudioSource nếu chưa có
        source = GetComponent<AudioSource>();
        
        // Kiểm tra an toàn để tránh lỗi NullReference
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
        }
    }

    // Bạn có thể gọi hàm này từ Update() khi nhấn chuột hoặc từ Event
    public void FireBullet()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Tạo bản sao của viên đạn tại vị trí và hướng của firePoint
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // Lấy thành phần Rigidbody để tác động lực
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        // Phát âm thanh bắn
        if (clip != null && source != null)
        {
            source.PlayOneShot(clip);
        }

        if (rb != null)
        {
            // Gán vận tốc cho viên đạn theo hướng phía trước của firePoint
            rb.linearVelocity = firePoint.forward * bulletSpeed;
        }

        // Tự động xóa viên đạn sau một khoảng thời gian để tránh nặng máy
        Destroy(bullet, bulletLifetime);
    }

}