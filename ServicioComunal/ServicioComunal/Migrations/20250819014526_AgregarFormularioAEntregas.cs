using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicioComunal.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFormularioAEntregas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FORMULARIO_Identificacion",
                table: "ENTREGA",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ENTREGA_FORMULARIO_Identificacion",
                table: "ENTREGA",
                column: "FORMULARIO_Identificacion");

            migrationBuilder.AddForeignKey(
                name: "FK_ENTREGA_FORMULARIO_FORMULARIO_Identificacion",
                table: "ENTREGA",
                column: "FORMULARIO_Identificacion",
                principalTable: "FORMULARIO",
                principalColumn: "Identificacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ENTREGA_FORMULARIO_FORMULARIO_Identificacion",
                table: "ENTREGA");

            migrationBuilder.DropIndex(
                name: "IX_ENTREGA_FORMULARIO_Identificacion",
                table: "ENTREGA");

            migrationBuilder.DropColumn(
                name: "FORMULARIO_Identificacion",
                table: "ENTREGA");
        }
    }
}
