using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInfoManager : MonoBehaviour
{
    [SerializeField]
    private Image _playerIconImage;

    private TMP_Text _playerNameText;

    public static PlayerInfoManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _playerNameText = GetComponentInChildren<TMP_Text>();
    }

    public void ChangeIcon(Sprite iconSprite) { _playerIconImage.sprite = iconSprite; }
    public void ChangeName(string playerName) { _playerNameText.text = playerName; }
}
