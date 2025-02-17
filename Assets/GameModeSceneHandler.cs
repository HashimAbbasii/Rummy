using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeSceneHandler : MonoBehaviour
{
    public void OnRumi31ModeClick()
    {
        GameManager.Instance.SetGameMode(GameMode.Rami_31);
    }

    public void OnRamiAnnetteModeClick()
    {
        GameManager.Instance.SetGameMode(GameMode.Rami_Annette);
    }
}
