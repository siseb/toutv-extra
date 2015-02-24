﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;

using ManyConsole;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace toutv
{
    public class Fetch : ConsoleCommand
    {
        public string MediaUrl { get; set; }

        private const string UrlGetClaims = "https://services.radio-canada.ca/media/validation/v2/GetClaims?token={0}";
        private const string UrlGetMediaMetadata = "http://ici.tou.tv/presentation/{0}?excludeLineups=True&v=2&d=phone-android";
        private const string UrlGetMediaPlaylist = "https://services.radio-canada.ca/media/validation/v2/?appCode=toutv&deviceType=iphone4&connectionType=wifi&idMedia={0}&claims={1}&output=json";

        private const string UserAgent = "TouTvApp/2.1.2.2 (samsung/jgedlteue/(SGH-I337M); API/19/-/Kitkat; en-ca)";

        public Fetch()
        {
            this.IsCommand("fetch", "Download a media");
            this.HasRequiredOption("m=", "tou.tv slug to the media. Look at the tou.tv website Ex: infoman/S15E23", x => MediaUrl = x);
        }

        public override int Run(string[] remainingArguments)
        {
            // Get media metadata
            var metaData = new MediaMetaData(JObject.Parse(new WebClient().DownloadString(string.Format(UrlGetMediaMetadata, MediaUrl))));
          
            // Get what would be the resulting output filename and check if the file already exists
            var outputFileName = GetOutputFileName(metaData);
            if (File.Exists(outputFileName))
            {
                Console.WriteLine("File \"{0}\" already exists. Skipping.");
                return 0;
            }

            // Read the access_token from the token file
            var accessToken = JsonConvert.DeserializeObject(File.ReadAllText("token.json"));

            // Exhange the access_token for a user claims
            var request1 = WebRequest.CreateHttp(string.Format(UrlGetClaims, accessToken));
            request1.PreAuthenticate = true;
            request1.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;
            request1.Accept = "application/json";
            request1.UserAgent = UserAgent;
            request1.Host = "services.radio-canada.ca";

            var o = JObject.Parse(new StreamReader(request1.GetResponse().GetResponseStream()).ReadToEnd());
            var claim = o["claims"].Value<string>();

            // Get the media URL
            var request2 = WebRequest.CreateHttp(string.Format(UrlGetMediaPlaylist, metaData.MediaId, claim));
            request2.PreAuthenticate = true;
            request2.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;
            request2.UserAgent = UserAgent;
            request2.Host = "services.radio-canada.ca";

            o = JObject.Parse(new StreamReader(request2.GetResponse().GetResponseStream()).ReadToEnd());
            var url = o["url"].Value<string>();

            // Start a ffmpeg process downloading the file
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = string.Format("-i \"{0}\" -c copy \"{1}\"", url, outputFileName)
                }
            };
            process.Start();
            process.WaitForExit();

            return 0;
        }

        private static string GetOutputFileName(MediaMetaData metaData)
        {
            var outputfilename = string.Format("{0} {1}{2}", metaData.Title, metaData.Saison, metaData.Episode).Trim();
            return outputfilename + ".ts";
        }
    }

    internal class MediaMetaData
    {
        public string MediaId { get; set; }
        public string Title { get; set; }
        public string Saison { get; set; }
        public string Episode { get; set; }

        public MediaMetaData(JObject json)
        {
            MediaId = json["IdMedia"].Value<string>();
            Title = json["Title"].Value<string>();
            Saison = json["StatsMetas"]["rc.saison"].Value<string>();
            Episode = json["StatsMetas"]["rc.episode"].Value<string>();
        }
    }
}
