# 🏫 SISTEMA DE SERVICIO COMUNAL - LICEO DE CARRILLOS
## Guía de Despliegue en Servidor Escolar

---

## 📦 CONTENIDO DEL PAQUETE

Este proyecto incluye todo lo necesario para desplegar el sistema en el servidor de la escuela:

### 🎯 Archivos de Ejecución
- **`PUBLICAR_APLICACION.bat`** - Genera la versión ejecutable del sistema
- **`INICIAR_SERVIDOR.bat`** - Inicia el servidor (copiar a carpeta publicada)
- **`CONFIGURAR_FIREWALL.bat`** - Configura el firewall de Windows

### 📚 Documentación
- **`GUIA_INSTALACION_SERVIDOR.md`** - Guía completa paso a paso
- **`GUIA_RAPIDA.md`** - Referencia rápida para uso diario

---

## 🚀 INICIO RÁPIDO

### Para el Desarrollador (Publicar la Aplicación)

1. **Ejecuta el script de publicación:**
   ```
   Doble click en: PUBLICAR_APLICACION.bat
   ```

2. **Espera a que termine** (3-5 minutos)

3. **Encuentra los archivos publicados en:**
   ```
   ServicioComunal\ServicioComunal\bin\Release\net9.0\win-x64\publish
   ```

4. **Copia estos archivos junto con:**
   - `INICIAR_SERVIDOR.bat`
   - `CONFIGURAR_FIREWALL.bat`
   - `GUIA_INSTALACION_SERVIDOR.md`
   - `GUIA_RAPIDA.md`

5. **Entrega todo al personal de la escuela**

---

### Para el Personal de la Escuela (Instalar en Servidor)

1. **LEE la guía completa:**
   ```
   Abre: GUIA_INSTALACION_SERVIDOR.md
   ```

2. **Sigue los 4 pasos principales:**
   - ✅ Copiar archivos al servidor
   - ✅ Configurar firewall
   - ✅ Iniciar servidor
   - ✅ Verificar acceso

3. **Para uso diario:**
   ```
   Consulta: GUIA_RAPIDA.md
   ```

---

## 🎯 CARACTERÍSTICAS DEL DESPLIEGUE

### ✅ Ventajas de esta Configuración

- **No requiere instalación de .NET** - Todo está incluido
- **Fácil de iniciar** - Un solo click para arrancar
- **Solo red local** - Seguro, solo accesible desde el WiFi de la escuela
- **Auto-contenido** - Todos los archivos necesarios están incluidos
- **Base de datos en la nube** - Ya configurada y lista para usar

### 🔒 Seguridad

- Servidor solo accesible desde la red local de la escuela
- No expuesto a Internet
- Base de datos con autenticación en servidor seguro
- Puerto específico configurable

### 💻 Requisitos del Servidor

- Windows 10/11 o Windows Server
- 4GB RAM mínimo
- 2GB espacio en disco
- Conexión a Internet (para base de datos)

---

## 📂 ESTRUCTURA DEL DEPLOYMENT

```
ServicioComunal_Publicado/
│
├── ServicioComunal.exe              ← Ejecutable principal
├── appsettings.json                 ← Configuración
├── INICIAR_SERVIDOR.bat             ← Script de inicio
├── CONFIGURAR_FIREWALL.bat          ← Configuración firewall
├── GUIA_INSTALACION_SERVIDOR.md     ← Guía completa
├── GUIA_RAPIDA.md                   ← Referencia rápida
│
├── wwwroot/                         ← Recursos web
├── Views/                           ← Vistas de la aplicación
└── [archivos .dll]                  ← Dependencias
```

---

## 🌐 INFORMACIÓN DE ACCESO

### Una vez iniciado el servidor:

**Desde el propio servidor:**
```
http://localhost:5000
```

**Desde otros dispositivos en la red:**
```
http://[IP-DEL-SERVIDOR]:5000
```

