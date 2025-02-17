using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using RTLTMPro;
using System;
using UnityEngine.Serialization;

public class OnlineGamePlayScreenHandler : GamePlayScreenHandler
{
    public static int GetUnixTime()
    {
        return (int)(DateTime.UtcNow - new DateTime(2023, 1, 1)).TotalSeconds;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        winningPoints.SetActive(false);
        roomKey.SetActive(true);
        roomKey.transform.GetComponentInChildren<RTLTextMeshPro>().text = GameManager.Instance.Translate("Room Key: ") + GameManager.Instance.room.roomKey;

        numberOfPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        playersDeck = new List<PlayerDeck>(numberOfPlayers);
        for (int i = 0; i < numberOfPlayers; i++)
        {
            PlayerDeck playerDeck = new()
            {
                playerName = "Player " + (i + 1),
                playerTiles = new List<Tile>()
            };
            playersDeck.Add(playerDeck);
        }

        runsSequenceButton.interactable = false;
        groupSequenceButton.interactable = false;
        DisplayButton(ButtonType.PickTileButton);
        HideMessage();
        resultPanel.SetActive(false);
        leaveGamePanel.SetActive(false);
        chatPanel.SetActive(false);
        
        
        DatabaseManager.Instance.GetPharases(OnGetPharasesComplete);
    }

    private void OnGetPharasesComplete(Pharases pharases, UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            chatContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, (chatMessage.GetComponent<RectTransform>().rect.height + 5) * pharases.data.Length);

