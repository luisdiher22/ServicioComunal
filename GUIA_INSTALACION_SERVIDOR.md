# 📘 GUÍA DE INSTALACIÓN Y DESPLIEGUE
## Sistema de Servicio Comunal - Liceo de Carrillos

---

## 🎯 RESUMEN RÁPIDO

Este sistema permite gestionar el servicio comunal estudiantil del Liceo de Carrillos. Esta guía explica cómo instalar y ejecutar el servidor en la escuela para que los estudiantes puedan acceder solo desde la red WiFi de la institución.

---

## 📋 REQUISITOS PREVIOS

### En la Computadora del Servidor:
- ✅ Windows 10/11 o Windows Server
- ✅ Mínimo 4GB de RAM
- ✅ 2GB de espacio en disco
- ✅ Conexión a Internet (solo para la instalación inicial)
- ✅ Base de datos SQL Server (ya configurada en la nube)

### NO se requiere:
- ❌ Instalar .NET (la aplicación incluye todo lo necesario)
- ❌ Instalar Visual Studio
- ❌ Conocimientos técnicos avanzados

---

## 🚀 PASOS DE INSTALACIÓN

### PASO 1: Preparar la Aplicación

#### Opción A: Si ya tiene la carpeta publicada
Si ya le entregaron la carpeta `ServicioComunal_Publicado`, vaya directamente al **PASO 2**.

#### Opción B: Publicar desde el código fuente
Si tiene el código fuente y necesita generar la aplicación:

1. Asegúrese de tener **.NET SDK 9.0** instalado
   - Descárguelo desde: https://dotnet.microsoft.com/download/dotnet/9.0
   
2. Haga doble clic en el archivo:
   ```
   PUBLICAR_APLICACION.bat
   ```

3. Espere a que termine (puede tardar 3-5 minutos)

4. Al finalizar, encontrará la aplicación publicada en:
   ```
   ServicioComunal\ServicioComunal\bin\Release\net9.0\win-x64\publish
   ```

5. **Copie toda la carpeta `publish`** a una memoria USB o al servidor

---

### PASO 2: Instalar en el Servidor de la Escuela

1. **Copie la carpeta completa** al servidor
   - Ubicación recomendada: `C:\ServicioComunal\`
   - Asegúrese de copiar TODOS los archivos

2. **Verifique que la carpeta contenga:**
   - `ServicioComunal.exe` (el archivo principal)
   - Múltiples archivos DLL
   - Carpetas: `wwwroot`, `Views`, etc.
   - Archivo `appsettings.json`

---

### PASO 3: Configurar el Firewall de Windows

El servidor necesita abrir el puerto 5000 para que otros equipos puedan conectarse.

1. **Copie el archivo** `CONFIGURAR_FIREWALL.bat` a la carpeta del servidor

2. **Haga clic derecho** en `CONFIGURAR_FIREWALL.bat`

3. Seleccione **"Ejecutar como administrador"**

4. Confirme cuando Windows pregunte si desea permitir cambios

5. Presione cualquier tecla cuando vea "Firewall configurado exitosamente"

> ⚠️ **IMPORTANTE**: Este paso requiere permisos de administrador en Windows

---

### PASO 4: Iniciar el Servidor

1. **Copie el archivo** `INICIAR_SERVIDOR.bat` a la carpeta donde está `ServicioComunal.exe`

2. **Haga doble clic** en `INICIAR_SERVIDOR.bat`

3. Verá una ventana mostrando:
   ```
   ╔════════════════════════════════════════════════════════════════╗
   ║        SISTEMA DE SERVICIO COMUNAL - LICEO DE CARRILLOS        ║
   ╚════════════════════════════════════════════════════════════════╝
   
   [INFO] Iniciando servidor...
   [INFO] La aplicación estará disponible en:
   
          • Desde esta computadora: http://localhost:5000
          • Desde la red local:      http://192.168.X.X:5000
   ```

4. **Anote la IP de la red local** que aparece (ejemplo: 192.168.1.100)

5. **¡El servidor está funcionando!** No cierre esta ventana

---

## 🌐 ACCEDER AL SISTEMA

### Desde la Computadora del Servidor:
Abra un navegador y vaya a: `http://localhost:5000`

### Desde Otros Dispositivos en la Escuela:
1. Conéctese al WiFi de la escuela
2. Abra un navegador
3. Vaya a: `http://[IP-DEL-SERVIDOR]:5000`
   - Ejemplo: `http://192.168.1.100:5000`

### Credenciales de Acceso Inicial:
Consulte con el administrador del sistema las credenciales de acceso.

---

## 🔄 USO DIARIO

### Para Iniciar el Servidor:
1. Vaya a la carpeta del servidor
2. Doble clic en `INICIAR_SERVIDOR.bat`
3. Mantenga la ventana abierta

### Para Detener el Servidor:
1. En la ventana del servidor, presione `Ctrl + C`
2. O simplemente cierre la ventana

### Para Reiniciar:
1. Cierre la ventana del servidor
2. Vuelva a ejecutar `INICIAR_SERVIDOR.bat`

---

## ⚙️ CONFIGURACIÓN AVANZADA

### Cambiar el Puerto (Opcional)

Si el puerto 5000 ya está en uso, puede cambiarlo:

