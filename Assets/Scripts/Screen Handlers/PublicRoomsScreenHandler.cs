using RTLTMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PublicRoomsScreenHandler : MonoBehaviour
{
    public RTLTextMeshPro pointsText;
    public GameObject roomsContent;
    public GameObject loader;
    public PublicRoom room;
    [SerializeField] private List<GameObject> publicRoomsList;

    public GameObject joinPublicRoomPanel;

    private void OnEnable()
    {
        pointsText.text = PreferenceManager.Points.ToString();
        joinPublicRoomPanel.SetActive(false);
        loader.SetActive(true);
        DatabaseManager.Instance.GetRoomsList(OnGetRoomsListComplete);
    }

    private void OnGetRoomsListComplete(PublicRooms publicRooms, UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            loader.SetActive(false);
            foreach (var _room in publicRoomsList)
                Destroy(_room);

            publicRoomsList.Clear();

            for (int i = 0; i < publicRooms.data.Length; i++)
            {
                publicRoomsList.Add(Instantiate(room.gameObject, roomsContent.transform));
                publicRoomsList[^1].GetComponent<PublicRoom>().SetData(publicRooms.data[i].code, publicRooms.data[i].roomCost);
            }
        }
        else
        {
            DatabaseManager.Instance.GetRoomsList(OnGetRoomsListComplete);
        }
    }

    public void JoinPublicRoom(string roomCode)
    {
        joinPublicRoomPanel.SetActive(true); 
        joinPublicRoomPanel.GetComponent<CreateOrJoinPublicRoom>().roomKeyInputField.text = roomCode;
    }

    public void OnBackButtonClick()
    {
        UIManager.Instance.DisplaySpecificScreen(GameScreens.MainScreen);
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }
}
