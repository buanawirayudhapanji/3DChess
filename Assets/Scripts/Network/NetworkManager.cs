using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using ExitGames.Client.Photon;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private static NetworkManager instance;
    private bool isQuickMatch = false;

    [Header("UI")]
    public TMP_Text roomCodeText;
    public TMP_InputField roomCodeInput;
    public UIManager uiManager;

    private bool isReady = false;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);
    }

private void Start()
{
    if (PhotonNetwork.IsConnected)
    {
        Debug.Log(
            "ALREADY CONNECTED");

        PhotonNetwork.JoinLobby();

        return;
    }

    Debug.Log(
        "CONNECTING TO PHOTON...");

    PhotonNetwork.AutomaticallySyncScene = true;

    PhotonNetwork.ConnectUsingSettings();
}

public override void OnConnectedToMaster()
{
    Debug.Log(
        "CONNECTED TO MASTER");

    Debug.Log(
        "JOINING LOBBY");

    PhotonNetwork.JoinLobby();
}

public override void OnJoinedLobby()
{
    Debug.Log(
        "JOINED LOBBY");

    isReady = true;

    Debug.Log(
        "IS READY = TRUE");
}

public void CreateRoom()
{

    isQuickMatch = false;
    Debug.Log("CREATE ROOM CLICKED");
    Debug.Log("IS READY = " + isReady);
    Debug.Log("IN LOBBY = " + PhotonNetwork.InLobby);

    if (!PhotonNetwork.IsConnectedAndReady)
    {
        Debug.LogWarning("Belum terhubung ke Master Server.");
        return;
    }
    string roomName =
        Random.Range(
            100000,
            999999).ToString();

    RoomOptions roomOptions =
        new RoomOptions();

    roomOptions.MaxPlayers = 2;

    Hashtable roomData =
        new Hashtable();

    roomData["Time"] =
        GameSettings.matchTimeMinutes;

    roomOptions.CustomRoomProperties =
        roomData;
    
    roomOptions.CustomRoomPropertiesForLobby =
    new string[]
    {
        "Time"
    };

    PhotonNetwork.CreateRoom(
        roomName,
        roomOptions);

    Debug.Log(
        "CREATING ROOM : "
        + roomName);

    Debug.Log(
        "ROOM TIME = "
        + GameSettings.matchTimeMinutes);
}

    public void JoinRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Belum terhubung ke Master Server.");
            return;
        }

        string roomCode =
            roomCodeInput.text.Trim();

        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogWarning(
                "Room code kosong.");

            return;
        }

        Debug.Log(
            "TRY JOIN ROOM : "
            + roomCode);

        PhotonNetwork.JoinRoom(
            roomCode);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log(
            "ROOM CREATED");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(
            "JOINED ROOM");

        Debug.Log(
            "ROOM NAME = "
            + PhotonNetwork.CurrentRoom.Name);

        if (PhotonNetwork.CurrentRoom
            .CustomProperties
            .ContainsKey("Time"))
        {
            object value =
                PhotonNetwork.CurrentRoom
                .CustomProperties["Time"];

            GameSettings.matchTimeMinutes =
                System.Convert.ToInt32(value);

            Debug.Log(
                "ROOM TIME = "
                + GameSettings.matchTimeMinutes);
        }

        if (roomCodeText != null)
        {
            roomCodeText.text =
                PhotonNetwork.CurrentRoom.Name;
        }

        if (uiManager != null)
        {
            if (!isQuickMatch)
            {
                uiManager.ShowWaitingRoom();
            }
        }
    }

    public override void OnPlayerEnteredRoom(
        Player newPlayer)
    {
        Debug.Log(
            "PLAYER JOINED : "
            + newPlayer.NickName);

        Debug.Log(
            "PLAYER COUNT : "
            + PhotonNetwork.CurrentRoom.PlayerCount);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            Debug.Log(
                "LOADING GAME SCENE");

            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            PhotonNetwork.LoadLevel(
                "Game");
        }
    }

    public override void OnCreateRoomFailed(
        short returnCode,
        string message)
    {
        Debug.LogError(
            "CREATE ROOM FAILED : "
            + message);
    }

    public override void OnJoinRoomFailed(
        short returnCode,
        string message)
    {
        Debug.LogError(
            "JOIN ROOM FAILED : "
            + message);
    }

public override void OnDisconnected(
    DisconnectCause cause)
{
    Debug.LogWarning(
        "DISCONNECTED : "
        + cause);

    isReady = false;
    isQuickMatch = false;
}

    public bool IsWhitePlayer()
    {
        return PhotonNetwork.LocalPlayer.ActorNumber == 1;
    }

    public void RefreshUIManager()
    {
        uiManager =
            FindFirstObjectByType<UIManager>();

        Debug.Log(
            "UI MANAGER REFRESHED = "
            + uiManager);
    }

    public void LeaveRoom()
    {
        isQuickMatch = false;

        PhotonNetwork.LeaveRoom();
    }

    public void CancelWaitingRoom()
    {
        isQuickMatch = false;
        Debug.Log("CANCEL WAITING ROOM");

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else if (uiManager != null)
        {
            uiManager.ShowPlayMenu();
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("LEFT ROOM");
        isQuickMatch = false;
        if (uiManager != null)
        {
            uiManager.ShowPlayMenu();
        }
    }

    public void QuickMatch()
{
    Debug.Log(
        "QUICK MATCH CLICKED");

    Debug.Log(
        "READY = "
        + isReady);

    Debug.Log(
        "CONNECTED = "
        + PhotonNetwork.IsConnectedAndReady);

    Debug.Log(
        "IN LOBBY = "
        + PhotonNetwork.InLobby);

    Debug.Log(
        "MATCH TIME = "
        + GameSettings.matchTimeMinutes);

    isQuickMatch = true;

    if (uiManager != null)
    {
        uiManager.ShowSearchingMatch();
    }
    if (!PhotonNetwork.IsConnectedAndReady)
    {
        Debug.LogWarning("Belum terhubung ke Master Server.");
        return;
    }

    Hashtable expected =
        new Hashtable();

    expected["Time"] =
        GameSettings.matchTimeMinutes;

    PhotonNetwork.JoinRandomRoom(
        expected,
        2);

    Debug.Log(
        "SEARCHING QUICK MATCH : "
        + GameSettings.matchTimeMinutes
        + " MINUTES");
}
    public override void OnJoinRandomFailed(
        short returnCode,
        string message)
    {
        Debug.Log(
        "JOIN RANDOM FAILED");

    Debug.Log(
        "RETURN CODE = "
        + returnCode);

    Debug.Log(
        "MESSAGE = "
        + message);

    Debug.Log(
        "MATCH TIME = "
        + GameSettings.matchTimeMinutes);
        Debug.Log(
            "NO ROOM FOUND");

        string roomName =
            Random.Range(
                100000,
                999999).ToString();

        RoomOptions roomOptions =
            new RoomOptions();

        roomOptions.MaxPlayers = 2;

        Hashtable roomData =
            new Hashtable();

        roomData["Time"] =
            GameSettings.matchTimeMinutes;

        roomOptions.CustomRoomProperties =
            roomData;

        roomOptions.CustomRoomPropertiesForLobby =
            new string[]
            {
                "Time"
            };

        PhotonNetwork.CreateRoom(
            roomName,
            roomOptions);

        Debug.Log(
            "CREATED QUICK MATCH ROOM");
    }

    public override void OnLeftLobby()
    {
        Debug.Log(
            "LEFT LOBBY");
    }
}