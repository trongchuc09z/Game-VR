using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro; // Bắt buộc để dùng UI Text

[RequireComponent(typeof(LineRenderer))]
public class GunController : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 40f;
    public int maxAmmo = 20; // Số đạn tối đa của súng

    private int currentAmmo;

    [Header("Laser Sight")]
    public float laserLength = 50f;
    private LineRenderer laserLine;

    [Header("UI")]
    public GameObject floatingText; // Chữ "Nhặt tôi"
    public TextMeshProUGUI ammoText; // Bảng LED đạn gắn trên súng

    private XRGrabInteractable grabInteractable;
    private bool isHolding = false;

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI(); // Hiện số đạn lúc mới bắt đầu

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
            if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, laserLength))
            {
                laserLine.SetPosition(1, hit.point);
            }
            else
            {
                laserLine.SetPosition(1, firePoint.position + firePoint.forward * laserLength);
            }
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        isHolding = true;
        laserLine.enabled = true;
        if (floatingText != null) floatingText.SetActive(false);
    }

    void OnReleased(SelectExitEventArgs args)
    {
        isHolding = false;
        laserLine.enabled = false;
    }

    void Shoot(ActivateEventArgs args)
    {
        if (currentAmmo <= 0) return; // Hết đạn thì không bắn được nữa

        currentAmmo--; // Trừ 1 viên đạn
        UpdateAmmoUI(); // Cập nhật chữ trên súng

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * bulletSpeed;
        }

        ShootingManager.Instance.OnBulletFired(); // Báo cho hệ thống biết đã bắn
        Destroy(bullet, 5f);
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = $"{currentAmmo} / {maxAmmo}";
    }
    // Thêm hàm này vào dưới cùng của class GunController
    public void ResetGun()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }
}