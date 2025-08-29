# Guía de Deployment - Sistema de Servicio Comunal

## Guía Completa de Despliegue

### Requisitos del Servidor

#### Mínimos
- **CPU**: 2 núcleos
- **RAM**: 4 GB
- **Almacenamiento**: 50 GB SSD
- **OS**: Windows Server 2019+ o Linux (Ubuntu 20.04+)
- **Red**: Puerto 80 (HTTP) y 443 (HTTPS)

#### Recomendados para Producción
- **CPU**: 4 núcleos
- **RAM**: 8 GB
- **Almacenamiento**: 100- Logs sin errores críticos

## Troubleshooting SSD
- **OS**: Windows Server 2022 o Ubuntu 22.04 LTS
- **Red**: Load balancer con SSL termination

### Software Requerido

#### En el Servidor
```
- .NET 8.0 Runtime (ASP.NET Core)
- SQL Server 2019+ (Express/Standard/Enterprise)
- IIS 10+ (Windows) o Nginx/Apache (Linux)
- SSL Certificate (producción)
```

#### Para Desarrollo/Staging
```
- .NET 8.0 SDK
- SQL Server LocalDB o SQL Server Express
- Visual Studio 2022 o VS Code
- Git
```

## Configuración de Entornos

### 1. Desarrollo (Local)
```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ServicioComunalDB_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

### 2. Staging (Pruebas)
```json
// appsettings.Staging.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=staging-server;Database=ServicioComunalDB_Staging;User ID=staging_user;Password=staging_password;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 3. Producción
```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=ServicioComunalDB;User ID=prod_user;Password=prod_password;TrustServerCertificate=False;Encrypt=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    }
  },
  "AllowedHosts": "liceocarrillos.edu.cr,www.liceocarrillos.edu.cr"
}
```

## Proceso de Build y Deployment

### Opción A: Deployment Manual

#### 1. Preparar el Build
```powershell
# Limpiar y construir
dotnet clean
dotnet restore
dotnet build --configuration Release

# Publicar aplicación
dotnet publish --configuration Release --output ./publish --self-contained false
```

#### 2. Configurar Base de Datos
```powershell
# Aplicar migraciones en producción
dotnet ef database update --connection "Server=prod-server;Database=ServicioComunalDB;..."

# O usar script SQL
sqlcmd -S prod-server -d ServicioComunalDB -i "SQL/ModeloCompleto_Corregido.sql"
```

#### 3. Desplegar Archivos
```powershell
# Copiar archivos al servidor
Copy-Item -Path "./publish/*" -Destination "C:\inetpub\wwwroot\ServicioComunal" -Recurse -Force

# Configurar IIS
# - Crear Application Pool
# - Configurar sitio web
# - Asignar permisos
```

### Opción B: Deployment Automatizado (CI/CD)

#### GitHub Actions Workflow
```yaml
# .github/workflows/deploy.yml
name: Deploy to Production

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore --configuration Release
      
    - name: Test
      run: dotnet test --no-build --verbosity normal
      
    - name: Publish
      run: dotnet publish -c Release -o publish
      
    - name: Deploy to Server
      uses: appleboy/scp-action@v0.1.4
      with:
        host: ${{ secrets.HOST }}
        username: ${{ secrets.USERNAME }}
        key: ${{ secrets.KEY }}
        source: "publish/*"
        target: "/var/www/serviciocomunal"
```

## Configuración de Base de Datos

### Crear Base de Datos en SQL Server
```sql
-- 1. Crear base de datos
CREATE DATABASE ServicioComunalDB
GO

-- 2. Crear usuario para la aplicación
USE [master]
GO
CREATE LOGIN [servicio_user] WITH PASSWORD=N'SecurePassword123!'
GO

USE [ServicioComunalDB]
GO
CREATE USER [servicio_user] FOR LOGIN [servicio_user]
GO
ALTER ROLE [db_datareader] ADD MEMBER [servicio_user]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [servicio_user]
GO
ALTER ROLE [db_ddladmin] ADD MEMBER [servicio_user]
GO
```

### Aplicar Schema y Datos
```powershell
# Opción 1: EF Core Migrations
dotnet ef database update --connection "conexion_string_aqui"

# Opción 2: Scripts SQL
sqlcmd -S servidor -d ServicioComunalDB -U servicio_user -P password -i "SQL/ModeloCompleto_Corregido.sql"
sqlcmd -S servidor -d ServicioComunalDB -U servicio_user -P password -i "SQL/DatosPrueba.sql"
```

## Configuración de Servidor Web

### IIS (Windows)

#### 1. Instalar IIS y ASP.NET Core Hosting Bundle
```powershell
# Habilitar IIS
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole, IIS-WebServer, IIS-CommonHttpFeatures, IIS-HttpErrors, IIS-HttpLogging, IIS-HttpRedirect, IIS-ApplicationDevelopment, IIS-NetFxExtensibility45, IIS-HealthAndDiagnostics, IIS-HttpLogging, IIS-Security, IIS-RequestFiltering, IIS-Performance, IIS-WebServerManagementTools, IIS-ManagementConsole, IIS-IIS6ManagementCompatibility, IIS-Metabase

# Descargar e instalar ASP.NET Core Hosting Bundle
# https://dotnet.microsoft.com/download/dotnet/8.0
```

