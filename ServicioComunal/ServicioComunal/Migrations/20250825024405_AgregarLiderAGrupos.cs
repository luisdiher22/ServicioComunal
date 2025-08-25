using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicioComunal.Migrations
{
    /// <inheritdoc />
    public partial class AgregarLiderAGrupos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LiderIdentificacion",
                table: "GRUPO",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GRUPO_LiderIdentificacion",
                table: "GRUPO",
                column: "LiderIdentificacion");

            migrationBuilder.AddForeignKey(
                name: "FK_GRUPO_ESTUDIANTE_LiderIdentificacion",
                table: "GRUPO",
                column: "LiderIdentificacion",
                principalTable: "ESTUDIANTE",
                principalColumn: "Identificacion",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GRUPO_ESTUDIANTE_LiderIdentificacion",
                table: "GRUPO");

            migrationBuilder.DropIndex(
                name: "IX_GRUPO_LiderIdentificacion",
                table: "GRUPO");

            migrationBuilder.DropColumn(
                name: "LiderIdentificacion",
                table: "GRUPO");
        }
    }
}
