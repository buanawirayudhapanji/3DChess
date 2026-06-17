using UnityEngine;
using System.Collections.Generic;



public class BoardManager : MonoBehaviour
{
    [Header("Highlight")]
    public GameObject moveHighlightPrefab;

    private List<GameObject> activeHighlights =
        new List<GameObject>();

    [Header("Grid Settings")]
    public float tileSize = 1f;
    public Vector3 gridOffset;

    private Vector3[,] tilePositions =
        new Vector3[8, 8];

    private ChessPiece[,] board =
        new ChessPiece[8, 8];

    [Header("Turn")]
    public bool isWhiteTurn = true;

    [Header("White Pieces")]
    public GameObject whitePawn;
    public GameObject whiteRook;
    public GameObject whiteKnight;
    public GameObject whiteBishop;
    public GameObject whiteQueen;
    public GameObject whiteKing;

    [Header("Black Pieces")]
    public GameObject blackPawn;
    public GameObject blackRook;
    public GameObject blackKnight;
    public GameObject blackBishop;
    public GameObject blackQueen;
    public GameObject blackKing;

    private ChessPiece promotionPawn;
    
    private void Start()
    {
        UpdateTurnStatus();
        GenerateGrid();
        SpawnPieces();

        Debug.Log("WHITE TURN");
        ChessPiece king =
            FindKing(true);

        Debug.Log(
            "White King = " +
            king.name);
    }

    public ChessPiece FindKing(
    bool isWhite)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece =
                    board[x, y];

                if (piece == null)
                    continue;

