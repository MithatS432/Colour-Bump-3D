using UnityEngine;

public class GlassCollision : MonoBehaviour
{
    private float lastHitTime;
    public float hitCooldown = 0.15f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Glass other = collision.gameObject.GetComponent<Glass>();
        if (!other) return;

        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        GameManager.Instance.PlayHitSound();

        Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
        if (!otherRb) return;

        if (rb.mass > otherRb.mass)
        {
            Vector3 pushDir = (otherRb.position - rb.position).normalized;

            float pushPower = Mathf.Clamp((rb.mass / otherRb.mass) * 0.3f, 0.2f, 0.6f);
            otherRb.AddForce(pushDir * pushPower, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Break"))
        {
            GameManager.Instance.PlayBreakSound(transform.position);
            Destroy(gameObject);
        }
    }
}