La IP se mostrará automáticamente al ejecutar `INICIAR_SERVIDOR.bat`

---

## 🔧 CONFIGURACIÓN DE BASE DE DATOS

La aplicación ya está configurada para usar una base de datos SQL Server en la nube:
- **Servidor:** SQL5106.site4now.net
- **Base de datos:** db_abef5f_liceocarrillos
- **Cadena de conexión:** Configurar `ConnectionStrings__DefaultConnection` en el entorno del hosting

No se requiere instalación de SQL Server local.

---

## 📱 DISPOSITIVOS COMPATIBLES

El sistema es accesible desde:
- 💻 Computadoras Windows/Mac/Linux
- 📱 Smartphones (Android/iOS)
- 📱 Tablets
- 🖥️ Cualquier dispositivo con navegador web

**Único requisito:** Estar conectado al WiFi de la escuela

---

## 🔄 ACTUALIZACIONES

Para actualizar el sistema:

1. Ejecuta nuevamente `PUBLICAR_APLICACION.bat`
2. Copia los nuevos archivos al servidor
3. Reinicia el servidor

---

## 🛠️ SOPORTE TÉCNICO

### Documentación
- **Guía completa:** `GUIA_INSTALACION_SERVIDOR.md`
- **Referencia rápida:** `GUIA_RAPIDA.md`

### Logs y Diagnóstico
Los errores se mostrarán en la ventana del servidor al ejecutar `INICIAR_SERVIDOR.bat`

---

## 📊 TECNOLOGÍAS UTILIZADAS

- **Framework:** ASP.NET Core 9.0
- **Base de datos:** SQL Server
- **ORM:** Entity Framework Core
- **Deployment:** Self-contained executable
- **Plataforma:** Windows x64

---

## 📜 LICENCIA

[Consulta el archivo LICENSE para más información]

---

## ✅ CHECKLIST PARA EL DESARROLLADOR

Antes de entregar al personal de la escuela:

- [ ] Ejecutar `PUBLICAR_APLICACION.bat`
- [ ] Verificar que la publicación fue exitosa
- [ ] Copiar `INICIAR_SERVIDOR.bat` a la carpeta publicada
- [ ] Copiar `CONFIGURAR_FIREWALL.bat` a la carpeta publicada
- [ ] Incluir `GUIA_INSTALACION_SERVIDOR.md`
- [ ] Incluir `GUIA_RAPIDA.md`
- [ ] Verificar que el hosting tenga configurada `ConnectionStrings__DefaultConnection`
- [ ] Probar localmente que todo funciona
- [ ] Comprimir todo en un archivo ZIP o copiar a USB

---

## ✅ CHECKLIST PARA EL PERSONAL DE LA ESCUELA

Al recibir los archivos:

- [ ] Copiar carpeta completa al servidor (ej: C:\ServicioComunal\)
- [ ] Ejecutar `CONFIGURAR_FIREWALL.bat` como administrador
- [ ] Ejecutar `INICIAR_SERVIDOR.bat`
- [ ] Anotar la IP mostrada en pantalla
- [ ] Probar acceso desde navegador local (localhost:5000)
- [ ] Probar acceso desde otro dispositivo en la red
- [ ] Crear acceso directo en escritorio (opcional)
- [ ] Imprimir `GUIA_RAPIDA.md` para referencia

---

## 🎓 RESULTADO FINAL

Una vez completada la configuración:

✅ El servidor estará corriendo en la computadora de la escuela  
✅ Estudiantes y profesores podrán acceder desde cualquier dispositivo  
✅ Solo funciona dentro de la red WiFi de la escuela (seguro)  
✅ La base de datos está en la nube (sin mantenimiento local)  
✅ Fácil de iniciar/detener/reiniciar  

---

**Sistema desarrollado para el Liceo de Carrillos**  
**Versión:** 1.0 | **Fecha:** Marzo 2026  

¡Éxito con el deployment! 🚀
