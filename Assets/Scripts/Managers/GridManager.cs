using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int xAxisDistance;
    public int yAxisDistance;
    [HideInInspector] public string sequence = null;
    [HideInInspector] public List<Tile> totalTiles = new();
    [HideInInspector] public List<Tile> tempTiles = new();
    public Transform gridParent;
    public Transform playerGridParent;
    public GridElement gridElementPrefab;
    public List<GridElement> grid;
    public List<GridElement> playerGrid;

    public const int TABLE_VERTICAL_GRID = 6;
    public const int TABLE_HORIZONTAL_GRID = 20; 
    
    public const int PLAYER_VERTICAL_GRID = 2;
    public const int PLAYER_HORIZONTAL_GRID = 19;

    #region Singleton

    private static GridManager _instance;
    public static GridManager Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }
    
    #endregion

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        int xAxis = 0;
        int yAxis = 0;

        //generating table grid
        for (int y = 0; y < TABLE_VERTICAL_GRID; y++)
        {
            xAxis = 0;
            for (int x = 0; x < TABLE_HORIZONTAL_GRID; x++)
            {
                grid.Add(Instantiate(gridElementPrefab));
                grid[^1].gridName = "Table Grid " + grid.Count.ToString();
                grid[^1].name = grid[^1].gridName;
                grid[^1].transform.SetParent(gridParent);
                grid[^1].transform.localPosition = new Vector3(xAxis, -yAxis);
                grid[^1].transform.localScale = new Vector3(1, 1, 1);
                grid[^1].playerGrid = false;
                xAxis += (int)(gridElementPrefab.GetComponent<RectTransform>().sizeDelta.x + xAxisDistance);
            }

            yAxis += (int)(gridElementPrefab.GetComponent<RectTransform>().sizeDelta.y + yAxisDistance);
        }

        xAxis = 0;
        yAxis = 0;

        //generating player grid
        for (int y = 0; y < PLAYER_VERTICAL_GRID; y++)
        {
            xAxis = 0;
            for (int x = 0; x < PLAYER_HORIZONTAL_GRID; x++)
            {
                playerGrid.Add(Instantiate(gridElementPrefab));
                playerGrid[^1].gridName = "Player Grid " + playerGrid.Count.ToString();
                grid[^1].name = grid[^1].gridName;
                playerGrid[^1].transform.SetParent(playerGridParent);
                playerGrid[^1].transform.localPosition = new Vector3(xAxis, -yAxis);
                playerGrid[^1].transform.localScale = new Vector3(1, 1, 1);
                playerGrid[^1].playerGrid = true;
                xAxis += (int)(gridElementPrefab.GetComponent<RectTransform>().sizeDelta.x + xAxisDistance);
            }

            yAxis += (int)(gridElementPrefab.GetComponent<RectTransform>().sizeDelta.y + yAxisDistance + 10);
        }
    }
    
    public GameObject FindGridElement(Vector2 tilePosition)
    {
        // Debug.Log("Find Grid Element");
        
        float x1, x2, y1, y2;

        foreach (GridElement gridElement in grid)
        {
            x1 = gridElement.transform.position.x - (gridElement.GetComponent<RectTransform>().rect.width / 2);
            x2 = gridElement.transform.position.x + (gridElement.GetComponent<RectTransform>().rect.width / 2);

            y1 = gridElement.transform.position.y - (gridElement.GetComponent<RectTransform>().rect.height / 2);
            y2 = gridElement.transform.position.y + (gridElement.GetComponent<RectTransform>().rect.height / 2);

            if (tilePosition.x >= x1 && tilePosition.x <= x2 && tilePosition.y >= y1 && tilePosition.y <= y2)
            {
                //Debug.Log("Grid Element Found");
                return gridElement.gameObject;
            }
        }

        foreach (GridElement gridElement in playerGrid)
        {
            x1 = gridElement.transform.position.x - (gridElement.GetComponent<RectTransform>().rect.width / 2);
            x2 = gridElement.transform.position.x + (gridElement.GetComponent<RectTransform>().rect.width / 2);

            y1 = gridElement.transform.position.y - (gridElement.GetComponent<RectTransform>().rect.height / 2);
            y2 = gridElement.transform.position.y + (gridElement.GetComponent<RectTransform>().rect.height / 2);

            if (tilePosition.x >= x1 && tilePosition.x <= x2 && tilePosition.y >= y1 && tilePosition.y <= y2)
            {
                //Debug.Log("Grid Element Found");
                return gridElement.gameObject;
            }
        }

        //Debug.Log("Grid Element Not Found");
        return null;
    }

    
    public bool EnoughSpace()
    {
        // Debug.Log("Enough Space");
        
        foreach (TilePicked tilePicked in GameManager.Instance.dragTiles)
        {
            GameObject targetGridElementGameObject = FindGridElement(new Vector2(tilePicked.tile.transform.position.x + (GameManager.Instance.dragTiles.IndexOf(tilePicked) * xAxisDistance), tilePicked.tile.transform.position.y));

            if ((targetGridElementGameObject == null) || (!tilePicked.sourceGridElement.playerGrid && targetGridElementGameObject.GetComponent<GridElement>().playerGrid))
            {
                return false;
            }
            else
            {
                if (targetGridElementGameObject.GetComponent<GridElement>().tileStatus == TileStatus.Filled)
                {
                    foreach (TilePicked _tile in GameManager.Instance.dragTiles)
                    {
                        if (targetGridElementGameObject.GetComponent<GridElement>().tile?.name == _tile.tile.name)
                            return true;
                    }

                    return false;
                }
            }
        }

        return true;
    }

    public Tile NextTile(string currentTileName)
    {
        // Debug.Log("Next Tile");
        
        foreach (GridElement gridElement in playerGrid)
        {
            if (currentTileName == gridElement.tile?.name && playerGrid.IndexOf(gridElement) != (playerGrid.Count - 1))
            {
                return playerGrid[playerGrid.IndexOf(gridElement) + 1].tile;
            }
        }

        foreach (GridElement gridElement in grid)
        {
            if (currentTileName == gridElement.tile?.name && grid.IndexOf(gridElement) != (grid.Count - 1))
            {
                return grid[grid.IndexOf(gridElement) + 1].tile;
            }
        }
        return null;
    }

    public void UpdateGridElementTileStatus(string tileName)
    {
        // Debug.Log("Update Grid Element Tile Status");
        
        foreach (GridElement gridElement in playerGrid)
        {
            if (tileName == gridElement.tile?.name)
            {
                gridElement.tileStatus = TileStatus.Moved;
                return;
            }
        }

        foreach (GridElement gridElement in grid)
        {
            if (tileName == gridElement.tile?.name)
            {
                gridElement.tileStatus = TileStatus.Moved;
                return;
            }
        }
    }

    public bool CorrectSequenceOnTable()
    {
        // Debug.Log("Correct Sequence On Table");
        
        if (GameManager.Instance.movedTiles.Count == 0)
        {
            //Debug.Log("Sequence Followed Log: Player didn't added any tile on board");
            return false;
        }

        bool gridEmpty = true;

        foreach (GridElement gridElement in grid)
        {
            if (gridElement.tileStatus == TileStatus.Filled)
            {
                gridEmpty = false;
                break;
            }
        }
        
        if (!gridEmpty)
        {
            for (int i = 0; i < grid.Count; i++)
            {
                if (grid[i].tileStatus == TileStatus.Filled)
                {
                    int j = 1;
                    while (true)
                    {
                        //add first tile
                        if (totalTiles.Count == 0)
                        {
                            totalTiles.Add(grid[i].tile);
                        }
                        else
                        {
                            //if it excedes the grid
                            if ((i + j) > (grid.Count - 1))
                            {
                                break;
                            }

                            if (grid[i + j].tileStatus == TileStatus.Filled && grid[i + j].transform.position.y == totalTiles[0].transform.position.y)
                            {
                                totalTiles.Add(grid[i + j].tile);
                                j++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (totalTiles.Count >= 3)
                    {
                        tempTiles.Add(totalTiles[0]);

                        if (TilesInSequence())
                        {
                            //Debug.Log("Sequence Followed Log: Tiles in sequence");
                            i += totalTiles.Count - 1;
                            ClearTempTileLists();
                        }
                        else
                        {
                            //Debug.Log("Sequence Followed Log: Tiles not in sequence");
                            ClearTempTileLists();
                            
                            switch (GameManager.Instance.gameMode)
                            {
                                case GameMode.Rami_31:
                                    UIManager.Instance.UIScreensReferences[GameScreens.OnlineGamePlayScreen].GetComponent<OnlineGamePlayScreenHandler>().DisplayMessage("The board is invalid");
                                    break;
                
                                case GameMode.Rami_Annette:
                                    UIManager.Instance.UIScreensReferences[GameScreens.RamiAnnetteScreen].GetComponent<RamiAnnetteGameplayScreenHandler>().DisplayMessage("The board is invalid");
                                    break;
            
                                case GameMode.Rami_51:
                                    break;
                            }
                            
                            
                            return false;
                        }
                    }
                    else
                    {
                        Debug.Log("Sequence Followed Log: Total tiles are less than 3");
                        foreach (var item in totalTiles)
                        {
                            Debug.Log("Tile: " + item.name);
                        }
                        
                        ClearTempTileLists();
                        switch (GameManager.Instance.gameMode)
                        {
                            case GameMode.Rami_31:
                                UIManager.Instance.UIScreensReferences[GameScreens.OnlineGamePlayScreen].GetComponent<OnlineGamePlayScreenHandler>().DisplayMessage("The board is invalid");
                                break;
                
                            case GameMode.Rami_Annette:
                                UIManager.Instance.UIScreensReferences[GameScreens.RamiAnnetteScreen].GetComponent<RamiAnnetteGameplayScreenHandler>().DisplayMessage("The board is invalid");
                                break;
            
                            case GameMode.Rami_51:
                                break;
                        }
                        return false;
                    }
                }
            }

            //Debug.Log("Sequence Followed Log: Return True");
            //GameManager.Instance.movedTiles.Clear();
            return true;
        }

        //Debug.Log("Sequence Followed Log: Grid is Empty, return False");
        return false;
    }

    public bool SequenceFollowed()
    {
        // Debug.Log("Sequence Followed");
        
        GameObject targetGridElementGameObject = FindGridElement(GameManager.Instance.dragTiles[0].tile.transform.position);

        //if target grid element is player's grid return true
        if (targetGridElementGameObject.GetComponent<GridElement>().playerGrid)
        {
            //Debug.Log("Sequence Followed Log: Player's Grid");
            return true;
        }

        for (int i = 0; i < grid.Count; i++)
        {
            if (grid[i] == targetGridElementGameObject.GetComponent<GridElement>())
            {
                //adding tiles present before if they exits
                int j = 1;
                while (true)
                {
                    //skip this if it's the first grid element
                    if ((i - j) < 0)
                    {
                        //Debug.Log("Sequence Followed Log: Skip First");
                        break;
                    }

                    if (grid[i - j].tileStatus == TileStatus.Filled && grid[i - j].transform.position.y == targetGridElementGameObject.transform.position.y)
                    {
                        totalTiles.Insert(0, grid[i - j].tile);
                        j++;
                    }
                    else
                    {
                        break;
                    }
                }

                //adding draggged tiles
                foreach (TilePicked tilePicked in GameManager.Instance.dragTiles)
                {
                    totalTiles.Add(tilePicked.tile);
                }

                j = GameManager.Instance.dragTiles.Count;
                //adding tiles present after if they exits
                while (true)
                {
                    //skip this if it's the last grid element
                    if ((i + j) > (grid.Count - 1))
                    {
                        //Debug.Log("Sequence Followed Log: Skip last");
                        break;
                    }

                    if (grid[i + j].tileStatus == TileStatus.Filled && grid[i + j].transform.position.y == targetGridElementGameObject.transform.position.y)
                    {
                        totalTiles.Add(grid[i + j].tile);
                        j++;
                    }
                    else
                    {
                        break;
                    }
                }

                //Debug.Log("Sequence Followed Log: Total tiles: " + totalTiles.Count);
                tempTiles.Add(totalTiles[0]);
                return TilesInSequence();
            }
        }

        return true;
    }

    public bool TilesInSequence()
    {
        // Debug.Log("Tiles in Sequence");
        
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
                TilesInSequence();
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
                    TilesInSequence();
                }
                else if (tempTiles[^2].tileColor == TileColor.Joker)
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
                if (tempTiles[^1].tileNumber == 1 && tempTiles.Count > 1 && tempTiles[^2].tileColor == TileColor.Joker)
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
            //check for next Joker Tile
            if (nextTile.tileColor == TileColor.Joker)
            {
                //check if current tile is not 13
                if (tempTiles[^1].tileColor != TileColor.Joker && tempTiles[^1].tileNumber != 13)
                {
                    tempTiles.Add(nextTile);
                    TilesInSequence();
                }
                //check if current tile is joker and second last is not 12 
                else if (tempTiles[^1].tileColor == TileColor.Joker && tempTiles[^2].tileNumber != 12)
                {
                    tempTiles.Add(nextTile);
                    TilesInSequence();
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
        else if (sequence == "Group")
        {
            //check for next Joker Tile
            if (nextTile.tileColor == TileColor.Joker && tempTiles.Count < 4)
            {
                tempTiles.Add(nextTile);
                TilesInSequence();
            }
            //if current tile is Joker
            else if (tempTiles[^1].tileColor == TileColor.Joker)
            {
                int compareIndex = 2;

                if (tempTiles[^2].tileColor == TileColor.Joker)
                    compareIndex = 3;

                CheckForGroupSequence(nextTile, compareIndex);
            }
            else
            {
                CheckForGroupSequence(nextTile, 1);
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

    private void CheckForRunsSequence(Tile nextTile, int compareIndex)
    {
        // Debug.Log("Check For Runs Sequence");
        
        if (nextTile.tileColor == tempTiles[^compareIndex].tileColor &&
            nextTile.tileNumber == (tempTiles[^compareIndex].tileNumber + compareIndex))
        {
            sequence = "Runs";
            tempTiles.Add(nextTile);
            TilesInSequence();
        }
    }

    private void CheckForGroupSequence(Tile nextTile, int compareIndex)
    {
        // Debug.Log("Check For Group Sequence");
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
                TilesInSequence();
            }
        }
    }

    public void ClearTempTileLists()
    {
        // Debug.Log("Clear Temp Tile Lists");
        
        sequence = null;
        tempTiles.Clear();
        totalTiles.Clear();
    }

    public void ResetGridManager()
    {
        // Debug.Log("Reset Grid");

        ClearTempTileLists();

        foreach (GridElement gridElement in grid)
        {
            gridElement.tileStatus = TileStatus.Vacant;
            gridElement.tile = null;
        }

        foreach (GridElement gridElement in playerGrid)
        {
            gridElement.tileStatus = TileStatus.Vacant;
            gridElement.tile = null;
        }
    }
}
