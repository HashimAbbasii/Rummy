using Photon.Pun;
using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[Serializable]
public class TilePicked
{
    public Tile tile;
    public GridElement sourceGridElement;
}

[Serializable]
public enum GameMode
{
    Rami_31,
    Rami_Annette,
    Rami_51
}

[Serializable]
public class MovedTile
{
    public Tile tile;
    public GridElement sourceGridElement;
    public GridElement targetGridElement;
}

[Serializable]
public struct TilesSequence
{
    public List<Tile> tiles;
}

[Serializable]
public struct Room
{
    public string username;
    public string userID;
    public string roomKey;
    public string roomCode;
    public int roomCost;
    public int standByTime;
}

[Serializable]
public struct Sentence
{
    public string englishSentance;
    public string hebrewSentence;
}

public class GameManager : MonoBehaviour
{
    [HideInInspector] public UnityEvent languageSwitch;
    public List<Sentence> sentences;
    [SerializedDictionary("English","Hebrew")]
    public SerializedDictionary<string, string> sentencesDictionary =  new();
    
    [Header("Mode")]
    public GameMode gameMode;
    
    [Header("Room")]
    public RoomType roomType;
    public Room room;

    [Header("Game Play")]
    public List<TilePicked> dragTiles;
    public List<MovedTile> movedTiles;
    /*[HideInInspector]*/ public List<TilesSequence> tilesList = new();
    
    public event Action onGameModeSelected;

    private static GameManager _instance;

    public static GameManager Instance
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

        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
    
    private void Start()
    {
        foreach (var sentence in sentences)
        {
            sentencesDictionary.Add(sentence.englishSentance, sentence.hebrewSentence);
        }
    }

    public void SetGameMode(GameMode gm)
    {
        UIManager.Instance.DisplaySpecificScreen(GameScreens.MainScreen);
        gameMode = gm;

        onGameModeSelected?.Invoke();
    }
    
    public string Translate(string message)
    {
        return PreferenceManager.Language == "English" ? message : sentencesDictionary[message];
    }

    public bool TileInRange()
    {
        float x1, x2, y1, y2;

        x1 = dragTiles[0].sourceGridElement.transform.position.x - (dragTiles[0].sourceGridElement.GetComponent<RectTransform>().rect.width / 2);
        x2 = dragTiles[0].sourceGridElement.transform.position.x + (dragTiles[0].sourceGridElement.GetComponent<RectTransform>().rect.width / 2);

        y1 = dragTiles[0].sourceGridElement.transform.position.y - (dragTiles[0].sourceGridElement.GetComponent<RectTransform>().rect.height / 2);
        y2 = dragTiles[0].sourceGridElement.transform.position.y + (dragTiles[0].sourceGridElement.GetComponent<RectTransform>().rect.height / 2);

#if UNITY_ANDROID || UNITY_IOS

        GamePlayScreenHandler gpsh = new GamePlayScreenHandler();
        
        
        switch (gameMode)
        {
            case GameMode.Rami_31:
                gpsh = UIManager.Instance.UIScreensReferences[GameScreens.OnlineGamePlayScreen].GetComponent<OnlineGamePlayScreenHandler>();
                break;
            
            case GameMode.Rami_Annette:
                gpsh = UIManager.Instance.UIScreensReferences[GameScreens.RamiAnnetteScreen].GetComponent<RamiAnnetteGameplayScreenHandler>();
                break;
            
            case GameMode.Rami_51:
                break;
        }
        
        if (dragTiles[0].tile.transform.position.x >= x1 && 
            dragTiles[0].tile.transform.position.x <= x2 && 
            dragTiles[0].tile.transform.position.y - gpsh.tilePrefab.GetComponent<RectTransform>().rect.height / 2 >= y1 && 
            dragTiles[0].tile.transform.position.y - gpsh.tilePrefab.GetComponent<RectTransform>().rect.height / 2 <= y2)
#else
        if (dragTiles[0].tile.transform.position.x >= x1 && dragTiles[0].tile.transform.position.x <= x2 && dragTiles[0].tile.transform.position.y >= y1 && dragTiles[0].tile.transform.position.y <= y2)
#endif
            return true;
        else
            return false;
    }
    
