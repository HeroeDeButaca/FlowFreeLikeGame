using System.IO;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Threading.Tasks;

public class PlayerData : MonoBehaviour
{
    private string saveDataFolder = "";
    //private readonly string DATA_FILE_NAME = "PlayerData.dat";
    private readonly string DATA_FILE_NAME = "PlayerData.save";

    private Data _data;
    public Data UserData => _data;

    [SerializeField]
    private CanvasGroup _userCreationGroup;

    public static PlayerData Instance;

    void Awake()
    {
        if(Instance != this && Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        saveDataFolder = Application.persistentDataPath + "/Saves/";
    }

    void Start()
    {
        string path = saveDataFolder + DATA_FILE_NAME;

        if (File.Exists(path))
        {
            // Cargar datos
            _data = JsonCreator.LoadData<Data>(path);
            IconManager.Instance.CheckListBoolData();
            IconManager.Instance.InitializeIcons();

            Sprite iconSprite = IconManager.Instance.GetIconSprite(_data.IconId);

            PlayerInfoManager.Instance.ChangeIcon(iconSprite);
            PlayerInfoManager.Instance.ChangeName(_data.PlayerName);
        }
        else
        {
            UserCreationControl(true);
        }
    }

    public void ReadUsernameInput(TMP_InputField usernameInput)
    {
        _data = new Data(usernameInput.text);

        usernameInput.interactable = false;
        usernameInput.text = "Loading...";

        StartCoroutine(CreateDataCo());
    }

    private IEnumerator CreateDataCo()
    {
        Sprite iconSprite = IconManager.Instance.GetIconSprite(_data.IconId);

        PlayerInfoManager.Instance.ChangeIcon(iconSprite);
        PlayerInfoManager.Instance.ChangeName(_data.PlayerName);

        IconManager.Instance.CheckListBoolData();

        bool saveCompleted = false;

        Task.Run(() =>
        {
            JsonCreator.SaveData(_data, saveDataFolder, DATA_FILE_NAME);
            saveCompleted = true;
        });

        while (!saveCompleted)
        {
            yield return null;
        }

        UserCreationControl(false);
        IconManager.Instance.InitializeIcons();
    }

    private void UserCreationControl(bool show)
    {
        if (_userCreationGroup == null)
            return;

        _userCreationGroup.alpha = show ? 1 : 0;
        _userCreationGroup.interactable = show;
        _userCreationGroup.blocksRaycasts = show;
    }

    public void SavePlayerData()
    {
        StartCoroutine(SavePlayerDataCo());
    }

    private IEnumerator SavePlayerDataCo()
    {
        bool saveCompleted = false;

        Task.Run(() =>
        {
            JsonCreator.SaveData(_data, saveDataFolder, DATA_FILE_NAME);
            saveCompleted = true;
        });

        while (!saveCompleted)
        {
            yield return null;
        }
    }
}
