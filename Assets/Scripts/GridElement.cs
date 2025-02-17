using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class GridElement : MonoBehaviour 
{
    public string gridName;
    public TileStatus tileStatus;
    [FormerlySerializedAs("playerGird")] public bool playerGrid;
    public Tile tile;

    public void Fill(Tile _tile)
    {
        tileStatus = TileStatus.Filled;
        tile = _tile;
    }

    public void Clear()
    {
        tileStatus = TileStatus.Vacant;
        tile = null;
    }
}

public enum TileStatus
{
    Vacant,
    Moved,
    Filled,
}
