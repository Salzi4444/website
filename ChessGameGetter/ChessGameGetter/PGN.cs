using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGameGetter
{
    internal class PGN
    {
        public string pgn;
        public string username;
        
        public PGN(string pgn, string username)
        {
            this.pgn = pgn;
            this.username = username;
        }
    }
}