            for (int i = 0; i < pharases.data.Length; i++)
            {
                GameObject tempChatMessage = Instantiate(chatMessage);
                tempChatMessage.transform.SetParent(chatContent.transform);
                tempChatMessage.GetComponent<ChatMessage>().messageText.text = pharases.data[i];
            }
        }
        else
        {

        }
    }

    private void Update()
    {
        //All players have joined the room, start the game
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length == numberOfPlayers && PhotonNetwork.CurrentRoom.IsOpen)
        {
            photonView.RPC(nameof(RPC_InstantiateTiles), RpcTarget.AllBuffered);
        }

        //All players left the game and only one player is present in room
        if (!PhotonNetwork.CurrentRoom.IsOpen && PhotonNetwork.PlayerList.Length == 1 && !resultPanel.activeInHierarchy)
        {
            photonView.RPC(nameof(RPC_DisplayResult), RpcTarget.All, false);
            ResetAll();
        }

        deckText.text = deckCount.ToString();

        if (timerRunning)
        {
            if (timer != GetUnixTime())
            {
                timer = GetUnixTime();
                GameSetup.Instance.players[currentTurn].playerIcon.timerImage.fillAmount += 1f / GameManager.Instance.room.standByTime;
            }

            //timer completed
            if (initialTime + GameManager.Instance.room.standByTime == GetUnixTime()/* && !tilePicked*/)
            {
                timerRunning = false;
                //for local client if any tile is in the air
                if (PhotonNetwork.NickName == playerTurn && GameManager.Instance.dragTiles.Count != 0)
                {
                    MoveBackDraggedTiles();
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    Invoke(nameof(ValidateBeforeNextTurn), 2f);
                }
            }
        }

    }

    // public void SetPlayerIcons()
    // {
    //     int count = 1;
    //     foreach (NetworkPlayer networkPlayer in GameSetup.Instance.players)
    //     {
    //         if (networkPlayer.player)
    //         {
    //             networkPlayer.playerIcon.gameIcon.SetActive(true);
    //             networkPlayer.playerIcon.playerNameText.text = networkPlayer.playerName;
    //             networkPlayer.playerIcon.playerNickName = networkPlayer.playerNickName;
    //
    //             networkPlayer.playerIcon.transform.position = networkPlayer.player.ViewID == GameSetup.Instance.myNetworkPlayer?.ViewID ? playerIconPositions[0].position : playerIconPositions[count++].position;
    //         }
    //     }
    //
    //     for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
    //     {
    //         Debug.Log("Log: " + i);
    //         GameSetup.Instance.players[i].playerIcon.waitingIcon.SetActive(GameSetup.Instance.players[i].seatAvailable);
    //     }
    // }

    [PunRPC]
    public void RPC_InstantiateTiles()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        winningPoints.SetActive(true);
        winningPoints.transform.GetChild(0).GetComponent<RTLTextMeshPro>().text = (GameManager.Instance.room.roomCost * playersDeck.Count).ToString();
        roomKey.SetActive(false);

        //instantiate tiles for the first time
        if (tiles.Count == 0)
        {
            //instantiating black tiles
            for (int i = 0; i < 2; i++)
            {
                for (int j = 1; j <= 13; j++)
                {
                    tiles.Add(Instantiate(tilePrefab));
                    tiles[^1].GetComponent<PhotonView>().ViewID = tiles.Count;
                    tiles[^1].transform.SetParent(deckParent);
                    tiles[^1].transform.position = deckParent.position;
                    tiles[^1].transform.localScale = new Vector3(1, 1, 1);
                    tiles[^1].tileColor = TileColor.Black;
                    tiles[^1].tileNumber = j;
                    tiles[^1].tileNumberText.text = j.ToString();
                    tiles[^1].tileNumberText.color = black;
                    tiles[^1].tileNumberText.gameObject.SetActive(true);
                    tiles[^1].jokerImage.gameObject.SetActive(false);
                    tiles[^1].name = "Tile " + j + " " + TileColor.Black.ToString() + " " + (i + 1);
                }
            }

            //instantiating blue tiles
            for (int i = 0; i < 2; i++)
            {
                for (int j = 1; j <= 13; j++)
                {
                    tiles.Add(Instantiate(tilePrefab));
                    tiles[^1].GetComponent<PhotonView>().ViewID = tiles.Count;
                    tiles[^1].transform.SetParent(deckParent);
                    tiles[^1].transform.position = deckParent.position;
                    tiles[^1].transform.localScale = new Vector3(1, 1, 1);
                    tiles[^1].tileColor = TileColor.Blue;
                    tiles[^1].tileNumber = j;
                    tiles[^1].tileNumberText.text = j.ToString();
                    tiles[^1].tileNumberText.color = blue;
                    tiles[^1].tileNumberText.gameObject.SetActive(true);
                    tiles[^1].jokerImage.gameObject.SetActive(false);
                    tiles[^1].name = "Tile " + j + " " + TileColor.Blue.ToString() + " " + (i + 1);
                }
            }

            //instantiating yellow tiles
            for (int i = 0; i < 2; i++)
            {
                for (int j = 1; j <= 13; j++)
                {
                    tiles.Add(Instantiate(tilePrefab));
                    tiles[^1].GetComponent<PhotonView>().ViewID = tiles.Count;
                    tiles[^1].transform.SetParent(deckParent);
                    tiles[^1].transform.position = deckParent.position;
                    tiles[^1].transform.localScale = new Vector3(1, 1, 1);
                    tiles[^1].tileColor = TileColor.Yellow;
                    tiles[^1].tileNumber = j;
                    tiles[^1].tileNumberText.text = j.ToString();
                    tiles[^1].tileNumberText.color = yellow;
                    tiles[^1].tileNumberText.gameObject.SetActive(true);
                    tiles[^1].jokerImage.gameObject.SetActive(false);
                    tiles[^1].name = "Tile " + j + " " + TileColor.Yellow.ToString() + " " + (i + 1);
                }
            }

            //instantiating red tiles
            for (int i = 0; i < 2; i++)
            {
                for (int j = 1; j <= 13; j++)
                {
                    tiles.Add(Instantiate(tilePrefab));
                    tiles[^1].GetComponent<PhotonView>().ViewID = tiles.Count;
                    tiles[^1].transform.SetParent(deckParent);
                    tiles[^1].transform.position = deckParent.position;
                    tiles[^1].transform.localScale = new Vector3(1, 1, 1);
                    tiles[^1].tileColor = TileColor.Red;
                    tiles[^1].tileNumber = j;
                    tiles[^1].tileNumberText.text = j.ToString();
                    tiles[^1].tileNumberText.color = red;
                    tiles[^1].tileNumberText.gameObject.SetActive(true);
                    tiles[^1].jokerImage.gameObject.SetActive(false);
                    tiles[^1].name = "Tile " + j + " " + TileColor.Red.ToString() + " " + (i + 1);
                }
            }

            //instantiating joker tiles
            for (int i = 0; i < 2; i++)
            {
                tiles.Add(Instantiate(tilePrefab));
                tiles[^1].GetComponent<PhotonView>().ViewID = tiles.Count;
                tiles[^1].transform.SetParent(deckParent);
                tiles[^1].transform.position = deckParent.position;
                tiles[^1].transform.localScale = new Vector3(1, 1, 1);
                tiles[^1].tileColor = TileColor.Joker;
                tiles[^1].tileNumber = 0;
                tiles[^1].tileNumberText.gameObject.SetActive(false);
                tiles[^1].jokerImage.gameObject.SetActive(true);
                tiles[^1].jokerImage.sprite = tiles[^1].jokerSprites[i];
                tiles[^1].name = "Tile " + TileColor.Joker.ToString() + " " + (i + 1);
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            CreateDeck();
        }
    }

    private void CreateDeck()
    {
        var rand = new System.Random();
        List<int> randomArray = new();

        for (int i = 0; i < tiles.Count; i++)
            randomArray.Add(rand.Next());

        photonView.RPC(nameof(RPC_ShuffleDeck), RpcTarget.AllBuffered, (object) randomArray.ToArray());
    }

    [PunRPC]
    private void RPC_ShuffleDeck(int[] _randomNumbers)
    {
        var i = 0;
        deck = tiles.OrderBy(x => _randomNumbers[i++]).ToList();

        foreach (Tile tile in deck)
            tile.transform.SetAsFirstSibling();

        deckCount = deck.Count;
        DistributeTiles();
    }

    private void DistributeTiles()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            //Testing
            //if (playersDeck[i].playerName == "Player 1")
            //{
            //    for (int j = 0; j < 8; j++)
            //    {
            //        foreach (Tile tile in deck)
            //        {
            //            if (tile.tileNumber == 9 || tile.tileNumber == 10 || tile.tileNumber == 11 || tile.tileNumber == 12 || tile.tileNumber == 13)
            //            {
            //                if (tile.tileColor == TileColor.Black)
            //                {
            //                    playersDeck[i].playerTiles.Add(tile);
            //                    deck.Remove(tile);
            //                    break;
            //                }
            //            }
            //        }
            //    }

            //    for (int j = 0; j < 2; j++)
            //    {
            //        foreach (Tile tile in deck)
            //        {
            //            if (tile.tileNumber == 0)
            //            {
            //                playersDeck[i].playerTiles.Add(tile);
            //                deck.Remove(tile);
            //                break;
            //            }
            //        }
            //    }

            //    for (int j = 0; j < 8; j++)
            //    {
            //        playersDeck[i].playerTiles.Add(deck[^1]);
            //        deck.RemoveAt(deck.Count - 1);
            //    }
            //}
            //else
            //{
            //    for (int j = 0; j < 14; j++)
            //    {
            //        playersDeck[i].playerTiles.Add(deck[^1]);
            //        deck.RemoveAt(deck.Count - 1);
            //    }
            //}
            //Testing

            //Original Code
            
            for (int j = 0; j < 14; j++)
            {
                playersDeck[i].playerTiles.Add(deck[^1]);
                deck.RemoveAt(deck.Count - 1);
            }
        }

        StartCoroutine(DistributeTilesCoroutine());
    }

    private IEnumerator DistributeTilesCoroutine()
    {
        yield return new WaitForSeconds(2f);

        AudioManager.Instance.PlayAudio(AudioName.FirstTileDistributeAudio);

        for (int i = 0; i < playersDeck[0].playerTiles.Count; i++)
        {
            AudioManager.Instance.PlayAudio(AudioName.DistributeTileAudio);

            for (int j = 0; j < numberOfPlayers; j++)
            {
                playersDeck[j].playerTiles[i].gameObject.SetActive(true);
                playersDeck[j].playerTiles[i].transform.LeanMove(GameSetup.Instance.players[j].playerIcon.transform.position, 0.5f);
                deckCount--; deckText.text = deckCount.ToString();
                StartCoroutine(HideTileCoverImageCoroutine(playersDeck[j].playerTiles[i]));
                GameSetup.Instance.players[j].playerIcon.tilesCount++;
            }

            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(0.5f);

        int playerNumber = PlayerNumber(PhotonNetwork.NickName);

        foreach (Tile tile in playersDeck[playerNumber - 1].playerTiles)
        {
            AudioManager.Instance.PlayAudio(AudioName.DistributeTileAudio);
            tile.transform.LeanMove(GridManager.Instance.playerGrid[playersDeck[playerNumber - 1].playerTiles.IndexOf(tile)].transform.position, 0.5f);
            GridManager.Instance.playerGrid[playersDeck[playerNumber - 1].playerTiles.IndexOf(tile)].Fill(tile);
            yield return new WaitForSeconds(0.2f);
        }

        runsSequenceButton.interactable = true;
        groupSequenceButton.interactable = true;

        if (PhotonNetwork.IsMasterClient)
        {
            GiveTurn();
        }
    }

    private IEnumerator HideTileCoverImageCoroutine(Tile tile)
    {
        yield return new WaitForSeconds(0.6f);
        tile.tileCoverImage.gameObject.SetActive(false);
    }

    private void GiveTurn()
    {
        if (!IsGameCompleted())
        {
            currentTurn = (currentTurn + 1) % numberOfPlayers;
            photonView.RPC(nameof(RPC_StartTimer), RpcTarget.AllBuffered, currentTurn);
        }
    }

    [PunRPC]
    private void RPC_StartTimer(int _currentTurn)
    {
        tilePicked = false;
        currentTurn = _currentTurn;
        playerTurn = "Player " + (currentTurn + 1).ToString();

        timer = GetUnixTime();
        initialTime = GetUnixTime();

        timerRunning = true;

        //coroutine = TimerCoroutine(currentTurn);
        //StartCoroutine(coroutine);
    }

    //private IEnumerator TimerCoroutine(int playerIndexNumber)
    //{
    //    while (GameSetup.Instance.players[playerIndexNumber].playerIcon.timerImage.fillAmount != 1)
    //    {
    //        GameSetup.Instance.players[playerIndexNumber].playerIcon.timerImage.fillAmount += 1f / GameManager.Instance.room.standByTime;
    //        yield return new WaitForSeconds(1f);
    //    }

    //    //for local client if any tile is in the air
    //    if (PhotonNetwork.NickName == playerTurn && GameManager.Instance.dragTiles.Count != 0)
    //    {
    //        MoveBackDraggedTiles();
    //    }

    //    if (PhotonNetwork.IsMasterClient)
    //    {
    //        Invoke(nameof(ValidateBeforeNextTurn), 2f);
    //    }  
    //}

    private void MoveBackDraggedTiles()
    {
        // Debug.Log("MoveBackDraggedTiles");
        
        List<TilePicked> tempDragTiles = new(GameManager.Instance.dragTiles);
        GameManager.Instance.dragTiles.Clear();
        foreach (TilePicked tilePicked in tempDragTiles)
        {
            tilePicked.tile.transform.LeanMove(tilePicked.sourceGridElement.transform.position, 0.5f);
            tilePicked.sourceGridElement.Fill(tilePicked.tile);
        }
    }

    private void ValidateBeforeNextTurn()
    {
        // Debug.Log("ValidateBeforeNextTurn");
        
        bool playerMovedTile = false;

        foreach (MovedTile movedTile in GameManager.Instance.movedTiles)
        {
            if ((!movedTile.sourceGridElement && !movedTile.targetGridElement.playerGrid) || (movedTile.sourceGridElement.playerGrid && !movedTile.targetGridElement.playerGrid))
            {
                playerMovedTile = true;
                break;
            }
        }

        if (GameManager.Instance.FirstMoveIsValid(PlayerNumber(playerTurn)))
        {
            photonView.RPC(nameof(RPC_Next), RpcTarget.All, true, GridManager.Instance.CorrectSequenceOnTable(), playerMovedTile);
        }
        else
        {
            photonView.RPC(nameof(RPC_Next), RpcTarget.All, false, true, playerMovedTile);
        }
    }

    [PunRPC]
    private void RPC_NextPlayerTurn(bool penalty)
    {
        // Debug.Log("RPC_NextPlayerTurn");
        
        int playerNumber = PlayerNumber(playerTurn);

        if (penalty && playersDeck[playerNumber - 1].playerTiles.Count != GridManager.Instance.playerGrid.Count)
        {
            playersDeck[playerNumber - 1].playerTiles.Add(deck[^1]);
            deck.RemoveAt(deck.Count - 1);
            deckCount--; deckText.text = deckCount.ToString();
            playersDeck[playerNumber - 1].playerTiles[^1].gameObject.SetActive(true);
            GameSetup.Instance.players[playerNumber - 1].playerIcon.tilesCount++;

            if (PhotonNetwork.NickName == playerTurn)
            {
                foreach (GridElement gridElement in GridManager.Instance.playerGrid)
                {
                    if (gridElement.tileStatus == TileStatus.Vacant)
                    {
                        playersDeck[playerNumber - 1].playerTiles[^1].transform.LeanMove(gridElement.transform.position, 0.5f);
                        gridElement.Fill(playersDeck[playerNumber - 1].playerTiles[^1]);
                        StartCoroutine(HideTileCoverImageCoroutine(playersDeck[playerNumber - 1].playerTiles[^1]));
                        break;
                    }
                }
            }
            else
            {
                playersDeck[playerNumber - 1].playerTiles[^1].transform.LeanMove(GameSetup.Instance.players[playerNumber - 1].playerIcon.transform.position, 0.5f);
                StartCoroutine(HideTileCoverImageCoroutine(playersDeck[playerNumber - 1].playerTiles[^1]));
            }
        }
        else
        {
            foreach (GridElement gridElement in GridManager.Instance.grid)
            {
                if(gridElement.tile != null)
                    gridElement.tile.boardTile = true;
            }
        }

        //StopCoroutine(coroutine);
        timerRunning = false;
        GameSetup.Instance.players[playerNumber - 1].playerIcon.timerImage.fillAmount = 0;

        if (PhotonNetwork.IsMasterClient)
        {
            GiveTurn();
        }
    }

    public void RemoveTileFromPlayerTiles(Tile tileMoved)
    {
        // Debug.Log("RemoveTileFromPlayerTiles");
        
        int playerNumber = PlayerNumber(playerTurn);

        foreach (Tile playerTile in playersDeck[playerNumber - 1].playerTiles)
        {
            if (playerTile.name == tileMoved.name)
            {
                playersDeck[playerNumber - 1].playerTiles.RemoveAt(playersDeck[playerNumber - 1].playerTiles.IndexOf(playerTile));
                GameSetup.Instance.players[playerNumber - 1].playerIcon.tilesCount--;
                break;
            }
        }
    }

    private bool IsGameCompleted()
    {
        //testing
        //PV.RPC(nameof(RPC_DisplayResult), RpcTarget.All, true);
        //return true;

        //if any player's deck gets empty
        
        foreach (PlayerDeck playerDeck in playersDeck)
        {
            if (playerDeck.playerTiles.Count == 0)
            {
                photonView.RPC(nameof(RPC_DisplayResult), RpcTarget.All, true);
                return true;
            }
        }

        //if deck gets empty
        if (deck.Count == 0)
        {
            photonView.RPC(nameof(RPC_DisplayResult), RpcTarget.All, true);
            return true;
        }

        return false;
    }

    [PunRPC]
    private void RPC_DisplayResult(bool playerAvailable)
    {
        resultPanel.SetActive(true);
        List<PlayerResultData> result = new(playersDeck.Count);
        PlayerResultData winner = new();

        foreach (PlayerDeck playerDeck in playersDeck)
        {
            result.Add(new PlayerResultData()
            {
                playerName = GameSetup.Instance.players[playersDeck.IndexOf(playerDeck)].playerName,
                playerUserId = GameSetup.Instance.players[playersDeck.IndexOf(playerDeck)].playerUserId,
                numberOfTilesLeft = playerDeck.playerTiles.Count
            });

            if (PhotonNetwork.NickName == playerDeck.playerName)
            {
                winner = result[^1];
                Debug.Log("Winner Name: " + winner.playerName);
            }
        }

        result = result.OrderBy(o => o.numberOfTilesLeft).ToList();

        if (!playerAvailable)
        {
            result.Remove(winner);
            result.Insert(0, winner);
        }

        resultPanel.GetComponent<GameResultPanelHandler>().DisplayGameResult(result, playerAvailable);
    }

    public void DisplayButton(ButtonType buttonType)
    {
        switch (buttonType)
        {
            case ButtonType.TurnButton:
                turnButtons.SetActive(true);
                pickTileButton.SetActive(false);
                break;
            case ButtonType.PickTileButton:
                turnButtons.SetActive(false);
                pickTileButton.SetActive(true);
                break;
        }
    }

    public void SendChatMessage(string message)
    {
        chatPanel.SetActive(false);
        photonView.RPC(nameof(RPC_SendChatMessage), RpcTarget.All, PhotonNetwork.NickName, message);
    }

    [PunRPC]
    public void RPC_SendChatMessage(string senderName, string message)
    {
        GameSetup.Instance.players[PlayerNumber(senderName) - 1].playerIcon.DisplayMessage(message);
    }


    #region Button Click functions

    public void OnChatButtonClick()
    {
        chatPanel.SetActive(!chatPanel.activeInHierarchy);
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void OnCopyButtonClick()
    {
        GUIUtility.systemCopyBuffer = GameManager.Instance.room.roomKey;
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void OnRunsSequenceButtonClick()
    {
        int playerNumber = PlayerNumber(PhotonNetwork.NickName);

        foreach (Tile tile in playersDeck[playerNumber - 1].playerTiles)
            tile.tileType = TileType.Primary;

        foreach (Tile tile in playersDeck[playerNumber - 1].playerTiles)
        {
            foreach (Tile item in playersDeck[playerNumber - 1].playerTiles)
            {
                if (tile.name != item.name &&
                    tile.tileColor == item.tileColor &&
                    tile.tileNumber == item.tileNumber &&
                    tile.tileType == item.tileType)
                {
                    tile.tileType = TileType.Secondary;
                    break;
                }
            }
        }

        playersDeck[playerNumber - 1].playerTiles = playersDeck[playerNumber - 1].playerTiles.OrderBy(tile => (tile.tileColor, tile.tileType, tile.tileNumber)).ToList();

        foreach (GridElement gridElement in GridManager.Instance.playerGrid)
        {
            gridElement.Clear();
        }

        //foreach (Tile tile in playersDeck[playerNumber - 1].playerTiles)
        //{
        //    tile.transform.LeanMove(GridManager.Instance.playerGrid[playersDeck[playerNumber - 1].playerTiles.IndexOf(tile)].transform.position, 0.2f);
        //    GridManager.Instance.playerGrid[playersDeck[playerNumber - 1].playerTiles.IndexOf(tile)].Fill(tile);
        //}

        //Milestone Part: 2
        
        PlaceTiles(playerNumber, "Runs");

        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void OnGroupSequenceButtonClick()
    {
        int playerNumber = PlayerNumber(PhotonNetwork.NickName); ;

        foreach (Tile tile in playersDeck[playerNumber - 1].playerTiles)
            tile.tileType = TileType.Primary;

        foreach (Tile tile in playersDeck[playerNumber - 1].playerTiles)
        {
            foreach (Tile item in playersDeck[playerNumber - 1].playerTiles)
            {
                if (tile.name != item.name &&
                    tile.tileColor == item.tileColor &&
                    tile.tileNumber == item.tileNumber &&
                    tile.tileType == item.tileType)
                {
                    tile.tileType = TileType.Secondary;
                    break;
                }
            }
        }

        playersDeck[playerNumber - 1].playerTiles = playersDeck[playerNumber - 1].playerTiles.OrderBy(tile => (tile.tileNumber, tile.tileType)).ToList();

        foreach (GridElement gridElement in GridManager.Instance.playerGrid)
        {
            gridElement.Clear();
        }

        //foreach (Tile tile in playersDeck[playerNumber - 1].playerTiles)
        //{
        //    tile.transform.LeanMove(GridManager.Instance.playerGrid[playersDeck[playerNumber - 1].playerTiles.IndexOf(tile)].transform.position, 0.2f);
        //    GridManager.Instance.playerGrid[playersDeck[playerNumber - 1].playerTiles.IndexOf(tile)].Fill(tile);
        //}

        //Milestone Part: 2
        
        PlaceTiles(playerNumber, "Group");

        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void OnPickTileButtonClick()
    {
        // Debug.Log("OnPickTileButtonClick");
        
        if (playerTurn == PhotonNetwork.NickName && !tilePicked)
        {
            tilePicked = true;
            if (!GridManager.Instance.CorrectSequenceOnTable())
            {
                photonView.RPC(nameof(RPC_Undo), RpcTarget.AllBuffered);
            }

            photonView.RPC(nameof(RPC_NextPlayerTurn), RpcTarget.AllBuffered, true);
            AudioManager.Instance.PlayAudio(AudioName.DrawTileAudio);
        }
    }

    public void OnNextButtonClick()
    {
        
        if (playerTurn == PhotonNetwork.NickName)
        {
            ValidateBeforeNextTurn();
        }
    }

    [PunRPC]
    private void RPC_Next(bool firstMoveIsValid, bool correctSequenceOnTable, bool playerMovedTile)
    {
        Debug.Log("Player Moved Tile: " + playerMovedTile + " First Move Is Valid: " + firstMoveIsValid + " Correct Sequence On Table: " + correctSequenceOnTable);

        if (!firstMoveIsValid || !correctSequenceOnTable || !playerMovedTile)
        {
            GameManager.Instance.tilesList.Clear();
            DisplayButton(ButtonType.PickTileButton);
            RPC_Undo();

            if (PhotonNetwork.NickName == playerTurn)
                AudioManager.Instance.PlayAudio(AudioName.WrongMoveAudio);

            RPC_NextPlayerTurn(true);
        }
        else
        {
            playersDeck[PlayerNumber(playerTurn) - 1].firstMove = true;
            GameManager.Instance.movedTiles.Clear();
            GameManager.Instance.tilesList.Clear();
            DisplayButton(ButtonType.PickTileButton);

            if (PhotonNetwork.NickName == playerTurn)
                AudioManager.Instance.PlayAudio(AudioName.MadeSetAudio);

            RPC_NextPlayerTurn(false);
        }
    }

    public void OnUndoButtonClick()
    {
        photonView.RPC(nameof(RPC_Undo), RpcTarget.AllBuffered);
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    [PunRPC]
    private void RPC_Undo()
    {
        int playerNumber = PlayerNumber(playerTurn);

        foreach (MovedTile movedTile in GameManager.Instance.movedTiles)
        {
            //for local player
            if (playerTurn == PhotonNetwork.NickName)
            {
                if (movedTile.sourceGridElement.playerGrid)
                {
                    playersDeck[playerNumber - 1].playerTiles.Add(movedTile.tile);
                    GameSetup.Instance.players[playerNumber - 1].playerIcon.tilesCount++;
                }
            }
            //for network player
            else
            {
                if (movedTile.sourceGridElement == null)
                {
                    playersDeck[playerNumber - 1].playerTiles.Add(movedTile.tile);
                    GameSetup.Instance.players[playerNumber - 1].playerIcon.tilesCount++;
                }
            }
        }

        GameManager.Instance.Undo(playerTurn, playerNumber);
        DisplayButton(ButtonType.PickTileButton);
    }

    public void OnHomeButtonClick()
    {
        leaveGamePanel.SetActive(true);
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void OnCancelButtonClick()
    {
        leaveGamePanel.SetActive(false);
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void OnConfirmButtonClick()
    {
        StartCoroutine(LeaveGameCouroutine());
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    private IEnumerator LeaveGameCouroutine()
    {
        PhotonNetwork.LeaveRoom();
        yield return new WaitWhile(() => PhotonNetwork.InRoom);

        if (PhotonNetwork.PlayerList.Length == 1 && string.IsNullOrEmpty(winner))
            DatabaseManager.Instance.SendGameResult(new GameResult() { id = GameManager.Instance.room.roomCode, status = "cancel" }, OnSendResultComplete);

        ResetAll();
        UIManager.Instance.DisplaySpecificScreen(GameScreens.MainScreen);
    }

    private void OnSendResultComplete(GameResult gameResult, UnityWebRequest request)
    {
        
    }

    public void PlaceTiles(int playerNumber, string _sequence)
    {
        //foreach (Tile tile in playersDeck[playerNumber - 1].playerTiles)
        //{
        //    Debug.Log("Place Tiles Log: Aranged Tile: " + tile.name);
        //}

        // Debug.Log("Place Tiles");
        
        int sequenceCount = 3;
        int sequencePlaced = 0;

        sequence = null;
        tempTiles.Clear();
        totalTiles.Clear();

        for (int i = 0; i < playersDeck[playerNumber - 1].playerTiles.Count - 2; i++)
        {
            sequenceCount = 3;

            for (int j = 0; j < sequenceCount; j++)
            {
                totalTiles.Add(playersDeck[playerNumber - 1].playerTiles[i + j]);
            }

            if (totalTiles.Count >= 3)
            {
                tempTiles.Clear(); tempTiles.Add(totalTiles[0]);

                //Debug.Log("------------------");
                //foreach (Tile tile in totalTiles)
                //{
                //    Debug.Log("Place Tiles Log: Tile: " + tile.name);
                //}

                if (_sequence == "Runs")
                {
                    if (TilesInRunsSequence())
                    {
                        bool flag = true;

                        while (flag)
                        {
                            if (TilesInRunsSequence() && (sequenceCount + 1 + i) <= playersDeck[playerNumber - 1].playerTiles.Count)
                            {
                                //Debug.Log("Place Tiles Log: Sequence Count: " + sequenceCount);
                                totalTiles.Add(playersDeck[playerNumber - 1].playerTiles[sequenceCount++ + i]);
                                tempTiles.Clear(); tempTiles.Add(totalTiles[0]);

                                //Debug.Log("------------------");
                                //foreach (Tile tile in totalTiles)
                                //{
                                //    Debug.Log("Place Tiles Log: Tile: " + tile.name);
                                //}
                            }
                            else if (TilesInRunsSequence() && (sequenceCount + i) == playersDeck[playerNumber - 1].playerTiles.Count)
                            {
                                sequenceCount++;
                            }
                            else
                            {
                                flag = false;
                            }
                        }

                        for (int k = 0; k < sequenceCount - 1; k++)
                        {
                            Tile tile = playersDeck[playerNumber - 1].playerTiles[i + k];

                            tile.transform.LeanMove(GridManager.Instance.playerGrid[sequencePlaced].transform.position, 0.2f);
                            GridManager.Instance.playerGrid[sequencePlaced].Fill(tile);
                            sequencePlaced++;
                        }

                        i += sequenceCount - 2;
                        sequencePlaced++;
                    }
                }
                else if (_sequence == "Group")
                {
                    if (TilesInGroupSequence())
                    {
                        bool flag = true;

                        while (flag)
                        {
                            if (TilesInGroupSequence() && (sequenceCount + 1 + i) <= playersDeck[playerNumber - 1].playerTiles.Count)
                            {
                                //Debug.Log("Place Tiles Log: Sequence Count: " + sequenceCount);
                                totalTiles.Add(playersDeck[playerNumber - 1].playerTiles[sequenceCount++ + i]);
                                tempTiles.Clear(); tempTiles.Add(totalTiles[0]);

                                //Debug.Log("------------------");
                                //foreach (Tile tile in totalTiles)
                                //{
                                //    Debug.Log("Place Tiles Log: Tile: " + tile.name);
                                //}
                            }
                            else if (TilesInGroupSequence() && (sequenceCount + i) == playersDeck[playerNumber - 1].playerTiles.Count)
                            {
                                sequenceCount++;
                            }
                            else
                            {
                                flag = false;
                            }
                        }

                        for (int k = 0; k < sequenceCount - 1; k++)
                        {
                            Tile tile = playersDeck[playerNumber - 1].playerTiles[i + k];

                            tile.transform.LeanMove(GridManager.Instance.playerGrid[sequencePlaced].transform.position, 0.2f);
                            GridManager.Instance.playerGrid[sequencePlaced].Fill(tile);
                            sequencePlaced++;
                        }

                        i += sequenceCount - 2;
                        sequencePlaced++;
                    }
                }
            }

            sequence = null;
            tempTiles.Clear();
            totalTiles.Clear();
        }

        foreach (Tile tile in playersDeck[playerNumber - 1].playerTiles)
        {
            bool tileAlreadyPlaced = false;
            foreach (GridElement gridElement in GridManager.Instance.playerGrid)
            {
                if (gridElement.tile == tile)
                {
                    tileAlreadyPlaced = true;
                    break;
                }
            }

            if (!tileAlreadyPlaced)
            {
                tile.transform.LeanMove(GridManager.Instance.playerGrid[sequencePlaced].transform.position, 0.2f);
                GridManager.Instance.playerGrid[sequencePlaced].Fill(tile);
                sequencePlaced++;
            }
        }
    }

    public bool TilesInRunsSequence()
    {
        // Debug.Log("TilesInRunsSequence");
        
        if (tempTiles.Count == totalTiles.Count)
        {
            return true;
        }

        Tile nextTile = new();

        foreach (Tile tile in totalTiles)
        {
            if (tile.name == tempTiles[^1].name)
            {
                nextTile = totalTiles[totalTiles.IndexOf(tile) + 1];
                break;
            }
        }

        if (string.IsNullOrEmpty(sequence))
        {
            //check for next Joker Tile
            if (nextTile.tileColor == TileColor.Joker)
            {
                tempTiles.Add(nextTile);
                TilesInRunsSequence();
            }
            //if current tile is Joker
            else if (tempTiles[^1].tileColor == TileColor.Joker)
            {
                if (tempTiles[^2].tileColor == TileColor.Joker)
                {
                    //check for Runs
                    CheckForRunsSequence(nextTile, 3);
                }
                else
                {
                    //check for Runs
                    CheckForRunsSequence(nextTile, 2);
                }
            }
            else
            {
                //check for Runs
                CheckForRunsSequence(nextTile, 1);
            }
        }
        else if (sequence == "Runs")
        {
            //check for next Joker Tile
            if (nextTile.tileColor == TileColor.Joker)
            {
                //check if current tile is not 13
                if (tempTiles[^1].tileColor != TileColor.Joker && tempTiles[^1].tileNumber != 13)
                {
                    tempTiles.Add(nextTile);
                    TilesInRunsSequence();
                }
                //check if current tile is joker and second last is not 12 
                else if (tempTiles[^1].tileColor == TileColor.Joker && tempTiles[^2].tileNumber != 12)
                {
                    tempTiles.Add(nextTile);
                    TilesInRunsSequence();
                }
            }
            //if current tile is Joker
            else if (tempTiles[^1].tileColor == TileColor.Joker)
            {
                int compareIndex = 2;

                if (tempTiles[^2].tileColor == TileColor.Joker)
                    compareIndex = 3;

                CheckForRunsSequence(nextTile, compareIndex);
            }
            else
            {
                //check for runs
                CheckForRunsSequence(nextTile, 1);
            }
        }

        if (tempTiles.Count == totalTiles.Count)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool TilesInGroupSequence()
    {
        // Debug.Log("TilesInGroupSequence");
        
        if (tempTiles.Count == totalTiles.Count)
        {
            return true;
        }

        Tile nextTile = new();

        foreach (Tile tile in totalTiles)
        {
            if (tile.name == tempTiles[^1].name)
            {
                nextTile = totalTiles[totalTiles.IndexOf(tile) + 1];
                break;
            }
        }

        if (string.IsNullOrEmpty(sequence))
        {
            //check for next Joker Tile
            if (nextTile.tileColor == TileColor.Joker)
            {
                tempTiles.Add(nextTile);
                TilesInGroupSequence();
            }
            //if current tile is Joker
            else if (tempTiles[^1].tileColor == TileColor.Joker)
            {
                if ((tempTiles.Count == 1 && tempTiles[0].tileColor == TileColor.Joker) ||
                    (tempTiles.Count == 2 && tempTiles[0].tileColor == TileColor.Joker && tempTiles[1].tileColor == TileColor.Joker))
                {
                    if (tempTiles.Count == 1 && nextTile.tileNumber == 1)
                        sequence = "Group";

                    if (tempTiles.Count == 2 && (nextTile.tileNumber == 1 || nextTile.tileNumber == 2))
                        sequence = "Group";

                    tempTiles.Add(nextTile);
                    TilesInGroupSequence();
                }
                else if (tempTiles[^2].tileColor == TileColor.Joker)
                {
                    //check for Group
                    CheckForGroupSequence(nextTile, 3);
                }
                else
                {
                    //check for Group
                    CheckForGroupSequence(nextTile, 2);
                }
            }
            else
            {
                //check for Group
                CheckForGroupSequence(nextTile, 1);
            }
        }
        else if (sequence == "Group")
        {
            //check for next Joker Tile
            if (nextTile.tileColor == TileColor.Joker && tempTiles.Count < 4)
            {
                tempTiles.Add(nextTile);
                TilesInGroupSequence();
            }
            ////if current tile is Joker
            //else if (tempTiles[^1].tileColor == TileColor.Joker)
            //{
            //    int compareIndex = 2;

            //    for (int i = 0; i < totalTiles.Count; i++)
            //    {
            //        Debug.Log("Total Tile: " + totalTiles[i]);
            //    }

            //    for (int i = 0; i < tempTiles.Count; i++)
            //    {
            //        Debug.Log("Temp Tile: " + tempTiles[i]);
            //    }


            //    if (tempTiles.Count >= 2 && tempTiles[^2].tileColor == TileColor.Joker)
            //        compareIndex = 3;

            //    CheckForGroupSequence(nextTile, compareIndex);
            //}
            else
            {
                CheckForGroupSequence(nextTile, 1);
            }
        }

        return tempTiles.Count == totalTiles.Count;
    }

    private void CheckForRunsSequence(Tile nextTile, int compareIndex)
    {
        // Debug.Log("CheckForRunsSequence");
        
        if (nextTile.tileColor == tempTiles[^compareIndex].tileColor &&
            nextTile.tileNumber == (tempTiles[^compareIndex].tileNumber + compareIndex))
        {
            sequence = "Runs";
            tempTiles.Add(nextTile);
            TilesInRunsSequence();
        }
    }

    private void CheckForGroupSequence(Tile nextTile, int compareIndex)
    {
        // Debug.Log("CheckForGroupSequence");
        
        if (tempTiles.Count < 4)
        {
            bool allDifferentColors = true;
            foreach (Tile tile in tempTiles)
            {
                if (nextTile.tileColor == tile.tileColor)
                {
                    allDifferentColors = false;
                    break;
                }
            }

            if (allDifferentColors && nextTile.tileNumber == tempTiles[^compareIndex].tileNumber)
            {
                sequence = "Group";
                tempTiles.Add(nextTile);
                TilesInGroupSequence();
            }
        }
    }

    #endregion


    #region Message Box

    public void DisplayMessage(string message)
    {
        if (PhotonNetwork.NickName == playerTurn)
        {
            Debug.Log("Message: " + message);
            messageBox.SetActive(true);
            messageText.text = GameManager.Instance.Translate(message);
            Invoke(nameof(HideMessage), 3f);
        }
    }

    private void HideMessage()
    {
        messageBox.SetActive(false);
    }

    #endregion

    public void PlayAgain(int _numberOfPlayers)
    {
        StopAllCoroutines();
        deckCount = 106;
        tilePicked = false;
        currentTurn = -1;
        playerTurn = null;
        winner = null;

        if (coroutine != null)
            StopCoroutine(coroutine);

        timerRunning = false;

        foreach (Tile tile in tiles)
        {
            tile.boardTile = false;
            tile.transform.SetParent(deckParent);
            tile.transform.position = deckParent.position;
            tile.tileCoverImage.gameObject.SetActive(true);
            tile.gameObject.SetActive(false);
        }

        playersDeck.Clear();
        deck.Clear();

        numberOfPlayers = _numberOfPlayers;
        playersDeck = new List<PlayerDeck>(numberOfPlayers);
        for (int i = 0; i < numberOfPlayers; i++)
        {
            PlayerDeck playerDeck = new();
            playerDeck.playerName = "Player " + (i + 1);
            playerDeck.playerTiles = new();
            playersDeck.Add(playerDeck);
        }

        runsSequenceButton.interactable = false;
        groupSequenceButton.interactable = false;
        DisplayButton(ButtonType.PickTileButton);
        HideMessage();

        GridManager.Instance.ResetGridManager();
        GameManager.Instance.ResetGameManager();

        if (PhotonNetwork.NickName == "Player 1")
        {
            photonView.RPC(nameof(RPC_InstantiateTiles), RpcTarget.AllBuffered);
        }
    }

    public void ResetAll()
    {
        StopAllCoroutines();
        deckCount = 106;
        tilePicked = false;
        numberOfPlayers = 0;
        currentTurn = -1;
        playerTurn = null;
        winner = null;

        if(coroutine != null)
            StopCoroutine(coroutine);

        timerRunning = false;

        foreach (Tile tile in tiles)
        {
            tile.boardTile = false;
            tile.transform.SetParent(deckParent);
            tile.transform.position = deckParent.position;
            tile.tileCoverImage.gameObject.SetActive(true);
            tile.gameObject.SetActive(false);
        }

        ChatMessage[] chatMessages = chatContent.GetComponentsInChildren<ChatMessage>();
        for (int i = 0; i < chatMessages.Length; i++)
        {
            Destroy(chatMessages[i].gameObject);
        }

        playersDeck.Clear();
        deck.Clear();

        GameSetup.Instance.ResetGameSetup();
        GridManager.Instance.ResetGridManager();
        GameManager.Instance.ResetGameManager();

    }
}

