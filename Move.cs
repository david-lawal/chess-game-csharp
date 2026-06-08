using System;
using System.Collections.Generic;
using System.Text;

namespace ChessGame
{
    public class Move
    {
        public Position Start {  get; set; }
        public Position End { get; set; }

        public Move(Position start, Position end)
        {
            Start = start;
            End = end;
        }
    }
}
