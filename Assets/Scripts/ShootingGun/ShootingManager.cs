using UnityEngine;
using TMPro;
using System.Collections;

public class ShootingManager : MonoBehaviour
{
    public static ShootingManager Instance;

    [Header("UI Settings")]
    public TextMeshProUGUI scoreBoardText;
    public GameObject winLossBoard; // Bảng kết quả (chứa nút Replay/Next)

    [Header("Target Settings")]
    public GameObject targetPrefab;
    public Transform targetSpawnPoint;

    [Header("Rules")]
    public int maxAmmo = 20;
    public int winScore = 8;

    private int currentScore = 0;
    private int bulletsFired = 0;
    private int targetsSpawned = 0;

    void Awake() { Instance = this; }

    void Start()
    {
        UpdateUI();
        SpawnTarget();
        if (winLossBoard != null) winLossBoard.SetActive(false);
    }

    public void AddScore()
    {
        currentScore++;
        UpdateUI();
    }

    public void OnBulletFired()
    {
        bulletsFired++;
        UpdateUI();

        if (bulletsFired >= maxAmmo)
        {
            StartCoroutine(CheckGameOver());
        }
    }

    public void SpawnTarget()
    {
        if (targetsSpawned < maxAmmo)
        {
            targetsSpawned++;
            // SỬA DÒNG NÀY: Dùng góc xoay của cái điểm Spawn luôn
            Instantiate(targetPrefab, targetSpawnPoint.position, targetSpawnPoint.rotation);
        }
    }

    void UpdateUI()
    {
        if (scoreBoardText != null)
            scoreBoardText.text = $"ĐIỂM: {currentScore}/{winScore}\nĐẠN: {maxAmmo - bulletsFired}";
    }

    IEnumerator CheckGameOver()
    {
        yield return new WaitForSeconds(2f); // Đợi đạn bay tới

        if (winLossBoard != null)
        {
            winLossBoard.SetActive(true);
            TextMeshProUGUI resultText = winLossBoard.transform.Find("ResultText").GetComponent<TextMeshProUGUI>();

            if (currentScore >= winScore)
                resultText.text = "<color=green>ĐẠT CHUẨN!</color>\nHoàn thành bài bắn.";
            else
                resultText.text = "<color=red>TRƯỢT!</color>\nYêu cầu bắn lại.";
        }
    }

    // GẮN HÀM NÀY VÀO NÚT REPLAY CỦA MÀN BẮN SÚNG
    public void ReplayGame()
    {
        // 1. Reset điểm
        currentScore = 0;
        bulletsFired = 0;
        targetsSpawned = 0;
        UpdateUI();

        // 2. Tắt bảng thông báo kết quả
        if (winLossBoard != null) winLossBoard.SetActive(false);

        // 3. Xóa bia cũ (nếu còn sót)
        MovingTarget oldTarget = FindFirstObjectByType<MovingTarget>();
        if (oldTarget != null) Destroy(oldTarget.gameObject);

        // 4. Sinh bia mới
        SpawnTarget();

        // LƯU Ý: Đạn của khẩu súng phải được nạp lại. 
        // Bạn có thể cần gọi GunController để reset đạn, hoặc đơn giản nhất là xóa súng cũ, Instantiate lại súng mới.
    }
}