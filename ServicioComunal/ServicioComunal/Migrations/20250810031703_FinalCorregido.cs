using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicioComunal.Migrations
{
    /// <inheritdoc />
    public partial class FinalCorregido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ESTUDIANTE",
                columns: table => new
                {
                    Identificacion = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Clase = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ESTUDIANTE", x => x.Identificacion);
                });

            migrationBuilder.CreateTable(
                name: "GRUPO",
                columns: table => new
                {
                    Numero = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GRUPO", x => x.Numero);
                });

            migrationBuilder.CreateTable(
                name: "PROFESOR",
                columns: table => new
                {
                    Identificacion = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROFESOR", x => x.Identificacion);
                });

            migrationBuilder.CreateTable(
                name: "ENTREGA",
                columns: table => new
                {
                    Identificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArchivoRuta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaLimite = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Retroalimentacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRetroalimentacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GRUPO_Numero = table.Column<int>(type: "int", nullable: false),
                    PROFESOR_Identificacion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENTREGA", x => x.Identificacion);
                    table.ForeignKey(
                        name: "FK_ENTREGA_GRUPO_GRUPO_Numero",
                        column: x => x.GRUPO_Numero,
                        principalTable: "GRUPO",
                        principalColumn: "Numero",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ENTREGA_PROFESOR_PROFESOR_Identificacion",
                        column: x => x.PROFESOR_Identificacion,
                        principalTable: "PROFESOR",
                        principalColumn: "Identificacion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FORMULARIO",
                columns: table => new
                {
                    Identificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArchivoRuta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PROFESOR_Identificacion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FORMULARIO", x => x.Identificacion);
                    table.ForeignKey(
                        name: "FK_FORMULARIO_PROFESOR_PROFESOR_Identificacion",
                        column: x => x.PROFESOR_Identificacion,
                        principalTable: "PROFESOR",
                        principalColumn: "Identificacion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GRUPO_ESTUDIANTE",
                columns: table => new
                {
                    ESTUDIANTE_Identificacion = table.Column<int>(type: "int", nullable: false),
                    GRUPO_Numero = table.Column<int>(type: "int", nullable: false),
                    PROFESOR_Identificacion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GRUPO_ESTUDIANTE", x => new { x.ESTUDIANTE_Identificacion, x.GRUPO_Numero });
                    table.ForeignKey(
                        name: "FK_GRUPO_ESTUDIANTE_ESTUDIANTE_ESTUDIANTE_Identificacion",
                        column: x => x.ESTUDIANTE_Identificacion,
                        principalTable: "ESTUDIANTE",
                        principalColumn: "Identificacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GRUPO_ESTUDIANTE_GRUPO_GRUPO_Numero",
                        column: x => x.GRUPO_Numero,
                        principalTable: "GRUPO",
                        principalColumn: "Numero",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GRUPO_ESTUDIANTE_PROFESOR_PROFESOR_Identificacion",
                        column: x => x.PROFESOR_Identificacion,
                        principalTable: "PROFESOR",
                        principalColumn: "Identificacion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NOTIFICACION",
                columns: table => new
                {
                    Identificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mensaje = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Leido = table.Column<bool>(type: "bit", nullable: false),
                    GRUPO_Numero = table.Column<int>(type: "int", nullable: false),
                    PROFESOR_Identificacion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICACION", x => x.Identificacion);
                    table.ForeignKey(
                        name: "FK_NOTIFICACION_GRUPO_GRUPO_Numero",
                        column: x => x.GRUPO_Numero,
                        principalTable: "GRUPO",
                        principalColumn: "Numero",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NOTIFICACION_PROFESOR_PROFESOR_Identificacion",
                        column: x => x.PROFESOR_Identificacion,
                        principalTable: "PROFESOR",
                        principalColumn: "Identificacion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "USUARIO",
                columns: table => new
                {
                    Identificacion = table.Column<int>(type: "int", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Contraseña = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UltimoAcceso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIO", x => x.Identificacion);
                    table.ForeignKey(
                        name: "FK_USUARIO_ESTUDIANTE_Identificacion",
                        column: x => x.Identificacion,
                        principalTable: "ESTUDIANTE",
                        principalColumn: "Identificacion",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_USUARIO_PROFESOR_Identificacion",
                        column: x => x.Identificacion,
                        principalTable: "PROFESOR",
                        principalColumn: "Identificacion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ENTREGA_GRUPO_Numero",
                table: "ENTREGA",
                column: "GRUPO_Numero");

            migrationBuilder.CreateIndex(
                name: "IX_ENTREGA_PROFESOR_Identificacion",
                table: "ENTREGA",
                column: "PROFESOR_Identificacion");

            migrationBuilder.CreateIndex(
                name: "IX_FORMULARIO_PROFESOR_Identificacion",
                table: "FORMULARIO",
                column: "PROFESOR_Identificacion");

            migrationBuilder.CreateIndex(
                name: "IX_GRUPO_ESTUDIANTE_GRUPO_Numero",
                table: "GRUPO_ESTUDIANTE",
                column: "GRUPO_Numero");

            migrationBuilder.CreateIndex(
                name: "IX_GRUPO_ESTUDIANTE_PROFESOR_Identificacion",
                table: "GRUPO_ESTUDIANTE",
                column: "PROFESOR_Identificacion");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACION_GRUPO_Numero",
                table: "NOTIFICACION",
                column: "GRUPO_Numero");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACION_PROFESOR_Identificacion",
                table: "NOTIFICACION",
                column: "PROFESOR_Identificacion");

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_Usuario",
                table: "USUARIO",
                column: "Usuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ENTREGA");

            migrationBuilder.DropTable(
                name: "FORMULARIO");

            migrationBuilder.DropTable(
                name: "GRUPO_ESTUDIANTE");

            migrationBuilder.DropTable(
                name: "NOTIFICACION");

            migrationBuilder.DropTable(
                name: "USUARIO");

            migrationBuilder.DropTable(
                name: "GRUPO");

            migrationBuilder.DropTable(
                name: "ESTUDIANTE");

            migrationBuilder.DropTable(
                name: "PROFESOR");
        }
    }
}
