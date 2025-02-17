using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Serialization;

public class MultiplayerRelatedGameSetup : MonoBehaviour
{
    public GameSetup gameSetup;
    public PhotonView pv;
    
    private void Start()
    {
        pv = GetComponent<PhotonView>();
        gameSetup = GameSetup.Instance;
        gameSetup.multiplayerRelatedGameSetup = this;
    
        // foreach (NetworkPlayer networkPlayer in gameSetup.players)
        //     networkPlayer.seatAvailable = true;
    }
    
    
    public void MpSetPlayer(string name, string nickName, string userID, int viewID)
    {
        pv.RPC(nameof(RPC_SetPlayer), RpcTarget.AllBuffered, name, nickName, userID, viewID);
    }
    
    [PunRPC]
    private void RPC_SetPlayer(string _name, string _nickName, string _userID, int _viewID)
    {
        PhotonView photonView = PhotonView.Find(_viewID);
    
        if (photonView.IsMine)
        {
            gameSetup.myNetworkPlayer = photonView;
        }
    
        int playerNumber = int.Parse(_nickName[^1].ToString());
    
        gameSetup.players[playerNumber - 1].player = photonView;
        gameSetup.players[playerNumber - 1].seatAvailable = false;
        gameSetup.players[playerNumber - 1].playerName = _name;
        gameSetup.players[playerNumber - 1].playerNickName = _nickName;
        gameSetup.players[playerNumber - 1].playerUserId = _userID;
        gameSetup.players[playerNumber - 1].readyToPlay = true;

        // ReSharper disable twice Unity.NoNullPropagation
        switch (GameManager.Instance.gameMode)
        {
            case GameMode.Rami_31:
                UIManager.Instance.onlineGamePlayScreenHandler?.SetPlayerIcons();
                break;
            case GameMode.Rami_Annette:
                UIManager.Instance.ramiAnnetteGameplayScreenHandler?.SetPlayerIcons();
                break;
        }
    }
    
}
