using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using System.Linq;

public class ConfigData
{
    public bool IsFullscreen;
    public float MusicVolume;
    public float SfxVolume;
    public int LanguageSelected;

    public ConfigData()
    {
        IsFullscreen = true;
        MusicVolume = 1f;
        SfxVolume = 1f;
        LanguageSelected = 0;
    }
}

public class OptionsManager : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup _optionsPanel;

    [SerializeField]
    private Toggle _fullscreenToggle;
    [SerializeField]
    private Slider _musicSlider;
    [SerializeField]
    private Slider _sfxSlider;
    [SerializeField]
    private TMP_Dropdown _languageDropdown;

    private ConfigData _configData;
    private string _saveDataFolder = "";
    private readonly string CONFIG_FILE_NAME = "config.json";

    void Start()
    {
        _saveDataFolder = Application.persistentDataPath + "/";
        string path = _saveDataFolder + CONFIG_FILE_NAME;

        if(File.Exists(path))
        {
            _configData = JsonCreator.LoadData<ConfigData>(path);
        }
        else
        {
            _configData = new ConfigData();
            JsonCreator.SaveData(_configData, _saveDataFolder, CONFIG_FILE_NAME);
        }

        _optionsPanel.SetVisible(false);
        InitializeConfig();
    }

    private void InitializeConfig()
    {
        _fullscreenToggle.onValueChanged.AddListener(ChangeFullscreen);
        _musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        _sfxSlider.onValueChanged.AddListener(ChangeSfxVolume);
        _languageDropdown.onValueChanged.AddListener(ChangeLanguage);

        ConfigureLanguageDropdown();

        bool isFullscreen = _configData.IsFullscreen;
        float musicVolume = _configData.MusicVolume;
        float sfxVolume = _configData.SfxVolume;
        int languageSelected = _configData.LanguageSelected;

        _fullscreenToggle.isOn = isFullscreen;
        _musicSlider.value = musicVolume;
        _sfxSlider.value = sfxVolume;
        _languageDropdown.value = languageSelected;

        ChangeFullscreen(isFullscreen);
        ChangeMusicVolume(musicVolume);
        ChangeSfxVolume(sfxVolume);
        ChangeLanguage(languageSelected);
    }

    #region Change option values
    private void ChangeFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        _configData.IsFullscreen = fullscreen;
        JsonCreator.SaveData(_configData, _saveDataFolder, CONFIG_FILE_NAME);
    }

    private void ChangeMusicVolume(float volume)
    {

        _configData.MusicVolume = volume;
        JsonCreator.SaveData(_configData, _saveDataFolder, CONFIG_FILE_NAME);
    }

    private void ChangeSfxVolume(float volume)
    {

        _configData.SfxVolume = volume;
        JsonCreator.SaveData(_configData, _saveDataFolder, CONFIG_FILE_NAME);
    }

    private void ChangeLanguage(int value)
    {
        _configData.LanguageSelected = value;
        StartCoroutine(ChangeLanguageCo(value));
    }

    private IEnumerator ChangeLanguageCo(int localeId)
    {
        _languageDropdown.interactable = false;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeId];

        _languageDropdown.interactable = true;
        JsonCreator.SaveData(_configData, _saveDataFolder, CONFIG_FILE_NAME);
    }
    #endregion

    private void ConfigureLanguageDropdown()
    {
        // Averiguamos los nombres de los locales que tiene el proyecto
        Locale[] locales = LocalizationSettings.AvailableLocales.Locales.ToArray();
        string[] languagesNames = new string[locales.Length];

        Debug.Log($"Total locales: {locales.Length}");

        for (int i = 0; i < locales.Length; i++)
        {
            languagesNames[i] = locales[i].LocaleName;
        }

        // Seteamos el nombre de las opciones en el dropdown
        _languageDropdown.AddOptions(languagesNames.ToList());
    }
}
