using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace X0Game.Migrations
{
    /// <inheritdoc />
    public partial class ChangeFieldTo2DArray : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ALTER COLUMN field (перезаписываем тип, чтобы EF точно применил)
            migrationBuilder.AlterColumn<string>(
                name: "field",
                table: "Games",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
            name: "field",
            table: "Games",
            type: "jsonb",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "jsonb");
        }
    }
}
