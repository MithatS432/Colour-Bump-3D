using UnityEngine;

public class MergeManager : MonoBehaviour
{
    private bool merged = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (merged) return;

        Glass other = collision.gameObject.GetComponent<Glass>();
        Glass self = GetComponent<Glass>();

        if (other == null) return;
        if (!self.canMerge || !other.canMerge) return;
        if (other.level != self.level) return;

        if (GetInstanceID() > collision.gameObject.GetInstanceID())
            return;

        merged = true;

        Vector3 spawnPos =
            (transform.position + collision.transform.position) / 2f;

        GameManager.Instance.PlayMergeSound();
        GameManager.Instance.PlayMergeVFX(spawnPos);

        Destroy(other.gameObject);
        Destroy(gameObject);

        GameObject next =
            GameManager.Instance.GetNextGlass(self.level);

        if (next != null)
        {
            GameObject newGlass =
                Instantiate(next, spawnPos, Quaternion.identity);

            newGlass.GetComponent<Glass>().canMerge = false;
        }
    }
}
