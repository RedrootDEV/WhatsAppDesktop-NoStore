import winreg
def find_powershell_path():
    try:
        # Abrir a chave de registro do PowerShell
        key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell")

        # Obter o valor do caminho do executável
        value, _ = winreg.QueryValueEx(key, "Path")

        return value
    
    except FileNotFoundError:
        # Chave de registro não encontrada
        return None