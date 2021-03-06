﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using JMMClient.Utilities;
using Microsoft.Win32;

namespace JMMClient.VideoPlayers
{
    public class MPCVideoPlayer : BaseVideoPlayer, IVideoPlayer
    {

        private System.Timers.Timer playerWebUiTimer = null;


        public override void Init()
        {
	        // nvo is nvidia optimus
            PlayerPath = Utils.CheckSysPath(new string[] { "mpc-hc64.exe", "mpc-hc.exe", "mpc-hc64_nvo.exe", "mpc-hc_nvo.exe" });
            //Look for 64bit
            if (string.IsNullOrEmpty(PlayerPath))
            { 
                PlayerPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Combined Community Codec Pack 64bit_is1", "InstallLocation", null);
                if (!string.IsNullOrEmpty(PlayerPath))
                    PlayerPath = System.IO.Path.Combine(PlayerPath, @"MPC\mpc-hc64.exe");
            }
            if (string.IsNullOrEmpty(PlayerPath))
            {
                PlayerPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Combined Community Codec Pack_is1", "InstallLocation", null);
                if (!string.IsNullOrEmpty(PlayerPath))
                    PlayerPath = System.IO.Path.Combine(PlayerPath, @"MPC\mpc-hc.exe");
            }
            if (string.IsNullOrEmpty(PlayerPath))
            {
                PlayerPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\KLiteCodecPack_is1", "InstallLocation", null);
                if (!string.IsNullOrEmpty(PlayerPath))
                    if (File.Exists(Path.Combine(PlayerPath, @"MPC-HC64\mpc-64.exe")))
                        PlayerPath = Path.Combine(PlayerPath, @"MPC-HC64\mpc-64.exe");
                    else if (File.Exists(Path.Combine(PlayerPath, @"MPC-HC64\mpc-hc64_nvo.exe")))
                        PlayerPath = Path.Combine(PlayerPath, @"MPC-HC64\mpc-hc64_nvo.exe");
                    else if (File.Exists(Path.Combine(PlayerPath, @"MPC-HC\mpc.exe")))
                        PlayerPath = Path.Combine(PlayerPath, @"MPC-HC\mpc.exe");
                    else if (File.Exists(Path.Combine(PlayerPath, @"MPC-HC\mpc-hc_nvo.exe")))
                        PlayerPath = Path.Combine(PlayerPath, @"MPC-HC\mpc-hc_nvo.exe");
                    else
                        PlayerPath = null;
            }
            if (string.IsNullOrEmpty(PlayerPath))
            {
                PlayerPath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\MPC-HC\MPC-HC", "ExePath", null);
            }
            if (string.IsNullOrEmpty(PlayerPath))
            {
                Active = false;
                return;
            }
            Active = true;
            if (AppSettings.MPCIniIntegration)
                StartWatchingFiles(AppSettings.MPCFolder);
            if (AppSettings.MPCWebUiIntegration)
            {
                playerWebUiTimer = new System.Timers.Timer();
                playerWebUiTimer.Elapsed += HandleWebUIRequest;
                playerWebUiTimer.Interval = 1000;
                playerWebUiTimer.Enabled = true;
            }
        }

        public VideoPlayer Player => VideoPlayer.MPC;

        internal override void FileChangeEvent(string filePath)
        {
            try
            {
                if (!AppSettings.VideoAutoSetWatched) return;

                List<int> allFilesHeaders = new List<int>();
                List<int> allFilesPositions = new List<int>();

                string[] lines = File.ReadAllLines(filePath);

                // really we only need to check the first file, but will do this just in case

                // MPC 1.3 and lower looks like this
                // File Name 0=M:\[ Anime to Watch - New ]\Arata Kangatari\[HorribleSubs] Arata Kangatari - 05 [720p].mkv
                // File Position 0=14251233493
                // File Name 1=M:\[ Anime to Watch - New ]\Hentai Ouji to Warawanai Neko\[gg]_Hentai_Ouji_to_Warawanai_Neko_-_04_[62E1DBF8].mkv
                // File Position 1=13688612500

                // MPC 1.6 and lower looks like this
                // File Name 0=M:\[ Anime to Watch - New ]\Arata Kangatari\[HorribleSubs] Arata Kangatari - 05 [720p].mkv
                // File Name 1=M:\[ Anime to Watch - New ]\Hentai Ouji to Warawanai Neko\[gg]_Hentai_Ouji_to_Warawanai_Neko_-_04_[62E1DBF8].mkv
                // File Position 0=14251233493
                // File Position 1=13688612500

                // get file name header lines
                string prefixHeader = string.Format("File Name ");
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (line.StartsWith(prefixHeader, StringComparison.InvariantCultureIgnoreCase)) allFilesHeaders.Add(i);

                    if (allFilesHeaders.Count == 20) break;
                }

                if (allFilesHeaders.Count == 0) return;

                string prefixPos = string.Format("File Position ");
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (line.StartsWith(prefixPos, StringComparison.InvariantCultureIgnoreCase)) allFilesPositions.Add(i);

                    if (allFilesPositions.Count == 20) break;
                }

