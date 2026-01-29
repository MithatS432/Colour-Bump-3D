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
    private bool isStatic = false;

    void Start()
    {
        // BOYUT AYARLA
        transform.localScale = Vector3.one * sizeMultiplier;

        // RİGİDBODY AYARLA
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = throwWeight * sizeMultiplier;
            rb.linearDamping = 0.8f; // Daha fazla sürtünme
            rb.angularDamping = 0.8f; // Daha fazla dönüş sürtünmesi
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
        // İLK BEKLEME
        yield return new WaitForSeconds(delay);

        // HAREKET KONTROLÜ
        float checkDuration = 3f; // Maksimum kontrol süresi
        float elapsed = 0f;

        while (elapsed < checkDuration)
        {
            // Hız kontrolü
            if (rb != null)
            {
                float speed = rb.linearVelocity.magnitude;

                // Eğer durdu kontrol et
                if (speed < 0.1f && !isStatic)
                {
                    isStatic = true;
                    yield return new WaitForSeconds(0.5f); // Son kontrol için bekle

                    // Hala duruyor mu?
                    if (rb.linearVelocity.magnitude < 0.1f)
                    {
                        break; // Tamamen durdu
                    }
                    else
                    {
                        isStatic = false; // Tekrar hareket etti
                    }
                }
            }

            elapsed += Time.deltaTime;
            yield return new WaitForSeconds(0.2f);
        }

        // SONUÇ KONTROLÜ
        if (!reachedLimit)
        {
            // Limiti geçemedi - Oyunu kaybet
            if (GameManager.Instance != null && !GameManager.Instance.IsGameOver)
            {
                Debug.Log($"[{gameObject.name}] Bardak limiti geçemedi! Oyun kaybediliyor...");
                GameManager.Instance.LoseGameByLimit();
            }
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Bardak başarıyla limiti geçti!");
        }
    }

    // LimitTrigger'a değdiğinde çağrılır
    public void OnReachedLimit()
    {
        if (reachedLimit) return; // Tekrar etme

        reachedLimit = true;
        Debug.Log($"[{gameObject.name}] Limit geçildi! ✓");

        // Limit geçtikten sonra fizik değişiklikleri
        if (rb != null)
        {
            rb.linearDamping = 1.5f; // Daha hızlı yavaşlama
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // GlassLimit tag kontrolü
        if (other.CompareTag("GlassLimit"))
        {
            OnReachedLimit();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Yere çarptığında hafif zıplama ekle (daha doğal)
        if (collision.gameObject.CompareTag("Ground") && rb != null)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.8f, rb.linearVelocity.y * 0.3f, rb.linearVelocity.z * 0.8f);
        }
    }
}