using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class MineManager : MonoBehaviour
{
    public static MineManager Instance;

    [Header("UI Settings")]
    public TextMeshProUGUI statusText; // Hiển thị Điểm và Thời gian
    public GameObject replayButton;
    public GameObject nextButton;

    [Header("Rules")]
    public int totalMines = 5;
    public int winScore = 3;
    public float timeLimit = 90f; // 1p30s
    public int totalFlags = 5;
    
    [HideInInspector]
    public int currentFlags;

    [Header("Audio")]
    public AudioClip winClip;
    public AudioClip loseClip;

    private int currentScore = 0;
    private float timeRemaining;
    private bool isGameActive = false;
    private bool isGameOver = false;

    void Awake() { Instance = this; }

    void Start()
    {
        timeRemaining = timeLimit;
        currentFlags = totalFlags;
        UpdateUI();
        if (replayButton != null) replayButton.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);
    }

    void Update()
    {
        if (isGameActive && !isGameOver)
        {
            timeRemaining -= Time.deltaTime;
            UpdateUI();

            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                GameOver(false, "HẾT GIỜ!");
            }
        }
    }

    // Được gọi khi người chơi bước vào bãi mìn
    public void StartGame()
    {
        if (!isGameActive && !isGameOver)
        {
            isGameActive = true;
            Debug.Log("Bắt đầu tính giờ dò mìn!");
        }
    }

    public void AddScore()
    {
        if (isGameOver) return;

        currentScore++;
        UpdateUI();

        if (currentScore >= winScore)
        {
            GameOver(true, "HOÀN THÀNH!");
        }
    }

    public bool UseFlag()
    {
        if (currentFlags > 0)
        {
            currentFlags--;
            UpdateUI();
            
            if (currentFlags == 0)
            {
                // Chờ 3 giây để xem lá cờ cuối cùng có rớt trúng mìn hay không
                Invoke("CheckGameOverFlags", 3f);
            }
            
            return true;
        }
        return false;
    }

    void CheckGameOverFlags()
    {
        if (currentScore < winScore && !isGameOver)
        {
            GameOver(false, "HẾT CỜ!");
        }
    }

    public void GameOver(bool isWin, string reason)
    {
        if (isGameOver) return;
        isGameOver = true;
        isGameActive = false;

        if (statusText != null)
        {
            statusText.text = isWin
                ? $"<color=green>{reason}</color>\nTìm được: {currentScore}/{winScore}"
                : $"<color=red>{reason}</color>\nTìm được: {currentScore}/{winScore}";
        }

        if (replayButton != null) replayButton.SetActive(true);
        if (nextButton != null) nextButton.SetActive(isWin);

        Vector3 sfxPos = Camera.main != null ? Camera.main.transform.position : transform.position;
        if (isWin && winClip != null) AudioSource.PlayClipAtPoint(winClip, sfxPos);
        else if (!isWin && loseClip != null) AudioSource.PlayClipAtPoint(loseClip, sfxPos);
    }

    // Nút Replay sẽ gọi hàm này. Nó sẽ Load lại nguyên Scene hiện tại để Reset toàn bộ bàn chơi.
    public void ReplayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void UpdateUI()
    {
        if (statusText != null && !isGameOver)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60F);
            int seconds = Mathf.FloorToInt(timeRemaining - minutes * 60);
            string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);

            statusText.text = $"THỜI GIAN: {timeString}\nĐÃ TÌM THẤY: {currentScore} / {winScore}\nCỜ CÒN LẠI: {currentFlags}";
        }
    }
}
