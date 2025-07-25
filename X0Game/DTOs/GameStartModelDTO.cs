using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace X0Game.DTOs
{
    public class GameStartModelDTO
    {      
        [Required(ErrorMessage = "Размер поля должен быть минимум 3 клетки в каждую из сторон")]        
        [Range(3, 100)]
        public int FieldSize { get; set; }

        [Range(3, 100)]
        public int VictoryCondition { get; set; }

        [Required(ErrorMessage = "Выберите либо x либо o")]
        [Column("next_player")]
        public string NextPlayer { get; set; } //x or o
    }
}
