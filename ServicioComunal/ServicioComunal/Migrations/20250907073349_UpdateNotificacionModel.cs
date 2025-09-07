using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicioComunal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificacionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NOTIFICACION_GRUPO_GRUPO_Numero",
                table: "NOTIFICACION");

            migrationBuilder.DropForeignKey(
                name: "FK_NOTIFICACION_PROFESOR_PROFESOR_Identificacion",
                table: "NOTIFICACION");

            migrationBuilder.RenameColumn(
                name: "PROFESOR_Identificacion",
                table: "NOTIFICACION",
                newName: "ProfesorIdentificacion");

            migrationBuilder.RenameColumn(
                name: "GRUPO_Numero",
                table: "NOTIFICACION",
                newName: "GrupoNumero");

            migrationBuilder.RenameIndex(
                name: "IX_NOTIFICACION_PROFESOR_Identificacion",
                table: "NOTIFICACION",
                newName: "IX_NOTIFICACION_ProfesorIdentificacion");

            migrationBuilder.RenameIndex(
                name: "IX_NOTIFICACION_GRUPO_Numero",
                table: "NOTIFICACION",
                newName: "IX_NOTIFICACION_GrupoNumero");

            migrationBuilder.AlterColumn<int>(
                name: "ProfesorIdentificacion",
                table: "NOTIFICACION",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "GrupoNumero",
                table: "NOTIFICACION",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "EntregaId",
                table: "NOTIFICACION",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GrupoId",
                table: "NOTIFICACION",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoNotificacion",
                table: "NOTIFICACION",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioDestino",
                table: "NOTIFICACION",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioOrigen",
                table: "NOTIFICACION",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACION_EntregaId",
                table: "NOTIFICACION",
                column: "EntregaId");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACION_GrupoId",
                table: "NOTIFICACION",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACION_UsuarioDestino",
                table: "NOTIFICACION",
                column: "UsuarioDestino");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICACION_UsuarioOrigen",
                table: "NOTIFICACION",
                column: "UsuarioOrigen");

            migrationBuilder.AddForeignKey(
                name: "FK_NOTIFICACION_ENTREGA_EntregaId",
                table: "NOTIFICACION",
                column: "EntregaId",
                principalTable: "ENTREGA",
                principalColumn: "Identificacion",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NOTIFICACION_GRUPO_GrupoId",
                table: "NOTIFICACION",
                column: "GrupoId",
                principalTable: "GRUPO",
                principalColumn: "Numero",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NOTIFICACION_GRUPO_GrupoNumero",
                table: "NOTIFICACION",
                column: "GrupoNumero",
                principalTable: "GRUPO",
                principalColumn: "Numero");

            migrationBuilder.AddForeignKey(
                name: "FK_NOTIFICACION_PROFESOR_ProfesorIdentificacion",
                table: "NOTIFICACION",
                column: "ProfesorIdentificacion",
                principalTable: "PROFESOR",
                principalColumn: "Identificacion");

            migrationBuilder.AddForeignKey(
                name: "FK_NOTIFICACION_USUARIO_UsuarioDestino",
                table: "NOTIFICACION",
                column: "UsuarioDestino",
                principalTable: "USUARIO",
                principalColumn: "Identificacion",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NOTIFICACION_USUARIO_UsuarioOrigen",
                table: "NOTIFICACION",
                column: "UsuarioOrigen",
                principalTable: "USUARIO",
                principalColumn: "Identificacion",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NOTIFICACION_ENTREGA_EntregaId",
                table: "NOTIFICACION");

            migrationBuilder.DropForeignKey(
                name: "FK_NOTIFICACION_GRUPO_GrupoId",
                table: "NOTIFICACION");

            migrationBuilder.DropForeignKey(
                name: "FK_NOTIFICACION_GRUPO_GrupoNumero",
                table: "NOTIFICACION");

            migrationBuilder.DropForeignKey(
                name: "FK_NOTIFICACION_PROFESOR_ProfesorIdentificacion",
                table: "NOTIFICACION");

            migrationBuilder.DropForeignKey(
                name: "FK_NOTIFICACION_USUARIO_UsuarioDestino",
                table: "NOTIFICACION");

            migrationBuilder.DropForeignKey(
                name: "FK_NOTIFICACION_USUARIO_UsuarioOrigen",
                table: "NOTIFICACION");

            migrationBuilder.DropIndex(
                name: "IX_NOTIFICACION_EntregaId",
                table: "NOTIFICACION");

            migrationBuilder.DropIndex(
                name: "IX_NOTIFICACION_GrupoId",
                table: "NOTIFICACION");

            migrationBuilder.DropIndex(
                name: "IX_NOTIFICACION_UsuarioDestino",
                table: "NOTIFICACION");

            migrationBuilder.DropIndex(
                name: "IX_NOTIFICACION_UsuarioOrigen",
                table: "NOTIFICACION");

            migrationBuilder.DropColumn(
                name: "EntregaId",
                table: "NOTIFICACION");

            migrationBuilder.DropColumn(
                name: "GrupoId",
                table: "NOTIFICACION");

            migrationBuilder.DropColumn(
                name: "TipoNotificacion",
                table: "NOTIFICACION");

            migrationBuilder.DropColumn(
                name: "UsuarioDestino",
                table: "NOTIFICACION");

            migrationBuilder.DropColumn(
                name: "UsuarioOrigen",
                table: "NOTIFICACION");

            migrationBuilder.RenameColumn(
                name: "ProfesorIdentificacion",
                table: "NOTIFICACION",
                newName: "PROFESOR_Identificacion");

            migrationBuilder.RenameColumn(
                name: "GrupoNumero",
                table: "NOTIFICACION",
                newName: "GRUPO_Numero");

            migrationBuilder.RenameIndex(
                name: "IX_NOTIFICACION_ProfesorIdentificacion",
                table: "NOTIFICACION",
                newName: "IX_NOTIFICACION_PROFESOR_Identificacion");

            migrationBuilder.RenameIndex(
                name: "IX_NOTIFICACION_GrupoNumero",
                table: "NOTIFICACION",
                newName: "IX_NOTIFICACION_GRUPO_Numero");

            migrationBuilder.AlterColumn<int>(
                name: "PROFESOR_Identificacion",
                table: "NOTIFICACION",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GRUPO_Numero",
                table: "NOTIFICACION",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NOTIFICACION_GRUPO_GRUPO_Numero",
                table: "NOTIFICACION",
                column: "GRUPO_Numero",
                principalTable: "GRUPO",
                principalColumn: "Numero",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NOTIFICACION_PROFESOR_PROFESOR_Identificacion",
                table: "NOTIFICACION",
                column: "PROFESOR_Identificacion",
                principalTable: "PROFESOR",
                principalColumn: "Identificacion",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