    public void Undo(string playerTurn, int playerNumber)
    {
        //for local player
        if (PhotonNetwork.NickName == playerTurn)
        {
            //clear the source grid element
            foreach (MovedTile movedTile in movedTiles)
            {
                movedTile.targetGridElement.Clear();
            }

            //fill the target grid element and move the tile to target grid element
            foreach (MovedTile movedTile in movedTiles)
            {
                if (movedTile.sourceGridElement.playerGrid && movedTile.sourceGridElement.tileStatus == TileStatus.Filled)
                {
                    foreach (GridElement gridElement in GridManager.Instance.playerGrid)
                    {
                        if (gridElement.tileStatus == TileStatus.Vacant)
                        {
                            movedTile.tile.transform.LeanMove(gridElement.transform.position, 0.5f);
                            gridElement.Fill(movedTile.tile);
                            break;
                        }
                    }
                }
                else
                {
                    movedTile.tile.transform.LeanMove(movedTile.sourceGridElement.transform.position, 0.5f);
                    movedTile.sourceGridElement.Fill(movedTile.tile);
                }
            }
        }
        //for network player
        else
        {
            //clear the source grid element
            foreach (MovedTile movedTile in movedTiles)
            {
                movedTile.targetGridElement.Clear();
            }

            //fill the target grid element and move the tile to target grid element
            foreach (MovedTile movedTile in movedTiles)
            {
                if (movedTile.sourceGridElement == null)
                {
                    movedTile.tile.transform.LeanMove(GameSetup.Instance.players[playerNumber - 1].playerIcon.transform.position, 0.5f);
                }
                else
                {
                    movedTile.tile.transform.LeanMove(movedTile.sourceGridElement.transform.position, 0.5f);
                    movedTile.sourceGridElement.Fill(movedTile.tile);
                }
            }
        }

        movedTiles.Clear();
    }
    
    public void ResetGameManager()
    {
        dragTiles.Clear();
        movedTiles.Clear();
    }

    
    
    #region Rumi 31 Only

