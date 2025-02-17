using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class StackManager : MonoBehaviour
{
    public int xAxisDistance;
    public int yAxisDistance;
    [HideInInspector] public string sequence = null;
    [HideInInspector] public List<Tile> totalTiles = new();
    [HideInInspector] public List<Tile> tempTiles = new();
    public Transform stackParent;
    public Transform playerGridParent;
    public GridElement gridElementPrefab;
    public StackElement stackElementPrefab;
    public List<GridElement> playerGrid;
    public StackElement stack;
    public Transform masterTileTransform;
    public Tile masterTile;
    // public const int TABLE_VERTICAL_GRID = 6;
    // public const int TABLE_HORIZONTAL_GRID = 20; 

    public const int PLAYER_VERTICAL_GRID = 2;
    public const int PLAYER_HORIZONTAL_GRID = 19;

    [HideInInspector] public bool isWinning;

    #region Singleton

    private static StackManager _instance;
    public static StackManager Instance
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


        stack = Instantiate(stackElementPrefab, stackParent);
        
        
        //generating player grid
        for (int y = 0; y < PLAYER_VERTICAL_GRID; y++)
        {
            xAxis = 0;
            for (int x = 0; x < PLAYER_HORIZONTAL_GRID; x++)
            {
                playerGrid.Add(Instantiate(gridElementPrefab));
                playerGrid[^1].gridName = "Player Grid " + playerGrid.Count.ToString();
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

        x1 = masterTileTransform.position.x - masterTileTransform.GetComponent<RectTransform>().rect.width / 2;
        x2 = masterTileTransform.position.x + masterTileTransform.GetComponent<RectTransform>().rect.width / 2;
        
        y1 = masterTileTransform.position.y - masterTileTransform.GetComponent<RectTransform>().rect.height / 2;
        y2 = masterTileTransform.position.y + masterTileTransform.GetComponent<RectTransform>().rect.height / 2;
        
        if (tilePosition.x >= x1 && tilePosition.x <= x2 && tilePosition.y >= y1 && tilePosition.y <= y2)
        {
            return masterTileTransform.gameObject;
        }
        
        x1 = stackParent.position.x - stackParent.GetComponent<RectTransform>().rect.width / 2;
        x2 = stackParent.position.x + stackParent.GetComponent<RectTransform>().rect.width / 2;
        
        y1 = stackParent.position.y - stackParent.GetComponent<RectTransform>().rect.height / 2;
        y2 = stackParent.position.y + stackParent.GetComponent<RectTransform>().rect.height / 2;

        if (tilePosition.x >= x1 && tilePosition.x <= x2 && tilePosition.y >= y1 && tilePosition.y <= y2)
        {
            return stack.gameObject;
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
    
    private Vector2 GetNormalizedPosition(Vector3 position, RectTransform rectTransform, float scaleFactor)
    {
        Vector2 normalizedPos = new Vector2(
            (position.x - rectTransform.rect.width * 0.5f) / rectTransform.rect.width,
            (position.y - rectTransform.rect.height * 0.5f) / rectTransform.rect.height
        );
        return normalizedPos / scaleFactor;
    }

    private Vector2 GetNormalizedSize(RectTransform rectTransform, float scaleFactor)
    {
        return new Vector2(rectTransform.rect.width, rectTransform.rect.height) / scaleFactor;
    }

    public void UpdateElementTileStatus(string tileName)
    {
        foreach (GridElement gridElement in playerGrid)
        {
            if (tileName == gridElement.tile?.name)
            {
                gridElement.tileStatus = TileStatus.Moved;
                return;
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

    public void AddTile(Tile tile)
    {
        tile.transform.SetAsLastSibling();
        tile.GetComponent<BoxCollider2D>().enabled = true;
        if (stack.tileStack.Count > 0)
        {
            stack.tileStack[^1].GetComponent<BoxCollider2D>().enabled = false;
        }
        stack.tileStack.Add(tile);
        // var e = stack.transform.eulerAngles;
        // e.z = Random.Range(-30f, 30f);
        // stack.transform.eulerAngles = e;
        tile.transform.LeanTransform(stack.transform, 0.2f);
        tile.boardTile = true;
    }

    public void RemoveTile(Tile tile)
    {
        stack.tileStack.Remove(tile);
        if (stack.tileStack.Count > 0)
        {
            stack.tileStack[^1].GetComponent<BoxCollider2D>().enabled = true;
        }
    }
    
    public void ResetStackManager()
    {
        // Debug.Log("Reset Grid");

        
        ClearTempTileLists();


        foreach (GridElement gridElement in playerGrid)
        {
            gridElement.tileStatus = TileStatus.Vacant;
            gridElement.tile = null;
        }
        
        stack.tileStack.Clear();

        if (masterTile)
            masterTile.isMasterTile = false;

        masterTileTransform.GetComponent<GridElement>().Clear();
        masterTile = null;
    }
}
