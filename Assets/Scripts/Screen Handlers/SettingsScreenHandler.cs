using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScreenHandler : MonoBehaviour
{
    public Toggle languageToggle;
    public TextMeshProUGUI toggleText;

    private void OnEnable()
    {
        if (PreferenceManager.Language == "English")
        {
            languageToggle.isOn = false;
        }
        else if (PreferenceManager.Language == "Hebrew")
        {
            languageToggle.isOn = true;
        }
    }

    public void OnToggleValueChange()
    {
        if (languageToggle.isOn)
        {
            PreferenceManager.Language = "Hebrew";
            //toggleText.text = "English";
        }
        else
        {
            PreferenceManager.Language = "English";
            //toggleText.text = "Hebrew";
        }

        GameManager.Instance.languageSwitch.Invoke();
    }

    public void OnShopButtonClick()
    {
        Application.OpenURL("https://wa.me/972522052529");
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void OnLogoutButtonClick()
    {
        PreferenceManager.UserID = null;
        PreferenceManager.Password = null;
        NetworkManager.Instance.userName = null;
        NetworkManager.Instance.password = null;
        UIManager.Instance.DisplaySpecificScreen(GameScreens.LoginScreen);
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void OnCloseButtonClick()
    {
        UIManager.Instance.HideScreen(GameScreens.SettingsScreen);
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }
}
