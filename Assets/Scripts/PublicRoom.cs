using RTLTMPro;
using UnityEngine;

public class PublicRoom : MonoBehaviour
{
    private string roomCode;
    private int roomCost;
    public RTLTextMeshPro roomCodeText;
    public RTLTextMeshPro roomCostText;

    private void Start()
    {
        GameManager.Instance.languageSwitch.AddListener(Response);
    }

    private void Response()
    {
        roomCodeText.text = GameManager.Instance.Translate("Code: ") + roomCode;
        roomCostText.text = GameManager.Instance.Translate("Cost: ") + roomCost;
    }

    public void SetData(string _roomCode, int _roomCost)
    {
        roomCode = _roomCode;
        roomCost = _roomCost;
        roomCodeText.text = GameManager.Instance.Translate("Code: ") + _roomCode;
        roomCostText.text = GameManager.Instance.Translate("Cost: ") + _roomCost;
    }

    public void OnJoinButtonClick()
    {
        UIManager.Instance.UIScreensReferences[GameScreens.PublicRoomsScreen].GetComponent<PublicRoomsScreenHandler>().JoinPublicRoom(roomCode);
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

}
