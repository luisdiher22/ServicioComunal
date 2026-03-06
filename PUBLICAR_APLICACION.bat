@echo off
echo ========================================
echo   PUBLICANDO APLICACION SERVICIO COMUNAL
echo ========================================
echo.

cd "%~dp0ServicioComunal\ServicioComunal"

echo Limpiando publicacion anterior...
if exist "bin\Release\net9.0\publish" (
    rmdir /s /q "bin\Release\net9.0\publish"
)

echo.
echo Publicando aplicacion (esto puede tardar unos minutos)...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:PublishReadyToRun=true

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo   PUBLICACION COMPLETADA EXITOSAMENTE
    echo ========================================
    echo.
    echo Los archivos publicados estan en:
    echo %cd%\bin\Release\net9.0\win-x64\publish
    echo.
    echo Ahora puede copiar esa carpeta al servidor de la escuela
    echo.
) else (
    echo.
    echo ========================================
    echo   ERROR EN LA PUBLICACION
    echo ========================================
    echo.
    echo Asegurese de tener .NET SDK 9.0 instalado
    echo.
)

pause
