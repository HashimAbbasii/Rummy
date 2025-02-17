using Newtonsoft.Json;
using RTLTMPro;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class CreateOrJoinPublicRoom : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public TMP_InputField userIDInputField;
    public TMP_InputField roomKeyInputField;
    public RTLTextMeshPro messageText;

    [HideInInspector] public bool requestSent = false;
    public GameObject loader;
    public GameObject buttonText;

    private void OnEnable()
    {
        requestSent = false;
        nameInputField.text = null;
        userIDInputField.text = PreferenceManager.UserID;
        userIDInputField.readOnly = true;
        roomKeyInputField.text = null;
        ClearMessage();
    }

    private void Update()
    {
        if (requestSent)
        {
            loader.SetActive(true);
            buttonText.SetActive(false);
        }
        else
        {
            loader.SetActive(false);
            buttonText.SetActive(true);
        }
    }

    public void OnJoinButtonClick()
    {
        if (string.IsNullOrEmpty(nameInputField.text))
        {
            DisplayMessage("Enter your name");
        }
        else if (string.IsNullOrEmpty(roomKeyInputField.text))
        {
            DisplayMessage("Enter room key");
        }
        else if (!requestSent)
        {
            requestSent = true;
            DatabaseManager.Instance.JoinRoom(new JoinRoomData() { userId = userIDInputField.text, code = roomKeyInputField.text }, OnPublicRoomJoinedComplete); 
        }

        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    private void OnPublicRoomJoinedComplete(JoinRoomData joinRoomData, UnityWebRequest request) 
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            GameManager.Instance.room = new Room
            {
                username = nameInputField.text,
                userID = joinRoomData.userId,
                roomKey = joinRoomData.code,
                roomCode = JsonConvert.DeserializeObject<Response>(request.downloadHandler.text).data._id,
                roomCost = JsonConvert.DeserializeObject<Response>(request.downloadHandler.text).data.roomCost,
                standByTime = JsonConvert.DeserializeObject<Response>(request.downloadHandler.text).data.standbyTime
            };

            NetworkManager.Instance.CreateOrJoinPublicRoom(GameManager.Instance.room.roomKey, JsonConvert.DeserializeObject<Response>(request.downloadHandler.text).data.playersNum);
        }
        else
        {
            requestSent = false;
            Dictionary<string, string> response = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
            DisplayMessage(response["message"]);
        }
    }

    public void OnCloseButtonClick()
    {
        gameObject.SetActive(false);
        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void DisplayMessage(string message)
    {
        messageText.text = GameManager.Instance.Translate(message);
        Invoke(nameof(ClearMessage), 5f);
    }

    public void ClearMessage()
    {
        messageText.text = null;
    }
}
