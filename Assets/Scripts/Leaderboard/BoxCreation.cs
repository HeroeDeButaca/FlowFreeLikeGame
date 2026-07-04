using UnityEngine;
using TMPro;
using Proyecto26;
using System.Collections;

public class BoxCreation : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _modeIdInput;
    [SerializeField]
    private TMP_InputField _usernameInput;
    [SerializeField]
    private TMP_InputField _scoreInput;

    private LeaderboardData _leaderboardData;

    private const string _databaseLink = "https://flowfreelikegameleaderboard-default-rtdb.europe-west1.firebasedatabase.app/";

    void Start()
    {
        
    }

    public void OnSubmit()
    {
        int modeId = int.Parse(_modeIdInput.text);
        string playerName = _usernameInput.text;
        int score = int.Parse(_scoreInput.text);

        _leaderboardData = new LeaderboardData(modeId, playerName, 0, score);

        PostToDatabase(modeId, playerName, 0, score);
    }

    private void PostToDatabase(int modeId, string playerName, int iconId, int newScore)
    {
        string key = $"{playerName}_Mode{modeId}";
        string url = $"{_databaseLink}{modeId}/{key}.json";

        RestClient.Get(url).Then(response =>
        {
            LeaderboardData data;

            if (response != null && !string.IsNullOrEmpty(response.Text) && response.Text != "null")
            {
                data = JsonUtility.FromJson<LeaderboardData>(response.Text);
            }
            else
            {
                data = new LeaderboardData(modeId, playerName, iconId, newScore);
            }

            if (data.TotalPoints < newScore)
                data.TotalPoints = newScore;

            data.PlayerName = playerName;
            data.ModeId = modeId;
            data.IconId = iconId;

            RestClient.Put(url, data)
                .Then(_ => Debug.Log("Score updated"))
                .Catch(err => Debug.LogError(err));
        })
        .Catch(err =>
        {
            Debug.LogError($"GET error: {err}");
        });
    }
}
