using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicioComunal.Migrations
{
    /// <inheritdoc />
    public partial class PermitirProfesorNullEnEntrega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PROFESOR_Identificacion",
                table: "ENTREGA",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PROFESOR_Identificacion",
                table: "ENTREGA",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
