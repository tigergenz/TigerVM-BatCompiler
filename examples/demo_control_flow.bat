@echo off
echo [*] Testing TigerVM Variables, Math, Logic and Loops:
set "MESSAGE=TigerGenZ_Virtual_Machine_Security"
echo Original: %MESSAGE%
echo Slice 0,8: %MESSAGE:~0,8%
echo Slice -8: %MESSAGE:~-8%
set /a NUM1=15+25
set /a NUM2=NUM1*2
echo Math result (15+25)*2 = %NUM2%

if "%NUM2%"=="80" (
    echo [PASS] Condition matched 80 correctly!
)

set COUNT=1
:loop_start
echo Loop Counter: %COUNT%
set /a COUNT=COUNT+1
if %COUNT% LEQ 3 goto loop_start

echo [*] TigerVM Execution Finished Successfully!
