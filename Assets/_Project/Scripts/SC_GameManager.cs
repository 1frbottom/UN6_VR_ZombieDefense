using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;



public class GameManager : MonoBehaviour
{
    [Header("기본 설정")]
    public GameObject zombiePrefab;
    public Transform[] spawnPoints;
    public Transform baseTarget;
    public GameObject magPrefab;
    public Transform magSpawnPoint;

    [Header("웨이브 설정")]
    public int currentWave = 1;
    public int zombiesPerWave = 3;
    public float spawnInterval = 3.0f;
    private int zombiesRemainingAlive = 0;
    private bool isWaveInProgress = false;

    [Header("게임 상태")]
    public float baseHealth = 100.0f;
    public int gold = 0;
    public bool isGameOver = false;

    [Header("UI 연결")]
    public GameObject shopUI; // 웨이브 끝나면 켜질 UI 캔버스

    [Header("HUD 연결")]
    public TextMeshProUGUI hpText;    // 좌하단 체력용
    public TextMeshProUGUI infoText;  // 우하단 웨이브/골드용

    [Header("게임 오버 연결")]
    public GameObject gameOverUI;

    [Header("오디오 설정")]
    public AudioSource gameOverAudio;


    void Start()
    {
        UpdateUI();
        shopUI.SetActive(true); // 처음엔 상점 열어두고 시작 대기
    }

    // --- 웨이브 로직 ---
    public void StartNextWave()
    {
        if (isWaveInProgress) return;

        shopUI.SetActive(false); // 상점 닫기
        isWaveInProgress = true;
        zombiesRemainingAlive = zombiesPerWave; // 이번 웨이브 좀비 수 설정
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < zombiesPerWave; i++)
        {
            if (isGameOver) yield break;

            int rnd = Random.Range(0, spawnPoints.Length);
            GameObject zombie = Instantiate(zombiePrefab, spawnPoints[rnd].position, spawnPoints[rnd].rotation);

            // 좀비에게 "BaseWall"을 타겟으로 지정
            // (주의: ZombieController에 target 변수가 있어야 함)
            var controller = zombie.GetComponent<ZombieController>();
            if (controller != null)
                controller.target = baseTarget;

            // 좀비가 죽었을 때 이 스크립트의 OnZombieDied를 부르도록 설정해야 함 (아래 설명 참조)

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // --- 이벤트 처리 ---
    public void OnZombieDied()
    {
        zombiesRemainingAlive--;
        UpdateUI();

        if (zombiesRemainingAlive <= 0)
        {
            EndWave();
        }
    }

    void EndWave()
    {
        isWaveInProgress = false;

        int reward = 200 + (currentWave * 100);
        gold += reward;
        currentWave++;
        zombiesPerWave += 2;

        shopUI.SetActive(true);
        UpdateUI();
    }

    public void OnBaseAttacked(float damage)
    {
        if (isGameOver)
            return;

        baseHealth -= damage;
        UpdateUI();

        if (baseHealth <= 0)
        {
            DoGameOver();
        }
    }

    void DoGameOver()
    {
        isGameOver = true;

        if (gameOverAudio != null)
            gameOverAudio.Play();

        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        ZombieController[] allZombies = FindObjectsByType<ZombieController>(FindObjectsSortMode.None);
        foreach (ZombieController zombie in allZombies)
        {
            Destroy(zombie.gameObject);
        }

        StopAllCoroutines();
    }

    // --- 상점 기능 (UI 버튼에 연결) ---
    public void BuyRepair()
    {
        if (gold >= 100)
        {
            gold -= 100;

            baseHealth = Mathf.Min(baseHealth + 30, 100); // 최대 100까지 회복

            UpdateUI();
        }
    }

    public void BuyMagazine()
    {
        if (gold >= 50)
        {
            gold -= 50;

            Instantiate(magPrefab, magSpawnPoint.position, Quaternion.identity);

            UpdateUI();
        }
    }

    void UpdateUI()
    {
        // 좌하단: 기지 체력
        if (hpText != null)
            hpText.text = $"BASE HP : {baseHealth} / 100";

        // 우하단: 웨이브 및 골드
        if (infoText != null)
            infoText.text = $"WAVE: {currentWave}\nGOLD: {gold}";
    }


}