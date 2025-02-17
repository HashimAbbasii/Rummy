using Newtonsoft.Json;
using Photon.Pun;
using RTLTMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class ResultPlayer
{
    public GameObject result;
    public List<GameObject> results;
    public RTLTextMeshPro playerName;
    public List<RTLTextMeshPro> playerNames;
    public TextMeshProUGUI playerScore;
    public List<TextMeshProUGUI> playerScores;
    public Image statusIcon;
    public List<Image> statusIcons;
    public string status;
}

public class GameResultPanelHandler : MonoBehaviour
{
    private PhotonView PV;
    private OnlineGamePlayScreenHandler _onlineGamePlayScreenHandler;
    private RamiAnnetteGameplayScreenHandler _ramiAnnetteGamePlayScreenHandler;
    private List<PlayerResultData> result = new List<PlayerResultData>();
    public List<ResultPlayer> resultPlayers;

    public Button playAgainButton;
    public List<Button> playAgainButtons;
    public RTLTextMeshPro playAgainText;
    public List<RTLTextMeshPro> playAgainTexts;
    public Sprite playAgainSprite;
    public Sprite notEnoughPointsSprite;

    private bool timerRuning;
    private int countDown;
    private int timer;

    public GameObject playAgainPanel;
    public GameObject winner;
    public GameObject loser;

    private void OnEnable()
    {
        PV = GetComponent<PhotonView>();
        switch (GameManager.Instance.gameMode)
        {
            case GameMode.Rami_31:
                _onlineGamePlayScreenHandler = UIManager.Instance.UIScreensReferences[GameScreens.OnlineGamePlayScreen].GetComponent<OnlineGamePlayScreenHandler>();
                break;
                
            case GameMode.Rami_Annette:
                _ramiAnnetteGamePlayScreenHandler = UIManager.Instance.UIScreensReferences[GameScreens.RamiAnnetteScreen].GetComponent<RamiAnnetteGameplayScreenHandler>();
                break;
            
            case GameMode.Rami_51:
                break;
        }
        
        playAgainButton.interactable = false;
        foreach (var p in playAgainButtons)
        {
            p.interactable = false;
        }
        timerRuning = false;
    }

    private void Update()
    {
        if (timerRuning)
        {
            if (timer != GetUnixTime())
            {
                timer = GetUnixTime();
                playAgainText.text = GameManager.Instance.Translate("Play Again") + " " + countDown--;
                foreach (var pat in playAgainTexts)
                {
                    pat.text = playAgainText.text;
                }
            }

            //timer completed
            if (countDown == 0)
            {
                timerRuning = false;

                int playerCount = 0;
                foreach (ResultPlayer resultPlayer in resultPlayers)
                {
                    if (resultPlayer.status == "Play Again")
                        playerCount++;
                }

                Debug.Log("Number of player wants to play: " + playerCount);

                foreach (NetworkPlayer networkPlayer in GameSetup.Instance.players.ToList())
                {
                    if (networkPlayer.playerUserId == PreferenceManager.UserID)
                    {
                        if (networkPlayer.playAgain && playerCount > 1)
                        {
                            PlayAgain(playerCount);
                            break;
                        }
                        else
                        {
                            switch (GameManager.Instance.gameMode)
                            {
                                case GameMode.Rami_31:
                                    _onlineGamePlayScreenHandler.OnConfirmButtonClick();
                                    break;
                
                                case GameMode.Rami_Annette:
                                    _ramiAnnetteGamePlayScreenHandler.OnConfirmButtonClick();
                                    break;
            
                                case GameMode.Rami_51:
                                    break;
                            }
                            
                            break;
                        }
                    }
                }
            }
        }
    }

    private IEnumerator CountdownCouroutine()
    {
        int countDown = 60;

        while (countDown != 0)
        {
            playAgainText.text = GameManager.Instance.Translate("Play Again") + " " + countDown--;
            foreach (var pat in playAgainTexts)
            {
                pat.text = playAgainText.text;
            }
            yield return new WaitForSeconds(1);
        }

        int playerCount = 0;
        foreach (ResultPlayer resultPlayer in resultPlayers)
        {
            if (resultPlayer.status == "Play Again")
                playerCount++;
        }

        Debug.Log("Number of player wants to play: " + playerCount);

        foreach (NetworkPlayer networkPlayer in GameSetup.Instance.players.ToList())
        {
            if (networkPlayer.playerUserId == PreferenceManager.UserID)
            {
                if (networkPlayer.playAgain && playerCount > 1)
                {
                    PlayAgain(playerCount);
                    break;
                }
                else
                {
                    switch (GameManager.Instance.gameMode)
                    {
                        case GameMode.Rami_31:
                            _onlineGamePlayScreenHandler.OnConfirmButtonClick();
                            break;
                
                        case GameMode.Rami_Annette:
                            _ramiAnnetteGamePlayScreenHandler.OnConfirmButtonClick();
                            break;
            
                        case GameMode.Rami_51:
                            break;
                    }
                    break;
                }
            }
        }
    }

