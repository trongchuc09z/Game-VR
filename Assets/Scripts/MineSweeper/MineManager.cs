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

    [Header("Player Settings")]
    public Transform playerTransform; // Đối tượng XR Origin hoặc VR Player
    public Transform spawnPoint;      // Điểm sinh ra (Mine spawn point)

    [Header("Audio")]
    public AudioClip winClip;
    public AudioClip loseClip;

    private int currentScore = 0;
    private float timeRemaining;
    private bool isGameActive = false;
    private bool isGameOver = false;

    private HiddenMine[] allMines;
    private MineFlag[] allFlags;
    private Vector3[] flagStartPos;
    private Quaternion[] flagStartRot;

    void Awake() { Instance = this; }

    void Start()
    {
        allMines = FindObjectsOfType<HiddenMine>();

        allFlags = FindObjectsOfType<MineFlag>();
        flagStartPos = new Vector3[allFlags.Length];
        flagStartRot = new Quaternion[allFlags.Length];
        for (int i = 0; i < allFlags.Length; i++)
        {
            flagStartPos[i] = allFlags[i].transform.position;
            flagStartRot[i] = allFlags[i].transform.rotation;
        }

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

    // Nút Replay sẽ gọi hàm này. Trạng thái chơi sẽ được reset mà không load lại Scene.
    public void ReplayGame()
    {
        // Reset biến trạng thái
        currentScore = 0;
        timeRemaining = timeLimit;
        currentFlags = totalFlags;
        isGameActive = false;
        isGameOver = false;

        // Khôi phục mìn
        if (allMines != null)
        {
            foreach (var mine in allMines)
            {
                if (mine != null)
                {
                    mine.isDefused = false;
                    mine.gameObject.SetActive(true);
                }
            }
        }

        // Khôi phục vị trí các lá cờ ban đầu
        if (allFlags != null)
        {
            for (int i = 0; i < allFlags.Length; i++)
            {
                if (allFlags[i] != null)
                {
                    allFlags[i].transform.position = flagStartPos[i];
                    allFlags[i].transform.rotation = flagStartRot[i];
                    allFlags[i].gameObject.SetActive(true);
                }
            }
        }

        // Xóa các cờ nhân bản bị rơi rải rác
        MineFlag[] currentFlagsInScene = FindObjectsOfType<MineFlag>();
        foreach (var flag in currentFlagsInScene)
        {
            // Nếu cờ này không nằm trong danh sách cờ gốc thì xóa đi
            if (flag != null && System.Array.IndexOf(allFlags, flag) == -1)
            {
                Destroy(flag.gameObject);
            }
        }

        // Đưa Player về điểm xuất phát (Spawn Point)
        if (playerTransform != null && spawnPoint != null)
        {
            // Tắt CharacterController tạm thời để code có thể đè vị trí
            CharacterController cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerTransform.position = spawnPoint.position;
            playerTransform.rotation = spawnPoint.rotation;

            if (cc != null) cc.enabled = true;
        }

        UpdateUI();
        if (replayButton != null) replayButton.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);
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
