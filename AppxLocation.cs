using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MicrosoftStoreDownloader
{
    public class AppxLocation
    {
        public string Name;
        public string URL;
        public DateTime ExpireTime;
        public string SHA1;
        public string PackageName;
        public Version Version;
        public string Architecture;

        public async Task<string> DownloadAsync(Action<DownloadProgressChangedEventArgs> callback = null)
        {
            if (ExpireTime < DateTime.Now)
            {
                throw new InvalidOperationException("The download link has expired");
            }

            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Create a new WebClient instance.
            using (WebClient myWebClient = new WebClient())
            {
                myWebClient.DownloadProgressChanged += (sender, args) => callback?.Invoke(args);
                // Download the Web resource and save it into the current filesystem folder.
                await myWebClient.DownloadFileTaskAsync(new Uri(URL), Path.Combine(path, Name));
            }

            return Path.Combine(path, Name);
        }
    }
}
