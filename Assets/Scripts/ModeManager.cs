using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CompetitiveMode
{
    public int ModeId;

    public LocalizedString LocalizedModeName;
    public string ModeName;
    public TMP_Text ModeTMP;

    public int TotalTime;
    public int TableHeight;
    public int TableWidth;
    public int NodesPerBoard;

    public void UpdateModeName()
    {
        ModeName = LocalizedModeName.GetLocalizedString();
        ModeTMP?.SetText(ModeName);
    }
}

public class ModeManager : MonoBehaviour
{
    [Header("Modes")]
    [SerializeField]
    private CompetitiveMode[] _competitiveModes;

    [SerializeField]
    private Transform _scrollContentModes;

    [SerializeField]
    private GameObject _prefabButtonMode;

    [Space(10)]
    [Header("Mode Panel")]
    [SerializeField]
    private CanvasGroup _modePanelGroup;
    [SerializeField]
    private TMP_Text _modeTitleText;
    [SerializeField]
    private TMP_Text _modeTimeText;
    [SerializeField]
    private TMP_Text _modeHeightText;
    [SerializeField]
    private TMP_Text _modeWidthText;
    [SerializeField]
    private TMP_Text _modeNodesText;

    [SerializeField]
    private Button _playButton;

    void Start()
    {
        SetButtonModes();
    }

    private void SetButtonModes()
    {
        for(int i = 0; i < _competitiveModes.Length; i++)
        {
            CompetitiveMode mode = _competitiveModes[i];
            mode.UpdateModeName();

            GameObject modeButtonGO = Instantiate(_prefabButtonMode, _scrollContentModes);

            Button modeButton = modeButtonGO.GetComponent<Button>();
            modeButton.onClick.AddListener(delegate
            {
                SelectMode(mode);

                LeaderboardController.Instance.LoadLeaderboard(mode.ModeId);
                _playButton.onClick.AddListener(delegate
                {
                    StartGame(mode);
                });
            });

            TMP_Text modeButText = modeButtonGO.GetComponentInChildren<TMP_Text>();
            modeButText.text = mode.ModeName;
        }
    }

    private void SelectMode(CompetitiveMode mode)
    {
        ModePanelGroupControl(true);

        _modeTitleText.text = mode.ModeName;
        _modeTimeText.text = mode.TotalTime.ToString("0");
        _modeHeightText.text = mode.TableHeight.ToString("0");
        _modeWidthText.text = mode.TableWidth.ToString("0");
        _modeNodesText.text = mode.NodesPerBoard.ToString("0");
    }

    public void ModePanelGroupControl(bool show)
    {
        _modePanelGroup.alpha = show ? 1 : 0;
        _modePanelGroup.interactable = show;
        _modePanelGroup.blocksRaycasts = show;
    }

    public void StartGame(CompetitiveMode mode)
    {
        PlayerData.Instance.SelectedMode = mode;
        SceneManager.LoadScene(1);
    }

    public void ResetPlayButton() { _playButton.onClick.RemoveAllListeners(); }
}
