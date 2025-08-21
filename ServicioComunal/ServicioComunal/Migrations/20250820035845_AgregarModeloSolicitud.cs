using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicioComunal.Migrations
{
    /// <inheritdoc />
    public partial class AgregarModeloSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SOLICITUD",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstudianteRemitenteId = table.Column<int>(type: "int", nullable: false),
                    EstudianteDestinatarioId = table.Column<int>(type: "int", nullable: false),
                    GrupoNumero = table.Column<int>(type: "int", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRespuesta = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SOLICITUD", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SOLICITUD_ESTUDIANTE_EstudianteDestinatarioId",
                        column: x => x.EstudianteDestinatarioId,
                        principalTable: "ESTUDIANTE",
                        principalColumn: "Identificacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SOLICITUD_ESTUDIANTE_EstudianteRemitenteId",
                        column: x => x.EstudianteRemitenteId,
                        principalTable: "ESTUDIANTE",
                        principalColumn: "Identificacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SOLICITUD_GRUPO_GrupoNumero",
                        column: x => x.GrupoNumero,
                        principalTable: "GRUPO",
                        principalColumn: "Numero",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SOLICITUD_EstudianteDestinatarioId",
                table: "SOLICITUD",
                column: "EstudianteDestinatarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SOLICITUD_EstudianteRemitenteId",
                table: "SOLICITUD",
                column: "EstudianteRemitenteId");

            migrationBuilder.CreateIndex(
                name: "IX_SOLICITUD_GrupoNumero",
                table: "SOLICITUD",
                column: "GrupoNumero");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SOLICITUD");
        }
    }
}
