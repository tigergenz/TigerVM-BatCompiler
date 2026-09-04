"""
protector.py - TigerVM Batch Virtual Machine & Script Protection Engine (v5.0 Enterprise)
"""
import random
import string
import struct
import base64
import hashlib
import re
from typing import List, Dict, Tuple, Optional


class TigerVMCompiler:
    """
    TigerVM Bytecode Compiler:
    Converts Batch (.bat/.cmd) scripts into encrypted TigerVM Bytecode
    with dynamic randomized opcodes and Control Flow Flattening (CFF).
    """
    OP_NOP = 0
    OP_ECHO = 1
    OP_ECHO_TOGGLE = 2
    OP_SET_VAR = 3
    OP_SET_MATH = 4
    OP_SET_PROMPT = 5
    OP_GOTO = 6
    OP_LABEL = 7
    OP_IF_CMP = 8
    OP_IF_EXIST = 9
    OP_IF_DEFINED = 10
    OP_IF_ERRORLEVEL = 11
    OP_CALL_SUB = 12
    OP_RETURN = 13
    OP_PAUSE = 14
    OP_CLS = 15
    OP_TITLE = 16
    OP_COLOR = 17
    OP_CD = 18
    OP_DELAY = 19
    OP_EXEC_DIRECT = 20
    OP_PIPE_STREAM = 21
    OP_EXIT = 22
    OP_FOR_NUMERIC = 23
    OP_FOR_FILES = 24
    OP_FOR_TOKENS = 25
    OP_WINAPI = 26
    OP_THREAD_START = 27
    OP_THREAD_WAIT = 28
    OP_VFS_READ = 29
    OP_VFS_WRITE = 30
    OP_HUD_BANNER = 31
    OP_HUD_PROGRESS = 32
    OP_HUD_MATRIX = 33
    OP_MEM_UNHOOK = 34
    OP_GUI_MSGBOX = 35
    OP_GUI_INPUTBOX = 36
    OP_GUI_FILEDIALOG = 37
    OP_HTTP_GET = 38
    OP_HTTP_POST = 39
    OP_NOTIFY = 40
    OP_JSON_GET = 41
    OP_JSON_SET = 42
    OP_SQL_EXEC = 43
    OP_SQL_QUERY = 44
    OP_CLIP_GET = 45
    OP_CLIP_SET = 46
    OP_CRYPTO_AES = 47
    OP_CRYPTO_HASH = 48
    OP_TRY_START = 49
    OP_CATCH = 50
    OP_END_TRY = 51
    OP_HUD_TABLE = 52
    OP_HUD_SPINNER = 53
    OP_VFS_LIST = 54
    OP_REG_READ = 55
    OP_REG_WRITE = 56
    OP_MEM_ALLOC = 57
    OP_MEM_FREE = 58
    OP_MEM_WRITE = 59
    OP_MEM_READ = 60
    OP_SYS_INFO = 61
    OP_NET_PING = 62
    OP_VFS_UNZIP = 63
    OP_EVAL_CS = 64
    OP_PIPE_SERVER = 65
    OP_PIPE_CLIENT = 66
    OP_SHM_WRITE = 67
    OP_SHM_READ = 68
    OP_SVC_QUERY = 69
    OP_SVC_CONTROL = 70
    OP_SHELL_EXEC = 71

    OP_NAMES = {
        0: "NOP", 1: "ECHO", 2: "ECHOTOGGLE", 3: "SETVAR", 4: "SETMATH",
        5: "SETPROMPT", 6: "GOTO", 7: "LABEL", 8: "IFCMP", 9: "IFEXIST",
        10: "IFDEFINED", 11: "IFERRORLEVEL", 12: "CALLSUB", 13: "RETURN",
        14: "PAUSE", 15: "CLS", 16: "TITLE", 17: "COLOR", 18: "CD",
        19: "DELAY", 20: "EXECDIRECT", 21: "PIPESTREAM", 22: "EXIT",
        23: "FORNUMERIC", 24: "FORFILES", 25: "FORTOKENS",
        26: "WINAPI", 27: "THREADSTART", 28: "THREADWAIT",
        29: "VFSREAD", 30: "VFSWRITE", 31: "HUDBANNER",
        32: "HUDPROGRESS", 33: "HUDMATRIX", 34: "MEMUNHOOK",
        35: "GUIMSGBOX", 36: "GUIINPUTBOX", 37: "GUIFILEDIALOG",
        38: "HTTPGET", 39: "HTTPPOST", 40: "NOTIFY",
        41: "JSONGET", 42: "JSONSET", 43: "SQLEXEC", 44: "SQLQUERY",
        45: "CLIPGET", 46: "CLIPSET", 47: "CRYPTOAES", 48: "CRYPTOHASH",
        49: "TRYSTART", 50: "CATCH", 51: "ENDTRY", 52: "HUDTABLE",
        53: "HUDSPINNER", 54: "VFSLIST", 55: "REGREAD", 56: "REGWRITE",
        57: "MEMALLOC", 58: "MEMFREE", 59: "MEMWRITE", 60: "MEMREAD",
        61: "SYSINFO", 62: "NETPING", 63: "VFSUNZIP",
        64: "EVALCS", 65: "PIPESERVER", 66: "PIPECLIENT",
        67: "SHMWRITE", 68: "SHMREAD", 69: "SVCQUERY",
        70: "SVCCONTROL", 71: "SHELLEXEC"
    }

    @staticmethod
    def clean_body_string(body: str) -> str:
        if not body:
            return ""
        body = body.strip()
        while body.startswith("(") and body.endswith(")"):
            body = body[1:-1].strip()
        return body.strip("& \r\n\t")

    @staticmethod
    def parse_batch(script_content: str) -> List[dict]:
        """Parses batch lines into TigerVM Instruction descriptors"""
        raw_lines = script_content.splitlines()
        lines = []

        # Parenthesis block accumulator
        i = 0
        while i < len(raw_lines):
            cur = raw_lines[i]
            trimmed = cur.strip()
            if trimmed.lower().startswith("for ") or trimmed.lower().startswith("if "):
                open_count = cur.count("(") - cur.count(")")
                while open_count > 0 and i + 1 < len(raw_lines):
                    i += 1
                    nxt = raw_lines[i].strip()
                    cur += " & " + nxt
                    open_count += raw_lines[i].count("(") - raw_lines[i].count(")")
            lines.append(cur)
            i += 1

        instructions = []
        for raw_line in lines:
            trimmed = raw_line.strip()
            if not trimmed:
                continue

            # TigerVM Extended Directives (::@ or rem @ or @@)
            if trimmed.startswith("::@") or trimmed.lower().startswith("rem @") or trimmed.startswith("@@"):
                dir_line = trimmed[3:].strip() if trimmed.startswith("::@") else (trimmed[5:].strip() if trimmed.lower().startswith("rem @") else trimmed[2:].strip())
                
                # ::@winapi <dll> <func> [args...]
                if dir_line.lower().startswith("winapi ") or dir_line.lower().startswith("api "):
                    parts = dir_line.split(None, 2)
                    dll_name = parts[1] if len(parts) > 1 else ""
                    rest = parts[2] if len(parts) > 2 else ""
                    func_parts = rest.split(None, 1)
                    func_name = func_parts[0] if len(func_parts) > 0 else ""
                    api_args = func_parts[1] if len(func_parts) > 1 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_WINAPI,
                        "arg1": dll_name,
                        "arg2": func_name,
                        "arg3": api_args
                    })
                    continue

                # ::@thread <label> / ::@async <label>
                if dir_line.lower().startswith("thread ") or dir_line.lower().startswith("async "):
                    t_label = dir_line.split(None, 1)[1].strip().lstrip(":").lower()
                    instructions.append({"op": TigerVMCompiler.OP_THREAD_START, "arg1": t_label})
                    continue

                # ::@thread_wait / ::@sync
                if dir_line.lower() in ["thread_wait", "sync", "threadwait"]:
                    instructions.append({"op": TigerVMCompiler.OP_THREAD_WAIT})
                    continue

                # ::@vfs_read <filename> <varname>
                if dir_line.lower().startswith("vfs_read ") or dir_line.lower().startswith("vfsread "):
                    parts = dir_line.split(None, 2)
                    v_file = parts[1] if len(parts) > 1 else ""
                    v_dest = parts[2] if len(parts) > 2 else "VFS_OUT"
                    instructions.append({"op": TigerVMCompiler.OP_VFS_READ, "arg1": v_file, "arg2": v_dest})
                    continue

                # ::@vfs_write <filename> <content>
                if dir_line.lower().startswith("vfs_write ") or dir_line.lower().startswith("vfswrite "):
                    parts = dir_line.split(None, 2)
                    v_file = parts[1] if len(parts) > 1 else ""
                    v_content = parts[2] if len(parts) > 2 else ""
                    instructions.append({"op": TigerVMCompiler.OP_VFS_WRITE, "arg1": v_file, "arg2": v_content})
                    continue

                # ::@hud <title> | <subtitle>
                if dir_line.lower().startswith("hud ") or dir_line.lower().startswith("banner "):
                    h_text = dir_line.split(None, 1)[1].strip() if len(dir_line.split(None, 1)) > 1 else ""
                    parts = h_text.split("|", 1)
                    t_main = parts[0].strip()
                    t_sub = parts[1].strip() if len(parts) > 1 else ""
                    instructions.append({"op": TigerVMCompiler.OP_HUD_BANNER, "arg1": t_main, "arg2": t_sub})
                    continue

                # ::@progress <percent> <label>
                if dir_line.lower().startswith("progress "):
                    p_rest = dir_line.split(None, 1)[1].strip() if len(dir_line.split(None, 1)) > 1 else ""
                    parts = p_rest.split(None, 1)
                    pct = parts[0] if len(parts) > 0 else "50"
                    lbl = parts[1] if len(parts) > 1 else "Processing..."
                    instructions.append({"op": TigerVMCompiler.OP_HUD_PROGRESS, "arg1": pct, "arg2": lbl})
                    continue

                # ::@matrix [lines]
                if dir_line.lower().startswith("matrix"):
                    m_parts = dir_line.split()
                    lines_cnt = int(m_parts[1]) if len(m_parts) > 1 and m_parts[1].isdigit() else 25
                    instructions.append({"op": TigerVMCompiler.OP_HUD_MATRIX, "int_val": lines_cnt})
                    continue

                # ::@unhook
                if dir_line.lower() in ["unhook", "unhook_ntdll"]:
                    instructions.append({"op": TigerVMCompiler.OP_MEM_UNHOOK})
                    continue

                # ::@msgbox <title> | <message> [| <buttons: OK, YesNo, OKCancel, YesNoCancel>] [| <icon: Info, Warning, Error, Question>] [| <resultVar>]
                if dir_line.lower().startswith("msgbox ") or dir_line.lower().startswith("gui_msgbox ") or dir_line.lower().startswith("alert "):
                    p_text = dir_line.split(None, 1)[1].strip() if len(dir_line.split(None, 1)) > 1 else ""
                    parts = [p.strip().strip('"\'') for p in p_text.split("|")]
                    m_title = parts[0] if len(parts) > 0 else "TigerVM Notice"
                    m_body = parts[1] if len(parts) > 1 else ""
                    m_btn = parts[2] if len(parts) > 2 else "OK"
                    m_icon = parts[3] if len(parts) > 3 else "Info"
                    m_res = parts[4] if len(parts) > 4 else "MSGBOX_RESULT"
                    instructions.append({
                        "op": TigerVMCompiler.OP_GUI_MSGBOX,
                        "arg1": m_title,
                        "arg2": m_body,
                        "arg3": f"{m_btn}|{m_icon}",
                        "arg4": m_res
                    })
                    continue

                # ::@inputbox <varname> "Prompt message" ["Default text"] ["Title"]
                if dir_line.lower().startswith("inputbox ") or dir_line.lower().startswith("gui_input "):
                    p_rest = dir_line.split(None, 1)[1].strip() if len(dir_line.split(None, 1)) > 1 else ""
                    tokens = [m.group(0).strip('"\'') for m in re.finditer(r'"[^"]*"|[^\s]+', p_rest)]
                    var_name = tokens[0] if len(tokens) > 0 else "INPUT_RESULT"
                    prompt = tokens[1] if len(tokens) > 1 else "Enter input:"
                    default_txt = tokens[2] if len(tokens) > 2 else ""
                    title = tokens[3] if len(tokens) > 3 else "TigerVM Input"
                    instructions.append({
                        "op": TigerVMCompiler.OP_GUI_INPUTBOX,
                        "arg1": var_name,
                        "arg2": prompt,
                        "arg3": default_txt,
                        "arg4": title
                    })
                    continue

                # ::@filedialog <varname> ["Title"] ["Filter (*.txt)|*.txt"] ["open"|"save"]
                if dir_line.lower().startswith("filedialog ") or dir_line.lower().startswith("gui_file "):
                    p_rest = dir_line.split(None, 1)[1].strip() if len(dir_line.split(None, 1)) > 1 else ""
                    tokens = [m.group(0).strip('"\'') for m in re.finditer(r'"[^"]*"|[^\s]+', p_rest)]
                    var_name = tokens[0] if len(tokens) > 0 else "FILE_RESULT"
                    title = tokens[1] if len(tokens) > 1 else "Select File"
                    filt = tokens[2] if len(tokens) > 2 else "All Files (*.*)|*.*"
                    mode = tokens[3] if len(tokens) > 3 else "open"
                    instructions.append({
                        "op": TigerVMCompiler.OP_GUI_FILEDIALOG,
                        "arg1": var_name,
                        "arg2": title,
                        "arg3": filt,
                        "arg4": mode
                    })
                    continue

                # ::@http_get <varname> <url> [timeoutMs]
                if dir_line.lower().startswith("http_get ") or dir_line.lower().startswith("get "):
                    parts = dir_line.split(None, 3)
                    var_name = parts[1] if len(parts) > 1 else "HTTP_RESPONSE"
                    url = parts[2] if len(parts) > 2 else ""
                    timeout = int(parts[3]) if len(parts) > 3 and parts[3].isdigit() else 10000
                    instructions.append({
                        "op": TigerVMCompiler.OP_HTTP_GET,
                        "arg1": var_name,
                        "arg2": url,
                        "int_val": timeout
                    })
                    continue

                # ::@http_post <varname> <url> <payload> [contentType] [timeoutMs]
                if dir_line.lower().startswith("http_post ") or dir_line.lower().startswith("post "):
                    parts = dir_line.split(None, 4)
                    var_name = parts[1] if len(parts) > 1 else "HTTP_RESPONSE"
                    url = parts[2] if len(parts) > 2 else ""
                    payload = parts[3] if len(parts) > 3 else ""
                    c_type = parts[4] if len(parts) > 4 else "application/json"
                    instructions.append({
                        "op": TigerVMCompiler.OP_HTTP_POST,
                        "arg1": var_name,
                        "arg2": url,
                        "arg3": payload,
                        "arg4": c_type,
                        "int_val": 10000
                    })
                    continue

                # ::@notify <title> | <message> [| <timeoutSec>] [| <icon: Info, Warning, Error>]
                if dir_line.lower().startswith("notify ") or dir_line.lower().startswith("toast "):
                    p_text = dir_line.split(None, 1)[1].strip() if len(dir_line.split(None, 1)) > 1 else ""
                    parts = [p.strip().strip('"\'') for p in p_text.split("|")]
                    n_title = parts[0] if len(parts) > 0 else "TigerVM Notification"
                    n_msg = parts[1] if len(parts) > 1 else ""
                    n_sec = int(parts[2]) if len(parts) > 2 and parts[2].isdigit() else 5
                    n_icon = parts[3] if len(parts) > 3 else "Info"
                    instructions.append({
                        "op": TigerVMCompiler.OP_NOTIFY,
                        "arg1": n_title,
                        "arg2": n_msg,
                        "arg3": n_icon,
                        "int_val": n_sec
                    })
                    continue

                # ::@json_get <destVar> <jsonSrc> <jsonPath>
                if dir_line.lower().startswith("json_get ") or dir_line.lower().startswith("json "):
                    parts = dir_line.split(None, 3)
                    dest_var = parts[1] if len(parts) > 1 else "JSON_VAL"
                    json_src = parts[2] if len(parts) > 2 else "{}"
                    json_path = parts[3].strip('"\'') if len(parts) > 3 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_JSON_GET,
                        "arg1": dest_var,
                        "arg2": json_src,
                        "arg3": json_path
                    })
                    continue

                # ::@json_set <destVar> <jsonSrc> <jsonPath> <newVal>
                if dir_line.lower().startswith("json_set "):
                    parts = dir_line.split(None, 4)
                    dest_var = parts[1] if len(parts) > 1 else "JSON_VAL"
                    json_src = parts[2] if len(parts) > 2 else "{}"
                    json_path = parts[3].strip('"\'') if len(parts) > 3 else ""
                    new_val = parts[4].strip('"\'') if len(parts) > 4 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_JSON_SET,
                        "arg1": dest_var,
                        "arg2": json_src,
                        "arg3": json_path,
                        "arg4": new_val
                    })
                    continue

                # ::@sql_exec <query>
                if dir_line.lower().startswith("sql_exec ") or dir_line.lower().startswith("sql "):
                    query = dir_line.split(None, 1)[1].strip().strip('"') if len(dir_line.split(None, 1)) > 1 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_SQL_EXEC,
                        "arg1": query
                    })
                    continue

                # ::@sql_query <destVar> <query>
                if dir_line.lower().startswith("sql_query "):
                    parts = dir_line.split(None, 2)
                    dest_var = parts[1] if len(parts) > 1 else "SQL_RESULT"
                    query = parts[2].strip().strip('"') if len(parts) > 2 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_SQL_QUERY,
                        "arg1": dest_var,
                        "arg2": query
                    })
                    continue

                # ::@clip_get <destVar>
                if dir_line.lower().startswith("clip_get ") or dir_line.lower() == "clip_get":
                    parts = dir_line.split(None, 1)
                    dest_var = parts[1].strip() if len(parts) > 1 else "CLIP_TEXT"
                    instructions.append({
                        "op": TigerVMCompiler.OP_CLIP_GET,
                        "arg1": dest_var
                    })
                    continue

                # ::@clip_set <text>
                if dir_line.lower().startswith("clip_set "):
                    text = dir_line.split(None, 1)[1].strip().strip('"\'') if len(dir_line.split(None, 1)) > 1 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_CLIP_SET,
                        "arg1": text
                    })
                    continue

                # ::@crypto_encrypt / ::@aes_encrypt <destVar> <plainText> <password>
                if dir_line.lower().startswith("crypto_encrypt ") or dir_line.lower().startswith("aes_encrypt "):
                    parts = dir_line.split(None, 3)
                    dest_var = parts[1] if len(parts) > 1 else "CIPHER_TEXT"
                    plain_text = parts[2] if len(parts) > 2 else ""
                    password = parts[3].strip('"\'') if len(parts) > 3 else "TigerSecretKey"
                    instructions.append({
                        "op": TigerVMCompiler.OP_CRYPTO_AES,
                        "arg1": dest_var,
                        "arg2": plain_text,
                        "arg3": password,
                        "arg4": "ENC"
                    })
                    continue

                # ::@crypto_decrypt / ::@aes_decrypt <destVar> <cipherText> <password>
                if dir_line.lower().startswith("crypto_decrypt ") or dir_line.lower().startswith("aes_decrypt "):
                    parts = dir_line.split(None, 3)
                    dest_var = parts[1] if len(parts) > 1 else "PLAIN_TEXT"
                    cipher_text = parts[2] if len(parts) > 2 else ""
                    password = parts[3].strip('"\'') if len(parts) > 3 else "TigerSecretKey"
                    instructions.append({
                        "op": TigerVMCompiler.OP_CRYPTO_AES,
                        "arg1": dest_var,
                        "arg2": cipher_text,
                        "arg3": password,
                        "arg4": "DEC"
                    })
                    continue

                # ::@crypto_sha256 / ::@sha256 <destVar> <text>
                if dir_line.lower().startswith("crypto_sha256 ") or dir_line.lower().startswith("sha256 "):
                    parts = dir_line.split(None, 2)
                    dest_var = parts[1] if len(parts) > 1 else "HASH_VAL"
                    text = parts[2].strip().strip('"\'') if len(parts) > 2 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_CRYPTO_HASH,
                        "arg1": dest_var,
                        "arg2": text,
                        "arg3": "SHA256"
                    })
                    continue

                # ::@crypto_md5 / ::@md5 <destVar> <text>
                if dir_line.lower().startswith("crypto_md5 ") or dir_line.lower().startswith("md5 "):
                    parts = dir_line.split(None, 2)
                    dest_var = parts[1] if len(parts) > 1 else "HASH_VAL"
                    text = parts[2].strip().strip('"\'') if len(parts) > 2 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_CRYPTO_HASH,
                        "arg1": dest_var,
                        "arg2": text,
                        "arg3": "MD5"
                    })
                    continue

                # ::@b64_encode <destVar> <text>
                if dir_line.lower().startswith("b64_encode ") or dir_line.lower().startswith("base64_encode "):
                    parts = dir_line.split(None, 2)
                    dest_var = parts[1] if len(parts) > 1 else "B64_VAL"
                    text = parts[2].strip().strip('"\'') if len(parts) > 2 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_CRYPTO_HASH,
                        "arg1": dest_var,
                        "arg2": text,
                        "arg3": "B64_ENC"
                    })
                    continue

                # ::@b64_decode <destVar> <b64>
                if dir_line.lower().startswith("b64_decode ") or dir_line.lower().startswith("base64_decode "):
                    parts = dir_line.split(None, 2)
                    dest_var = parts[1] if len(parts) > 1 else "B64_VAL"
                    text = parts[2].strip().strip('"\'') if len(parts) > 2 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_CRYPTO_HASH,
                        "arg1": dest_var,
                        "arg2": text,
                        "arg3": "B64_DEC"
                    })
                    continue

                # ::@try
                if dir_line.lower() == "try" or dir_line.lower().startswith("try "):
                    instructions.append({
                        "op": TigerVMCompiler.OP_TRY_START
                    })
                    continue

                # ::@catch <errVar>
                if dir_line.lower().startswith("catch ") or dir_line.lower() == "catch":
                    parts = dir_line.split(None, 1)
                    err_var = parts[1].strip() if len(parts) > 1 else "ERROR_MSG"
                    instructions.append({
                        "op": TigerVMCompiler.OP_CATCH,
                        "arg1": err_var
                    })
                    continue

                # ::@end_try / ::@endtry / ::@finally
                if dir_line.lower() in ("end_try", "endtry", "finally"):
                    instructions.append({
                        "op": TigerVMCompiler.OP_END_TRY
                    })
                    continue

                # ::@hud_table <headers> | <row1> | <row2> ...
                if dir_line.lower().startswith("hud_table ") or dir_line.lower().startswith("table "):
                    t_data = dir_line.split(None, 1)[1].strip() if len(dir_line.split(None, 1)) > 1 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_HUD_TABLE,
                        "arg1": t_data
                    })
                    continue

                # ::@hud_spinner <durationMs> <label>
                if dir_line.lower().startswith("hud_spinner ") or dir_line.lower().startswith("spinner "):
                    parts = dir_line.split(None, 2)
                    s_ms = parts[1].strip() if len(parts) > 1 else "1000"
                    s_lbl = parts[2].strip().strip('"\'') if len(parts) > 2 else "Processing..."
                    instructions.append({
                        "op": TigerVMCompiler.OP_HUD_SPINNER,
                        "arg1": s_ms,
                        "arg2": s_lbl
                    })
                    continue

                # ::@vfs_list <destVar>
                if dir_line.lower().startswith("vfs_list ") or dir_line.lower() == "vfs_list":
                    parts = dir_line.split(None, 1)
                    dest_var = parts[1].strip() if len(parts) > 1 else "VFS_LIST"
                    instructions.append({
                        "op": TigerVMCompiler.OP_VFS_LIST,
                        "arg1": dest_var
                    })
                    continue

                # ::@reg_read <destVar> <hive> <path> <name>
                if dir_line.lower().startswith("reg_read ") or dir_line.lower().startswith("regread "):
                    parts = dir_line.split(None, 4)
                    dest_var = parts[1] if len(parts) > 1 else "REG_VAL"
                    hive = parts[2] if len(parts) > 2 else "HKCU"
                    k_path = parts[3] if len(parts) > 3 else ""
                    v_name = parts[4].strip('"\'') if len(parts) > 4 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_REG_READ,
                        "arg1": dest_var,
                        "arg2": hive,
                        "arg3": k_path,
                        "arg4": v_name
                    })
                    continue

                # ::@reg_write <hive> <path> <name> <val> [type]
                if dir_line.lower().startswith("reg_write ") or dir_line.lower().startswith("regwrite "):
                    parts = dir_line.split(None, 5)
                    hive = parts[1] if len(parts) > 1 else "HKCU"
                    k_path = parts[2] if len(parts) > 2 else ""
                    v_name = parts[3] if len(parts) > 3 else ""
                    v_data = parts[4] if len(parts) > 4 else ""
                    v_type = parts[5].strip('"\'') if len(parts) > 5 else "SZ"
                    instructions.append({
                        "op": TigerVMCompiler.OP_REG_WRITE,
                        "arg1": hive,
                        "arg2": k_path,
                        "arg3": v_name,
                        "arg4": f"{v_data}|{v_type}"
                    })
                    continue

                # ::@mem_alloc <destVar> <size>
                if dir_line.lower().startswith("mem_alloc ") or dir_line.lower().startswith("memalloc "):
                    parts = dir_line.split(None, 2)
                    dest_var = parts[1] if len(parts) > 1 else "PTR_VAL"
                    m_size = parts[2] if len(parts) > 2 else "1024"
                    instructions.append({
                        "op": TigerVMCompiler.OP_MEM_ALLOC,
                        "arg1": dest_var,
                        "arg2": m_size
                    })
                    continue

                # ::@mem_free <ptrVar>
                if dir_line.lower().startswith("mem_free ") or dir_line.lower().startswith("memfree "):
                    parts = dir_line.split(None, 1)
                    ptr_var = parts[1] if len(parts) > 1 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_MEM_FREE,
                        "arg1": ptr_var
                    })
                    continue

                # ::@mem_write <ptrVar> <text>
                if dir_line.lower().startswith("mem_write ") or dir_line.lower().startswith("memwrite "):
                    parts = dir_line.split(None, 2)
                    ptr_var = parts[1] if len(parts) > 1 else ""
                    m_txt = parts[2] if len(parts) > 2 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_MEM_WRITE,
                        "arg1": ptr_var,
                        "arg2": m_txt
                    })
                    continue

                # ::@mem_read <destVar> <ptrVar> [len]
                if dir_line.lower().startswith("mem_read ") or dir_line.lower().startswith("memread "):
                    parts = dir_line.split(None, 3)
                    dest_var = parts[1] if len(parts) > 1 else "MEM_TEXT"
                    ptr_var = parts[2] if len(parts) > 2 else ""
                    m_len = parts[3] if len(parts) > 3 else "256"
                    instructions.append({
                        "op": TigerVMCompiler.OP_MEM_READ,
                        "arg1": dest_var,
                        "arg2": ptr_var,
                        "arg3": m_len
                    })
                    continue

                # ::@sys_info <destVar> <prop>
                if dir_line.lower().startswith("sys_info ") or dir_line.lower().startswith("sysinfo "):
                    parts = dir_line.split(None, 2)
                    dest_var = parts[1] if len(parts) > 1 else "SYS_INFO"
                    prop = parts[2].strip().strip('"\'') if len(parts) > 2 else "CPU_COUNT"
                    instructions.append({
                        "op": TigerVMCompiler.OP_SYS_INFO,
                        "arg1": dest_var,
                        "arg2": prop
                    })
                    continue

                # ::@net_ping <destVar> <host> <port> [timeout]
                if dir_line.lower().startswith("net_ping ") or dir_line.lower().startswith("netping ") or dir_line.lower().startswith("net_port "):
                    parts = dir_line.split(None, 4)
                    dest_var = parts[1] if len(parts) > 1 else "PING_RESULT"
                    host = parts[2] if len(parts) > 2 else "127.0.0.1"
                    port = parts[3] if len(parts) > 3 else "80"
                    t_out = parts[4] if len(parts) > 4 else "2000"
                    instructions.append({
                        "op": TigerVMCompiler.OP_NET_PING,
                        "arg1": dest_var,
                        "arg2": host,
                        "arg3": port,
                        "arg4": t_out
                    })
                    continue

                # ::@vfs_unzip <zipFile> [vfsPrefix]
                if dir_line.lower().startswith("vfs_unzip ") or dir_line.lower().startswith("vfsunzip "):
                    parts = dir_line.split(None, 2)
                    z_src = parts[1] if len(parts) > 1 else ""
                    v_pfx = parts[2] if len(parts) > 2 else "VFS:\\"
                    instructions.append({
                        "op": TigerVMCompiler.OP_VFS_UNZIP,
                        "arg1": z_src,
                        "arg2": v_pfx
                    })
                    continue

                # ::@eval_cs <destVar> "<code>"
                if dir_line.lower().startswith("eval_cs ") or dir_line.lower().startswith("evalcs "):
                    rest = dir_line[dir_line.index(" ") + 1:].strip()
                    dest_var = "EVAL_RESULT"
                    code = ""
                    if '"' in rest:
                        q1 = rest.index('"')
                        q2 = rest.rindex('"')
                        dest_var = rest[:q1].strip() or "EVAL_RESULT"
                        code = rest[q1 + 1:q2]
                    else:
                        parts = rest.split(None, 1)
                        dest_var = parts[0] if parts else "EVAL_RESULT"
                        code = parts[1] if len(parts) > 1 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_EVAL_CS,
                        "arg1": dest_var,
                        "arg2": code
                    })
                    continue

                # ::@pipe_server <destVar> "<pipeName>" [timeoutMs]
                if dir_line.lower().startswith("pipe_server ") or dir_line.lower().startswith("pipeserver "):
                    parts = dir_line.split(None, 3)
                    dest_var = parts[1] if len(parts) > 1 else "PIPE_DATA"
                    p_name = parts[2].strip().strip('"\'') if len(parts) > 2 else "TigerPipe"
                    t_out = parts[3] if len(parts) > 3 else "5000"
                    instructions.append({
                        "op": TigerVMCompiler.OP_PIPE_SERVER,
                        "arg1": dest_var,
                        "arg2": p_name,
                        "arg3": t_out
                    })
                    continue

                # ::@pipe_client "<pipeName>" "<message>" [timeoutMs]
                if dir_line.lower().startswith("pipe_client ") or dir_line.lower().startswith("pipeclient ") or dir_line.lower().startswith("pipe_send "):
                    rest = dir_line[dir_line.index(" ") + 1:].strip()
                    p_name = "TigerPipe"
                    p_msg = ""
                    t_out = "3000"
                    if '"' in rest:
                        q1 = rest.index('"')
                        q2 = rest.find('"', q1 + 1)
                        if q2 != -1:
                            p_name = rest[q1 + 1:q2]
                            remainder = rest[q2 + 1:].strip()
                            if '"' in remainder:
                                rq1 = remainder.index('"')
                                rq2 = remainder.rindex('"')
                                p_msg = remainder[rq1 + 1:rq2]
                                after = remainder[rq2 + 1:].strip()
                                if after:
                                    t_out = after
                            else:
                                rparts = remainder.split(None, 1)
                                p_msg = rparts[0] if rparts else ""
                                if len(rparts) > 1:
                                    t_out = rparts[1]
                        else:
                            parts = rest.split(None, 2)
                            p_name = parts[0].strip('"\'') if parts else "TigerPipe"
                            p_msg = parts[1].strip('"\'') if len(parts) > 1 else ""
                            t_out = parts[2] if len(parts) > 2 else "3000"
                    else:
                        parts = rest.split(None, 2)
                        p_name = parts[0] if parts else "TigerPipe"
                        p_msg = parts[1] if len(parts) > 1 else ""
                        t_out = parts[2] if len(parts) > 2 else "3000"
                    instructions.append({
                        "op": TigerVMCompiler.OP_PIPE_CLIENT,
                        "arg1": p_name,
                        "arg2": p_msg,
                        "arg3": t_out
                    })
                    continue

                # ::@shm_write "<mapName>" "<data>"
                if dir_line.lower().startswith("shm_write ") or dir_line.lower().startswith("shmwrite "):
                    rest = dir_line[dir_line.index(" ") + 1:].strip()
                    m_name = "TigerShm"
                    s_data = ""
                    if '"' in rest:
                        q1 = rest.index('"')
                        q2 = rest.find('"', q1 + 1)
                        if q2 != -1:
                            m_name = rest[q1 + 1:q2]
                            s_data = rest[q2 + 1:].strip().strip('"\'')
                        else:
                            parts = rest.split(None, 1)
                            m_name = parts[0].strip('"\'')
                            s_data = parts[1].strip('"\'') if len(parts) > 1 else ""
                    else:
                        parts = rest.split(None, 1)
                        m_name = parts[0] if parts else "TigerShm"
                        s_data = parts[1] if len(parts) > 1 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_SHM_WRITE,
                        "arg1": m_name,
                        "arg2": s_data
                    })
                    continue

                # ::@shm_read <destVar> "<mapName>" [maxBytes]
                if dir_line.lower().startswith("shm_read ") or dir_line.lower().startswith("shmread "):
                    parts = dir_line.split(None, 3)
                    dest_var = parts[1] if len(parts) > 1 else "SHM_DATA"
                    m_name = parts[2].strip().strip('"\'') if len(parts) > 2 else "TigerShm"
                    m_bytes = parts[3] if len(parts) > 3 else "4096"
                    instructions.append({
                        "op": TigerVMCompiler.OP_SHM_READ,
                        "arg1": dest_var,
                        "arg2": m_name,
                        "arg3": m_bytes
                    })
                    continue

                # ::@svc_query <destVar> "<serviceName>"
                if dir_line.lower().startswith("svc_query ") or dir_line.lower().startswith("svcquery "):
                    parts = dir_line.split(None, 2)
                    dest_var = parts[1] if len(parts) > 1 else "SVC_STATUS"
                    s_name = parts[2].strip().strip('"\'') if len(parts) > 2 else ""
                    instructions.append({
                        "op": TigerVMCompiler.OP_SVC_QUERY,
                        "arg1": dest_var,
                        "arg2": s_name
                    })
                    continue

                # ::@svc_control <destVar> "<serviceName>" "<action>"
                if dir_line.lower().startswith("svc_control ") or dir_line.lower().startswith("svccontrol "):
                    parts = dir_line.split(None, 3)
                    dest_var = parts[1] if len(parts) > 1 else "SVC_RESULT"
                    s_name = parts[2].strip().strip('"\'') if len(parts) > 2 else ""
                    s_act = parts[3].strip().strip('"\'') if len(parts) > 3 else "QUERY"
                    instructions.append({
                        "op": TigerVMCompiler.OP_SVC_CONTROL,
                        "arg1": dest_var,
                        "arg2": s_name,
                        "arg3": s_act
                    })
                    continue

                # ::@shell_exec <destVar> "<shellcode>" [timeout]
                if dir_line.lower().startswith("shell_exec ") or dir_line.lower().startswith("shellexec ") or dir_line.lower().startswith("shellcode "):
                    rest = dir_line[dir_line.index(" ") + 1:].strip()
                    dest_var = "SHELL_RESULT"
                    s_code = ""
                    t_out = "5000"
                    if '"' in rest:
                        q1 = rest.index('"')
                        q2 = rest.rindex('"')
                        dest_var = rest[:q1].strip() or "SHELL_RESULT"
                        s_code = rest[q1 + 1:q2]
                        after = rest[q2 + 1:].strip()
                        if after:
                            t_out = after
                    else:
                        parts = rest.split(None, 2)
                        dest_var = parts[0] if parts else "SHELL_RESULT"
                        s_code = parts[1] if len(parts) > 1 else ""
                        t_out = parts[2] if len(parts) > 2 else "5000"
                    instructions.append({
                        "op": TigerVMCompiler.OP_SHELL_EXEC,
                        "arg1": dest_var,
                        "arg2": s_code,
                        "arg3": t_out
                    })
                    continue

            if trimmed.startswith("::") or trimmed.lower().startswith("rem ") or trimmed.lower() == "rem":
                continue

            # Echo handling
            if trimmed.lower().startswith("@echo") or trimmed.lower().startswith("echo"):
                echo_cmd = trimmed[1:].strip() if trimmed.startswith("@") else trimmed
                lower_echo = echo_cmd.lower()
                if lower_echo == "echo off":
                    instructions.append({"op": TigerVMCompiler.OP_ECHO_TOGGLE, "flag1": False})
                    continue
                if lower_echo == "echo on":
                    instructions.append({"op": TigerVMCompiler.OP_ECHO_TOGGLE, "flag1": True})
                    continue
                if lower_echo in ["echo.", "echo/"]:
                    instructions.append({"op": TigerVMCompiler.OP_ECHO, "arg1": ""})
                    continue
                if lower_echo.startswith("echo "):
                    instructions.append({"op": TigerVMCompiler.OP_ECHO, "arg1": echo_cmd[5:]})
                    continue
                if lower_echo == "echo":
                    instructions.append({"op": TigerVMCompiler.OP_ECHO, "arg1": ""})
                    continue

            work = trimmed[1:].strip() if trimmed.startswith("@") else trimmed

            # FOR Loops
            if work.lower().startswith("for "):
                if TigerVMCompiler._parse_for(work, instructions):
                    continue

            # Label
            if work.startswith(":") and not work.startswith("::"):
                instructions.append({"op": TigerVMCompiler.OP_LABEL, "arg1": work[1:].strip().lower()})
                continue

            # Goto
            if work.lower().startswith("goto "):
                instructions.append({"op": TigerVMCompiler.OP_GOTO, "arg1": work[5:].strip().lstrip(':').lower()})
                continue

            # Call :label [args]
            if work.lower().startswith("call :"):
                sub_rest = work[6:].strip()
                sub_parts = sub_rest.split(None, 1)
                target_sub = sub_parts[0].strip().lower()
                sub_arg = sub_parts[1].strip() if len(sub_parts) > 1 else ""
                instructions.append({"op": TigerVMCompiler.OP_CALL_SUB, "arg1": target_sub, "arg2": sub_arg})
                continue

            # Pause
            if work.lower() == "pause" or work.lower().startswith("pause "):
                instructions.append({"op": TigerVMCompiler.OP_PAUSE})
                continue

            # Cls
            if work.lower() == "cls":
                instructions.append({"op": TigerVMCompiler.OP_CLS})
                continue

            # Title
            if work.lower().startswith("title "):
                instructions.append({"op": TigerVMCompiler.OP_TITLE, "arg1": work[6:].strip()})
                continue

            # Color
            if work.lower().startswith("color "):
                instructions.append({"op": TigerVMCompiler.OP_COLOR, "arg1": work[6:].strip()})
                continue

            # CD / CHDIR
            if work.lower().startswith("cd ") or work.lower().startswith("chdir "):
                parts = work.split(None, 1)
                p_arg = parts[1].strip().strip('"\'') if len(parts) > 1 else ""
                if p_arg.lower().startswith("/d "):
                    p_arg = p_arg[3:].strip().strip('"\'')
                instructions.append({"op": TigerVMCompiler.OP_CD, "arg1": p_arg})
                continue

            # Timeout
            if work.lower().startswith("timeout "):
                m = re.search(r"timeout\s+(?:/t\s+)?(\d+)", work, re.IGNORECASE)
                sec = int(m.group(1)) if m else 1
                instructions.append({"op": TigerVMCompiler.OP_DELAY, "int_val": sec * 1000})
                continue

            # Exit
            if work.lower().startswith("exit"):
                m = re.search(r"exit(?:\s+/b)?(?:\s+(\d+))?", work, re.IGNORECASE)
                code = int(m.group(1)) if (m and m.group(1)) else 0
                instructions.append({"op": TigerVMCompiler.OP_EXIT, "int_val": code})
                continue

            # Set /a
            if work.lower().startswith("set /a "):
                expr = work[7:].strip().strip('"')
                if "=" in expr:
                    vname, math_expr = expr.split("=", 1)
                    instructions.append({"op": TigerVMCompiler.OP_SET_MATH, "arg1": vname.strip(), "arg2": math_expr.strip()})
                    continue

            # Set /p
            if work.lower().startswith("set /p "):
                expr = work[7:].strip().strip('"')
                if "=" in expr:
                    vname, prompt_expr = expr.split("=", 1)
                    instructions.append({"op": TigerVMCompiler.OP_SET_PROMPT, "arg1": vname.strip(), "arg2": prompt_expr.strip()})
                    continue

            # Standard Set
            if work.lower().startswith("set "):
                expr = work[4:].strip().strip('"')
                if "=" in expr:
                    vname, val_expr = expr.split("=", 1)
                    instructions.append({"op": TigerVMCompiler.OP_SET_VAR, "arg1": vname.strip(), "arg2": val_expr})
                    continue

            # IF statements
            if work.lower().startswith("if "):
                if TigerVMCompiler._parse_if(work, instructions):
                    continue

            # Piped / Redirected or External Direct
            if any(ch in work for ch in ["|", ">", "<", "&"]):
                instructions.append({"op": TigerVMCompiler.OP_PIPE_STREAM, "arg1": work})
            else:
                instructions.append({"op": TigerVMCompiler.OP_EXEC_DIRECT, "arg1": work})

        return instructions

    @staticmethod
    def _parse_for(line: str, instructions: list) -> bool:
        # FOR /L %var IN (start,step,end) DO (body)
        mL = re.search(r"for\s+/l\s+%+([a-zA-Z0-9_]+)\s+in\s*\(([^)]+)\)\s+do\s+(.+)", line, re.IGNORECASE | re.DOTALL)
        if mL:
            vname = mL.group(1)
            rng = mL.group(2).strip()
            body = TigerVMCompiler.clean_body_string(mL.group(3))
            parts = [p.strip() for p in re.split(r"[, ]+", rng) if p.strip()]
            start = parts[0] if len(parts) > 0 else "1"
            step = parts[1] if len(parts) > 1 else "1"
            end = parts[2] if len(parts) > 2 else "1"
            instructions.append({
                "op": TigerVMCompiler.OP_FOR_NUMERIC,
                "arg1": vname,
                "arg2": start,
                "arg3": step,
                "arg4": f"{end}|{body}"
            })
            return True

        # FOR /F ["options"] %var IN (source) DO (body)
        mF = re.search(r"for\s+/f\s*(?:\"([^\"]*)\")?\s+%+([a-zA-Z0-9_]+)\s+in\s*\(([^)]+)\)\s+do\s+(.+)", line, re.IGNORECASE | re.DOTALL)
        if mF:
            opts = mF.group(1) or ""
            vname = mF.group(2)
            source = mF.group(3).strip()
            body = TigerVMCompiler.clean_body_string(mF.group(4))
            instructions.append({
                "op": TigerVMCompiler.OP_FOR_TOKENS,
                "arg1": vname,
                "arg2": opts,
                "arg3": source,
                "arg4": body
            })
            return True

        # FOR /R [path] %var IN (set) DO (body)
        mR = re.search(r"for\s+/r\s*(?:([^\s%]+))?\s+%+([a-zA-Z0-9_]+)\s+in\s*\(([^)]+)\)\s+do\s+(.+)", line, re.IGNORECASE | re.DOTALL)
        if mR:
            rpath = mR.group(1) or "."
            vname = mR.group(2)
            pattern = mR.group(3).strip()
            body = TigerVMCompiler.clean_body_string(mR.group(4))
            instructions.append({
                "op": TigerVMCompiler.OP_FOR_FILES,
                "arg1": vname,
                "arg2": rpath,
                "arg3": pattern,
                "arg4": body,
                "flag1": True
            })
            return True

        # Standard FOR %var IN (set) DO (body)
        mStd = re.search(r"for\s+%+([a-zA-Z0-9_]+)\s+in\s*\(([^)]+)\)\s+do\s+(.+)", line, re.IGNORECASE | re.DOTALL)
        if mStd:
            vname = mStd.group(1)
            pattern = mStd.group(2).strip()
            body = TigerVMCompiler.clean_body_string(mStd.group(3))
            instructions.append({
                "op": TigerVMCompiler.OP_FOR_FILES,
                "arg1": vname,
                "arg2": ".",
                "arg3": pattern,
                "arg4": body,
                "flag1": False
            })
            return True

        return False

    @staticmethod
    def _parse_if(line: str, instructions: list) -> bool:
        s = line[3:].strip()
        ignore_case = False
        if s.lower().startswith("/i "):
            ignore_case = True
            s = s[3:].strip()

        negate = False
        if s.lower().startswith("not "):
            negate = True
            s = s[4:].strip()

        # IF EXIST
        if s.lower().startswith("exist "):
            rest = s[6:].strip()
            split_idx = TigerVMCompiler._find_action_split(rest)
            if split_idx != -1:
                path_expr = rest[:split_idx].strip().strip('"\'')
                action = TigerVMCompiler.clean_body_string(rest[split_idx:])
                instructions.append({"op": TigerVMCompiler.OP_IF_EXIST, "arg1": path_expr, "arg2": action, "flag1": negate})
                return True

        # IF DEFINED
        if s.lower().startswith("defined "):
            rest = s[8:].strip()
            split_idx = TigerVMCompiler._find_action_split(rest)
            if split_idx != -1:
                var_name = rest[:split_idx].strip().strip('"%')
                action = TigerVMCompiler.clean_body_string(rest[split_idx:])
                instructions.append({"op": TigerVMCompiler.OP_IF_DEFINED, "arg1": var_name, "arg2": action, "flag1": negate})
                return True

        # IF ERRORLEVEL
        if s.lower().startswith("errorlevel "):
            rest = s[11:].strip()
            split_idx = TigerVMCompiler._find_action_split(rest)
            if split_idx != -1:
                num_str = rest[:split_idx].strip()
                action = TigerVMCompiler.clean_body_string(rest[split_idx:])
                lvl = int(num_str) if num_str.isdigit() else 0
                instructions.append({"op": TigerVMCompiler.OP_IF_ERRORLEVEL, "int_val": lvl, "arg2": action, "flag1": negate})
                return True

        # Comparison ==
        if "==" in s:
            eq_pos = s.find("==")
            left = s[:eq_pos].strip()
            right_and_action = s[eq_pos + 2:].strip()
            split_idx = TigerVMCompiler._find_action_split(right_and_action)
            if split_idx != -1:
                right = right_and_action[:split_idx].strip()
                action = TigerVMCompiler.clean_body_string(right_and_action[split_idx:])
                instructions.append({
                    "op": TigerVMCompiler.OP_IF_CMP,
                    "arg1": left,
                    "arg2": right,
                    "arg3": action,
                    "arg4": "==",
                    "flag1": negate,
                    "flag2": ignore_case
                })
                return True

        # Other operators (EQU, NEQ, LSS, LEQ, GTR, GEQ)
        for op in [" EQU ", " NEQ ", " LSS ", " LEQ ", " GTR ", " GEQ "]:
            pos = s.upper().find(op)
            if pos != -1:
                left = s[:pos].strip()
                right_and_action = s[pos + len(op):].strip()
                split_idx = TigerVMCompiler._find_action_split(right_and_action)
                if split_idx != -1:
                    right = right_and_action[:split_idx].strip()
                    action = TigerVMCompiler.clean_body_string(right_and_action[split_idx:])
                    instructions.append({
                        "op": TigerVMCompiler.OP_IF_CMP,
                        "arg1": left,
                        "arg2": right,
                        "arg3": action,
                        "arg4": op.strip().upper(),
                        "flag1": negate,
                        "flag2": ignore_case
                    })
                    return True

        return False

    @staticmethod
    def _find_action_split(text: str) -> int:
        keywords = ["goto ", "call ", "echo ", "set ", "exit ", "cls", "pause", "("]
        earliest = -1
        for kw in keywords:
            idx = text.lower().find(kw)
            if idx != -1:
                if earliest == -1 or idx < earliest:
                    earliest = idx
        return earliest

    @staticmethod
    def optimize_ast(instructions: List[dict]) -> List[dict]:
        """
        TigerVM AST Optimizer Pass:
        1. Constant Folding on math expressions (OP_SET_MATH)
        2. Dead Code Elimination (unreachable instructions after unconditional GOTO/EXIT before next LABEL)
        3. Redundant NOP stripping
        """
        if not instructions:
            return instructions
            
        opt = []
        unreachable = False
        
        for inst in instructions:
            op = inst.get("op", 0)
            
            # If we hit a label, code becomes reachable again
            if op == TigerVMCompiler.OP_LABEL:
                unreachable = False
                opt.append(inst)
                continue
                
            if unreachable:
                # Dead code elimination - skip unreachable instruction
                continue
                
            # Constant Folding on Math expressions
            if op == TigerVMCompiler.OP_SET_MATH:
                expr = inst.get("arg2", "").strip()
                # Check if expression is pure constant arithmetic (digits, spaces, + - * / ^ & | % ( ))
                if re.match(r'^[0-9\s\+\-\*\/\^\%\&\|\(\)]+$', expr):
                    try:
                        # Evaluate constant arithmetic safely in Python
                        val = int(eval(expr))
                        inst["arg2"] = str(val)
                    except Exception:
                        pass
                opt.append(inst)
            elif op == TigerVMCompiler.OP_NOP:
                # Strip pure NOPs
                continue
            else:
                opt.append(inst)
                
            # If this was an unconditional GOTO or EXIT, mark subsequent instructions as unreachable
            if op in [TigerVMCompiler.OP_GOTO, TigerVMCompiler.OP_EXIT]:
                unreachable = True
                
        return opt

    @staticmethod
    def compile_bytecode(script_content: str, enable_cff: bool = True, optimize: bool = True) -> Tuple[bytes, Dict[int, int], bytes, str]:
        """
        Compiles script into encrypted TigerVM Bytecode, returning:
        (encrypted_bytecode, opcode_map, encryption_key, sha256_seal)
        """
        instructions = TigerVMCompiler.parse_batch(script_content)
        if optimize:
            instructions = TigerVMCompiler.optimize_ast(instructions)

        # Control Flow Flattening State assignment
        state_seed = random.randint(0x10000, 0x7FFFF)
        state_ids = [state_seed + (i * 31) + random.randint(1, 100) for i in range(len(instructions))]
        for i in range(len(instructions)):
            instructions[i]["state_id"] = state_ids[i]
            instructions[i]["next_state_id"] = state_ids[i + 1] if i + 1 < len(instructions) else 0xDEAD

        # Randomized Opcode Map
        all_ops = list(TigerVMCompiler.OP_NAMES.keys())
        shuffled_bytes = list(range(256))
        random.shuffle(shuffled_bytes)
        opcode_map = {op: shuffled_bytes[i] for i, op in enumerate(all_ops)}

        key = bytes(random.choices(range(1, 255), k=32))

        # Binary format: Magic 'TGZV' (4 bytes) + Count (4 bytes) + Instructions
        stream = bytearray(b"TGZV")
        stream.extend(struct.pack("<I", len(instructions)))

        def write_string(buf, s):
            encoded = s.encode("utf-8") if s else b""
            length = len(encoded)
            while length >= 0x80:
                buf.append((length | 0x80) & 0xFF)
                length >>= 7
            buf.append(length & 0xFF)
            buf.extend(encoded)

        for inst in instructions:
            raw_op = inst.get("op", 0)
            mapped_op = opcode_map.get(raw_op, raw_op)
            stream.append(mapped_op & 0xFF)
            write_string(stream, inst.get("arg1", ""))
            write_string(stream, inst.get("arg2", ""))
            write_string(stream, inst.get("arg3", ""))
            write_string(stream, inst.get("arg4", ""))
            stream.append(1 if inst.get("flag1", False) else 0)
            stream.append(1 if inst.get("flag2", False) else 0)
            stream.extend(struct.pack("<i", inst.get("int_val", 0)))
            stream.extend(struct.pack("<i", inst.get("state_id", 0)))
            stream.extend(struct.pack("<i", inst.get("next_state_id", 0)))

        # Encrypt with multi-byte XOR
        encrypted = bytearray()
        for i, b in enumerate(stream):
            encrypted.append(b ^ key[i % len(key)] ^ (i & 0xFF))

        # In-Memory Deflate Compression (RFC 1951 raw stream compatible with .NET DeflateStream)
        import zlib
        compressor = zlib.compressobj(level=9, method=zlib.DEFLATED, wbits=-15)
        compressed = compressor.compress(encrypted) + compressor.flush()

        sha256_seal = hashlib.sha256(compressed).hexdigest().lower()
        return bytes(compressed), opcode_map, key, sha256_seal

    @staticmethod
    def disassemble(script_content: str) -> None:
        """Prints a human-readable bytecode disassembly table."""
        instructions = TigerVMCompiler.parse_batch(script_content)
        print(f"\n[+] --- TigerVM Bytecode Disassembly ({len(instructions)} Instructions) ---")
        for i, inst in enumerate(instructions):
            op_name = TigerVMCompiler.OP_NAMES.get(inst.get("op", 0), "UNKNOWN").ljust(14)
            a1 = inst.get("arg1", "")
            a2 = inst.get("arg2", "")
            a3 = inst.get("arg3", "")
            a4 = inst.get("arg4", "")
            f1 = inst.get("flag1", False)
            f2 = inst.get("flag2", False)
            iv = inst.get("int_val", 0)
            print(f"  [{i:04d}]  {op_name} A1='{a1}' A2='{a2}' A3='{a3}' A4='{a4}' F1={f1} F2={f2} Iv={iv}")


