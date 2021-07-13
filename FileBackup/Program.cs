using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BackupFolders
{
    class Program
    {
        private static string Destination = "";
        private static List<string> Sources;

        static void Main(string[] args)
        {
            Sources = new List<string>();
            try
            {
                UpdateSourcesAndDestination();
                if (Sources.Count == 0 && args.Length > 2)
                {
                    Destination = args[0];
                    for (int i = 1; i < args.Length; ++i)
                    {
                        Sources.Add(args[i]);
                        CopyAllFiles(args[i], Path.Combine(Destination, Path.GetFileName(args[i])));
                    }
                }
                else if (Sources.Count > 1 && !string.IsNullOrEmpty(Destination))
                {
                    foreach (string str in Sources)
                    {
                        CopyAllFiles(str, Path.Combine(Destination, Path.GetFileName(str)));
                    }
                }
                else
                {
                    CreateTextFileLog($"File path source/destination issue in main().");
                    return;
                }
            }
            catch (Exception ex)
            {
                CreateTextFileLog($"Task failed in Main(), Exception: {ex.Message}.");
            }
            CreateTextFileLog("Task Completed successfully.");
        }

        private static void UpdateSourcesAndDestination()
        {
            try
            {
                string mainFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string desiredFile = "";
                List<string> txtFiles = GetFiles(mainFolder, "*.txt");

                foreach (string fileName in txtFiles)
                {
                    if (fileName.ToLower().Contains("source") || fileName.ToLower().Contains("destination"))
                    {
                        desiredFile = fileName;
                        break;
                    }
                }

                string[] lines = File.ReadAllLines(desiredFile, Encoding.UTF8);

                foreach (string line in lines)
                {
                    if (line.ToLower().Contains("source"))
                    {
                        string[] split = line.Split('=');
                        if (split.Length > 1)
                        {
                            Sources.Add(split[1].Trim('\"'));
                        }
                    }
                    else if (line.ToLower().Contains("destination"))
                    {
                        string[] split = line.Split('=');
                        if (split.Length > 1)
                        {
                            Destination = split[1].Trim('\"');
                        }
                    }
                }
                if (Sources.Count == 0 || string.IsNullOrWhiteSpace(Destination))
                {
                    CreateTextFileLog($"Source or Destination not found in text file, Source count found is: {Sources.Count}, Destination found is: {Destination}.");
                }
            }
            catch (Exception ex)
            {
                CreateTextFileLog($"Task failed in UpdateSourceAndDestination(), Exception: {ex.Message}.");
            }
        }

        private static List<string> GetFiles(string path, string searchPattern)
        {
            try
            {
                return Directory.GetFiles(path, searchPattern).ToList();
            }
            catch (UnauthorizedAccessException ex)
            {
                CreateTextFileLog($"Exception thrown while in GetDirectories(), Exception: {ex.Message}");
                return new List<string>();
            }
        }

        private static void CopyAllFiles(string Source, string Destination)
        {
            try
            {
                // Get the subdirectories for the specified directory
                DirectoryInfo dir = new DirectoryInfo(Source);
                DirectoryInfo[] dirs = dir.GetDirectories();
                if (!dir.Exists)
                {
                    CreateTextFileLog("Source directory does not exist or could not be found: "
                        + Source);
                }

                // If the destination directory doesn't exist, create it
                if (!Directory.Exists(Destination))
                {
                    Directory.CreateDirectory(Destination);
                }

                // Get the files in the directory and copy them to the new location
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(Destination, file.Name);
                    file.CopyTo(temppath, true);
                }

                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(Destination, subdir.Name);
                    CopyAllFiles(subdir.FullName, temppath);
                }
            }
            catch (Exception ex)
            {
                CreateTextFileLog($"Exception thrown in CopyAllFiles(), Exception: {ex.Message}");
            }
        }

        private static void CreateTextFileLog(string Message)
        {
            StreamWriter SW;
            string dateTime = DateTime.Now.ToString("yyyyMMdd");
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "txt_" + dateTime + ".txt")))
            {
                SW = File.CreateText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "txt_" + dateTime + ".txt"));
                SW.Close();
            }
            using (SW = File.AppendText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "txt_" + dateTime + ".txt")))
            {
                SW.WriteLine(DateTime.Now.ToString("yyyy:MM:dd:hh:mm:ss") + ": " + Message);
                SW.Close();
            }
        }
    }
}
