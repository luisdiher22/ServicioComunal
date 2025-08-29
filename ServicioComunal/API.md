# Documentación de API - Sistema de Servicio Comunal

## Descripción General

El Sistema de Servicio Comunal expone endpoints internos para la gestión de usuarios, grupos y entregas. Esta documentación describe las rutas disponibles y su funcionalidad.

**Nota**: Esta es una aplicación MVC que utiliza formularios web. Los endpoints listados son las rutas HTTP que maneja la aplicación.

## Autenticación

### Sistema de Sesiones
- **Tipo**: Session-based authentication
- **Duración**: 30 minutos de inactividad
- **Storage**: Server-side memory cache

### Roles de Usuario
- **Administrador**: Acceso completo al sistema
- **Profesor**: Gestión de grupos y entregas
- **Estudiante**: Acceso a dashboard y gestión de grupos

## Endpoints por Controlador

### AuthController - Autenticación

#### POST /Auth/Login
**Descripción**: Iniciar sesión en el sistema  
**Método**: POST  
**Parámetros**:
```csharp
{
    "Usuario": "string",     // Nombre de usuario
    "Contraseña": "string"   // Contraseña en texto plano
}
```
**Respuesta Exitosa**:
- **200**: Redirección al dashboard correspondiente
- **400**: Credenciales incorrectas

**Ejemplo**:
```html
<!-- Formulario de login -->
<form method="post" action="/Auth/Login">
    <input name="Usuario" value="admin" />
    <input name="Contraseña" type="password" value="password123" />
    <button type="submit">Iniciar Sesión</button>
</form>
```

#### POST /Auth/Logout
**Descripción**: Cerrar sesión actual  
**Método**: POST  
**Autorización**: Usuario autenticado  
**Respuesta**: Redirección a /Auth/Login

---

### HomeController - Páginas Principales

#### GET /
**Descripción**: Página principal (redirección a login)  
**Método**: GET  
**Respuesta**: Redirección a /Auth/Login

#### GET /Home/Dashboard
**Descripción**: Dashboard principal después del login  
**Método**: GET  
**Autorización**: Usuario autenticado  
**Respuesta**: Vista del dashboard según el rol del usuario

#### GET /Home/SeedData
**Descripción**: Poblar base de datos con datos de prueba  
**Método**: GET  
**Autorización**: Solo en desarrollo  
**Respuesta**: Mensaje de confirmación de datos creados

#### GET /Home/VerificarUsuarios
**Descripción**: Verificar usuarios creados en el sistema  
**Método**: GET  
**Autorización**: Administrador  
**Respuesta**: Lista de usuarios registrados

---

### EstudianteController - Funcionalidades de Estudiantes

#### GET /Estudiante/Dashboard
**Descripción**: Dashboard específico para estudiantes  
**Método**: GET  
**Autorización**: Rol Estudiante  
**Respuesta**: Vista con información del grupo y opciones disponibles

#### GET /Estudiante/GestionGrupos
**Descripción**: Página de gestión de grupos  
**Método**: GET  
**Autorización**: Rol Estudiante  
**Respuesta**: Vista con grupos disponibles y opciones de gestión

#### POST /Estudiante/CrearGrupo
**Descripción**: Crear un nuevo grupo de trabajo  
**Método**: POST  
**Autorización**: Rol Estudiante  
**Parámetros**:
```csharp
{
    "Nombre": "string",           // Nombre del grupo
    "Descripcion": "string",      // Descripción opcional
    "ProyectoEnfoque": "string"   // Enfoque del proyecto
}
```
**Respuesta**: Redirección al dashboard con el nuevo grupo

#### POST /Estudiante/SolicitarIngreso/{grupoId}
**Descripción**: Solicitar ingreso a un grupo existente  
**Método**: POST  
**Autorización**: Rol Estudiante  
**Parámetros**: 
- `grupoId`: ID del grupo al que se quiere ingresar
**Respuesta**: Confirmación de solicitud enviada

#### GET /Estudiante/MisSolicitudes
**Descripción**: Ver solicitudes enviadas y recibidas  
**Método**: GET  
**Autorización**: Rol Estudiante  
**Respuesta**: Lista de solicitudes con su estado

