using UnityEngine;
using TMPro;



public class GameUIManager : MonoBehaviour
{
    public TMP_Text gameStatusText;
    [Header("Popups")]
    public GameObject promotionPopup;
    public GameObject surrenderPopup;
    public GameObject drawRequestPopup;

    [Header("Result Panels")]
    public ResultPanelUI winPanel;   // Wadah untuk menang (Victory)
    public ResultPanelUI losePanel;  // Wadah untuk kalah (Lose)
    public ResultPanelUI drawPanel;  // Wadah untuk seri (Draw)

    public bool IsGameFinished => 
        (winPanel != null && winPanel.gameObject.activeSelf) ||
        (losePanel != null && losePanel.gameObject.activeSelf) ||
        (drawPanel != null && drawPanel.gameObject.activeSelf);

    private void Start()
    {
        // Putar musik saat memasuki game
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayGameMusic();
        }

        if (promotionPopup != null)
            promotionPopup.SetActive(false);

        if (surrenderPopup != null)
            surrenderPopup.SetActive(false);

        if (drawRequestPopup != null)
            drawRequestPopup.SetActive(false);

        if (winPanel != null)
            winPanel.gameObject.SetActive(false);

        if (losePanel != null)
            losePanel.gameObject.SetActive(false);

        if (drawPanel != null)
            drawPanel.gameObject.SetActive(false);
    }

    #region SHOW

    public void ShowPromotion()
    {
        if (promotionPopup != null)
            promotionPopup.SetActive(true);
    }

    public void ShowSurrender()
    {
        if (surrenderPopup != null)
            surrenderPopup.SetActive(true);
    }

    public void ShowDrawRequest()
    {
        if (drawRequestPopup != null)
            drawRequestPopup.SetActive(true);
    }

    public void ShowPlayer1Win(string pillText = "", string details = "")
    {
        HidePromotion();
        HideSurrender();
        HideDrawRequest();

        // Pemain lokal adalah Putih -> Mereka MENANG! Tampilkan winPanel (Victory)
        // Pemain lokal adalah Hitam -> Mereka KALAH! Tampilkan losePanel (Lose)
        bool localIsWhite = !Photon.Pun.PhotonNetwork.IsConnected || PlayerRole.IsWhitePlayer();
        ResultPanelUI activePanel = localIsWhite ? winPanel : losePanel;

        if (activePanel != null)
        {
            activePanel.Setup(pillText, localIsWhite ? "VICTORY" : "LOSE", details);
            activePanel.gameObject.SetActive(true);
        }

        if (MusicManager.Instance != null) 
        {
            MusicManager.Instance.StopMusic();
            if (localIsWhite) MusicManager.Instance.PlayWin();
            else MusicManager.Instance.PlayLose();
        }
    }

    public void ShowPlayer2Win(string pillText = "", string details = "")
    {
        HidePromotion();
        HideSurrender();
        HideDrawRequest();

        // Pemain lokal adalah Hitam -> Mereka MENANG! Tampilkan winPanel (Victory)
        // Pemain lokal adalah Putih -> Mereka KALAH! Tampilkan losePanel (Lose)
        bool localIsBlack = Photon.Pun.PhotonNetwork.IsConnected && !PlayerRole.IsWhitePlayer();
        ResultPanelUI activePanel = localIsBlack ? winPanel : losePanel;

        if (activePanel != null)
        {
            activePanel.Setup(pillText, localIsBlack ? "VICTORY" : "LOSE", details);
            activePanel.gameObject.SetActive(true);
        }

        if (MusicManager.Instance != null) 
        {
            MusicManager.Instance.StopMusic();
            if (localIsBlack) MusicManager.Instance.PlayWin();
            else MusicManager.Instance.PlayLose();
        }
    }

    public void ShowDraw(string pillText = "", string details = "")
    {
        HidePromotion();
        HideSurrender();
        HideDrawRequest();

        if (drawPanel != null)
        {
            drawPanel.Setup(pillText, "DRAW", details);
            drawPanel.gameObject.SetActive(true);
        }

        if (MusicManager.Instance != null) 
        {
            MusicManager.Instance.StopMusic();
            MusicManager.Instance.PlayDraw();
        }
    }

    #endregion

    #region HIDE

    public void HidePromotion()
    {
        if (promotionPopup != null)
            promotionPopup.SetActive(false);
    }

    public void HideSurrender()
    {
        if (surrenderPopup != null)
            surrenderPopup.SetActive(false);
    }

    public void HideDrawRequest()
    {
        if (drawRequestPopup != null)
            drawRequestPopup.SetActive(false);
    }

    public void HidePlayer1Win()
    {
        if (winPanel != null) winPanel.gameObject.SetActive(false);
        if (losePanel != null) losePanel.gameObject.SetActive(false);
    }

    public void HidePlayer2Win()
    {
        if (winPanel != null) winPanel.gameObject.SetActive(false);
        if (losePanel != null) losePanel.gameObject.SetActive(false);
    }

    public void HideDraw()
    {
        if (drawPanel != null)
            drawPanel.gameObject.SetActive(false);
    }

    #endregion

    #region DEBUG

    private void Update()
    {
        // Tombol 1 = Promotion
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShowPromotion();
        }

        // Tombol 2 = White Win
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ShowPlayer1Win("SKAKMAT!", "Sisi hitam terkena skakmat");
        }

        // Tombol 3 = Black Win
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ShowPlayer2Win("SKAKMAT!", "Sisi putih terkena skakmat");
        }

        // Tombol 4 = Draw
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ShowDraw("REMIS!", "Persetujuan Bersama");
        }

        // Tombol 0 = Tutup Semua
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            HidePromotion();
            HideSurrender();
            HideDrawRequest();
            HidePlayer1Win();
            HidePlayer2Win();
            HideDraw();
        }
    }

    #endregion
    public void SelectQueen()
    {
        Promote(
            PromotionType.Queen);
    }

    public void SelectRook()
    {
        Promote(
            PromotionType.Rook);
    }

    public void SelectBishop()
    {
        Promote(
            PromotionType.Bishop);
    }

    public void SelectKnight()
    {
        Promote(
            PromotionType.Knight);
    }

    private void Promote(
        PromotionType type)
    {
        BoardManager boardManager =
            FindFirstObjectByType<BoardManager>();

        if (boardManager == null)
            return;

        ChessPiece pawn =
            boardManager.GetPromotionPawn();

        if (pawn == null)
            return;

        GameNetworkManager.Instance.SendPromotion(
            pawn.currentX,
            pawn.currentY,
            type);
        SetStatus(
            "Promotion Complete");
        HidePromotion();
    }
    public void SetStatus(string message)
    {
        if (gameStatusText != null)
        {
            gameStatusText.text = message;
        }

        Debug.Log("STATUS : " + message);
    }
}