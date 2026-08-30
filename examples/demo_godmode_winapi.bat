@echo off
:: TigerVM v6.0-ULTRA Win32 Direct FFI Gateway Demo
:: No drop files, no powershell child process, 100% In-Memory API Execution

::@hud "TigerVM v6.0-ULTRA" | "Direct Win32/NTAPI FFI Invocation Engine"
echo [*] Initializing Direct Win32 Subsystem in RAM...

:: Call kernel32.dll Beep(750, 200)
echo [+] Triggering Hardware Sound Beep via kernel32.dll Beep()...
::@winapi kernel32.dll Beep 750 200

:: Call kernel32.dll GetTickCount64()
::@winapi kernel32.dll GetTickCount64
echo [+] System Uptime Raw Milliseconds: %WINAPI_RESULT% ms

:: Call user32.dll MessageBeep(0)
::@winapi user32.dll MessageBeep 0

:: Call user32.dll MessageBoxW(0, "TigerVM Native In-Memory Win32 Execution!", "TigerVM God Mode", 64)
echo [+] Spawning Native Win32 Modal GUI Dialog...
::@winapi user32.dll MessageBoxW 0 "TigerVM v6.0-ULTRA: Zero-Disk Win32 API Execution directly from Batch!" "TigerVM FFI Gateway" 64
echo [+] MessageBox Returned ID: %WINAPI_RESULT%

echo [*] Win32 Direct API Pipeline Verified Successfully!
pause
