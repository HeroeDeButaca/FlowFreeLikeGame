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
        string fileRequestName = modeId.ToString("0") + "/" + playerName + "_Mode" + modeId.ToString("0");

        StartCoroutine(PostToDatabase(fileRequestName));
    }

    private IEnumerator PostToDatabase(string fileRequestName)
    {
        LeaderboardData oldPlayerData = null;
        bool dataObtained = false;

        RestClient.Get<LeaderboardData>(_databaseLink + fileRequestName + ".json").Then(response =>
        {
            oldPlayerData = response;
            dataObtained = true;
        });

        while (!dataObtained)
            yield return null;

        bool doPutRequest = true;

        if(oldPlayerData != null)
        {
            if (oldPlayerData.TotalPoints > _leaderboardData.TotalPoints)
                doPutRequest = false;
        }

        if(doPutRequest)
            RestClient.Put(_databaseLink + fileRequestName +".json", _leaderboardData);
    }
}
