using System;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if UNITY_IOS
public class Tile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
#else
public class Tile : MonoBehaviour
#endif
{
    public GameSetup gameSetup;
    public PhotonView PV;
    public TileColor tileColor;
    public TileType tileType;
    public int tileNumber;
    public bool boardTile;
    public bool isMasterTile;
    public TextMeshProUGUI tileNumberText;
    public Image jokerImage;
    public List<Sprite> jokerSprites;
    public Image tileCoverImage;
    private string sequence = null;
    public Texture2D cursorTexture;

    public int sequenceNumber = -1;
    
    public bool isInRunSequence;
    public bool isInColorSequence;

    [FormerlySerializedAs("_pickUpPosition")] public Vector3 pickUpPosition;

    private void OnEnable()
    {
        PV = GetComponent<PhotonView>();
        tileCoverImage.gameObject.SetActive(true);
    }

    // UIManager.Instance.UIScreensReferences[GameScreens.OnlineGamePlayScreen].GetComponent<OnlineGamePlayScreenHandler>()

    #region IOS

#if UNITY_IOS
    private void Update()
    {
        switch (GameManager.Instance.gameMode)
        {
            case GameMode.Rami_31:
                if (UIManager.Instance.onlineGamePlayScreenHandler.playerTurn == PhotonNetwork.NickName)
                {
                    for (int i = 0; i < GameManager.Instance.dragTiles.Count; i++)
                    {
                        GameManager.Instance.dragTiles[i].tile.transform.position = new Vector2(Input.mousePosition.x + (i * UIManager.Instance.onlineGamePlayScreenHandler.tilePrefab.GetComponent<RectTransform>().rect.width * FindObjectOfType<Canvas>().transform.localScale.x), Input.mousePosition.y + ((UIManager.Instance.onlineGamePlayScreenHandler.tilePrefab.GetComponent<RectTransform>().rect.height / 2) * FindObjectOfType<Canvas>().transform.localScale.y));
                    }
                }
                break;
            
            case GameMode.Rami_Annette:
                if (UIManager.Instance.ramiAnnetteGameplayScreenHandler.playerTurn == PhotonNetwork.NickName)
                {
                    for (int i = 0; i < GameManager.Instance.dragTiles.Count; i++)
                    {
                        GameManager.Instance.dragTiles[i].tile.transform.position = new Vector2(Input.mousePosition.x + (i * UIManager.Instance.ramiAnnetteGameplayScreenHandler.tilePrefab.GetComponent<RectTransform>().rect.width * FindObjectOfType<Canvas>().transform.localScale.x), Input.mousePosition.y + ((UIManager.Instance.ramiAnnetteGameplayScreenHandler.tilePrefab.GetComponent<RectTransform>().rect.height / 2) * FindObjectOfType<Canvas>().transform.localScale.y));
                    }
                }
                break;
            
            case GameMode.Rami_51:
                break;
        }
        
        
    }

    // ReSharper disable twice CompareOfFloatsByEqualityOperator
    public void OnPointerDown(PointerEventData eventData)
    {
        switch (GameManager.Instance.gameMode)
        {
            case GameMode.Rami_31:
                // Debug.Log("Tile Log: Start");
                if (UIManager.Instance.onlineGamePlayScreenHandler.playerTurn != PhotonNetwork.NickName || GameSetup.Instance.players[UIManager.Instance.onlineGamePlayScreenHandler.PlayerNumber(PhotonNetwork.NickName) - 1]
                        .playerIcon.timerImage.fillAmount == 1) return;
                AddTile(this);
                StartCoroutine(PickNextTileCoroutine());
                break;
            
            case GameMode.Rami_Annette:
                AddTile(this);
                break;
            
            case GameMode.Rami_51:
                break;
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Tile Log: End");

        switch (GameManager.Instance.gameMode)
        {
            case GameMode.Rami_31:
                if (GameManager.Instance.dragTiles.Count != 0)
                {
                    if (GridManager.Instance.EnoughSpace() &&
                        GridManager.Instance.SequenceFollowed() &&
                        UIManager.Instance.onlineGamePlayScreenHandler.playerTurn == PhotonNetwork.NickName &&
                       GameSetup.Instance.players[UIManager.Instance.onlineGamePlayScreenHandler.PlayerNumber(PhotonNetwork.NickName) - 1].playerIcon.timerImage.fillAmount != 1)
                    {
                        PlaceTiles();
                    }
                    else
                    {
                        //Debug.Log("Incorrect Place");

                        foreach (TilePicked tilePicked in GameManager.Instance.dragTiles)
                        {
                            tilePicked.tile.transform.LeanMove(tilePicked.sourceGridElement.transform.position, 0.5f);
                            tilePicked.sourceGridElement.tileStatus = TileStatus.Filled;
                        }

                        GameManager.Instance.dragTiles.Clear();
                        sequence = null;

                        AudioManager.Instance.PlayAudio(AudioName.WrongMoveAudio);
                    }

                    GridManager.Instance.ClearTempTileLists();
                }
                break;
            
            case GameMode.Rami_Annette:
                MoveTile();
                break;
            
            case GameMode.Rami_51:
                break;
        }
        
       
    }

#endif

    #endregion


    #region Non-IOS

#if !UNITY_IOS
    // ReSharper disable twice CompareOfFloatsByEqualityOperator
    private void OnMouseDown()
    {
        switch (GameManager.Instance.gameMode)
        {
            case GameMode.Rami_31:
                // Check if it is your turn plus 
                // if (UIManager.Instance.onlineGamePlayScreenHandler.playerTurn != PhotonNetwork.NickName || 
                //     GameSetup.Instance.players[UIManager.Instance.onlineGamePlayScreenHandler.PlayerNumber(PhotonNetwork.NickName) - 1].playerIcon.timerImage.fillAmount == 1) return;
                
                AddTile(this);
                StartCoroutine(PickNextTileCoroutine());
                break;

            case GameMode.Rami_Annette:
                // if (UIManager.Instance.ramiAnnetteGameplayScreenHandler.playerTurn != PhotonNetwork.NickName || GameSetup.Instance.players[UIManager.Instance.ramiAnnetteGameplayScreenHandler.PlayerNumber(PhotonNetwork.NickName) - 1].playerIcon.timerImage.fillAmount == 1) return;

                AddTile(this);

                // StartCoroutine(PickNextTileCoroutine());
                break;

            case GameMode.Rami_51:
                break;
        }
    }

    private void OnMouseDrag()
    {
        switch (GameManager.Instance.gameMode)
        {
            case GameMode.Rami_31:
                // if (UIManager.Instance.onlineGamePlayScreenHandler.playerTurn != PhotonNetwork.NickName) return;
                break;

            case GameMode.Rami_Annette:
                // if (UIManager.Instance.ramiAnnetteGameplayScreenHandler.playerTurn != PhotonNetwork.NickName) return;
                break;

            case GameMode.Rami_51:
                break;
        }

        for (int i = 0; i < GameManager.Instance.dragTiles.Count; i++)
        {
#if UNITY_ANDROID
                GameManager.Instance.dragTiles[i].tile.transform.position =
 new Vector2(Input.mousePosition.x + (i * GetComponent<RectTransform>().rect.width * FindObjectOfType<Canvas>().transform.localScale.x), Input.mousePosition.y + ((GetComponent<RectTransform>().rect.height / 2) * FindObjectOfType<Canvas>().transform.localScale.y));
#else
            GameManager.Instance.dragTiles[i].tile.transform.position = new Vector2(Input.mousePosition.x + (i * GetComponent<RectTransform>().rect.width * FindObjectOfType<Canvas>().transform.localScale.x), Input.mousePosition.y);
#endif

        }
    }

    // ReSharper disable once CompareOfFloatsByEqualityOperator
    public void OnMouseUp()
    {
        switch (GameManager.Instance.gameMode)
        {
            case GameMode.Rami_31:

                if (GameManager.Instance.dragTiles.Count == 0) return;

                if (GridManager.Instance.EnoughSpace() && 
                    GridManager.Instance.SequenceFollowed() && 
                    // UIManager.Instance.onlineGamePlayScreenHandler.playerTurn == PhotonNetwork.NickName &&
                    GameSetup.Instance.players[UIManager.Instance.onlineGamePlayScreenHandler.PlayerNumber(PhotonNetwork.NickName) - 1].playerIcon.timerImage.fillAmount != 1)
                {
                    PlaceTiles();
                }
                else
                {
                    foreach (TilePicked tilePicked in GameManager.Instance.dragTiles)
                    {
                        tilePicked.tile.transform.LeanMove(tilePicked.sourceGridElement.transform.position, 0.5f);
                        tilePicked.sourceGridElement.tileStatus = TileStatus.Filled;
                    }

                    GameManager.Instance.dragTiles.Clear();
                    sequence = null;

                    AudioManager.Instance.PlayAudio(AudioName.WrongMoveAudio);
                }

                GridManager.Instance.ClearTempTileLists();
                break;

            case GameMode.Rami_Annette:
                // if (UIManager.Instance.ramiAnnetteGameplayScreenHandler.playerTurn != PhotonNetwork.NickName || GameSetup.Instance.
                //         players[UIManager.Instance.ramiAnnetteGameplayScreenHandler.PlayerNumber(PhotonNetwork.NickName) - 1].playerIcon.timerImage.fillAmount == 1) return;
                
                // Debug.Log("Move Tile Called");
                
                MoveTile();
                break;

            case GameMode.Rami_51:
                break;
        }
    }


#endif

    #endregion


    private void AddTile(Tile tile)
    {
        TilePicked tilePicked = new();
        tilePicked.tile = tile;
        switch (GameManager.Instance.gameMode)
        {
            case GameMode.Rami_31:
            {
                GridManager.Instance.UpdateGridElementTileStatus(tile.name);
                tile.transform.SetAsLastSibling();

                AudioManager.Instance.PlayAudio(AudioName.TouchTileAudio);

                foreach (GridElement gridElement in GridManager.Instance.grid)
                {
                    if (gridElement.tile?.name != tile.name) continue;

                    tilePicked.sourceGridElement = gridElement;
                    GameManager.Instance.dragTiles.Add(tilePicked);
                    return;
                }

                foreach (GridElement gridElement in GridManager.Instance.playerGrid)
                {
                    if (gridElement.tile?.name != tile.name) continue;

                    tilePicked.sourceGridElement = gridElement;
                    GameManager.Instance.dragTiles.Add(tilePicked);
                    return;
                }

                break;
            }
            case GameMode.Rami_Annette:
            {
                // Debug.Log("Tile Added");
                // Debug.Log("Touched");
                StackManager.Instance.UpdateElementTileStatus(tile.name);
                AudioManager.Instance.PlayAudio(AudioName.TouchTileAudio);
                GameManager.Instance.dragTiles.Add(tilePicked);

                tile.transform.SetAsLastSibling();
                pickUpPosition = StackManager.Instance.FindGridElement(tilePicked.tile.transform.position).transform.position;
                // MoveTile(this);
                break;
            }
        }
    }


    private IEnumerator PickNextTileCoroutine()
    {
        // Debug.Log("PickNextTileCoroutine");

        yield return new WaitForSeconds(0.5f);

        if (GameManager.Instance.dragTiles.Count == 0 || !GameManager.Instance.TileInRange()) yield break;

        Tile nextTile = GridManager.Instance.NextTile(GameManager.Instance.dragTiles[^1].tile.name);

        //if next tile does not exist, break
        if (nextTile == null) yield break;

        if (string.IsNullOrEmpty(sequence))
        {
            //Debug.Log("Sequence: " + sequence);

            //check for next Joker Tile
            if (nextTile.tileColor == TileColor.Joker)
            {
                AddTile(nextTile);
                StartCoroutine(PickNextTileCoroutine());
            }
            //if current tile is Joker
            else if (GameManager.Instance.dragTiles[^1].tile.tileColor == TileColor.Joker)
            {
                if ((GameManager.Instance.dragTiles.Count == 1 &&
                     GameManager.Instance.dragTiles[0].tile.tileColor == TileColor.Joker) ||
                    (GameManager.Instance.dragTiles.Count == 2 &&
                     GameManager.Instance.dragTiles[0].tile.tileColor == TileColor.Joker &&
                     GameManager.Instance.dragTiles[1].tile.tileColor == TileColor.Joker))
                {
                    switch (GameManager.Instance.dragTiles.Count)
                    {
                        case 1 when nextTile.tileNumber == 1:
                        case 2 when (nextTile.tileNumber == 1 || nextTile.tileNumber == 2):
                            sequence = "Group";
                            break;
                    }

                    AddTile(nextTile);
                    StartCoroutine(PickNextTileCoroutine());
                }
                else if (GameManager.Instance.dragTiles[^2].tile.tileColor == TileColor.Joker)
                {
                    //check for Runs
                    CheckForRunsSequence(nextTile, 3);

                    //check for Group
                    CheckForGroupSequence(nextTile, 3);
                }
                else
                {
                    //check for Runs
                    CheckForRunsSequence(nextTile, 2);

                    //check for Group
                    CheckForGroupSequence(nextTile, 2);
                }
            }
            else
            {
                if (GameManager.Instance.dragTiles[^1].tile.tileNumber == 1 &&
                    GameManager.Instance.dragTiles.Count > 1 &&
                    GameManager.Instance.dragTiles[^2].tile.tileColor == TileColor.Joker)
                {
                    //check for Group
                    CheckForGroupSequence(nextTile, 1);
                }
                else
                {
                    //check for Runs
                    CheckForRunsSequence(nextTile, 1);

                    //check for Group
                    CheckForGroupSequence(nextTile, 1);
                }
            }
        }
        else if (sequence == "Runs")
        {
            //Debug.Log("Sequence: " + sequence);
            //check for next Joker Tile
            if (nextTile.tileColor == TileColor.Joker)
            {
                //check if last tile is not 13
                if (GameManager.Instance.dragTiles[^1].tile.tileColor != TileColor.Joker &&
                    GameManager.Instance.dragTiles[^1].tile.tileNumber != 13)
                {
                    AddTile(nextTile);
                    StartCoroutine(PickNextTileCoroutine());
                }
                //check if current tile is joker and second last is not 12 
                else if (GameManager.Instance.dragTiles[^1].tile.tileColor == TileColor.Joker &&
                         GameManager.Instance.dragTiles[^2].tile.tileNumber != 12)
                {
                    AddTile(nextTile);
                    StartCoroutine(PickNextTileCoroutine());
                }

            }
            //if current tile is Joker
            else if (GameManager.Instance.dragTiles[^1].tile.tileColor == TileColor.Joker)
            {
                int compareIndex = 2;

                if (GameManager.Instance.dragTiles[^2].tile.tileColor == TileColor.Joker)
                    compareIndex = 3;

                CheckForRunsSequence(nextTile, compareIndex);
            }
            else
            {
                //check for runs
                CheckForRunsSequence(nextTile, 1);
            }
        }
        else if (sequence == "Group")
        {
            //Debug.Log("Sequence: " + sequence);
            //check for next Joker Tile
            if (nextTile.tileColor == TileColor.Joker && GameManager.Instance.dragTiles.Count < 4)
            {
                AddTile(nextTile);
                StartCoroutine(PickNextTileCoroutine());
            }
            //if current tile is Joker
            else if (GameManager.Instance.dragTiles[^1].tile.tileColor == TileColor.Joker)
            {
                int compareIndex = 2;

                if (GameManager.Instance.dragTiles[^2].tile.tileColor == TileColor.Joker)
                    compareIndex = 3;

                CheckForGroupSequence(nextTile, compareIndex);
            }
            else
            {
                CheckForGroupSequence(nextTile, 1);
            }
        }
    }


    private void CheckForRunsSequence(Tile nextTile, int compareIndex)
    {
        // Debug.Log("CheckForRunsSequence");

        if (nextTile.tileColor != GameManager.Instance.dragTiles[^compareIndex].tile.tileColor || nextTile.tileNumber !=
            (GameManager.Instance.dragTiles[^compareIndex].tile.tileNumber + compareIndex)) return;

        sequence = "Runs"; //Debug.Log("Sequence: " + sequence);
        AddTile(nextTile);
        StartCoroutine(PickNextTileCoroutine());
    }


    private void CheckForGroupSequence(Tile nextTile, int compareIndex)
    {
        // Debug.Log("CheckForGroupSequence");

        if (GameManager.Instance.dragTiles.Count >= 4) return;

        bool allDifferentColors = true;
        foreach (TilePicked tilePicked in GameManager.Instance.dragTiles)
        {
            if (nextTile.tileColor != tilePicked.tile.tileColor) continue;

            allDifferentColors = false;
            break;
        }

        if (!allDifferentColors ||
            nextTile.tileNumber != GameManager.Instance.dragTiles[^compareIndex].tile.tileNumber) return;

        sequence = "Group"; //Debug.Log("Sequence: " + sequence);
        AddTile(nextTile);
        StartCoroutine(PickNextTileCoroutine());
    }


    private void PlaceTiles()
    {
        // Debug.Log("PlaceTiles");

        List<int> tempTileNumber = new();
        List<string> tempSourceGridName = new();
        List<string> tempTargetGridName = new();

        //find the target grid element and clear the source grid element
        foreach (TilePicked tilePicked in GameManager.Instance.dragTiles)
        {
            GameObject targetGridElementGameObject = GridManager.Instance.FindGridElement(new Vector2(
                tilePicked.tile.transform.position.x + (GameManager.Instance.dragTiles.IndexOf(tilePicked) *
                                                        GridManager.Instance.xAxisDistance),
                tilePicked.tile.transform.position.y));

            if (!targetGridElementGameObject.GetComponent<GridElement>().playerGrid)
            {
                if (UIManager.Instance.onlineGamePlayScreenHandler.playerTurn != PhotonNetwork.NickName)
                {
                    foreach (var tp in GameManager.Instance.dragTiles)
                    {
                       tp.tile.transform.LeanMove(tp.sourceGridElement.transform.position, 0.2f);
                    }
                    GameManager.Instance.dragTiles.Clear();
                    return;
                }
                
                bool alreadyExist = false;

                foreach (MovedTile movedTile in GameManager.Instance.movedTiles)
                {
                    if (movedTile.tile.name != tilePicked.tile.name) continue;

                    movedTile.targetGridElement = targetGridElementGameObject.GetComponent<GridElement>();
                    alreadyExist = true;
                    break;
                }

                if (!alreadyExist)
                {
                    GameManager.Instance.movedTiles.Add(new MovedTile
                    {
                        tile = tilePicked.tile, sourceGridElement = tilePicked.sourceGridElement,
                        targetGridElement = targetGridElementGameObject.GetComponent<GridElement>()
                    });
                }
            }

            if (tilePicked.sourceGridElement.playerGrid && !targetGridElementGameObject.GetComponent<GridElement>().playerGrid)
            {
                UIManager.Instance.onlineGamePlayScreenHandler.DisplayButton(ButtonType.TurnButton);
            }

            if (targetGridElementGameObject.GetComponent<GridElement>().gridName != tilePicked.sourceGridElement.gridName)
            {
                tilePicked.sourceGridElement.Clear();
            }

            if (!targetGridElementGameObject.GetComponent<GridElement>().playerGrid)
            {
                UIManager.Instance.onlineGamePlayScreenHandler.RemoveTileFromPlayerTiles(tilePicked.tile);
            }

        }

        //find the target grid element, fill the target grid element and move the tile to target grid element
        foreach (TilePicked tilePicked in GameManager.Instance.dragTiles)
        {
            GameObject targetGridElementGameObject = GridManager.Instance.FindGridElement(new Vector2(
                    tilePicked.tile.transform.position.x + (GameManager.Instance.dragTiles.IndexOf(tilePicked) * GridManager.Instance.xAxisDistance),
                    tilePicked.tile.transform.position.y));
            
            tilePicked.tile.transform.LeanMove(targetGridElementGameObject.transform.position, 0.2f);
            targetGridElementGameObject.GetComponent<GridElement>().Fill(tilePicked.tile);
            tempTileNumber.Add(tilePicked.tile.GetComponent<PhotonView>().ViewID);

            tempSourceGridName.Add(tilePicked.sourceGridElement.playerGrid ? "" : tilePicked.sourceGridElement.gridName);

            tempTargetGridName.Add(targetGridElementGameObject.GetComponent<GridElement>().playerGrid ? "" : targetGridElementGameObject.GetComponent<GridElement>().gridName);
        }

        AudioManager.Instance.PlayAudio(GameManager.Instance.dragTiles[0].sourceGridElement.playerGrid ? AudioName.MovedTileOnTableFromRackAudio : AudioName.MovedTileOnTableAudio);

        GetComponent<PhotonView>().RPC(nameof(RPC_PlaceTiles), RpcTarget.OthersBuffered, 
            tempTileNumber.ToArray(), tempSourceGridName.ToArray(), tempTargetGridName.ToArray());
        GameManager.Instance.dragTiles.Clear();
        sequence = null;
    }

    [PunRPC]
    private void RPC_PlaceTiles(int[] tileNumber, string[] sourceGridElementName, string[] targetGridElementName)
    {
        List<MovedTile> movedTileTemp = new();

        for (int i = 0; i < tileNumber.Length; i++)
        {
            GridElement targetGridElement = new();
            GridElement sourceGridElement = new();
            Tile tile = new();
            MovedTile movedTile = new();

            foreach (GridElement gridElement in GridManager.Instance.grid)
            {
                if (gridElement.gridName == sourceGridElementName[i])
                {
                    sourceGridElement = gridElement;
                    movedTile.sourceGridElement = gridElement;
                }

                if (gridElement.gridName != targetGridElementName[i]) continue;

                targetGridElement = gridElement;
                movedTile.targetGridElement = gridElement;
            }

            if (targetGridElement != null)
            {
                tile = PhotonView.Find(tileNumber[i]).GetComponent<Tile>();

                movedTile.tile = tile;
                bool alreadyExist = false;

                foreach (MovedTile _movedTile in GameManager.Instance.movedTiles)
                {
                    if (_movedTile.tile.name != tile.name) continue;

                    _movedTile.targetGridElement = targetGridElement;
                    alreadyExist = true;
                    break;
                }

                if (!alreadyExist)
                {
                    GameManager.Instance.movedTiles.Add(movedTile);
                }

                movedTileTemp.Add(movedTile);

                if (sourceGridElementName[i].Length != 0 && targetGridElement.gridName != sourceGridElement.gridName)
                    sourceGridElement.Clear();


                switch (GameManager.Instance.gameMode)
                {
                    case GameMode.Rami_31:
                        if (!targetGridElement.playerGrid)
                            UIManager.Instance.onlineGamePlayScreenHandler.RemoveTileFromPlayerTiles(tile);
                        break;

                    case GameMode.Rami_Annette:

                        break;

                    case GameMode.Rami_51:
                        break;
                }

            }

            if (targetGridElement != null)
            {
                tile.transform.LeanMove(targetGridElement.transform.position, 0.2f);
            }
        }

        foreach (MovedTile _movedTile in movedTileTemp)
        {
            _movedTile.targetGridElement.Fill(_movedTile.tile);
        }
    }


    private void MoveTile()
    {
        
        var raGSH = UIManager.Instance.ramiAnnetteGameplayScreenHandler;
        int playerNumber = raGSH.PlayerNumber(raGSH.playerTurn);
        var player = GameSetup.Instance.players[playerNumber - 1];

        var tile = GameManager.Instance.dragTiles[0].tile;
        var viewID = GameManager.Instance.dragTiles[0].tile.GetComponent<PhotonView>().ViewID;
        GameManager.Instance.dragTiles.RemoveAt(0);

        if (StackManager.Instance.isWinning)
        {
            tile.transform.LeanMove(pickUpPosition, 0.2f);
            return;
        }

        var iniPos = StackManager.Instance.FindGridElement(pickUpPosition);
        var placePos = StackManager.Instance.FindGridElement(tile.transform.position);

        // Debug.Log(placePos);

        //Placed into Deck
        if (placePos?.GetComponent<StackElement>())
        {
            if (!raGSH.nextButton.interactable || tile.isMasterTile || player.ramiAnnetteTurn.placedATile ||
                UIManager.Instance.ramiAnnetteGameplayScreenHandler.playerTurn != PhotonNetwork.NickName)
            {
                tile.transform.LeanMove(pickUpPosition, 0.2f);
                return;
            }

            //RPC from P.Grid to Stack
            iniPos.GetComponent<GridElement>().Clear();

            CheckSequences();
            CheckForWin();

            PV.RPC(nameof(RPC_MoveTile), RpcTarget.All, viewID, true);

        }
        //Placed into Grid
        else if (placePos?.GetComponent<GridElement>())
        {
            var ge = placePos.GetComponent<GridElement>();

            if (ge.tileStatus == TileStatus.Vacant)
            {
                // Is moving in player grid or from master to grid
                if (iniPos.GetComponent<GridElement>())
                {
                    iniPos.GetComponent<GridElement>().Clear();
                    tile.transform.LeanMove(placePos.transform.position, 0.2f);
                    ge.Fill(tile);

                    RearrangeTiles();
                    raGSH.tileSequences.Clear();
                    CheckSequences();
                    CheckForWin();

                    if (!tile.isMasterTile) return;

                    // Debug.LogError("Is Master Tile");

                    if (player.ramiAnnetteTurn.tookATile ||
                        UIManager.Instance.ramiAnnetteGameplayScreenHandler.playerTurn != PhotonNetwork.NickName)
                    {
                        // Debug.LogError("MT not your turn");
                        tile.transform.LeanMove(pickUpPosition, 0.2f);
                        iniPos.GetComponent<GridElement>().Fill(tile);
                        ge.Clear();
                        return;
                    }


                    if (CheckForWin())
                    {
                        Debug.LogError("MT Win Yes");
                        tile.transform.LeanMove(placePos.transform.position, 0.2f);
                        PV.RPC(nameof(RPC_MoveTile), RpcTarget.AllBuffered, viewID, false);
                        StackManager.Instance.isWinning = true;
                    }
                    else
                    {
                        tile.transform.LeanMove(pickUpPosition, 0.2f);
                        iniPos.GetComponent<GridElement>().Fill(tile);
                        ge.Clear();
                    }

                    RearrangeTiles();
                    raGSH.tileSequences.Clear();
                    CheckSequences();
                    CheckForWin();
                }
                // Is moved from stack to grid
                else
                {
                    if (player.ramiAnnetteTurn.tookATile ||
                        UIManager.Instance.ramiAnnetteGameplayScreenHandler.playerTurn != PhotonNetwork.NickName)
                    {
                        tile.transform.LeanMove(pickUpPosition, 0.2f);
                        return;
                    }

                    //RPC from Stack to P.Grid

                    PV.RPC(nameof(RPC_MoveTile), RpcTarget.All, viewID, false);
                    tile.transform.LeanMove(placePos.transform.position, 0.2f);
                    ge.Fill(tile);
                    raGSH.tilePicked = true;
                    RearrangeTiles();
                    raGSH.tileSequences.Clear();
                    CheckSequences();
                    CheckForWin();
                }
            }
            else
            {
                tile.transform.LeanMove(pickUpPosition, 0.2f);
            }
        }
        else
        {
            tile.transform.LeanMove(pickUpPosition, 0.2f);
        }
    }

    [PunRPC]
    private void RPC_MoveTile(int _tileID, bool gridToStack)
    {
        var raGSH = UIManager.Instance.ramiAnnetteGameplayScreenHandler;
        int playerNumber = raGSH.PlayerNumber(raGSH.playerTurn);

        var player = GameSetup.Instance.players[playerNumber - 1];

        var tile = PhotonView.Find(_tileID).GetComponent<Tile>();


        if (gridToStack)
        {
            raGSH.playersDeck[playerNumber - 1].playerTiles.Remove(tile);
            player.playerIcon.tilesCount--;
            tile.transform.LeanMove(StackManager.Instance.stackParent.position, 0.2f);
            player.ramiAnnetteTurn.placedATile = true;
            StackManager.Instance.AddTile(tile);

        }
        else
        {
            raGSH.playersDeck[playerNumber - 1].playerTiles.Add(tile);
            player.playerIcon.tilesCount++;
            player.ramiAnnetteTurn.tookATile = true;
            player.lastTileInteractedWith = tile;
            tile.boardTile = false;

            if (StackManager.Instance.stack.tileStack.Contains(tile))
            {
                StackManager.Instance.RemoveTile(tile);
            }

            if (PhotonNetwork.NickName != raGSH.playerTurn)
            {
                tile.transform.LeanMove(GameSetup.Instance.players[playerNumber - 1].playerIcon.transform.position, 0.2f);
            }
        }
    }
    
    
    public void RearrangeTiles()
    {
        var raGSH = UIManager.Instance.ramiAnnetteGameplayScreenHandler;
        int playerNumber = raGSH.PlayerNumber(raGSH.playerTurn);
        var player = GameSetup.Instance.players[playerNumber - 1];
        var playerTiles = raGSH.playersDeck[playerNumber - 1].playerTiles;

        playerTiles.Clear();

        foreach (var ge in StackManager.Instance.playerGrid)
        {
            if (ge.tile == null) continue;

            playerTiles.Add(ge.tile);
        }
        
        int[] tempVID = new int[playerTiles.Count];
        
        for (int i = 0; i < playerTiles.Count; i++)
        {
            tempVID[i] = playerTiles[i].GetComponent<PhotonView>().ViewID;
        }
        
        PV.RPC(nameof(RPC_RearrangeTiles), RpcTarget.All, tempVID);
    }

    [PunRPC]
    private void RPC_RearrangeTiles(int[] tileViewIDs)
    {
        var raGSH = UIManager.Instance.ramiAnnetteGameplayScreenHandler;
        int playerNumber = raGSH.PlayerNumber(raGSH.playerTurn);
        
        List<Tile> tiles = new(); 
        
        foreach (var vID in tileViewIDs)
        {
            tiles.Add(PhotonView.Find(vID).GetComponent<Tile>());
        }

        raGSH.playersDeck[playerNumber - 1].playerTiles.Clear();

        foreach (var t in tiles)
        {
            raGSH.playersDeck[playerNumber - 1].playerTiles.Add(t);
        }
    }
    
    private void CheckSequences()
    {
        UIManager.Instance.ramiAnnetteGameplayScreenHandler.tileSequences.Clear();
        UIManager.Instance.ramiAnnetteGameplayScreenHandler.CheckSequences();
    }

    
    private bool CheckForWin()
    { 
        var raGSH = UIManager.Instance.ramiAnnetteGameplayScreenHandler;
        int playerNumber = raGSH.PlayerNumber(raGSH.playerTurn);
        var playerTiles = raGSH.playersDeck[playerNumber - 1].playerTiles;
       
        if (playerTiles.Count != 14) return false;
        
        if (raGSH.tileSequences.Count == 0) return false;
        
        bool allSequencesHaveThreeOrMoreTiles = UIManager.Instance.ramiAnnetteGameplayScreenHandler.tileSequences.All(t => t.tileSet.Count >= 3);

        PV.RPC(nameof(RPC_CheckForWin), RpcTarget.All, allSequencesHaveThreeOrMoreTiles);
        
        return allSequencesHaveThreeOrMoreTiles;
    }

    [PunRPC]
    private void RPC_CheckForWin(bool seq)
    {
        var raGSH = UIManager.Instance.ramiAnnetteGameplayScreenHandler;
        int playerNumber = raGSH.PlayerNumber(raGSH.playerTurn);

        var player = GameSetup.Instance.players[playerNumber - 1];

        player.allSequencesHaveThreeOrMoreTiles = seq;
    }
    
}

public enum TileColor
{
    Black,
    Blue,
    Yellow,
    Red,
    Joker
}

public enum TileType
{
    Primary,
    Secondary
}