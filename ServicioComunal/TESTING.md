# Guía de Testing - Sistema de Servicio Comunal

## Plan de Pruebas para QA

### Objetivo
Esta guía proporciona casos de prueba sistemáticos para validar todas las funcionalidades del sistema de gestión de servicio comunal.

## Configuración de Ambiente de Testing

### Prerrequisitos
1. **Base de datos de prueba configurada**
2. **Datos semilla cargados** (ejecutar seeder)
3. **Aplicación ejecutándose** en `https://localhost:7164`

### Usuarios de Prueba Disponibles
```
Administrador:
- Usuario: admin
- Contraseña: password123

Profesores:
- Usuario: carlos.jimenez / Contraseña: password123
- Usuario: ana.mora / Contraseña: password123

Estudiantes:
- Usuario: luis.perez / Contraseña: password123
- Usuario: sofia.ramirez / Contraseña: password123
- Usuario: diego.hernandez / Contraseña: password123
- Usuario: camila.vargas / Contraseña: password123
```

## Casos de Prueba por Módulo

### 1. Módulo de Autenticación

#### TC-001: Login Exitoso
**Objetivo**: Verificar que el login funciona correctamente
**Pasos**:
1. Navegar a `/Auth/Login`
2. Ingresar usuario: `admin` 
3. Ingresar contraseña: `password123`
4. Hacer clic en "Iniciar Sesión"

**Resultado Esperado**: 
- Redirección al dashboard correspondiente
- Sesión activa por 30 minutos

#### TC-002: Login con Credenciales Incorrectas
**Objetivo**: Verificar manejo de credenciales inválidas
**Pasos**:
1. Navegar a `/Auth/Login`
2. Ingresar usuario: `usuario_falso`
3. Ingresar contraseña: `contraseña_falsa`
4. Hacer clic en "Iniciar Sesión"

**Resultado Esperado**: 
- Mensaje de error: "Usuario o contraseña incorrectos"
- Permanecer en página de login

#### TC-003: Logout
**Objetivo**: Verificar que el logout funciona correctamente
**Pasos**:
1. Hacer login exitoso
2. Hacer clic en "Cerrar Sesión"

**Resultado Esperado**:
- Redirección a página de login
- Sesión terminada

#### TC-004: Acceso sin Autenticación
**Objetivo**: Verificar que páginas protegidas requieren login
**Pasos**:
1. Sin hacer login, navegar a `/Estudiante/Dashboard`

**Resultado Esperado**:
- Redirección automática a `/Auth/Login`

### 2. Módulo de Estudiantes

#### TC-005: Dashboard de Estudiante
**Objetivo**: Verificar que el dashboard muestra información correcta
**Pasos**:
1. Login como estudiante (`luis.perez`)
2. Verificar información mostrada en dashboard

**Resultado Esperado**:
- Información personal del estudiante
- Estado del grupo actual
- Opciones de gestión de grupos

#### TC-006: Crear Grupo
**Objetivo**: Verificar creación de nuevo grupo
**Pasos**:
1. Login como estudiante sin grupo
2. Ir a "Gestión de Grupos"
3. Hacer clic en "Crear Nuevo Grupo"
4. Completar formulario de grupo

**Resultado Esperado**:
- Grupo creado exitosamente
- Estudiante asignado como líder
- Redirección a dashboard con información del nuevo grupo

#### TC-007: Solicitar Ingreso a Grupo
**Objetivo**: Verificar solicitud de ingreso a grupo existente
**Pasos**:
1. Login como estudiante sin grupo
2. Ir a "Gestión de Grupos"
3. Seleccionar grupo disponible
4. Hacer clic en "Solicitar Ingreso"

**Resultado Esperado**:
- Solicitud enviada al líder del grupo
- Mensaje de confirmación
- Solicitud visible en "Mis Solicitudes"

#### TC-008: Aprobar/Rechazar Solicitudes (Como Líder)
**Objetivo**: Verificar gestión de solicitudes por líder
**Pasos**:
1. Login como líder de grupo
2. Ir a "Solicitudes Recibidas"
3. Aprobar o rechazar solicitud pendiente

**Resultado Esperado**:
- Solicitud procesada correctamente
- Estudiante agregado al grupo (si se aprueba)
- Notificación al solicitante

### 3. Módulo de Tutores

#### TC-009: Dashboard de Tutor
**Objetivo**: Verificar que tutores ven información de supervisión
**Pasos**:
1. Login como tutor (`carlos.jimenez`)
2. Revisar información en dashboard

**Resultado Esperado**:
- Lista de grupos asignados
- Información de estudiantes supervisados
- Opciones de gestión de entregas