    public bool FirstMoveIsValid(int playerNumber)
    {
        // Debug.Log("FistMoveIsValid");
        
        if (movedTiles.Count == 0)
            return true;

        if (!UIManager.Instance.UIScreensReferences[GameScreens.OnlineGamePlayScreen].GetComponent<OnlineGamePlayScreenHandler>().playersDeck[playerNumber - 1].firstMove)
        {
            //if any tile manipulated from the board
            foreach (MovedTile movedTile in movedTiles)
            {
                if (movedTile.sourceGridElement?.playerGrid == false && movedTile.sourceGridElement != movedTile.targetGridElement)
                {
                    UIManager.Instance.UIScreensReferences[GameScreens.OnlineGamePlayScreen].GetComponent<OnlineGamePlayScreenHandler>().DisplayMessage("You can't manipulate board tiles on your first move");
                    return false;
                }
            }

            for (int i = 0; i < GridManager.Instance.grid.Count; i++)
            {
                if (GridManager.Instance.grid[i].tile != null && !GridManager.Instance.grid[i].tile.boardTile)
                {
                    //if tile(s) placed before board tile
                    if (i != 0 && GridManager.Instance.grid[i - 1].tile?.boardTile == true && GridManager.Instance.grid[i].tile?.transform.position.y == GridManager.Instance.grid[i - 1].tile?.transform.position.y)
                    {
                        UIManager.Instance.UIScreensReferences[GameScreens.OnlineGamePlayScreen].GetComponent<OnlineGamePlayScreenHandler>().DisplayMessage("You can't manipulate board tiles on your first move");
                        return false;
                    }

                    int j = 1;
                    while (true)
                    {
                        //if next tile exist
                        if ((i + j) <= (GridManager.Instance.grid.Count - 1) && GridManager.Instance.grid[i + j].tile)
                        {
                            //Next tile is a board tile
                            if (GridManager.Instance.grid[i + j].tile.boardTile && GridManager.Instance.grid[i].tile?.transform.position.y == GridManager.Instance.grid[i + j].tile?.transform.position.y)
                            {
                                UIManager.Instance.UIScreensReferences[GameScreens.OnlineGamePlayScreen].GetComponent<OnlineGamePlayScreenHandler>().DisplayMessage("You can't manipulate board tiles on your first move");
                                return false;
                            }
                            //Next tile is not a board tile
                            else
                            {
                                j++;
                            }
                        }
                        else
                        {
                            List<Tile> tilesTemp = new();

                            for (int k = i; k < (i + j); k++)
                            {
                                tilesTemp.Add(GridManager.Instance.grid[k].tile);
                            }

                            tilesList.Add(new TilesSequence { tiles = tilesTemp });
                            i += tilesTemp.Count;
                            break;
                        }
                    }
                }
            }

            //Check minimum 30 points at first turn
            int totalPoints = 0;

            foreach (TilesSequence tilesSequence in tilesList)
            {
                if (tilesSequence.tiles.Count < 3)
                {
                    UIManager.Instance.UIScreensReferences[GameScreens.OnlineGamePlayScreen].GetComponent<OnlineGamePlayScreenHandler>().DisplayMessage("The board is invalid");
                    return false;
                }
                else
                {
                    int points = 0;

                    if (GroupSequence(tilesSequence.tiles))
                    {
                        Debug.Log("Points Count Log: Group Sequence");
                        foreach (Tile tile in tilesSequence.tiles)
                        {
                            if (tile.tileNumber != 0)
                            {
                                points = tile.tileNumber * tilesSequence.tiles.Count;
                                Debug.Log("Points Count Log: Points: " + points);
                                break;
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("Points Count Log: Runs Sequence");
                        foreach (Tile tile in tilesSequence.tiles)
                        {
                            if (tile.tileNumber != 0)
                            {
                                //Normal Tile found at start
                                if (tilesSequence.tiles.IndexOf(tile) == 0)
                                {
                                    for (int i = tilesSequence.tiles.IndexOf(tile); i < tilesSequence.tiles.Count; i++)
                                    {
                                        points += (tilesSequence.tiles[tilesSequence.tiles.IndexOf(tile)].tileNumber + i);
                                    }
                                    Debug.Log("Points Count Log: Points: " + points);
                                }
                                //Joker tile at start
                                else
                                {
                                    points = tilesSequence.tiles[tilesSequence.tiles.IndexOf(tile)].tileNumber - 1;

                                    for (int i = tilesSequence.tiles.IndexOf(tile); i < tilesSequence.tiles.Count; i++)
                                    {
                                        points += (tilesSequence.tiles[tilesSequence.tiles.IndexOf(tile)].tileNumber + (i - 1));
                                    }
                                    Debug.Log("Points Count Log: Points: " + points);
                                }

                                break;
                            }
                        }
                    }

                    totalPoints += points;
                    Debug.Log("Points Count Log: Points of sequence " + (tilesList.IndexOf(tilesSequence) + 1) + " :" + points);
                }
            }

            Debug.Log("Points Count Log: Total Points: " + totalPoints);

            if (totalPoints < 30)
            {
                UIManager.Instance.UIScreensReferences[GameScreens.OnlineGamePlayScreen].GetComponent<OnlineGamePlayScreenHandler>().DisplayMessage("Your initial move must have a value of 30 points or more");
                return false;
            }
        }

        UIManager.Instance.UIScreensReferences[GameScreens.OnlineGamePlayScreen].GetComponent<OnlineGamePlayScreenHandler>().playersDeck[playerNumber - 1].firstMove = true;
        return true;
    }
    
    private bool GroupSequence(List<Tile> tilesSequence)
    {
        Tile compareTile = new();

        foreach (Tile tile in tilesSequence)
        {
            if (tile.tileNumber != 0)
            {
                compareTile = tile;
                break;
            }
        }

        //check if all tiles have different color
        foreach (Tile tile in tilesSequence)
        {
            if (tile.name != compareTile.name && tile.tileColor != TileColor.Joker && compareTile.tileColor == tile.tileColor)
            {
                return false;
            }
        }

        //check if all tiles have same number
        foreach (Tile tile in tilesSequence)
        {
            if (tile.name != compareTile.name && tile.tileNumber != 0 && compareTile.tileNumber != tile.tileNumber)
            {
                return false;
            }
        }

        return true;
    }

    #endregion
    
}

public enum RoomType
{
    Private,
    Public
}