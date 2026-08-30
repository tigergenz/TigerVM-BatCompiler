@echo off
setlocal EnableDelayedExpansion

::@hud "TIGERVM TITAN v9.0" | "REGISTRY, MEMORY, TELEMETRY & NETWORKING ENGINE"

echo [*] Initializing TigerVM v9.0-TITAN Subsystems...
::@hud_spinner 800 "Booting Zero-Disk Stack & Hardware Probes..."

echo.
echo ========================================================
echo   [1] HARDWARE & SYSTEM TELEMETRY (OPCODE 61)
echo ========================================================
::@sys_info CPU_COUNT CPU_COUNT
::@sys_info OS_VER OS_VERSION
::@sys_info IS_64 IS_64BIT
::@sys_info NODE_NAME MACHINE_NAME
::@sys_info CUR_USER USER_NAME
::@sys_info UPTIME_S UPTIME_SEC

echo [+] CPU Logical Cores  : %CPU_COUNT%
echo [+] Operating System   : %OS_VER%
echo [+] 64-Bit OS Detected : %IS_64%
echo [+] Machine Hostname   : %NODE_NAME%
echo [+] Active User        : %CUR_USER%
echo [+] System Uptime (sec): %UPTIME_S%

echo.
echo ========================================================
echo   [2] IN-MEMORY REGISTRY DIRECT GATEWAY (OPCODES 55, 56)
echo ========================================================
:: Write test key to HKCU
::@reg_write HKCU "Software\TigerGenZ\TitanVM" "EngineVersion" "v9.0-TITAN" SZ
echo [+] Registry Key Write Status: %REG_RESULT%

:: Read test key from HKCU
::@reg_read READ_VER HKCU "Software\TigerGenZ\TitanVM" "EngineVersion"
echo [+] Registry Key Read Value  : %READ_VER%

:: Read Windows Product Name or System Info from HKLM
::@reg_read WIN_PROD HKLM "Software\Microsoft\Windows NT\CurrentVersion" "ProductName"
echo [+] System OS Product Name   : %WIN_PROD%

echo.
echo ========================================================
echo   [3] RAW UNMANAGED RAM BUFFER & POINTERS (OPCODES 57-60)
echo ========================================================
:: Allocate 512 bytes unmanaged memory
::@mem_alloc PTR 512
echo [+] Allocated 512 bytes unmanaged RAM at address: %PTR%

:: Write string into RAM buffer
::@mem_write %PTR% "TigerVM In-Memory Protected Binary Stream 2026"
echo [+] Written string payload into RAM address %PTR%

:: Read string back from RAM buffer
::@mem_read RAM_STR %PTR% 64
echo [+] Readback from RAM address %PTR%: "%RAM_STR%"

:: Free memory buffer
::@mem_free %PTR%
echo [+] Memory buffer at %PTR% successfully freed!

echo.
echo ========================================================
echo   [4] ZERO-PROCESS NETWORK SOCKET PROBE (OPCODE 62)
echo ========================================================
echo [*] Testing Localhost DNS / HTTP port...
::@net_ping DNS_CHECK 127.0.0.1 53 500
echo [+] Local DNS Port 53 Status: %DNS_CHECK%

::@net_ping GITHUB_CHECK github.com 443 1500
echo [+] GitHub HTTPS Port 443   : %GITHUB_CHECK%

echo.
echo ========================================================
echo   [5] TELEMETRY DASHBOARD TABLE (OPCODE 52)
echo ========================================================
::@hud_table "PROPERTY | VALUE | STATUS" | "CPU Cores | %CPU_COUNT% Cores | ONLINE" | "OS Architecture | 64-Bit (%IS_64%) | VERIFIED" | "Registry Gateway | %READ_VER% | PASS" | "RAM Buffer Pointer | %PTR% | FREED" | "GitHub Port 443 | %GITHUB_CHECK% | REACHABLE"

echo.
echo ========================================================
echo   [6] STRUCTURED EXCEPTION HANDLING (SEH) (OPCODES 49-51)
echo ========================================================
::@try
    echo [*] Inside SEH Try Block - testing arithmetic and memory safety...
    set /a "DIV_SAFE=100 / 5"
    echo [+] Safe division result: %DIV_SAFE%
::@catch ERR_MSG
    echo [-] SEH Fault Intercepted: %ERR_MSG%
::@end_try

echo.
echo [OK] TigerVM v9.0-TITAN Complete System Showcase Finished Successfully!
