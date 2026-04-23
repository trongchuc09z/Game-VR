using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance;

    [Header("References")]
    public GameObject barrelPrefab;
    public Collider spawnArea;

    [Header("Grenade Settings")]
    public GameObject grenadePrefab;
    public Transform[] grenadeSpawnPoints;

    [Header("UI Settings")]
    public TextMeshProUGUI scoreText;
    public GameObject replayButton;
    public GameObject nextButton;

    [Header("Audio Settings")]
    public AudioClip winClip;
    public AudioClip loseClip;

    [Header("Rules")]
    public int barrelsPerRound = 10;
    public int maxThrows = 3;
    public int winScore = 12;

    [Header("Spawn Settings")]
    public float spawnMargin = 0.5f;
    public float minDistance = 1.2f;
    public int maxSpawnAttempts = 30;

    private int currentScore = 0;
    private int currentThrows = 0;

    // Biến để quản lý thời gian chờ, tránh lỗi ném 2 quả liên tiếp
    private Coroutine roundTimerCoroutine;

    void Awake() { Instance = this; }

    void Start()
    {
        AutoBindUiButtonsIfMissing();

        SpawnBarrels();
        SpawnGrenades();
        UpdateScoreUI();

        if (replayButton != null) replayButton.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);
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
                {
                    replayButton = button.gameObject;
                }

                if (nextButton == null && target is GameFlowManager && method == nameof(GameFlowManager.NextToShootingGame))
                {
                    nextButton = button.gameObject;
                }
            }
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();
    }

    // THAY THẾ HÀM CŨ: Hàm này được gọi khi lựu đạn PHÁT NỔ [cite: 4]
    public void OnGrenadeExploded()
    {
        currentThrows++;
        UpdateScoreUI();

        // Nếu có bộ đếm cũ đang chạy, dừng nó lại để đếm lại 10s từ đầu
        if (roundTimerCoroutine != null)
        {
            StopCoroutine(roundTimerCoroutine);
        }

        // Bắt đầu đếm 10 giây chờ kết quả [cite: 4]
        roundTimerCoroutine = StartCoroutine(WaitAndCheckState(10f));
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"SCORE: {currentScore} / {winScore}\nTHROWS: {currentThrows} / {maxThrows}";
        }
    }

    IEnumerator WaitAndCheckState(float delay)
    {
        // Chờ đúng 10 giây kể từ lúc nổ để vật lý hoạt động [cite: 4]
        yield return new WaitForSeconds(delay);

        if (currentThrows >= maxThrows)
        {
            // Kết thúc game
            if (currentScore >= winScore)
            {
                scoreText.text += "\n<color=green>VICTORY!</color>";
                if (winClip != null && Camera.main != null)
                    AudioSource.PlayClipAtPoint(winClip, Camera.main.transform.position);
                if (nextButton != null) nextButton.SetActive(true);
                if (replayButton != null) replayButton.SetActive(false);
            }
            else
            {
                scoreText.text += "\n<color=red>DEFEAT!</color>";
                if (loseClip != null && Camera.main != null)
                    AudioSource.PlayClipAtPoint(loseClip, Camera.main.transform.position);
                if (replayButton != null) replayButton.SetActive(true);
                if (nextButton != null) nextButton.SetActive(false);
            }
        }
        else
        {
            // Nếu chưa ném hết 3 quả, reset bàn chơi cho lượt tiếp theo
            ClearOldBarrels();
            SpawnBarrels();
        }
    }

    void SpawnGrenades()
    {
        foreach (Transform spawnPoint in grenadeSpawnPoints)
        {
            if (spawnPoint != null && grenadePrefab != null)
                Instantiate(grenadePrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    void SpawnBarrels()
    {
        Bounds bounds = spawnArea.bounds;
        List<Vector3> spawnedPositions = new List<Vector3>();

        for (int i = 0; i < barrelsPerRound; i++)
        {
            Vector3 randomPos = Vector3.zero;
            bool validPosition = false;
            int attempts = 0;

            while (!validPosition && attempts < maxSpawnAttempts)
            {
                attempts++;
                randomPos = new Vector3(
                    Random.Range(bounds.min.x + spawnMargin, bounds.max.x - spawnMargin),
                    bounds.max.y + 0.5f,
                    Random.Range(bounds.min.z + spawnMargin, bounds.max.z - spawnMargin)
                );

                validPosition = true;
                foreach (Vector3 pos in spawnedPositions)
                {
                    if (Vector3.Distance(randomPos, pos) < minDistance)
                    {
                        validPosition = false;
                        break;
                    }
                }
            }

            if (validPosition)
            {
                Instantiate(barrelPrefab, randomPos, Quaternion.identity);
                spawnedPositions.Add(randomPos);
            }
        }
    }

    void ClearOldBarrels()
    {
        GameObject[] oldBarrels = GameObject.FindGameObjectsWithTag("Barrel");
        foreach (GameObject barrel in oldBarrels)
        {
            Destroy(barrel);
        }
    }
    // GẮN HÀM NÀY VÀO NÚT REPLAY TRONG UNITY
    public void ReplayGame()
    {
        // 1. Reset điểm và lượt ném
        currentScore = 0;
        currentThrows = 0;
        UpdateScoreUI();

        if (replayButton != null) replayButton.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);

        // 2. Dừng mọi bộ đếm thời gian đang chạy dở
        StopAllCoroutines();

        // 3. Dọn dẹp thùng cũ và sinh thùng mới
        ClearOldBarrels();
        SpawnBarrels();

        // 4. Tìm và xóa sạch lựu đạn cũ đang rải rác trên sân
        GameObject[] oldGrenades = GameObject.FindGameObjectsWithTag("Grenade");
        foreach (GameObject g in oldGrenades)
        {
            Destroy(g);
        }

        // 5. Đặt lại 3 quả lựu đạn mới lên bàn
        SpawnGrenades();
    }
}