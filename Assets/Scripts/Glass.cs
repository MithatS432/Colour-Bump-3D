using UnityEngine;
using System.Collections;

public enum GlassColor
{
    Pink,
    White,
    Green,
    Blue,
    Brown,
    Yellow
}
public enum MissionType
{
    Score,
    MergeAny,
    MergeColor
}
[System.Serializable]
public class Mission
{
    public MissionType type;
    public int targetValue;
    public GlassColor targetColor;
    public string description;
}


public class Glass : MonoBehaviour
{
    public int level;
    public GlassColor color;
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

