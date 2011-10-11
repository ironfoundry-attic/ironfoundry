@echo off

setlocal

set VCVARSALL="C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"
set SLN="%~dp0\CloudFoundry.Net.sln"

if not exist %VCVARSALL% (
    echo Required file %VCVARSALL% not found.
    exit 1
)

if not exist %SLN% (
    echo Required file %SLN% not found.
    exit 1
)

rem Prevent this from being run multiple times on a dev machine
if "%DevEnvDir%"=="" (
    call %VCVARSALL% x86
)

rem msbuild /v:n /t:build /p:Configuration=Debug %SLN%

msbuild /v:n /t:rebuild /p:Configuration=Release %SLN%

exit /b %ERRORLEVEL%
