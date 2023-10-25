using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MicrosoftStoreDownloader
{
    public class MicrosoftStoreApp
    {
        private string response = null;
        private readonly string token = "9NKSQGP7F2NH"; // Assign your token here

        public List<AppxLocation> Locations { get; } = new List<AppxLocation>();

        public static async Task Main(string[] args)
        {
            MicrosoftStoreApp app = new MicrosoftStoreApp();
            await app.StartDownloadAsync();
        }

        public async Task StartDownloadAsync()
        {
            await LoadAsync();
            AppxLocation msixBundleLocation = FindFirstMsixBundleLocation();
            if (msixBundleLocation != null)
            {
                string version = GetVersionFromFileName(msixBundleLocation.Name);
                if (!IsNewVersion(version))
                {
                    Console.WriteLine("The current version is equal or superior. No need to download.");
                    return;
                }

                DownloadMsixBundle(msixBundleLocation);
                Console.WriteLine("Download complete.");
                KillWhatsappProcess();
                // Install the application
                if (InstallAppx(msixBundleLocation.Name))
                {
                    Console.WriteLine("Installation complete.");
                    // Delete the downloaded file
                    File.Delete(msixBundleLocation.Name);
                    // Register the new version
                    RegisterVersion(version);
                }
                else
                {
                    Console.WriteLine("Error during installation.");
                }
            }
            else
            {
                Console.WriteLine("No .msixbundle files found for download.");
            }
        }
        private void KillWhatsappProcess()
        {
            try
            {
                string processName = "Whatsapp"; // Name of the Whatsapp process

                System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(processName);
                foreach (System.Diagnostics.Process process in processes)
                {
                    process.Kill();
                }

                Console.WriteLine($"{processName} process stopped.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping Whatsapp process: {ex.Message}");
            }
        }

        private async Task LoadAsync()
        {
            try
            {
                string url = "https://store.rg-adguard.net/api/GetFiles";
                string postData = $"type=url&url={Uri.EscapeUriString($"https://www.microsoft.com/store/apps/{token}")}";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(postData);
                }

                WebResponse response = await request.GetResponseAsync();
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    this.response = streamReader.ReadToEnd();
                }

                ParseLocations();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void ParseLocations()
        {
            Locations.Clear();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);

            List<HtmlNode> table = doc.DocumentNode.SelectSingleNode("//table[@class='tftable']")
                        .Descendants("tr")
                        .Where(tr => tr.Elements("td").Count() >= 1)
                        .ToList();

            foreach (HtmlNode row in table)
            {
                List<HtmlNode> data = row.Elements("td").ToList();
                string url = data[0].Descendants("a").FirstOrDefault()?.GetAttributeValue("href", null);
                string name = data[0].InnerText;
                string expire = data[1].InnerText;
                string sha1 = data[2].InnerText;

                string[] info = name.Split('_');

                string[] allowedExtensions = { ".msixbundle" };
                if (allowedExtensions.Contains(Path.GetExtension(name)))
                {
                    Locations.Add(new AppxLocation()
                    {
                        URL = url,
                        Name = name,
                        ExpireTime = DateTime.Parse(expire),
                        SHA1 = sha1,
                        PackageName = info[0],
                        Version = Version.Parse(info[1]),
                        Architecture = info[2]
                    });
                }
            }
        }

        private AppxLocation FindFirstMsixBundleLocation()
        {
            return Locations.FirstOrDefault(location => location.Name.EndsWith(".msixbundle"));
        }

        private string GetVersionFromFileName(string fileName)
        {
            // Use a regular expression to extract the version from the file name
            string pattern = @"_(\d+\.\d+\.\d+\.\d+)_";
            Match match = Regex.Match(fileName, pattern);
            return match.Success ? match.Groups[1].Value : null;
        }

        private bool IsNewVersion(string newVersion)
        {
            if (newVersion == null)
            {
                return false;
            }

            if (File.Exists("last_version.txt"))
            {
                string existingVersion = File.ReadAllText("last_version.txt");
                return string.Compare(newVersion, existingVersion) > 0;
            }

            return true;
        }

        private void RegisterVersion(string version)
        {
            File.WriteAllText("last_version.txt", version);
        }

        private bool InstallAppx(string fileName)
        {
            try
            {
                // Run the PowerShell command to install the application
                string installCommand = $"Add-AppxPackage -Path .\\{fileName}";
                Console.WriteLine($"Installing the application: {fileName}");

                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                System.Diagnostics.Process process = new System.Diagnostics.Process
                {
                    StartInfo = psi
                };

                _ = process.Start();
                process.StandardInput.WriteLine(installCommand);
                process.StandardInput.WriteLine("exit");
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine($"Error during installation. Exit code: {process.ExitCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running PowerShell command: {ex.Message}");
                return false;
            }
        }

        private void DownloadMsixBundle(AppxLocation location)
        {
            if (location != null)
            {
                try
                {
                    string downloadUrl = location.URL;
                    string fileName = location.Name;

                    Console.WriteLine($"Downloading: {fileName}");

                    using (WebClient webClient = new WebClient())
                    {
                        // Download the file with progress bar
                        webClient.DownloadFile(downloadUrl, fileName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error downloading the file: {ex.Message}");
                }
            }
        }
    }
}
