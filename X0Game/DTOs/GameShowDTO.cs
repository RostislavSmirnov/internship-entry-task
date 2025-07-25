using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace X0Game.DTOs
{
    public class GameShowDTO
    {
        public int GameId { get; set; }

        public List<List<string>> Field { get; set; }

        public int FieldSize { get; set; }

        public int VictoryCondition { get; set; }

        public int CounterOfMoves { get; set; }

        public string GameStatus { get; set; }

        public string NextPlayer { get; set; } //x or o

        public string Version { get; set; }
    }
}
