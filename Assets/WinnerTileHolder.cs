using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinnerTileHolder : MonoBehaviour
{
    public List<Transform> winnerTiles;

    public void TurnOffPanel()
    {
        gameObject.SetActive(false);
    }
}