#### POST /Estudiante/ProcesarSolicitud
**Descripción**: Aprobar o rechazar solicitud (solo líderes)  
**Método**: POST  
**Autorización**: Líder de grupo  
**Parámetros**:
```csharp
{
    "SolicitudId": "int",    // ID de la solicitud
    "Accion": "string"       // "Aprobar" o "Rechazar"
}
```
**Respuesta**: Confirmación de acción procesada

#### GET /Estudiante/Perfil
**Descripción**: Ver y editar perfil del estudiante  
**Método**: GET  
**Autorización**: Rol Estudiante  
**Respuesta**: Vista con información personal

---

### TutorController - Panel de Tutores

#### GET /Tutor/Dashboard
**Descripción**: Dashboard para profesores/tutores  
**Método**: GET  
**Autorización**: Rol Profesor  
**Respuesta**: Vista con grupos asignados y entregas

#### GET /Tutor/GruposAsignados
**Descripción**: Ver grupos bajo supervisión  
**Método**: GET  
**Autorización**: Rol Profesor  
**Respuesta**: Lista de grupos con estudiantes

#### GET /Tutor/GestionEntregas
**Descripción**: Gestionar entregas y tareas  
**Método**: GET  
**Autorización**: Rol Profesor  
**Respuesta**: Vista de gestión de entregas

#### POST /Tutor/CrearEntrega
**Descripción**: Crear nueva entrega para un grupo  
**Método**: POST  
**Autorización**: Rol Profesor  
**Parámetros**:
```csharp
{
    "Titulo": "string",          // Título de la entrega
    "Descripcion": "string",     // Descripción detallada
    "FechaLimite": "datetime",   // Fecha límite
    "GrupoId": "int"            // ID del grupo
}
```
**Respuesta**: Confirmación de entrega creada

---

### AdminController - Administración del Sistema

#### GET /Admin/Dashboard
**Descripción**: Panel de administración completo  
**Método**: GET  
**Autorización**: Rol Administrador  
**Respuesta**: Vista con estadísticas y opciones administrativas

#### GET /Admin/GestionUsuarios
**Descripción**: Gestionar usuarios del sistema  
**Método**: GET  
**Autorización**: Rol Administrador  
**Respuesta**: Lista de todos los usuarios

#### POST /Admin/CrearUsuario
**Descripción**: Crear nuevo usuario  
**Método**: POST  
**Autorización**: Rol Administrador  
**Parámetros**:
```csharp
{
    "Identificacion": "string",  // Cédula
    "Usuario": "string",         // Nombre de usuario
    "Rol": "string",            // Estudiante/Profesor/Administrador
    "Nombre": "string",         // Nombre completo
    "Email": "string"           // Email (opcional)
}
```
**Respuesta**: Usuario creado con contraseña temporal

#### POST /Admin/EditarUsuario/{id}
**Descripción**: Editar usuario existente  
**Método**: POST  
**Autorización**: Rol Administrador  
**Parámetros**: ID del usuario + datos a actualizar  
**Respuesta**: Confirmación de actualización

#### POST /Admin/EliminarUsuario/{id}
**Descripción**: Eliminar usuario del sistema  
**Método**: POST  
**Autorización**: Rol Administrador  
**Parámetros**: ID del usuario a eliminar  
**Respuesta**: Confirmación de eliminación

#### POST /Admin/ReiniciarSistema
**Descripción**: Reiniciar grupos del sistema  
**Método**: POST  
**Autorización**: Rol Administrador  
**Respuesta**: Confirmación de reinicio

#### GET /Admin/EstadisticasGenerales
**Descripción**: Ver estadísticas del sistema  
**Método**: GET  
**Autorización**: Rol Administrador  
**Respuesta**: Dashboard con métricas y estadísticas

---

## Modelos de Datos

### Usuario
```csharp
{
    "Identificacion": "string",      // Cédula (PK)
    "Usuario": "string",             // Nombre de usuario único
    "Contraseña": "string",          // Hash SHA256 + salt
    "Rol": "string",                 // Estudiante/Profesor/Administrador
    "FechaCreacion": "datetime",     // Fecha de creación
    "UltimoAcceso": "datetime",      // Último login
    "Activo": "boolean",             // Estado del usuario
    "RequiereCambioContraseña": "boolean"  // Cambio obligatorio
}
```

