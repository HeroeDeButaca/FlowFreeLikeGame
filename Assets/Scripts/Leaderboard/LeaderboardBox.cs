using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardBox : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _leaderboardPositionText;
    [SerializeField]
    private Image _iconImage;
    [SerializeField]
    private TMP_Text _playerNameText;
    [SerializeField]
    private TMP_Text _pointsText;

    public void SetLeaderboardBox(int position, Sprite iconSprite, string playerName, int points)
    {
        _leaderboardPositionText.text = "#" + position.ToString("0");
        _iconImage.sprite = iconSprite;
        _playerNameText.text = playerName;
        _pointsText.text = points.ToString("0");
    }
}
