# Servicio Comunal - Sistema Completo ✅

## 🚀 Estado Actual: LISTO PARA USAR

### ✅ **PROBLEMA FK RESUELTO:**
- ✅ Error de Foreign Key constraint solucionado
- ✅ Seeder corregido para insertar datos en orden correcto
- ✅ Profesores y estudiantes se crean ANTES que usuarios
- ✅ Compilación exitosa sin errores

### **Orden de Inserción Corregido:**
1. **Profesores** → se insertan primero
2. **Estudiantes** → se insertan segundo  
3. **Grupos** → se insertan tercero
4. **🔄 SaveChanges()** → se guardan para crear las FK
5. **Usuarios** → se crean usando las FK existentes
6. **Resto de datos** → grupos, entregas, notificaciones

---

## ⚠️ **Para Resolver el Error FK que Tenías:**

**El error "FK_USUARIO_PROFESOR_Identificacion" se solucionó reorganizando el seeder.**

## 1️⃣ Ejecutar la Aplicación
```bash
cd ServicioComunal
dotnet run
```

## 2️⃣ Poblar la Base de Datos (CON FK CORREGIDAS)
- Ve a: `https://localhost:xxxx/Home/SeedData`
- Ahora insertará datos en el orden correcto sin errores FK

## 3️⃣ Verificar que los Usuarios se Crearon
- Ve a: `https://localhost:xxxx/Home/VerificarUsuarios`
- Deberías ver 7 usuarios creados

## 4️⃣ Probar el Login
- Ve a: `https://localhost:xxxx/Auth/Login`
- Usuario: `admin`
- Contraseña: `password123`

---

## 📋 URLs del Sistema
- **🔐 Login:** `/Auth/Login`
- **📊 Dashboard:** `/Home/Dashboard`
- **🗄️ Poblar BD:** `/Home/SeedData`
- **👥 Verificar Usuarios:** `/Home/VerificarUsuarios`

## 👤 Datos de Prueba Predeterminados
- **Admin:** usuario=`admin`, contraseña=`password123`
- **Profesor:** usuario=`jperez`, contraseña=`profesor123`
- **Estudiante:** usuario=`estudiant1`, contraseña=`estudiante123`

---

## 📁 Estructura del Proyecto

## Modelos Creados

### Entidades Principales:
- **Profesor**: Representa a los profesores del sistema
- **Estudiante**: Representa a los estudiantes
- **Usuario**: Maneja la autenticación (usuarios y contraseñas)
- **Grupo**: Representa los grupos de trabajo
- **GrupoEstudiante**: Tabla de relación muchos a muchos entre Grupo y Estudiante
- **Entrega**: Representa las entregas/tareas asignadas
- **Formulario**: Representa los formularios del sistema
- **Notificacion**: Representa las notificaciones del sistema

## Configuración de Base de Datos

El proyecto está configurado para usar SQL Server con Entity Framework Core. La cadena de conexión está configurada en `appsettings.json`.

