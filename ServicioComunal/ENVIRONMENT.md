# Configuración de Entorno - Sistema de Servicio Comunal

## Variables de Entorno

### Desarrollo Local
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:7164;http://localhost:5133
```

### Staging/Pruebas
```bash
ASPNETCORE_ENVIRONMENT=Staging
ASPNETCORE_URLS=https://staging.liceocarrillos.edu.cr
```

### Producción
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://liceocarrillos.edu.cr
```

## Estructura de Archivos de Configuración

```
ServicioComunal/
├── appsettings.json                    # Configuración base
├── appsettings.Development.json        # Configuración desarrollo
├── appsettings.Staging.json           # Configuración staging
├── appsettings.Production.json        # Configuración producción
└── web.config                         # Configuración IIS (generado)
```

## Configuración de Base de Datos por Entorno

### Desarrollo
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ServicioComunalDB_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"
}
```

### Staging
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=staging-sql-server;Database=ServicioComunalDB_Staging;User ID=staging_user;Password=staging_password;TrustServerCertificate=True"
}
```

### Producción
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=prod-sql-server;Database=ServicioComunalDB;User ID=prod_user;Password=prod_password;TrustServerCertificate=False;Encrypt=True"
}
```

## Configuración de Seguridad

### Headers de Seguridad
```json
"SecurityHeaders": {
  "X-Frame-Options": "SAMEORIGIN",
  "X-Content-Type-Options": "nosniff",
  "X-XSS-Protection": "1; mode=block",
  "Strict-Transport-Security": "max-age=31536000; includeSubDomains"
}
```

### Configuración de Sesiones
```json
"Session": {
  "IdleTimeout": "00:30:00",
  "Cookie": {
    "HttpOnly": true,
    "IsEssential": true,
    "SameSite": "Strict"
  }
}
```

## Configuración de Logging

### Desarrollo
```json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "Microsoft.AspNetCore": "Information",
    "Microsoft.EntityFrameworkCore": "Information"
  }
}
```

### Producción
```json
"Logging": {
  "LogLevel": {
    "Default": "Warning",
    "Microsoft.AspNetCore": "Error",
    "Microsoft.EntityFrameworkCore": "Error"
  }
}
```

## Configuración de CORS (si es necesario)

```json
"CORS": {
  "AllowedOrigins": [
    "https://liceocarrillos.edu.cr",
    "https://www.liceocarrillos.edu.cr"
  ],
  "AllowedMethods": ["GET", "POST"],
  "AllowCredentials": true
}
```

## Configuración de Email (futuro)

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "EnableSsl": true,
  "FromEmail": "noreply@liceocarrillos.edu.cr",
  "FromName": "Sistema Servicio Comunal"
}
```

## Configuración de Features Flags

```json
"FeatureFlags": {
  "EnableUserRegistration": false,
  "EnableEmailNotifications": false,
  "EnableAdvancedReports": true,
  "MaintenanceMode": false
}
```

## Configuración de Cache

```json
"Caching": {
  "DefaultExpiration": "00:15:00",
  "SlidingExpiration": true,
  "AbsoluteExpiration": "01:00:00"
}
```

## Configuración de Mobile/Responsive

```json
"UI": {
  "Theme": "default",
  "EnableMobileView": true,
  "DefaultPageSize": 10,
  "MaxPageSize": 50
}
```

## Configuración de Rate Limiting (futuro)

```json
"RateLimiting": {
  "EnableRateLimiting": true,
  "FixedWindow": {
    "Window": "00:01:00",
    "PermitLimit": 100
  }
}
```

## Configuración de File Upload (futuro)

```json
"FileUpload": {
  "MaxFileSize": 5242880,
  "AllowedExtensions": [".pdf", ".doc", ".docx", ".jpg", ".png"],
  "UploadPath": "uploads/"
}
```

## Configuración de Health Checks

```json
"HealthChecks": {
  "Database": {
    "Enabled": true,
    "Timeout": "00:00:30"
  },
  "DiskSpace": {
    "Enabled": true,
    "MinimumFreeSpace": 1073741824
  }
}
```

## Configuración de Performance

```json
"Performance": {
  "EnableCompression": true,
  "EnableCaching": true,
  "StaticFilesCacheDuration": "30.00:00:00"
}
```

## Notas de Configuración

### Para Administradores de Sistema:
1. **Nunca incluir contraseñas en archivos de configuración versionados**
2. **Usar Azure Key Vault, AWS Secrets Manager o variables de entorno para secrets**
3. **Revisar configuración de cada entorno antes del deployment**
4. **Mantener backups de configuraciones de producción**

### Para Desarrolladores:
1. **Usar appsettings.Development.json para configuración local**
2. **No commitear archivos con datos sensibles**
3. **Validar configuración al cambiar de rama/entorno**
4. **Documentar cambios de configuración en PRs**

### Para QA:
1. **Verificar que cada entorno tiene su configuración específica**
2. **Validar que los feature flags están correctamente configurados**
3. **Confirmar que los logs se generan según el entorno**
4. **Verificar configuración de seguridad en staging/producción**

---

**Mantenido por**: Equipo de DevOps  
**Última actualización**: Agosto 28, 2024  
**Versión**: 1.0
