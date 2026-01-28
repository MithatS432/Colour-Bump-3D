using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Glasses")]
    public GameObject[] glasses;
    public Transform glassSpawnPoint;

    [Header("UI")]
    public Image powerBar;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioSource sfxOneShotSource;
    public AudioClip chargeLoopClip;
    public AudioClip hitClip;
    public AudioClip mergeClip;
    public AudioClip breakClip;

    [Header("VFX")]
    public ParticleSystem mergeVFX;

    private bool canSpawn = true;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetPower(0f);
        SpawnRandomGlass();
    }

    // SPAWN
    public void SpawnRandomGlass()
    {
        if (!canSpawn) return;
        canSpawn = false;

        int randomIndex = Random.Range(0, glasses.Length);
        GameObject glass = Instantiate(glasses[randomIndex], glassSpawnPoint.position, Quaternion.identity);
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

    // UI
    public void SetPower(float value)
    {
        if (powerBar != null)
            powerBar.fillAmount = value;
    }

    // AUDIO
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

    // VFX
    public void PlayMergeVFX(Vector3 position)
    {
        if (mergeVFX == null) return;
        ParticleSystem vfx = Instantiate(mergeVFX, position, Quaternion.identity);
        Destroy(vfx.gameObject, 2f);
    }
}