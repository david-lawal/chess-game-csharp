using System;
using System.Collections.Generic;
using System.Text;

namespace ChessGame
{
    public class Board
    {
        private string[,] squares = new string[8, 8];

        //castling variables
        private bool whiteKingMoved = false;
        private bool blackKingMoved = false;
        private bool whiteLeftRookMoved = false;
        private bool whiteRightRookMoved = false;
        private bool blackLeftRookMoved = false;
        private bool blackRightRookMoved = false;    

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

        public Move? FindMoveFromNotation(string input, char currentPlayer)
        { 
            input = input.Trim();

            string promotionPiece = "";

            if (input.Contains("="))
            {
                string[] promotionparts = input.Split('=');

                if (promotionparts.Length != 2)
                {
                    return null;
                }

                input = promotionparts[0];
                promotionPiece = promotionparts[1].ToUpper();
            }

            input = input.Replace("+", "");
            input = input.Replace("#", "");

            bool isCapture = input.Contains("x");

            //handle pawn capturing notation as it is slightly different
            char? pawnStartFile = null;

            if (isCapture && char.IsLower(input[0]) && input.Length >= 4)
            {
                pawnStartFile = input[0];
            }

            input = input.Replace("x", "");

            if (pawnStartFile != null)
            {
                input = input.Substring(1);
            }

            if (input.Length < 2)
            {
                return null;
            }

            char pieceSymbol = 'P';
            string destinationText;

            char firstChar = char.ToUpper(input[0]);

            if (firstChar == 'N' || firstChar == 'B' || firstChar == 'Q' || firstChar == 'K')
            {
                pieceSymbol = firstChar;
                destinationText = input.Substring(1);
            }
            else
            { 
                destinationText = input; //these are for the pawn moves
            }

            Position? destination = ParseDestination(destinationText);

            if (destination == null)
            {
                return null;
            }

            string targetPiece = currentPlayer == 'W'
                ? pieceSymbol.ToString()
                : pieceSymbol.ToString().ToLower();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++) 
                {
                    if (squares[row, col] != targetPiece)
                    {
                        continue;
                    }

                    if (pawnStartFile != null)
                    {
                        int requiredColumn = pawnStartFile.Value - 'a';

                        if (col != requiredColumn)
                        {
                            continue;
                        }
                    }

                    Move possibleMove = new Move(
                        new Position(row, col),
                        destination
                    );

                    string target = squares[destination.Row, destination.Column];

                    if (isCapture && target == ".")
                    {
                        continue;
                    }

                    if (!isCapture && target != ".")
                    {
                        continue;
                    }

                    if (IsSameColour(targetPiece, target))
                    {
                        continue;
                    }                    
                    
                    if (!IsValidMove(targetPiece, possibleMove))
                    {
                        continue;
                    }

                    if (WouldLeaveKingInCheck(possibleMove, currentPlayer)) 
                    {
                        continue;
                    }

                    possibleMove.PromotionPiece = promotionPiece;
                    return possibleMove;
                }
            }

