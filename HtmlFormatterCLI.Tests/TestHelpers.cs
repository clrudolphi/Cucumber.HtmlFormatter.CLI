using System.Diagnostics;
using System.Text;

namespace HtmlFormatterCLI.Tests
{
    public static class TestHelpers
    {
        public static string CreateTempDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        public static void DeleteTempDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public static (int exitCode, string output) RunCommandLine(string executable, string arguments, string workingDirectory)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                string output;
                //var outputBuilder = new StringBuilder();
                //var errorBuilder = new StringBuilder();

                //process.OutputDataReceived += (sender, e) =>
                //{
                //    if (e.Data != null)
                //    {
                //        outputBuilder.AppendLine(e.Data);
                //    }
                //};

                //process.ErrorDataReceived += (sender, e) =>
                //{
                //    if (e.Data != null)
                //    {
                //        errorBuilder.AppendLine(e.Data);
                //    }
                //};

                process.Start();
                //process.BeginOutputReadLine();
                //process.BeginErrorReadLine();
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                process.WaitForExit(1000);

                //var output = outputBuilder.ToString();
                //var error = errorBuilder.ToString();

                //if (!string.IsNullOrEmpty(error))
                //{
                //    output += Environment.NewLine + error;
                //}

                return (process.ExitCode, output);
            }
        }
        public static IEnumerable<string> QuoteStrings(IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                if (filePath.Contains(' '))
                {
                    yield return $"\"{filePath}\"";
                    continue;
                }
                yield return filePath;
            }
        }

        internal static string QuoteString(string v)
        {
            if (v.Contains(' '))
                return $"\"{v}\"";
            return v;
        }
    }
}
