@echo off
:: ======================================================
:: TigerVM v7.0-TITAN Direct GUI & HTTP Webhook Demo
:: Demonstrates:
::  - Native In-Memory GUI Dialogs (MsgBox, InputBox, FileDialog)
::  - Native System Tray Toast Notifications
::  - In-Memory HTTP GET Request without Curl or PowerShell
:: ======================================================

::@hud "TigerVM v7.0-TITAN" | "Direct GUI Dialogs & HTTP Gateway in RAM"

echo [*] 1. Dispatching System Tray Toast Notification...
::@notify "TigerVM Alert" | "Native In-Memory GUI and Webhook Pipeline Active!" | 3 | Info
echo [PASS] Toast Notification Dispatched.

echo.
echo [*] 2. Spawning Native GUI Input Dialog...
::@inputbox USER_NAME "Please enter your operator callsign:" "Agent_Tiger" "TigerVM Security Console"
echo [RESULT] Received Operator Name: %USER_NAME%

echo.
echo [*] 3. Spawning Native GUI Modal Question Dialog...
::@msgbox "Security Clearance Check" | "Operator: %USER_NAME%\nGrant root execution level to TigerVM engine?" | YesNo | Question | USER_CHOICE
echo [RESULT] User Dialog Decision: %USER_CHOICE%

echo.
echo [*] 4. Testing In-Memory HTTP GET Webhook Request...
echo [*] Querying httpbin.org public endpoint in RAM...
::@http_get HTTP_DATA "https://httpbin.org/get" 8000
echo [RESULT] HTTP Response Length:
echo ------------------------------------------------------
echo %HTTP_DATA:~0,180%...
echo ------------------------------------------------------

echo.
echo ======================================================
echo   TigerVM v7.0 GUI & HTTP Pipeline Verified!
echo ======================================================
pause
