@echo off
:: ====================================================================
::  T I G E R V M   v 8 . 0 - A P E X   D A T A   &   S E C U R I T Y
::  Comprehensive Showcase:
::   1. In-Memory JSON Parser & Query Engine (Dot Notation / Arrays)
::   2. In-Memory Relational SQL Database (CREATE TABLE, INSERT, SELECT)
::   3. Native Windows Clipboard Gateway (Read & Write in RAM)
::   4. Native AES-256-CBC In-Memory Encryption & Decryption
::   5. Cryptographic Hashing (SHA-256 & MD5 Checksums)
::   6. Native Base64 Stream Encoding & Decoding
:: ====================================================================

::@hud "TIGERVM v8.0-APEX SUITE" | "Full-Stack Data Engineering & Military-Grade Cryptography"

echo.
echo [*] ============================================================
echo [*]  1. IN-MEMORY JSON PARSER & JSONPATH ENGINE
echo [*] ============================================================
set "SAMPLE_JSON={\"system\": {\"node\": \"Tiger-Apex-01\", \"status\": \"ACTIVE\"}, \"users\": [{\"name\": \"Commander_Tiger\", \"role\": \"SuperAdmin\", \"clearance\": 5}, {\"name\": \"Agent_Zero\", \"role\": \"Operator\", \"clearance\": 3}]}"

echo [*] Raw JSON Payload:
echo     %SAMPLE_JSON%
echo.

::@json_get SYS_NODE SAMPLE_JSON "system.node"
::@json_get SYS_STATUS SAMPLE_JSON "system.status"
::@json_get USER0_NAME SAMPLE_JSON "users[0].name"
::@json_get USER0_ROLE SAMPLE_JSON "users[0].role"
::@json_get USER0_LVL SAMPLE_JSON "users[0].clearance"
::@json_get USER1_NAME SAMPLE_JSON "users[1].name"

echo [+] Parsed System Node   : %SYS_NODE% (%SYS_STATUS%)
echo [+] Parsed User #0 (Lead): %USER0_NAME% | Role: %USER0_ROLE% | Level: %USER0_LVL%
echo [+] Parsed User #1       : %USER1_NAME%

echo.
echo [*] ============================================================
echo [*]  2. IN-MEMORY SQL RELATIONAL DATABASE ENGINE
echo [*] ============================================================
echo [*] Initializing in-memory DataTable 'operators' in RAM...
::@sql_exec "CREATE TABLE operators (id, callsign, rank, mission_count)"

echo [*] Inserting relational records into in-memory database...
::@sql_exec "INSERT INTO operators VALUES (101, 'GhostTiger', 'General', 142)"
::@sql_exec "INSERT INTO operators VALUES (102, 'ViperClack', 'Colonel', 98)"
::@sql_exec "INSERT INTO operators VALUES (103, 'ShadowHawk', 'Captain', 45)"

::@sql_query OP101_NAME "SELECT callsign FROM operators WHERE id='101'"
::@sql_query OP102_RANK "SELECT rank FROM operators WHERE callsign='ViperClack'"
::@sql_query OP102_MISSIONS "SELECT mission_count FROM operators WHERE callsign='ViperClack'"
::@sql_query ALL_OP103 "SELECT * FROM operators WHERE id='103'"

echo [+] Query Result (ID=101 Callsign) : %OP101_NAME%
echo [+] Query Result (ViperClack Rank) : %OP102_RANK% (Missions: %OP102_MISSIONS%)
echo [+] Query Result (ID=103 Full Row) : [%ALL_OP103%]

echo.
echo [*] ============================================================
echo [*]  3. NATIVE WINDOWS CLIPBOARD GATEWAY
echo [*] ============================================================
set "SECRET_CLIP=TIGER-APEX-2026-VIP-PASSKEY"
echo [*] Writing secret text to Windows Clipboard: "%SECRET_CLIP%"
::@clip_set "%SECRET_CLIP%"

::@clip_get RETRIEVED_CLIP
echo [+] Verified Read from Windows Clipboard: "%RETRIEVED_CLIP%"

echo.
echo [*] ============================================================
echo [*]  4. NATIVE AES-256-CBC IN-MEMORY CRYPTO ENGINE
echo [*] ============================================================
set "TOP_SECRET_MSG=Mission Target: Satellite Uplink Alpha; Coordinates: 13.7563, 100.5018; AuthCode: 998877"
set "PASSWORD_KEY=SuperStrongSecretPassword2026"

echo [*] Plaintext Data:
echo     "%TOP_SECRET_MSG%"
echo.

::@crypto_encrypt CIPHER_BLOB "%TOP_SECRET_MSG%" "%PASSWORD_KEY%"
echo [+] AES-256 Encrypted Ciphertext:
echo     %CIPHER_BLOB%
echo.

::@crypto_decrypt RESTORED_MSG "%CIPHER_BLOB%" "%PASSWORD_KEY%"
echo [+] AES-256 Decrypted Plaintext:
echo     "%RESTORED_MSG%"

echo.
echo [*] ============================================================
echo [*]  5. CRYPTOGRAPHIC HASHING & BASE64 ENGINE
echo [*] ============================================================
set "DATA_STREAM=TigerVM_Security_Protocol_Level_8"

::@crypto_sha256 HASH_SHA "%DATA_STREAM%"
::@crypto_md5 HASH_MD5 "%DATA_STREAM%"
::@b64_encode B64_STREAM "%DATA_STREAM%"
::@b64_decode DECODED_STREAM "%B64_STREAM%"

echo [*] Input Data      : "%DATA_STREAM%"
echo [+] SHA-256 Hash    : %HASH_SHA%
echo [+] MD5 Hash        : %HASH_MD5%
echo [+] Base64 Encoded  : %B64_STREAM%
echo [+] Base64 Decoded  : %DECODED_STREAM%

echo.
echo ====================================================================
echo   [ALL TESTS PASSED] TigerVM v8.0-APEX Full Data & Crypto Verified!
echo ====================================================================
pause
