using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public Image fade;
    public float fadeDuration = 2f;

    void Start()
    {
        fade.gameObject.SetActive(true);
        fade.CrossFadeAlpha(0f, fadeDuration, false);
    }

}
