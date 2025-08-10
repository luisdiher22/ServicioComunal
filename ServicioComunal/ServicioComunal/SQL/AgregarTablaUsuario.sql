-- Script SQL para agregar la tabla USUARIO al modelo relacional existente
-- Este script debe ejecutarse después de crear las tablas originales

-- Tabla: USUARIO
CREATE TABLE USUARIO (
    Identificacion int NOT NULL,
    Usuario varchar(50) NOT NULL,
    Contraseña varchar(255) NOT NULL,
    Rol varchar(20) NOT NULL,
    FechaCreacion datetime NOT NULL DEFAULT GETDATE(),
    UltimoAcceso datetime NULL,
    Activo bit NOT NULL DEFAULT 1,
    CONSTRAINT USUARIO_pk PRIMARY KEY (Identificacion),
    CONSTRAINT USUARIO_Usuario_uk UNIQUE (Usuario)
);

-- Foreign keys para USUARIO
-- Un usuario puede ser un estudiante O un profesor (pero no ambos)
-- La relación se maneja a nivel de aplicación

-- Comentarios sobre la tabla USUARIO:
-- - Identificacion: Cédula de identidad (misma que en ESTUDIANTE o PROFESOR)
-- - Usuario: Nombre de usuario único para login
-- - Contraseña: Hash de la contraseña (nunca almacenar en texto plano)
-- - Rol: 'Estudiante', 'Profesor', 'Administrador'
-- - FechaCreacion: Fecha de creación del usuario
-- - UltimoAcceso: Última vez que el usuario hizo login
-- - Activo: Si el usuario está activo o no

-- Índices adicionales para mejorar rendimiento
CREATE INDEX IX_USUARIO_Rol ON USUARIO(Rol);
CREATE INDEX IX_USUARIO_Activo ON USUARIO(Activo);
CREATE INDEX IX_USUARIO_UltimoAcceso ON USUARIO(UltimoAcceso);

-- Ejemplos de inserción de usuarios (con contraseñas hasheadas)
-- IMPORTANTE: Estas son solo para pruebas, en producción usar el sistema de hash implementado
/*
INSERT INTO USUARIO (Identificacion, Usuario, Contraseña, Rol, FechaCreacion, Activo) VALUES
(123456789, 'admin', 'hash_de_contraseña_admin', 'Administrador', GETDATE(), 1),
(987654321, 'profesor1', 'hash_de_contraseña_profesor', 'Profesor', GETDATE(), 1),
(111222333, 'estudiante1', 'hash_de_contraseña_estudiante', 'Estudiante', GETDATE(), 1);
*/
