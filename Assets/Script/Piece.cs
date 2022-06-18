using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool isWhite;
    public bool isKing;

    public bool isCanKill(Piece[,] board, int x, int y)
    {
        if (isWhite || isKing)
        {
            //top left
            if (x >= 2 && y <= 5)
            {
                Piece piece = board[x - 1, y + 1];
                if (piece != null && piece.isWhite != isWhite) //check if there's a piece in between and enemy
                {
                    if (board[x - 2, y + 2] == null) //check if landing is null
                        return true;
                }
            }

            //top right
            if (x <= 5 && y <= 5)
            {
                Piece piece = board[x + 1, y + 1];
                if (piece != null && piece.isWhite != isWhite) //check if there's a piece in between and enemy
                {
                    if (board[x + 2, y + 2] == null) //check if landing is null
                        return true;
                }
            }
        }

        if (!isWhite || isKing) //for black
        {
            //bottom left
            if (x >= 2 && y >= 2)
            {
                Piece piece = board[x - 1, y - 1];
                if (piece != null && piece.isWhite != isWhite) //check if there's a piece in between and enemy
                {
                    if (board[x - 2, y - 2] == null) //check if landing is null
                        return true;
                }
            }

            //bottom right
            if (x <= 5 && y >= 2)
            {
                Piece piece = board[x + 1, y - 1];
                if (piece != null && piece.isWhite != isWhite) //check if there's a piece in between and enemy
                {
                    if (board[x + 2, y - 2] == null) //check if landing is null
                        return true;
                }
            }
        }

        return false;
    }

    public bool validMove(Piece[,] board, int x1, int y1, int x2, int y2)
    {
        // is piece move on top of another
        if (board[x2, y2] != null)
            return false;

        int moveRangeX = Mathf.Abs(x1 - x2);
        int moveRangeY = y2 - y1;

        if (isWhite || isKing) //legal move for white
        {
            if (moveRangeX == 1) //normal move
            {
                if (moveRangeY == 1)
                    return true;
            }
            else if (moveRangeX == 2) //killing move
            {
                if (moveRangeY == 2)
                {
                    Piece piece = board[(x1 + x2) / 2, (y1 + y2) / 2]; // if there's piece between move
                    if (piece != null && piece.isWhite != isWhite) // if not null and is enemy then kill
                    {
                        return true;
                    }
                }
            }
        }

        if (!isWhite || isKing) //legal move for black
        {
            if (moveRangeX == 1) //normal move
            {
                if (moveRangeY == -1)
                    return true;
            }
            else if (moveRangeX == 2) //killing move
            {
                if (moveRangeY == -2)
                {
                    Piece piece = board[(x1 + x2) / 2, (y1 + y2) / 2]; // if there's piece between move
                    if (piece != null && piece.isWhite != isWhite) // if not null and is enemy then kill
                    {
                        return true;
                    }
                }
            }
        }

        return false; //if doesnt correlate with the above then its illegal move
    }
}