                Dictionary<string, long> filePositions = new Dictionary<string, long>();
                foreach (int lineNumber in allFilesHeaders)
                {
                    // find the last file played
                    string fileNameLine = lines[lineNumber];

                    // find the number of this file
                    string temp = fileNameLine.Trim().Replace(prefixHeader, "");
                    int iPosTemp = temp.IndexOf("=");
                    if (iPosTemp < 0) continue;

                    string fileNumber = temp.Substring(0, iPosTemp);

                    // now find the match play position line
                    string properPosLine = string.Empty;
                    foreach (int lineNumberPos in allFilesPositions)
                    {
                        string filePosLineTemp = lines[lineNumberPos];

                        // find the number of this file
                        string temp2 = filePosLineTemp.Trim().Replace(prefixPos, "");
                        int iPosTemp2 = temp2.IndexOf("=");
                        if (iPosTemp2 < 0) continue;

                        string fileNumber2 = temp2.Substring(0, iPosTemp2);
                        if (fileNumber.Equals(fileNumber2, StringComparison.InvariantCultureIgnoreCase))
                        {
                            properPosLine = filePosLineTemp;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(properPosLine)) continue;

                    // extract the file name and position
                    int iPos1 = fileNameLine.IndexOf("=");
                    if (iPos1 < 0) continue;

                    int iPos2 = properPosLine.IndexOf("=");

                    string fileName = fileNameLine.Substring(iPos1 + 1, fileNameLine.Length - iPos1 - 1);
                    string position = properPosLine.Substring(iPos2 + 1, properPosLine.Length - iPos2 - 1);

                    long mpcPos = 0;
                    long.TryParse(position, out mpcPos);

                    // if mpcPos == 0, it means that file either finished playing or have been stopped or re/started
                    // please note that mpc does not trigger *.ini file update at all times or at least periodically
                    // manual change of file position via clicks modify the file as well as starting playback and swiching file
                    // using arrows to navigate forward and backwards however do not as well regular playback

                    // MPC position is in 10^-7 s		                      
                    // convert to milli-seconds (10^-3 s)

                    // dont worry about remainder, as we're checking against 1 s precision later anyway
                    filePositions[fileName] = mpcPos / 10000;
                }
                OnPositionChangeEvent(filePositions);
            }
            catch (Exception ex)
            {
                logger.ErrorException(ex.ToString(), ex);
            }
        }

        // Make and handle MPC-HC Web UI request
        private async void HandleWebUIRequest(object source, System.Timers.ElapsedEventArgs e)
        {
            // Stop timer for the time request is processed
            playerWebUiTimer.Stop();
            // Request
            string mpcUIFullUrl = "http://" + AppSettings.MPCWebUIUrl + ":" + AppSettings.MPCWebUIPort + "/variables.html";
            // Helper variables
            string responseString = "";
            string nowPlayingFile = "";
            string nowPlayingFilePosition = "";
            string nowPlayingFileDuration = "";
            // Regex for extracting relevant information
            Regex fileRegex = new Regex("<p id=\"filepath\">(.*?)</p>");
            Regex filePositionRegex = new Regex("<p id=\"position\">(.*?)</p>");
            Regex fileDurationRegex = new Regex("<p id=\"duration\">(.*?)</p>");

            try
            {
                // Make HTTP request to Web UI
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(mpcUIFullUrl))
                using (HttpContent content = response.Content)
                {
                    // Check if request was ok
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        // Read the string
                        responseString = await content.ReadAsStringAsync();
                        // Parse result
                        if (responseString != null)
                        {
                            // extract currently playing video informations
                            nowPlayingFile = fileRegex.Match(responseString).Groups[1].ToString();
                            nowPlayingFilePosition = filePositionRegex.Match(responseString).Groups[1].ToString();
                            nowPlayingFileDuration = fileDurationRegex.Match(responseString).Groups[1].ToString();
                            // Parse number values for future aritmetics
                            double filePosition;
                            double fileDuration;
                            Double.TryParse(nowPlayingFilePosition, out filePosition);

                            Dictionary<string, long> pos=new Dictionary<string, long>();
                            pos.Add(nowPlayingFile,(long)filePosition);
                            OnPositionChangeEvent(pos);
                        }
                    }
                    // Start timer again
                    playerWebUiTimer.Start();
                }
            }
            catch (Exception exception)
            {
                logger.ErrorException(exception.ToString(), exception);
                playerWebUiTimer.Start();
            }
        }
    }
}
