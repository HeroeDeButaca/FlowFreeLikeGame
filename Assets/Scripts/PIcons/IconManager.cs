using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using TMPro;

public class IconManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("All the icon data should be here")]
    private IconData[] _iconsData;

    [SerializeField]
    private GameObject _prefabIcon;

    [SerializeField]
    private Transform _contentIcons;

    [SerializeField]
    private TMP_Text _totalIconsUnlockedText;

    private int _totalIconsUnlocked = 1;

    void Start()
    {
        InitializeIcons();
    }

    private void InitializeIcons()
    {
        for(int i = 0; i < _iconsData.Length; i++)
        {
            IconData iconData = _iconsData[i];
            GameObject iconGO = Instantiate(_prefabIcon, _contentIcons);

            iconGO.transform.GetChild(0).GetComponent<Image>().sprite = iconData.IconSprite;
            iconGO.transform.GetChild(1).gameObject.SetActive(iconData.IsLocked);
            iconGO.transform.GetChild(2).gameObject.SetActive(iconData.IsSecret);
        }

        _totalIconsUnlockedText.text = $"{_totalIconsUnlocked} / {_iconsData.Length}";
    }
}
