using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class DatabaseManager : MonoBehaviour
{
    
    private string backendURL = "https://rummy777-e2631948f8d9.herokuapp.com/api/";

    private static DatabaseManager _instance;
    public static DatabaseManager Instance
    {
        get => _instance;
        private set => _instance = value;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Login(Login loginData, Action<Login, UnityWebRequest> onCompleted)
    {
        StartCoroutine(LoginCoroutine(loginData, onCompleted));
    }

    private IEnumerator LoginCoroutine(Login loginData, Action<Login, UnityWebRequest> onCompleted)
    {
        using (UnityWebRequest request = new (backendURL + "auth/loginapp", "POST"))
        {
            string jsonBody = JsonConvert.SerializeObject(loginData);
            byte[] rawBody = new System.Text.UTF8Encoding().GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            //request.SetRequestHeader("Authorization", "");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API Log: Logged In Successfully!");
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(loginData, request);
            }
            else
            {
                Debug.Log("API Log: Login Failed!");
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(loginData, request);
            }
        };
    }

    
    
    public void GetUserPoints(string userID, Action<string, UnityWebRequest> onCompleted)
    {
        StartCoroutine(GetUserPointsCoroutine(userID, onCompleted));
    }

    private IEnumerator GetUserPointsCoroutine(string userID, Action<string, UnityWebRequest> onCompleted)
    {
        using (UnityWebRequest request = new (backendURL + "user/p/" + userID, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API Log: User points fetched Successfully!");
                Debug.Log("API Log:  " + request.downloadHandler.text);
                onCompleted?.Invoke("", request);
            }
            else
            {
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke("", request);
            }
        };
    }

    public void CreatePrivateRoom(CreateRoomData createRoomData, Action<CreateRoomData, UnityWebRequest> onCompleted)
    {
        StartCoroutine(CreatePrivateRoomCoroutine(createRoomData, onCompleted));
    }

    private IEnumerator CreatePrivateRoomCoroutine(CreateRoomData createRoomData, Action<CreateRoomData, UnityWebRequest> onCompleted)
    {
        using (UnityWebRequest request = new UnityWebRequest(backendURL + "room/start", "POST"))
        {
            string jsonBody = JsonConvert.SerializeObject(createRoomData);
            byte[] rawBody = new System.Text.UTF8Encoding().GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            //request.SetRequestHeader("Authorization", "");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API Log: Room created Successfully!");
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(createRoomData, request);
            }
            else
            {
                Debug.Log("API Log: Room creation Failed!");
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(createRoomData, request);
            }
        };
    }

    public void JoinRoom(JoinRoomData joinRoomData, Action<JoinRoomData, UnityWebRequest> onCompleted)
    {
        StartCoroutine(JoinRoomCoroutine(joinRoomData, onCompleted));
    }

    private IEnumerator JoinRoomCoroutine(JoinRoomData joinRoomData, Action<JoinRoomData, UnityWebRequest> onCompleted)
    {
        using (UnityWebRequest request = new UnityWebRequest(backendURL + "room/join", "POST"))
        {
            string jsonBody = JsonConvert.SerializeObject(joinRoomData);
            byte[] rawBody = new System.Text.UTF8Encoding().GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            //request.SetRequestHeader("Authorization", "");

            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API Log: Room joined Successfully!");
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(joinRoomData, request);
            }
            else
            {
                Debug.Log("API Log: Room joining Failed!");
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(joinRoomData, request);
            }
        };
    }

    public void PlayAgain(PlayAgain playAgain, Action<PlayAgain, UnityWebRequest> onCompleted)
    {
        Debug.Log("PA Web Request");
        StartCoroutine(PlayAgainCoroutine(playAgain, onCompleted));
    }

    private IEnumerator PlayAgainCoroutine(PlayAgain playAgain, Action<PlayAgain, UnityWebRequest> onCompleted)
    {
        Debug.Log("PA Web Request Coroutine");
        using (UnityWebRequest request = new(backendURL + "room/rejoin", "POST"))
        {
            string jsonBody = JsonConvert.SerializeObject(playAgain);
            Debug.Log(jsonBody);
            byte[] rawBody = new System.Text.UTF8Encoding().GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            //request.SetRequestHeader("Authorization", "");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API Log: Room re-created Successfully!");
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(playAgain, request);
            }
            else
            {
                Debug.Log("API Log: Room re-creation Failed!");
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(playAgain, request);
            }
        };
    }

    public void GetRoomsList(Action<PublicRooms, UnityWebRequest> onCompleted)
    {
        StartCoroutine(GetRoomsListCoroutine(onCompleted));
    }

    public IEnumerator GetRoomsListCoroutine(Action<PublicRooms, UnityWebRequest> onCompleted)
    {
        using (UnityWebRequest request = new UnityWebRequest(backendURL + "room/public", "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            PublicRooms publicRooms = new();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API Log: Rooms List fetched Successfully!");
                Debug.Log("API Log:  " + request.downloadHandler.text);
                publicRooms = JsonConvert.DeserializeObject<PublicRooms>(request.downloadHandler.text);
                onCompleted?.Invoke(publicRooms, request);
            }
            else
            {
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(publicRooms, request);
            }
        };
    }

    public void GetPharases(Action<Pharases, UnityWebRequest> onCompleted)
    {
        StartCoroutine(GetPharasesCoroutine(onCompleted));
    }

    public IEnumerator GetPharasesCoroutine(Action<Pharases, UnityWebRequest> onCompleted)
    {
        using (UnityWebRequest request = new UnityWebRequest(backendURL + "phrases", "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            Pharases pharases = new();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API Log: Pharases fetched Successfully!");
                Debug.Log("API Log:  " + request.downloadHandler.text);
                pharases = JsonConvert.DeserializeObject<Pharases>(request.downloadHandler.text);
                onCompleted?.Invoke(pharases, request);
            }
            else
            {
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(pharases, request);
            }
        };
    }

    public void SendGameResult(GameResult gameResult, Action<GameResult, UnityWebRequest> onCompleted)
    {
        Debug.Log("Hello");
        StartCoroutine(SendGameResultCoroutine(gameResult, onCompleted));
    }

    private IEnumerator SendGameResultCoroutine(GameResult gameResult, Action<GameResult, UnityWebRequest> onCompleted)
    {
        using (UnityWebRequest request = new UnityWebRequest(backendURL + "room/over", "POST"))
        {
            string jsonBody = JsonConvert.SerializeObject(gameResult);
            byte[] rawBody = new System.Text.UTF8Encoding().GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            //request.SetRequestHeader("Authorization", "");

            
            
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API Log: Game result sent Successfully!");
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(gameResult, request);
            }
            else
            {
                Debug.Log("API Log: Error!");
                Debug.Log("API Log: " + request.downloadHandler.text);
                onCompleted?.Invoke(gameResult, request);
            }
        };
    }
}

public class Login
{
    public string phone;
    public string apppass;
}

public class CreateRoomData
{
    public string userId;
    public string roomCost;
    public string playersNum;
    public string mode;
}

public class JoinRoomData
{
    public string userId;
    public string code;
}

public class PlayAgain
{
    public string code;
    public string userId;
    public string[] players;
}

public class GameResult
{
    public string winner;
    public string id;
    public string status;
}

public struct Pharases
{
    public string[] data; 
}

public class Data
{
    public string _id;
    public string code;
    public int playersNum;
    public int standbyTime;
    public int roomCost;
    public float points;
    public string[] players;
    public string mode;
}

public class Response
{
    public Data data;
}

public class PublicRooms
{
    public Data[] data;
}
