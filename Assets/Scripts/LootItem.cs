using UnityEngine;

public class LootItem : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool useTrigger = true;
    public GameObject scenario1;
    public GameObject scenario3;

    private void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return;
        if (other.CompareTag(playerTag))
        {
            gameObject.SetActive(false);
            if (scenario3 != null)
            {
                scenario1.SetActive(false);
                scenario3.SetActive(true);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (useTrigger) return;
        if (collision.gameObject.CompareTag(playerTag))
            gameObject.SetActive(false);
    }
}