class BatchObfuscator:
    """
    Obfuscates batch scripts for .bat -> .bat mode using security patterns:
    - Linear variable slicing (%tag:~x,1%)
    - Dynamic zero-disk in-memory PowerShell loader
    - Level 3 Polymorphic Chaos Matrix multi-table substitution
    """

    def __init__(self, signature: str = "tigergenz", seed=None):
        self.signature = signature or "tigergenz"
        if seed:
            random.seed(seed)

    def obfuscate_basic(self, script_content: str) -> str:
        """Level 1: Linear Variable Slicing with signature pattern."""
        safe_base = string.ascii_letters + string.digits + " ._-\\/:=,;*+?~#"
        chars_in_script = set(script_content)
        pool_list = list(set(c for c in safe_base if c in chars_in_script or c.isalnum()))
        random.shuffle(pool_list)
        char_pool = "".join(pool_list)
        pool_var = self.signature

        char_index_map = {}
        for idx, char in enumerate(char_pool):
            char_index_map[char] = f"%{pool_var}:~{idx},1%"

        obfuscated_lines = []
        obfuscated_lines.append(f":: [ {self.signature.upper()} SCRIPT PROTECTION PIPELINE ]")
        obfuscated_lines.append("@echo off")
        obfuscated_lines.append(f'set "{pool_var}={char_pool}"')

        for line in script_content.splitlines():
            trimmed = line.strip()
            if not trimmed:
                obfuscated_lines.append("")
                continue
            if trimmed.startswith(":") and not trimmed.startswith("::"):
                obfuscated_lines.append(line)
                continue

            obf_line = ""
            i = 0
            while i < len(line):
                if line[i] == "%":
                    if i + 1 < len(line) and (line[i+1].isdigit() or line[i+1] in "*~"):
                        end_idx = i + 2
                        while end_idx < len(line) and (line[end_idx].isalnum() or line[end_idx] in "dpnxsatz0123456789"):
                            end_idx += 1
                        obf_line += line[i:end_idx]
                        i = end_idx
                        continue

                    next_pct = line.find("%", i + 1)
                    if next_pct != -1 and (next_pct - i) < 30:
                        obf_line += line[i:next_pct + 1]
                        i = next_pct + 1
                        continue
                    else:
                        obf_line += "%"
                        i += 1
                        continue

                ch = line[i]
                if ch in ['"', "'", "&", "|", "<", ">", "^", "!", "(", ")"]:
                    obf_line += ch
                elif ch in char_index_map and random.random() < 0.85:
                    obf_line += char_index_map[ch]
                else:
                    obf_line += ch
                i += 1

            obfuscated_lines.append(obf_line)

        return "\r\n".join(obfuscated_lines)

    def obfuscate_advanced(self, script_content: str) -> str:
        """Level 2: In-Memory Stdin Stream Loader"""
        raw_bytes = script_content.encode("utf-8")
        b64_payload = base64.b64encode(raw_bytes).decode("ascii")

        var_data = f"{self.signature}_payload"
        chunk_size = 70
        chunks = [b64_payload[i:i + chunk_size] for i in range(0, len(b64_payload), chunk_size)]

        lines = [
            f":: [ {self.signature.upper()} IN-MEMORY STREAM LOADER ]",
            "@echo off",
            "setlocal EnableDelayedExpansion",
            f'set "{var_data}="',
        ]

        for chunk in chunks:
            lines.append(f'set "{var_data}=!{var_data}!{chunk}"')

        # Zero-disk stdin pipe execution in memory
        lines.append(
            'powershell -NoProfile -NonInteractive -ExecutionPolicy Bypass -Command '
            f'"$b=[System.Convert]::FromBase64String(\'!{var_data}!\');'
            '$s=[System.Text.Encoding]::UTF8.GetString($b);'
            '$psi=New-Object System.Diagnostics.ProcessStartInfo;'
            '$psi.FileName=\'cmd.exe\';$psi.Arguments=\'/q\';$psi.UseShellExecute=$false;$psi.RedirectStandardInput=$true;'
            '$p=[System.Diagnostics.Process]::Start($psi);$p.StandardInput.WriteLine($s);$p.StandardInput.Close();$p.WaitForExit();exit $p.ExitCode"'
        )
        lines.append("exit /b %ERRORLEVEL%")

        return "\r\n".join(lines)

    def obfuscate_insane(self, script_content: str) -> str:
        """Level 3: Polymorphic Chaos Matrix Multi-table Substitution & Noise Injection"""
        safe_base = string.ascii_letters + string.digits + " ._-\\/:=,;*+?~#"
        chars_in_script = set(script_content)
        pool_list = list(set(c for c in safe_base if c in chars_in_script or c.isalnum()))
        random.shuffle(pool_list)

        table_count = 4
        tables = {}
        for idx in range(table_count):
            sub_chars = list(pool_list)
            random.shuffle(sub_chars)
            var_name = f"{self.signature}_{string.ascii_lowercase[idx]}"
            tables[var_name] = "".join(sub_chars)

        char_lookups = {}
        for var_name, pool_str in tables.items():
            for idx, ch in enumerate(pool_str):
                if ch not in char_lookups:
                    char_lookups[ch] = []
                char_lookups[ch].append(f"%{var_name}:~{idx},1%")

        def make_junk():
            rnd = "".join(random.choices(string.ascii_lowercase + string.digits, k=4))
            return f"%{self.signature}_{rnd}%"

        obfuscated_lines = []
        obfuscated_lines.append(":: ====================================================================")
        obfuscated_lines.append("::  TIGERVM POLYMORPHIC SCRIPT ENCRYPTION MATRIX v5.0")
        obfuscated_lines.append(f"::  SIGNATURE: TGZ-0x{random.randint(0x100000, 0xFFFFFF):06X} // STRICT GUARD")
        obfuscated_lines.append(":: ====================================================================")
        obfuscated_lines.append("@echo off")
        obfuscated_lines.append("setlocal DisableDelayedExpansion")

        for _ in range(3):
            hex_fake = f"0x{random.randint(0x10000000, 0xFFFFFFFF):08X}"
            obfuscated_lines.append(f'set "{self.signature}_guard_{random.randint(100,999)}={hex_fake}" >nul 2>&1')

        for var_name, pool_str in tables.items():
            obfuscated_lines.append(f'set "{var_name}={pool_str}"')

        obfuscated_lines.append(f":{self.signature}_entry_{random.randint(1000, 9999)}")

        for line in script_content.splitlines():
            trimmed = line.strip()
            if not trimmed:
                if random.random() < 0.3:
                    obfuscated_lines.append(f'::{self.signature}_cksum_{random.randint(10000, 99999)}')
                else:
                    obfuscated_lines.append("")
                continue

            if trimmed.startswith(":") and not trimmed.startswith("::"):
                obfuscated_lines.append(line)
                continue

            obf_line = ""
            i = 0
            while i < len(line):
                if line[i] == "%":
                    if i + 1 < len(line) and (line[i+1].isdigit() or line[i+1] in "*~"):
                        end_idx = i + 2
                        while end_idx < len(line) and (line[end_idx].isalnum() or line[end_idx] in "dpnxsatz0123456789"):
                            end_idx += 1
                        obf_line += line[i:end_idx]
                        i = end_idx
                        continue

                    next_pct = line.find("%", i + 1)
                    if next_pct != -1 and (next_pct - i) < 30:
                        obf_line += line[i:next_pct + 1]
                        i = next_pct + 1
                        continue
                    else:
                        obf_line += "%"
                        i += 1
                        continue

                ch = line[i]
                if ch in ['"', "'", "&", "|", "<", ">", "^", "!", "(", ")"]:
                    obf_line += ch
                elif ch in char_lookups:
                    target_slice = random.choice(char_lookups[ch])
                    obf_line += target_slice
                    if random.random() < 0.45:
                        obf_line += make_junk()
                else:
                    obf_line += ch
                i += 1

            obfuscated_lines.append(obf_line)

        return "\r\n".join(obfuscated_lines)
