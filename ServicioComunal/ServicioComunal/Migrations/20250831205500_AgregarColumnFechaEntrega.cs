using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicioComunal.Migrations
{
    /// <inheritdoc />
    public partial class AgregarColumnFechaEntrega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Solo agregar la columna FechaEntrega si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ENTREGA]') AND name = 'FechaEntrega')
                BEGIN
                    ALTER TABLE [ENTREGA] ADD [FechaEntrega] datetime2 NULL;
                END
            ");
            
            // Hacer FechaRetroalimentacion nullable si no lo es
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ENTREGA]') AND name = 'FechaRetroalimentacion' AND is_nullable = 0)
                BEGIN
                    ALTER TABLE [ENTREGA] ALTER COLUMN [FechaRetroalimentacion] datetime2 NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaEntrega",
                table: "ENTREGA");
                
            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaRetroalimentacion",
                table: "ENTREGA",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
