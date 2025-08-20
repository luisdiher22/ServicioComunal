-- Script para agregar profesores con rol "Tutor" para pruebas
-- Ejecutar después de los datos de prueba iniciales

-- Agregar profesores con rol Tutor
INSERT INTO PROFESOR (Identificacion, Nombre, Apellidos, Rol) VALUES
(234567890, 'Patricia', 'Rodríguez Campos', 'Tutor'),
(345678901, 'Miguel', 'Sánchez Herrera', 'Tutor'),
(567890123, 'Elena', 'Castro Mendoza', 'Tutor');

-- Agregar usuarios para los tutores (contraseña: password123)
INSERT INTO USUARIO (Identificacion, Usuario, Contraseña, Rol, FechaCreacion, Activo) VALUES
(234567890, 'patricia.rodriguez', 'b7a9c94b9a4bfab7f3b63c9fbfdc9c2d5f26d5e86a1c5f8b3c2a9d7b6f4e2d1a$ZGVmYXVsdFNhbHQ=', 'Profesor', GETDATE(), 1),
(345678901, 'miguel.sanchez', 'b7a9c94b9a4bfab7f3b63c9fbfdc9c2d5f26d5e86a1c5f8b3c2a9d7b6f4e2d1a$ZGVmYXVsdFNhbHQ=', 'Profesor', GETDATE(), 1),
(567890123, 'elena.castro', 'b7a9c94b9a4bfab7f3b63c9fbfdc9c2d5f26d5e86a1c5f8b3c2a9d7b6f4e2d1a$ZGVmYXVsdFNhbHQ=', 'Profesor', GETDATE(), 1);

-- Asignar algunos grupos a los tutores
INSERT INTO GRUPO_PROFESOR (GRUPO_Numero, PROFESOR_Identificacion, FechaAsignacion) VALUES
(3, 234567890, GETDATE()),  -- Grupo 3 asignado a Patricia
(1, 345678901, GETDATE()),  -- Grupo 1 también asignado a Miguel (co-tutoria)
(2, 567890123, GETDATE());  -- Grupo 2 también asignado a Elena (co-tutoria)

-- Verificar los tutores creados
SELECT 
    p.Identificacion,
    p.Nombre,
    p.Apellidos,
    p.Rol,
    u.Usuario
FROM PROFESOR p
INNER JOIN USUARIO u ON p.Identificacion = u.Identificacion
WHERE p.Rol = 'Tutor';
