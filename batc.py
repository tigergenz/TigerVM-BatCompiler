#!/usr/bin/env python3
"""
TigerVM Batch to Native PE Compiler & Obfuscation Engine (batc)
Version 5.0.0-PRO [Zero-Disk In-Memory Virtual Stack // Enterprise Hardening Suite]
"""

import os
import sys
import argparse

# Ensure UTF-8 output on Windows consoles
if sys.platform == "win32":
    try:
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
        sys.stderr.reconfigure(encoding="utf-8", errors="replace")
    except Exception:
        pass

from builder import compile_batch_to_exe, find_csc_compiler
from protector import BatchObfuscator, TigerVMCompiler
from decompiler import BatchDecompiler

BANNER = r"""
 +==============================================================+
 |  T I G E R V M   ::   B A T C H   C O M P I L E R   P R O    |
 |  Zero-Disk Virtual Machine & Enterprise Binary Hardening     |
 |  Build v8.0.0-APEX  | Arch: JIT + SQL/Data + Crypto AES-256  |
 +==============================================================+
"""

def print_banner():
    print(BANNER)


def main():
    parser = argparse.ArgumentParser(
        description="TigerVM Hardened Batch Compiler & Code Virtualization Suite v8.0-APEX.",
        formatter_class=argparse.RawTextHelpFormatter,
    )

    parser.add_argument("-i", "--input", required=True, help="Path to input .bat or .cmd script")
    parser.add_argument("-o", "--output", help="Path to output file (.exe or .bat). Auto-generated if omitted.")
    parser.add_argument(
        "--mode",
        choices=["compile", "obf", "decompile"],
        help="Target mode: 'compile' (native binary), 'obf' (obfuscate), or 'decompile' (restore script).",
    )
    parser.add_argument(
        "-d",
        "--decompile",
        "--deobf",
        action="store_true",
        help="Decompile and deobfuscate protected batch scripts",
    )
    parser.add_argument(
        "--cff",
        action="store_true",
        default=True,
        help="Enable Control Flow Flattening state machine (Default: True)",
    )
    parser.add_argument(
        "--no-cff",
        action="store_true",
        help="Disable Control Flow Flattening",
    )
    parser.add_argument(
        "--armor",
        "--anti-debug",
        action="store_true",
        default=True,
        help="Enable Anti-Analysis, Anti-Debug, and SHA-256 Anti-Tamper Seal (Default: True)",
    )
    parser.add_argument(
        "--no-armor",
        action="store_true",
        help="Disable Anti-Analysis Armor",
    )
    parser.add_argument(
        "--jit",
        action="store_true",
        default=True,
        help="Enable Native x86/x64 In-Memory JIT Math Engine (Default: True)",
    )
    parser.add_argument(
        "--no-jit",
        action="store_true",
        help="Disable JIT Machine Code Math Engine",
    )
    parser.add_argument(
        "--unhook",
        action="store_true",
        default=True,
        help="Enable Per-Process Pristine NTDLL Unhooking & Memory Integrity (Default: True)",
    )
    parser.add_argument(
        "--no-unhook",
        action="store_true",
        help="Disable NTDLL Unhooking",
    )
    parser.add_argument(
        "--cyberpunk",
        "--hud",
        action="store_true",
        help="Enable Cyberpunk Neon HUD & Graphic Console Interface",
    )
    parser.add_argument(
        "--anti-vm",
        "--anti-sandbox",
        action="store_true",
        help="Enable Hypervisor & Automated Sandbox Environment Evasion",
    )
    parser.add_argument(
        "--disasm",
        action="store_true",
        help="Disassemble script into human-readable TigerVM Bytecode table",
    )
    parser.add_argument(
        "--simulate",
        action="store_true",
        help="Run in-terminal TigerVM simulation and state trace",
    )
    parser.add_argument(
        "--level",
        type=int,
        choices=[1, 2, 3],
        default=3,
        help="Obfuscation level (1 = Linear Slicing, 2 = In-Memory Stream, 3 = Polymorphic Chaos Matrix). Default: 3",
    )
    parser.add_argument(
        "--insane",
        action="store_true",
        help="Shortcut for Level 3 Polymorphic Chaos Matrix",
    )
    parser.add_argument(
        "--tag",
        "--signature",
        default="tigergenz",
        help="Security signature tag prefix (Default: tigergenz)",
    )
    parser.add_argument("--hide", "--hidden", action="store_true", help="Hide console subsystem window during execution")
    parser.add_argument("--admin", action="store_true", help="Require Administrator privileges via application manifest")
    parser.add_argument("--icon", help="Path to custom application icon (.ico)")
    parser.add_argument("--embed", nargs="*", help="Additional asset files to encapsulate within the binary")

    # Metadata
    parser.add_argument("--title", default="TigerVM Standalone Application", help="Assembly Title")
    parser.add_argument("--desc", default="Compiled TigerVM Hardened Executable", help="File Description")
    parser.add_argument("--company", default="tigergenz", help="Company Name")
    parser.add_argument("--version", default="6.0.0.0", help="File Version")
    parser.add_argument("--copyright", default="Copyright (C) tigergenz", help="Legal Copyright")

    args = parser.parse_args()
    print_banner()

    if not os.path.exists(args.input):
        print(f"[!] Error: Target script not found: {args.input}")
        sys.exit(1)

    with open(args.input, "r", encoding="utf-8", errors="replace") as f:
        script_content = f.read()

    # Disassembly Option
    if args.disasm:
        TigerVMCompiler.disassemble(script_content)
        return

    # Simulation Option (delegate to C# runner or internal trace)
    if args.simulate:
        csc = find_csc_compiler()
        batc_exe = os.path.join(os.path.dirname(os.path.abspath(__file__)), "batc.exe")
        if os.path.exists(batc_exe):
            import subprocess
            subprocess.run([batc_exe, "-i", os.path.abspath(args.input), "--simulate"])
        else:
            TigerVMCompiler.disassemble(script_content)
        return

    input_base = os.path.splitext(args.input)[0]

    # Decompilation / Deobfuscation Mode
    if args.decompile or args.mode == "decompile":
        if not args.output:
            args.output = input_base + "_decompiled.bat"
        print(f"[*] Source Target   : {os.path.abspath(args.input)}")
        print(f"[*] Output Script   : {os.path.abspath(args.output)}")
        print(f"[*] Operational Mode: DECOMPILE & DEOBFUSCATE")
        print("\n[+] Running Multi-Pass Batch Decompiler & Deobfuscator Engine...")
        try:
            decompiled = BatchDecompiler.deobfuscate_script(script_content)
            with open(args.output, "w", encoding="utf-8") as f:
                f.write(decompiled)
            size_kb = os.path.getsize(args.output) / 1024.0
            print(f"[OK] Decompiled script saved: {args.output} ({size_kb:.1f} KB)")
        except Exception as e:
            print(f"[!] Decompilation Error: {e}")
            sys.exit(1)
        return

    # Determine mode & output path
    if args.mode:
        mode = args.mode
    else:
        if args.output and args.output.lower().endswith(".bat"):
            mode = "obf"
        else:
            mode = "compile"

    if not args.output:
        if mode == "compile":
            args.output = input_base + ".exe"
        else:
            args.output = input_base + "_protected.bat"

    print(f"[*] Source Target   : {os.path.abspath(args.input)}")
    print(f"[*] Output Binary   : {os.path.abspath(args.output)}")
    print(f"[*] Operational Mode: {mode.upper()}")

    if mode == "compile":
        enable_armor = not args.no_armor
        enable_cff = not args.no_cff
        enable_jit = not args.no_jit
        enable_unhook = not args.no_unhook
        print(f"[*] Execution Model : TigerVM v6.0-ULTRA (Zero-Disk In-Memory Virtual Stack)")
        print(f"[*] Native JIT      : {'Active x86/x64 Machine Code Math Engine' if enable_jit else 'Standard Math'}")
        print(f"[*] Control Flow    : {'Control Flow Flattening (CFF ACTIVE)' if enable_cff else 'Direct Linear Execution'}")
        print(f"[*] Armor Engine    : {'Anti-Analysis, Anti-Debug, SHA-256 Seal ACTIVE' if enable_armor else 'Standard TigerVM'}")
        print(f"[*] Memory Shield   : {'Per-Process Pristine NTDLL Unhooking ACTIVE' if enable_unhook else 'Standard'}")
        print(f"[*] Sandbox Evasion : {'Active Hypervisor & Spec Checks' if args.anti_vm else 'Standard'}")
        print(f"[*] Console Window  : {'Hidden (Background)' if args.hide else 'Standard Console'}")
        print(f"[*] Privilege Level : {'Elevated Administrator (requireAdministrator)' if args.admin else 'Standard User (asInvoker)'}")
        if args.icon:
            print(f"[*] Application Icon: {args.icon}")
        if args.embed:
            print(f"[*] Encapsulated    : {len(args.embed)} file(s)")

        metadata = {
            "title": args.title,
            "description": args.desc,
            "company": args.company,
            "version": args.version,
            "copyright": args.copyright,
        }

        print("\n[+] Compiling Bytecode and Hardening Native Binary...")
        try:
            success = compile_batch_to_exe(
                input_bat_path=args.input,
                output_exe_path=args.output,
                hidden=args.hide,
                require_admin=args.admin,
                armor=enable_armor,
                cff=enable_cff,
                anti_vm=args.anti_vm,
                icon_path=args.icon,
                embed_files=args.embed,
                metadata=metadata,
                jit=enable_jit,
                unhook=enable_unhook,
                cyberpunk=args.cyberpunk,
            )
            if success and os.path.exists(args.output):
                size_kb = os.path.getsize(args.output) / 1024.0
                print(f"[OK] Build complete: {args.output} ({size_kb:.1f} KB)")
            else:
                print("[!] Build failed.")
                sys.exit(1)
        except Exception as e:
            print(f"[!] Compilation Error: {e}")
            sys.exit(1)

    elif mode == "obf":
        obf_level = 3 if args.insane else args.level
        print(f"[*] Signature Tag   : {args.tag}")
        print(f"[*] Obfuscation     : Level {obf_level} {'[Polymorphic Chaos Matrix]' if obf_level == 3 else ''}")
        print("\n[+] Obfuscating Batch Script...")
        try:
            obfuscator = BatchObfuscator(signature=args.tag)
            if obf_level == 1:
                result_code = obfuscator.obfuscate_basic(script_content)
            elif obf_level == 2:
                result_code = obfuscator.obfuscate_advanced(script_content)
            else:
                result_code = obfuscator.obfuscate_insane(script_content)

            with open(args.output, "w", encoding="utf-8") as f:
                f.write(result_code)

            size_kb = os.path.getsize(args.output) / 1024.0
            print(f"[OK] Obfuscated script saved: {args.output} ({size_kb:.1f} KB)")
        except Exception as e:
            print(f"[!] Obfuscation Error: {e}")
            sys.exit(1)


if __name__ == "__main__":
    main()
