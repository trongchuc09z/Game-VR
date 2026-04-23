using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public Image fade;
    public float fadeDuration = 2f;

    void Start()
    {
        QualitySettings.vSyncCount = 0;

        // Khóa FPS ở mức 60 (bạn có thể thay đổi số này thành 30, 90, 120...)
        Application.targetFrameRate = 72;
        fade.gameObject.SetActive(true);
        fade.CrossFadeAlpha(0f, fadeDuration, false);
    }

}