#### TC-010: Gestión de Entregas
**Objetivo**: Verificar que tutores pueden gestionar entregas
**Pasos**:
1. Login como tutor
2. Navegar a gestión de entregas
3. Crear nueva entrega para un grupo

**Resultado Esperado**:
- Entrega creada exitosamente
- Visible para estudiantes del grupo
- Fecha límite configurada correctamente

### 4. Módulo de Administración

#### TC-011: Dashboard de Administrador
**Objetivo**: Verificar funcionalidades administrativas
**Pasos**:
1. Login como administrador (`admin`)
2. Revisar todas las opciones disponibles

**Resultado Esperado**:
- Acceso a gestión de usuarios
- Opciones de configuración del sistema
- Estadísticas generales

#### TC-012: Gestión de Usuarios
**Objetivo**: Verificar CRUD de usuarios
**Pasos**:
1. Login como administrador
2. Ir a "Gestión de Usuarios"
3. Crear, editar y eliminar usuario de prueba

**Resultado Esperado**:
- Operaciones CRUD funcionan correctamente
- Validaciones de formulario activas
- Mensajes de confirmación apropiados

#### TC-013: Reinicio del Sistema
**Objetivo**: Verificar funcionalidad de reinicio
**Pasos**:
1. Login como administrador
2. Ir a "Configuración del Sistema"
3. Ejecutar reinicio de grupos

**Resultado Esperado**:
- Sistema reiniciado correctamente
- Grupos eliminados
- Usuarios conservados

## Pruebas de Seguridad

### TC-014: Validación de Roles
**Objetivo**: Verificar que los roles se respetan
**Pasos**:
1. Login como estudiante
2. Intentar acceder a `/Admin/Dashboard` directamente

**Resultado Esperado**:
- Acceso denegado
- Redirección o mensaje de error

### TC-015: Seguridad de Sesiones
**Objetivo**: Verificar timeout de sesión
**Pasos**:
1. Login exitoso
2. Esperar 31 minutos sin actividad
3. Intentar realizar acción

**Resultado Esperado**:
- Sesión expirada
- Redirección a login

### TC-016: Validación de Hash de Contraseñas
**Objetivo**: Verificar que las contraseñas no se almacenan en texto plano
**Pasos**:
1. Revisar base de datos directamente
2. Verificar tabla USUARIO, campo Contraseña

**Resultado Esperado**:
- Contraseñas hasheadas con SHA256
- Salt incluido en el hash

## Pruebas de Interfaz de Usuario

### TC-017: Responsividad
**Objetivo**: Verificar que la interfaz es responsiva
**Pasos**:
1. Abrir aplicación en navegador
2. Cambiar tamaño de ventana (mobile, tablet, desktop)
3. Verificar que todos los elementos se adaptan

**Resultado Esperado**:
- Interfaz se adapta a diferentes tamaños
- Elementos accesibles en todos los dispositivos

### TC-018: Navegación
**Objetivo**: Verificar que la navegación es intuitiva
**Pasos**:
1. Navegar por todas las secciones
2. Verificar breadcrumbs y menús
3. Probar botones de retroceso

**Resultado Esperado**:
- Navegación clara y consistente
- Breadcrumbs funcionales
- Enlaces funcionan correctamente

## Pruebas de Base de Datos

### TC-019: Integridad de Datos
**Objetivo**: Verificar que las relaciones FK funcionan
**Pasos**:
1. Intentar eliminar usuario con grupos asociados
2. Verificar comportamiento de cascade/restrict

**Resultado Esperado**:
- Restricciones FK se respetan
- Mensajes de error apropiados

### TC-020: Migraciones
**Objetivo**: Verificar que las migraciones funcionan
**Pasos**:
1. Ejecutar `dotnet ef database update`
2. Verificar que la BD se crea correctamente

**Resultado Esperado**:
- Migraciones se aplican sin errores
- Todas las tablas creadas correctamente

## Casos de Prueba de Carga

### TC-021: Múltiples Usuarios Concurrentes
**Objetivo**: Verificar comportamiento con múltiples usuarios
**Pasos**:
1. Simular 10 usuarios logueados simultáneamente
2. Realizar operaciones concurrentes

**Resultado Esperado**:
- Sistema maneja múltiples sesiones
- No hay conflictos de datos

## Reporte de Bugs

### Formato de Reporte
```
ID: BUG-XXX
Título: [Descripción corta del bug]
Severidad: [Crítica/Alta/Media/Baja]
Prioridad: [Alta/Media/Baja]
Módulo: [Autenticación/Estudiantes/Tutores/Admin]
Pasos para Reproducir:
1. ...
2. ...
3. ...
Resultado Actual:
Resultado Esperado:
Navegador/OS:
Screenshots: [si aplica]
```





---

**Última actualización**: Agosto 28, 2024  
**Versión del documento**: 1.0  

