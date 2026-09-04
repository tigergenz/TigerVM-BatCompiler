@echo off
where py >nul 2>nul
if %errorlevel% equ 0 (
    py "%~dp0batc.py" %*
) else (
    python "%~dp0batc.py" %*
)
