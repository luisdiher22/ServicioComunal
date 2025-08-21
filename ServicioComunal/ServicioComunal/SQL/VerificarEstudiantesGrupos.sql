-- Verificar estudiantes y sus grupos
SELECT 
    E.Identificacion,
    E.Nombre,
    E.Apellidos,
    E.Clase,
    U.NombreUsuario,
    CASE WHEN GE.GrupoNumero IS NOT NULL THEN GE.GrupoNumero ELSE NULL END AS GrupoAsignado,
    CASE WHEN GE.GrupoNumero IS NOT NULL THEN 'SÍ' ELSE 'NO' END AS TieneGrupo
FROM ESTUDIANTE E
LEFT JOIN USUARIO U ON E.Identificacion = U.Identificacion
LEFT JOIN GRUPO_ESTUDIANTE GE ON E.Identificacion = GE.ESTUDIANTE_Identificacion
ORDER BY E.Identificacion;

-- Contar estudiantes con y sin grupo
SELECT 
    'Con Grupo' as Estado,
    COUNT(*) as Cantidad
FROM ESTUDIANTE E
JOIN GRUPO_ESTUDIANTE GE ON E.Identificacion = GE.ESTUDIANTE_Identificacion

UNION ALL

SELECT 
    'Sin Grupo' as Estado,
    COUNT(*) as Cantidad
FROM ESTUDIANTE E
LEFT JOIN GRUPO_ESTUDIANTE GE ON E.Identificacion = GE.ESTUDIANTE_Identificacion
WHERE GE.ESTUDIANTE_Identificacion IS NULL;
