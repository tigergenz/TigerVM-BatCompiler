"""
decompiler.py - TigerVM Batch Decompiler & Multi-Pass Deobfuscator Engine
"""
import re
import base64
from typing import List, Dict, Tuple, Optional
from protector import TigerVMCompiler


class BatchDecompiler:
    """
    Enterprise Batch Decompiler & Deobfuscator:
    Reverse-engineers and normalizes heavily obfuscated batch scripts, including:
    1. Multi-Table Polymorphic Chaos Matrix (%tag_a:~x,1% + junk noise)
    2. In-Memory PowerShell Base64 Stream Loaders
    3. Linear Variable Slicing (%tag:~x,1%)
    4. Caret Escape Fragmentation (^c^m^d -> cmd)
    5. Double-Quote Noise Insertion (c""m""d -> cmd)
    6. TigerVM Bytecode to Batch Reconstruction
    """

    @staticmethod
    def deobfuscate_script(script_content: str) -> str:
        """
        Runs multi-pass analysis and deobfuscation pipeline.
        """
        content = script_content

        # Pass 1: Check for In-Memory Base64 Stream Loader
        b64_extracted = BatchDecompiler._try_extract_base64_payload(content)
        if b64_extracted:
            content = b64_extracted

        # Pass 2: Extract variable definition dictionary (Linear and Multi-Table)
        var_dict = BatchDecompiler._extract_variable_definitions(content)

        # Pass 3: Multi-pass variable slice expansion
        content = BatchDecompiler._expand_variable_slices(content, var_dict)

        # Pass 4: Caret and Quote normalization
        content = BatchDecompiler._normalize_escapes(content)

        # Pass 5: Remove obfuscation boilerplate and dead noise lines
        content = BatchDecompiler._strip_obfuscation_artifacts(content)

        # Pass 6: Beautify and format output
        content = BatchDecompiler.beautify_batch(content)

        return content

    @staticmethod
    def _try_extract_base64_payload(content: str) -> Optional[str]:
        """
        Detects PowerShell Base64 payload loaders and decodes embedded scripts.
        """
        if "powershell" in content.lower() and "frombase64string" in content.lower():
            # Extract all chunks assigned to payload variables: set "tag_payload=!tag_payload!CHUNK"
            chunk_matches = re.findall(r'set\s+"[^=]+=(?:![^!]+!)?([A-Za-z0-9+/=]{4,})"', content, re.IGNORECASE)
            if chunk_matches:
                full_b64 = "".join(chunk_matches)
                try:
                    decoded = base64.b64decode(full_b64).decode("utf-8", errors="replace")
                    if decoded and len(decoded) > 5:
                        return decoded
                except Exception:
                    pass

            # Alternative: find direct FromBase64String('...') payload
            direct_b64 = re.search(r"FromBase64String\(['\"]([A-Za-z0-9+/=]+)['\"]\)", content, re.IGNORECASE)
            if direct_b64:
                try:
                    decoded = base64.b64decode(direct_b64.group(1)).decode("utf-8", errors="replace")
                    if decoded and len(decoded) > 5:
                        return decoded
                except Exception:
                    pass

        return None

    @staticmethod
    def _extract_variable_definitions(content: str) -> Dict[str, str]:
        """
        Extracts variable string pools defined via `set "VAR=VALUE"` or `set VAR=VALUE`.
        """
        var_dict = {}
        # Match set "var=val" or set var=val
        matches = re.findall(r'^\s*set\s+(?:"([^=]+)=([^"]*)"|([^=\s]+)=([^\r\n]*))', content, re.MULTILINE | re.IGNORECASE)
        for m in matches:
            if m[0]:
                k, v = m[0].strip(), m[1]
            else:
                k, v = m[2].strip(), m[3].rstrip()
            
            # Skip environment special keywords unless string literal
            if k and not k.startswith("/") and len(v) > 0:
                var_dict[k] = v

        return var_dict

    @staticmethod
    def _expand_variable_slices(content: str, var_dict: Dict[str, str]) -> str:
        """
        Resolves variable slice expressions like %var:~start,length% and %var:~start%
        Iterates multiple passes to resolve nested references.
        """
        max_passes = 6
        current = content

        for _ in range(max_passes):
            changed = False

            # Pattern for %VAR:~start,length% or %VAR:~start%
            def slice_replacer(match):
                nonlocal changed
                var_name = match.group(1)
                slice_spec = match.group(2)

                if var_name in var_dict:
                    val = var_dict[var_name]
                    parts = slice_spec.split(",")
                    try:
                        start = int(parts[0])
                        if start < 0:
                            start = max(0, len(val) + start)
                        if start >= len(val):
                            changed = True
                            return ""
                        if len(parts) > 1:
                            length = int(parts[1])
                            if length < 0:
                                length = max(0, len(val) - start + length)
                            length = min(length, len(val) - start)
                            changed = True
                            return val[start:start + length]
                        else:
                            changed = True
                            return val[start:]
                    except (ValueError, IndexError):
                        pass

                return match.group(0)

            new_content = re.sub(r"%([a-zA-Z0-9_#$@]+):~(-?\d+(?:,-?\d+)?|\d+)%", slice_replacer, current, flags=re.IGNORECASE)
            if new_content != current:
                changed = True
                current = new_content

            if not changed:
                break

        return current

    @staticmethod
    def _normalize_escapes(content: str) -> str:
        """
        Removes command-line obfuscation tricks:
        1. Caret escapes in alphanumeric words (^c^m^d -> cmd)
        2. Empty double quote insertions (c""m""d -> cmd)
        3. Dangling unresolved noise variables (%tag_1234%)
        """
        lines = content.splitlines()
        normalized_lines = []

        for line in lines:
            trimmed = line.strip()

            # Skip comments
            if trimmed.startswith("::") or trimmed.lower().startswith("rem "):
                normalized_lines.append(line)
                continue

            cur = line

            # 1. Remove unresolved junk variables that match %tag_digits%
            cur = re.sub(r"%[a-zA-Z0-9_]+_\d{3,6}%", "", cur)

            # 2. Normalize Caret escapes (^a -> a) when outside literal quotes
            # Example: ^e^c^h^o -> echo
            def unescape_carets(text):
                # Replace carets followed by letters, digits, or spaces
                return re.sub(r"\^([a-zA-Z0-9_\-/\\])", r"\1", text)

            cur = unescape_carets(cur)

            # 3. Normalize empty quotes inserted in command names: c""m""d -> cmd
            cur = re.sub(r'(?<=[a-zA-Z0-9])""(?=[a-zA-Z0-9])', "", cur)

            normalized_lines.append(cur)

        return "\r\n".join(normalized_lines)

    @staticmethod
    def _strip_obfuscation_artifacts(content: str) -> str:
        """
        Removes obfuscator banners, guard variables, and character lookup table definitions.
        """
        lines = content.splitlines()
        clean_lines = []

        for line in lines:
            trimmed = line.strip()
            if not trimmed:
                clean_lines.append("")
                continue

            lower = trimmed.lower()

            # Remove TigerVM/Obfuscator Header banners
            if "script protection pipeline" in lower or "polymorphic script encryption matrix" in lower or "in-memory stream loader" in lower:
                continue
            if trimmed.startswith(":: ====") or "signature:" in lower or "strict guard" in lower:
                continue
            if re.match(r"^::[a-zA-Z0-9_]+_cksum_\d+", trimmed):
                continue

            # Remove guard variables: set "tag_guard_123=0x..." >nul 2>&1
            if re.match(r'^set\s+"?[a-zA-Z0-9_]+_guard_\d+=0x[0-9a-fA-F]+"?.*$', trimmed, re.IGNORECASE):
                continue

            # Remove character lookup tables: set "tag_a=abcdef..." or set "tag=abcdef..."
            # Heuristic: variable containing long pool of unique ASCII characters
            table_match = re.match(r'^set\s+"?([a-zA-Z0-9_]+)=([^"]+)"?$', trimmed, re.IGNORECASE)
            if table_match:
                val = table_match.group(2)
                if len(val) >= 30 and len(set(val)) >= 25:
                    continue

            # Remove entry jump labels generated by obfuscator: :tag_entry_1234
            if re.match(r"^:[a-zA-Z0-9_]+_entry_\d+$", trimmed, re.IGNORECASE):
                continue

            # Remove setlocal DisableDelayedExpansion inserted solely by obfuscators if directly after echo off
            if lower == "setlocal disabledelayedexpansion" and len(clean_lines) <= 2:
                continue

            clean_lines.append(line)

        # Collapse excess empty lines
        result = []
        consecutive_empty = 0
        for l in clean_lines:
            if not l.strip():
                consecutive_empty += 1
                if consecutive_empty <= 1:
                    result.append("")
            else:
                consecutive_empty = 0
                result.append(l)

        return "\r\n".join(result).strip()

    @staticmethod
    def decompile_bytecode_to_batch(instructions: List[dict]) -> str:
        """
        Reconstructs formatted Batch Script from TigerVM Bytecode AST.
        """
        lines = []
        indent = 0

        def get_indent():
            return "    " * indent

        for inst in instructions:
            op = inst.get("op", 0)
            a1 = inst.get("arg1", "")
            a2 = inst.get("arg2", "")
            a3 = inst.get("arg3", "")
            a4 = inst.get("arg4", "")
            f1 = inst.get("flag1", False)
            f2 = inst.get("flag2", False)
            iv = inst.get("int_val", 0)

            if op == TigerVMCompiler.OP_ECHO_TOGGLE:
                lines.append(get_indent() + ("@echo on" if f1 else "@echo off"))
            elif op == TigerVMCompiler.OP_ECHO:
                lines.append(get_indent() + (f"echo {a1}" if a1 else "echo."))
            elif op == TigerVMCompiler.OP_SET_VAR:
                lines.append(get_indent() + f'set "{a1}={a2}"')
            elif op == TigerVMCompiler.OP_SET_MATH:
                lines.append(get_indent() + f'set /a {a1}={a2}')
            elif op == TigerVMCompiler.OP_SET_PROMPT:
                lines.append(get_indent() + f'set /p {a1}="{a2}"')
            elif op == TigerVMCompiler.OP_LABEL:
                lines.append(f"\n:{a1}")
            elif op == TigerVMCompiler.OP_GOTO:
                lines.append(get_indent() + f"goto {a1}")
            elif op == TigerVMCompiler.OP_CALL_SUB:
                lines.append(get_indent() + (f"call :{a1} {a2}" if a2 else f"call :{a1}"))
            elif op == TigerVMCompiler.OP_IF_CMP:
                neg = "not " if f1 else ""
                cas = "/i " if f2 else ""
                lines.append(get_indent() + f"if {cas}{neg}{a1} {a4} {a2} ({a3})")
            elif op == TigerVMCompiler.OP_IF_EXIST:
                neg = "not " if f1 else ""
                lines.append(get_indent() + f"if {neg}exist {a1} ({a2})")
            elif op == TigerVMCompiler.OP_IF_DEFINED:
                neg = "not " if f1 else ""
                lines.append(get_indent() + f"if {neg}defined {a1} ({a2})")
            elif op == TigerVMCompiler.OP_IF_ERRORLEVEL:
                neg = "not " if f1 else ""
                lines.append(get_indent() + f"if {neg}errorlevel {iv} ({a2})")
            elif op == TigerVMCompiler.OP_FOR_NUMERIC:
                parts = (a4 or "").split("|", 1)
                end = parts[0] if len(parts) > 0 else "1"
                body = parts[1] if len(parts) > 1 else ""
                lines.append(get_indent() + f"for /L %%{a1} in ({a2},{a3},{end}) do (\n{get_indent()}    {body}\n{get_indent()})")
            elif op == TigerVMCompiler.OP_FOR_TOKENS:
                opts = f'"{a2}" ' if a2 else ""
                lines.append(get_indent() + f"for /F {opts}%%{a1} in ({a3}) do (\n{get_indent()}    {a4}\n{get_indent()})")
            elif op == TigerVMCompiler.OP_FOR_FILES:
                rec = "/R " if f1 else ""
                rpath = f"{a2} " if a2 and a2 != "." else ""
                lines.append(get_indent() + f"for {rec}{rpath}%%{a1} in ({a3}) do (\n{get_indent()}    {a4}\n{get_indent()})")
            elif op == TigerVMCompiler.OP_PAUSE:
                lines.append(get_indent() + "pause")
            elif op == TigerVMCompiler.OP_CLS:
                lines.append(get_indent() + "cls")
            elif op == TigerVMCompiler.OP_TITLE:
                lines.append(get_indent() + f"title {a1}")
            elif op == TigerVMCompiler.OP_COLOR:
                lines.append(get_indent() + f"color {a1}")
            elif op == TigerVMCompiler.OP_CD:
                lines.append(get_indent() + f'cd /d "{a1}"')
            elif op == TigerVMCompiler.OP_DELAY:
                lines.append(get_indent() + f"timeout /t {iv // 1000} >nul")
            elif op == TigerVMCompiler.OP_EXIT:
                lines.append(get_indent() + (f"exit /b {iv}" if iv != 0 else "exit /b 0"))
            elif op == TigerVMCompiler.OP_TRY_START:
                lines.append(get_indent() + "::@try")
            elif op == TigerVMCompiler.OP_CATCH:
                lines.append(get_indent() + f"::@catch {a1}")
            elif op == TigerVMCompiler.OP_END_TRY:
                lines.append(get_indent() + "::@end_try")
            elif op == TigerVMCompiler.OP_HUD_TABLE:
                lines.append(get_indent() + f"::@hud_table {a1}")
            elif op == TigerVMCompiler.OP_HUD_SPINNER:
                lines.append(get_indent() + f"::@hud_spinner {a1} \"{a2}\"")
            elif op == TigerVMCompiler.OP_VFS_LIST:
                lines.append(get_indent() + f"::@vfs_list {a1}")
            elif op == TigerVMCompiler.OP_REG_READ:
                lines.append(get_indent() + f"::@reg_read {a1} {a2} {a3} \"{a4}\"")
            elif op == TigerVMCompiler.OP_REG_WRITE:
                rw_parts = a4.split("|", 1) if a4 else ["", "SZ"]
                lines.append(get_indent() + f"::@reg_write {a1} {a2} {a3} \"{rw_parts[0]}\" {rw_parts[1] if len(rw_parts) > 1 else 'SZ'}")
            elif op == TigerVMCompiler.OP_MEM_ALLOC:
                lines.append(get_indent() + f"::@mem_alloc {a1} {a2}")
            elif op == TigerVMCompiler.OP_MEM_FREE:
                lines.append(get_indent() + f"::@mem_free {a1}")
            elif op == TigerVMCompiler.OP_MEM_WRITE:
                lines.append(get_indent() + f"::@mem_write {a1} \"{a2}\"")
            elif op == TigerVMCompiler.OP_MEM_READ:
                lines.append(get_indent() + f"::@mem_read {a1} {a2} {a3}")
            elif op == TigerVMCompiler.OP_SYS_INFO:
                lines.append(get_indent() + f"::@sys_info {a1} {a2}")
            elif op == TigerVMCompiler.OP_NET_PING:
                lines.append(get_indent() + f"::@net_ping {a1} {a2} {a3} {a4}")
            elif op == TigerVMCompiler.OP_VFS_UNZIP:
                lines.append(get_indent() + f"::@vfs_unzip \"{a1}\" \"{a2}\"")
            elif op in [TigerVMCompiler.OP_EXEC_DIRECT, TigerVMCompiler.OP_PIPE_STREAM]:
                if a1:
                    lines.append(get_indent() + a1)

        return "\r\n".join(lines).strip()

    @staticmethod
    def beautify_batch(script_content: str) -> str:
        """
        Beautifies Batch syntax, normalizes indentation, and fixes common spacing.
        """
        lines = script_content.splitlines()
        beautified = []
        indent = 0

        for line in lines:
            trimmed = line.strip()
            if not trimmed:
                beautified.append("")
                continue

            # Adjust indentation for closing parenthesis
            if trimmed.startswith(")"):
                indent = max(0, indent - 1)

            leading = "    " * indent

            # Labels shouldn't be indented
            if trimmed.startswith(":") and not trimmed.startswith("::"):
                beautified.append("\n" + trimmed)
                continue

            beautified.append(leading + trimmed)

            # Adjust indentation for opening parenthesis
            if trimmed.endswith("("):
                indent += 1

        # Clean trailing empty lines
        res = "\r\n".join(beautified).strip() + "\r\n"
        return res
