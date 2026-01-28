using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndThrow : MonoBehaviour
{
    public float minForce = 5f;
    public float maxForce = 12f;
    public float chargeTime = 1.2f;
    public float sideSensitivity = 0.005f;
    public float maxSideOffset = 0.5f;

    [Header("Power Bar Settings")]
    [Range(0.1f, 3f)]
    public float powerSensitivity = 1.5f;
    public AnimationCurve powerCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float holdTime;
    private float sideOffset;
    private bool isCharging;
    private bool isCurrent;
    private Vector3 startPos;
    private Rigidbody rb;
    private Camera cam;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        startPos = transform.position;
        cam = Camera.main;
    }

    public void SetAsCurrent()
    {
        isCurrent = true;
        isCharging = false;
        holdTime = 0f;
        sideOffset = 0f;
        startPos = transform.position;
        GameManager.Instance.SetPower(0f);
    }

    void Update()
    {
        if (!isCurrent) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        HandleMouse();
        HandleTouch();
    }

    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0) && IsPointerOnThisGlass(Input.mousePosition))
        {
            isCharging = true;
            holdTime = 0f;
            GameManager.Instance.PlayChargeSound();
        }

        if (Input.GetMouseButton(0) && isCharging)
        {
            holdTime += Time.deltaTime;
            HandleSideMovement(Input.GetAxis("Mouse X"));
            UpdatePower();
        }

        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            Throw();
        }
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began && IsPointerOnThisGlass(touch.position))
        {
            isCharging = true;
            holdTime = 0f;
            GameManager.Instance.PlayChargeSound();
        }

        if (touch.phase == TouchPhase.Moved && isCharging)
        {
            holdTime += Time.deltaTime;
            HandleSideMovement(touch.deltaPosition.x * 0.01f);
            UpdatePower();
        }

        if (touch.phase == TouchPhase.Ended && isCharging)
        {
            Throw();
        }
    }

    bool IsPointerOnThisGlass(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
            return hit.collider.gameObject == gameObject;
        return false;
    }

    void HandleSideMovement(float deltaX)
    {
        sideOffset += deltaX * sideSensitivity;
        sideOffset = Mathf.Clamp(sideOffset, -maxSideOffset, maxSideOffset);
        Vector3 pos = startPos;
        pos.x += sideOffset;
        transform.position = pos;
    }

    // GÜNCELLENMİŞ POWER BAR KONTROLÜ
    void UpdatePower()
    {
        float raw = Mathf.Clamp01(holdTime / chargeTime);
        float smooth = powerCurve.Evaluate(raw);
        float finalPower = Mathf.Pow(smooth, powerSensitivity);
        GameManager.Instance.SetPower(finalPower);
    }

    void Throw()
    {
        isCharging = false;
        isCurrent = false;
        rb.isKinematic = false;
        GetComponent<Glass>().canMerge = true;

        float raw = Mathf.Clamp01(holdTime / chargeTime);
        float t = powerCurve.Evaluate(raw);
        t = Mathf.Pow(t, powerSensitivity);
        float finalForce = Mathf.Lerp(minForce, maxForce, t);

        Vector3 dir = new Vector3(sideOffset, 0.15f, 1f).normalized;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        float weight = GetComponent<Glass>().throwWeight;
        rb.linearVelocity = dir * finalForce * weight;

        GameManager.Instance.StopChargeSound();
        GameManager.Instance.SetPower(0f);
        GameManager.Instance.AllowNextSpawn();
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StopChargeSound();
    }
}