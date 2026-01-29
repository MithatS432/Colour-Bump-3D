using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DragAndThrow : MonoBehaviour
{
    [Header("Force")]
    public float minForce = 6f;
    public float maxForce = 14f;

    [Header("Charge")]
    public float chargeTime = 2f;
    public float minHoldTime = 0.2f;

    [Header("Side Movement")]
    public float sideSensitivity = 0.8f;
    public float maxSideOffset = 0.7f;
    public float sideSmoothSpeed = 15f;

    [Header("Power Bar")]
    public float powerSensitivity = 2f;
    public AnimationCurve powerCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Throw Physics")]
    public float throwUpAngle = 0.3f;
    public float forceMultiplier = 12f;
    public float drag = 0.5f; // Hava sürtünmesi

    // PRIVATE VARIABLES
    private float holdTime;
    private float targetSideOffset;
    private float currentSideOffset;
    private bool isCharging;
    private bool isCurrent;
    private bool touchStartedOnGlass = false;

    private Vector3 startPos;
    private Rigidbody rb;
    private Camera cam;
    private Vector2 touchStartPos;
    private Vector3 velocityBeforeThrow;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.linearDamping = drag; // Sürtünme ekle
        startPos = transform.position;
        cam = Camera.main;
    }

    public void SetAsCurrent()
    {
        isCurrent = true;
        isCharging = false;
        holdTime = 0f;
        targetSideOffset = 0f;
        currentSideOffset = 0f;
        touchStartedOnGlass = false;
        startPos = transform.position;

        // Rigidbody sıfırla
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        GameManager.Instance.SetPower(0f);
    }

    void Update()
    {
        if (!isCurrent) return;
        if (GameManager.Instance.IsGameOver) return;

        // YUMUŞAK HAREKET
        float smoothTime = Time.deltaTime * sideSmoothSpeed;
        currentSideOffset = Mathf.Lerp(currentSideOffset, targetSideOffset, smoothTime);

        // POZİSYONU GÜNCELLE (teleport yok)
        Vector3 newPos = startPos + Vector3.right * currentSideOffset;
        transform.position = Vector3.Lerp(transform.position, newPos, smoothTime);

        // KONTROLLER
        if (Application.isEditor)
            HandleMouse();
        else
            HandleTouch();
    }

    #region TOUCH CONTROLS
    void HandleTouch()
    {
        if (Input.touchCount == 0)
        {
            if (touchStartedOnGlass)
            {
                touchStartedOnGlass = false;
            }
            return;
        }

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                if (IsTouchOnGlass(touch.position))
                {
                    touchStartedOnGlass = true;
                    touchStartPos = touch.position;
                    StartCharge();
                }
                break;

            case TouchPhase.Moved:
                if (isCharging && touchStartedOnGlass)
                {
                    holdTime += Time.deltaTime;

                    // GERÇEKÇİ YAN HAREKET
                    float deltaX = (touch.position.x - touchStartPos.x) / Screen.width * 100f;
                    targetSideOffset = Mathf.Clamp(deltaX * sideSensitivity, -maxSideOffset, maxSideOffset);

                    UpdatePower();
                }
                break;

            case TouchPhase.Stationary:
                if (isCharging && touchStartedOnGlass)
                {
                    holdTime += Time.deltaTime;
                    UpdatePower();
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (isCharging && touchStartedOnGlass)
                {
                    Throw();
                }
                touchStartedOnGlass = false;
                break;
        }
    }

    bool IsTouchOnGlass(Vector2 touchPos)
    {
        // Collider bazlı kontrol (daha doğru)
        Ray ray = cam.ScreenPointToRay(touchPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            return hit.collider.gameObject == gameObject;
        }
        return false;
    }
    #endregion

    #region MOUSE CONTROLS
    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOnThisGlass(Input.mousePosition))
            {
                touchStartedOnGlass = true;
                touchStartPos = Input.mousePosition;
                StartCharge();
            }
        }

        if (Input.GetMouseButton(0) && isCharging && touchStartedOnGlass)
        {
            holdTime += Time.deltaTime;

            float deltaX = (Input.mousePosition.x - touchStartPos.x) / Screen.width * 100f;
            targetSideOffset = Mathf.Clamp(deltaX * sideSensitivity, -maxSideOffset, maxSideOffset);

            UpdatePower();
        }

        if (Input.GetMouseButtonUp(0) && isCharging && touchStartedOnGlass)
        {
            Throw();
            touchStartedOnGlass = false;
        }
    }

    bool IsPointerOnThisGlass(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            return hit.collider.gameObject == gameObject;
        }
        return false;
    }
    #endregion

    void StartCharge()
    {
        isCharging = true;
        holdTime = 0f;
        GameManager.Instance.PlayChargeSound();
    }

    void UpdatePower()
    {
        if (holdTime < minHoldTime)
        {
            GameManager.Instance.SetPower(0f);
            return;
        }

        float raw = Mathf.Clamp01(holdTime / chargeTime);
        float smooth = powerCurve.Evaluate(raw);
        float finalPower = Mathf.Pow(smooth, powerSensitivity);

        GameManager.Instance.SetPower(finalPower);
    }

    void Throw()
    {
        if (holdTime < minHoldTime)
        {
            CancelCharge();
            return;
        }

        isCharging = false;
        isCurrent = false;

        // FİZİĞİ AKTİF ET
        rb.isKinematic = false;

        // GÜÇ HESAPLA
        float raw = Mathf.Clamp01(holdTime / chargeTime);
        float t = Mathf.Pow(powerCurve.Evaluate(raw), powerSensitivity);
        float finalForce = Mathf.Lerp(minForce, maxForce, t);

        // YÖN HESAPLA (daha doğal)
        Vector3 dir = new Vector3(currentSideOffset * 1.5f, throwUpAngle, 1f).normalized;

        // KADEMELİ GÜÇ UYGULA (anlık değil)
        StartCoroutine(ApplyForceOverTime(dir, finalForce));

        GetComponent<Glass>().canMerge = true;
        GameManager.Instance.StopChargeSound();
        GameManager.Instance.SetPower(0f);
        GameManager.Instance.UseMove();

        // YENİ BARDAK (gecikmeli)
        Invoke(nameof(AllowNextSpawn), 0.5f);
    }

    // KADEMELİ GÜÇ UYGULAMA (ışınlanma efekti yok)
    IEnumerator ApplyForceOverTime(Vector3 direction, float force)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float currentForce = Mathf.Lerp(0f, force, t);

            rb.AddForce(direction * currentForce * forceMultiplier * Time.deltaTime * 50f, ForceMode.Force);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Limit kontrolü başlat
        GetComponent<Glass>().StartLimitCheck(1.5f);
    }

    void AllowNextSpawn()
    {
        GameManager.Instance.AllowNextSpawn();
    }

    void CancelCharge()
    {
        isCharging = false;
        touchStartedOnGlass = false;
        targetSideOffset = 0f;
        GameManager.Instance.StopChargeSound();
        GameManager.Instance.SetPower(0f);
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StopChargeSound();
    }
}