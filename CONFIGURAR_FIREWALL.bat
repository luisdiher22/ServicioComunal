@echo off
echo ========================================
echo   CONFIGURANDO FIREWALL DE WINDOWS
echo ========================================
echo.
echo Este script abrira el puerto 5000 en el firewall de Windows
echo para permitir el acceso desde la red local.
echo.
echo NOTA: Se requieren permisos de administrador
echo.
pause

echo.
echo Agregando regla de firewall...

netsh advfirewall firewall add rule name="Servicio Comunal - Puerto 5000" dir=in action=allow protocol=TCP localport=5000

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo   FIREWALL CONFIGURADO EXITOSAMENTE
    echo ========================================
    echo.
    echo El puerto 5000 ahora esta abierto para conexiones entrantes
    echo.
) else (
    echo.
    echo ========================================
    echo   ERROR AL CONFIGURAR FIREWALL
    echo ========================================
    echo.
    echo Asegurese de ejecutar este script como Administrador
    echo Click derecho en el archivo ^> "Ejecutar como administrador"
    echo.
)

pause
