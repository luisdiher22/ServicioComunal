-- Script para insertar datos de prueba en la base de datos ServicioComunal
-- Ejecutar después de aplicar las migraciones

-- Insertar profesores de prueba
INSERT INTO PROFESOR (Identificacion, Nombre, Apellidos, Rol) VALUES
(123456789, 'María', 'González Rodríguez', 'Coordinadora'),
(987654321, 'Carlos', 'Jiménez Vargas', 'Profesor Guía'),
(456789123, 'Ana', 'Mora Solís', 'Profesor Supervisor');

-- Insertar estudiantes de prueba
INSERT INTO ESTUDIANTE (Identificacion, Nombre, Apellidos, Clase) VALUES
(111222333, 'Luis', 'Pérez Castro', '11-A'),
(444555666, 'Sofia', 'Ramírez León', '11-B'),
(777888999, 'Diego', 'Hernández Vega', '10-A'),
(101112131, 'Camila', 'Vargas Muñoz', '10-B');

-- Insertar grupos de prueba
INSERT INTO GRUPO (Numero) VALUES
(1),
(2),
(3);

-- Insertar usuarios de prueba (con contraseñas hasheadas)
-- Contraseña para todos es "password123" (en producción usar el PasswordHelper)
INSERT INTO USUARIO (Identificacion, Usuario, Contraseña, Rol, FechaCreacion, Activo) VALUES
-- Administrador
(123456789, 'admin', 'b7a9c94b9a4bfab7f3b63c9fbfdc9c2d5f26d5e86a1c5f8b3c2a9d7b6f4e2d1a$ZGVmYXVsdFNhbHQ=', 'Administrador', GETDATE(), 1),
-- Profesores
(987654321, 'carlos.jimenez', 'b7a9c94b9a4bfab7f3b63c9fbfdc9c2d5f26d5e86a1c5f8b3c2a9d7b6f4e2d1a$ZGVmYXVsdFNhbHQ=', 'Profesor', GETDATE(), 1),
(456789123, 'ana.mora', 'b7a9c94b9a4bfab7f3b63c9fbfdc9c2d5f26d5e86a1c5f8b3c2a9d7b6f4e2d1a$ZGVmYXVsdFNhbHQ=', 'Profesor', GETDATE(), 1),
-- Estudiantes  
(111222333, 'luis.perez', 'b7a9c94b9a4bfab7f3b63c9fbfdc9c2d5f26d5e86a1c5f8b3c2a9d7b6f4e2d1a$ZGVmYXVsdFNhbHQ=', 'Estudiante', GETDATE(), 1),
(444555666, 'sofia.ramirez', 'b7a9c94b9a4bfab7f3b63c9fbfdc9c2d5f26d5e86a1c5f8b3c2a9d7b6f4e2d1a$ZGVmYXVsdFNhbHQ=', 'Estudiante', GETDATE(), 1),
(777888999, 'diego.hernandez', 'b7a9c94b9a4bfab7f3b63c9fbfdc9c2d5f26d5e86a1c5f8b3c2a9d7b6f4e2d1a$ZGVmYXVsdFNhbHQ=', 'Estudiante', GETDATE(), 1),
(101112131, 'camila.vargas', 'b7a9c94b9a4bfab7f3b63c9fbfdc9c2d5f26d5e86a1c5f8b3c2a9d7b6f4e2d1a$ZGVmYXVsdFNhbHQ=', 'Estudiante', GETDATE(), 1);

-- Asignar estudiantes a grupos
INSERT INTO GRUPO_ESTUDIANTE (ESTUDIANTE_Identificacion, GRUPO_Numero, PROFESOR_Identificacion) VALUES
(111222333, 1, 987654321),  -- Luis en grupo 1 con Carlos
(444555666, 1, 987654321),  -- Sofia en grupo 1 con Carlos
(777888999, 2, 456789123),  -- Diego en grupo 2 con Ana
(101112131, 2, 456789123);  -- Camila en grupo 2 con Ana

-- Insertar una entrega de prueba
INSERT INTO ENTREGA (Identificacion, Nombre, Descripcion, ArchivoRuta, FechaLimite, Retroalimentacion, FechaRetroalimentacion, GRUPO_Numero, PROFESOR_Identificacion) VALUES
(1, 'Propuesta de Proyecto', 'Primera entrega: propuesta del proyecto de servicio comunal', '/uploads/entregas/propuesta_1.pdf', DATEADD(day, 30, GETDATE()), 'Pendiente de revisión', GETDATE(), 1, 987654321);

-- Insertar un formulario de prueba
INSERT INTO FORMULARIO (Identificacion, Nombre, Descripcion, ArchivoRuta, FechaIngreso, PROFESOR_Identificacion) VALUES
(1, 'Formulario de Inscripción', 'Formulario inicial para inscribirse al servicio comunal', '/uploads/formularios/inscripcion.pdf', GETDATE(), 123456789);

-- Insertar notificaciones de prueba
INSERT INTO NOTIFICACION (Identificacion, Mensaje, FechaHora, Leido, GRUPO_Numero, PROFESOR_Identificacion) VALUES
(1, 'Bienvenidos al programa de Servicio Comunal. Por favor revisen el formulario de inscripción.', GETDATE(), 0, 1, 987654321),
(2, 'Recordatorio: La primera entrega vence en 30 días.', DATEADD(hour, -2, GETDATE()), 0, 1, 987654321),
(3, 'Se ha creado su grupo de trabajo. Coordinen para la primera reunión.', DATEADD(hour, -1, GETDATE()), 0, 2, 456789123);

-- Verificar los datos insertados
SELECT 'PROFESORES' as Tabla, COUNT(*) as Registros FROM PROFESOR
UNION ALL
SELECT 'ESTUDIANTES', COUNT(*) FROM ESTUDIANTE  
UNION ALL
SELECT 'USUARIOS', COUNT(*) FROM USUARIO
UNION ALL
SELECT 'GRUPOS', COUNT(*) FROM GRUPO
UNION ALL
SELECT 'GRUPO_ESTUDIANTE', COUNT(*) FROM GRUPO_ESTUDIANTE
UNION ALL
SELECT 'ENTREGAS', COUNT(*) FROM ENTREGA
UNION ALL
SELECT 'FORMULARIOS', COUNT(*) FROM FORMULARIO
UNION ALL
SELECT 'NOTIFICACIONES', COUNT(*) FROM NOTIFICACION;
