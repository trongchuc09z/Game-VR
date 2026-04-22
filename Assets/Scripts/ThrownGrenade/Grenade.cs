using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Grenade : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public float delayTime = 3f;
    public GameObject explosionEffect;

    public GameObject floatingText;

    private bool isArmed = false;
    private XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        if (floatingText != null)
        {
            floatingText.SetActive(false);
        }
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (!isArmed)
        {
            isArmed = true;
            // Chỉ đếm ngược để nổ, KHÔNG gọi GameManager ở đây nữa [cite: 2]
            Invoke("Explode", delayTime);
        }
    }

    void Explode()
    {
        // SỬA DÒNG NÀY: Lưu hiệu ứng vào biến và tự hủy nó sau 2 giây
        if (explosionEffect != null)
        {
            GameObject fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        MiniGameManager.Instance.OnGrenadeExploded();
        Destroy(gameObject);
    }
}