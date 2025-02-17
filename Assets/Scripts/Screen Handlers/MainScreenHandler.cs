using Newtonsoft.Json;
using RTLTMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MainScreenHandler : MonoBehaviour
{
    public RTLTextMeshPro pointsText;
    public GameObject mobilePanel;
    public GameObject webPanel;
    public GameObject createPrivateRoomPanel;
    public GameObject joinPrivateRoomPanel;
    public GameObject joinPublicRoomPanel;
    public RTLTextMeshPro modeSelectionText;

    

    private void OnEnable()
    {
        createPrivateRoomPanel.SetActive(false);
        joinPrivateRoomPanel.SetActive(false);
        joinPublicRoomPanel.SetActive(false);

#if UNITY_ANDROID || UNITY_IOS
        mobilePanel.SetActive(true);
        webPanel.SetActive(false);
#else
        mobilePanel.SetActive(false);
        webPanel.SetActive(true);
#endif

        DatabaseManager.Instance.GetUserPoints(PreferenceManager.UserID, OnGetUserPointsComplete);
        
        GameManager.Instance.onGameModeSelected += OnGameModeSelected;
        GameManager.Instance.languageSwitch.AddListener(OnTranslate);
    }

    private void Start()
    {
        pointsText.text = null;
    }

    private void OnGetUserPointsComplete(string userPoints, UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            PreferenceManager.Points = JsonConvert.DeserializeObject<Response>(request.downloadHandler.text).data.points;
            pointsText.text = PreferenceManager.Points.ToString();
        }
        else
        {
            DatabaseManager.Instance.GetUserPoints(PreferenceManager.UserID, OnGetUserPointsComplete);
        }
    }

    public void JoinPublicRoom(string roomCode)
    {
        joinPublicRoomPanel.SetActive(true);
        joinPublicRoomPanel.GetComponent<CreateOrJoinPublicRoom>().roomKeyInputField.readOnly = true;
        joinPublicRoomPanel.GetComponent<CreateOrJoinPublicRoom>().roomKeyInputField.text = roomCode;
    }

    public void OnGameModeClick()
    {
        UIManager.Instance.DisplaySpecificScreen(GameScreens.GameModeSelectionScreen);
    }

    private void OnGameModeSelected()
    {
        modeSelectionText.text = GameManager.Instance.gameMode.ToString();
        var text = modeSelectionText.text.Replace("_", " ");
        modeSelectionText.text = GameManager.Instance.Translate(text);
    }

    private void OnTranslate()
    {
        var text = GameManager.Instance.gameMode.ToString().Replace("_", " ");
        modeSelectionText.text = GameManager.Instance.Translate(text);
    }
    
    public void OnShopButtonClick()
    {
        Application.OpenURL("https://wa.me/972522052529");
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void OnCreatePrivateRoomButtonClick()
    {
        createPrivateRoomPanel.SetActive(true);
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void OnJoinPrivateRoomButtonClick()
    {
        joinPrivateRoomPanel.SetActive(true);
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void OnJoinPublicRoomButtonClick()
    {
        joinPublicRoomPanel.SetActive(true);
        joinPublicRoomPanel.GetComponent<CreateOrJoinPublicRoom>().roomKeyInputField.readOnly = false;
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void OnPublicRoomsButtonClick()
    {
        UIManager.Instance.DisplaySpecificScreen(GameScreens.PublicRoomsScreen);
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void OnSettingsButtonClick()
    {
        UIManager.Instance.DisplayScreen(GameScreens.SettingsScreen);
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void LoadRamiAnnetteScene()
    {
        SceneManager.LoadScene(1);
    }
}
