using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicioComunal.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposAnexoYLimpiarEntregas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eliminar entregas existentes para evitar conflictos
            migrationBuilder.Sql("DELETE FROM [ENTREGA]");
            
            // Agregar FechaEntrega si no existe
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
            
            // Agregar DatosFormularioJson si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ENTREGA]') AND name = 'DatosFormularioJson')
                BEGIN
                    ALTER TABLE [ENTREGA] ADD [DatosFormularioJson] nvarchar(max) NULL;
                END
            ");

            // Agregar TipoAnexo si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ENTREGA]') AND name = 'TipoAnexo')
                BEGIN
                    ALTER TABLE [ENTREGA] ADD [TipoAnexo] int NOT NULL DEFAULT 0;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ENTREGA]') AND name = 'DatosFormularioJson')
                BEGIN
                    ALTER TABLE [ENTREGA] DROP COLUMN [DatosFormularioJson];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ENTREGA]') AND name = 'TipoAnexo')
                BEGIN
                    ALTER TABLE [ENTREGA] DROP COLUMN [TipoAnexo];
                END
            ");
        }
    }
}
