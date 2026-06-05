using UnityEngine;

public enum PieceType
{
    Pawn,
    Rook,
    Knight,
    Bishop,
    Queen,
    King
}

public class ChessPiece : MonoBehaviour
{
    public PieceType pieceType;

    public bool isWhite;

    public int currentX;
    public int currentY;
    public bool hasMoved = false;
    public void SetPosition(int x, int y)
    {
        currentX = x;
        currentY = y;
    }
}