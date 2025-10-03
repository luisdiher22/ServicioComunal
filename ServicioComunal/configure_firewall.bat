@echo off
echo Configurando el firewall para permitir conexiones a la aplicacion ASP.NET Core...
netsh advfirewall firewall add rule name="AspNet Core Dev Server" dir=in action=allow protocol=TCP localport=5133
echo.
echo Configuracion completada. Presiona cualquier tecla para continuar.
pause