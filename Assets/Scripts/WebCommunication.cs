using UnityEngine;

public class WebCommunication : MonoBehaviour
{
    public void CreatePublicRoom(string roomKey)
    {
        Debug.Log("Message received from Web. Room Key: " + roomKey);
        UIManager.Instance.UIScreensReferences[GameScreens.MainScreen].GetComponent<MainScreenHandler>().JoinPublicRoom(roomKey);
    }
}
