@ECHO OFF
SETLOCAL
PUSHD "%~dp0"

SET MigName=
SET /P MigName=Migration name: 
IF "%MigName%"=="" GOTO END
DOTNET ef migrations add "%MigName%" -o Migrations -s ..\RomsBrowse.Web
GOTO END

:END
POPD
ENDLOCAL