    public void DisplayGameResult(List<PlayerResultData> _result, bool playerAvailable)
    {
        result = _result;
        
        foreach (NetworkPlayer networkPlayer in GameSetup.Instance.players)
            networkPlayer.playAgain = false;

        foreach (ResultPlayer rp in resultPlayers)
        {
            rp.result.SetActive(false);
        }

        foreach (PlayerResultData playerData in result)
        {
            resultPlayers[result.IndexOf(playerData)].result.SetActive(true);
            resultPlayers[result.IndexOf(playerData)].playerName.text = playerData.playerName;
            resultPlayers[result.IndexOf(playerData)].statusIcon.gameObject.SetActive(false);
            resultPlayers[result.IndexOf(playerData)].status = null;
        }

        resultPlayers[0].playerScore.text = (GameManager.Instance.room.roomCost * result.Count).ToString();

        switch (GameManager.Instance.gameMode)
        {
            case GameMode.Rami_31:
                _onlineGamePlayScreenHandler.winner = result[0].playerName;
                break;
                
            case GameMode.Rami_Annette:
                _ramiAnnetteGamePlayScreenHandler.winner = result[0].playerName;
                break;
            
            case GameMode.Rami_51:
                break;
        }
        
        if (PhotonNetwork.IsMasterClient)
        {
            DatabaseManager.Instance.SendGameResult(new GameResult() { winner = result[0].playerUserId, id = GameManager.Instance.room.roomCode, status = "over"}, OnSendResultComplete);
        }

        if (playerAvailable /*&& GameManager.Instance.roomType == RoomType.Private*/)
        {
            DatabaseManager.Instance.GetUserPoints(PreferenceManager.UserID, OnGetUserPointsComplete);
        }
    }

    public void DisplayGameResultAnnette(PlayerResultData winnerData, List<PlayerResultData> _result, bool playerAvailable)
    {
        result = _result;
        
        foreach (NetworkPlayer networkPlayer in GameSetup.Instance.players)
            networkPlayer.playAgain = false;

        foreach (ResultPlayer rp in resultPlayers)
        {
            rp.result.SetActive(false);
            foreach (var r in rp.results)
            {
                r.SetActive(false);
            }
        }
        
        foreach (PlayerResultData playerData in result)
        {
            resultPlayers[result.IndexOf(playerData)].result.SetActive(true);
            resultPlayers[result.IndexOf(playerData)].playerName.text = playerData.playerName;
            resultPlayers[result.IndexOf(playerData)].statusIcon.gameObject.SetActive(false);
            resultPlayers[result.IndexOf(playerData)].status = null;
            
            foreach (var r in resultPlayers[result.IndexOf(playerData)].results)
            {
                r.SetActive(true);
            }
            foreach (var pn in resultPlayers[result.IndexOf(playerData)].playerNames)
            {
                pn.text = playerData.playerName;
            }
            foreach (var si in resultPlayers[result.IndexOf(playerData)].statusIcons)
            {
                si.gameObject.SetActive(false);
            }
        }

        resultPlayers[0].playerScore.text = (GameManager.Instance.room.roomCost * result.Count).ToString();
        foreach (var ps in resultPlayers[0].playerScores)
        {
            ps.text = (GameManager.Instance.room.roomCost * result.Count).ToString();
        }
        
        _ramiAnnetteGamePlayScreenHandler.winner = winnerData.playerName;
        
        if (PhotonNetwork.IsMasterClient)
        {
            DatabaseManager.Instance.SendGameResult(new GameResult() { winner = winnerData.playerUserId, id = GameManager.Instance.room.roomCode, status = "over"}, OnSendResultComplete);
        }

        if (playerAvailable /*&& GameManager.Instance.roomType == RoomType.Private*/)
        {
            DatabaseManager.Instance.GetUserPoints(PreferenceManager.UserID, OnGetUserPointsComplete);
        }
    }
    
