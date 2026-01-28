using UnityEngine;
using System.Collections;

public class Glass : MonoBehaviour
{
    public int level;
    [HideInInspector] public bool canMerge = false;

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

