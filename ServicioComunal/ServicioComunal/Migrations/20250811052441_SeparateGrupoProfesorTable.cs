using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicioComunal.Migrations
{
    /// <inheritdoc />
    public partial class SeparateGrupoProfesorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GRUPO_ESTUDIANTE_PROFESOR_PROFESOR_Identificacion",
                table: "GRUPO_ESTUDIANTE");

            migrationBuilder.DropIndex(
                name: "IX_GRUPO_ESTUDIANTE_PROFESOR_Identificacion",
                table: "GRUPO_ESTUDIANTE");

            migrationBuilder.DropColumn(
                name: "PROFESOR_Identificacion",
                table: "GRUPO_ESTUDIANTE");

            migrationBuilder.CreateTable(
                name: "GRUPO_PROFESOR",
                columns: table => new
                {
                    GRUPO_Numero = table.Column<int>(type: "int", nullable: false),
                    PROFESOR_Identificacion = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GRUPO_PROFESOR", x => new { x.GRUPO_Numero, x.PROFESOR_Identificacion });
                    table.ForeignKey(
                        name: "FK_GRUPO_PROFESOR_GRUPO_GRUPO_Numero",
                        column: x => x.GRUPO_Numero,
                        principalTable: "GRUPO",
                        principalColumn: "Numero",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GRUPO_PROFESOR_PROFESOR_PROFESOR_Identificacion",
                        column: x => x.PROFESOR_Identificacion,
                        principalTable: "PROFESOR",
                        principalColumn: "Identificacion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GRUPO_PROFESOR_PROFESOR_Identificacion",
                table: "GRUPO_PROFESOR",
                column: "PROFESOR_Identificacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GRUPO_PROFESOR");

            migrationBuilder.AddColumn<int>(
                name: "PROFESOR_Identificacion",
                table: "GRUPO_ESTUDIANTE",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GRUPO_ESTUDIANTE_PROFESOR_Identificacion",
                table: "GRUPO_ESTUDIANTE",
                column: "PROFESOR_Identificacion");

            migrationBuilder.AddForeignKey(
                name: "FK_GRUPO_ESTUDIANTE_PROFESOR_PROFESOR_Identificacion",
                table: "GRUPO_ESTUDIANTE",
                column: "PROFESOR_Identificacion",
                principalTable: "PROFESOR",
                principalColumn: "Identificacion",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