    private void OnGetUserPointsComplete(string userPoints, UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            PreferenceManager.Points = JsonConvert.DeserializeObject<Response>(request.downloadHandler.text).data.points;

            timer = GetUnixTime();
            countDown = 60;
            playAgainText.text = GameManager.Instance.Translate("Play Again") + " " + countDown;
            foreach (var pat in playAgainTexts)
            {
                pat.text = playAgainText.text;
            }
            timerRuning = true;

            //StartCoroutine(CountdownCouroutine());

            if (PreferenceManager.Points < GameManager.Instance.room.roomCost)
            {
                Debug.Log("Not Enough Points");
                
                playAgainButton.interactable = false;
                foreach (var p in playAgainButtons)
                {
                    p.interactable = false;
                }

                PV.RPC(nameof(RPC_UpdateStatus), RpcTarget.All, PreferenceManager.UserID, "Not Enough Points");
            }
            else
            {
                Debug.Log("Enough Points");
                
                playAgainButton.interactable = true;
                foreach (var p in playAgainButtons)
                {
                    p.interactable = true;
                }
                
                PV.RPC(nameof(RPC_UpdateStatus), RpcTarget.All, PreferenceManager.UserID, "Enough Points");
            }
        }
        else
        {
            DatabaseManager.Instance.GetUserPoints(PreferenceManager.UserID, OnGetUserPointsComplete);
        }
    }

    private void OnSendResultComplete(GameResult gameResult, UnityWebRequest request)
    {
        if (request.downloadHandler.text.Contains("Room is not active"))
        {
            return;
        }
        if (request.result != UnityWebRequest.Result.Success)
        {
            DatabaseManager.Instance.SendGameResult(new GameResult() { winner = gameResult.winner, id = gameResult.id, status = gameResult.status },
                OnSendResultComplete);
        }
    }

    [PunRPC]
    private void RPC_UpdateStatus(string userID, string status)
    {
        // if (result.Count == 0) return;
        // Debug.Log(userID);
        
        foreach (PlayerResultData playerData in result.Where(player => player.playerUserId == userID))
        {
            Debug.Log(userID + " Status : " + status);
            resultPlayers[result.IndexOf(playerData)].status = status;

            if (status == "Not Enough Points")
            {
                resultPlayers[result.IndexOf(playerData)].statusIcon.gameObject.SetActive(true);
                resultPlayers[result.IndexOf(playerData)].statusIcon.sprite = notEnoughPointsSprite;

                foreach (var si in resultPlayers[result.IndexOf(playerData)].statusIcons)
                {
                    si.gameObject.SetActive(true);
                    si.sprite = notEnoughPointsSprite;
                }
            }
            else if (status == "Enough Points")
            {
                
            }
            else if(status == "Play Again")
            {
                foreach (NetworkPlayer networkPlayer in GameSetup.Instance.players.Where(np => np.playerUserId == playerData.playerUserId))
                    networkPlayer.playAgain = true;

                resultPlayers[result.IndexOf(playerData)].statusIcon.gameObject.SetActive(true);
                resultPlayers[result.IndexOf(playerData)].statusIcon.sprite = playAgainSprite;

                foreach (var si in resultPlayers[result.IndexOf(playerData)].statusIcons)
                {
                    si.gameObject.SetActive(true);
                    si.sprite = playAgainSprite;
                }
                
                foreach (ResultPlayer resultPlayer in resultPlayers)
                {
                    if (!string.IsNullOrEmpty(resultPlayer.status) && resultPlayer.status != "Play Again") return;
                }

                timerRuning = false;
                PlayAgain(PhotonNetwork.PlayerList.Length);
            }
        }
    }

    private void PlayAgain(int playerCount)
    {
        GameSetup.Instance.PlayAgain();

        if (PhotonNetwork.NickName == "Player 1")
        {
            PlayAgain playAgain = new()
            {
                code = GameManager.Instance.room.roomKey,
                userId = GameManager.Instance.room.userID,
                players = new string[playerCount]
            };

            foreach (var player in GameSetup.Instance.players.Where(p => !string.IsNullOrEmpty(p.playerUserId)))
            {
                playAgain.players[GameSetup.Instance.players.IndexOf(player)] = player.playerUserId;
            }
            
            DatabaseManager.Instance.PlayAgain(playAgain, OnPlayAgainComplete);
        }
    }

    private void OnPlayAgainComplete(PlayAgain playAgain, UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            PV.RPC(nameof(RPC_PlayAgain), RpcTarget.All, JsonConvert.DeserializeObject<Response>(request.downloadHandler.text).data.code, 
                JsonConvert.DeserializeObject<Response>(request.downloadHandler.text).data._id,
                JsonConvert.DeserializeObject<Response>(request.downloadHandler.text).data.playersNum);
        }
    }

    [PunRPC]
    private void RPC_PlayAgain(string roomKey, string roomCode, int playerCount)
    {
        GameManager.Instance.room.roomKey = roomKey;
        GameManager.Instance.room.roomCode = roomCode;
        gameObject.SetActive(false);

        switch (GameManager.Instance.gameMode)
        {
            case GameMode.Rami_31:
                _onlineGamePlayScreenHandler.PlayAgain(playerCount);
                break;
                
            case GameMode.Rami_Annette:
                _ramiAnnetteGamePlayScreenHandler.PlayAgain(playerCount);
                break;
            
            case GameMode.Rami_51:
                break;
        }
    }

    public void OnPlayAgainButtonClick()
    {
        playAgainButton.interactable = false;
        foreach (var p in playAgainButtons)
        {
            p.interactable = false;
        }
        PV.RPC(nameof(RPC_UpdateStatus), RpcTarget.All, PreferenceManager.UserID, "Play Again");
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public static int GetUnixTime()
    {
        return (int)(DateTime.UtcNow - new DateTime(2023, 1, 1)).TotalSeconds;
    }
}

