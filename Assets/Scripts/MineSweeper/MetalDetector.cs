using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(AudioSource))]
public class MetalDetector : MonoBehaviour
{
    public Transform sensorPoint; // Đầu dò
    public float detectionRadius = 2f;

    public AudioClip beepClip;

    private AudioSource audioSource;
    private XRGrabInteractable grabInteractable;
    private bool isHolding = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs args) { isHolding = true; }
    void OnReleased(SelectExitEventArgs args) { isHolding = false; }

    void Update()
    {
        if (!isHolding)
        {
            if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
            return;
        }

        // Quét tìm mìn trong bán kính
        Collider[] hits = Physics.OverlapSphere(sensorPoint.position, detectionRadius);
        float closestDistance = detectionRadius;
        bool foundMine = false;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Mine"))
            {
                HiddenMine mine = hit.GetComponent<HiddenMine>();
                // Chỉ kêu nếu mìn chưa bị cắm cờ đánh dấu
                if (mine != null && !mine.isDefused)
                {
                    foundMine = true;
                    float dist = Vector3.Distance(sensorPoint.position, hit.transform.position);
                    if (dist < closestDistance) closestDistance = dist;
                }
            }
        }

        if (foundMine)
        {
            if (!audioSource.isPlaying && beepClip != null)
            {
                audioSource.clip = beepClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
