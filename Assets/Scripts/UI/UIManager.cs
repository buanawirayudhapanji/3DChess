using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Settings")]
    public Toggle blitzToggle;
    public Toggle standardToggle;
    public Toggle longTimeToggle;

    [Header("Pages")]
    public GameObject fr1MainMenu;
    public GameObject fr2Settings;
    public GameObject fr3PlayMenu;
    public GameObject fr4SearchingMatch;
    public GameObject fr5WaitingRoom;

    [Header("Room UI")]
    public TMP_Text roomCodeText;
    public TMP_InputField roomCodeInput;

    private void Start()
    {
        ShowMainMenu();

        // Putar musik Menu
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMenuMusic();
        }

        NetworkManager nm =
            FindFirstObjectByType<NetworkManager>();

        if (nm != null)
        {
            nm.uiManager = this;
            nm.roomCodeText = roomCodeText;
            nm.roomCodeInput = roomCodeInput;

            Debug.Log(
                "UI REFERENCES REGISTERED");

            // Ensure we join the lobby if we returned from a game
            if (Photon.Pun.PhotonNetwork.IsConnectedAndReady && !Photon.Pun.PhotonNetwork.InLobby)
            {
                Debug.Log("Connected to master but not in lobby, joining lobby...");
                Photon.Pun.PhotonNetwork.JoinLobby();
            }
        }
    }

    private void HideAllPages()
    {
        if (fr1MainMenu != null)
            fr1MainMenu.SetActive(false);

        if (fr2Settings != null)
            fr2Settings.SetActive(false);

        if (fr3PlayMenu != null)
            fr3PlayMenu.SetActive(false);

        if (fr4SearchingMatch != null)
            fr4SearchingMatch.SetActive(false);

        if (fr5WaitingRoom != null)
            fr5WaitingRoom.SetActive(false);
    }

    #region MAIN PAGE

    public void ShowMainMenu()
    {
        HideAllPages();

        if (fr1MainMenu != null)
        {
            fr1MainMenu.SetActive(true);
        }
    }

    public void ShowSettings()
    {
        HideAllPages();

        if (fr2Settings != null)
        {
            fr2Settings.SetActive(true);
        }
    }

    public void ShowPlayMenu()
    {
        HideAllPages();

        if (fr3PlayMenu != null)
        {
            fr3PlayMenu.SetActive(true);
        }

        Debug.Log(
            "Current Match Time = "
            + GameSettings.matchTimeMinutes
            + " Minutes");
    }

    public void ShowSearchingMatch()
    {
        HideAllPages();

        if (fr4SearchingMatch != null)
        {
            fr4SearchingMatch.SetActive(true);
        }
    }

    public void ShowWaitingRoom()
    {
        Debug.Log(
            "SHOW WAITING ROOM");

        HideAllPages();

        if (fr5WaitingRoom != null)
        {
            fr5WaitingRoom.SetActive(true);
        }
    }

    #endregion

    #region SETTINGS

    public void SaveSettings()
    {
        if (blitzToggle != null &&
            blitzToggle.isOn)
        {
            GameSettings.matchTimeMinutes = 5;
        }
        else if (standardToggle != null &&
                 standardToggle.isOn)
        {
            GameSettings.matchTimeMinutes = 15;
        }
        else if (longTimeToggle != null &&
                 longTimeToggle.isOn)
        {
            GameSettings.matchTimeMinutes = 30;
        }

        Debug.Log(
            "Saved Time = "
            + GameSettings.matchTimeMinutes
            + " Minutes");

        ShowMainMenu();
    }

    #endregion

    #region ROOM

    public void CreateRoom()
    {
        NetworkManager networkManager =
            FindFirstObjectByType<NetworkManager>();

        if (networkManager != null)
        {
            networkManager.CreateRoom();
        }
    }

    public void JoinRoom()
    {
        NetworkManager networkManager =
            FindFirstObjectByType<NetworkManager>();

        if (networkManager != null)
        {
            networkManager.JoinRoom();
        }
    }
    public void CancelWaitingRoom()
    {
        NetworkManager networkManager =
            FindFirstObjectByType<NetworkManager>();

        if (networkManager != null)
        {
            networkManager.CancelWaitingRoom();
        }
    }

    #endregion

    #region SYSTEM

    public void QuitGame()
    {
        Debug.Log(
            "Quit Game");

        Application.Quit();
    }

    #endregion


}