### Estudiante
```csharp
{
    "Identificacion": "string",      // Cédula (PK)
    "Nombre": "string",              // Nombre completo
    "Email": "string",               // Email opcional
    "Telefono": "string",            // Teléfono opcional
    "Direccion": "string",           // Dirección opcional
    "FechaNacimiento": "datetime",   // Fecha de nacimiento opcional
    "Activo": "boolean"              // Estado del estudiante
}
```

### Grupo
```csharp
{
    "Numero": "int",                 // Número de grupo (PK)
    "Nombre": "string",              // Nombre del grupo
    "Descripcion": "string",         // Descripción opcional
    "ProyectoEnfoque": "string",     // Enfoque del proyecto
    "FechaCreacion": "datetime",     // Fecha de creación
    "Activo": "boolean",             // Estado del grupo
    "LiderIdentificacion": "string"  // Cédula del líder
}
```

### Solicitud
```csharp
{
    "Id": "int",                     // ID único (PK)
    "EstudianteIdentificacion": "string", // Cédula del solicitante
    "GrupoNumero": "int",           // Número del grupo
    "FechaSolicitud": "datetime",    // Fecha de solicitud
    "Estado": "string",              // Pendiente/Aprobada/Rechazada
    "FechaProcesamiento": "datetime" // Fecha de procesamiento (opcional)
}
```

---

## Flujos de Trabajo

### Flujo de Autenticación
1. **GET** `/Auth/Login` - Mostrar formulario
2. **POST** `/Auth/Login` - Validar credenciales
3. Redirección según rol:
   - Estudiante → `/Estudiante/Dashboard`
   - Profesor → `/Tutor/Dashboard`
   - Admin → `/Admin/Dashboard`

### Flujo de Creación de Grupo
1. **GET** `/Estudiante/GestionGrupos` - Ver opciones
2. **POST** `/Estudiante/CrearGrupo` - Crear grupo
3. Redirección → `/Estudiante/Dashboard` con confirmación

### Flujo de Solicitud de Ingreso
1. **GET** `/Estudiante/GestionGrupos` - Ver grupos disponibles
2. **POST** `/Estudiante/SolicitarIngreso/{id}` - Enviar solicitud
3. **GET** `/Estudiante/MisSolicitudes` - Verificar estado
4. Líder: **POST** `/Estudiante/ProcesarSolicitud` - Aprobar/Rechazar

## Códigos de Estado HTTP

- **200 OK**: Operación exitosa
- **302 Found**: Redirección (común en aplicaciones MVC)
- **400 Bad Request**: Datos inválidos en formulario
- **401 Unauthorized**: Usuario no autenticado
- **403 Forbidden**: Sin permisos para la acción
- **404 Not Found**: Página o recurso no encontrado
- **500 Internal Server Error**: Error del servidor

## Headers de Respuesta Comunes

```http
Content-Type: text/html; charset=utf-8
Set-Cookie: AspNetCore.Session=...; path=/; httponly
X-Frame-Options: SAMEORIGIN
X-Content-Type-Options: nosniff
```

## Endpoints de Testing/Debug

### GET /Home/SeedData
**Descripción**: Poblar BD con datos de prueba  
**Disponible**: Solo en desarrollo  
**Respuesta**: Datos creados exitosamente

### GET /Home/VerificarUsuarios
**Descripción**: Listar usuarios del sistema  
**Disponible**: Solo administradores  
**Respuesta**: Lista de usuarios con roles

---

## Notas para Desarrolladores

1. **Autenticación**: Todos los endpoints excepto `/Auth/Login` requieren autenticación
2. **Autorización**: Los endpoints verifican roles antes de ejecutar acciones
3. **Validación**: Los formularios incluyen validación del lado del servidor
4. **Seguridad**: Las contraseñas se hashean con SHA256 + salt
5. **Sesiones**: Timeout automático después de 30 minutos de inactividad

**Última actualización**: Agosto 28, 2024  
**Versión de API**: 1.0  
**Contacto**: dev@liceocarrillos.edu.cr
