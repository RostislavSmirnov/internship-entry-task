using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace X0Game.Migrations
{
    /// <inheritdoc />
    public partial class ChangeVersionToUint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Version",
                table: "Games",
                newName: "xmin");

            migrationBuilder.AlterColumn<uint>(
                name: "xmin",
                table: "Games",
                type: "xid",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "xmin",
                table: "Games",
                newName: "Version");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Version",
                table: "Games",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true);
        }
    }
}
