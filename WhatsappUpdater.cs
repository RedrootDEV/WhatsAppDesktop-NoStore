using HtmlAgilityPack;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MicrosoftStoreDownloader
{
    public class MicrosoftStoreApp
    {
        private readonly string token = "9NKSQGP7F2NH";
        private string response = null;

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

                if (InstallAppx(msixBundleLocation.Name))
                {
                    Console.WriteLine("Installation complete.");
                    DeleteMsixBundle(msixBundleLocation.Name);
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

                using (HttpClient httpClient = new HttpClient())
                {
                    var content = new StringContent(postData, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
                    var response = await httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        this.response = await response.Content.ReadAsStringAsync();
                        ParseLocations();
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
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

            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\WhatsappUpdater", "LastVersion", null) is string existingVersion)
            {
                return string.Compare(newVersion, existingVersion) > 0;
            }

            return true;
        }

        private void RegisterVersion(string version)
        {
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\WhatsappUpdater", "LastVersion", version);
        }

        private bool InstallAppx(string fileName)
        {
            try
            {
                string installCommand = $"Add-AppxPackage -Path .\\{fileName}";
                Console.WriteLine($"Installing the application: {fileName}");

                using (System.Diagnostics.Process process = new System.Diagnostics.Process())
                {
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                    process.StartInfo = psi;
                    process.Start();

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running PowerShell command: {ex.Message}");
                return false;
            }
        }

        private void DeleteMsixBundle(string fileName)
        {
            try
            {
                File.Delete(fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting the file: {ex.Message}");
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

                    using (HttpClient httpClient = new HttpClient())
                    {
                        var content = httpClient.GetByteArrayAsync(downloadUrl).Result;
                        File.WriteAllBytes(fileName, content);
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
