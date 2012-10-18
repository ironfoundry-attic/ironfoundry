@echo off

setlocal

set VCVARSALL="C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"

set SLN="%~dp0\IronFoundry.sln"
set VERSION=1.9.0

set NOCLEAN=0
if /i "%1"=="NOCLEAN" set NOCLEAN=1

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

if %NOCLEAN% equ 0 (
  echo CLEANING...
  powershell -noprofile -nologo -file clean.ps1
  echo DONE.
)

msbuild /v:n /t:build /p:Configuration=Debug /p:Platform=x86 %SLN%
if ERRORLEVEL 1 goto build_failed

msbuild /v:n /t:build /p:Configuration=Debug /p:Platform=x64 %SLN%
if ERRORLEVEL 1 goto build_failed

msbuild /v:n /t:build /p:Configuration=Release /p:Platform=x86 /p:WixValues="VERSION=%VERSION%" %SLN%
if ERRORLEVEL 1 goto build_failed

msbuild /v:n /t:build /p:Configuration=Release /p:Platform=x64 /p:WixValues="VERSION=%VERSION%" %SLN%
if ERRORLEVEL 1 goto build_failed

exit /b %ERRORLEVEL%

:build_failed
echo Build failed!
exit /b 1
