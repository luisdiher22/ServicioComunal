# Gestión de Grupos - DESHABILITADA

## ¿Por qué se deshabilitó GestionGrupos para estudiantes?

La funcionalidad de "Gestión de Grupos" se ha deshabilitado para estudiantes por las siguientes razones:

1. **Privacidad**: Los estudiantes no deberían ver información de otros grupos
2. **Simplificación**: La interfaz era confusa y compleja para los estudiantes
3. **Enfoque**: Los estudiantes solo necesitan gestionar su propio grupo

## ¿Qué reemplaza esta funcionalidad?

Los estudiantes ahora solo tienen acceso a:
- **Mi Grupo**: Para ver y gestionar su grupo específico
- **Solicitudes**: Para manejar invitaciones y solicitudes de grupo

## Archivos afectados

- `GestionGrupos.cshtml` → `GestionGrupos.cshtml.disabled`
- `EstudianteController.cs` → Acción GestionGrupos comentada
- Todas las vistas de estudiante → Enlaces a GestionGrupos eliminados

## Para reactivar (si es necesario)

1. Descomenta la acción `GestionGrupos` en `EstudianteController.cs`
2. Renombra `GestionGrupos.cshtml.disabled` a `GestionGrupos.cshtml`
3. Agrega los enlaces de navegación de vuelta en las vistas de estudiante
