-- Script para agregar entregas de anexos de prueba

-- Eliminar entregas existentes que puedan estar duplicadas
DELETE FROM ENTREGA WHERE Identificacion > 1;

-- Insertar entregas de anexos para el grupo 1
INSERT INTO ENTREGA (Identificacion, Nombre, Descripcion, ArchivoRuta, FechaLimite, Retroalimentacion, FechaRetroalimentacion, GRUPO_Numero, PROFESOR_Identificacion, TipoAnexo) VALUES
(2008, 'Anexo #1 - Solicitud de Inscripción', 'Formulario de solicitud para inscribirse al proyecto de servicio comunal', '', DATEADD(day, 15, GETDATE()), 'Pendiente', NULL, 1, 987654321, 1),
(2009, 'Anexo #2 - Áreas de Interés', 'Formulario para indicar áreas de interés del proyecto', '', DATEADD(day, 20, GETDATE()), 'Pendiente', NULL, 1, 987654321, 2),
(2010, 'Anexo #3 - Plan de Trabajo', 'Plan detallado del proyecto a realizar', '', DATEADD(day, 25, GETDATE()), 'Pendiente', NULL, 1, 987654321, 3),
(2011, 'Anexo #5 - Evaluación Final', 'Evaluación final del proyecto completado', '', DATEADD(day, 60, GETDATE()), 'Pendiente', NULL, 1, 987654321, 5);

-- Insertar entregas de anexos para el grupo 2
INSERT INTO ENTREGA (Identificacion, Nombre, Descripcion, ArchivoRuta, FechaLimite, Retroalimentacion, FechaRetroalimentacion, GRUPO_Numero, PROFESOR_Identificacion, TipoAnexo) VALUES
(2012, 'Anexo #1 - Solicitud de Inscripción', 'Formulario de solicitud para inscribirse al proyecto de servicio comunal', '', DATEADD(day, 15, GETDATE()), 'Pendiente', NULL, 2, 456789123, 1),
(2013, 'Anexo #2 - Áreas de Interés', 'Formulario para indicar áreas de interés del proyecto', '', DATEADD(day, 20, GETDATE()), 'Pendiente', NULL, 2, 456789123, 2),
(2014, 'Anexo #3 - Plan de Trabajo', 'Plan detallado del proyecto a realizar', '', DATEADD(day, 25, GETDATE()), 'Pendiente', NULL, 2, 456789123, 3),
(2015, 'Anexo #5 - Evaluación Final', 'Evaluación final del proyecto completado', '', DATEADD(day, 60, GETDATE()), 'Pendiente', NULL, 2, 456789123, 5);

-- Verificar las entregas creadas
SELECT 
    e.Identificacion,
    e.Nombre,
    e.TipoAnexo,
    e.GRUPO_Numero,
    p.Nombre + ' ' + p.Apellidos as Profesor
FROM ENTREGA e
INNER JOIN PROFESOR p ON e.PROFESOR_Identificacion = p.Identificacion
WHERE e.TipoAnexo > 0
ORDER BY e.GRUPO_Numero, e.TipoAnexo;
