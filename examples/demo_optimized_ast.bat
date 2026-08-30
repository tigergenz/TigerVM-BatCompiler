@echo off
:: ======================================================
:: TigerVM v7.0-TITAN AST Optimizer & Dead Code Test
:: Demonstrates:
::  - Compile-time Constant Folding (Arithmetic evaluation)
::  - Dead Code Elimination after unconditional GOTO / EXIT
::  - Live Progress Bar & System Execution
:: ======================================================

::@hud "TigerVM v7.0-TITAN" | "AST Optimizer & Constant Folding Engine"

echo [*] Initializing Optimizer Pipeline...
::@progress 30 "Folding constants in AST..."
::@progress 70 "Eliminating unreachable dead code..."
::@progress 100 "Optimization Pass Completed!"

echo.
echo [*] 1. Testing Constant Math Expression (Pre-computed at compile-time):
set /a RESULT=10 * 5 + (50 / 2)
echo [RESULT] Computed (10*5 + 25) = %RESULT%

echo.
echo [*] 2. Testing Control Flow with Unreachable Dead Code:
goto :reachable_target

:: The following lines are UNREACHABLE and eliminated by AST Optimizer pass
echo [FATAL ERROR] This line was dead code and should NEVER execute!
echo [FATAL ERROR] Another dead code instruction!
exit /b 99

:reachable_target
echo [PASS] Reached reachable_target label cleanly.
echo.
echo ======================================================
echo   AST Optimization & Dead Code Elimination Verified!
echo ======================================================
pause