### Cadena de Conexión por Defecto:
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ServicioComunalDB;Trusted_Connection=true;MultipleActiveResultSets=true"
```

## Comandos para Migraciones

### 1. Crear la primera migración:
```bash
dotnet ef migrations add InitialCreate
```

### 2. Aplicar la migración a la base de datos:
```bash
dotnet ef database update
```

### 3. Para crear migraciones adicionales después de cambios en los modelos:
```bash
dotnet ef migrations add [NombreDeLaMigracion]
dotnet ef database update
```

### 4. Para eliminar la última migración (si no ha sido aplicada):
```bash
dotnet ef migrations remove
```

### 5. Para ver el estado de las migraciones:
```bash
dotnet ef migrations list
```

## Estructura de las Relaciones

### Relaciones Configuradas:
1. **Usuario** → **Estudiante** (1:1, opcional)
2. **Usuario** → **Profesor** (1:1, opcional)
3. **Profesor** → **Entrega** (1:N)
4. **Profesor** → **Formulario** (1:N)
5. **Profesor** → **GrupoEstudiante** (1:N)
6. **Profesor** → **Notificacion** (1:N)
7. **Grupo** → **Entrega** (1:N)
8. **Grupo** → **GrupoEstudiante** (1:N)
9. **Grupo** → **Notificacion** (1:N)
10. **Estudiante** → **GrupoEstudiante** (1:N)

### Claves Compuestas:
- **GrupoEstudiante**: Clave compuesta formada por `EstudianteIdentificacion` y `GrupoNumero`

### Índices Únicos:
- **Usuario.NombreUsuario**: Índice único para evitar usuarios duplicados

## Características Implementadas

- ✅ Mapeo completo de todas las tablas del modelo relacional
- ✅ **Sistema de autenticación con tabla Usuario**
- ✅ **Hash seguro de contraseñas con salt**
- ✅ **Generación de contraseñas temporales**
- ✅ Configuración de relaciones con Foreign Keys
- ✅ Configuración de Delete Behavior como Restrict para evitar eliminaciones en cascada
- ✅ Anotaciones de Data Annotations para validaciones
- ✅ Configuración del DbContext con Fluent API
- ✅ Propiedades de navegación virtuales para Lazy Loading
- ✅ **Índices únicos para optimización**

## Próximos Pasos

1. Ejecutar `dotnet ef migrations add InitialCreate` para crear la primera migración
2. Ejecutar `dotnet ef database update` para aplicar la migración
3. La base de datos estará lista para usar

## Sistema de Autenticación

### Tabla Usuario
La tabla `USUARIO` maneja la autenticación del sistema con los siguientes campos:
- **Identificacion**: Cédula de identidad (PK, relaciona con Estudiante/Profesor)
- **Usuario**: Nombre de usuario único
- **Contraseña**: Hash seguro de la contraseña (SHA256 + salt)
- **Rol**: 'Estudiante', 'Profesor', 'Administrador'
- **FechaCreacion**: Fecha de creación del usuario
- **UltimoAcceso**: Última vez que hizo login
- **Activo**: Estado del usuario

### Seguridad de Contraseñas
- ✅ **Hash SHA256 con salt aleatorio**
- ✅ **Nunca se almacenan contraseñas en texto plano**
- ✅ **Generación automática de contraseñas temporales**
- ✅ **Verificación segura de contraseñas**

### Proceso de Importación (Futuro)
El sistema está preparado para:
1. Importar estudiantes desde archivos Excel/CSV
2. Crear automáticamente usuarios con contraseñas temporales
3. Enviar credenciales por correo o entregar en físico
4. Forzar cambio de contraseña en primer login

## Notas Importantes

- Todas las relaciones están configuradas con `DeleteBehavior.Restrict` para evitar eliminaciones accidentales en cascada
- Las propiedades de navegación están marcadas como `virtual` para habilitar Lazy Loading
- Se usaron Data Annotations para mapear correctamente los nombres de tablas y columnas del modelo original
- **CORREGIDO**: El campo `FechaLimite` en la tabla `ENTREGA` ahora es `datetime` (era `int` en el modelo original)
- **AGREGADO**: Sistema completo de autenticación con tabla `USUARIO`

## Archivos SQL Disponibles

- `SQL/ModeloCompleto_Corregido.sql` - Script completo con todas las tablas (esquema corregido)
- `SQL/AgregarTablaUsuario.sql` - Script para agregar solo la tabla USUARIO a BD existente
- `SQL/DatosPrueba.sql` - Datos de prueba para testing

### Usuarios de Prueba Creados
- **admin** / password123 (Administrador)
- **carlos.jimenez** / password123 (Profesor)
- **ana.mora** / password123 (Profesor)  
- **luis.perez** / password123 (Estudiante)
- **sofia.ramirez** / password123 (Estudiante)
- **diego.hernandez** / password123 (Estudiante)
- **camila.vargas** / password123 (Estudiante)
