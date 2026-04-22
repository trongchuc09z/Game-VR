using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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
        SpawnBarrels();
        SpawnGrenades();
        UpdateScoreUI();
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
            scoreText.text = $"ĐIỂM SỐ: {currentScore} / {winScore}\nLƯỢT NÉM: {currentThrows} / {maxThrows}";
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
                scoreText.text += "\n<color=green>THẮNG RỒI!</color>";
            else
                scoreText.text += "\n<color=red>THUA RỒI!</color>";
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