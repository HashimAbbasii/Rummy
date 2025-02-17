using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PhotonView))]
public class GamePlayScreenHandler : MonoBehaviour
{
    [HideInInspector] public PhotonView photonView;
    public PlayerIconsHandler playerIconsHandler;
    public List<Transform> playerIconPositions;
    public bool tilePicked;
    
    protected int deckCount = 106;
    public int numberOfPlayers;
    protected int currentTurn = -1;
    protected IEnumerator coroutine;
    protected bool timerRunning;
    protected int initialTime;
    protected int timer;
    public Color black, blue, red, yellow;
    public string playerTurn;
    public string winner;
    public GameObject winningPoints;
    public GameObject roomKey;
    public TextMeshProUGUI deckText;
    public Button runsSequenceButton, groupSequenceButton;
    public GameObject turnButtons, pickTileButton;
    public Button nextButton;
    public Tile tilePrefab;
    // public Transform cardAnchor;
    public Transform deckParent;
    public GameObject resultPanel;
    public List<Tile> tiles;
    public List<Tile> deck;
    public List<PlayerDeck> playersDeck;
    public string sequence = null;
    public List<Tile> totalTiles = new();
    public List<Tile> tempTiles = new();

    [Header("Leave Game Panel")]
    public GameObject leaveGamePanel;

    [Header("Chat Panel")]
    public GameObject chatPanel;
    public GameObject chatContent;
    public GameObject chatMessage;

    [Header("Message Box")]
    public GameObject messageBox;
    public RTLTextMeshPro messageText;
    [SerializeField] protected int amountOfTilesInHand;
    
    
    public virtual void OnEnable()
    {
        photonView = GetComponent<PhotonView>();
        
        foreach (NetworkPlayer networkPlayer in GameSetup.Instance.players)
        { 
            networkPlayer.playerIcon.gameIcon.SetActive(false);
            networkPlayer.playerIcon.waitingIcon.SetActive(false);
        }
    }
    
    public void SetPlayerIcons()
    {
        int count = 1;
        foreach (var networkPlayer in GameSetup.Instance.players.Where(networkPlayer => networkPlayer.player))
        {
            networkPlayer.playerIcon.gameIcon.SetActive(true);
            networkPlayer.playerIcon.playerNameText.text = networkPlayer.playerName;
            networkPlayer.playerIcon.playerNickName = networkPlayer.playerNickName;

            networkPlayer.playerIcon.transform.position = networkPlayer.player.ViewID == GameSetup.Instance.myNetworkPlayer?.ViewID ? playerIconPositions[0].position : playerIconPositions[count++].position;
        }

        for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
        {
            // Debug.Log("Log: " + i);
            GameSetup.Instance.players[i].playerIcon.waitingIcon.SetActive(GameSetup.Instance.players[i].seatAvailable);
        }
        
        // foreach (var networkPlayer in GameSetup.Instance.players.Where(networkPlayer => networkPlayer.player))
        // {
        //     networkPlayer.readyToPlay = true;
        // }
    }

    public int PlayerNumber(string playerName)
    {
        return int.Parse(playerName[^1].ToString());
    }
}

[Serializable]
public class PlayerDeck
{
    public string playerName;
    public bool firstMove = false;
    public List<Tile> playerTiles;
}

[Serializable]
public class PlayerResultData
{
    public string playerName;
    public string playerUserId;
    public int numberOfTilesLeft;
}

public enum ButtonType
{
    PickTileButton,
    TurnButton
}