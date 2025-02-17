using System;
using Photon.Pun;
using System.Linq;
using UnityEngine;

public class PhotonPlayer : MonoBehaviour
{
    private PhotonView PV;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        PV = GetComponent<PhotonView>();

        if (PV.IsMine)
        {
            //for (int i = 0; i < GameSetup.Instance.players.Count; i++)
            //{
            //    if (GameSetup.Instance.players[i].seatAvailable)
            //    {
            //        PhotonNetwork.NickName = "Player " + (i + 1);
            //        GameSetup.Instance.SetPlayer(GameManager.Instance.room.username, PhotonNetwork.NickName, GameManager.Instance.room.userID, PV.ViewID);
            //        break;
            //    }
            //}

            foreach (NetworkPlayer networkPlayer in GameSetup.Instance.players.Where(np => np.seatAvailable))
            {
                Debug.Log("Player " + (GameSetup.Instance.players.IndexOf(networkPlayer) + 1) + " Joined");
                PhotonNetwork.NickName = "Player " + (GameSetup.Instance.players.IndexOf(networkPlayer) + 1);
                GameSetup.Instance.SetPlayer(GameManager.Instance.room.username, PhotonNetwork.NickName, GameManager.Instance.room.userID, PV.ViewID);
                break;
            }
        }
    }
}
