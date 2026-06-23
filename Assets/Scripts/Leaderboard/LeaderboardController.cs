using Proyecto26;
using System;
using UnityEngine;

public class LeaderboardData
{
    public int ModeId;

    public string PlayerName;
    public int IconId;
    public int TotalPoints;

    public LeaderboardData() { }

    public LeaderboardData(int modeId, string playerName, int iconId, int points)
    {
        ModeId = modeId;
        PlayerName = playerName;
        IconId = iconId;
        TotalPoints = points;
    }
}

public class LeaderboardController : MonoBehaviour
{
    [SerializeField]
    private GameObject _prefabLeaderboard;

    [SerializeField]
    private Transform _contentLeaderboard;

    private LeaderboardData[] _leaderboardData;

    void Start()
    {
        
    }

    private void RetrieveFromDatabase(int modeId)
    {
        try
        {
            RestClient.GetArray<LeaderboardData>("https://flowfreelikegameleaderboard-default-rtdb.europe-west1.firebasedatabase.app/" + modeId.ToString("0") + "/").Then(response =>
            {

            });
        }
        catch(Exception e)
        {
            Debug.LogError("Error from RetrieveFromDatabase:\n" + e);
        }
        
    }
}
