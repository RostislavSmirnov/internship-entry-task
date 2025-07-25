using System.ComponentModel.DataAnnotations;

namespace X0Game.DTOs
{
    public class GameMoveDTO
    {
        [Required]
        public int X { get; set; }

        [Required]
        public int Y { get; set; }

        [Required]
        public string Version { get; set; }
    }
}
