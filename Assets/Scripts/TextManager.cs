using TMPro;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    public static TextManager Instance;
    public TextMeshPro[] textElements;
    private void Awake()
    {
        Instance = this;
    }


}
