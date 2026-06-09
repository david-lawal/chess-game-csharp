using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ChessGame
{
    public class Game
    {
        private Board board = new Board();
        private char currentPlayer = 'W';

        public void Start()
        {
            board.SetUpBoard();
            

            while (true)
            {
                Console.Clear();
                board.PrintBoard(); 

                Console.Write(currentPlayer == 'W' ? "White move: " : "Black move:  ");
                string? input = Console.ReadLine();

                if (input == "quit")
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }         

                //move piece
                
                Move? move = ParseCastlingMove(input); //if player wants to castle

                if (move == null)
                {
                    move = board.FindMoveFromNotation(input, currentPlayer); //if not then proceed with a normal move
                }

                if (move == null)
                {
                    move = ParseMove(input);
                }

                if (move == null)
                {
                    Console.WriteLine("Invalid move format. Use format like e2 e4, O-O, or O-O-O.");
                    Console.ReadKey();
                    continue;
                }
               
                    bool moved = board.MovePiece(move, currentPlayer);

                if (!moved)
                {              
                    Console.ReadKey();
                    continue;
                }
            
                char opponent = currentPlayer == 'W' ? 'B' : 'W';

                if (board.IsKingInCheck(opponent))
                {
                    if (!board.HasAnyLegalMoves(opponent)) //checkmate logic
                    {
                        Console.Clear();
                        board.PrintBoard();
                        Console.WriteLine(currentPlayer == 'W' ? "Checkmate! White wins!" : "Checkmate! Black wins!");
                        Console.ReadKey();
                        break;
                    }

                    //normal check logic

                    Console.Clear();
                    board.PrintBoard();
                    Console.WriteLine(opponent == 'W' ? "White king is in check!" : "Black king is in check!");
                    Console.ReadKey();
                }

                //statemate logic
                if (!board.IsKingInCheck(opponent) && !board.HasAnyLegalMoves(opponent))
                {
                    Console.Clear();
                    board.PrintBoard();
                    Console.WriteLine("Stalemate! The game is a draw.");
                    break;
                }


                SwitchPlayer();
                
            }

        }

        private void SwitchPlayer()
        {
            currentPlayer = currentPlayer == 'W' ? 'B' : 'W';
        }

        private Move? ParseCastlingMove(string input)
        {
            string upperInput = input.ToUpper();

            if (upperInput == "O-O")
            {
                return currentPlayer == 'W'
                    ? new Move(new Position(7, 4), new Position(7, 6))
                    : new Move(new Position(0, 4), new Position(0, 6));
            }

            if (upperInput == "O-O-O")
            {
                return currentPlayer == 'W'
                    ? new Move(new Position(7, 4), new Position(7, 2))
                    : new Move(new Position(0, 4), new Position(0, 2));
            }

            return null;
        }

        private Move? ParseMove(string input)
        {
            string[] parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2) 
            {
                return null;
            }

            Position? start = ParsePosition(parts[0]);
            Position? end = ParsePosition(parts[1]);

            if (start == null || end == null)
            {
                return null;
            }

            return new Move(start, end);
        }

        private Position ParsePosition(string positionText)
        {
            if (positionText.Length != 2)
            {
                return null;
            }

            char file = char.ToLower(positionText[0]);
            char rank = positionText[1];

            if (file < 'a' || file > 'h')
            {
                return null;
            }

            int column = file - 'a';
            int row = 8 - int.Parse(rank.ToString());

            return new Position(row, column);
        }
    }

}
