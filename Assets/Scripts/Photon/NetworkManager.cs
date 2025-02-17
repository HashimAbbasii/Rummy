using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private bool tryToConnentToPhotonServer;
    public bool connectedToPhotonServer;
    public bool joinedLobby;
    public bool joinedRoom;

    [HideInInspector] public string userName;
    [HideInInspector] public string password;
    
    private static NetworkManager _instance;
    public static NetworkManager Instance
    {
        get => _instance;
        private set => _instance = value;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (!tryToConnentToPhotonServer)
        {
            PhotonNetwork.ConnectUsingSettings();
            tryToConnentToPhotonServer = true;
            Invoke(nameof(CheckIsUserIsConnectedToMasterServer), 5f);
        }
    }

    #region Server Connection

    private void CheckIsUserIsConnectedToMasterServer()
    {
        if (!PhotonNetwork.IsConnected)
        {
            connectedToPhotonServer = false;
        }

        if (!connectedToPhotonServer)
        {
            tryToConnentToPhotonServer = false;
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon Log: Connected to " + PhotonNetwork.CloudRegion + " server");
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.JoinLobby();
        connectedToPhotonServer = true;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log("Photon Log: Disconnected from " + PhotonNetwork.CloudRegion + " server");
        connectedToPhotonServer = false;
        tryToConnentToPhotonServer = false;
    }

    #endregion

    
    #region Lobby

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("Photon Log: Joined Lobby");
        joinedLobby = true;
    }

    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        Debug.Log("Photon Log: Left Lobby");
        joinedLobby = false;
    }

    #endregion


    //create a private room
    public void CreatePrivateRoom(string _roomKey, int _roomSize)
    {
        Debug.Log("Photon Log: Room Key: " + _roomKey);
        GameManager.Instance.roomType = RoomType.Private;
        RoomOptions roomOptions = new() { IsVisible = false, IsOpen = true, MaxPlayers = (byte)_roomSize};
        PhotonNetwork.CreateRoom(_roomKey, roomOptions, TypedLobby.Default);
    }

    public void JoinPrivateRoom(string _roomKey)
    {
        GameManager.Instance.roomType = RoomType.Private;
        PhotonNetwork.JoinRoom(_roomKey);
    }

    public void CreateOrJoinPublicRoom(string _roomKey, int _roomSize)
    {
        Debug.Log("Photon Log: Room Key: " + _roomKey);
        GameManager.Instance.roomType = RoomType.Public;
        RoomOptions roomOptions = new() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)_roomSize };
        PhotonNetwork.JoinOrCreateRoom(_roomKey, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Photon Log: Room Joined. Number of player(s) in room: " + PhotonNetwork.PlayerList.Length);
        joinedRoom = true;

        if (FindObjectOfType<CreatePrivateRoomHandler>())
            FindObjectOfType<CreatePrivateRoomHandler>().requestSent = false;

        if (FindObjectOfType<JoinPrivateRoomHandler>())
            FindObjectOfType<JoinPrivateRoomHandler>().requestSent = false;

        if (FindObjectOfType<CreateOrJoinPublicRoom>())
            FindObjectOfType<CreateOrJoinPublicRoom>().requestSent = false;

        StartCoroutine(GameModeLoading());

    }

    public IEnumerator GameModeLoading()
    {
        switch (GameManager.Instance.gameMode)
        {
            case GameMode.Rami_31:
                UIManager.Instance.DisplaySpecificScreen(GameScreens.OnlineGamePlayScreen);
                CreatePlayer();
                break;
            
            case GameMode.Rami_Annette:
                var a = SceneManager.LoadSceneAsync(UIManager.Instance.ramiAnnetteScene);
                while (!a.isDone) yield return null;
                yield return new WaitForSeconds(1f);
                Debug.Log("Scene Loaded");
                CreatePlayer();
                break;
            
            case GameMode.Rami_51:
                break;
        }
        yield return null;
    }
    
    public IEnumerator LeaveRoom()
    {
        var a = SceneManager.LoadSceneAsync(UIManager.Instance.rami31Scene);
        while (!a.isDone) yield return null;
        UIManager.Instance.DisplaySpecificScreen(GameScreens.MainScreen);
    }
    
    public override void OnLeftRoom()
    {
        Debug.Log("Photon Log: Player left the room");
        joinedRoom = false;
        
        if(PhotonNetwork.InRoom)
            GameSetup.Instance.UpdateNetworkPlayersListOnRoomLeft();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Photon Log: Tried to create a room but failed. There must be a room of same name.");
        FindObjectOfType<CreatePrivateRoomHandler>()?.DisplayMessage(message);
        FindObjectOfType<CreateOrJoinPublicRoom>()?.DisplayMessage(message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);

        JoinPrivateRoomHandler joinPrivateRoomHandler = FindObjectOfType<JoinPrivateRoomHandler>();
        CreateOrJoinPublicRoom createOrJoinPublicRoom = FindObjectOfType<CreateOrJoinPublicRoom>();

        if (joinPrivateRoomHandler)
        {
            joinPrivateRoomHandler.DisplayMessage(message);
            joinPrivateRoomHandler.requestSent = false;
        }

        if (createOrJoinPublicRoom)
        {
            createOrJoinPublicRoom.DisplayMessage(message);
            createOrJoinPublicRoom.requestSent = false;
        }
    }

    private void CreatePlayer()
    {
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), transform.position, Quaternion.identity);
    }
}
