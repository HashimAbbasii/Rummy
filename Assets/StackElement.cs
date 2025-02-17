using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackElement : MonoBehaviour
{
    public List<Tile> tileStack = new List<Tile>();

    public void Fill(Tile _tile)
    {
        tileStack.Add(_tile);
    }

    public Tile Remove()
    {
        var tempTile = tileStack[^1];
        tileStack.RemoveAt(tileStack.Count - 1);
        return tempTile;
    }
}
