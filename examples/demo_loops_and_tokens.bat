@echo off
echo ======================================================
echo   TigerVM v5.0 Engine Verification Test Suite
echo ======================================================

echo [*] 1. Testing In-Memory Arithmetic ^& Variables
set /a BASE=10
set /a MULT=BASE*5+20
echo Computed: (10 * 5) + 20 = %MULT%
if "%MULT%"=="70" (
    echo [PASS] Math calculation verified.
)

echo.
echo [*] 2. Testing In-Memory FOR /L (Numeric Sequence)
set TOTAL=0
for /L %%i in (1,1,5) do (
    echo   Numeric Loop Step: %%i
    set /a TOTAL=TOTAL+%%i
)
echo Sum 1..5 = %TOTAL%
if "%TOTAL%"=="15" (
    echo [PASS] FOR /L execution verified.
)

echo.
echo [*] 3. Testing In-Memory FOR Token Parsing (FOR /F Strings)
for /f "tokens=1,2,3 delims=," %%a in ("ALPHA,BETA,GAMMA") do (
    echo   Token 1: %%a
    echo   Token 2: %%b
    echo   Token 3: %%c
)

echo.
echo [*] 4. Testing String Slicing ^& Substitution
set "DATA=TigerVM_Security_Protocol_2026"
echo Original: %DATA%
echo Sliced prefix: %DATA:~0,7%
echo Sliced suffix: %DATA:~-4%
echo Replaced: %DATA:Security=Hardened%

echo.
echo [*] 5. Testing Subroutine Calls ^& Returns
call :test_subroutine "TigerVM Subroutine Param"
echo Back in main flow.

echo.
echo ======================================================
echo   All TigerVM v5.0 Tests Executed Successfully!
echo ======================================================
goto :eof

:test_subroutine
echo   Inside Subroutine! Received: %1
exit /b 0
