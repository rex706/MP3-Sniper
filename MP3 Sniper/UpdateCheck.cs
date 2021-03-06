﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MP3_Sniper
{
    class UpdateCheck
    {
        /// <summary>
        /// String containing the latest version acquired from online text file.
        /// </summary>
        public static string latestVersion { get; set; }

        /// <summary>
        /// Check program for updates with the given text url.
        /// Returns 1 if the user chose not to update or 0 if there is no update available.
        /// </summary>
        public static async Task<int> CheckForUpdate(string url)
        {
            // Nkosi Note: Always use asynchronous versions of network and IO methods.

            // Check for version updates
            var client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 0, 10);

            // If a new version of the updater was downloaded, replace the old one.
            if (File.Exists("Updater_new.exe"))
            {
                File.Delete("Updater.exe");
                File.Move("Updater_new.exe", "Updater.exe");
            }

            try
            {
                // Open the text file using a stream reader.
                using (Stream stream = await client.GetStreamAsync(url))
                {
                    StreamReader reader = new StreamReader(stream);

                    // Get current and latest versions of program.
                    Version current = Assembly.GetExecutingAssembly().GetName().Version;
                    Version latest = Version.Parse(await reader.ReadLineAsync());

                    // Update latest version string class member.
                    latestVersion = latest.ToString();

                    // Initialize variables.
                    StringBuilder parameters = new StringBuilder();

                    // First parameter is the name of the running application to be opened after the update is complete.
                    parameters.Append(System.AppDomain.CurrentDomain.FriendlyName + " ");

                    // Load parameters string with urls and file names.
                    while (!reader.EndOfStream)
                    {
                        parameters.Append(await reader.ReadLineAsync() + " ");
                    }

                    // If the version from the online text is newer than the current version,
                    // ask user if they would like to download and install update now.
                    if (latest > current)
                    {
                        // Show message box that an update is available.
                        MessageBoxResult answer = MessageBox.Show("A new version of " +
                            AppDomain.CurrentDomain.FriendlyName.Substring(0, AppDomain.CurrentDomain.FriendlyName.IndexOf('.')) +
                            " is available!\n\nCurrent Version     " + current + "\nLatest Version        " + latest +
                            "\n\nUpdate now?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);

                        // Update is available, and user wants to update. Requires app to close.
                        if (answer == MessageBoxResult.Yes)
                        {
                            // Setup update process information.
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.CreateNoWindow = false;
                            startInfo.UseShellExecute = true;
                            startInfo.FileName = "Updater.exe";
                            startInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                            startInfo.WindowStyle = ProcessWindowStyle.Normal;
                            startInfo.Arguments = parameters.ToString();

                            // Launch updater and exit.
                            Process.Start(startInfo);
                            Environment.Exit(0);

                            return 2;
                        }

                        // Update is available, but user chose not to update just yet.
                        return 1;
                    }
                }

                // No update available.
                return 0;
            }
            catch
            {
                // Some error occured or there is no internet connection.
                return -1;
            }
        }
    }
}
