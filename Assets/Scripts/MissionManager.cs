using UnityEngine;
using TMPro;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    public TextMeshProUGUI missionText;
    public Mission[] missions;

    private Mission currentMission;
    private int progress;

    public bool MissionCompleted { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartMission();
    }

    void StartMission()
    {
        MissionCompleted = false;
        progress = 0;

        if (missions == null || missions.Length == 0)
        {
            missionText.text = "No missions";
            return;
        }

        currentMission = missions[Random.Range(0, missions.Length)];
        missionText.text = currentMission.description;
    }


    // ===== SCORE =====
    public void OnScoreChanged(int totalScore)
    {
        if (MissionCompleted) return;
        if (currentMission.type != MissionType.Score) return;

        if (totalScore >= currentMission.targetValue)
            CompleteMission();
    }

    // ===== MERGE =====
    public void OnMerge()
    {
        if (MissionCompleted) return;

        if (currentMission.type != MissionType.MergeAny)
            return;

        progress++;

        if (progress >= currentMission.targetValue)
            CompleteMission();
    }
    public void OnMergeColor(GlassColor color)
    {
        if (MissionCompleted) return;
        if (currentMission.type != MissionType.MergeColor) return;

        if (color != currentMission.targetColor) return;

        progress++;

        if (progress >= currentMission.targetValue)
            CompleteMission();
    }

    void CompleteMission()
    {
        MissionCompleted = true;

        if (missionText != null)
            missionText.text = "Mission Completed";

        GameManager.Instance.WinGame();
    }
    public MissionType CurrentMissionType
    {
        get { return currentMission.type; }
    }
}
