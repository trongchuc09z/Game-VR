using UnityEngine;

public class HiddenMine : MonoBehaviour
{
    public GameObject explosionEffect;
    public AudioClip explosionClip;
    public bool isDefused = false;

    void OnTriggerEnter(Collider other)
    {
        if (isDefused) return;

        // Nếu người chơi dẫm phải (XR Origin có tag Player hoặc tên chứa VR Player)
        if (other.CompareTag("Player") || other.name.Contains("XR Origin") || other.name.Contains("VR Player"))
        {
            Explode();
        }
    }

    public void Defuse()
    {
        isDefused = true;
        // Báo cho Manager cộng điểm
        MineManager.Instance.AddScore();
    }

    void Explode()
    {
        if (explosionEffect != null)
        {
            GameObject fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        if (explosionClip != null)
        {
            AudioSource.PlayClipAtPoint(explosionClip, transform.position);
        }

        MineManager.Instance.GameOver(false, "DẪM PHẢI MÌN!");
        gameObject.SetActive(false);
    }
}