1. Edite el archivo `INICIAR_SERVIDOR.bat` con el Bloc de notas
2. Cambie `5000` por otro número (ej: `5001`, `8080`)
3. Guarde el archivo
4. Reconfigure el firewall para el nuevo puerto

### Ejecutar como Servicio de Windows

Para que el servidor inicie automáticamente con Windows:

1. Presione `Win + R`, escriba `shell:startup` y presione Enter
2. Cree un acceso directo a `INICIAR_SERVIDOR.bat` en esa carpeta
3. El servidor ahora iniciará con Windows

**Alternativa profesional** (requiere más conocimientos):
Use `NSSM` (Non-Sucking Service Manager) para crear un servicio de Windows real.

---

## 🔒 SEGURIDAD

### Acceso Limitado a la Red Local
El sistema solo es accesible desde dispositivos conectados al WiFi de la escuela. No se puede acceder desde Internet.

### Base de Datos
La base de datos está alojada en un servidor seguro en la nube (site4now.net) y ya está configurada.

### Recomendaciones:
- ✅ Use contraseñas seguras para todos los usuarios
- ✅ Mantenga Windows actualizado
- ✅ Configure copias de seguridad regulares
- ✅ No comparta las credenciales de administrador

---

## 🛠️ SOLUCIÓN DE PROBLEMAS

### Problema: No puedo acceder desde otros dispositivos

**Solución:**
1. Verifique que el firewall esté configurado (PASO 3)
2. Confirme que el dispositivo esté en la misma red WiFi
3. Pruebe con la IP exacta mostrada al iniciar el servidor
4. Desactive temporalmente el antivirus para probar

### Problema: "El puerto 5000 ya está en uso"

**Solución:**
1. Otro programa está usando el puerto
2. Ejecute este comando en PowerShell (como administrador):
   ```powershell
   Get-Process -Id (Get-NetTCPConnection -LocalPort 5000).OwningProcess
   ```
3. Cierre el programa que usa el puerto o cambie el puerto del servidor

### Problema: Error de conexión a la base de datos

**Solución:**
1. Verifique que el servidor tenga conexión a Internet
2. Contacte al administrador del sistema
3. Revise el archivo `appsettings.json` para confirmar la cadena de conexión

### Problema: La aplicación se cierra inmediatamente

**Solución:**
1. Verifique que copió TODOS los archivos de la carpeta publicada
2. Ejecute `ServicioComunal.exe` directamente desde el Explorador de archivos
3. Revise los mensajes de error que aparezcan

---

## 📞 SOPORTE TÉCNICO

Si encuentra problemas que no puede resolver:

1. **Documente el error:**
   - Tome una captura de pantalla del mensaje de error
   - Anote qué estaba haciendo cuando ocurrió
   - Verifique la ventana del servidor por mensajes

2. **Revise los logs:**
   - Los archivos de log están en la carpeta `logs` (si existen)

3. **Contacte al desarrollador:**
   - Proporcione toda la información recolectada
   - Incluya la versión del sistema operativo

---

## 📂 ESTRUCTURA DE ARCHIVOS

```
ServicioComunal/
│
├── ServicioComunal.exe          ← Archivo principal ejecutable
├── appsettings.json             ← Configuración (conexión a BD)
├── INICIAR_SERVIDOR.bat         ← Script para iniciar el servidor
├── CONFIGURAR_FIREWALL.bat      ← Script para configurar firewall
│
├── wwwroot/                     ← Archivos estáticos (CSS, JS, imágenes)
├── Views/                       ← Vistas HTML de la aplicación
└── [múltiples archivos .dll]   ← Librerías necesarias
```

---

## ✅ CHECKLIST DE INSTALACIÓN

Use esta lista para verificar que todo esté listo:

- [ ] Carpeta completa copiada al servidor
- [ ] Archivo `ServicioComunal.exe` presente
- [ ] Firewall configurado (puerto 5000 abierto)
- [ ] Servidor iniciado correctamente con `INICIAR_SERVIDOR.bat`
- [ ] IP del servidor anotada
- [ ] Acceso probado desde el navegador local (localhost:5000)
- [ ] Acceso probado desde otro dispositivo en la red
- [ ] Credenciales de acceso funcionando
- [ ] Accesos directos creados (opcional)

---

## 📝 NOTAS ADICIONALES

- **Rendimiento**: El servidor puede manejar múltiples usuarios simultáneos sin problemas
- **Espacio**: La aplicación usa aproximadamente 200MB de disco
- **Memoria**: Consume aproximadamente 100-200MB de RAM durante la ejecución
- **Red**: Requiere conexión a Internet solo para acceder a la base de datos

---

## 🔄 ACTUALIZACIONES

Cuando reciba una nueva versión:

1. Detenga el servidor (cierre la ventana)
2. Haga una copia de seguridad de la carpeta actual
3. Reemplace los archivos con la nueva versión
4. Inicie el servidor nuevamente con `INICIAR_SERVIDOR.bat`

---

**Última actualización:** Marzo 2026  
**Versión de la guía:** 1.0  
**Sistema:** Servicio Comunal - Liceo de Carrillos

---

## 🎓 ¡LISTO PARA USAR!

Ahora el sistema está funcionando y los estudiantes pueden acceder desde cualquier dispositivo conectado al WiFi de la escuela. ¡Éxito!
