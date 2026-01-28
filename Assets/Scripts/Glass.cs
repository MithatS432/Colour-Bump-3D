using UnityEngine;
using System.Collections;

public class Glass : MonoBehaviour
{
    public int level;
    [HideInInspector] public bool canMerge = false;

    [Header("Throw Feel")]
    [Range(0.3f, 1.5f)]
    public float throwWeight = 1f;

    void Start()
    {
        StartCoroutine(EnableMerge());
    }

    IEnumerator EnableMerge()
    {
        yield return new WaitForSeconds(0.1f);
        canMerge = true;
    }
}

