using System;
using System.Collections;
using UnityEngine;

public class DeveloperManager : MonoBehaviour
{
    
    #region Developer Property

    [SerializeField] private bool developer;

    public bool Developer
    {
        get => developer;
        set
        {
            developer = value;

            developerLoginUI.SetActive(value);
        }
    }
    
    #endregion
    
    public GameObject developerLoginUI;

    public LoginScreenHandler loginScreen;

    private void Update()
    {
        Developer = loginScreen.gameObject.activeSelf && developer;
    }

    private void OnValidate()
    {
        Developer = developer;
    }

    private IEnumerator Start()
    {
        developerLoginUI.SetActive(false);

        yield return new WaitForSeconds(0.1f);
        
        if (Developer)
        {
            developerLoginUI.SetActive(true);
        }
        
// #if !UNITY_EDITOR
//         Developer = false;
// #endif
    }

    public void FillInfo(int userNumber)
    {
        switch (userNumber)
        {
            case 0:
                loginScreen.userIDInputField.text = "923165446561";
                loginScreen.passwordInputField.text = "1111";
                break;
            case 1:
                loginScreen.userIDInputField.text = "921234567";
                loginScreen.passwordInputField.text = "1111";
                break;
            case 2:
                loginScreen.userIDInputField.text = "921111111";
                loginScreen.passwordInputField.text = "1111";
                break;
            case 3:
                loginScreen.userIDInputField.text = "922222222";
                loginScreen.passwordInputField.text = "1111";
                break;
        }
    }
    
}
