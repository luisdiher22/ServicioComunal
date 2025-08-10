using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicioComunal.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUsuarioRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_USUARIO_ESTUDIANTE_Identificacion",
                table: "USUARIO");

            migrationBuilder.DropForeignKey(
                name: "FK_USUARIO_PROFESOR_Identificacion",
                table: "USUARIO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_USUARIO_ESTUDIANTE_Identificacion",
                table: "USUARIO",
                column: "Identificacion",
                principalTable: "ESTUDIANTE",
                principalColumn: "Identificacion",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_USUARIO_PROFESOR_Identificacion",
                table: "USUARIO",
                column: "Identificacion",
                principalTable: "PROFESOR",
                principalColumn: "Identificacion",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
