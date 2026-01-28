using UnityEngine;

public class MergeManager : MonoBehaviour
{
    private bool merged;

    void OnCollisionEnter(Collision collision)
    {
        if (merged) return;

        Glass self = GetComponent<Glass>();
        Glass other = collision.gameObject.GetComponent<Glass>();

        if (!other) return;
        if (!self.canMerge || !other.canMerge) return;
        if (self.level != other.level) return;

        if (GetInstanceID() > other.GetInstanceID()) return;

        merged = true;

        Vector3 pos = (transform.position + other.transform.position) / 2f;

        GameManager.Instance.AddScore(self.level);
        MissionManager.Instance.OnMerge();
        MissionManager.Instance.OnMergeColor(self.color);
        GameManager.Instance.PlayMergeSound();
        GameManager.Instance.PlayMergeVFX(pos);

        GameObject next = GameManager.Instance.GetNextGlass(self.level + 1);

        Destroy(other.gameObject);
        Destroy(gameObject);

        if (next != null)
        {
            GameObject g = Instantiate(next, pos, Quaternion.identity);
            Glass glass = g.GetComponent<Glass>();
            glass.canMerge = false;

        }
    }
}
