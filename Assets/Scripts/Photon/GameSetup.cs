using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NetworkPlayer
{
    public string playerName;
    public string playerNickName;
    public string playerUserId;
    public PhotonView player;
    public PlayerIcon playerIcon;
    public bool seatAvailable;
    public bool playAgain;
    public bool readyToPlay;
    public bool allSequencesHaveThreeOrMoreTiles;
    public RamiAnnetteTurn ramiAnnetteTurn;
    public Tile lastTileInteractedWith;
}

[System.Serializable]
public class RamiAnnetteTurn
{
    public bool tookATile;
    public bool placedATile;

    public bool TurnTaken()
    {
        return tookATile && placedATile;
    }
    
    public void ResetTurn()
    {
        tookATile = false;
        placedATile = false;
    }
}

//Setup of Game for Player After It Has Joined

public class GameSetup : MonoBehaviour
{
    public MultiplayerRelatedGameSetup multiplayerRelatedGameSetup;
    
    // private PhotonView PV;

    // public OnlineGamePlayScreenHandler onlineGamePlayScreenHandler;
    // public RamiAnnetteGameplayScreenHandler ramiAnnetteGameplayScreenHandler;

    public PhotonView myNetworkPlayer;
    public List<NetworkPlayer> players;

    private static GameSetup _instance;

    public static GameSetup Instance
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
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        // PV = GetComponent<PhotonView>();

        foreach (NetworkPlayer networkPlayer in players)
            networkPlayer.seatAvailable = true;
        
        if (/*GameManager.Instance.gameMode == GameMode.Rami_Annette && */ UIManager.Instance.ramiAnnetteGameplayScreenHandler)
        {
            UIManager.Instance.ramiAnnetteGameplayScreenHandler.gameObject.SetActive(true);
        }
    }

    public void SetPlayer(string name, string nickName, string userID, int viewID)
    {
        multiplayerRelatedGameSetup.MpSetPlayer(name, nickName, userID, viewID);
        // PV.RPC(nameof(RPC_SetPlayer), RpcTarget.AllBuffered, name, nickName, userID, viewID);
    }
    
    // [PunRPC]
    // private void RPC_SetPlayer(string _name, string _nickName, string _userID, int _viewID)
    // {
    //     PhotonView photonView = PhotonView.Find(_viewID);
    //
    //     if (photonView.IsMine)
    //     {
    //         myNetworkPlayer = photonView;
    //     }
    //
    //     int playerNumber = int.Parse(_nickName[^1].ToString());
    //
    //     players[playerNumber - 1].player = photonView;
    //     players[playerNumber - 1].seatAvailable = false;
    //     players[playerNumber - 1].playerName = _name;
    //     players[playerNumber - 1].playerNickName = _nickName;
    //     players[playerNumber - 1].playerUserId = _userID;
    //     players[playerNumber - 1].readyToPlay = true;
    //
    //     // ReSharper disable twice Unity.NoNullPropagation
    //     switch (GameManager.Instance.gameMode)
    //     {
    //         case GameMode.Rami_31:
    //             UIManager.Instance.onlineGamePlayScreenHandler?.SetPlayerIcons();
    //             break;
    //         case GameMode.Rami_Annette:
    //             UIManager.Instance.ramiAnnetteGameplayScreenHandler?.SetPlayerIcons();
    //             break;
    //     }
    // }

    public void UpdateNetworkPlayersListOnRoomLeft()
    {
        if (PhotonNetwork.CurrentRoom.IsOpen)
        {
            foreach (NetworkPlayer networkPlayer in players)
            {
                if (!networkPlayer.seatAvailable && !networkPlayer.player)
                {
                    networkPlayer.seatAvailable = true;
                    networkPlayer.playerName = null;
                    networkPlayer.playerNickName = null;
                }
            }
        }
    }

    public void PlayAgain()
    {
        Debug.Log("PLAYYYY");
        
        List<NetworkPlayer> newPlayersList = new();
        foreach (NetworkPlayer networkPlayer in players)
        {
            if (networkPlayer.playAgain)
            {
                newPlayersList.Add(new NetworkPlayer
                {
                    playerName = networkPlayer.playerName,
                    playerNickName = networkPlayer.playerNickName,
                    playerUserId = networkPlayer.playerUserId,
                    player = networkPlayer.player,
                    seatAvailable = networkPlayer.seatAvailable,
                    allSequencesHaveThreeOrMoreTiles = false,
                    playAgain = false,
                });

                if (newPlayersList[^1].player.IsMine)
                {
                    PhotonNetwork.NickName = "Player " + newPlayersList.Count;
                }
            }
            else
            {
                Destroy(networkPlayer.player != null ? networkPlayer.player.gameObject : null);
            }
        }

        foreach (NetworkPlayer networkPlayer in newPlayersList)
        {
            players[newPlayersList.IndexOf(networkPlayer)].playerName = networkPlayer.playerName;
            players[newPlayersList.IndexOf(networkPlayer)].playerUserId = networkPlayer.playerUserId;
            players[newPlayersList.IndexOf(networkPlayer)].player = networkPlayer.player;
            players[newPlayersList.IndexOf(networkPlayer)].seatAvailable = networkPlayer.seatAvailable;
            players[newPlayersList.IndexOf(networkPlayer)].playAgain = networkPlayer.playAgain;
            players[newPlayersList.IndexOf(networkPlayer)].allSequencesHaveThreeOrMoreTiles = false;
            players[newPlayersList.IndexOf(networkPlayer)].playerIcon.tilesCount = 0;
            players[newPlayersList.IndexOf(networkPlayer)].playerIcon.timerImage.fillAmount = 0;
        }

        for (int i = newPlayersList.Count; i < players.Count; i++)
        {
            players[i].playerName = null;
            players[i].playerNickName = null;
            players[i].playerUserId = null;
            players[i].player = null;
            players[i].seatAvailable = true;
            players[i].allSequencesHaveThreeOrMoreTiles = false;
            players[i].playerIcon.gameObject.SetActive(false);
        }

        
        switch (GameManager.Instance.gameMode)
        {
            case GameMode.Rami_31:
                UIManager.Instance.UIScreensReferences[GameScreens.OnlineGamePlayScreen].GetComponent<OnlineGamePlayScreenHandler>().SetPlayerIcons();
                break;
                
            case GameMode.Rami_Annette:
                UIManager.Instance.UIScreensReferences[GameScreens.RamiAnnetteScreen].GetComponent<RamiAnnetteGameplayScreenHandler>().SetPlayerIcons();
                break;
            
            case GameMode.Rami_51:
                break;
        }
        
    }

    public void ResetGameSetup()
    {
        foreach (NetworkPlayer networkPlayer in players)
        {
            networkPlayer.playerName = null;
            networkPlayer.playerNickName = null;
            networkPlayer.playerUserId = null;
            networkPlayer.seatAvailable = true;
            networkPlayer.allSequencesHaveThreeOrMoreTiles = false;
            networkPlayer.playerIcon.timerImage.fillAmount = 0;
        }
    }
}
