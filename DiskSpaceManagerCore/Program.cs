using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskSpaceManager
{
    class Program
    {
        const int Kilobyte = 1024;
        const int Megabyte = 1048576;
        const int Gigabyte = 1073741824;

        static StreamWriter? outputFile;

        static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            var parameters = ParseCommandLine(args);
            ValidateParameters(parameters);

            if (!String.IsNullOrWhiteSpace(parameters.OutputFile))
            {
                outputFile = new StreamWriter(new FileStream(parameters.OutputFile, FileMode.Create, FileAccess.Write));
            }

            var directory = new DirectoryInfo(parameters.RootDirectory);

            var directoriesList = await GetDirectorySizesAsync(directory);
            var filesList = await GetFileSizesAsync(directory.GetFiles());

            WriteOutput($"{parameters.RootDirectory} {directoriesList.Count} SubDirectories:");

            foreach (var directoryInfo in directoriesList.OrderBy(d => d.Key))
            {
                WriteOutput($"{directoryInfo.Key}{"\t"}{directoryInfo.Value}"); 
            }

            WriteOutput($"{parameters.RootDirectory} {filesList.Count} Files:");

            foreach (var fileInfo in filesList)
            {
                WriteOutput($"{fileInfo.Key}{"\t"}{fileInfo.Value}");
            }

            if (outputFile != null)
            {
                outputFile.Dispose();
            }

            Pause();
        }

        private static async Task<ConcurrentBag<KeyValuePair<string, string>>> GetDirectorySizesAsync(DirectoryInfo contextDirectory)
        {
            var directories = new ConcurrentBag<KeyValuePair<string, string>>();

            await Task.Run(() =>
            {
                Parallel.ForEach(contextDirectory.GetDirectories(), directory =>
                {
                    directories.Add(new KeyValuePair<string, string>(
                        directory.Name,
                        GetFriendlyBytesAmount(GetDirectorySize(directory.FullName))));
                });
            });

            return directories;
        }

        private static async Task<ConcurrentBag<KeyValuePair<string, string>>> GetFileSizesAsync(IEnumerable<FileInfo> fileInfos)
        {
            var fileDetails = new ConcurrentBag<KeyValuePair<string, string>>();

            await Task.Run(() =>
            {
                Parallel.ForEach(fileInfos, fileInfo =>
                {
                    fileDetails.Add(new KeyValuePair<string, string>(
                        fileInfo.Name,
                        GetFriendlyBytesAmount(fileInfo.Length)));
                });
            });

            return fileDetails;
        }

        static void Pause()
        {
            Console.WriteLine("Press [Enter] to close.");
            Console.ReadLine();
        }

        static string GetFriendlyBytesAmount(long bytes)
        {
            if (bytes >= Gigabyte)
            {
                return string.Format("{0} GB", ((double)bytes / (double)Gigabyte).ToString("F2", CultureInfo.InvariantCulture));
            }

            if (bytes >= Megabyte)
            {
                return string.Format("{0} MB", ((double)bytes / (double)Megabyte).ToString("F2", CultureInfo.InvariantCulture));
            }

            if (bytes >= Kilobyte)
            {
                return string.Format("{0} KB", ((double)bytes / (double)Kilobyte).ToString("F2", CultureInfo.InvariantCulture));
            }

            return string.Format("{0} bytes", bytes);
        }

        static void WriteOutput(string output)
        {
            if (outputFile != null)
            {
                outputFile.WriteLine(output);
            }

            if (outputFile != null)
            {
                outputFile.WriteLine(output);
            }

            if (output.Contains("GB"))
            {
                ConsoleExtension.WriteLine(ConsoleColor.Red, output);
            }
            else if (output.Contains("MB"))
            {
                ConsoleExtension.WriteLine(ConsoleColor.Yellow, output);
            }
            else if (output.Contains("KB"))
            {
                ConsoleExtension.WriteLine(ConsoleColor.Green, output);
            }
            else
            {
                Console.WriteLine(output);
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (outputFile != null)
            {
                outputFile.Dispose();
            }
        }

        static long GetDirectorySize(string path)
        {
            var directorySize = 0L;

            var directory = new DirectoryInfo(path);

            try
            {
                foreach (var child in directory.GetDirectories())
                {
                    try
                    {
                        directorySize += GetDirectorySize(child.FullName);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        continue;
                    }
                }

                foreach (var file in directory.GetFiles())
                {
                    directorySize += file.Length;
                }
            }
            catch (UnauthorizedAccessException)
            { }

            return directorySize;
        }

        static Parameters ParseCommandLine(string[] args)
        {
            var parameters = new Parameters();

            for (var i = 0; i < args.Length; i++)
            {
                var argument = args[i].Substring(args[i].IndexOf("-") + 1);

                switch ((CommandLineParameters)(Enum.Parse(typeof(CommandLineParameters), argument, true)))
                {
                    case CommandLineParameters.Root:
                        parameters.RootDirectory = args[++i];
                        break;
                    case CommandLineParameters.OutputFile:
                        parameters.OutputFile = args[++i];
                        break;
                    default:
                        continue;
                }
            }

            return parameters;
        }

        static void ValidateParameters(Parameters parameters)
        {
            if (!Directory.Exists(parameters.RootDirectory))
            {
                throw new ValidationException("The specified Root Directory does not exist.");
            }
        }

        struct Parameters
        {
            public string RootDirectory;
            public string OutputFile;
        }
    }
}
