-- Script SQL corregido para el modelo relacional de Servicio Comunal
-- IMPORTANTE: Este script corrige el tipo de dato FechaLimite a datetime

-- Table: ENTREGA (CORREGIDA)
CREATE TABLE ENTREGA (
    Identificacion int  NOT NULL,
    Nombre varchar(255)  NOT NULL,
    Descripcion varchar(500)  NOT NULL,
    ArchivoRuta nvarchar(500)  NOT NULL,
    FechaLimite datetime  NOT NULL,  -- CAMBIADO DE int A datetime
    Retroalimentacion varchar(1000)  NOT NULL,
    FechaRetroalimentacion datetime  NOT NULL,
    GRUPO_Numero int  NOT NULL,
    PROFESOR_Identificacion int  NOT NULL,
    CONSTRAINT ENTREGA_pk PRIMARY KEY (Identificacion)
);

-- Table: ESTUDIANTE
CREATE TABLE ESTUDIANTE (
    Identificacion int  NOT NULL,
    Nombre Varchar(100)  NOT NULL,
    Apellidos Varchar(100)  NOT NULL,
    Clase Varchar(50)  NOT NULL,
    CONSTRAINT ESTUDIANTE_pk PRIMARY KEY (Identificacion)
);

-- Table: FORMULARIO
CREATE TABLE FORMULARIO (
    Identificacion int  NOT NULL,
    Nombre varchar(255)  NOT NULL,
    Descripcion varchar(500)  NOT NULL,
    ArchivoRuta varchar(500)  NOT NULL,
    FechaIngreso datetime  NOT NULL,
    PROFESOR_Identificacion int  NOT NULL,
    CONSTRAINT FORMULARIO_pk PRIMARY KEY (Identificacion)
);

-- Table: GRUPO
CREATE TABLE GRUPO (
    Numero int  NOT NULL,
    CONSTRAINT GRUPO_pk PRIMARY KEY (Numero)
);

-- Table: GRUPO_ESTUDIANTE
CREATE TABLE GRUPO_ESTUDIANTE (
    ESTUDIANTE_Identificacion int  NOT NULL,
    GRUPO_Numero int  NOT NULL,
    PROFESOR_Identificacion int  NOT NULL,
    CONSTRAINT GRUPO_ESTUDIANTE_pk PRIMARY KEY (ESTUDIANTE_Identificacion,GRUPO_Numero)
);

-- Table: NOTIFICACION
CREATE TABLE NOTIFICACION (
    Identificacion int  NOT NULL,
    Mensaje varchar(1000)  NOT NULL,
    FechaHora datetime  NOT NULL,
    Leido bit  NOT NULL,
    GRUPO_Numero int  NOT NULL,
    PROFESOR_Identificacion int  NOT NULL,
    CONSTRAINT NOTIFICACION_pk PRIMARY KEY (Identificacion)
);

-- Table: PROFESOR
CREATE TABLE PROFESOR (
    Identificacion int  NOT NULL,
    Nombre Varchar(100)  NOT NULL,
    Apellidos Varchar(100)  NOT NULL,
    Rol Varchar(50)  NOT NULL,
    CONSTRAINT PROFESOR_pk PRIMARY KEY (Identificacion)
);

-- Table: USUARIO (NUEVA)
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

-- Foreign keys
-- Reference: ENTREGA_GRUPO (table: ENTREGA)
ALTER TABLE ENTREGA ADD CONSTRAINT ENTREGA_GRUPO 
    FOREIGN KEY (GRUPO_Numero) REFERENCES GRUPO (Numero);

-- Reference: ENTREGA_PROFESOR (table: ENTREGA)
ALTER TABLE ENTREGA ADD CONSTRAINT ENTREGA_PROFESOR 
    FOREIGN KEY (PROFESOR_Identificacion) REFERENCES PROFESOR (Identificacion);

-- Reference: ESTUDIANTE_GRUPO_ESTUDIANTE (table: GRUPO_ESTUDIANTE)
ALTER TABLE GRUPO_ESTUDIANTE ADD CONSTRAINT ESTUDIANTE_GRUPO_ESTUDIANTE 
    FOREIGN KEY (ESTUDIANTE_Identificacion) REFERENCES ESTUDIANTE (Identificacion);

-- Reference: FORMULARIO_PROFESOR (table: FORMULARIO)
ALTER TABLE FORMULARIO ADD CONSTRAINT FORMULARIO_PROFESOR 
    FOREIGN KEY (PROFESOR_Identificacion) REFERENCES PROFESOR (Identificacion);

-- Reference: GRUPO_ESTUDIANTE_PROFESOR (table: GRUPO_ESTUDIANTE)
ALTER TABLE GRUPO_ESTUDIANTE ADD CONSTRAINT GRUPO_ESTUDIANTE_PROFESOR 
    FOREIGN KEY (PROFESOR_Identificacion) REFERENCES PROFESOR (Identificacion);

-- Reference: GRUPO_GRUPO_ESTUDIANTE (table: GRUPO_ESTUDIANTE)
ALTER TABLE GRUPO_ESTUDIANTE ADD CONSTRAINT GRUPO_GRUPO_ESTUDIANTE 
    FOREIGN KEY (GRUPO_Numero) REFERENCES GRUPO (Numero);

-- Reference: NOTIFICACION_GRUPO (table: NOTIFICACION)
ALTER TABLE NOTIFICACION ADD CONSTRAINT NOTIFICACION_GRUPO 
    FOREIGN KEY (GRUPO_Numero) REFERENCES GRUPO (Numero);

-- Reference: NOTIFICACION_PROFESOR (table: NOTIFICACION)
ALTER TABLE NOTIFICACION ADD CONSTRAINT NOTIFICACION_PROFESOR 
    FOREIGN KEY (PROFESOR_Identificacion) REFERENCES PROFESOR (Identificacion);

-- Índices adicionales para mejorar rendimiento
CREATE INDEX IX_USUARIO_Rol ON USUARIO(Rol);
CREATE INDEX IX_USUARIO_Activo ON USUARIO(Activo);
CREATE INDEX IX_USUARIO_UltimoAcceso ON USUARIO(UltimoAcceso);
CREATE INDEX IX_ENTREGA_FechaLimite ON ENTREGA(FechaLimite);
CREATE INDEX IX_NOTIFICACION_FechaHora ON NOTIFICACION(FechaHora);
