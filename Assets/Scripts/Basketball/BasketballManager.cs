using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;

public class BasketballManager : MonoBehaviour
{
    public static BasketballManager Instance;

    [Header("UI Settings")]
    public TextMeshProUGUI scoreBoardText;
    public GameObject replayButton;
    public GameObject nextButton;

    [Header("Spawning Settings")]
    public GameObject basketballPrefab;
    public Transform ballSpawnPoint;

    [Header("Player Setup")]
    public Transform playerSpawnPoint; // Vị trí người chơi cần đứng ném bóng

    [Header("Rules")]
    public int maxThrows = 5;
    public int winScore = 2;

    [Header("Audio")]
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip scoreClip; // Tiếng "Swish" khi bóng lọt lưới (Tùy chọn)

    private int currentScore = 0;
    private int currentThrows = 0;

    private readonly HashSet<int> scoredBallIds = new HashSet<int>();
    private bool isGameOver;

    void Awake() { Instance = this; }

    void Start()
    {
        AutoBindUiButtonsIfMissing();
        UpdateUI();
        if (replayButton != null) replayButton.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);

        StartCoroutine(FixPlayerPosition()); // Ép vị trí người chơi sau khi load 
        SpawnBall();
    }

    public void AddScore()
    {
        // Giữ lại cho tương thích (nếu có script khác đang gọi).
        Vector3 sfxPos = (Camera.main != null) ? Camera.main.transform.position : transform.position;
        AddScoreInternal(sfxPos);
    }

    public void OnBallScored(GameObject ball, Vector3 scorePosition)
    {
        if (isGameOver) return;
        if (ball == null) return;

        int ballId = ball.GetInstanceID();
        if (scoredBallIds.Contains(ballId)) return;

        scoredBallIds.Add(ballId);

        // Chống ăn điểm 2 lần nếu bóng vướng lưới nảy nảy
        ball.tag = "Untagged";

        AddScoreInternal(scorePosition);

        Destroy(ball);

        if (currentThrows >= maxThrows)
        {
            EndGame();
            return;
        }

        SpawnBall();
    }

    private void AddScoreInternal(Vector3 sfxPosition)
    {
        currentScore++;
        UpdateUI();

        if (scoreClip != null)
            AudioSource.PlayClipAtPoint(scoreClip, sfxPosition);
    }

    // Hàm này được gọi từ quả bóng ngay khi người chơi thả tay ra
    public void OnBallThrown(GameObject ball)
    {
        if (isGameOver) return;

        currentThrows++;
        UpdateUI();

        int ballId = ball != null ? ball.GetInstanceID() : 0;
        StartCoroutine(WaitAndProcessThrow(ballId, ball));
    }

    IEnumerator WaitAndProcessThrow(int ballId, GameObject ball)
    {
        // Chờ 4 giây để quả bóng bay tới rổ và nảy nốt quỹ đạo
        yield return new WaitForSeconds(4f);

        if (isGameOver) yield break;

        // Nếu quả bóng này đã ăn điểm (và đã spawn bóng mới) thì không làm gì nữa.
        if (ballId != 0 && scoredBallIds.Contains(ballId))
            yield break;

        if (currentThrows >= maxThrows)
        {
            EndGame();
        }
        else
        {
            // Chưa hết lượt -> Xóa bóng cũ (nếu còn) và đẻ bóng mới
            if (ball != null) Destroy(ball);
            SpawnBall();
        }
    }

    private void EndGame()
    {
        if (isGameOver) return;
        isGameOver = true;

        bool isWin = currentScore >= winScore;

        if (scoreBoardText != null)
        {
            scoreBoardText.text += isWin
                ? "\n\n<color=green>CHIẾN THẮNG!</color>"
                : "\n\n<color=red>THẤT BẠI!</color>";
        }

        // Đảm bảo luôn có nút để người chơi thoát/đánh lại
        if (replayButton != null) replayButton.SetActive(true);
        if (nextButton != null) nextButton.SetActive(isWin);

        Vector3 sfxPos = (Camera.main != null) ? Camera.main.transform.position : transform.position;
        if (isWin)
        {
            if (winClip != null) AudioSource.PlayClipAtPoint(winClip, sfxPos);
        }
        else
        {
            if (loseClip != null) AudioSource.PlayClipAtPoint(loseClip, sfxPos);
        }

        if (replayButton == null)
            Debug.LogWarning("[BasketballManager] replayButton is not assigned, so it cannot be shown on Game Over.");
        if (nextButton == null)
            Debug.LogWarning("[BasketballManager] nextButton is not assigned, so it cannot be shown on Victory.");
    }

    void SpawnBall()
    {
        if (basketballPrefab != null && ballSpawnPoint != null)
        {
            Instantiate(basketballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation);
        }
    }

    void ClearOldBalls()
    {
        GameObject[] oldBalls = GameObject.FindGameObjectsWithTag("Basketball");
        foreach (GameObject ball in oldBalls) Destroy(ball);
    }

    public void ReplayGame()
    {
        isGameOver = false;
        scoredBallIds.Clear();

        currentScore = 0;
        currentThrows = 0;
        UpdateUI();

        if (replayButton != null) replayButton.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);

        ClearOldBalls();
        SpawnBall();
    }

    public void LoadNextScene(string nextSceneName)
    {
        SceneManager.LoadScene(nextSceneName);
    }

    void UpdateUI()
    {
        if (scoreBoardText != null)
            scoreBoardText.text = $"ĐIỂM SỐ: {currentScore} / {winScore}\nĐÃ NÉM: {currentThrows} / {maxThrows}";
    }

    private void AutoBindUiButtonsIfMissing()
    {
        if (replayButton != null && nextButton != null) return;

        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            if (button == null) continue;

            int eventCount = button.onClick.GetPersistentEventCount();
            for (int i = 0; i < eventCount; i++)
            {
                Object target = button.onClick.GetPersistentTarget(i);
                string method = button.onClick.GetPersistentMethodName(i);

                if (replayButton == null && target == this && method == nameof(ReplayGame))
                    replayButton = button.gameObject;

                // Next thường là gọi LoadNextScene (static string) hoặc một hàm điều hướng khác.
                if (nextButton == null && target == this && method == nameof(LoadNextScene))
                    nextButton = button.gameObject;
            }
        }
    }

    IEnumerator FixPlayerPosition()
    {
        // Chờ 0.2s để XR rig và thiết bị nhận đủ tracking
        yield return new WaitForSeconds(0.2f);
        
        Unity.XR.CoreUtils.XROrigin origin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
        if (origin != null && playerSpawnPoint != null)
        {
            CharacterController cc = origin.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            
            yield return new WaitForEndOfFrame();

            Transform rigCamera = origin.Camera != null ? origin.Camera.transform : Camera.main.transform;
            if (rigCamera != null)
            {
                // Bù trừ Tracking y hệt như logic ở GameFlowManager của bạn
                Vector3 cameraOffset = rigCamera.position - origin.transform.position;
                Vector3 horizontalOffset = new Vector3(cameraOffset.x, 0f, cameraOffset.z);
                
                Vector3 newRigPosition = playerSpawnPoint.position - horizontalOffset;
                newRigPosition.y = playerSpawnPoint.position.y;
                
                origin.transform.position = newRigPosition;
                origin.transform.rotation = Quaternion.Euler(0f, playerSpawnPoint.eulerAngles.y, 0f);
            }
            else
            {
                origin.transform.position = playerSpawnPoint.position;
            }

            Physics.SyncTransforms();
            if (cc != null) cc.enabled = true;
        }
    }
}