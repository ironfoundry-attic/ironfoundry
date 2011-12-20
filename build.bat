@echo off

setlocal

set VCVARSALL="C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"

set SLN="%~dp0\IronFoundry.sln"
set VERSION=1.1.0.0

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

powershell -nologo -file clean.ps1

msbuild /v:n /t:build /p:Configuration=Debug /p:Platform="Any CPU" %SLN%

msbuild /v:n /t:build /p:Configuration=Release /p:Platform=x86 /p:WixValues="PLATFORM=x86;VERSION=%VERSION%" %SLN%
msbuild /v:n /t:build /p:Configuration=Release /p:Platform=x64 /p:WixValues="PLATFORM=x64;VERSION=%VERSION%" %SLN%

exit /b %ERRORLEVEL%
