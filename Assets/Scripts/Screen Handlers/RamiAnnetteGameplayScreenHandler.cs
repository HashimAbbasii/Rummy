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
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class RamiAnnetteGameplayScreenHandler : GamePlayScreenHandler
{
    public WinnerTileHolder winnerTileHolder;
    
    public List<TileSequence> tileSequences = new();

    public List<Tile> winnersDeck = new();
    
    public bool isFirstTurn;
    
    private static int GetUnixTime()
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

        var pp = FindObjectsOfType<PhotonPlayer>();

        Debug.Log("Amount of players in Room right now = " + pp.Length);

        foreach (var np in GameSetup.Instance.players.Where(np => np.allSequencesHaveThreeOrMoreTiles))
        {
            np.allSequencesHaveThreeOrMoreTiles = false;
        }
        
        // foreach (var p in GameSetup.Instance.players.Where(p => p.player.IsMine))
        // {
        //     p.readyToPlay = true;
        // }
    }

    private void OnGetPharasesComplete(Pharases pharases, UnityWebRequest request)
    {
        if (request.result != UnityWebRequest.Result.Success) return;

        // Debug.Log(gameObject);
        // Debug.Log(this);
        
        if (!chatContent)
        {
            Debug.Log(chatContent);
            return;
        }

        chatContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, (chatMessage.GetComponent<RectTransform>().rect.height + 5) * pharases.data.Length);

        foreach (var str in pharases.data)
        {
            var tempChatMessage = Instantiate(chatMessage, chatContent.transform, true);
            tempChatMessage.GetComponent<ChatMessage>().messageText.text = str;
        }
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length == numberOfPlayers && PhotonNetwork.CurrentRoom.IsOpen)
        {
            if (PhotonNetwork.PlayerList.Length == FindObjectsOfType<PhotonPlayer>().Length)
            {
                Debug.Log("Only once");
                var insTiles = true;

                foreach (var p in GameSetup.Instance.players.Where(p => p.player != null).Where(p => !p.readyToPlay))
                {
                    insTiles = false;
                }

                if (insTiles)
                {
                    photonView.RPC(nameof(RPC_InstantiateTiles), RpcTarget.AllBuffered);
                }
            }
        }

        //All players left the game and only one player is present in room
        if (PhotonNetwork.CurrentRoom != null)
        {
            if (!PhotonNetwork.CurrentRoom.IsOpen && PhotonNetwork.PlayerList.Length == 1 &&
                !resultPanel.activeInHierarchy)
            {
                // photonView.RPC(nameof(RPC_DisplayResult), RpcTarget.All, false);

                // resultPanel.SetActive(true);
                // resultPanel.GetComponent<GameResultPanelHandler>().winner.SetActive(true);
                // resultPanel.GetComponent<GameResultPanelHandler>().loser.SetActive(false);

                photonView.RPC(nameof(RPC_DisplayResultPanel), RpcTarget.AllBuffered, false);
                ResetAll();
            }
        }

        deckText.text = deckCount.ToString();


        if (timerRunning)
        {
            if (timer != GetUnixTime())
            {
                timer = GetUnixTime();
                GameSetup.Instance.players[currentTurn].playerIcon.timerImage.fillAmount +=
                    1f / GameManager.Instance.room.standByTime;
            }

            if (GameSetup.Instance.myNetworkPlayer == GameSetup.Instance.players[currentTurn].player)
            {
                nextButton.interactable = !(GameSetup.Instance.players[currentTurn].playerIcon.timerImage.fillAmount > 0.95f);
            }

            //timer completed
            if (initialTime + GameManager.Instance.room.standByTime == GetUnixTime())
            {
                timerRunning = false;

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
    
    
    #region Tile Creation and Distribution

    [PunRPC]
    public void RPC_InstantiateTiles()
    {
        isFirstTurn = true;
        
        winnersDeck.Clear();
        
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

        foreach (var t in tiles)
        {
            t.GetComponent<BoxCollider2D>().enabled = false;
            t.transform.localScale = new Vector3(1, 1, 1);
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

        photonView.RPC(nameof(RPC_ShuffleDeckFirstTime), RpcTarget.AllBuffered, (object) randomArray.ToArray());
    }

    [PunRPC]
    private void RPC_ShuffleDeckFirstTime(int[] _randomNumbers)
    {
        var i = 0;
        deck = tiles.OrderBy(x => _randomNumbers[i++]).ToList();

        foreach (Tile tile in deck)
            tile.transform.SetAsFirstSibling();

        deckCount = deck.Count;
        DistributeTiles();
    }

    //In RPC
    private void DistributeTiles()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            for (int j = 0; j < amountOfTilesInHand; j++)
            {
                playersDeck[i].playerTiles.Add(deck[^1]);
                deck.RemoveAt(deck.Count - 1);
            }
        }

        var mT = deck[^1];
        StackManager.Instance.masterTile = mT;
        mT.gameObject.SetActive(true);
        mT.GetComponent<BoxCollider2D>().enabled = true;
        StackManager.Instance.masterTileTransform.GetComponent<GridElement>().Fill(mT);
        
        deck.RemoveAt(deck.Count - 1);
        StartCoroutine(DistributeTilesCoroutine());
    }

    //In RPC
    private IEnumerator DistributeTilesCoroutine()
    {
        yield return new WaitForSeconds(1f);

        AudioManager.Instance.PlayAudio(AudioName.FirstTileDistributeAudio);

        StackManager.Instance.masterTile.tileCoverImage.gameObject.SetActive(false);
        StackManager.Instance.masterTile.transform.LeanMove(StackManager.Instance.masterTileTransform.position, 0.5f);
        StackManager.Instance.masterTile.isMasterTile = true;

        // For others
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

        var playerNumber = PlayerNumber(PhotonNetwork.NickName);

        // For me
        foreach (Tile tile in playersDeck[playerNumber - 1].playerTiles)
        {
            AudioManager.Instance.PlayAudio(AudioName.DistributeTileAudio);
            tile.transform.LeanMove(StackManager.Instance.playerGrid[playersDeck[playerNumber - 1].playerTiles.IndexOf(tile)].transform.position, 0.5f);
            StackManager.Instance.playerGrid[playersDeck[playerNumber - 1].playerTiles.IndexOf(tile)].Fill(tile);
            tile.GetComponent<BoxCollider2D>().enabled = true;
            yield return new WaitForSeconds(0.2f);
        }

        runsSequenceButton.interactable = true;
        groupSequenceButton.interactable = true;

        if (!PhotonNetwork.IsMasterClient) yield break;
        
        playerTurn = "Player 1";
        GiveTurn();
    }

    private IEnumerator HideTileCoverImageCoroutine(Tile tile)
    {
        yield return new WaitForSeconds(0.6f);
        tile.tileCoverImage.gameObject.SetActive(false);
    }
    
    
    #endregion

    
    #region Start Turn
    
    // When Turn End's, Called on Master Client only
    public void ValidateBeforeNextTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_NextPlayerTurn), RpcTarget.AllBuffered);
        }
    }
    
    //In RPC
    private void GiveTurn()
    {
        if (IsGameCompleted())
        {
            photonView.RPC(nameof(RPC_DisplayResultPanel), RpcTarget.AllBuffered, true);
            
            Debug.LogError("Game Over");
            return;
        }

        StartCoroutine(StartTimerCoroutine());
    }


    public IEnumerator StartTimerCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        
        currentTurn = (currentTurn + 1) % numberOfPlayers;
        
        photonView.RPC(nameof(RPC_StartTimer), RpcTarget.AllBuffered, currentTurn);
    }
    
    [PunRPC]
    private void RPC_StartTimer(int _currentTurn)
    {
        
        tilePicked = false;
        currentTurn = _currentTurn;
        playerTurn = "Player " + (currentTurn + 1);

        timer = GetUnixTime();
        initialTime = GetUnixTime();

        timerRunning = true;

        if (isFirstTurn)
        {
            isFirstTurn = false;
            OnPickTileButtonClick();
        }
        
        if (GameSetup.Instance.myNetworkPlayer != GameSetup.Instance.players[currentTurn].player) return;
        
        tileSequences.Clear();
        CheckSequences();
        CheckForWin();
        foreach (var ge in StackManager.Instance.playerGrid.Where(ge => ge.tile == null))
        {
            ge.Clear();
        }
    }
    
    
    #endregion
    
    private void MoveBackDraggedTiles()
    {
        var tile = GameManager.Instance.dragTiles[0].tile;
        GameManager.Instance.dragTiles.RemoveAt(0);

        tile.transform.LeanMove(tile.pickUpPosition, 0.2f);

    }
    
    
    // ReSharper disable once ConvertIfStatementToSwitchStatement
    [PunRPC]
    private void RPC_NextPlayerTurn()
    {
        if (PhotonNetwork.NickName == playerTurn)
        {
            StartCoroutine(CoroutineDisablePickTileButton());
        }

        var playerNumber = PlayerNumber(playerTurn);
        var player = GameSetup.Instance.players[playerNumber - 1];
        
        timerRunning = false;
        player.playerIcon.timerImage.fillAmount = 0;
        
        
        if (!player.ramiAnnetteTurn.placedATile && player.ramiAnnetteTurn.tookATile)
        {
            // Debug.Log("!place & took");
            StartCoroutine(CheckGameCompletion(player, GameStatus.Took));
        }
        else if (player.ramiAnnetteTurn.placedATile && !player.ramiAnnetteTurn.tookATile)
        {
            // Debug.Log("place & !took");
            // TileFromDeckToGrid();
            // StartCoroutine(FinishTurn(player, 0.2f));
            
            StartCoroutine(CheckGameCompletion(player, GameStatus.Place));
        }
        else if (player.ramiAnnetteTurn.placedATile && player.ramiAnnetteTurn.tookATile)
        {
            // Debug.Log("place & took");
            // StartCoroutine(FinishTurn(player, 0.2f));
            
            StartCoroutine(CheckGameCompletion(player, GameStatus.TookAndPlace));
        }
        else if (!player.ramiAnnetteTurn.placedATile && !player.ramiAnnetteTurn.tookATile)
        {
            // Debug.Log("!place & !took");
            // TileFromDeckToStack();
            // StartCoroutine(FinishTurn(player, 0.2f));
            
            StartCoroutine(CheckGameCompletion(player, GameStatus.None));
        }
        
    }

    private enum GameStatus
    {
        TookAndPlace,
        Took,
        Place,
        None
    }
    
    //In RPC
    private IEnumerator CheckGameCompletion(NetworkPlayer player, GameStatus state)
    {
        if (PhotonNetwork.NickName == playerTurn)
        {
            tileSequences.Clear();
            CheckSequences();
            CheckForWin();
        }
        
        yield return new WaitForSeconds(0.1f);

        if (IsGameCompleted())
        {
            StartCoroutine(FinishTurn(player, 0.2f));
        }
        else
        {
            switch (state)
            {
                case GameStatus.TookAndPlace:
                    StartCoroutine(FinishTurn(player, 0.2f));
                    break;
                case GameStatus.Took:
                    TileFromGridToStack(player);
                    StartCoroutine(FinishTurn(player, 0.2f));
                    break;
                case GameStatus.Place:
                    TileFromDeckToGrid();
                    StartCoroutine(FinishTurn(player, 0.2f));
                    break;
                case GameStatus.None:
                    TileFromDeckToStack();
                    StartCoroutine(FinishTurn(player, 0.2f));
                    break;
            }
        }
    }

    private IEnumerator CoroutineDisablePickTileButton()
    {
        nextButton.interactable = false;
        GameSetup.Instance.players[currentTurn].playerIcon.timerImage.fillAmount = 1f;
        yield return new WaitForSeconds(1f);
        nextButton.interactable = true;
    }

    [PunRPC]
    private IEnumerator FinishTurn(NetworkPlayer player, float wait)
    {
        if (PhotonNetwork.NickName == playerTurn)
        {
            tileSequences.Clear();
            CheckSequences();
            CheckForWin();
        }
        
        yield return new WaitForSeconds(wait);
        
        player.lastTileInteractedWith = null;
        player.ramiAnnetteTurn.ResetTurn();

        yield return new WaitForSeconds(0.3f);

        if (!PhotonNetwork.IsMasterClient) yield break;
        ShuffleDeckAgain();
        GiveTurn();
    }

    [PunRPC]
    private void TileFromGridToStack(NetworkPlayer player)
    {
        // Debug.Log("TileFromPlayerToStack");
        playersDeck[PlayerNumber(playerTurn) - 1].playerTiles.Remove(player.lastTileInteractedWith);
        player.playerIcon.tilesCount--;
        foreach (var gridElement in StackManager.Instance.playerGrid.Where(ge => ge.tile == player.lastTileInteractedWith))
        {
            gridElement.Clear();
        }
        StackManager.Instance.AddTile(player.lastTileInteractedWith);
        player.lastTileInteractedWith.tileCoverImage.gameObject.SetActive(false);
        GameSetup.Instance.players[PlayerNumber(playerTurn) - 1].ramiAnnetteTurn.placedATile = true;
    }
    
    private void TileFromDeckToStack()
    {
        // Debug.Log("TileFromDeckToStack");
        var tile = deck[^1];
        tile.GetComponent<BoxCollider2D>().enabled = true;
        deck.Remove(tile);
        deckCount--;
        deckText.text = deckCount.ToString();
        tile.gameObject.SetActive(true);
        StackManager.Instance.AddTile(tile);
        tile.tileCoverImage.gameObject.SetActive(false);
        GameSetup.Instance.players[PlayerNumber(playerTurn) - 1].ramiAnnetteTurn.tookATile = true;
        GameSetup.Instance.players[PlayerNumber(playerTurn) - 1].ramiAnnetteTurn.placedATile = true;
    }
    
    [PunRPC]
    private void TileFromDeckToGrid()
    {
        // Add Tile from Deck to player (script)
        var playerNumber = PlayerNumber(playerTurn);
        var player = GameSetup.Instance.players[playerNumber - 1];
        var tile = deck[^1]; 
        playersDeck[playerNumber - 1].playerTiles.Add(tile);
        deck.RemoveAt(deck.Count - 1);
        deckCount--;
        deckText.text = deckCount.ToString();
        playersDeck[playerNumber - 1].playerTiles[^1].gameObject.SetActive(true);
        player.playerIcon.tilesCount++;
        player.ramiAnnetteTurn.tookATile = true;
        tilePicked = true;
        player.lastTileInteractedWith = tile;
        tile.GetComponent<BoxCollider2D>().enabled = true;
        
        // Add Tile from Deck to Player (graphics) [for me]
        if (PhotonNetwork.NickName == playerTurn)
        {
            foreach (var gridElement in StackManager.Instance.playerGrid.Where(gridElement => gridElement.tileStatus == TileStatus.Vacant))
            {
                playersDeck[playerNumber - 1].playerTiles[^1].transform.LeanTransform(gridElement.transform, 0.5f);
                gridElement.Fill(playersDeck[playerNumber - 1].playerTiles[^1]);
                StartCoroutine(HideTileCoverImageCoroutine(playersDeck[playerNumber - 1].playerTiles[^1]));
                tileSequences.Clear();
                CheckSequences();
                CheckForWin();
                break;
            }
        }
        // Add Tile from Deck to player (graphics) [for others]
        else
        {
            playersDeck[playerNumber - 1].playerTiles[^1].transform.LeanTransform(GameSetup.Instance.players[playerNumber - 1].playerIcon.transform, 0.5f);
            StartCoroutine(HideTileCoverImageCoroutine(playersDeck[playerNumber - 1].playerTiles[^1]));
        }
        
    }
    
    //In RPC
    private bool IsGameCompleted()
    {
        var playerNumber = PlayerNumber(playerTurn);
        var player = GameSetup.Instance.players[playerNumber - 1];

        Debug.Log("playersDeck[playerNumber - 1].playerTiles.Count == 14 is " + (playersDeck[playerNumber - 1].playerTiles.Count == 14));
        Debug.Log("player.allSequencesHaveThreeOrMoreTiles = " + player.allSequencesHaveThreeOrMoreTiles);
        
        return playersDeck[playerNumber - 1].playerTiles.Count == 14 && player.allSequencesHaveThreeOrMoreTiles;
    }

    
    private bool CheckForWin()
    {
        var playerNumber = PlayerNumber(playerTurn);
        var playerTiles = playersDeck[playerNumber - 1].playerTiles;
        
        if (playerTiles.Count != 14) return false;
        
        if (tileSequences.Count == 0) return false;
        
        var allSequencesHaveThreeOrMoreTiles = UIManager.Instance.ramiAnnetteGameplayScreenHandler.tileSequences.All(t => t.tileSet.Count >= 3);

        photonView.RPC(nameof(RPC_CheckForWin), RpcTarget.All, allSequencesHaveThreeOrMoreTiles);
        
        return allSequencesHaveThreeOrMoreTiles;
    }
    
    [PunRPC]
    private void RPC_CheckForWin(bool seq)
    {
        var raGSH = UIManager.Instance.ramiAnnetteGameplayScreenHandler;
        var playerNumber = raGSH.PlayerNumber(raGSH.playerTurn);

        var player = GameSetup.Instance.players[playerNumber - 1];

        player.allSequencesHaveThreeOrMoreTiles = seq;
    }
    
   //  public void CheckSequences()
   //  {
   //      int playerNumber = PlayerNumber(playerTurn);
   //      var player = GameSetup.Instance.players[playerNumber - 1];
   //
   //      var playerTiles = playersDeck[playerNumber - 1].playerTiles;
   //
   //      List<Tile> seqTiles = new List<Tile>();
   //      SequenceType sequenceType = SequenceType.None;
   //
   //      for (int i = 0; i < playerTiles.Count; i++)
   //      {
   //          seqTiles.Add(playerTiles[i]);
   //
   //          //Last tile
   //          if (i + 1 > playerTiles.Count - 1)
   //          {
   //              var temp = new TileSequence
   //              {
   //                  tileSet = new List<Tile>(seqTiles)
   //              };
   //              tileSequences.Add(temp);
   //              seqTiles.Clear();
   //              break;
   //          }
			//
			//
			// //Next Tile is a Joker
			// if (playerTiles[i + 1].tileColor == TileColor.Joker)
   //          {
   //              switch (sequenceType)
   //              {
   //                  case SequenceType.None:
   //                      continue;
   //                      break;
   //                  
   //                  case SequenceType.Runs:
   //                      continue;
   //                      break;
   //                  
   //                  case SequenceType.Groups:
   //                      if (seqTiles.Count < 4) continue;
   //                      break;
   //              }
   //          }
			//
			//
			// //Check for Runs Sequence
   //          if (playerTiles[i].tileColor == playerTiles[i + 1].tileColor)
   //          {
   //              if (playerTiles[i].tileNumber % 13 == (playerTiles[i + 1].tileNumber - 1) % 13)
   //              {
   //                  sequenceType = SequenceType.Runs;
   //                  continue;
   //              }
   //          }
   //
   //
   //          //Check for Group Sequence
   //          if (playerTiles[i].tileNumber == playerTiles[i + 1].tileNumber)
   //          {
   //              if (seqTiles.Count < 4)
   //              {
   //                  bool allDifferentColors = seqTiles.All(t => playerTiles[i + 1].tileColor != t.tileColor);
   //                  
   //                  if (allDifferentColors)
   //                  {
   //                      sequenceType = SequenceType.Groups;
   //                      continue;
   //                  }
   //              }
   //          }
			//
   //
   //          //Current Tile is joker
   //          if (playerTiles[i].tileColor == TileColor.Joker)
   //          {
   //              switch (sequenceType)
   //              {
   //                  //Joker already part of a Runs Sequence
   //                  case SequenceType.Runs:
   //                      if (playerTiles[i - 1].tileColor == playerTiles[i + 1].tileColor)
   //                      {
   //                          if ((playerTiles[i - 1].tileNumber + 2) % 13 == playerTiles[i + 1].tileNumber % 13)
   //                          {
   //                              continue;
   //                          }
   //                      }
   //
   //                      break;
   //
   //                  //Joker already part of a Group Sequence
   //                  case SequenceType.Groups:
   //                      if (playerTiles[i - 1].tileNumber == playerTiles[i + 1].tileNumber)
   //                      {
   //                          if (seqTiles.Count < 4)
   //                          {
   //                              bool allDifferentColors = seqTiles.All(t => playerTiles[i + 1].tileColor != t.tileColor || playerTiles[i + 1].tileColor == TileColor.Joker);
   //
   //                              if (allDifferentColors)
   //                              {
   //                                  continue;
   //                              }
   //                          }
   //                      }
   //
   //                      break;
			// 		
   //                  //Joker is not part of any sequence
   //                  case SequenceType.None:
   //
   //                      // The first tile
   //                      if (i == 0)
   //                      {
   //                          //Check if the next tile and next next tile in runs order
   //                          if (playerTiles[i + 1].tileColor == playerTiles[i + 2].tileColor)
   //                          {
   //                              if ((playerTiles[i + 1].tileNumber + 1) % 13 == playerTiles[i + 2].tileNumber % 13)
   //                              {
   //                                  sequenceType = SequenceType.Runs;
   //                              }
   //                          }
   //
   //                          //Check if the next tile and next next tile in groups order
   //                          if (playerTiles[i + 1].tileNumber == playerTiles[i + 2].tileNumber)
   //                          {
   //                              if (playerTiles[i + 1].tileColor != playerTiles[i + 2].tileColor)
   //                              {
   //                                  sequenceType = SequenceType.Groups;
   //                              }
   //                          }
   //                          
   //                          continue;
   //                      }
   //                      
   //                      // Not the first tile
   //                      
   //                      //Check if the next tile and previous tile in runs order
   //                      if (playerTiles[i - 1].tileColor == playerTiles[i + 1].tileColor)
   //                      {
   //                          if ((playerTiles[i - 1].tileNumber + 2) % 13 == playerTiles[i + 1].tileNumber % 13)
   //                          {
   //                              sequenceType = SequenceType.Runs;
   //                              continue;
   //                          }
   //                      }
   //
   //                      //Check if the next tile and previous tile in groups order
   //                      if (playerTiles[i - 1].tileNumber == playerTiles[i + 1].tileNumber)
   //                      {
   //                          if (playerTiles[i - 1].tileColor != playerTiles[i + 1].tileColor)
   //                          {
   //                              sequenceType = SequenceType.Groups;
   //                              continue;
   //                          }
   //                      }
   //
   //                      if (i == 1 && playerTiles[i - 1].tileColor == TileColor.Joker)
   //                      {
   //                          continue;
   //                      }
   //                      
   //                      // Not the second tile
   //                      if (i != 1)
   //                      {
   //                          if (playerTiles[i - 2].tileColor == playerTiles[i + 1].tileColor)
   //                          {
   //                              if ((playerTiles[i - 2].tileNumber + 3) % 13 == playerTiles[i + 1].tileNumber % 13)
   //                              {
   //                                  sequenceType = SequenceType.Runs;
   //                                  continue;
   //                              }
   //                          }
   //                              
   //                          if (playerTiles[i - 2].tileNumber == playerTiles[i + 1].tileNumber)
   //                          {
   //                              if (playerTiles[i - 2].tileColor != playerTiles[i + 1].tileColor)
   //                              {
   //                                  sequenceType = SequenceType.Groups;
   //                                  continue;
   //                              }
   //                          }
   //                      }
   //                      break;
   //              }
   //          }
   //          
   //          //Make the current sequence in a sequence set and Start over a new Sequence
   //          var ts = new TileSequence
   //          {
   //              tileSet = new List<Tile>(seqTiles)
   //          };
   //          tileSequences.Add(ts);
   //          seqTiles.Clear();
   //          sequenceType = SequenceType.None;
   //      }
   //     
   //      
   //  }

    public void CheckSequences()
    {
        var playerNumber = PlayerNumber(playerTurn);
        var player = GameSetup.Instance.players[playerNumber - 1];

        // var playerTiles = playersDeck[playerNumber - 1].playerTiles;

        var grid = StackManager.Instance.playerGrid;
        
        var seqTiles = new List<Tile>();
        var sequenceType = SequenceType.None;

        for (var i = 0; i < grid.Count; i++)
        {
            if (grid[i].tileStatus == TileStatus.Vacant)
            {
                if (seqTiles.Count > 0)
                {
                    var temp = new TileSequence
                    {
                        tileSet = new List<Tile>(seqTiles)
                    };
                    tileSequences.Add(temp);
                    seqTiles.Clear();
                }
                continue;
            }
            
            seqTiles.Add(grid[i].tile);

            if (i + 1 <= grid.Count - 1)
            {
                if (grid[i + 1].tileStatus == TileStatus.Vacant)
                {
                    if (seqTiles.Count > 0)
                    {
                        var temp = new TileSequence
                        {
                            tileSet = new List<Tile>(seqTiles)
                        };
                        tileSequences.Add(temp);
                        seqTiles.Clear();
                    }

                    continue;
                }
            }
            
            //Last tile
            if (i + 1 > grid.Count - 1)
            {
                if (seqTiles.Count > 0)
                {
                    var temp = new TileSequence
                    {
                        tileSet = new List<Tile>(seqTiles)
                    };
                    tileSequences.Add(temp);
                    seqTiles.Clear();
                }
                break;
            }

            //Next Tile is a Joker
            if (i + 1 <= grid.Count - 1)
            {
                if (grid[i + 1].tile.tileColor == TileColor.Joker)
                {
                    switch (sequenceType)
                    {
                        case SequenceType.None:
                            continue;
                            break;

                        case SequenceType.Runs:
                            continue;
                            break;

                        case SequenceType.Groups:
                            if (seqTiles.Count < 4) continue;
                            break;
                    }
                }
            }

            //Check for Runs Sequence
            if (i + 1 <= grid.Count - 1)
            {
                if (grid[i].tile.tileColor == grid[i + 1].tile.tileColor)
                {
                    if (grid[i].tile.tileNumber % 13 == (grid[i + 1].tile.tileNumber - 1) % 13)
                    {
                        sequenceType = SequenceType.Runs;
                        continue;
                    }
                }
            }

            //Check for Group Sequence
            if (i + 1 <= grid.Count - 1)
            {
                if (grid[i].tile.tileNumber == grid[i + 1].tile.tileNumber)
                {
                    if (seqTiles.Count < 4)
                    {
                        bool allDifferentColors = seqTiles.All(t => grid[i + 1].tile.tileColor != t.tileColor);

                        if (allDifferentColors)
                        {
                            sequenceType = SequenceType.Groups;
                            continue;
                        }
                    }
                }
            }
            
            //Current Tile is joker
            if (grid[i].tile.tileColor == TileColor.Joker)
            {
                switch (sequenceType)
                {
                    //Joker already part of a Runs Sequence
                    case SequenceType.Runs:

                        if (i - 1 >= 0)
                        {
                            if (grid[i - 1].tileStatus == TileStatus.Vacant)
                            {
                                continue;
                            }
                        }

                        if (i + 1 <= grid.Count - 1 || i - 1 >= 0)
                        {
                            if (grid[i - 1].tile.tileColor == grid[i + 1].tile.tileColor)
                            {
                                if ((grid[i - 1].tile.tileNumber + 2) % 13 == grid[i + 1].tile.tileNumber % 13)
                                {
                                    continue;
                                }
                            }
                        }

                        break;

                    //Joker already part of a Group Sequence
                    case SequenceType.Groups:
                        if (i - 1 >= 0)
                        {
                            if (grid[i - 1].tileStatus == TileStatus.Vacant)
                            {
                                continue;
                            }
                        }

                        if (i + 1 <= grid.Count - 1 || i - 1 >= 0)
                        {
                            if (grid[i - 1].tile.tileNumber == grid[i + 1].tile.tileNumber)
                            {
                                if (seqTiles.Count < 4)
                                {
                                    bool allDifferentColors = seqTiles.All(t =>
                                        grid[i + 1].tile.tileColor != t.tileColor ||
                                        grid[i + 1].tile.tileColor == TileColor.Joker);

                                    if (allDifferentColors)
                                    {
                                        continue;
                                    }
                                }
                            }
                        }

                        break;
					
                    //Joker is not part of any sequence
                    case SequenceType.None:

                        // The first tile
                        if (i == 0)
                        {
                            if (i + 2 <= grid.Count - 1)
                            {
                                if (grid[i + 2].tileStatus == TileStatus.Vacant)
                                {
                                    continue;
                                }
                            }

                            if (i + 2 <= grid.Count - 1)
                            {
                                //Check if the next tile and next next tile in runs order
                                if (grid[i + 1].tile.tileColor == grid[i + 2].tile.tileColor)
                                {
                                    if ((grid[i + 1].tile.tileNumber + 1) % 13 == grid[i + 2].tile.tileNumber % 13)
                                    {
                                        sequenceType = SequenceType.Runs;
                                    }
                                }
                            }

                            if (i + 2 <= grid.Count - 1)
                            {
                                //Check if the next tile and next next tile in groups order
                                if (grid[i + 1].tile.tileNumber == grid[i + 2].tile.tileNumber)
                                {
                                    if (grid[i + 1].tile.tileColor != grid[i + 2].tile.tileColor)
                                    {
                                        sequenceType = SequenceType.Groups;
                                    }
                                }
                            }

                            continue;
                        }
                        
                        // Not the first tile

                        if (i - 1 >= 0)
                        {
                            if (grid[i - 1].tileStatus == TileStatus.Vacant)
                            {
                                continue;
                            }
                        }

                        if (i + 1 <= grid.Count - 1 || i - 1 >= 0)
                        {
                            //Check if the next tile and previous tile in runs order
                            if (grid[i - 1].tile.tileColor == grid[i + 1].tile.tileColor)
                            {
                                if ((grid[i - 1].tile.tileNumber + 2) % 13 == grid[i + 1].tile.tileNumber % 13)
                                {
                                    sequenceType = SequenceType.Runs;
                                    continue;
                                }
                            }
                        }

                        if (i + 1 <= grid.Count - 1 || i - 1 >= 0)
                        {
                            //Check if the next tile and previous tile in groups order
                            if (grid[i - 1].tile.tileNumber == grid[i + 1].tile.tileNumber)
                            {
                                if (grid[i - 1].tile.tileColor != grid[i + 1].tile.tileColor)
                                {
                                    sequenceType = SequenceType.Groups;
                                    continue;
                                }
                            }
                        }

                        if (i - 1 >= 0)
                        {
                            if (i == 1 && grid[i - 1].tile.tileColor == TileColor.Joker)
                            {
                                continue;
                            }
                        }
                        
                        // Not the second tile
                        if (i != 1)
                        {
                            if (i - 2 >= 0)
                            {
                                if (grid[i - 2].tileStatus == TileStatus.Vacant)
                                {
                                    continue;
                                }
                            }

                            if (i + 1 <= grid.Count - 1 || i - 2 >= 0)
                            {
                                if (grid[i - 2].tile.tileColor == grid[i + 1].tile.tileColor)
                                {
                                    if ((grid[i - 2].tile.tileNumber + 3) % 13 == grid[i + 1].tile.tileNumber % 13)
                                    {
                                        sequenceType = SequenceType.Runs;
                                        continue;
                                    }
                                }
                            }

                            if (i + 1 <= grid.Count - 1 || i - 2 >= 0)
                            {
                                if (grid[i - 2].tile.tileNumber == grid[i + 1].tile.tileNumber)
                                {
                                    if (grid[i - 2].tile.tileColor != grid[i + 1].tile.tileColor)
                                    {
                                        sequenceType = SequenceType.Groups;
                                        continue;
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            
            //Make the current sequence in a sequence set and Start over a new Sequence
            var ts = new TileSequence
            {
                tileSet = new List<Tile>(seqTiles)
            };
            tileSequences.Add(ts);
            seqTiles.Clear();
            sequenceType = SequenceType.None;
        }
       
        
    }

   
    
    #region Graphical UI
    
    [PunRPC]
    private void RPC_DisplayResultPanel(bool playerAvailable)
    {
        resultPanel.SetActive(true);
        
        resultPanel.GetComponent<GameResultPanelHandler>().playAgainPanel.SetActive(true);

        var winnerData = new PlayerResultData();
        
        if (playerAvailable)
        {
            var pT = PlayerNumber(playerTurn);
            var player = GameSetup.Instance.players[pT - 1];
            winner = player.playerName;

            winnersDeck = playersDeck[pT - 1].playerTiles;

            winnerTileHolder.gameObject.SetActive(true);
            
            for (var i = 0; i < winnersDeck.Count; i++)
            {
                winnersDeck[i].transform.SetParent(winnerTileHolder.winnerTiles[i].transform);
                winnersDeck[i].transform.localPosition = Vector3.zero;
                winnersDeck[i].transform.localScale = Vector3.one;
                winnersDeck[i].tileCoverImage.gameObject.SetActive(false);
            }

            winnerData = new PlayerResultData
            {
                playerName = winner,
                playerUserId = player.playerUserId,
                numberOfTilesLeft = playersDeck[pT - 1].playerTiles.Count
            };
        }
        else
        {
            playerTurn = GameSetup.Instance.players.Single(t => t.player == GameSetup.Instance.myNetworkPlayer).playerNickName;
            var pT = PlayerNumber(playerTurn);
            var player = GameSetup.Instance.players[pT - 1];
            winner = player.playerName;

            winnersDeck = playersDeck[pT - 1].playerTiles;

            winnerData = new PlayerResultData
            {
                playerName = winner,
                playerUserId = player.playerUserId,
                numberOfTilesLeft = playersDeck[pT - 1].playerTiles.Count
            };
        }

        List<PlayerResultData> result = new(playersDeck.Count) { winnerData };
        
        foreach (var playerDeck in playersDeck)
        {
            // if (PhotonNetwork.NickName != playerDeck.playerName) continue;
            
            if (GameSetup.Instance.players[playersDeck.IndexOf(playerDeck)].playerName == winner) continue;
            
            result.Add(new PlayerResultData()
            {
                playerName = GameSetup.Instance.players[playersDeck.IndexOf(playerDeck)].playerName,
                playerUserId = GameSetup.Instance.players[playersDeck.IndexOf(playerDeck)].playerUserId,
                numberOfTilesLeft = playerDeck.playerTiles.Count
            });
        }

        resultPanel.GetComponent<GameResultPanelHandler>().DisplayGameResultAnnette(winnerData, result, playerAvailable);
    }
    
    [PunRPC]
    private void RPC_DisplayResult(bool playerAvailable)
    {
        resultPanel.SetActive(true);
        List<PlayerResultData> result = new(playersDeck.Count);
        PlayerResultData winner = new();

        foreach (var playerDeck in playersDeck)
        {
            result.Add(new PlayerResultData()
            {
                playerName = GameSetup.Instance.players[playersDeck.IndexOf(playerDeck)].playerName,
                playerUserId = GameSetup.Instance.players[playersDeck.IndexOf(playerDeck)].playerUserId,
                numberOfTilesLeft = playerDeck.playerTiles.Count
            });

            if (PhotonNetwork.NickName != playerDeck.playerName) continue;
            
            winner = result[^1];
            Debug.Log("Winner Name: " + winner.playerName);
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

    [PunRPC]
    public void RPC_SendChatMessage(string senderName, string message)
    {
        GameSetup.Instance.players[PlayerNumber(senderName) - 1].playerIcon.DisplayMessage(message);
    }
    
    public void SendChatMessage(string message)
    {
        chatPanel.SetActive(false);
        photonView.RPC(nameof(RPC_SendChatMessage), RpcTarget.AllBuffered, PhotonNetwork.NickName, message);
    }

    
    #endregion
    

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
        var playerNumber = PlayerNumber(PhotonNetwork.NickName);

        foreach (var tile in playersDeck[playerNumber - 1].playerTiles)
        {
            tile.tileType = TileType.Primary;
        }

        foreach (var tile in playersDeck[playerNumber - 1].playerTiles)
        {
            foreach (var item in playersDeck[playerNumber - 1].playerTiles.
                         Where(item => tile.name != item.name && 
                                       tile.tileColor == item.tileColor && 
                                       tile.tileNumber == item.tileNumber && 
                                       tile.tileType == item.tileType))
            {
                tile.tileType = TileType.Secondary;
                break;
            }
        }

        playersDeck[playerNumber - 1].playerTiles = playersDeck[playerNumber - 1].playerTiles.OrderBy(tile => (tile.tileColor, tile.tileType, tile.tileNumber)).ToList();

        foreach (var gridElement in StackManager.Instance.playerGrid)
        {
            gridElement.Clear();
        }

        //foreach (Tile tile in playersDeck[playerNumber - 1].playerTiles)
        //{
        //    tile.transform.LeanMove(StackManager.Instance.playerGrid[playersDeck[playerNumber - 1].playerTiles.IndexOf(tile)].transform.position, 0.2f);
        //    StackManager.Instance.playerGrid[playersDeck[playerNumber - 1].playerTiles.IndexOf(tile)].Fill(tile);
        //}

        //Milestone Part: 2
        
        PlaceTiles(playerNumber, "Runs");

        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void OnGroupSequenceButtonClick()
    {
        var playerNumber = PlayerNumber(PhotonNetwork.NickName); ;

        foreach (var tile in playersDeck[playerNumber - 1].playerTiles)
        {
            tile.tileType = TileType.Primary;
        }

        foreach (var tile in playersDeck[playerNumber - 1].playerTiles)
        {
            foreach (var item in playersDeck[playerNumber - 1].playerTiles.
                         Where(item => tile.name != item.name && 
                                       tile.tileColor == item.tileColor && 
                                       tile.tileNumber == item.tileNumber && 
                                       tile.tileType == item.tileType))
            {
                tile.tileType = TileType.Secondary;
                break;
            }
        }

        playersDeck[playerNumber - 1].playerTiles = playersDeck[playerNumber - 1].playerTiles.OrderBy(tile => (tile.tileNumber, tile.tileType)).ToList();

        foreach (var gridElement in StackManager.Instance.playerGrid)
        {
            gridElement.Clear();
        }

        //foreach (Tile tile in playersDeck[playerNumber - 1].playerTiles)
        //{
        //    tile.transform.LeanMove(StackManager.Instance.playerGrid[playersDeck[playerNumber - 1].playerTiles.IndexOf(tile)].transform.position, 0.2f);
        //    StackManager.Instance.playerGrid[playersDeck[playerNumber - 1].playerTiles.IndexOf(tile)].Fill(tile);
        //}

        //Milestone Part: 2
        
        PlaceTiles(playerNumber, "Group");

        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void OnPickTileButtonClick()
    {
        if (playerTurn != PhotonNetwork.NickName || tilePicked) return;

        tilePicked = true;
        
        photonView.RPC(nameof(RPC_PlayerPickUpTileFromDeck), RpcTarget.AllBuffered);

        AudioManager.Instance.PlayAudio(AudioName.DrawTileAudio);
    }
    
    
    [PunRPC]
    public void RPC_PlayerPickUpTileFromDeck()
    {
        TileFromDeckToGrid();
    }
    
    public void OnNextButtonClick()
    {
        if (playerTurn == PhotonNetwork.NickName)
        {
            photonView.RPC(nameof(RPC_NextPlayerTurn), RpcTarget.AllBuffered);
        }
    }

    // [PunRPC]
    // private void RPC_Next(bool firstMoveIsValid, bool correctSequenceOnTable, bool playerMovedTile)
    // {
    //     Debug.Log("Player Moved Tile: " + playerMovedTile + " First Move Is Valid: " + firstMoveIsValid + " Correct Sequence On Table: " + correctSequenceOnTable);
    //
    //     if (!firstMoveIsValid || !correctSequenceOnTable || !playerMovedTile)
    //     {
    //         GameManager.Instance.tilesList.Clear();
    //         DisplayButton(ButtonType.PickTileButton);
    //         // RPC_Undo();
    //
    //         if (PhotonNetwork.NickName == playerTurn)
    //             AudioManager.Instance.PlayAudio(AudioName.WrongMoveAudio);
    //
    //         RPC_NextPlayerTurn(true);
    //     }
    //     else
    //     {
    //         playersDeck[PlayerNumber(playerTurn) - 1].firstMove = true;
    //         GameManager.Instance.movedTiles.Clear();
    //         GameManager.Instance.tilesList.Clear();
    //         DisplayButton(ButtonType.PickTileButton);
    //
    //         if (PhotonNetwork.NickName == playerTurn)
    //             AudioManager.Instance.PlayAudio(AudioName.MadeSetAudio);
    //
    //         RPC_NextPlayerTurn(false);
    //     }
    // }

    
    #region Undo

    public void OnUndoButtonClick()
    {
        photonView.RPC(nameof(RPC_Undo), RpcTarget.AllBuffered);
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    [PunRPC]
    private void RPC_Undo()
    {
        var playerNumber = PlayerNumber(playerTurn);

        foreach (MovedTile movedTile in GameManager.Instance.movedTiles)
        {
            //for local player
            if (playerTurn == PhotonNetwork.NickName)
            {
                if (!movedTile.sourceGridElement.playerGrid) continue;
                
                playersDeck[playerNumber - 1].playerTiles.Add(movedTile.tile);
                GameSetup.Instance.players[playerNumber - 1].playerIcon.tilesCount++;
            }
            //for network player
            else
            {
                if (movedTile.sourceGridElement != null) continue;
                
                playersDeck[playerNumber - 1].playerTiles.Add(movedTile.tile);
                GameSetup.Instance.players[playerNumber - 1].playerIcon.tilesCount++;
            }
        }

        GameManager.Instance.Undo(playerTurn, playerNumber);
        DisplayButton(ButtonType.PickTileButton);
    }

    
    #endregion
    
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
        StartCoroutine(LeaveGameCoroutine());
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    private IEnumerator LeaveGameCoroutine()
    {
        // foreach (var p in GameSetup.Instance.players.Where(p => p.player.IsMine))
        // {
        //     p.readyToPlay = false;
        // }

        if (PhotonNetwork.CurrentRoom == null)
        {
            if (PhotonNetwork.PlayerList.Length == 1 && string.IsNullOrEmpty(winner))
            {
                DatabaseManager.Instance.SendGameResult(new GameResult() { id = GameManager.Instance.room.roomCode, status = "cancel" }, OnSendResultComplete);
            }
            ResetAll();
            StartCoroutine(NetworkManager.Instance.LeaveRoom());
        }
        else
        {
            PhotonNetwork.LeaveRoom();
            yield return new WaitWhile(() => PhotonNetwork.InRoom);

            if (PhotonNetwork.PlayerList.Length == 1 && string.IsNullOrEmpty(winner))
            {
                DatabaseManager.Instance.SendGameResult(new GameResult() { id = GameManager.Instance.room.roomCode, status = "cancel" }, OnSendResultComplete);
            }

            ResetAll();

            StartCoroutine(NetworkManager.Instance.LeaveRoom());
        }
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
        
        var sequenceCount = 3;
        var sequencePlaced = 0;

        sequence = null;
        tempTiles.Clear();
        totalTiles.Clear();

         for (var i = 0; i < playersDeck[playerNumber - 1].playerTiles.Count - 2; i++)
        {
            sequenceCount = 3;

            for (var j = 0; j < sequenceCount; j++)
            {
                totalTiles.Add(playersDeck[playerNumber - 1].playerTiles[i + j]);
            }

            if (totalTiles.Count >= 3)
            {
                tempTiles.Clear(); tempTiles.Add(totalTiles[0]);

                switch (_sequence)
                {
                    case "Runs":
                    {
                        if (TilesInRunsSequence())
                        {
                            var flag = true;

                            while (flag)
                            {
                                if (TilesInRunsSequence() && (sequenceCount + 1 + i) <= playersDeck[playerNumber - 1].playerTiles.Count)
                                {
                                    totalTiles.Add(playersDeck[playerNumber - 1].playerTiles[sequenceCount++ + i]);
                                    tempTiles.Clear(); tempTiles.Add(totalTiles[0]);
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

                            for (var k = 0; k < sequenceCount - 1; k++)
                            {
                                var tile = playersDeck[playerNumber - 1].playerTiles[i + k];

                                tile.transform.LeanMove(StackManager.Instance.playerGrid[sequencePlaced].transform.position, 0.2f);
                                StackManager.Instance.playerGrid[sequencePlaced].Fill(tile);
                                sequencePlaced++;
                            }

                            i += sequenceCount - 2;
                            sequencePlaced++;
                        }

                        break;
                    }
                    case "Group":
                    {
                        if (TilesInGroupSequence())
                        {
                            var flag = true;

                            while (flag)
                            {
                                if (TilesInGroupSequence() && (sequenceCount + 1 + i) <= playersDeck[playerNumber - 1].playerTiles.Count)
                                {
                                    totalTiles.Add(playersDeck[playerNumber - 1].playerTiles[sequenceCount++ + i]);
                                    tempTiles.Clear(); tempTiles.Add(totalTiles[0]);
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

                            for (var k = 0; k < sequenceCount - 1; k++)
                            {
                                var tile = playersDeck[playerNumber - 1].playerTiles[i + k];

                                tile.transform.LeanMove(StackManager.Instance.playerGrid[sequencePlaced].transform.position, 0.2f);
                                StackManager.Instance.playerGrid[sequencePlaced].Fill(tile);
                                sequencePlaced++;
                            }

                            i += sequenceCount - 2;
                            sequencePlaced++;
                        }

                        break;
                    }
                }
            }

            sequence = null;
            tempTiles.Clear();
            totalTiles.Clear();
        }

        foreach (Tile tile in playersDeck[playerNumber - 1].playerTiles)
        {
            var tileAlreadyPlaced = StackManager.Instance.playerGrid.Any(gridElement => gridElement.tile == tile);

            if (tileAlreadyPlaced) continue;
            
            tile.transform.LeanMove(StackManager.Instance.playerGrid[sequencePlaced].transform.position, 0.2f);
            StackManager.Instance.playerGrid[sequencePlaced].Fill(tile);
            sequencePlaced++;
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

        foreach (var tile in totalTiles.Where(tile => tile.name == tempTiles[^1].name))
        {
            nextTile = totalTiles[totalTiles.IndexOf(tile) + 1];
            break;
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
                //check for Runs
                //check for Runs
                CheckForRunsSequence(nextTile, tempTiles[^2].tileColor == TileColor.Joker ? 3 : 2);
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
                var compareIndex = 2;

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

        return tempTiles.Count == totalTiles.Count;
    }

    public bool TilesInGroupSequence()
    {
        Debug.Log("TilesInGroupSequence");
        
        if (tempTiles.Count == totalTiles.Count)
        {
            return true;
        }

        Tile nextTile = new();

        foreach (var tile in totalTiles.Where(tile => tile.name == tempTiles[^1].name))
        {
            nextTile = totalTiles[totalTiles.IndexOf(tile) + 1];
            break;
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

        if (nextTile.tileColor != tempTiles[^compareIndex].tileColor 
        || nextTile.tileNumber != (tempTiles[^compareIndex].tileNumber + compareIndex)) return;
        
        sequence = "Runs";
        tempTiles.Add(nextTile);
        TilesInRunsSequence();
    }

    private void CheckForGroupSequence(Tile nextTile, int compareIndex)
    {
        // Debug.Log("CheckForGroupSequence");

        if (tempTiles.Count >= 4) return;
        
        var allDifferentColors = tempTiles.All(tile => nextTile.tileColor != tile.tileColor);

        if (!allDifferentColors || nextTile.tileNumber != tempTiles[^compareIndex].tileNumber) return;
        
        sequence = "Group";
        tempTiles.Add(nextTile);
        TilesInGroupSequence();
    }

    
    #endregion


    #region Message Box

    public void DisplayMessage(string message)
    {
        if (PhotonNetwork.NickName != playerTurn) return;
        
        Debug.Log("Message: " + message);
        messageBox.SetActive(true);
        messageText.text = GameManager.Instance.Translate(message);
        Invoke(nameof(HideMessage), 3f);
    }

    private void HideMessage()
    {
        messageBox.SetActive(false);
    }

    #endregion

    
    private List<Tile> _stackWithoutTop = new List<Tile>();

    private void ShuffleDeckAgain()
    {
        if (deck.Count != 0) return;
        
        var rand = new System.Random();
        List<int> randomArray = new();

        _stackWithoutTop = StackManager.Instance.stack.tileStack;
        _stackWithoutTop.RemoveAt(_stackWithoutTop.Count - 1);
        
        for (int i = 0; i < _stackWithoutTop.Count; i++)
            randomArray.Add(rand.Next());

        photonView.RPC(nameof(RPC_ShuffleDeckAgain), RpcTarget.AllBuffered, (object)randomArray.ToArray());
    }
    
    [PunRPC]
    public void RPC_ShuffleDeckAgain(int[] _randomNumbers)
    {
        _stackWithoutTop = StackManager.Instance.stack.tileStack;
        _stackWithoutTop.RemoveAt(_stackWithoutTop.Count - 1);

        var i = 0;
        deck = _stackWithoutTop.OrderBy(x => _randomNumbers[i++]).ToList();

        foreach (var tile in deck)
            tile.transform.SetAsFirstSibling();

        deckCount = deck.Count;

        foreach (var tile in deck)
        {
            tile.tileCoverImage.gameObject.SetActive(true);
            tile.transform.LeanTransform(deckParent, 0.2f);
        }
        
        var lastTile = StackManager.Instance.stack.tileStack[^1];
        StackManager.Instance.stack.tileStack.Clear();
        StackManager.Instance.stack.tileStack.Add(lastTile);
    }
    
    public void PlayAgain(int _numberOfPlayers)
    {

        foreach (var player in GameSetup.Instance.players)
        {
            player.playerIcon.tilesCount = 0;
            player.playerIcon.timerImage.fillAmount = 0f;
        }

        StopAllCoroutines();
        deckCount = 106;
        tilePicked = false;
        currentTurn = -1;
        playerTurn = null;
        winner = null;

        if (coroutine != null)
            StopCoroutine(coroutine);

        timerRunning = false;

        foreach (var tile in tiles)
        {
            tile.boardTile = false;
            tile.isMasterTile = false;
            tile.isInColorSequence = false;
            tile.isInRunSequence = false;
            tile.transform.SetParent(deckParent);
            tile.transform.position = deckParent.position;
            tile.tileCoverImage.gameObject.SetActive(true);
            tile.gameObject.SetActive(false);
        }

        playersDeck.Clear();
        deck.Clear();

        numberOfPlayers = _numberOfPlayers;
        playersDeck = new List<PlayerDeck>(numberOfPlayers);
        for (var i = 0; i < numberOfPlayers; i++)
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
        
        
        StackManager.Instance.ResetStackManager();
        GameManager.Instance.ResetGameManager();

        if (PhotonNetwork.NickName == "Player 1")
        {
            photonView.RPC(nameof(RPC_InstantiateTiles), RpcTarget.AllBuffered);
        }
    }

    private void ResetAll()
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

        foreach (var tile in tiles)
        {
            tile.boardTile = false;
            tile.isMasterTile = false;
            tile.isInColorSequence = false;
            tile.isInRunSequence = false;
            tile.transform.SetParent(deckParent);
            tile.transform.position = deckParent.position;
            tile.tileCoverImage.gameObject.SetActive(true);
            tile.gameObject.SetActive(false);
        }

        ChatMessage[] chatMessages = chatContent.GetComponentsInChildren<ChatMessage>();
        
        foreach (var chat in chatMessages)
        {
            Destroy(chat.gameObject);
        }

        playersDeck.Clear();
        deck.Clear();

        GameSetup.Instance.ResetGameSetup();
        StackManager.Instance.ResetStackManager();
        GameManager.Instance.ResetGameManager();

    }
}

[Serializable]
public class TileSequence
{
    public List<Tile> tileSet;
}

public enum SequenceType
{
    None,
    Runs,
    Groups
}