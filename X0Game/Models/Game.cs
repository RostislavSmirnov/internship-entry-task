using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace X0Game.Models
{
    public class Game
    {        
        [Key]
        [Column("game_id")]
        public int GameId { get; set; }
              
        [Required(ErrorMessage = "Размер поля должен быть минимум 3 клетки в каждую из сторон")]
        [Column("field",TypeName = "jsonb")]
        public List<List<string>> Field { get; set; }

        [Column("field_size")]
        [Range(3, 100)]
        public int FieldSize { get; set; }

        [Column("vicory_condition")]
        [Range(3, 100)]
        public int VictoryCondition { get; set; }

        [Column("game_counter_of_moves")]
        public int CounterOfMoves { get; set; }

        public string GameStatus { get; set; }
        
        [Required]
        [Column("next_player")]
        public string NextPlayer { get; set; }

        public uint Version { get; set; }
    }
}
