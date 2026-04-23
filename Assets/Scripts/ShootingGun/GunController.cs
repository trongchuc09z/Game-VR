using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(AudioSource))] // Tự động gắn thêm Component AudioSource
public class GunController : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 40f;
    public int maxAmmo = 20;
    public bool useHitscan = true; // Tính hit theo tia laser để tránh hụt do vật lý
    public LayerMask hitMask = ~0;
    private int currentAmmo;

    [Header("Laser Sight")]
    public float laserLength = 50f;
    private LineRenderer laserLine;

    [Header("UI & Audio")]
    public GameObject floatingText;
    public TextMeshProUGUI ammoText;
    public AudioClip pickupClip; // Tiếng lên đạn/nhặt súng
    public AudioClip shootClip;  // Tiếng súng nổ
    private AudioSource audioSource;

    private XRGrabInteractable grabInteractable;
    private bool isHolding = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentAmmo = maxAmmo;
        UpdateAmmoUI();

        laserLine = GetComponent<LineRenderer>();
        laserLine.positionCount = 2;
        laserLine.enabled = false;

        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
        grabInteractable.activated.AddListener(Shoot);
    }

    void Update()
    {
        if (isHolding)
        {
            laserLine.SetPosition(0, firePoint.position);
            RaycastHit hit;
            if (TryGetLaserHit(out hit))
                laserLine.SetPosition(1, hit.point);
            else
                laserLine.SetPosition(1, firePoint.position + firePoint.forward * laserLength);
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        isHolding = true;
        laserLine.enabled = true;
        if (floatingText != null) floatingText.SetActive(false);
        if (pickupClip != null) audioSource.PlayOneShot(pickupClip);
    }

    void OnReleased(SelectExitEventArgs args)
    {
        isHolding = false;
        laserLine.enabled = false;
    }

    void Shoot(ActivateEventArgs args)
    {
        if (currentAmmo <= 0 || firePoint == null) return;

        currentAmmo--;
        UpdateAmmoUI();

        // Phát tiếng nổ
        if (shootClip != null) audioSource.PlayOneShot(shootClip);

        // Hit detection bám theo tia laser để cảm giác bắn "ngắm đâu trúng đó"
        if (useHitscan)
        {
            RaycastHit hit;
            if (TryGetLaserHit(out hit))
            {
                MovingTarget target = hit.collider.GetComponentInParent<MovingTarget>();
                if (target != null)
                    target.HitByBullet();
            }
        }

        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // Nếu đã xử lý bằng hitscan thì tắt collider viên đạn để không cộng điểm 2 lần.
            if (useHitscan)
            {
                Collider bulletCollider = bullet.GetComponent<Collider>();
                if (bulletCollider != null) bulletCollider.enabled = false;
            }

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = firePoint.forward * bulletSpeed;

            Destroy(bullet, 5f);
        }

        ShootingManager.Instance.OnBulletFired();
    }

    bool TryGetLaserHit(out RaycastHit hit)
    {
        return Physics.Raycast(firePoint.position, firePoint.forward, out hit, laserLength, hitMask, QueryTriggerInteraction.Collide);
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null) ammoText.text = $"{currentAmmo} / {maxAmmo}";
    }

    public void ResetGun()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }
}