            return null;
        }

        private Position? ParseDestination(string text)
        {
            if (text.Length != 2)
            {
                return null;
            }

            char file = char.ToLower(text[0]);
            char rank = text[1];

            if (file < 'a' || file > 'h')
            {
                return null;
            }

            if (rank < '1' || rank > '8')
            {
                return null;
            }

            int column = file - 'a';
            int row = 8 - int.Parse(rank.ToString());

            return new Position(row, column);
        }

        private bool WouldLeaveKingInCheck(Move move, char player)
        {
            string piece = squares[move.Start.Row, move.Start.Column];
            string target = squares[move.End.Row, move.End.Column];

            squares[move.End.Row, move.End.Column] = piece;
            squares[move.Start.Row, move.Start.Column] = ".";

            bool inCheck = IsKingInCheck(player);

            squares[move.Start.Row, move.Start.Column] = piece;
            squares[move.End.Row, move.End.Column] = target;

            return inCheck;

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

            if (IsCastlingMove(piece, move))
            {
                return Castle(move, currentPlayer);
            }

            if (!IsValidMove(piece, move))
            {
                Console.WriteLine("Invalid move for that piece.");
                return false;
            }

            string originalPiece = squares[move.Start.Row, move.Start.Column];
            string capturedPiece = squares[move.End.Row, move.End.Column];

            squares[move.End.Row, move.End.Column] = piece;
            squares[move.Start.Row, move.Start.Column] = ".";

            //prevent players from putting their king in check
            if (IsKingInCheck(currentPlayer))
            {
                squares[move.Start.Row, move.Start.Column] = originalPiece;
                squares[move.End.Row, move.End.Column] = capturedPiece;

                Console.WriteLine("You cannot leave your king in check.");
                return false;
            }

            PromotePawnIfNeeded(move);
            MarkMovedPieces(piece, move);

            return true;
        }

        //marking normal king and rook moves that aren't castling 
        private void MarkMovedPieces(string piece, Move move)
        { 
            if (piece == "K") whiteKingMoved = true;
            if (piece == "k") blackKingMoved = true;

            if (piece == "R")
            {
                MarkRookMoved(move.Start.Row, move.Start.Column);
            }

            if (piece == "r")
            {
                MarkRookMoved(move.Start.Row, move.Start.Column);
            }

        }


        //checking for legal moves
        public bool HasAnyLegalMoves(char player)
        {
            for (int startRow = 0; startRow < 8; startRow++) 
            {
                for (int startCol = 0; startCol < 8; startCol++)
                { 
                    string piece = squares[startRow, startCol];

                    if (piece == ".")
                    {
                        continue;
                    }

                    if (!IsCorrectPlayerPiece(piece, player))
                    {
                        continue;
                    }

                    for (int endRow = 0; endRow < 8; endRow++)
                    {
                        for (int endCol = 0; endCol < 8; endCol++) 
                        {
                            Move move = new Move(
                                new Position(startRow, startCol),
                                new Position(endRow, endCol)
                            );

                            if (IsLegalMoveSimulation(move, player))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        //castling logic
        private bool IsCastlingMove(string piece, Move move)
        {
            return piece.ToLower() == "k" &&
                   move.Start.Row == move.End.Row &&
                   Math.Abs(move.End.Column - move.Start.Column) == 2;
        }

        private bool Castle(Move move, char currentPlayer)
        {
            string king = squares[move.Start.Row, move.Start.Column];

            if (IsKingInCheck(currentPlayer))
            {
                Console.WriteLine("You cannot castle while in check.");
                return false;
            }

            bool isWhite = currentPlayer == 'W';
            bool kingSide = move.End.Column == 6; //king side castling (O-O)
            bool queenSide = move.End.Column == 2; //queen side castling (O-O-O)

            if (isWhite && whiteKingMoved) return false;
            if (!isWhite && blackKingMoved) return false;

            int row = isWhite ? 7 : 0;

            if (kingSide)
            {
                if (squares[row, 5] != "." || squares[row, 6] != ".")
                {
                    Console.WriteLine("Castling path is blocked.");
                    return false;
                }

                if (!CanKingPassThrough(row, 5, currentPlayer) ||
                    !CanKingPassThrough(row, 6, currentPlayer))
                {
                    Console.WriteLine("You cannot castle through check.");
                    return false;
                }

                squares[row, 6] = king;
                squares[row, 5] = squares[row, 7];
                squares[row, 4] = ".";
                squares[row, 7] = ".";

                MarkKingMoved(currentPlayer);
                MarkRookMoved(row, 7);

                return true;
            }

            if (queenSide)
            {
                if (squares[row, 1] != "." || squares[row, 2] != "." || squares[row, 3] != ".")
                {
                    Console.WriteLine("Castling path is blocked.");
                    return false;
                }

                if (!CanKingPassThrough(row, 3, currentPlayer) ||
                    !CanKingPassThrough(row, 2, currentPlayer))
                {
                    Console.WriteLine("You cannot castle through check.");
                    return false;
                }

                squares[row, 2] = king;
                squares[row, 3] = squares[row, 7];
                squares[row, 4] = ".";
                squares[row, 0] = ".";

                MarkKingMoved(currentPlayer);
                MarkRookMoved(row, 0);

                return true;
            }

            return false;
        }

        private bool CanKingPassThrough(int row, int col, char currentPlayer)
        {
            string king = currentPlayer == 'W' ? "K" : "k";

            int startCol = 4;
            string originalStart = squares[row, startCol];
            string originalTarget = squares[row, col];

            squares[row, col] = king;
            squares[row, startCol] = ".";

            bool inCheck = IsKingInCheck(currentPlayer);

            squares[row, startCol] = originalStart;
            squares[row, col] = originalTarget;

            return !inCheck;
        }

        private void MarkKingMoved(char player)
        {
            if (player == 'W')
            {
                whiteKingMoved = true;
            }
            else
            { 
                blackKingMoved = true;
            }
        }

        private void MarkRookMoved(int row, int col)
        { 
            if (row == 7 && col == 0) whiteLeftRookMoved = true;
            if (row == 7 && col == 7) whiteRightRookMoved = true;
            if (row == 0 && col == 0) blackLeftRookMoved = true;
            if (row == 0 && col == 7) blackRightRookMoved = true;
        }
        private bool IsLegalMoveSimulation(Move move, char player)
        {
            string piece = squares[move.Start.Row, move.Start.Column];
            string target = squares[move.End.Row, move.End.Column];

            if (piece == ".")
            {
                return false;
            }

            if (!IsCorrectPlayerPiece(piece, player))
            {
                return false;
            }

            if (IsSameColour(piece, target))
            {
                return false;
            }

            if (!IsValidMove(piece, move))
            {
                return false;
            }

            squares[move.End.Row, move.End.Column] = piece;
            squares[move.Start.Row, move.Start.Column] = ".";

            bool leavesKingInCheck = IsKingInCheck(player);

            squares[move.Start.Row, move.Start.Column] = piece;
            squares[move.End.Row, move.End.Column] = target;

            return !leavesKingInCheck;

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

        //check detection
        public bool IsKingInCheck(char player)
        {
            string king = player == 'W' ? "K" : "k";
            Position? kingPosition = FindKing(king);

            if (kingPosition == null)
            {
                return false;
            }

            char opponent = player == 'W' ? 'B' : 'W';

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    string piece = squares[row, col];

                    if (piece == ".")
                    {
                        continue;
                    }

                    if (!IsCorrectPlayerPiece(piece, opponent))
                    {
                        continue;
                    }

                    Move attackMove = new Move(
                        new Position(row, col),
                        kingPosition
                    );

                    if (IsValidMove(piece, attackMove))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private Position? FindKing(string king)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (squares[row, col] == king)
                    { 
                        return new Position(row, col);
                    }
                }
            }

            return null; 
        }


        //pawn promotion logic
        private void PromotePawnIfNeeded(Move move)
        {
            string piece = squares[move.End.Row, move.End.Column];

            if (piece != "P" && piece != "p")
            {
                return;
            }

            bool whitePromotion = piece == "P" && move.End.Row == 0;
            bool blackPromotion = piece == "p" && move.End.Row == 7;

            if (!whitePromotion && !blackPromotion)
            {
                return; //code for if the move wasnt a promotion move
            }

            string promotionPiece = move.PromotionPiece;

            if (promotionPiece != "Q" &&
                promotionPiece != "R" &&
                promotionPiece != "B" &&
                promotionPiece != "N")
            {
                promotionPiece = "Q";
            }

            if (piece == "p")
            {
                promotionPiece = promotionPiece.ToLower();
            }

            squares[move.End.Row, move.End.Column] = promotionPiece;

        }
    }
}
