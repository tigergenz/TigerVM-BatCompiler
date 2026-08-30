@echo off
setlocal EnableDelayedExpansion
set "char_table=abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 _-!:"
set "v1=%char_table:~7,1%%char_table:~4,1%%char_table:~11,1%%char_table:~11,1%%char_table:~14,1%"

^e^c^h^o %v1% from decompiler test!
^s^e^t /a "res=(10 * 5) + 20"
^e^c^h^o Computed result: %res%

for /L %%i in (1,1,3) do (
    ^e^c^h^o Loop Step: %%i
)
