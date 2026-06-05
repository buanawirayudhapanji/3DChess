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

    private void Start()
    {
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
        Debug.Log(
        "Target Piece = " +
        board[targetX, targetY]);
        
        ChessPiece targetPiece =
            board[targetX, targetY];

        if (targetPiece != null)
        {
            Destroy(targetPiece.gameObject);
        }

        board[
            piece.currentX,
            piece.currentY
        ] = null;

        board[
            targetX,
            targetY
        ] = piece;

        piece.SetPosition(
            targetX,
            targetY);

        piece.hasMoved = true;

        piece.transform.position =
            GetTilePosition(
                targetX,
                targetY);
    }

    public void SwitchTurn()
    {
        isWhiteTurn = !isWhiteTurn;

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
    
}