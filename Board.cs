using System;
using System.Collections.Generic;
using System.Text;

namespace ChessGame
{
    public class Board
    {
        private string[,] squares = new string[8, 8];

        public void SetUpBoard()
        {
            squares = new string[,]
            {
                { "r", "n", "b", "q", "k", "b", "n", "r" },
                { "p", "p", "p", "p", "p", "p", "p", "p" },
                { ".", ".", ".", ".", ".", ".", ".", "." },
                { ".", ".", ".", ".", ".", ".", ".", "." },
                { ".", ".", ".", ".", ".", ".", ".", "." },
                { ".", ".", ".", ".", ".", ".", ".", "." },
                { "P", "P", "P", "P", "P", "P", "P", "P" },
                { "R", "N", "B", "Q", "K", "B", "N", "R" }
            };
        }

        public void PrintBoard()
        {
            Console.WriteLine();

            for (int row = 0; row < 8; row++)
            {
                Console.Write(8 - row + " ");

                for (int col = 0; col < 8; col++)
                {
                    Console.Write(squares[row, col] + " ");
                }

                Console.WriteLine();
            }

            Console.WriteLine("  a b c d e f g h");
            Console.WriteLine();
        }

        public bool MovePiece(Move move, char currentPlayer)
        {
            string piece = squares[move.Start.Row, move.Start.Column];
            string target= squares[move.End.Row, move.End.Column];

            if (piece == ".")
            {
                Console.WriteLine("No piece found at that position.");
                return false;
            }

            if (!IsCorrectPlayerPiece(piece, currentPlayer))
            {
                Console.WriteLine("You can only move your own pieces.");
                return false;
            }

            if (IsSameColour(piece, target)) 
            {
                Console.WriteLine("You cannot move onto your own piece.");
                return false;
            }

            if (!IsValidMove(piece, move))
            {
                Console.WriteLine("Invalid move for that piece.");
                return false;
            }

            squares[move.End.Row, move.End.Column] = piece;
            squares[move.Start.Row, move.Start.Column] = ".";

            PromotePawnIfNeeded(move.End);

            return true;
        }

        private bool IsCorrectPlayerPiece(string piece, char currentPlayer)
        {
            if (currentPlayer == 'W')
            { 
                return char.IsUpper(piece[0]);
            }

            return char.IsLower(piece[0]);
        }

        private bool IsSameColour(string piece, string target) 
        {
            if (target == ".")
            {
                return false;
            }

            return char.IsUpper(piece[0]) == char.IsUpper(target[0]);
        }

        private bool IsValidMove(string piece, Move move)
        { 
            int rowDiff = move.End.Row - move.Start.Row;
            int colDiff = move.End.Column - move.Start.Column;

            switch (piece.ToLower())
            {
                case "p":
                    return IsValidPawnMove(piece, move);

                case "r":
                    return IsValidRookMove(rowDiff, colDiff, move);

                case "n":
                    return IsValidKnightMove(rowDiff, colDiff);

                case "b":
                    return IsValidBishopMove(rowDiff, colDiff, move);

                case "q":
                    return IsValidQueenMove(rowDiff, colDiff, move);

                case "k":
                    return IsValidKingMove(rowDiff, colDiff);

                default:
                    return false;
                    
            }
        }

        // ----------------------- PIECE RULES ------------------------------

        //pawn
        private bool IsValidPawnMove(string piece, Move move)
        {
            int direction = char.IsUpper(piece[0]) ? -1 : 1;
            int startrow = char.IsUpper(piece[0]) ? 6 : 1;

            int rowDiff = move.End.Row - move.Start.Row;
            int colDiff = move.End.Column - move.Start.Column;

            string target = squares[move.End.Row, move.End.Column];

            //move 1 forward
            if (colDiff == 0 && rowDiff == direction && target == ".")
            {
                return true;
            }

            //move 2 forward from starting position
            if (colDiff == 0 && move.Start.Row == startrow && rowDiff == direction * 2)
            { 
                int middleRow = move.Start.Row + direction;

                if (squares[middleRow, move.Start.Column] == "." && target == ".")
                {
                    return true;
                }
            }

            //capture diagonally
            if (Math.Abs(colDiff) == 1 && rowDiff == direction && target != ".") 
            {
                return true;
            }

            return false;

        }

        //rook
        private bool IsValidRookMove(int rowDiff, int colDiff, Move move)
        {
            if (rowDiff != 0 && colDiff != 0)
            {
                return false;
            }

            return IsPathClear(move);

        }

        //knight
        private bool IsValidKnightMove(int rowDiff, int colDiff)
        {
            return (Math.Abs(rowDiff) == 2 && Math.Abs(colDiff) == 1) || 
                    (Math.Abs(rowDiff) == 1 && Math.Abs(colDiff) == 2);
        }

        //bishop
        private bool IsValidBishopMove(int rowDiff, int colDiff, Move move)
        {
            if (Math.Abs(rowDiff) != Math.Abs(colDiff))
            {
                return false;
            }

            return IsPathClear(move);
        }

        //queen
        private bool IsValidQueenMove(int rowDiff, int colDiff, Move move)
        {
            bool movesLikeRook = rowDiff == 0 || colDiff == 0;
            bool movesLikeBishop = Math.Abs(rowDiff) == Math.Abs(colDiff);

            if (!movesLikeRook && !movesLikeBishop)
            {
                return false;
            }

            return IsPathClear(move);
        }

        //king
        private bool IsValidKingMove(int rowDiff, int colDiff)
        {
            return Math.Abs(rowDiff) <= 1 && Math.Abs(colDiff) <= 1;
        }

        private bool IsPathClear(Move move)
        {
            int rowDirection = Math.Sign(move.End.Row - move.Start.Row);
            int colDirection = Math.Sign(move.End.Column - move.Start.Column);

            int currentRow = move.Start.Row + rowDirection;
            int currentCol = move.Start.Column + colDirection;

            while (currentRow != move.End.Row || currentCol != move.End.Column) 
            {
                if (squares[currentRow, currentCol] != ".")
                {
                    return false;
                }

                currentRow += rowDirection;
                currentCol += colDirection;
            }

            return true;
        }

        public bool IsKingCaptured()
        {
            bool whiteKingExists = false;
            bool blackKingExists = false;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++) 
                {
                    if (squares[row, col] == "K")
                    {
                        whiteKingExists = true;
                    }

                    if (squares[row, col] == "k")
                    {
                        blackKingExists = true;
                    }
                }
            }

            return !whiteKingExists || !blackKingExists;
        }

        private void PromotePawnIfNeeded(Position position)
        {
            string piece = squares[position.Row, position.Column];

            if (piece == "P" && position.Row == 0)
            {
                //--- for now --- auto promote to a queen
                squares[position.Row, position.Column] = "Q";
            }
            else if (piece == "p" && position.Row == 7)
            {
                squares[position.Row, position.Column] = "q";
            }
        }
    }
}
