using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[CreateAssetMenu(menuName = ("IconData/Create Icon Data"))]
public class IconData : ScriptableObject
{
    public int IconId;
    public Sprite IconSprite;

    public bool IsLocked = true;
    public LocalizedString RequerimentLocale;
    public string RequerimentToUnlock { private set; get; }

    public bool IsSecret = false;

    private bool _initialized = false;

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        RequerimentLocale.StringChanged -= UpdateName;

        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;

        _initialized = false;
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        RequerimentLocale.StringChanged += UpdateName;
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        RequerimentLocale.RefreshString();
    }

    private void OnLocaleChanged(Locale locale)
    {
        RequerimentLocale.RefreshString();
    }

    private void UpdateName(string value)
    {
        RequerimentToUnlock = value;
    }
}
