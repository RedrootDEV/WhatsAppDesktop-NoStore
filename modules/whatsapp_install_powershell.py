from modules.powershell_path import find_powershell_path
import os, subprocess, sys
def whatsapp_install_in_powershell():
    whatsapp_filename_path = os.path.join(os.getcwd(), "whatsapp.msixbundle")
    
    powershell_command = f'Add-AppxPackage .\whatsapp.msixbundle'
    if find_powershell_path == None:
        print("PowerShell not found")
        sys.exit()
    else:
        powershell_path = find_powershell_path()

    process = subprocess.Popen([powershell_path,
                powershell_command], 
                stdout=sys.stdout)
    process.communicate()
    #Remove the "whatsapp.msixbundle" file
    os.remove(whatsapp_filename_path)