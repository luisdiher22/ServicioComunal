@echo off
chcp 65001 >nul
color 0A
title Servidor Servicio Comunal - Liceo de Carrillos

echo ╔════════════════════════════════════════════════════════════════╗
echo ║        SISTEMA DE SERVICIO COMUNAL - LICEO DE CARRILLOS        ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.

REM Obtener la IP local de la máquina
for /f "tokens=2 delims=:" %%a in ('ipconfig ^| findstr /c:"IPv4"') do (
    set IP=%%a
    goto :found
)
:found
set IP=%IP:~1%

echo [INFO] Iniciando servidor...
echo [INFO] La aplicación estará disponible en:
echo.
echo        • Desde esta computadora: http://localhost:5000
echo        • Desde la red local:      http://%IP%:5000
echo.
echo [INFO] Asegúrese de que el puerto 5000 esté abierto en el firewall
echo [INFO] Solo será accesible desde la red WiFi de la escuela
echo.
echo ════════════════════════════════════════════════════════════════
echo.
echo [ESTADO] Presione Ctrl+C para detener el servidor
echo.
echo ════════════════════════════════════════════════════════════════
echo.

REM Ejecutar la aplicación
ServicioComunal.exe --urls "http://0.0.0.0:5000"

pause
