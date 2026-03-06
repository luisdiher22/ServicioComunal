@echo off
chcp 65001 >nul
color 0B
title Preparar Paquete para Entrega

echo ╔════════════════════════════════════════════════════════════════╗
echo ║     PREPARAR PAQUETE PARA INSTALACIÓN EN LA ESCUELA           ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.

echo [PASO 1] Verificando que la aplicación esté publicada...
echo.

set "PUBLISH_PATH=%~dp0ServicioComunal\ServicioComunal\bin\Release\net9.0\win-x64\publish"

if not exist "%PUBLISH_PATH%" (
    echo [ERROR] La aplicación no ha sido publicada.
    echo.
    echo Por favor, ejecute primero: PUBLICAR_APLICACION.bat
    echo.
    pause
    exit /b 1
)

echo [OK] Aplicación publicada encontrada
echo.

echo [PASO 2] Creando carpeta para el paquete...
echo.

set "PACKAGE_PATH=%~dp0ServicioComunal_Paquete_Instalacion"

if exist "%PACKAGE_PATH%" (
    echo [INFO] Eliminando paquete anterior...
    rmdir /s /q "%PACKAGE_PATH%"
)

mkdir "%PACKAGE_PATH%"
echo [OK] Carpeta creada
echo.

echo [PASO 3] Copiando archivos de la aplicación...
echo.

xcopy "%PUBLISH_PATH%\*" "%PACKAGE_PATH%\" /E /I /H /Y >nul

if %ERRORLEVEL% EQU 0 (
    echo [OK] Archivos de aplicación copiados
) else (
    echo [ERROR] No se pudieron copiar los archivos
    pause
    exit /b 1
)
echo.

echo [PASO 4] Copiando scripts de instalación...
echo.

copy "%~dp0INICIAR_SERVIDOR.bat" "%PACKAGE_PATH%\" >nul
copy "%~dp0CONFIGURAR_FIREWALL.bat" "%PACKAGE_PATH%\" >nul

echo [OK] Scripts copiados
echo.

echo [PASO 5] Copiando documentación...
echo.

copy "%~dp0GUIA_INSTALACION_SERVIDOR.md" "%PACKAGE_PATH%\" >nul
copy "%~dp0GUIA_RAPIDA.md" "%PACKAGE_PATH%\" >nul
copy "%~dp0INSTRUCCIONES_INSTALACION.txt" "%PACKAGE_PATH%\" >nul
copy "%~dp0CHECKLIST_INSTALACION.txt" "%PACKAGE_PATH%\" >nul
copy "%~dp0DEPLOYMENT_README.md" "%PACKAGE_PATH%\" >nul

echo [OK] Documentación copiada
echo.

echo [PASO 6] Creando archivo LEEME.txt en el paquete...
echo.

(
echo ════════════════════════════════════════════════════════════════
echo    SISTEMA DE SERVICIO COMUNAL - PAQUETE DE INSTALACION
echo    Liceo de Carrillos
echo ════════════════════════════════════════════════════════════════
echo.
echo CONTENIDO DE ESTE PAQUETE:
echo.
echo ✓ ServicioComunal.exe - Aplicacion principal
echo ✓ Archivos DLL y dependencias necesarias
echo ✓ Carpetas wwwroot, Views, etc.
echo.
echo SCRIPTS DE INSTALACION:
echo.
echo ✓ INICIAR_SERVIDOR.bat - Para iniciar el servidor
echo ✓ CONFIGURAR_FIREWALL.bat - Para configurar Windows Firewall
echo.
echo DOCUMENTACION:
echo.
echo ✓ INSTRUCCIONES_INSTALACION.txt - LEER PRIMERO
echo ✓ GUIA_INSTALACION_SERVIDOR.md - Guia completa con capturas
echo ✓ GUIA_RAPIDA.md - Referencia rapida para uso diario
echo ✓ CHECKLIST_INSTALACION.txt - Para imprimir y usar durante instalacion
echo ✓ DEPLOYMENT_README.md - Informacion tecnica del deployment
echo.
echo ════════════════════════════════════════════════════════════════
echo PASOS RAPIDOS:
echo ════════════════════════════════════════════════════════════════
echo.
echo 1. Copie toda esta carpeta al servidor de la escuela
echo    Ubicacion recomendada: C:\ServicioComunal\
echo.
echo 2. Lea INSTRUCCIONES_INSTALACION.txt
echo.
echo 3. Ejecute CONFIGURAR_FIREWALL.bat como Administrador
echo.
echo 4. Ejecute INICIAR_SERVIDOR.bat
echo.
echo 5. Anote la IP que aparece en pantalla
echo.
echo 6. Acceda desde cualquier navegador:
echo    http://[IP-ANOTADA]:5000
echo.
echo ════════════════════════════════════════════════════════════════
echo SOPORTE:
echo ════════════════════════════════════════════════════════════════
echo.
echo Para mas informacion, consulte la documentacion incluida.
echo Todos los archivos estan preparados para una instalacion facil.
echo.
echo Sistema preparado: %date% %time%
echo.
echo ════════════════════════════════════════════════════════════════
) > "%PACKAGE_PATH%\LEEME.txt"

echo [OK] Archivo LEEME.txt creado
echo.

echo ╔════════════════════════════════════════════════════════════════╗
echo ║                  PAQUETE CREADO EXITOSAMENTE                   ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.
echo El paquete completo está en:
echo %PACKAGE_PATH%
echo.
echo Ahora puede:
echo   1. Copiar esta carpeta a una memoria USB
echo   2. Comprimirla en un archivo ZIP
echo   3. Transferirla por red al servidor de la escuela
echo.
echo El paquete contiene TODO lo necesario para la instalación.
echo.
echo ════════════════════════════════════════════════════════════════
explorer "%PACKAGE_PATH%"
echo.
pause