#### 2. Configurar Application Pool
```
Nombre: ServicioComunalAppPool
.NET CLR Version: No Managed Code
Managed Pipeline Mode: Integrated
Process Model > Identity: ApplicationPoolIdentity
```

#### 3. Configurar Sitio Web
```
Site Name: ServicioComunal
Physical Path: C:\inetpub\wwwroot\ServicioComunal
Port: 80 (HTTP), 443 (HTTPS)
Application Pool: ServicioComunalAppPool
```

#### 4. web.config
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\ServicioComunal.dll" 
                  stdoutLogEnabled="false" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

### Nginx (Linux)

#### 1. Instalar .NET 8.0 Runtime
```bash
# Ubuntu 22.04
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-8.0
```

#### 2. Configurar Nginx
```nginx
# /etc/nginx/sites-available/serviciocomunal
server {
    listen 80;
    server_name liceocarrillos.edu.cr www.liceocarrillos.edu.cr;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

#### 3. Servicio Systemd
```ini
# /etc/systemd/system/serviciocomunal.service
[Unit]
Description=Sistema de Servicio Comunal
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /var/www/serviciocomunal/ServicioComunal.dll
Restart=always
RestartSec=5
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
WorkingDirectory=/var/www/serviciocomunal

[Install]
WantedBy=multi-user.target
```

## Configuración de Seguridad

### 1. SSL/TLS Certificate
```bash
# Let's Encrypt (Linux)
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d liceocarrillos.edu.cr -d www.liceocarrillos.edu.cr

# Windows - Usar certificado de CA comercial o IIS
```

### 2. Seguridad de Base de Datos
```sql
-- Cambiar contraseñas por defecto
UPDATE USUARIO SET Contraseña = 'nuevo_hash_seguro' WHERE Usuario = 'admin';

-- Configurar backup automático
EXEC sp_addumpdevice 'disk', 'ServicioComunalDB_Backup',
'C:\Backups\ServicioComunalDB.bak';
```

### 3. Configuración de Firewall
```powershell
# Windows
New-NetFirewallRule -DisplayName "HTTP-In" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow
New-NetFirewallRule -DisplayName "HTTPS-In" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow

# Linux
sudo ufw allow 'Nginx Full'
sudo ufw allow ssh
sudo ufw enable
```

## Monitoreo y Logs

### 1. Configurar Logging
```json
// appsettings.Production.json
{
  "Serilog": {
    "MinimumLevel": "Warning",
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.txt",
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

### 2. Health Checks
```csharp
// En Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ServicioComunalDbContext>();

app.MapHealthChecks("/health");
```

### 3. Monitoreo de Performance
- **Windows**: Performance Monitor, Event Viewer
- **Linux**: htop, journalctl, Prometheus + Grafana

## Proceso de Actualización

### 1. Backup
```sql
-- Backup de base de datos
BACKUP DATABASE ServicioComunalDB 
TO DISK = 'C:\Backups\ServicioComunalDB_PreUpdate_YYYYMMDD.bak'
```

### 2. Deploy Nueva Versión
```powershell
# Detener aplicación
Stop-Website "ServicioComunal"

# Backup archivos actuales
Copy-Item "C:\inetpub\wwwroot\ServicioComunal" "C:\Backups\ServicioComunal_PreUpdate_YYYYMMDD" -Recurse

# Aplicar nueva versión
Copy-Item "./publish/*" "C:\inetpub\wwwroot\ServicioComunal" -Recurse -Force

# Aplicar migraciones
dotnet ef database update

# Iniciar aplicación
Start-Website "ServicioComunal"
```

### 3. Verificación Post-Deploy
- [ ] Aplicación inicia correctamente
- [ ] Base de datos accesible
- [ ] Login funciona
- [ ] Funcionalidades principales operativas
- [ ] Logs sin errores críticos

## 🆘 Troubleshooting

### Problemas Comunes

#### Error: "Unable to connect to database"
```
1. Verificar cadena de conexión
2. Confirmar que SQL Server está ejecutándose
3. Validar credenciales de usuario
4. Verificar firewall/networking
```

#### Error: "Application failed to start"
```
1. Verificar logs de aplicación
2. Confirmar .NET 8.0 Runtime instalado
3. Verificar permisos de archivos
4. Revisar web.config/configuración
```

#### Error: "502 Bad Gateway" (Nginx)
```
1. Verificar que la aplicación está ejecutándose
2. Confirmar puerto de proxy correcto
3. Revisar logs de Nginx y aplicación
4. Verificar configuración de proxy
```

## Checklist de Deployment

### Pre-Deploy
- [ ] Tests unitarios pasando
- [ ] Base de datos respaldada
- [ ] Configuración de producción validada
- [ ] SSL certificate configurado
- [ ] Monitoring configurado

### Deploy
- [ ] Aplicación compilada sin errores
- [ ] Archivos copiados al servidor
- [ ] Migraciones aplicadas
- [ ] Servicios reiniciados
- [ ] DNS apuntando al servidor

### Post-Deploy
- [ ] Aplicación accesible vía web
- [ ] Login funcional
- [ ] Base de datos operativa
- [ ] Logs sin errores críticos
- [ ] Performance aceptable
- [ ] Backup post-deploy creado

---

