using Microsoft.EntityFrameworkCore;
using X0Game.Models;
using System.Text.Json;

namespace X0Game.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
        {
        }
        public DbSet<Game> Games { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>()
                .Property(g => g.Field)
                .HasColumnType("jsonb")
                .HasConversion(
                     v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                     v => JsonSerializer.Deserialize<List<List<string>>>(v, (JsonSerializerOptions)null));

            modelBuilder.Entity<Game>(entity =>
            {
                entity.Property(e => e.Version).IsRowVersion();
            });

            base.OnModelCreating(modelBuilder);               
        }
    }
}