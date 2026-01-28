using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public Image[] healthBars;              // 3 Image
    public Sprite fullHealthSprite;         // ❤️
    public Sprite emptyHealthSprite;        // 💀

    [Header("Game Values")]
    public int moveCount = 15;
    public int health = 3;
    public int scoreCount = 0;
    public bool IsGameOver { get; private set; } = false;

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
    private int currentVfxIndex = 0;

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
        if (!canSpawn) return;
        canSpawn = false;

        int randomIndex = Random.Range(0, glasses.Length);
        GameObject glass = Instantiate(
            glasses[randomIndex],
            glassSpawnPoint.position,
            Quaternion.identity
        );

        glass.GetComponent<DragAndThrow>().SetAsCurrent();
    }

    public void AllowNextSpawn()
    {
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
        if (powerBar != null)
            powerBar.fillAmount = value;
    }
    public void UpdateUI()
    {
        scoreText.text = "Score:" + scoreCount.ToString();
        moveLeftText.text = moveCount.ToString();

        for (int i = 0; i < healthBars.Length; i++)
        {
            int index = healthBars.Length - 1 - i;

            if (i < health)
                healthBars[index].sprite = fullHealthSprite;
            else
                healthBars[index].sprite = emptyHealthSprite;
        }
    }


    // ================= GAME LOGIC =================
    public void UseMove()
    {
        moveCount--;
        UpdateUI();

        if (moveCount <= 0)
        {
            sfxSource.PlayOneShot(loseClip);
            IsGameOver = true;
            Invoke("RestartGame", 2f);
        }
    }

    public void OnGlassBroken()
    {
        health--;
        UpdateUI();

        if (health <= 0)
        {
            sfxSource.PlayOneShot(loseClip);
            IsGameOver = true;
            Invoke("RestartGame", 2f);
        }
    }

    public void AddScore(int level)
    {
        int gained = scorePerLevel[level - 1];
        scoreCount += gained;
        UpdateUI();
    }

    void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    // ================= AUDIO =================
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

    public void PlayHitSound()
    {
        if (sfxSource == null || hitClip == null) return;
        sfxSource.PlayOneShot(hitClip);
    }

    public void PlayMergeSound()
    {
        if (sfxOneShotSource == null || mergeClip == null) return;
        sfxOneShotSource.PlayOneShot(mergeClip);
    }

    public void PlayBreakSound(Vector3 position)
    {
        if (breakClip == null) return;
        AudioSource.PlayClipAtPoint(breakClip, position);
    }

    public void PlayMergeVFX(Vector3 position)
    {
        if (mergeVFXList == null || mergeVFXList.Length == 0) return;

        ParticleSystem vfxPrefab = mergeVFXList[currentVfxIndex];
        ParticleSystem vfx = Instantiate(vfxPrefab, position, Quaternion.identity);
        Destroy(vfx.gameObject, 2f);

        currentVfxIndex++;

        if (currentVfxIndex >= mergeVFXList.Length)
            currentVfxIndex = 0;
    }

}
