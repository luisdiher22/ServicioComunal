# Changelog - Sistema de Servicio Comunal


## [1.0.0] - 2024-08-28

### Agregado
- **Sistema de Autenticación Completo**
  - Login con hash seguro de contraseñas (SHA256 + salt)
  - Gestión de sesiones con timeout de 30 minutos
  - Roles diferenciados: Estudiante, Profesor, Administrador
  - Cambio obligatorio de contraseña en primer acceso

- **Gestión de Grupos de Estudiantes**
  - Creación automática de grupos con líderes designados
  - Sistema de solicitudes de ingreso a grupos
  - Límite de 4 estudiantes por grupo
  - Administración de miembros del grupo

- **Panel de Administración**
  - Gestión completa de usuarios (crear, editar, eliminar)
  - Reinicio automático del sistema de grupos
  - Monitoreo de actividades del sistema
  - Seeder automático de datos de prueba

- **Dashboards Específicos por Rol**
  - Dashboard de estudiantes con información del grupo
  - Panel de tutores para supervisión de grupos
  - Área administrativa completa con estadísticas

- **Base de Datos y Migraciones**
  - Modelo de datos completo con 11 entidades
  - Migraciones de Entity Framework Core
  - Scripts SQL para población inicial
  - Relaciones con Foreign Keys configuradas

- **Sistema de Solicitudes**
  - Estudiantes pueden solicitar ingreso a grupos
  - Líderes pueden aprobar/rechazar solicitudes
  - Notificaciones de estado de solicitudes

### Técnico
- Framework: ASP.NET Core 8.0
- Base de Datos: SQL Server con Entity Framework Core
- Arquitectura: MVC (Model-View-Controller)
- Autenticación: Session-based authentication
- Seguridad: Hash de contraseñas con salt, validación de sesiones

### Estructura del Proyecto
- **Controllers/**: 6 controladores principales
- **Models/**: 11 modelos de entidades
- **Services/**: 2 servicios (DataSeeder, UsuarioService)
- **Data/**: Contexto de Entity Framework
- **Migrations/**: 10 migraciones de base de datos
- **Views/**: Vistas Razor organizadas por controlador
- **SQL/**: 6 scripts SQL para mantenimiento

### Datos de Prueba Incluidos
- 1 Usuario Administrador
- 2 Profesores/Tutores
- 4 Estudiantes
- Grupos de ejemplo preconfigurados
- Contraseñas por defecto: `password123`

### URLs del Sistema Implementadas
- `/Auth/Login` - Página de inicio de sesión
- `/Estudiante/Dashboard` - Panel de estudiantes
- `/Tutor/Dashboard` - Panel de tutores
- `/Admin/Dashboard` - Panel de administración
- `/Auth/Logout` - Cerrar sesión

## [0.9.0] - 2024-08-26

### Agregado
- Configuración inicial del proyecto ASP.NET Core
- Estructura básica MVC
- Configuración de Entity Framework Core
- Modelos de datos iniciales

### Corregido
- **CRÍTICO**: Error de Foreign Key constraint resuelto
- Reorganización del DataSeeder para insertar datos en orden correcto
- Compilación exitosa sin errores de dependencias

## [0.8.0] - 2024-08-25

### Agregado
- Modelo de datos inicial
- Configuración de base de datos SQL Server
- Migraciones básicas de Entity Framework

### Corregido
- Relaciones entre entidades
- Configuración de claves foráneas

## Tipos de Cambios

- `Agregado` para nuevas funcionalidades
- `Corregido` para corrección de bugs
- `Cambiado` para cambios en funcionalidades existentes
- `Eliminado` para funcionalidades removidas
- `Seguridad` para vulnerabilidades corregidas
- `Rendimiento` para mejoras de rendimiento

## Próximas Versiones Planificadas

### [1.1.0] - Mejoras de UX/UI
- [ ] Interfaz mejorada con Bootstrap 5
- [ ] Responsive design para móviles
- [ ] Temas de color personalizables
- [ ] Mejoras en navegación

### [1.2.0] - Funcionalidades Avanzadas
- [ ] Sistema de notificaciones por email
- [ ] Calendario de entregas y actividades
- [ ] Reportes y estadísticas avanzadas
- [ ] Exportación de datos a Excel/PDF

### [1.3.0] - Integración y API
- [ ] API REST para integración externa
- [ ] Integración con sistemas académicos
- [ ] Single Sign-On (SSO)
- [ ] Webhooks para notificaciones

---

