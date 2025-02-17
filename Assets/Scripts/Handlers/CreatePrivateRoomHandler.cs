using System;
using Newtonsoft.Json;
using RTLTMPro;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CreatePrivateRoomHandler : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public TMP_InputField userIDInputField;
    public TMP_InputField roomCostInputField;
    public int numberOfPlayers;
    public Toggle twoPlayersToggle, threePlayersToggle, fourPlayersToggle;
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
        roomCostInputField.text = null;
        HandleToggles(2);
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

    public void OnToggleClick(int _numberOfPlayers)
    {
        HandleToggles(_numberOfPlayers);
    }

    private void HandleToggles(int _numberOfPlayers)
    {
        numberOfPlayers = _numberOfPlayers;
        if (_numberOfPlayers == 2)
        {
            twoPlayersToggle.isOn = true;
            threePlayersToggle.isOn = false;
            fourPlayersToggle.isOn = false;
        }
        else if (_numberOfPlayers == 3)
        {
            twoPlayersToggle.isOn = false;
            threePlayersToggle.isOn = true;
            fourPlayersToggle.isOn = false;
        }
        else if (_numberOfPlayers == 4)
        {
            twoPlayersToggle.isOn = false;
            threePlayersToggle.isOn = false;
            fourPlayersToggle.isOn = true;
        }

        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    public void OnCreateButtonClick()
    {
        if (string.IsNullOrEmpty(nameInputField.text))
        {
            DisplayMessage("Enter your name");
        }
        else if (string.IsNullOrEmpty(roomCostInputField.text))
        {
            DisplayMessage("Enter room cost");
        }
        else if (ulong.Parse(roomCostInputField.text) < 20)
        {
            DisplayMessage("Minimum room cost is 20");
        }
        else if(!requestSent)
        {
            requestSent = true;
            DatabaseManager.Instance.CreatePrivateRoom(new CreateRoomData()
            {
                userId = userIDInputField.text, roomCost = roomCostInputField.text, playersNum = numberOfPlayers.ToString(), mode = GameManager.Instance.gameMode.ToString()
            }, OnPrivateRoomCreatedComplete);
        }

        AudioManager.Instance.PlayAudio(AudioName.ButtonAudio);
    }

    private void OnPrivateRoomCreatedComplete(CreateRoomData createRoomData, UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            GameManager.Instance.room = new Room
            {
                username = nameInputField.text,
                userID = createRoomData.userId,
                roomKey = JsonConvert.DeserializeObject<Response>(request.downloadHandler.text).data.code,
                roomCode = JsonConvert.DeserializeObject<Response>(request.downloadHandler.text).data._id,
                roomCost = JsonConvert.DeserializeObject<Response>(request.downloadHandler.text).data.roomCost,
                standByTime = 30
            };

            NetworkManager.Instance.CreatePrivateRoom(GameManager.Instance.room.roomKey, numberOfPlayers);
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

    private string AutoGenerateRoomKey()
    {
        string[] alphabets = new string[26] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        string[] specialCharacter = new string[6] { "#", "@", "$", "%", "&", "*" };
        string roomKey = Random.Range(10, 1000) + alphabets[Random.Range(0, alphabets.Length)] + alphabets[Random.Range(0, alphabets.Length)] + specialCharacter[Random.Range(0, specialCharacter.Length)] + "room" + Random.Range(10, 1000);
        return roomKey;
    }
}
