using UnityEngine;
using TMPro;
using System.Collections;

public class ShootingManager : MonoBehaviour
{
    public static ShootingManager Instance;

    [Header("UI Settings")]
    public TextMeshProUGUI scoreBoardText;
    public GameObject replayButton;
    public GameObject nextButton;

    [Header("Target Settings")]
    public GameObject targetPrefab;
    public Transform targetSpawnPoint;

    // THÊM MỚI: Cài đặt cho Súng
    [Header("Gun Settings")]
    public GameObject gunPrefab;
    public Transform gunSpawnPoint;

    [Header("Rules")]
    public int maxAmmo = 20;
    public int winScore = 8;

    [Header("Audio Settings")]
    public AudioClip winClip;
    public AudioClip loseClip;

    private int currentScore = 0;
    private int bulletsFired = 0;
    private int targetsSpawned = 0;

    void Awake() { Instance = this; }

    void Start()
    {
        UpdateUI();
        SpawnTarget();
        if (replayButton != null) replayButton.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);

        // Sinh súng lần đầu tiên khi bắt đầu màn chơi
        SpawnGun();
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
            Instantiate(targetPrefab, targetSpawnPoint.position, targetSpawnPoint.rotation);
        }
    }

    // THÊM MỚI: Hàm sinh súng
    public void SpawnGun()
    {
        if (gunPrefab != null && gunSpawnPoint != null)
        {
            Instantiate(gunPrefab, gunSpawnPoint.position, gunSpawnPoint.rotation);
        }
    }

    void UpdateUI()
    {
        if (scoreBoardText != null)
            scoreBoardText.text = $"SCORE: {currentScore} / {winScore}\nAMMO: {maxAmmo - bulletsFired} / {maxAmmo}";
    }

    IEnumerator CheckGameOver()
    {
        yield return new WaitForSeconds(1.5f);

        if (scoreBoardText != null)
        {
            if (currentScore >= winScore)
            {
                scoreBoardText.text += "\n\n<color=green>VICTORY!</color>";
                if (nextButton != null) nextButton.SetActive(true);

                // Tiếng Thắng
                if (winClip != null && Camera.main != null)
                    AudioSource.PlayClipAtPoint(winClip, Camera.main.transform.position);
            }
            else
            {
                scoreBoardText.text += "\n\n<color=red>DEFEAT!</color>\nPlease try again.";

                // Tiếng Thua
                if (loseClip != null && Camera.main != null)
                    AudioSource.PlayClipAtPoint(loseClip, Camera.main.transform.position);
            }

            if (replayButton != null) replayButton.SetActive(true);
        }
    }

    public void ReplayGame()
    {
        // 1. Reset thông số
        currentScore = 0;
        bulletsFired = 0;
        targetsSpawned = 0;
        UpdateUI();

        // 2. Ẩn các nút đi
        if (replayButton != null) replayButton.SetActive(false);
        if (nextButton != null) nextButton.SetActive(false);

        // 3. Xóa bia cũ còn kẹt trên sân
        MovingTarget oldTarget = FindFirstObjectByType<MovingTarget>();
        if (oldTarget != null) Destroy(oldTarget.gameObject);

        // 4. Sinh bia mới
        SpawnTarget();

        // 5. THAY ĐỔI: Xóa súng cũ và đẻ ra súng mới
        GunController oldGun = FindFirstObjectByType<GunController>();
        if (oldGun != null)
        {
            Destroy(oldGun.gameObject);
        }
        SpawnGun();
    }
}