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

    [Header("Audio Settings")]
    public AudioClip pickupClip;   // Tiếng lách cách khi nhặt
    public AudioClip explosionClip; // Tiếng BÙM!

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
        if (floatingText != null) floatingText.SetActive(false);

        // Phát tiếng nhặt lựu đạn
        if (pickupClip != null)
        {
            AudioSource.PlayClipAtPoint(pickupClip, transform.position);
        }
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (!isArmed)
        {
            isArmed = true;
            Invoke("Explode", delayTime);
        }
    }

    void Explode()
    {
        if (explosionEffect != null)
        {
            GameObject fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        // Phát tiếng nổ siêu to khổng lồ
        if (explosionClip != null)
        {
            AudioSource.PlayClipAtPoint(explosionClip, transform.position, 1f); // 1f là max volume
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