using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(AudioSource))]
public class Basketball : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip bounceClip; // Tiếng bóng đập vào sàn/vành rổ
    [Min(0f)]
    public float bounceMaxDuration = 0f; // 0 = phát hết clip; >0 = cắt tiếng sau X giây

    private AudioSource audioSource;
    private XRGrabInteractable grabInteractable;
    private bool hasBeenThrown = false;
    private Coroutine stopBounceRoutine;
    private int bouncePlayId = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (!hasBeenThrown)
        {
            hasBeenThrown = true;
            // Báo cho Manager biết bóng đã được ném
            if (BasketballManager.Instance != null)
                BasketballManager.Instance.OnBallThrown(gameObject);

            // Bóng tự hủy sau 5 giây để dọn dẹp bộ nhớ
            Destroy(gameObject, 5f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Tính toán lực va chạm để phát tiếng to hay nhỏ
        if (bounceClip != null && collision.relativeVelocity.magnitude > 1f)
        {
            // Càng đập mạnh tiếng càng to (chia 10 để chuẩn hóa volume từ 0 - 1)
            float volume = Mathf.Clamp01(collision.relativeVelocity.magnitude / 10f);
            audioSource.PlayOneShot(bounceClip, volume);

            if (bounceMaxDuration > 0f && bounceMaxDuration < bounceClip.length)
            {
                bouncePlayId++;
                if (stopBounceRoutine != null)
                    StopCoroutine(stopBounceRoutine);
                stopBounceRoutine = StartCoroutine(StopBounceAfter(bouncePlayId, bounceMaxDuration));
            }
        }
    }

    private IEnumerator StopBounceAfter(int playId, float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        if (playId == bouncePlayId)
            audioSource.Stop();
    }
}