# Automatic Download and Installation of WhatsApp Desktop

This repository contains a Python script that automates the process of downloading and installing the WhatsApp application from the website "https://store.rg-adguard.net/". The script uses the Selenium and BeautifulSoup libraries to interact with the web browser and parse the page source code.

## Prerequisites

Before running the script, make sure you have the following installed:

- Python 3.x: https://www.python.org/downloads/
- Access your prompt and go to the directory where you cloned the source. Then, run `pip install -r requirements.txt`
<br>Or, if needed:
- Selenium: `pip install selenium`
- BeautifulSoup: `pip install beautifulsoup4`
- Webdriver Manager: `pip install webdriver_manager`
- Requests: `pip install requests`

Also, make sure you have Google Chrome/Chromium installed, as the script uses the Chrome driver provided by Selenium.

## How to Use the Script

1. Clone or download this repository to your local machine.

2. Open a terminal and navigate to the repository directory.

3. Run the following command to execute the script: python main.py

The script will open a Chrome/Chromium browser window in "headless" mode (without a graphical interface) and start the automatic download and installation process of WhatsApp.

4. Wait for the script to complete. During execution, progress messages will be displayed in the console.

5. After the script finishes, the WhatsApp application will be installed on your system.

## How the Script Works

The script follows these steps to download and install WhatsApp:

1. Opens the Chrome/Chromium browser and visits the website "https://store.rg-adguard.net/".

2. Sends the URL of the WhatsApp download page to the input field on the website.

3. Clicks the download button.

4. Waits for the page to load and retrieves the page source code.

5. Parses the source code using BeautifulSoup and searches for the download link of the ".msixbundle" file.

6. Downloads the ".msixbundle" file and saves it to the local file system.

7. If a "whatsapp.msixbundle" file already exists, it will be deleted before downloading a new one.

8. Renames the downloaded file to "whatsapp.msixbundle".

9. Closes the browser.

10. Closes the WhatsApp application if it is running.

11. Installs the WhatsApp application using PowerShell and the "whatsapp.msixbundle" file.

12. Deletes the "whatsapp.msixbundle" file after installation.

## Contribution

If you want to contribute to this project, feel free to send pull requests with improvements or bug fixes.

## Additional Notes

- This script has been developed and tested in a Windows environment using Google Chrome/Chromium. It may require modifications to work with other operating systems or browsers.

- Please note that the use of this script may be subject to the terms and conditions of the WhatsApp download website. Make sure to comply with legal and ethical requirements when using this script.

- The script is provided "as is" without warranty of any kind. Use it at your own risk.

## License

This project is licensed under the [MIT License](LICENSE).
