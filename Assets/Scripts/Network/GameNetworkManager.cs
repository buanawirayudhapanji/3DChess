using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameNetworkManager : MonoBehaviourPunCallbacks
{
    public static GameNetworkManager Instance;

    private BoardManager boardManager;
    private GameUIManager gameUIManager;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        boardManager =
            FindFirstObjectByType<BoardManager>();

        gameUIManager =
            FindFirstObjectByType<GameUIManager>();

        Debug.Log("GAME NETWORK READY");
    }

    #region MOVE PIECE

    [PunRPC]
    public void RPC_MovePiece(
        int fromX,
        int fromY,
        int toX,
        int toY)
    {
        ChessPiece piece =
            boardManager.GetPiece(
                fromX,
                fromY);

        if (piece == null)
        {
            Debug.LogWarning(
                "PIECE NOT FOUND");

            return;
        }

        boardManager.ExecuteMove(
            piece,
            toX,
            toY);

        Debug.Log(
            $"RPC MOVE {fromX},{fromY} -> {toX},{toY}");
    }

    public void SendMove(
        int fromX,
        int fromY,
        int toX,
        int toY)
    {
        photonView.RPC(
            nameof(RPC_MovePiece),
            RpcTarget.All,
            fromX,
            fromY,
            toX,
            toY);
    }

    #endregion

    #region DEBUG TEST

    [PunRPC]
    private void RPC_TestPromotion()
    {
        gameUIManager.ShowPromotion();

        Debug.Log("PROMOTION TEST");
    }

    public void TestPromotion()
    {
        photonView.RPC(
            nameof(RPC_TestPromotion),
            RpcTarget.All);
    }

    [PunRPC]
    private void RPC_TestWhiteWin()
    {
        gameUIManager.ShowPlayer1Win();

        Debug.Log("WHITE WIN TEST");
    }

    public void TestWhiteWin()
    {
        photonView.RPC(
            nameof(RPC_TestWhiteWin),
            RpcTarget.All);
    }

    [PunRPC]
    private void RPC_TestBlackWin()
    {
        gameUIManager.ShowPlayer2Win();

        Debug.Log("BLACK WIN TEST");
    }

    public void TestBlackWin()
    {
        photonView.RPC(
            nameof(RPC_TestBlackWin),
            RpcTarget.All);
    }

    [PunRPC]
    private void RPC_TestDraw()
    {
        gameUIManager.ShowDraw();

        Debug.Log("DRAW TEST");
    }

    public void TestDraw()
    {
        photonView.RPC(
            nameof(RPC_TestDraw),
            RpcTarget.All);
    }

    [PunRPC]
    private void RPC_TestCheck()
    {
        Debug.Log("CHECK TEST");
    }

    public void TestCheck()
    {
        photonView.RPC(
            nameof(RPC_TestCheck),
            RpcTarget.All);
    }

    #endregion

    #region SURRENDER

    [PunRPC]
    private void RPC_Surrender(
        bool surrenderedWhite)
    {
        if (gameUIManager == null)
            return;

        if (gameUIManager.IsGameFinished)
        {
            Debug.Log("RPC_Surrender: Game already finished.");
            return;
        }

        if (surrenderedWhite)
        {
            Debug.Log("WHITE SURRENDER");

            gameUIManager.ShowPlayer2Win("MENYERAH!", "Sisi putih menyerah");
        }
        else
        {
            Debug.Log("BLACK SURRENDER");

            gameUIManager.ShowPlayer1Win("MENYERAH!", "Sisi hitam menyerah");
        }
    }

    public void Surrender()
    {
        if (gameUIManager != null && gameUIManager.IsGameFinished)
        {
            Debug.Log("Surrender: Game already finished.");
            return;
        }

        photonView.RPC(
            nameof(RPC_Surrender),
            RpcTarget.All,
            PlayerRole.IsWhitePlayer());
    }

    #endregion

    #region DRAW REQUEST

    [PunRPC]
    private void RPC_RequestDraw()
    {
        if (gameUIManager == null)
            return;

        Debug.Log("DRAW REQUEST RECEIVED");

        gameUIManager.ShowDrawRequest();

        gameUIManager.SetStatus(
            "Opponent Requested Draw");
    }

    public void RequestDraw()
    {
        photonView.RPC(
            nameof(RPC_RequestDraw),
            RpcTarget.Others);

        if (gameUIManager != null)
        {
            gameUIManager.SetStatus(
                "Waiting Draw Response...");
        }
    }

    [PunRPC]
    private void RPC_DrawAccepted()
    {
        if (gameUIManager == null)
            return;

        if (gameUIManager.IsGameFinished)
        {
            Debug.Log("RPC_DrawAccepted: Game already finished.");
            return;
        }

        Debug.Log("DRAW ACCEPTED");

        gameUIManager.HideDrawRequest();

        gameUIManager.ShowDraw("SERI!", "Persetujuan bersama (Draw by agreement)");
        gameUIManager.SetStatus(
            "Draw");
    }

    public void AcceptDraw()
    {
        photonView.RPC(
            nameof(RPC_DrawAccepted),
            RpcTarget.All);
    }

    [PunRPC]
    private void RPC_DrawRejected()
    {
        if (gameUIManager == null)
            return;

        Debug.Log("DRAW REJECTED");

        gameUIManager.HideDrawRequest();
    }

    public void RejectDraw()
    {
        photonView.RPC(
            nameof(RPC_DrawRejected),
            RpcTarget.All);
    }

    #endregion

   #region MAIN MENU

public void BackToMainMenu()
{
    Debug.Log(
        "LEAVING ROOM");

    PhotonNetwork.LeaveRoom();
}

public override void OnLeftRoom()
{
    Debug.Log(
        "GAME NETWORK ON LEFT ROOM");

    SceneManager.LoadScene(
        "MainMenu");
}

public override void OnPlayerLeftRoom(Player otherPlayer)
{
    Debug.Log("MUSUH KELUAR DARI ROOM!");

    if (gameUIManager != null)
    {
        if (gameUIManager.IsGameFinished)
        {
            Debug.Log("OnPlayerLeftRoom: Game is already finished, ignoring disconnection.");
            return;
        }

        gameUIManager.SetStatus("Opponent Disconnected");
        
        if (PlayerRole.IsWhitePlayer())
        {
            gameUIManager.ShowPlayer1Win("MUSUH KELUAR!", "Sisi hitam terputus");
        }
        else
        {
            gameUIManager.ShowPlayer2Win("MUSUH KELUAR!", "Sisi putih terputus");
        }
    }
}

public override void OnDisconnected(DisconnectCause cause)
{
    Debug.Log("GAME NETWORK DISCONNECTED : " + cause);
    SceneManager.LoadScene("MainMenu");
}

#endregion

    [PunRPC]
    private void RPC_PromotePawn(
        int x,
        int y,
        int promotionType)
    {
        ChessPiece pawn =
            boardManager.GetPiece(
                x,
                y);

        if (pawn == null)
            return;

        boardManager.SetPromotionPawn(
            pawn);

        boardManager.PromoteSelectedPawn(
            (PromotionType)promotionType);
    }

    public void SendPromotion(
        int x,
        int y,
        PromotionType type)
    {
        photonView.RPC(
            nameof(RPC_PromotePawn),
            RpcTarget.All,
            x,
            y,
            (int)type);
    }
}