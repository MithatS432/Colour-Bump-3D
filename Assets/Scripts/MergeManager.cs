using UnityEngine;

public class MergeManager : MonoBehaviour
{
    private bool merged;

    private void OnCollisionEnter(Collision collision)
    {
        if (merged) return;

        Glass self = GetComponent<Glass>();
        Glass other = collision.gameObject.GetComponent<Glass>();

        if (self == null || other == null) return;
        if (!self.canMerge || !other.canMerge) return;
        if (self.level != other.level) return;

        // Çift birleşme önleme
        if (GetInstanceID() > other.GetInstanceID()) return;

        merged = true;

        // Orta nokta hesaplama
        Vector3 pos = (transform.position + other.transform.position) * 0.5f;

        // Skor ve mission güncelleme
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

        // Efektler
        GameManager.Instance.PlayMergeSound();
        GameManager.Instance.PlayMergeVFX(pos);

        // Yeni seviye bardağı oluştur
        GameObject next = GameManager.Instance.GetNextGlass(self.level + 1);

        if (next != null)
        {
            GameObject g = Instantiate(next, pos, Quaternion.identity);
            Glass glass = g.GetComponent<Glass>();

            // Yeni bardak başlangıçta birleşemez
            glass.canMerge = false;

            // Rigidbody kontrolü
            Rigidbody rb = g.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // Fizik aktif
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        GameManager.Instance.OnGlassMerged();

        // Objeleri yok et
        Destroy(other.gameObject);
        Destroy(gameObject);
    }
}