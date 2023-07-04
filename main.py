###############################################################
###       Whatsapp Desktop No-Store Installer/Updater       ###
###   https://github.com/redr00t/WhatsAppDesktop-NoStore    ###
###############################################################

from selenium import webdriver
import os, subprocess
from modules.driver_get_whatsapp import get_whatsapp_installable_file
from modules.whatsapp_install_powershell import whatsapp_install_in_powershell
from modules.shutdown_whatsapp import shutdown_whatsapp
from selenium import webdriver
from webdriver_manager.chrome import ChromeDriverManager
from time import sleep

def set_chrome_options():
    options = webdriver.ChromeOptions()
    options.add_argument("--headless")
    options.add_argument("--log-level=3")  # Silences error messages from Selenium console

    return options

# Create a new browser instance
driver = webdriver.Chrome(ChromeDriverManager().install(), options=set_chrome_options())
while True:
    get_whatsapp_installable_file(driver)

    if os.path.exists(os.path.join(os.path.dirname(os.path.abspath(__file__)), "whatsapp.msixbundle")):
        try:
            shutdown_whatsapp()
            whatsapp_install_in_powershell()
            break
        except subprocess.CalledProcessError as e:
            print(f'Error executing command: {e}')
            sleep(10)
        except OSError as e:
            print(f'Error removing file: {e}')
            sleep(10)
    else:
        print("Whatsapp package not found")
        sleep(10)