                if (
                    piece.pieceType ==
                    PieceType.King
                    &&
                    piece.isWhite ==
                    isWhite)
                {
                    return piece;
                }
            }
        }

        return null;
    }
    public bool WouldMoveLeaveKingInCheck(
    ChessPiece piece,
    int targetX,
    int targetY)
    {
        int oldX = piece.currentX;
        int oldY = piece.currentY;

        ChessPiece capturedPiece =
            board[targetX, targetY];

        // Simulasi langkah
        board[oldX, oldY] = null;

        board[targetX, targetY] = piece;

        piece.currentX = targetX;
        piece.currentY = targetY;

        bool kingInCheck =
            IsKingInCheck(piece.isWhite);
        Debug.Log(
        $"SIMULATE {piece.name} -> {targetX},{targetY}");

        Debug.Log(
        $"KING INCHECK = {kingInCheck}");

        Debug.Log(
            $"Move To {targetX},{targetY} = {kingInCheck}");

        // Kembalikan posisi semula
        board[oldX, oldY] = piece;

        board[targetX, targetY] =
            capturedPiece;

        piece.currentX = oldX;
        piece.currentY = oldY;

        return kingInCheck;
    }

    public bool IsSquareAttacked(
    int targetX,
    int targetY,
    bool isWhiteKing)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece =
                    board[x, y];

                if (piece == null)
                    continue;

                // hanya cek musuh
                if (piece.isWhite == isWhiteKing)
                    continue;

                // sementara hanya Rook
                if (piece.pieceType ==
                    PieceType.Rook)
                {
                    if (CanRookAttack(
                        piece,
                        targetX,
                        targetY))
                    {
                        return true;
                    }
                }
                if (piece.pieceType ==
                    PieceType.Bishop)
                {
                    if (CanBishopAttack(
                        piece,
                        targetX,
                        targetY))
                    {
                        return true;
                    }
                }

                if (piece.pieceType ==
                    PieceType.Queen)
                {
                    if (
                        CanRookAttack(
                            piece,
                            targetX,
                            targetY)
                        ||
                        CanBishopAttack(
                            piece,
                            targetX,
                            targetY))
                    {
                        return true;
                    }
                }
                if (piece.pieceType ==
                    PieceType.Knight)
                {
                    if (CanKnightAttack(
                        piece,
                        targetX,
                        targetY))
                    {
                        return true;
                    }
                }
                if (piece.pieceType ==
                    PieceType.Pawn)
                {
                    if (CanPawnAttack(
                        piece,
                        targetX,
                        targetY))
                    {
                        return true;
                    }
                }
                if (piece.pieceType ==
                    PieceType.King)
                {
                    if (CanKingAttack(
                        piece,
                        targetX,
                        targetY))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool CanKingAttack(
    ChessPiece king,
    int targetX,
    int targetY)
    {
        int deltaX =
            Mathf.Abs(
                king.currentX -
                targetX);

        int deltaY =
            Mathf.Abs(
                king.currentY -
                targetY);

        return
            deltaX <= 1
            &&
            deltaY <= 1;
    }
    private bool CanRookAttack(
    ChessPiece rook,
    int targetX,
    int targetY)
    {
        // harus satu baris atau satu kolom
        if (rook.currentX != targetX &&
            rook.currentY != targetY)
        {
            return false;
        }

        int dirX =
            targetX > rook.currentX ? 1 :
            targetX < rook.currentX ? -1 : 0;

        int dirY =
            targetY > rook.currentY ? 1 :
            targetY < rook.currentY ? -1 : 0;

        int x =
            rook.currentX + dirX;

        int y =
            rook.currentY + dirY;

        while (x != targetX ||
            y != targetY)
        {
            if (board[x, y] != null)
                return false;

            x += dirX;
            y += dirY;
        }

        return true;
    }

    private bool CanPawnAttack(
    ChessPiece pawn,
    int targetX,
    int targetY)
    {
        int direction =
            pawn.isWhite ? 1 : -1;

        return
            (targetX ==
                pawn.currentX - 1
            &&
            targetY ==
                pawn.currentY + direction)
            ||
            (targetX ==
                pawn.currentX + 1
            &&
            targetY ==
                pawn.currentY + direction);
    }

    private bool CanKnightAttack(
    ChessPiece knight,
    int targetX,
    int targetY)
    {
        int deltaX =
            Mathf.Abs(
                knight.currentX -
                targetX);

        int deltaY =
            Mathf.Abs(
                knight.currentY -
                targetY);

        return
            (deltaX == 2 && deltaY == 1)
            ||
            (deltaX == 1 && deltaY == 2);
    }

    private bool CanBishopAttack(
    ChessPiece bishop,
    int targetX,
    int targetY)
    {
        int deltaX =
            Mathf.Abs(
                targetX -
                bishop.currentX);

        int deltaY =
            Mathf.Abs(
                targetY -
                bishop.currentY);

        if (deltaX != deltaY)
            return false;

        int dirX =
            targetX > bishop.currentX ? 1 : -1;

        int dirY =
            targetY > bishop.currentY ? 1 : -1;

        int x =
            bishop.currentX + dirX;

        int y =
            bishop.currentY + dirY;

        while (x != targetX &&
            y != targetY)
        {
            if (board[x, y] != null)
                return false;

            x += dirX;
            y += dirY;
        }

        return true;
    }

    public bool IsKingInCheck(
        bool isWhite)
    {
        ChessPiece king =
            FindKing(isWhite);

        if (king == null)
            return false;

        return IsSquareAttacked(
            king.currentX,
            king.currentY,
            isWhite);
    }



    public bool IsCheckmate(
    bool isWhite)
{
    bool inCheck =
        IsKingInCheck(isWhite);

    bool hasMove =
        HasAnyLegalMove(isWhite);

    Debug.Log(
        $"CHECKMATE TEST | " +
        $"White={isWhite} | " +
        $"InCheck={inCheck} | " +
        $"HasMove={hasMove}");

    return
        inCheck &&
        !hasMove;
}

public bool IsStalemate(
    bool isWhite)
{
    if (IsKingInCheck(isWhite))
        return false;

    return !HasAnyLegalMove(
        isWhite);
}

    private bool PieceHasLegalMove(
    ChessPiece piece)
{
    switch (piece.pieceType)
    {
        case PieceType.Pawn:
            return PawnHasLegalMove(piece);

        case PieceType.Rook:
            return RookHasLegalMove(piece);

        case PieceType.Knight:
            return KnightHasLegalMove(piece);

        case PieceType.Bishop:
            return BishopHasLegalMove(piece);

        case PieceType.Queen:
            return QueenHasLegalMove(piece);

        case PieceType.King:
            return KingHasLegalMove(piece);
    }

    return false;
}

private bool PawnHasLegalMove(
    ChessPiece piece)
{
    int direction =
        piece.isWhite ? 1 : -1;

    // maju 1
    int oneStepY =
        piece.currentY + direction;

    if (
        IsInsideBoard(
            piece.currentX,
            oneStepY)
        &&
        IsTileEmpty(
            piece.currentX,
            oneStepY)
        &&
        !WouldMoveLeaveKingInCheck(
            piece,
            piece.currentX,
            oneStepY))
    {
        return true;
    }

    // makan kiri
    int leftX =
        piece.currentX - 1;

    int captureY =
        piece.currentY + direction;

    if (
        IsInsideBoard(
            leftX,
            captureY))
    {
        ChessPiece target =
            GetPiece(
                leftX,
                captureY);

        if (
            target != null
            &&
            target.isWhite !=
            piece.isWhite
            &&
            !WouldMoveLeaveKingInCheck(
                piece,
                leftX,
                captureY))
        {
            return true;
        }
    }

    // makan kanan
    int rightX =
        piece.currentX + 1;

    if (
        IsInsideBoard(
            rightX,
            captureY))
    {
        ChessPiece target =
            GetPiece(
                rightX,
                captureY);

        if (
            target != null
            &&
            target.isWhite !=
            piece.isWhite
            &&
            !WouldMoveLeaveKingInCheck(
                piece,
                rightX,
                captureY))
        {
            return true;
        }
    }

    return false;
}
    private void GenerateGrid()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                tilePositions[x, y] =
                    transform.position +
                    gridOffset +
                    new Vector3(
                        x * tileSize,
                        0f,
                        y * tileSize
                    );
            }
        }
    }

    public Vector3 GetTilePosition(int x, int y)
    {
        return tilePositions[x, y];
    }

    public bool IsInsideBoard(int x, int y)
    {
        return x >= 0 &&
               x < 8 &&
               y >= 0 &&
               y < 8;
    }

    public ChessPiece GetPiece(int x, int y)
    {
        if (!IsInsideBoard(x, y))
            return null;

        return board[x, y];
    }

    public bool IsTileEmpty(int x, int y)
    {
        return GetPiece(x, y) == null;
    }

    public void MovePiece(
    ChessPiece piece,
    int targetX,
    int targetY)
    {
        Debug.Log("Target Piece = " + board[targetX, targetY]);
        
        ChessPiece targetPiece = board[targetX, targetY];

        // UPDATE LOGICAL BOARD IMMEDIATELY
        board[piece.currentX, piece.currentY] = null;
        board[targetX, targetY] = piece;
        piece.SetPosition(targetX, targetY);
        piece.hasMoved = true;
        
        Debug.Log(piece.name + " hasMoved = " + piece.hasMoved);

        // VISUAL ANIMATION
        Vector3 targetPos = GetTilePosition(targetX, targetY);
        
        piece.MoveTo(targetPos, () => 
        {
            if (targetPiece != null)
            {
                if (MusicManager.Instance != null) MusicManager.Instance.PlayCapture();
                piece.PlayAttackAnimation();
                
                targetPiece.PlayLoseAnimation(() => {
                    if (targetPiece != null && targetPiece.gameObject != null)
                    {
                        Destroy(targetPiece.gameObject);
                    }
                });
            }
        });
    }

    public void CheckPromotion(ChessPiece piece)
    {
        if (piece.pieceType != PieceType.Pawn)
        {
            return;
        }

        GameUIManager ui = FindFirstObjectByType<GameUIManager>();
        GameTimer timer = FindFirstObjectByType<GameTimer>();
        OrbitCamera cam = FindFirstObjectByType<OrbitCamera>();

        // White Promotion
        if (piece.isWhite && piece.currentY == 7)
        {
            promotionPawn = piece;
            if (PlayerRole.IsWhitePlayer()) // Player is White
            {
                if (timer != null) timer.isPaused = true;
                
                if (cam != null)
                {
                    PromotionVFX vfx = piece.gameObject.AddComponent<PromotionVFX>();
                    vfx.PlayPawnSpin();

                    cam.FocusOn(GetTilePosition(piece.currentX, piece.currentY), 4f, () => {
                        if (ui != null) ui.ShowPromotion();
                    });
                }
                else
                {
                    if (ui != null) ui.ShowPromotion();
                }
            }
            return;
        }

        // Black Promotion
        if (!piece.isWhite && piece.currentY == 0)
        {
            promotionPawn = piece;
            if (!PlayerRole.IsWhitePlayer()) // Player is Black
            {
                if (timer != null) timer.isPaused = true;
                
                if (cam != null)
                {
                    PromotionVFX vfx = piece.gameObject.AddComponent<PromotionVFX>();
                    vfx.PlayPawnSpin();

                    cam.FocusOn(GetTilePosition(piece.currentX, piece.currentY), 4f, () => {
                        if (ui != null) ui.ShowPromotion();
                    });
                }
                else
                {
                    if (ui != null) ui.ShowPromotion();
                }
            }
            return;
        }
    }

