# TigerVM Enterprise Batch Compiler, Decompiler & Hardened Binary Suite
**Version 10.0.0-SINGULARITY | Dynamic In-Memory C# Evaluator, Win32 Named Pipes & Shared Memory IPC, Advapi32 Service Gateway & Native Machine Code Engine**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Platform: Windows](https://img.shields.io/badge/Platform-Windows%207%2F8%2F10%2F11-0078D6.svg)](https://microsoft.com/windows)
[![Architecture: x86/x64](https://img.shields.io/badge/Arch-x86%20%7C%20x64%20Native%20JIT-brightgreen.svg)]()
[![Build: v10.0.0-SINGULARITY](https://img.shields.io/badge/Build-v10.0.0--SINGULARITY-red.svg)]()
[![Open Source: GitHub Ready](https://img.shields.io/badge/Open%20Source-GitHub%20Ready-success.svg)]()

TigerVM is a high-performance open-source batch script compilation, virtualization, and decompiler toolchain. It provides complete end-to-end tooling to convert standard Windows Batch scripts (`.bat` / `.cmd`) into standalone native PE executables (`.exe`), apply polymorphic script obfuscation, or reverse-engineer and deobfuscate heavily obfuscated batch scripts back into clean, readable code.

Built with a proprietary **Zero-Disk In-Memory Virtual Stack**, TigerVM executes batch logic entirely within memory without dropping temporary batch files to disk or `%TEMP%`, neutralizing runtime extraction and memory-dumping attacks.

---

## 🛡️ Security Architecture & Toolchain Pipeline

```
  ┌────────────────────────────────────────────────────────┐
  │           Source Script / Obfuscated Payload           │
  └──────────────┬───────────────────────────┬─────────────┘
                 │                           │
                 ▼                           ▼
  ┌─────────────────────────────┐ ┌─────────────────────────────┐
  │   TigerVM Compiler & PE     │ │   TigerVM Decompiler Core   │
  │ • Dynamic In-Memory C# Eval │ │ • Multi-Table Chaos Matrix  │
  │ • Win32 Named Pipe IPC Host │ │ • Base64 Stream Extractor   │
  │ • Shared Memory (MMF) Engine│ │ • Caret / Quote Normalizer  │
  │ • Advapi32 Service Gateway  │ │ • Bytecode AST Reconstruct  │
  │ • RWX Machine Code / Shell  │ │ • Polymorphic Junk Cleaner  │
  │ • In-Memory Deflate Stream  │ │ • 72-Opcode Decompilation   │
  │ • Native x86/x64 JIT Engine │ │                             │
  │ • SEH Exception Guard (RAM) │ │                             │
  │ • Direct Win32/NTAPI FFI    │ │                             │
  │ • In-Memory Registry Engine │ │                             │
  │ • Raw RAM Pointer Buffers   │ │                             │
  │ • Hardware/Sys Telemetry    │ │                             │
  │ • Zero-Process Net Probe    │ │                             │
  │ • In-Memory SQL & JSONPath  │ │                             │
  │ • AES-256 / SHA-256 Suite   │ │                             │
  │ • VFS File Tree in RAM      │ │                             │
  │ • Multithreaded Subroutines │ │                             │
  │ • Interactive HUD & Tables  │ │                             │
  │ • Pristine NTDLL Unhooking  │ │                             │
  └──────────────┬──────────────┘ └──────────────┬──────────────┘
                 │                               │
                 ▼                               ▼
  ┌─────────────────────────────┐ ┌─────────────────────────────┐
  │   Hardened Standalone PE    │ │  Clean Reconstructed Batch  │
  │  (.exe RAM Virtual Stack)   │ │      (.bat / .cmd Source)   │
  └─────────────────────────────┘ └─────────────────────────────┘
```

---

## ⚡ TigerVM v10.0-SINGULARITY Core Capabilities

### 1. Dynamic In-Memory C# Evaluator (Zero-Disk)
* Directive: `::@eval_cs <destVar> "<code>"`
* Dynamically compiles and executes arbitrary C# / .NET expressions or full classes in memory via `Microsoft.CSharp.CSharpCodeProvider` with zero disk touch. Ideal for advanced mathematical operations, high-performance regex, and direct CLR reflection.

### 2. High-Speed Inter-Process Communication (IPC)
* **Win32 Named Pipes:** Directives `::@pipe_server <destVar> "<pipeName>" [timeoutMs]` and `::@pipe_client "<pipeName>" "<message>" [timeoutMs]` provide fast local and inter-process client/server message exchange.
* **Shared Memory (MMF):** Directives `::@shm_write "<mapName>" "<data>"` and `::@shm_read <destVar> "<mapName>" [maxBytes]` allocate and access memory-mapped file buffers in RAM across independent processes.

### 3. Direct Win32 Service Gateway (Advapi32)
* Directives: `::@svc_query <destVar> "<serviceName>"` and `::@svc_control <destVar> "<serviceName>" "<START|STOP>"`.
* Queries service status (`RUNNING`, `STOPPED`, `START_PENDING`, etc.) and sends control signals via `OpenSCManager`, `OpenService`, and `QueryServiceStatus` without spawning external processes like `sc.exe` or `net.exe`.

### 4. Native In-Memory Machine Code & Shellcode Engine
* Directive: `::@shell_exec <destVar> "<hexBytes|base64>" [timeoutMs]`
* Allocates executable `PAGE_EXECUTE_READWRITE` memory via `VirtualAlloc`, copies raw machine code instructions or shellcode, spawns a native worker thread via `CreateThread`, waits for execution, and captures the thread exit code.

### 5. In-Memory Registry Direct Gateway (Zero-Process)
* Directives: `::@reg_read <destVar> <hive> <path> <keyName>` and `::@reg_write <hive> <path> <keyName> <value> [type]`.
* Direct Win32 Registry access (`HKLM`, `HKCU`) in RAM without spawning `reg.exe` or triggering Process Creation telemetry.

### 6. Raw Unmanaged RAM Buffers & Pointer Operations
* Directives: `::@mem_alloc <ptrVar> <size>`, `::@mem_free <ptrVar>`, `::@mem_write <ptrVar> <text>`, `::@mem_read <destVar> <ptrVar> <len>`.
* Allocates, reads, and writes memory pointers directly in unmanaged memory for low-level Win32 struct passing.

### 7. Native Hardware & System Telemetry
* Directive: `::@sys_info <destVar> <CPU_COUNT|RAM_TOTAL_MB|RAM_FREE_MB|UPTIME_SEC|OS_VERSION|IS_64BIT|MACHINE_NAME|USER_NAME>`.
* Queries hardware configuration, CPU cores, RAM, and OS telemetry in microseconds with zero external tools (`wmic` / `systeminfo`).

### 8. Zero-Process Network Socket & Port Probes
* Directive: `::@net_ping <destVar> <host> <port> [timeoutMs]`.
* Native TCP socket health checks without spawning `ping.exe` or `curl.exe`.

### 9. In-Memory Bytecode Compression (Deflate Stream)
* Compresses encrypted VM bytecode streams using **RFC 1951 Raw Deflate** before binary emission.
* Decreases executable footprint by up to 70% and generates high payload entropy that disrupts static pattern matching.

### 10. Structured Exception Handling (SEH) for Batch VM
* Native directives: `::@try`, `::@catch <errorVar>`, `::@end_try` (or `::@finally`).
* Traps hardware faults, missing Win32 APIs, invalid arithmetic, and IO errors within batch execution without process crashes.

### 11. Hierarchical In-Memory Virtual File System (VFS)
* Virtual file mounting and management in RAM via `::@vfs_write`, `::@vfs_read`, `::@vfs_list`, and `::@vfs_unzip`.
* 100% Zero-Disk I/O: no physical temporary files are ever created.

### 12. In-Memory JSON Parser & JSONPath Query Engine
* Native directives: `::@json_get <destVar> <srcJson> <jsonPath>` and `::@json_set <destVar> <srcJson> <jsonPath> <newVal>`.
* Queries complex nested JSON objects and arrays (e.g. `users[0].name`, `config.database.port`).

### 13. In-Memory Relational SQL Database
* Native directives: `::@sql_exec <query>` and `::@sql_query <destVar> <query>`.
* Full in-memory relational ADO.NET table supporting `CREATE TABLE`, `INSERT INTO`, and `SELECT WHERE`.

### 14. Native AES-256-CBC Cryptography Suite
* Native directives: `::@crypto_encrypt <destVar> <plainText> <password>` and `::@crypto_decrypt <destVar> <cipherText> <password>`.
* Military-grade AES-256-CBC encryption in RAM with PBKDF2 (1,000 iterations) and 64-bit cryptographic salt.

### 15. Interactive Terminal HUD Widgets
* **ASCII Data Table (`::@hud_table`):** Auto-calculates column widths, borders, and header rows.
* **Interactive Spinner (`::@hud_spinner`):** In-place ASCII rotating spinner (`|`, `/`, `-`, `\`) for background task feedback.
* **Neon Banners & Matrix Rain (`::@hud`, `::@matrix`, `::@progress`).**

### 16. Native x86/x64 JIT Machine Code Math Engine
* Emits and executes raw x86 / x64 machine code in `PAGE_EXECUTE_READWRITE` memory.
* High-speed arithmetic evaluation with hardware zero-divisor guards.

### 17. Direct Win32 / NTAPI FFI Gateway
* Directive: `::@winapi <dll> <function> <args...>`
* Dynamically resolves exports and dispatches unmanaged 64-bit function pointers with automated memory marshaling.

### 18. Multithreaded Subroutines & Synchronization
* Directives: `::@thread <label>` and `::@thread_wait`.
* Spawns non-blocking worker threads in background and synchronizes execution across thread barriers.

### 19. Active Memory Defense & Anti-Tamper
* **Per-Process Pristine NTDLL Unhooking:** Maps a pristine copy of `ntdll.dll` from disk to strip EDR/API hooks in memory.
* **Hardware Breakpoint & Anti-Debug Guard:** Checks DR0-DR7 debug registers and RDTSC timing traps.
* **Control Flow Flattening (CFF):** Obfuscates basic blocks into a state-driven dispatcher loop.

---

## 📋 TigerVM Opcode Instruction Set (0 - 71)

| Opcode | Name | Directive | Description |
| :---: | :--- | :--- | :--- |
| `0` | `NOP` | - | No operation |
| `1` | `ECHO` | `echo <text>` | Print text with variable expansion |
| `2` | `ECHOTOGGLE` | `@echo on/off` | Toggle command echoing |
| `3` | `SETVAR` | `set "var=val"` | Assign string variable |
| `4` | `SETMATH` | `set /a "var=expr"` | Evaluate arithmetic expression (Native JIT) |
| `5` | `SETPROMPT` | `set /p "var=prompt"` | Prompt user input |
| `6` | `GOTO` | `goto :label` | Jump to label or exit |
| `7` | `LABEL` | `:label` | Subroutine or branch marker |
| `8` | `IFCMP` | `if "a"=="b"` | Compare strings or numbers |
| `9` | `IFEXIST` | `if exist path` | Check file or directory existence |
| `10` | `IFDEFINED` | `if defined var` | Check if variable is defined |
| `11` | `IFERRORLEVEL` | `if errorlevel n` | Check process exit code |
| `12` | `CALLSUB` | `call :sub` | Push return state and call subroutine |
| `13` | `RETURN` | `exit /b` | Pop return state |
| `14` | `PAUSE` | `pause` | Wait for user keystroke (Pipe-safe) |
| `15` | `CLS` | `cls` | Clear terminal screen |
| `16` | `TITLE` | `title <text>` | Set console window title |
| `17` | `COLOR` | `color 0A` | Set console color attribute |
| `18` | `CD` | `cd /d <path>` | Change working directory |
| `19` | `DELAY` | `timeout /t n` | Sleep delay in milliseconds |
| `20` | `EXECDIRECT` | `<command>` | Execute process with direct stream |
| `21` | `PIPESTREAM` | - | Execute command via memory stdin pipe |
| `22` | `EXIT` | `exit /b n` | Terminate process with exit code |
| `23` | `FORNUMERIC` | `for /l %%i in (...)` | Numeric sequence loop in RAM |
| `24` | `FORFILES` | `for /r %%f in (...)` | Recursive directory file search |
| `25` | `FORTOKENS` | `for /f "tokens=..."` | String token parsing in RAM |
| `26` | `WINAPI` | `::@winapi <dll> <fn>` | Direct Win32 / NTAPI dynamic FFI call |
| `27` | `THREADSTART` | `::@thread <label>` | Dispatch asynchronous worker thread |
| `28` | `THREADWAIT` | `::@thread_wait` | Thread barrier synchronization (join) |
| `29` | `VFSREAD` | `::@vfs_read <f> <v>` | Read in-memory VFS buffer |
| `30` | `VFSWRITE` | `::@vfs_write <f> <c>` | Write in-memory VFS buffer |
| `31` | `HUDBANNER` | `::@hud "T" \| "S"` | Render Cyberpunk ASCII HUD banner |
| `32` | `HUDPROGRESS` | `::@progress <pct> <l>`| Display glowing console progress bar |
| `33` | `HUDMATRIX` | `::@matrix <lines>` | Render falling matrix text rain |
| `34` | `MEMUNHOOK` | `::@unhook` | Clean `.text` NTDLL memory unhooker |
| `35` | `GUIMSGBOX` | `::@msgbox "T" \| "M"` | Native Windows Forms MessageBox dialog |
| `36` | `GUIINPUTBOX` | `::@inputbox <var> "P"`| Native GUI text input dialog |
| `37` | `GUIFILEDIALOG`| `::@filedialog <var>` | Native Open / Save file dialog |
| `38` | `HTTPGET` | `::@http_get <var> <u>`| In-memory HTTP GET request |
| `39` | `HTTPPOST` | `::@http_post <var> <u>`| In-memory HTTP POST request |
| `40` | `NOTIFY` | `::@notify "T" \| "M"` | Windows System Tray Toast Notification |
| `41` | `JSONGET` | `::@json_get <d> <s> <p>`| In-memory JSONPath parser & query |
| `42` | `JSONSET` | `::@json_set <d> <s> <p>`| In-memory JSON mutation |
| `43` | `SQLEXEC` | `::@sql_exec <query>` | In-memory relational SQL executor |
| `44` | `SQLQUERY` | `::@sql_query <d> <q>` | In-memory relational SQL table query |
| `45` | `CLIPGET` | `::@clip_get <dest>` | Native Windows Clipboard read |
| `46` | `CLIPSET` | `::@clip_set <text>` | Native Windows Clipboard write |
| `47` | `CRYPTOAES` | `::@crypto_encrypt ...`| AES-256-CBC in-memory encryption |
| `48` | `CRYPTOHASH`| `::@crypto_sha256 ...` | Cryptographic SHA256/MD5/Base64 engine |
| `49` | `TRYSTART` | `::@try` | Begin SEH structured exception block |
| `50` | `CATCH` | `::@catch <errVar>` | Intercept runtime error & trap exception |
| `51` | `ENDTRY` | `::@end_try` | End structured exception block |
| `52` | `HUDTABLE` | `::@hud_table "A" \| "B"`| Render ASCII formatted table in Console |
| `53` | `HUDSPINNER` | `::@hud_spinner <ms> "L"`| In-place animated ASCII loading spinner |
| `54` | `VFSLIST` | `::@vfs_list <destVar>` | List all mounted VFS files in RAM |
| `55` | `REGREAD` | `::@reg_read <d> <h> <p>`| Direct In-Memory Win32 Registry Read |
| `56` | `REGWRITE` | `::@reg_write <h> <p> <v>`| Direct In-Memory Win32 Registry Write |
| `57` | `MEMALLOC` | `::@mem_alloc <p> <sz>`| Allocate unmanaged RAM buffer |
| `58` | `MEMFREE` | `::@mem_free <ptr>` | Free unmanaged RAM buffer |
| `59` | `MEMWRITE` | `::@mem_write <p> <t>` | Write string to RAM pointer address |
| `60` | `MEMREAD` | `::@mem_read <d> <p> <l>`| Read string from RAM pointer address |
| `61` | `SYSINFO` | `::@sys_info <d> <prop>`| Native Hardware & System Telemetry |
| `62` | `NETPING` | `::@net_ping <d> <h> <p>`| Zero-Process TCP Socket Port Probe |
| `63` | `VFSUNZIP` | `::@vfs_unzip <z> <pfx>` | Extract ZIP archive to in-memory VFS |
| `64` | `EVALCS` | `::@eval_cs <v> "<code>"`| Dynamic in-memory C# compilation & eval |
| `65` | `PIPESERVER`| `::@pipe_server <v> "<p>"`| Win32 Named Pipe server listener / reader |
| `66` | `PIPECLIENT`| `::@pipe_client "<p>" "<m>"`| Win32 Named Pipe client sender |
| `67` | `SHMWRITE` | `::@shm_write "<m>" "<d>"`| Memory-Mapped File shared memory writer |
| `68` | `SHMREAD` | `::@shm_read <v> "<m>"` | Memory-Mapped File shared memory reader |
| `69` | `SVCQUERY` | `::@svc_query <v> "<s>"` | Direct Advapi32 service status query |
| `70` | `SVCCONTROL`| `::@svc_control <v> "<s>" "<a>"`| Direct Advapi32 service start/stop |
| `71` | `SHELLEXEC` | `::@shell_exec <v> "<code>"`| In-memory machine code / shellcode exec |

---

## 💻 Quick Start & Usage

### 1. Interactive CLI Mode

Launch `batc.exe` or `TigerGenZ_BatCompiler.exe` directly:

```text
 ╔══════════════════════════════════════════════════════════════╗
 ║  T I G E R V M   ::   B A T C H   C O M P I L E R   P R O    ║
 ║  Zero-Disk Virtual Machine & Enterprise Binary Hardening     ║
 ║  Build v10.0.0-SINGULARITY | Arch: Roslyn C# + IPC + Advapi  ║
 ╚══════════════════════════════════════════════════════════════╝

[?] Enter path or drag and drop .bat / .cmd file here:
 >> examples\demo_singularity_v10.bat

[*] Selected Target: demo_singularity_v10.bat

[::] Operational Pipeline:
  [1] Standalone Executable (TigerVM Virtual Machine - Zero-Disk)
  [2] Hardened Executable (TigerVM + CFF + Anti-Analysis + Anti-Tamper)
  [3] Maximum Defense PE (TigerVM + CFF + Anti-VM + Sandbox Evasion)
  [4] Script Obfuscation (Level 3 - Polymorphic Chaos Matrix)
  [5] Script Obfuscation (Level 2 - In-Memory Stdin Stream Loader)
  [6] Disassemble & Inspect TigerVM Bytecode
  [7] Run In-Terminal TigerVM Simulator & Tracer
  [8] Decompile & Deobfuscate Batch Script (.bat)

 Selection [1-8] (Default: 2): 
```

### 2. Command Line Interface (CLI)

```cmd
:: 1. Compile with CFF + Maximum Defense Armor
batc.exe -i examples\demo_singularity_v10.bat -o singularity_app.exe --cff --armor

:: 2. Compile with Data Engine & AES-256 Suite
batc.exe -i examples\demo_data_and_security.bat -o security_app.exe --cff --armor

:: 3. Python CLI Toolchain
python batc.py -i examples\demo_singularity_v10.bat -o singularity_app.exe --cff --armor

:: 4. Disassemble Script Bytecode (View 72-Opcode AST table)
python batc.py -i examples\demo_singularity_v10.bat --disasm

:: 5. Decompile & Deobfuscate Protected Batch Scripts
python batc.py --decompile -i protected.bat -o clean_restored.bat
```

---

## 📂 Project Structure

```text
Bat Compiler/
├── LICENSE                       # MIT Open-Source License
├── README.md                     # Technical Documentation & Opcode Specification
├── .gitignore                    # Git Ignore Rules
├── TigerGenZ_BatCompiler.cs      # Monolithic C# Compiler & Singularity Engine (v10.0)
├── TigerGenZ_BatCompiler.exe     # Standalone Native Compiler Executable (v10.0-SINGULARITY)
├── batc.exe                      # Command Line Interface Binary (v10.0-SINGULARITY)
├── batc.bat                      # CLI Launcher Script
├── batc.py                       # Python CLI Toolchain & Disassembler (v10.0-SINGULARITY)
├── builder.py                    # Python Standalone CSC Invocation & JIT Stub Generator
├── decompiler.py                 # Python Multi-Pass Batch Decompiler & Deobfuscator
├── protector.py                  # Python TigerVM Bytecode AST Compiler & Deflate Engine
└── examples/                     # Demonstration & Verification Test Suite
    ├── demo_basic.bat            # Basic Echo, Arguments & Directory Expansion
    ├── demo_control_flow.bat     # Math, String Slicing, If/Else & Subroutines
    ├── demo_loops_and_tokens.bat # FOR /L numeric loops & FOR /F token parsing
    ├── demo_godmode_winapi.bat   # Direct Win32 FFI (MessageBoxA, Beep, GetTickCount64)
    ├── demo_jit_and_threads.bat  # x86/x64 Native JIT Math & Parallel Multithreading
    ├── demo_cyberpunk_hud.bat    # Neon ASCII HUD, Progress Bars & Matrix Rain
    ├── demo_gui_and_http.bat     # Native GUI Dialogs, System Tray Toast & HTTP GET/POST
    ├── demo_data_and_security.bat# In-Memory JSON, SQL Database, AES-256 & Clipboard
    ├── demo_apex_v8.bat          # SEH Guards, In-Memory VFS, HUD Tables & Spinners
    ├── demo_apex_titan_v9.bat    # Registry Direct, RAM Pointers, Telemetry & Sockets
    ├── demo_singularity_v10.bat  # In-Memory C# Eval, Win32 Named Pipes & Shared Memory, Advapi32, Shellcode
    ├── demo_optimized_ast.bat    # AST Constant Folding & Dead Code Elimination
    └── demo_decompile_test.bat   # Obfuscation Patterns Verification Suite
```

---

## 🤝 Contributing

Contributions are welcome! If you'd like to improve the compiler, add new opcodes, or enhance deobfuscation algorithms:
1. Fork the Repository
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📜 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.
