using Photon.Pun;
using UnityEngine;

public class PlayerRole : MonoBehaviour
{
    public static bool IsWhitePlayer()
    {
        Debug.Log(
            "IS MASTER = "
            + PhotonNetwork.IsMasterClient);

        return PhotonNetwork.IsMasterClient;
    }
}