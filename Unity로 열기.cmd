@echo off
set "DRAGON_EDITOR=C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe"
if not exist "%DRAGON_EDITOR%" (
  echo Unity 6000.3.10f1 is required. Open this folder from Unity Hub.
  pause
  exit /b 1
)
start "" "%DRAGON_EDITOR%" -projectPath "%~dp0."
