using Newtonsoft.Json;
using RTLTMPro;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginScreenHandler : MonoBehaviour
{
    [Header("Loading Panel")]
    public GameObject loadingPanel;

    [Header("Login Panel")]
    public GameObject loginPanel;
    public TMP_InputField userIDInputField;
    public TMP_InputField passwordInputField;
    public Toggle rememberMeToggle;
    public RTLTextMeshPro messageText;

    [HideInInspector] public bool requestSent = false;
    public GameObject loader;
    public GameObject buttonText;

    private void OnEnable()
    {
        if (!string.IsNullOrEmpty(NetworkManager.Instance?.userName))
        {
            userIDInputField.text = NetworkManager.Instance.userName;
            passwordInputField.text = NetworkManager.Instance.password;
            loadingPanel.SetActive(true);
            loginPanel.SetActive(false);
            requestSent = true;
            DatabaseManager.Instance.Login(new Login() { phone = NetworkManager.Instance.userName, apppass = NetworkManager.Instance.password }, OnLoginComplete);
        }
        
        //PlayerPrefs.DeleteAll();
        else if (string.IsNullOrEmpty(PreferenceManager.Password))
        {
            loadingPanel.SetActive(false);
            loginPanel.SetActive(true);
            requestSent = false;
            userIDInputField.text = null;
            passwordInputField.text = null;
            ClearMessage();
        }
        else
        {
            userIDInputField.text = PreferenceManager.UserID;
            loadingPanel.SetActive(true);
            loginPanel.SetActive(false);
            requestSent = true;
            DatabaseManager.Instance.Login(new Login() { phone = PreferenceManager.UserID, apppass = PreferenceManager.Password }, OnLoginComplete);
        }
    }

    public void OnLoginButtonClick()
    {
        if (string.IsNullOrEmpty(userIDInputField.text))
        {
            DisplayMessage("Enter user ID");
        }
        else if (string.IsNullOrEmpty(passwordInputField.text))
        {
            DisplayMessage("Enter password");
        }
        else if (!requestSent)
        {
            NetworkManager.Instance.userName = userIDInputField.text;
            NetworkManager.Instance.password = passwordInputField.text;
            
            requestSent = true;
            loader.SetActive(true);
            buttonText.SetActive(false);
            DatabaseManager.Instance.Login(new Login() { phone = userIDInputField.text, apppass = passwordInputField.text }, OnLoginComplete);
        }

        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    private void OnLoginComplete(Login loginData, UnityWebRequest request)
    {
        loader.SetActive(false);
        buttonText.SetActive(true);

        if (request.result == UnityWebRequest.Result.Success)
        {
            // if (string.IsNullOrEmpty(PreferenceManager.UserID))
            // {
            //     PreferenceManager.UserID = userIDInputField.text;
            // }
            
            PreferenceManager.UserID = userIDInputField.text;

            if (rememberMeToggle.isOn)
            {
                PreferenceManager.Password = passwordInputField.text;
            }

            UIManager.Instance.DisplaySpecificScreen(GameScreens.GameModeSelectionScreen);
        }
        else
        {
            requestSent = false;
            Dictionary<string, string> response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
            DisplayMessage(response["message"]);
        }
    }

    public void DisplayMessage(string message)
    {
        messageText.text = GameManager.Instance.Translate(message);
        Invoke(nameof(ClearMessage), 5f);
    }

    public void ClearMessage()
    {
        messageText.text = null;
    }
}
