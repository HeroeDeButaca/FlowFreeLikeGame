using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GameController : MonoBehaviour
{
    private enum GameState { LoadingMap, InGame, Finished }
    private GameState _gameState = GameState.LoadingMap;

    private float _currentTime;
    private float _maxTime;
    [SerializeField]
    private TMP_Text _timeText;

    private int _nodes;

    private int _completedBoards;
    [SerializeField]
    private TMP_Text _completedBoardsText;

    public UnityEvent OnReset;

    public static GameController Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        NodesGenerator.Instance.OnBoardReady.AddListener(delegate
        {
            _gameState = GameState.InGame;
        });

        TouchDetector.Instance.OnTableFilled.AddListener(TableFilled);

        SetGame();
    }

    void Update()
    {
        if (_gameState == GameState.InGame)
        {
            _currentTime -= Time.deltaTime;
            _timeText.text = _currentTime.ToString("0");

            if (_currentTime <= 0f)
            {
                TouchDetector.Instance.CanTouch = false;
                _gameState = GameState.Finished;
                _timeText.text = "0";
                TimesOutManager.Instance.ShowPanel(_completedBoards);
            }

        }
    }

    private void SetGame()
    {
        CompetitiveMode mode = PlayerData.Instance.SelectedMode;
        int width = mode.TableWidth;
        int height = mode.TableHeight;
        _nodes = mode.NodesPerBoard;

        _maxTime = mode.TotalTime;
        _currentTime = _maxTime;

        BoardGenerator.Instance.GenerateBoard(width, height, _nodes);
    }

    private void TableFilled()
    {
        _gameState = GameState.LoadingMap;
        OnReset?.Invoke();
        _completedBoards++;
        _completedBoardsText.text = _completedBoards.ToString("0");

        NodesGenerator.Instance.CreateCorrectBoard(_nodes);
    }
}
