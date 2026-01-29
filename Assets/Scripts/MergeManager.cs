using UnityEngine;

public class MergeManager : MonoBehaviour
{
    private bool merged;

    private void OnCollisionEnter(Collision collision)
    {
        if (merged) return;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        Glass self = GetComponent<Glass>();
        Glass other = collision.gameObject.GetComponent<Glass>();

        if (self == null || other == null) return;
        if (!self.canMerge || !other.canMerge) return;
        if (self.level != other.level) return;

        if (GetInstanceID() > other.GetInstanceID()) return;

        merged = true;

        Vector3 pos = (transform.position + other.transform.position) * 0.5f;

        GameManager.Instance.AddScore(self.level);

        MissionManager mm = MissionManager.Instance;

        if (mm != null && !mm.MissionCompleted)
        {
            if (mm.CurrentMissionType == MissionType.MergeAny)
            {
                mm.OnMerge();
            }
            else if (mm.CurrentMissionType == MissionType.MergeColor)
            {
                mm.OnMergeColor(self.color);
            }
        }

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
