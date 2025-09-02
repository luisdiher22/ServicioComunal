-- Verificar la información de Elena Castro
SELECT 
    u.Identificacion,
    u.NombreUsuario,
    u.Contraseña,
    u.Rol,
    u.RequiereCambioContraseña,
    p.Nombre,
    p.Apellidos,
    p.Rol as RolProfesor
FROM USUARIO u
INNER JOIN PROFESOR p ON u.Identificacion = p.Identificacion
WHERE p.Nombre LIKE '%Elena%' OR u.NombreUsuario LIKE '%elena%';
