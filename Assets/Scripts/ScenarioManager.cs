using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenarioManager : MonoBehaviour
{
    [Header("Scene To Load")]
    [SerializeField] private string sceneName;

    [Header("Optional Filter")]
    [SerializeField] private string requiredTag;

    [Header("Collision Mode")]
    [SerializeField] private bool useTrigger = true;

    private bool _loading;

    private void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return;
        TryLoad(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (useTrigger) return;
        TryLoad(collision.gameObject);
    }

    private void TryLoad(GameObject other)
    {
        if (_loading) return;
        if (string.IsNullOrWhiteSpace(sceneName)) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        _loading = true;
        SceneManager.LoadScene(sceneName);
    }
}
