using UnityEngine;

public class MergeManager : MonoBehaviour
{
    private bool merged = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (merged) return;

        Glass self = GetComponent<Glass>();
        Glass other = collision.gameObject.GetComponent<Glass>();

        if (!other) return;
        if (!self.canMerge || !other.canMerge) return;
        if (self.level != other.level) return;

        if (GetInstanceID() > collision.gameObject.GetInstanceID())
            return;

        merged = true;

        Vector3 spawnPos = (transform.position + collision.transform.position) / 2f;

        GameManager.Instance.AddScore(self.level);
        GameManager.Instance.PlayMergeSound();
        GameManager.Instance.PlayMergeVFX(spawnPos);

        int nextLevel = self.level + 1;
        GameObject nextGlass = GameManager.Instance.GetNextGlass(nextLevel);

        Destroy(other.gameObject);
        Destroy(gameObject);

        if (nextGlass != null)
        {
            Rigidbody rb = Instantiate(
                nextGlass,
                spawnPos,
                Quaternion.identity
            ).GetComponent<Rigidbody>();

            if (rb != null)
                rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
        }
    }
}
