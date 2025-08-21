-- Verificar estudiantes existentes
SELECT E.Identificacion, E.Nombre, E.Apellidos, E.Clase, 
       CASE WHEN U.Identificacion IS NOT NULL THEN 'Sí' ELSE 'No' END AS TieneUsuario
FROM ESTUDIANTE E
LEFT JOIN USUARIO U ON E.Identificacion = U.Identificacion
ORDER BY E.Identificacion;

-- Crear usuarios para estudiantes que no tienen usuario
INSERT INTO USUARIO (Identificacion, NombreUsuario, Contraseña, Rol, FechaCreacion, Activo)
SELECT E.Identificacion, 
       CAST(E.Identificacion AS VARCHAR(50)), 
       '$2a$11$N0yTJo/Pc3mzQdH3J1FYbuBU7DYHRjGG6/VVzH.WHFP.YHgOzjzTO', -- Password: "12345" hasheado
       'Estudiante',
       GETDATE(),
       1
FROM ESTUDIANTE E
LEFT JOIN USUARIO U ON E.Identificacion = U.Identificacion
WHERE U.Identificacion IS NULL;

-- Verificar usuarios creados
SELECT U.Identificacion, U.NombreUsuario, U.Rol, 
       E.Nombre + ' ' + E.Apellidos AS NombreCompleto
FROM USUARIO U
JOIN ESTUDIANTE E ON U.Identificacion = E.Identificacion
WHERE U.Rol = 'Estudiante'
ORDER BY U.Identificacion;