private void PromotePawn(
    ChessPiece pawn,
    bool isWhite)
{
    int x = pawn.currentX;
    int y = pawn.currentY;

    Destroy(
        pawn.gameObject);

    GameObject queenPrefab =
        isWhite
        ? whiteQueen
        : blackQueen;

    GameObject queen =
        Instantiate(
            queenPrefab,
            GetTilePosition(x, y),
            Quaternion.identity);

    ChessPiece cp =
        queen.GetComponent<ChessPiece>();

    cp.currentX = x;
    cp.currentY = y;

    cp.hasMoved = true;

    board[x, y] = cp;

    Debug.Log(
        (isWhite
        ? "WHITE"
        : "BLACK")
        +
        " PROMOTED TO QUEEN");
}

    public void SwitchTurn()
    {
        isWhiteTurn = !isWhiteTurn;

        UpdateTurnStatus();

        Debug.Log(
            isWhiteTurn
            ? "WHITE TURN"
            : "BLACK TURN");
    }

    private void SpawnPiece(
        GameObject prefab,
        int x,
        int y)
    {
        if (prefab == null)
        {
            Debug.LogWarning(
                $"Prefab kosong [{x},{y}]");

            return;
        }

        GameObject piece =
            Instantiate(
                prefab,
                GetTilePosition(x, y),
                Quaternion.identity);

        ChessPiece cp =
            piece.GetComponent<ChessPiece>();

        if (cp != null)
        {
            cp.currentX = x;
            cp.currentY = y;

            board[x, y] = cp;
        }
    }

    private void SpawnPieces()
    {
        // White Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnPiece(whitePawn, i, 1);
        }

        // Black Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnPiece(blackPawn, i, 6);
        }

        // White Back Row
        SpawnPiece(whiteRook, 0, 0);
        SpawnPiece(whiteKnight, 1, 0);
        SpawnPiece(whiteBishop, 2, 0);
        SpawnPiece(whiteQueen, 3, 0);
        SpawnPiece(whiteKing, 4, 0);
        SpawnPiece(whiteBishop, 5, 0);
        SpawnPiece(whiteKnight, 6, 0);
        SpawnPiece(whiteRook, 7, 0);

        // Black Back Row
        SpawnPiece(blackRook, 0, 7);
        SpawnPiece(blackKnight, 1, 7);
        SpawnPiece(blackBishop, 2, 7);
        SpawnPiece(blackQueen, 3, 7);
        SpawnPiece(blackKing, 4, 7);
        SpawnPiece(blackBishop, 5, 7);
        SpawnPiece(blackKnight, 6, 7);
        SpawnPiece(blackRook, 7, 7);
    }

    public void ShowHighlight(int x, int y)
    {
        if (!IsInsideBoard(x, y))
            return;

        GameObject highlight =
            Instantiate(
                moveHighlightPrefab,
                GetTilePosition(x, y)
                + Vector3.up * 0.02f,
                Quaternion.Euler(90, 0, 0)
            );

        MoveHighlight mh =
            highlight.GetComponent<MoveHighlight>();

        if (mh != null)
        {
            mh.targetX = x;
            mh.targetY = y;
        }

        activeHighlights.Add(highlight);
    }

    public void ClearHighlights()
    {
        foreach (GameObject h in activeHighlights)
        {
            Destroy(h);
        }

        activeHighlights.Clear();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Vector3 pos =
                    transform.position +
                    gridOffset +
                    new Vector3(
                        x * tileSize,
                        0.1f,
                        y * tileSize
                    );

                Gizmos.DrawSphere(pos, 0.08f);
            }
        }
    }
    public bool HasAnyLegalMove(
    bool isWhite)
{
    for (int x = 0; x < 8; x++)
    {
        for (int y = 0; y < 8; y++)
        {
            ChessPiece piece =
                board[x, y];

            if (piece == null)
                continue;

            if (piece.isWhite != isWhite)
                continue;

            if (PieceHasLegalMove(piece))
            {
                return true;
            }
        }
    }

    return false;
}

    // private bool PieceHasLegalMove(
    //     ChessPiece piece)
    // {
    //     switch (piece.pieceType)
    //     {
    //         case PieceType.Pawn:
    //             return PawnHasLegalMove(piece);

    //         case PieceType.Rook:
    //             return false;

    //         case PieceType.Knight:
    //             return false;

    //         case PieceType.Bishop:
    //             return false;

    //         case PieceType.Queen:
    //             return false;

    //         case PieceType.King:
    //             return false;
    //     }

    //     return false;
    // }

    // private bool PawnHasLegalMove(
    //     ChessPiece piece)
    // {
    //     int direction =
    //         piece.isWhite ? 1 : -1;

    //     int targetY =
    //         piece.currentY + direction;

    //     if (
    //         IsInsideBoard(
    //             piece.currentX,
    //             targetY)
    //         &&
    //         IsTileEmpty(
    //             piece.currentX,
    //             targetY)
    //         &&
    //         !WouldMoveLeaveKingInCheck(
    //             piece,
    //             piece.currentX,
    //             targetY))
    //     {
    //         return true;
    //     }

    //     return false;
    // }

    public bool CanCastleKingSide(
        ChessPiece king)
    {
        if (king.hasMoved)
            return false;

        int rookX = 7;
        int row = king.currentY;

        ChessPiece rook =
            GetPiece(rookX, row);

        if (rook == null)
            return false;

        if (rook.pieceType != PieceType.Rook)
            return false;

        if (rook.hasMoved)
            return false;

        if (!IsTileEmpty(5, row))
            return false;

        if (!IsTileEmpty(6, row))
            return false;

        return true;
    }

    public void CastleKingSide(
    ChessPiece king)
{
    int row =
        king.currentY;

    ChessPiece rook =
        GetPiece(7, row);

    if (rook == null)
    {
        Debug.LogError(
            "Castle Failed : Rook NULL");

        return;
    }

    MovePiece(
        king,
        6,
        row);

    MovePiece(
        rook,
        5,
        row);
}

    public bool CanCastleQueenSide(
        ChessPiece king)
    {
        if (king.hasMoved)
            return false;

        int row = king.currentY;

        ChessPiece rook =
            GetPiece(0, row);

        if (rook == null)
            return false;

        if (rook.pieceType !=
            PieceType.Rook)
            return false;

        if (rook.hasMoved)
            return false;

        if (!IsTileEmpty(1, row))
            return false;

        if (!IsTileEmpty(2, row))
            return false;

        if (!IsTileEmpty(3, row))
            return false;

        return true;
    }

    public void CastleQueenSide(
    ChessPiece king)
{
    int row =
        king.currentY;

    ChessPiece rook =
        GetPiece(0, row);

    if (rook == null)
    {
        Debug.LogError(
            "Castle Failed : Rook NULL");

            return;
    }

    MovePiece(
        king,
        2,
        row);

    MovePiece(
        rook,
        3,
        row);
}

    private bool KnightHasLegalMove(
        ChessPiece piece)
    {
        int[,] moves =
        {
            { 1, 2 },
            { -1, 2 },

            { 1, -2 },
            { -1, -2 },

            { 2, 1 },
            { 2, -1 },

            { -2, 1 },
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

            if (!IsInsideBoard(x, y))
                continue;

            ChessPiece target =
                GetPiece(x, y);

            if (
                target == null
                ||
                target.isWhite != piece.isWhite)
            {
                if (!WouldMoveLeaveKingInCheck(
                        piece,
                        x,
                        y))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool KingHasLegalMove(
        ChessPiece piece)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int x =
                    piece.currentX + dx;

                int y =
                    piece.currentY + dy;

                if (!IsInsideBoard(x, y))
                    continue;

                ChessPiece target =
                    GetPiece(x, y);

                // kosong
                if (target == null)
                {
                    if (!WouldMoveLeaveKingInCheck(
                            piece,
                            x,
                            y))
                    {
                        return true;
                    }
                }
                // musuh
                else if (
                    target.isWhite !=
                    piece.isWhite)
                {
                    if (!WouldMoveLeaveKingInCheck(
                            piece,
                            x,
                            y))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    private bool RookHasLegalMove(
        ChessPiece piece)
    {
        int[,] directions =
        {
            { 0, 1 },
            { 0, -1 },
            { 1, 0 },
            { -1, 0 }
        };

        for (int d = 0; d < 4; d++)
        {
            int dirX =
                directions[d, 0];

            int dirY =
                directions[d, 1];

            int x =
                piece.currentX + dirX;

            int y =
                piece.currentY + dirY;

            while (IsInsideBoard(x, y))
            {
                ChessPiece target =
                    GetPiece(x, y);

                // kotak kosong
                if (target == null)
                {
                    if (!WouldMoveLeaveKingInCheck(
                            piece,
                            x,
                            y))
                    {
                        return true;
                    }
                }
                else
                {
                    // musuh
                    if (target.isWhite !=
                        piece.isWhite)
                    {
                        if (!WouldMoveLeaveKingInCheck(
                                piece,
                                x,
                                y))
                        {
                            return true;
                        }
                    }

                    break;
                }

                x += dirX;
                y += dirY;
            }
        }

        return false;
    }

    private bool BishopHasLegalMove(
        ChessPiece piece)
    {
        int[,] directions =
        {
            { 1, 1 },
            { -1, 1 },
            { 1, -1 },
            { -1, -1 }
        };

        for (int d = 0; d < 4; d++)
        {
            int dirX =
                directions[d, 0];

            int dirY =
                directions[d, 1];

            int x =
                piece.currentX + dirX;

            int y =
                piece.currentY + dirY;

            while (IsInsideBoard(x, y))
            {
                ChessPiece target =
                    GetPiece(x, y);

                if (target == null)
                {
                    if (!WouldMoveLeaveKingInCheck(
                            piece,
                            x,
                            y))
                    {
                        return true;
                    }
                }
                else
                {
                    if (target.isWhite !=
                        piece.isWhite)
                    {
                        if (!WouldMoveLeaveKingInCheck(
                                piece,
                                x,
                                y))
                        {
                            return true;
                        }
                    }

                    break;
                }

                x += dirX;
                y += dirY;
            }
        }

        return false;
    }

    private bool QueenHasLegalMove(
        ChessPiece piece)
    {
        return
            RookHasLegalMove(piece)
            ||
            BishopHasLegalMove(piece);
    }

    public void ExecuteMove(
    ChessPiece piece,
    int targetX,
    int targetY)
    {
        bool isCastling = piece.pieceType == PieceType.King && Mathf.Abs(targetX - piece.currentX) == 2;

        if (isCastling)
        {
            if (targetX == 6)
            {
                CastleKingSide(piece);
            }
            else if (targetX == 2)
            {
                CastleQueenSide(piece);
            }
        }
        else
        {
            MovePiece(
                piece,
                targetX,
                targetY);

            CheckPromotion(
                piece);
        }

        ClearHighlights();

        SwitchTurn();

        GameUIManager ui =
            FindFirstObjectByType<GameUIManager>();

        // ==========================
        // WHITE STATUS
        // ==========================
        if (IsCheckmate(true))
        {
            Debug.Log("WHITE CHECKMATE");

            ChessPiece king = FindKing(true);
            if (king != null)
            {
                king.PlayLoseAnimation(() => {
                    if (ui != null)
                    {
                        ui.SetStatus("White Checkmate");
                        ui.ShowPlayer2Win("SKAKMAT!", "Sisi putih terkena skakmat");
                    }
                });
            }
            else if (ui != null)
            {
                ui.SetStatus("White Checkmate");
                ui.ShowPlayer2Win("SKAKMAT!", "Sisi putih terkena skakmat");
            }

            return;
        }

        if (IsStalemate(true))
        {
            Debug.Log("WHITE STALEMATE");

            if (ui != null)
            {
                ui.SetStatus(
                    "Stalemate");

                ui.ShowDraw("REMIS!", "Stalemate - Sisi putih");
            }

            return;
        }

        if (IsKingInCheck(true))
        {
            Debug.Log("WHITE CHECK");

            if (MusicManager.Instance != null) MusicManager.Instance.PlayCheck();

            if (ui != null)
            {
                ui.SetStatus(
                    "White King In Check");
            }
        }

        // ==========================
        // BLACK STATUS
        // ==========================
        if (IsCheckmate(false))
        {
            Debug.Log("BLACK CHECKMATE");

            ChessPiece king = FindKing(false);
            if (king != null)
            {
                king.PlayLoseAnimation(() => {
                    if (ui != null)
                    {
                        ui.SetStatus("Black Checkmate");
                        ui.ShowPlayer1Win("SKAKMAT!", "Sisi hitam terkena skakmat");
                    }
                });
            }
            else if (ui != null)
            {
                ui.SetStatus("Black Checkmate");
                ui.ShowPlayer1Win("SKAKMAT!", "Sisi hitam terkena skakmat");
            }

            return;
        }

        if (IsStalemate(false))
        {
            Debug.Log("BLACK STALEMATE");

            if (ui != null)
            {
                ui.SetStatus(
                    "Stalemate");

                ui.ShowDraw("REMIS!", "Stalemate - Sisi hitam");
            }

            return;
        }

        if (IsKingInCheck(false))
        {
            Debug.Log("BLACK CHECK");

            if (MusicManager.Instance != null) MusicManager.Instance.PlayCheck();

            if (ui != null)
            {
                ui.SetStatus(
                    "Black King In Check");
            }
        }

        Debug.Log(
            $"Moved To {targetX},{targetY}");
    }

    public void PromoteSelectedPawn(
        PromotionType type)
    {
        if (promotionPawn == null)
            return;

        int x = promotionPawn.currentX;
        int y = promotionPawn.currentY;
        bool isWhite = promotionPawn.isWhite;

        Destroy(
            promotionPawn.gameObject);

        GameObject prefab = null;

        switch (type)
        {
            case PromotionType.Queen:
                prefab =
                    isWhite
                    ? whiteQueen
                    : blackQueen;
                break;

            case PromotionType.Rook:
                prefab =
                    isWhite
                    ? whiteRook
                    : blackRook;
                break;

            case PromotionType.Bishop:
                prefab =
                    isWhite
                    ? whiteBishop
                    : blackBishop;
                break;

            case PromotionType.Knight:
                prefab =
                    isWhite
                    ? whiteKnight
                    : blackKnight;
                break;
        }

        GameObject piece =
            Instantiate(
                prefab,
                GetTilePosition(x, y),
                Quaternion.identity);

        ChessPiece cp =
            piece.GetComponent<ChessPiece>();

        cp.currentX = x;
        cp.currentY = y;
        cp.hasMoved = true;

        board[x, y] = cp;

        promotionPawn = null;

        OrbitCamera cam = FindFirstObjectByType<OrbitCamera>();
        if (cam != null && cam.hasZoomedIn)
        {
            PromotionVFX vfx = piece.AddComponent<PromotionVFX>();
            vfx.PlaySpawnPop(() => {
                cam.ResetFocus(() => {
                    GameTimer timer = FindFirstObjectByType<GameTimer>();
                    if (timer != null) timer.isPaused = false;
                });
            });
        }
        else
        {
            GameTimer timer = FindFirstObjectByType<GameTimer>();
            if (timer != null) timer.isPaused = false;
        }
    }

    public void SetPromotionPawn(
        ChessPiece pawn)
    {
        promotionPawn = pawn;
    }
    public ChessPiece GetPromotionPawn()
    {
        return promotionPawn;
    }
    private void UpdateTurnStatus()
    {
        GameUIManager ui =
            FindFirstObjectByType<GameUIManager>();

        if (ui != null)
        {
            bool myTurn =
                isWhiteTurn ==
                PlayerRole.IsWhitePlayer();

            ui.SetStatus(
                myTurn
                ? "Your Turn"
                : "Opponent's Turn");
        }
    }


    


}