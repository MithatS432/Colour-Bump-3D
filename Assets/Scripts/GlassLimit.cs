using UnityEngine;

public class GlassLimit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Glass glass = other.GetComponent<Glass>();
        if (glass != null)
        {
            glass.OnReachedLimit();
            Debug.Log($"GlassLimit: {other.gameObject.name} limiti geçti!");
        }
    }
}