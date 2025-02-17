using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class Screens
{
    public GameScreens ScreenName;
    public GameScreen GameScreen;

    
}

public class UIManager : MonoBehaviour
{
    [Header("DATA PERSISTENCE")]
    public DataManagerPersistancy dataManagerPersistancy;
    public Texture2D cursorTexture;

    public Screens[] UIScreens;
    [SerializedDictionary("Game Screen Enum", "Game Screen Object")]
    public SerializedDictionary<GameScreens, GameScreen> UIScreensReferences = new SerializedDictionary<GameScreens, GameScreen>();

    public SceneField rami31Scene;
    public SceneField ramiAnnetteScene;
    public SceneField rami51Scene;

    public OnlineGamePlayScreenHandler onlineGamePlayScreenHandler;
    public RamiAnnetteGameplayScreenHandler ramiAnnetteGameplayScreenHandler;
    
    private static UIManager _instance;
    public static UIManager Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        // DontDestroyOnLoad(gameObject);

        Screen.SetResolution(1920, 1080, true);

    }
   
    private void Start()
    {
        AddAllScreens();

        for (var i = 0; i < GameSetup.Instance.players.Count; i++)
        {
            var player = GameSetup.Instance.players[i];
            
            // switch (GameManager.Instance.gameMode)
            // {
            //     case GameMode.Rami_31:
            //         player.playerIcon = onlineGamePlayScreenHandler.playerIconsHandler.players[i];
            //         break;
            //     case GameMode.Rami_Annette:
            //         player.playerIcon = ramiAnnetteGameplayScreenHandler.playerIconsHandler.players[i];
            //         break;
            // }
            if (ramiAnnetteGameplayScreenHandler)
            {
                player.playerIcon = ramiAnnetteGameplayScreenHandler.playerIconsHandler.players[i];
            }
            else if (onlineGamePlayScreenHandler)
            {
                player.playerIcon = onlineGamePlayScreenHandler.playerIconsHandler.players[i];
            }
        }

        if (ramiAnnetteGameplayScreenHandler)
        {
            ramiAnnetteGameplayScreenHandler.gameObject.SetActive(true);
        }
    }


    public void CheckForOutsideClick()
    {
        //if (Input.GetMouseButtonDown(0)) // Detect left mouse click or touch
        //{
            Debug.Log("Checking for outside click...");

            // Check if the click is outside the keyboard panel
            if (!IsClickOnKeyboardPanel() && dataManagerPersistancy.tnVirtualKeyboard.vkCanvas.activeSelf)
            {
                Debug.Log("Click detected outside the keyboard panel. Hiding the virtual keyboard.");
                dataManagerPersistancy.tnVirtualKeyboard.HideVirtualKeyboard();
            }
            else
            {
                Debug.Log("Click detected on the keyboard panel. Keeping the virtual keyboard visible.");
            }
        //}
    }


    private bool IsClickOnKeyboardPanel()
    {
        // Ensure the EventSystem is present
        if (EventSystem.current == null)
        {
            Debug.LogWarning("EventSystem is not present in the scene.");
            return false;
        }

        // Create a PointerEventData object to raycast UI elements
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        // Raycast all UI elements under the pointer
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        // Check if any of the raycasted elements are part of the keyboard panel
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("KeyboardPanel")) // Assuming your keyboard panel has this tag
            {
                Debug.Log($"Click detected on keyboard panel: {result.gameObject.name}");
                return true;
            }
        }

        // No keyboard panel detected under the pointer
        return false;
    }





    private void AddAllScreens()
    {
        foreach (var item in UIScreens)
        {
            item.GameScreen.MyName = item.ScreenName;
            UIScreensReferences.Add(item.ScreenName, item.GameScreen);
        }

        if (onlineGamePlayScreenHandler)
        {
            DisplaySpecificScreen(GameScreens.LoginScreen);
        }
    }

    public void DisplayScreen(GameScreens screenName)
    {
        UIScreensReferences[screenName].gameObject.SetActive(true);
    }

    public void DisplaySpecificScreen(GameScreens screenName)
    {
        foreach (var item in UIScreens)
        {
            UIScreensReferences[item.ScreenName].gameObject.SetActive(false);
        }
        UIScreensReferences[screenName].gameObject.SetActive(true);
    }

    public void HideScreen(GameScreens screenName)
    {
        UIScreensReferences[screenName].gameObject.SetActive(false);
    }

    public void HideAllScreens()
    {
        foreach (var item in UIScreens)
        {
            item.GameScreen.gameObject.SetActive(false);
        }
    }

    public void ShowAllScreens()
    {
        foreach (var item in UIScreens)
        {
            item.GameScreen.gameObject.SetActive(true);
        }
    }

    public void MouseEnter()
    {
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.ForceSoftware);
    }

    public void MouseExit()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
    }
}







public enum GameScreens
{
    LoginScreen,
    MainScreen,
    SettingsScreen,
    PublicRoomsScreen,
    OnlineGamePlayScreen,
    GameModeSelectionScreen,
    RamiAnnetteScreen
}

