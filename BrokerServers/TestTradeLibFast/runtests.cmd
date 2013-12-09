@echo off
if %4=="x64" (
set CFIXRUNNER=cfix64.exe
) else (
set CFIXRUNNER=cfix32.exe
)
echo using %CFIXRUNNER% in %3\cfix\bin\ for platform: %4
echo Attemping to switch working directory to %1
cd %1
echo Running tests in %2
%3\cfix\bin\%CFIXRUNNER% -z -u %2
