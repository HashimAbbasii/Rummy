using RTLTMPro;
using UnityEngine;

public class ChatMessage : MonoBehaviour
{
    public RTLTextMeshPro messageText;

    public void OnSendMessageButtonClick()
    {
        switch (GameManager.Instance.gameMode)
        {
            case GameMode.Rami_31:
                UIManager.Instance.UIScreensReferences[GameScreens.OnlineGamePlayScreen].GetComponent<OnlineGamePlayScreenHandler>().SendChatMessage(messageText.text);
                break;
                
            case GameMode.Rami_Annette:
                UIManager.Instance.UIScreensReferences[GameScreens.RamiAnnetteScreen].GetComponent<RamiAnnetteGameplayScreenHandler>().OnConfirmButtonClick();
                break;
            
            case GameMode.Rami_51:
                break;
        }
        
       
    }
}
