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
        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        if (collision.gameObject.GetComponent<Glass>())
            GameManager.Instance.PlayHitSound();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Break"))
        {
            GameManager.Instance.OnGlassBroken();
            GameManager.Instance.PlayBreakSound(transform.position);
            Destroy(gameObject);
        }
    }
}
