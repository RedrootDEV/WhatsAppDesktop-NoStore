
from selenium.webdriver.common.by import By
from bs4 import BeautifulSoup
import time, requests, os

def get_whatsapp_installable_file(driver):
    try:
        # Visit the web page
        driver.get("https://store.rg-adguard.net/")

        # Wait for the page to fully load
        time.sleep(2)

        # Locate the input field and send the URL
        input_element = driver.find_element(By.ID, "url")
        input_element.clear()
        input_element.send_keys("https://apps.microsoft.com/store/detail/whatsapp/9NKSQGP7F2NH")

        # Locate the button and click it
        button = driver.find_element(By.XPATH, "//input[@type='button']")
        button.click()

        # Wait for the page to fully load
        time.sleep(10)

        # Get the page source
        page_source = driver.page_source

        # Parse the page source with BeautifulSoup
        soup = BeautifulSoup(page_source, 'html.parser')

        # Find the first occurrence of a .msixbundle link and download it
        for link in soup.find_all('a'):
            if '.msixbundle' in link.text:
                # File URL
                url = link['href']

                # If the file to download already exists, remove it
                filename = link.text
                if os.path.isfile(filename):
                    os.remove(filename)

                # Download the file
                response = requests.get(url, stream=True)
                with open(filename, 'wb') as fd:
                    for chunk in response.iter_content(chunk_size=1024):
                        fd.write(chunk)

                # If 'whatsapp.msixbundle' file already exists, remove it
                new_filename = "whatsapp.msixbundle"
                if os.path.isfile(new_filename):
                    os.remove(new_filename)

                # Rename the downloaded file to "whatsapp.msixbundle"
                os.rename(filename, new_filename)
                break
    finally:
        # Close the browser
        driver.quit()