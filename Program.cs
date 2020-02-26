using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueInProgress
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Game game = new Game(800, 600, "RogueInProgress"))
            {
                game.Run(60.0);
            }
        }
    }
}
