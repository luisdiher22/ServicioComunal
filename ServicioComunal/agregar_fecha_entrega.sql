-- Script para agregar la columna FechaEntrega a la tabla ENTREGA
USE ServicioComunalDB;
GO

-- Verificar si la columna ya existe antes de agregarla
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ENTREGA]') AND name = 'FechaEntrega')
BEGIN
    ALTER TABLE [ENTREGA] ADD [FechaEntrega] datetime2 NULL;
    PRINT 'Columna FechaEntrega agregada exitosamente';
END
ELSE
BEGIN
    PRINT 'La columna FechaEntrega ya existe';
END

-- Verificar que FechaRetroalimentacion sea nullable
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ENTREGA]') AND name = 'FechaRetroalimentacion' AND is_nullable = 0)
BEGIN
    ALTER TABLE [ENTREGA] ALTER COLUMN [FechaRetroalimentacion] datetime2 NULL;
    PRINT 'FechaRetroalimentacion actualizada a nullable';
END
ELSE
BEGIN
    PRINT 'FechaRetroalimentacion ya es nullable';
END

GO
