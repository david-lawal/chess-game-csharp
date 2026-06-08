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
                Move? move = ParseMove(input);

                if (move == null)
                {
                    Console.WriteLine("Invalid move format. Use format like e2 e4.");
                    Console.ReadKey();
                    continue;
                }
               
                    bool moved = board.MovePiece(move, currentPlayer);

                if (!moved)
                {              
                    Console.ReadKey();
                    continue;
                }

                if (board.IsKingCaptured()) //not a proper ending but gives the game an end
                {
                    Console.Clear();
                    board.PrintBoard();
                    Console.WriteLine(currentPlayer == 'W' ? "White wins!" : "Black wins!");
                    Console.ReadKey();
                    break;                    
                }

                SwitchPlayer();
                
            }

        }

        private void SwitchPlayer()
        {
            currentPlayer = currentPlayer == 'W' ? 'B' : 'W';
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
