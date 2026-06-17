using UnityEngine;

public class PieceSelector : MonoBehaviour
{
    private Camera mainCamera;

    [SerializeField]
    private BoardManager boardManager;

    private ChessPiece selectedPiece;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectPiece();
        }
    }

    private void SelectPiece()
    {
        Ray ray =
            mainCamera.ScreenPointToRay(
                Input.mousePosition);

        RaycastHit[] hits =
            Physics.RaycastAll(ray);
            
        Debug.Log("Klik! Jumlah objek yang tertembak Raycast: " + hits.Length);

        // ==========================
        // PRIORITAS 1
        // CEK HIGHLIGHT DULU
        // ==========================
        foreach (RaycastHit hit in hits)
        {
            MoveHighlight highlight =
                hit.collider.GetComponentInParent<MoveHighlight>();

            if (highlight != null)
            {
                MoveSelectedPiece(
                    highlight.targetX,
                    highlight.targetY);

                return;
            }
        }

        // ==========================
        // PRIORITAS 2
        // CEK BIDAK
        // ==========================
        foreach (RaycastHit hit in hits)
        {
            ChessPiece piece =
                hit.collider.GetComponentInParent<ChessPiece>();

            if (piece == null)
                continue;
                
            Debug.Log(
                $"TERKLIK: {piece.name} | " +
                $"Bidak White: {piece.isWhite} | " +
                $"Player White: {PlayerRole.IsWhitePlayer()} | " +
                $"Turn White: {boardManager.isWhiteTurn}");

            if (piece.isWhite != PlayerRole.IsWhitePlayer())
            {
                Debug.Log("Gagal: Anda mengklik bidak musuh (Beda warna dengan Player).");
                return; // <-- Ini membatalkan proses jika musuh keklik duluan
            }

            if (piece.isWhite != boardManager.isWhiteTurn)
            {
                Debug.Log("Gagal: Belum giliran warna ini jalan.");
                boardManager.ClearHighlights();
                return;
            }

            selectedPiece = piece;

            boardManager.ClearHighlights();

            ShowLegalMoves(piece);

            Debug.Log(
                $"{(piece.isWhite ? "White" : "Black")} {piece.pieceType} Berhasil Dipilih!"
            );

            return;
        }
    }

    private void ShowLegalMoves(
        ChessPiece piece)
    {
        switch (piece.pieceType)
        {
            case PieceType.Pawn:
                ShowPawnMoves(piece);
                break;

            case PieceType.Rook:
                ShowRookMoves(piece);
                break;
            case PieceType.Knight:
                ShowKnightMoves(piece);
                break;
            case PieceType.Bishop:
                ShowBishopMoves(piece);
                break;
            case PieceType.Queen:
                ShowQueenMoves(piece);
                break;
            case PieceType.King:
                ShowKingMoves(piece);
                break;
        }
    }

    private void ShowKingMoves(
    ChessPiece piece)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int targetX =
                    piece.currentX + x;

                int targetY =
                    piece.currentY + y;

                if (!boardManager.IsInsideBoard(
                        targetX,
                        targetY))
                {
                    continue;
                }

                ChessPiece target =
                    boardManager.GetPiece(
                        targetX,
                        targetY);

                // Kosong
                if (target == null)
                {
                    // CHECK VALIDATION
                    if (!boardManager.WouldMoveLeaveKingInCheck(
                            piece,
                            targetX,
                            targetY))
                    {
                        boardManager.ShowHighlight(
                            targetX,
                            targetY);
                    }
                }
                // Musuh
                else if (
                    target.isWhite != piece.isWhite)
                {
                    // CHECK VALIDATION
                    if (!boardManager.WouldMoveLeaveKingInCheck(
                            piece,
                            targetX,
                            targetY))
                    {
                        boardManager.ShowHighlight(
                            targetX,
                            targetY);
                    }
                }
            }
        }

        if (boardManager.CanCastleKingSide(
                piece))
        {
            boardManager.ShowHighlight(
                6,
                piece.currentY);
        }

        if (boardManager.CanCastleQueenSide(
            piece))
        {
            boardManager.ShowHighlight(
                2,
                piece.currentY);
        }
    }

    private void ShowPawnMoves(
        ChessPiece piece)
    {
        int direction =
            piece.isWhite ? 1 : -1;

        // ==========================
        // MAJU 1 LANGKAH
        // ==========================
        int oneStepY =
            piece.currentY + direction;

        if (boardManager.IsInsideBoard(
                piece.currentX,
                oneStepY)
            &&
            boardManager.IsTileEmpty(
                piece.currentX,
                oneStepY))
        {
            if (!boardManager.WouldMoveLeaveKingInCheck(
                    piece,
                    piece.currentX,
                    oneStepY))
            {
                boardManager.ShowHighlight(
                    piece.currentX,
                    oneStepY);
            }

            // ==========================
            // MAJU 2 LANGKAH
            // ==========================
            if (!piece.hasMoved)
            {
                int twoStepY =
                    piece.currentY +
                    (direction * 2);

                if (boardManager.IsInsideBoard(
                        piece.currentX,
                        twoStepY)
                    &&
                    boardManager.IsTileEmpty(
                        piece.currentX,
                        twoStepY))
                {
                    if (!boardManager.WouldMoveLeaveKingInCheck(
                            piece,
                            piece.currentX,
                            twoStepY))
                    {
                        boardManager.ShowHighlight(
                            piece.currentX,
                            twoStepY);
                    }
                }
            }
        }

        // ==========================
        // CAPTURE KIRI
        // ==========================
        int captureLeftX =
            piece.currentX - 1;

        int captureY =
            piece.currentY + direction;

        if (boardManager.IsInsideBoard(
                captureLeftX,
                captureY))
        {
            ChessPiece target =
                boardManager.GetPiece(
                    captureLeftX,
                    captureY);

            if (target != null &&
                target.isWhite != piece.isWhite)
            {
                // CHECK VALIDATION
                if (!boardManager.WouldMoveLeaveKingInCheck(
                        piece,
                        captureLeftX,
                        captureY))
                {
                    boardManager.ShowHighlight(
                        captureLeftX,
                        captureY);
                }
            }
        }

        // ==========================
        // CAPTURE KANAN
        // ==========================
        int captureRightX =
            piece.currentX + 1;

        if (boardManager.IsInsideBoard(
                captureRightX,
                captureY))
        {
            ChessPiece target =
                boardManager.GetPiece(
                    captureRightX,
                    captureY);

            if (target != null &&
                target.isWhite != piece.isWhite)
            {
                // CHECK VALIDATION
                if (!boardManager.WouldMoveLeaveKingInCheck(
                        piece,
                        captureRightX,
                        captureY))
                {
                    boardManager.ShowHighlight(
                        captureRightX,
                        captureY);
                }
            }
        }
    }

    private void ShowQueenMoves(
    ChessPiece piece)
    {
        ShowRookMoves(piece);

        ShowBishopMoves(piece);
    }

    private void ShowRookMoves(
        ChessPiece piece)
    {
        // Atas
        ShowRookDirection(
            piece,
            0,
            1);

        // Bawah
        ShowRookDirection(
            piece,
            0,
            -1);

        // Kanan
        ShowRookDirection(
            piece,
            1,
            0);

        // Kiri
        ShowRookDirection(
            piece,
            -1,
            0);
    }

    private void ShowRookDirection(
        ChessPiece piece,
        int dirX,
        int dirY)
    {
        int x =
            piece.currentX + dirX;

        int y =
            piece.currentY + dirY;

        while (
            boardManager.IsInsideBoard(
                x,
                y))
        {
            ChessPiece target =
                boardManager.GetPiece(
                    x,
                    y);

            // Petak kosong
            if (target == null)
            {
                // CHECK VALIDATION
                if (!boardManager.WouldMoveLeaveKingInCheck(
                        piece,
                        x,
                        y))
                {
                    boardManager.ShowHighlight(
                        x,
                        y);
                }
            }
            else
            {
                // Musuh
                if (target.isWhite != piece.isWhite)
                {
                    // CHECK VALIDATION
                    if (!boardManager.WouldMoveLeaveKingInCheck(
                            piece,
                            x,
                            y))
                    {
                        boardManager.ShowHighlight(
                            x,
                            y);
                    }
                }

                break;
            }

            x += dirX;
            y += dirY;
        }
    }
    private void ShowBishopDirection(
    ChessPiece piece,
    int dirX,
    int dirY)
    {
    int x =
        piece.currentX + dirX;

    int y =
        piece.currentY + dirY;

    while (
        boardManager.IsInsideBoard(
            x,
            y))
    {
        ChessPiece target =
            boardManager.GetPiece(
                x,
                y);

        if (target == null)
        {
            // CHECK VALIDATION
            if (!boardManager.WouldMoveLeaveKingInCheck(
                    piece,
                    x,
                    y))
            {
                boardManager.ShowHighlight(
                    x,
                    y);
            }
        }
        else
        {
            if (target.isWhite != piece.isWhite)
            {
                // CHECK VALIDATION
                if (!boardManager.WouldMoveLeaveKingInCheck(
                        piece,
                        x,
                        y))
                {
                    boardManager.ShowHighlight(
                        x,
                        y);
                }
            }

            break;
        }

        x += dirX;
        y += dirY;
    }
    }
    private void ShowKnightMoves(
    ChessPiece piece)
    {
        int[,] moves =
        {
            {  1,  2 },
            { -1,  2 },

            {  1, -2 },
            { -1, -2 },

            {  2,  1 },
            {  2, -1 },

            { -2,  1 },
            { -2, -1 }
        };

        for (int i = 0; i < 8; i++)
        {
            int x =
                piece.currentX +
                moves[i, 0];

            int y =
                piece.currentY +
                moves[i, 1];

            if (!boardManager.IsInsideBoard(
                    x,
                    y))
            {
                continue;
            }

            ChessPiece target =
                boardManager.GetPiece(
                    x,
                    y);

            // kosong
            if (target == null)
            {
                // CHECK VALIDATION
                if (!boardManager.WouldMoveLeaveKingInCheck(
                        piece,
                        x,
                        y))
                {
                    boardManager.ShowHighlight(
                        x,
                        y);
                }
            }
            // musuh
            else if (
                target.isWhite !=
                piece.isWhite)
            {
                // CHECK VALIDATION
                if (!boardManager.WouldMoveLeaveKingInCheck(
                        piece,
                        x,
                        y))
                {
                    boardManager.ShowHighlight(
                        x,
                        y);
                }
            }
        }
    }
    private void ShowBishopMoves(
    ChessPiece piece)
    {
        ShowBishopDirection(
            piece,
            1,
            1);

        ShowBishopDirection(
            piece,
            -1,
            1);

        ShowBishopDirection(
            piece,
            1,
            -1);

        ShowBishopDirection(
            piece,
            -1,
            -1);
    }

    private void MoveSelectedPiece(
        int targetX,
        int targetY)
    {
        if (selectedPiece == null)
        {
            Debug.Log(
                "SELECTED PIECE NULL");

            return;
        }

        // CASTLING
        // Castling sekarang dideteksi otomatis oleh ExecuteMove di BoardManager
        // melalui pengiriman SendMove di bawah ini.

        GameNetworkManager.Instance.SendMove(
        selectedPiece.currentX,
        selectedPiece.currentY,
        targetX,
        targetY);
    }
}