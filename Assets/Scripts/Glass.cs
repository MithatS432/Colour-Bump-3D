using UnityEngine;
using System.Collections;

public enum GlassColor
{
    Pink,
    White,
    Green,
    Blue,
    Brown,
    Yellow
}
public enum MissionType
{
    Score,
    MergeAny,
    MergeColor
}
[System.Serializable]
public class Mission
{
    public MissionType type;
    public int targetValue;
    public GlassColor targetColor;
    public string description;
}


public class Glass : MonoBehaviour
{
    public int level;
    public GlassColor color;
    [HideInInspector] public bool canMerge = false;

    [Header("Physics")]
    public float sizeMultiplier = 1f;
    public float throwWeight = 1f;

    private bool reachedLimit = false;
    private bool limitCheckStarted = false;
    private Rigidbody rb;

    void Start()
    {
        // BOYUT AYARLA
        transform.localScale = Vector3.one * sizeMultiplier;

        // RİGİDBODY AYARLA
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = throwWeight * sizeMultiplier;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;
        }

        StartCoroutine(EnableMerge());
    }

    IEnumerator EnableMerge()
    {
        yield return new WaitForSeconds(0.15f);
        canMerge = true;
    }

    public void StartLimitCheck(float delay)
    {
        if (limitCheckStarted) return;

        limitCheckStarted = true;
        StartCoroutine(LimitCheckCoroutine(delay));
    }

    IEnumerator LimitCheckCoroutine(float delay)
    {
        // ÖNCE BEKLE
        yield return new WaitForSeconds(delay);

        // HALA HAREKET EDİYOR MU KONTROL ET
        while (rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            yield return new WaitForSeconds(0.5f);
        }

        // DURDU VE LİMİTE ULAŞMADIYSA KAYBET
        if (!reachedLimit && GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        {
            Debug.Log("Bardak limiti geçemedi! Oyun kaybediliyor...");
            GameManager.Instance.LoseGameByLimit();
        }
    }

    // LimitTrigger'a değdiğinde ÇAĞRILACAK
    public void OnReachedLimit()
    {
        reachedLimit = true;
        Debug.Log("Bardak limiti geçti!");

        // OPTİONAL: Limit geçince fizik değiştir
        if (rb != null)
        {
            rb.linearDamping = 1f; // Daha çok sürtünme
        }
    }
    public void ReachedLimit()
    {
        reachedLimit = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // LIMIT TAG'İNİ KONTROL ET
        if (other.CompareTag("GlassLimit"))
        {
            OnReachedLimit();
        }
    }
}