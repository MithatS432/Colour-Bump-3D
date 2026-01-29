using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DragAndThrow : MonoBehaviour
{
    [Header("Force")]
    public float minForce = 3f;
    public float maxForce = 10f;

    [Header("Charge")]
    public float chargeTime = 2f;
    public float minHoldTime = 0.3f; // Minimum basma süresi arttırıldı

    [Header("Side Movement")]
    public float sideSensitivity = 1.2f;
    public float maxSideOffset = 1.5f;
    public float moveSpeed = 8f; // Doğrudan hareket hızı

    [Header("Power Bar")]
    public float powerSensitivity = 3f; // Daha yavaş güç artışı
    public AnimationCurve powerCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Throw Physics")]
    public float throwUpAngle = 0.25f; // Daha az yukarı açı
    public float forceMultiplier = 0.85f; // Genel güç çarpanı azaltıldı
    public float drag = 0.5f; // Hava sürtünmesi arttırıldı

    // PRIVATE VARIABLES
    private float holdTime;
    private float targetX; // Hedef X pozisyonu
    private bool isCharging;
    private bool isCurrent;
    private bool isDragging = false;

    private Vector3 startPos;
    private Rigidbody rb;
    private Camera cam;
    private float initialTouchX;
    private float minX;
    private float maxX;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.linearDamping = 0.5f; // Drag ile senkronize
        startPos = transform.position;
        cam = Camera.main;

        // Hareket limitlerini hesapla
        minX = startPos.x - maxSideOffset;
        maxX = startPos.x + maxSideOffset;
        targetX = startPos.x;
    }

    public void SetAsCurrent()
    {
        isCurrent = true;
        isCharging = false;
        isDragging = false;
        holdTime = 0f;
        startPos = transform.position;
        targetX = startPos.x;
        minX = startPos.x - maxSideOffset;
        maxX = startPos.x + maxSideOffset;

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

        // POZİSYON GÜNCELLEME - Yumuşak hareket
        if (isDragging || Mathf.Abs(transform.position.x - targetX) > 0.01f)
        {
            float newX = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * moveSpeed);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }

        // KONTROLLER
        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsEditor)
            HandleMouse();
        else
            HandleTouch();
    }

    #region TOUCH CONTROLS
    void HandleTouch()
    {
        if (Input.touchCount == 0)
        {
            if (isDragging && isCharging)
            {
                Throw();
            }
            isDragging = false;
            return;
        }

        Touch touch = Input.GetTouch(0);
        Vector3 touchWorldPos = cam.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, cam.WorldToScreenPoint(transform.position).z));

        switch (touch.phase)
        {
            case TouchPhase.Began:
                if (IsTouchOnGlass(touch.position))
                {
                    isDragging = true;
                    initialTouchX = touchWorldPos.x;
                    StartCharge();
                }
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (isDragging && isCharging)
                {
                    holdTime += Time.deltaTime;

                    // DOĞRUDAN WORLD POZİSYONU İLE HAREKET
                    float deltaX = touchWorldPos.x - initialTouchX;
                    float newTargetX = startPos.x + (deltaX * sideSensitivity);
                    targetX = Mathf.Clamp(newTargetX, minX, maxX);

                    UpdatePower();
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (isDragging && isCharging)
                {
                    Throw();
                }
                isDragging = false;
                break;
        }
    }

    bool IsTouchOnGlass(Vector2 touchPos)
    {
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
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.WorldToScreenPoint(transform.position).z));

        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOnThisGlass(Input.mousePosition))
            {
                isDragging = true;
                initialTouchX = mouseWorldPos.x;
                StartCharge();
            }
        }

        if (Input.GetMouseButton(0) && isDragging && isCharging)
        {
            holdTime += Time.deltaTime;

            float deltaX = mouseWorldPos.x - initialTouchX;
            float newTargetX = startPos.x + (deltaX * sideSensitivity);
            targetX = Mathf.Clamp(newTargetX, minX, maxX);

            UpdatePower();
        }

        if (Input.GetMouseButtonUp(0) && isDragging && isCharging)
        {
            Throw();
            isDragging = false;
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
        isDragging = false;

        // FİZİĞİ AKTİF ET
        rb.isKinematic = false;

        // GÜÇ HESAPLA
        float raw = Mathf.Clamp01(holdTime / chargeTime);
        float t = Mathf.Pow(powerCurve.Evaluate(raw), powerSensitivity);
        float finalForce = Mathf.Lerp(minForce, maxForce, t);

        // YÖN HESAPLA - Yan hareket dahil
        float sideOffset = transform.position.x - startPos.x;
        Vector3 dir = new Vector3(sideOffset * 0.8f, throwUpAngle, 1f).normalized;

        // ANLIK GÜÇ UYGULA (Fiziksel)
        rb.AddForce(dir * finalForce * forceMultiplier, ForceMode.VelocityChange);

        GetComponent<Glass>().canMerge = true;
        GameManager.Instance.StopChargeSound();
        GameManager.Instance.SetPower(0f);
        GameManager.Instance.UseMove();

        // LİMİT KONTROLÜNÜ BAŞLAT
        GetComponent<Glass>().StartLimitCheck(0.9f);

        // YENİ BARDAK
        Invoke(nameof(AllowNextSpawn), 0.3f);
    }

    void AllowNextSpawn()
    {
        GameManager.Instance.AllowNextSpawn();
    }

    void CancelCharge()
    {
        isCharging = false;
        isDragging = false;
        GameManager.Instance.StopChargeSound();
        GameManager.Instance.SetPower(0f);
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StopChargeSound();
    }
}