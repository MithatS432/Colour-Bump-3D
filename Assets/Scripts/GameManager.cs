using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Glasses")]
    public GameObject[] glasses;
    public Transform glassSpawnPoint;

    [Header("UI")]
    public Image powerBar;
    public TextMeshProUGUI moveLeftText;
    public TextMeshProUGUI scoreText;

    [Header("Health UI")]
    public Image[] healthBars;
    public Sprite fullHealthSprite;
    public Sprite emptyHealthSprite;

    [Header("Game Values")]
    public int moveCount = 20;
    public int health = 3;
    public int scoreCount = 0;
    public bool IsGameOver { get; private set; }

    [Header("Score Settings")]
    public int[] scorePerLevel;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioSource sfxOneShotSource;
    public AudioClip chargeLoopClip;
    public AudioClip hitClip;
    public AudioClip mergeClip;
    public AudioClip breakClip;
    public AudioClip winClip;
    public AudioClip loseClip;

    [Header("VFX")]
    public ParticleSystem[] mergeVFXList;
    private int currentVfxIndex;

    private bool canSpawn = true;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetPower(0f);
        UpdateUI();
        SpawnRandomGlass();
    }

    // ================= SPAWN =================
    public void SpawnRandomGlass()
    {
        if (MissionManager.Instance != null && MissionManager.Instance.MissionCompleted)
            return;

        if (!canSpawn || IsGameOver) return;
        canSpawn = false;

        int index = Random.Range(0, glasses.Length);
        GameObject glass = Instantiate(glasses[index], glassSpawnPoint.position, Quaternion.identity);
        glass.GetComponent<DragAndThrow>().SetAsCurrent();
    }

    public void AllowNextSpawn()
    {
        StartCoroutine(SpawnNextFrame());
    }

    IEnumerator SpawnNextFrame()
    {
        yield return null;
        canSpawn = true;
        SpawnRandomGlass();
    }

    // ================= BİRLEŞME SONRASI SPAWN =================
    public void OnGlassMerged()
    {
        // Birleşme sonrası yeni bardak spawn et
        StartCoroutine(SpawnAfterMerge());
    }

    IEnumerator SpawnAfterMerge()
    {
        // Birleşme animasyonu için bekle
        yield return new WaitForSeconds(0.4f);

        // Yeni bardak spawn et
        canSpawn = true;
        SpawnRandomGlass();
    }

    public GameObject GetNextGlass(int level)
    {
        int index = level - 1;
        if (index < 0 || index >= glasses.Length) return null;
        return glasses[index];
    }

    // ================= UI =================
    public void SetPower(float value)
    {
        powerBar.fillAmount = value;
    }

    public void UpdateUI()
    {
        scoreText.text = "Score: " + scoreCount;
        moveLeftText.text = moveCount.ToString();

        for (int i = 0; i < healthBars.Length; i++)
        {
            int idx = healthBars.Length - 1 - i;
            healthBars[idx].sprite = i < health ? fullHealthSprite : emptyHealthSprite;
        }
    }

    // ================= GAME FLOW =================
    public void WinGame()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        StopChargeSound();
        sfxOneShotSource.PlayOneShot(winClip);
        Invoke(nameof(RestartGame), 2f);
    }

    public void UseMove()
    {
        if (IsGameOver) return;

        moveCount--;
        UpdateUI();

        if (MissionManager.Instance.MissionCompleted) return;

        if (moveCount <= 0)
            LoseGame();
    }

    public void OnGlassBroken()
    {
        if (IsGameOver) return;

        health--;
        UpdateUI();

        if (MissionManager.Instance.MissionCompleted) return;

        if (health <= 0)
            LoseGame();
    }

    void LoseGame()
    {
        IsGameOver = true;
        StopChargeSound();
        sfxOneShotSource.PlayOneShot(loseClip);
        Invoke(nameof(RestartGame), 2f);
    }

    public void LoseGameByLimit()
    {
        if (IsGameOver) return;

        IsGameOver = true;

        if (sfxOneShotSource != null && loseClip != null)
            sfxOneShotSource.PlayOneShot(loseClip);

        Invoke(nameof(RestartGame), 2f);
    }

    public void AddScore(int level)
    {
        if (level - 1 < 0 || level - 1 >= scorePerLevel.Length)
            return;

        int gained = scorePerLevel[level - 1];
        scoreCount += gained;
        UpdateUI();

        MissionManager.Instance.OnScoreChanged(scoreCount);
    }

    void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    // ================= AUDIO & VFX =================
    public void PlayMergeSound()
    {
        sfxOneShotSource.PlayOneShot(mergeClip);
    }

    public void PlayMergeVFX(Vector3 pos)
    {
        if (mergeVFXList.Length == 0) return;

        ParticleSystem vfx = Instantiate(mergeVFXList[currentVfxIndex], pos, Quaternion.identity);
        Destroy(vfx.gameObject, 2f);

        currentVfxIndex = (currentVfxIndex + 1) % mergeVFXList.Length;
    }

    public void PlayHitSound()
    {
        sfxSource.PlayOneShot(hitClip);
    }

    public void PlayBreakSound(Vector3 pos)
    {
        AudioSource.PlayClipAtPoint(breakClip, pos);
    }

    public void PlayChargeSound()
    {
        if (sfxSource == null || chargeLoopClip == null) return;
        if (sfxSource.isPlaying) return;

        sfxSource.clip = chargeLoopClip;
        sfxSource.loop = true;
        sfxSource.Play();
    }

    public void StopChargeSound()
    {
        if (sfxSource == null) return;

        sfxSource.Stop();
        sfxSource.loop = false;
    }

}