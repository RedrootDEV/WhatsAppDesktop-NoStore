import subprocess

def shutdown_whatsapp():
    try:
        subprocess.run(['taskkill', '/F', '/IM', 'WhatsApp.exe'], check=True, stderr=subprocess.DEVNULL)  # Silences taskkill errors
    
    except subprocess.CalledProcessError as e:
        if e.returncode != 128:
            raise
    