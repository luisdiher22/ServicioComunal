using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicioComunal.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRequiereCambioContraseña : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiereCambioContraseña",
                table: "USUARIO",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiereCambioContraseña",
                table: "USUARIO");
        }
    }
}
