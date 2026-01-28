using UnityEngine;
using TMPro;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    [Header("UI")]
    public TextMeshProUGUI missionText;

    [Header("Missions")]
    public string[] missions;

    private string currentMission;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateNewMission();
    }

    public void GenerateNewMission()
    {
        int randomIndex = Random.Range(0, missions.Length);
        currentMission = missions[randomIndex];
        missionText.text = currentMission;
    }

    public void OnMissionCompleted()
    {
        GenerateNewMission();
    }

    public void OnMissionFailed()
    {
        GenerateNewMission();
    }
}
