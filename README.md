# Sistema de Servicio Comunal

Aplicación web para coordinar el servicio comunal estudiantil. Reúne en un solo lugar la gestión de grupos, las entregas de estudiantes y el seguimiento de tutores y administradores.

## Funciones principales

- Acceso con áreas para estudiantes, tutores y administradores.
- Creación y organización de grupos, con solicitudes para integrarse.
- Asignación y entrega de actividades con archivos adjuntos.
- Formularios descargables y administración de documentos.
- Seguimiento de entregas, revisiones y notificaciones.

## Tecnologías

- ASP.NET Core MVC con .NET 9.
- Entity Framework Core 9 y SQL Server.
- Vistas Razor, JavaScript y CSS.

## Ejecutar localmente

Necesitas el SDK de .NET 9 y una instancia de SQL Server accesible desde tu equipo.

1. Abre una terminal en la carpeta del proyecto:

   ```powershell
   cd ServicioComunal/ServicioComunal
   ```

2. Crea tu archivo local de configuración y ajusta la cadena de conexión:

   ```powershell
   Copy-Item .env.example .env
   ```

   Edita `.env` y configura `ConnectionStrings__DefaultConnection`. La aplicación carga esa variable al iniciar. `.env` está excluido de Git; no agregues credenciales al repositorio.

3. Si aún no tienes instalada la herramienta de Entity Framework, instálala y aplica las migraciones:

   ```powershell
   dotnet tool install --global dotnet-ef --version 9.0.0
   dotnet ef database update
   ```

4. Inicia la aplicación:

   ```powershell
   dotnet run
   ```

   Abre la dirección local que indique la terminal.

## Estructura

```text
ServicioComunal/ServicioComunal/
├── Controllers/   Acciones y rutas MVC
├── Data/          Contexto de Entity Framework
├── Migrations/    Cambios del esquema de base de datos
├── Models/        Entidades y modelos de datos
├── Services/      Inicialización y lógica de servicios
├── Views/         Vistas Razor
└── wwwroot/       CSS, JavaScript, imágenes y recursos web
```

## Documentación

- [API](ServicioComunal/API.md)
- [Pruebas y revisión manual](ServicioComunal/TESTING.md)
- [Configuración de entorno](ServicioComunal/ENVIRONMENT.md)
- [Despliegue](ServicioComunal/DEPLOYMENT.md)

Consulta [LICENSE](ServicioComunal/LICENSE) para los términos de uso del proyecto.
