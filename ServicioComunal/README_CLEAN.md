# Sistema de Gestión de Servicio Comunal ⚡

## 📋 Descripción del Proyecto

Sistema web desarrollado en **ASP.NET Core** para la gestión de estudiantes, profesores y grupos en programas de servicio comunal. Permite la administración de usuarios, creación de grupos de trabajo, gestión de entregas y seguimiento de actividades.

## 🚀 Características Principales

### ✅ **Sistema de Autenticación Seguro**
- Hash de contraseñas con SHA256 + salt
- Gestión de sesiones
- Roles diferenciados (Estudiante, Profesor, Administrador)
- Cambio obligatorio de contraseña en primer acceso

### ✅ **Gestión de Grupos**
- Creación automática de grupos con líderes
- Solicitudes de ingreso a grupos
- Administración de miembros
- Límite de 4 estudiantes por grupo

### ✅ **Panel de Administración**
- Gestión completa de usuarios
- Reinicio de grupos
- Monitoreo del sistema
- Seeder de datos automático

### ✅ **Dashboards Específicos por Rol**
- Dashboard de estudiantes con información del grupo
- Panel de tutores para supervisión
- Área administrativa completa

## 🛠️ Tecnologías Utilizadas

- **Framework**: ASP.NET Core 8.0
- **Base de Datos**: SQL Server con Entity Framework Core
- **Autenticación**: Session-based authentication
- **Frontend**: MVC con Razor Views
- **ORM**: Entity Framework Core
- **Arquitectura**: MVC (Model-View-Controller)

## 📁 Estructura del Proyecto

```
ServicioComunal/
├── Controllers/           # Controladores MVC
│   ├── AuthController.cs         # Autenticación y sesiones
│   ├── EstudianteController.cs   # Funcionalidades de estudiantes
│   ├── TutorController.cs        # Panel de tutores
│   ├── AdminController.cs        # Administración del sistema
│   └── HomeController.cs         # Página principal
├── Models/               # Modelos de datos
│   ├── Usuario.cs               # Modelo de usuarios
│   ├── Estudiante.cs            # Modelo de estudiantes
│   ├── Profesor.cs              # Modelo de profesores
│   ├── Grupo.cs                 # Modelo de grupos
│   ├── Solicitud.cs             # Solicitudes de ingreso
│   ├── Entrega.cs               # Entregas y tareas
│   └── ...
├── Data/                 # Contexto de base de datos
│   └── ServicioComunalDbContext.cs
├── Services/             # Servicios de la aplicación
│   ├── DataSeederService.cs     # Poblado inicial de datos
│   └── UsuarioService.cs        # Servicios de usuario
├── Utilities/            # Utilidades del sistema
├── Views/                # Vistas Razor
├── wwwroot/              # Archivos estáticos
└── Migrations/           # Migraciones de EF Core
```

## 🗄️ Modelo de Base de Datos

### Entidades Principales:
- **Usuario**: Autenticación y roles
- **Estudiante**: Información de estudiantes
- **Profesor**: Información de profesores/tutores
- **Grupo**: Grupos de trabajo con líderes
- **GrupoEstudiante**: Relación muchos a muchos
- **Solicitud**: Solicitudes de ingreso a grupos
- **Entrega**: Tareas y entregas del sistema
- **Formulario**: Formularios del sistema
- **Notificacion**: Sistema de notificaciones

## ⚙️ Configuración y Despliegue

### 1️⃣ Requisitos Previos
- .NET 8.0 SDK
- SQL Server (LocalDB o instancia completa)
- Visual Studio 2022 o VS Code

### 2️⃣ Configuración de Base de Datos
```bash
# Actualizar la cadena de conexión en appsettings.json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ServicioComunalDB;Trusted_Connection=true;MultipleActiveResultSets=true"

# Aplicar migraciones
dotnet ef database update
```

### 3️⃣ Ejecutar la Aplicación
```bash
cd ServicioComunal
dotnet run
```

### 4️⃣ Datos Iniciales
Al ejecutar la aplicación por primera vez, se poblarán automáticamente datos de prueba:

**Usuarios por Defecto:**
- **admin** / password123 (Administrador)
- **carlos.jimenez** / password123 (Profesor)
- **ana.mora** / password123 (Profesor)  
- **luis.perez** / password123 (Estudiante)
- **sofia.ramirez** / password123 (Estudiante)
- **diego.hernandez** / password123 (Estudiante)
- **camila.vargas** / password123 (Estudiante)

## 🔐 Seguridad Implementada

### Autenticación
- Hash seguro de contraseñas (SHA256 + salt)
- Validación de sesiones
- Contraseñas temporales para nuevos usuarios

### Autorización
- Control de acceso por roles
- Validación de permisos en cada acción
- Restricción de acceso a funcionalidades según el rol

### Protección de Datos
- Validación de entrada en formularios
- Prevención de SQL Injection mediante EF Core
- Manejo seguro de sesiones

## 🌐 URLs del Sistema

- **🏠 Inicio**: `/`
- **🔐 Login**: `/Auth/Login`
- **📊 Dashboard Estudiante**: `/Estudiante/Dashboard`
- **👨‍🏫 Dashboard Tutor**: `/Tutor/Dashboard`
- **⚙️ Panel Admin**: `/Admin/Dashboard`
- **🚪 Logout**: `/Auth/Logout`

## 🚦 Flujo de Trabajo

### Para Estudiantes:
1. **Login** con credenciales
2. **Dashboard** con información del grupo
3. **Gestión de Grupos** - crear o solicitar ingreso
4. **Solicitudes** - ver enviadas y recibidas
5. **Perfil** - información personal

### Para Tutores:
1. **Login** con credenciales de profesor
2. **Dashboard** con grupos asignados
3. **Supervisión** de entregas y actividades
4. **Gestión** de estudiantes del grupo

### Para Administradores:
1. **Panel completo** de administración
2. **Gestión de usuarios** y grupos
3. **Reinicio del sistema** cuando sea necesario
4. **Monitoreo** de actividades

## 📈 Funcionalidades Futuras

- [ ] Sistema de notificaciones por email
- [ ] Calendario de entregas
- [ ] Reportes y estadísticas avanzadas
- [ ] Integración con APIs externas
- [ ] App móvil complementaria

## 🐛 Solución de Problemas

### Error de Conexión a BD
```bash
# Verificar que SQL Server esté ejecutándose
# Actualizar la cadena de conexión en appsettings.json
# Ejecutar migraciones: dotnet ef database update
```

### Problemas de Login
```bash
# Verificar que los datos del seeder se cargaron correctamente
# Navegar a /Home/VerificarUsuarios para confirmar usuarios creados
# Usar las credenciales por defecto listadas arriba
```

## 👥 Contribución

1. Fork el proyecto
2. Crear una rama para la funcionalidad (`git checkout -b feature/AmazingFeature`)
3. Commit de los cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abrir un Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo `LICENSE` para más detalles.

## 📞 Contacto

**Desarrollador**: [Tu Nombre]  
**Email**: [tu.email@ejemplo.com]  
**Proyecto**: [Link al repositorio]

---

⚡ **Sistema de Servicio Comunal** - Facilitando la gestión educativa desde 2024
