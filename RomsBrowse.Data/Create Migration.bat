@ECHO OFF
SETLOCAL
PUSHD "%~dp0"

SET MigName=
SET /P MigName=Migration name: 
IF "%MigName%"=="" GOTO END
DOTNET ef migrations add "%MigName%" --context SqlServerContext -o Migrations\SqlServer -s ..\RomsBrowse.Web
DOTNET ef migrations add "%MigName%" --context SQLiteContext -o Migrations\SQLite -s ..\RomsBrowse.Web
GOTO END

:END
POPD
ENDLOCAL
