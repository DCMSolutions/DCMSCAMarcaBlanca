Set objShell = CreateObject("Shell.Application")

' Ruta del archivo .exe
exePath = "C:\Program Files (x86)\DCM\DCMSIM\BlazorApp1.Server.exe"

' Crear objeto para la aplicación ShellExecute
Set objApp = CreateObject("Shell.Application")

' Ejecutar el archivo .exe como administrador sin mostrar ventana de cmd
objApp.ShellExecute exePath, "", "", "runas", 0

Set objShell = Nothing
Set objApp = Nothing
