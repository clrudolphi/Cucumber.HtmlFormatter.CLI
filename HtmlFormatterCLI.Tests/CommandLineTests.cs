using FluentAssertions;
using FluentAssertions.Execution;
using System.Collections.Generic;
using System.Reflection;

namespace HtmlFormatterCLI.Tests
{
    [TestClass]
    public sealed class CommandLineTests
    {
        [TestMethod]
        public void ReportsErrorWhenFileNotFound()
        {
            using (new AssertionScope())
            {
                var filename = "notafile.ndjson";
                var testLocation = TestHelpers.CreateTempDirectory();
                try
                {
                    var working = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var destinationFilePath = Path.Combine(testLocation, filename);

                    (int exitCode, string output) = TestHelpers.RunCommandLine("HtmlFormatterCli.exe", destinationFilePath, working);
                    exitCode.Should().Be(-1);
                    output.Should().Contain($"An error occurred while processing {destinationFilePath}");
                }
                finally
                {
                    TestHelpers.DeleteTempDirectory(testLocation);
                }
            }
        }

        [TestMethod]
        public void ReportsErrorWhenDirectoryNotFound()
        {
            using (new AssertionScope())
            {
                var directoryName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
                var testLocation = TestHelpers.CreateTempDirectory();
                try
                {
                    var working = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var destinationFilePath = Path.Combine(testLocation, directoryName);

                    (int exitCode, string output) = TestHelpers.RunCommandLine("HtmlFormatterCli.exe", destinationFilePath, working);
                    exitCode.Should().Be(-1);
                    output.Should().Contain($"An error occurred while processing {destinationFilePath}");
                }
                finally
                {
                    TestHelpers.DeleteTempDirectory(testLocation);
                }
            }
        }

        [TestMethod]
        public void ReportsErrorWhenNoNDJSONFilesFoundInDirectory()
        {
            var testLocation = TestHelpers.CreateTempDirectory();
            var working = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (new AssertionScope())
                try
                {
                    (int exitCode, string output) = TestHelpers.RunCommandLine("HtmlFormatterCli.exe", testLocation, working);
                    exitCode.Should().Be(-1);
                    output.Should().Contain($"An error occurred while processing {testLocation}");
                }
                finally
                {
                    TestHelpers.DeleteTempDirectory(testLocation);
                }

        }
        [TestMethod]
        public void ReportsErrorWhenFormattingFails()
        {
            using (new AssertionScope())
            {
                var filename = "BADDATA.ndjson";
                var testLocation = TestHelpers.CreateTempDirectory();
                try
                {
                    var working = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var sourceFilePath = Path.Combine(working, "SampleData/bad", filename);
                    var destinationFilePath = Path.Combine(testLocation, filename);
                    File.Copy(sourceFilePath, destinationFilePath);

                    (int exitCode, string output) = TestHelpers.RunCommandLine("HtmlFormatterCli.exe", destinationFilePath, working);
                    exitCode.Should().Be(-1);
                    output.Should().Contain($"An error occurred while processing {destinationFilePath}");
                }
                finally
                {
                    TestHelpers.DeleteTempDirectory(testLocation);
                }
            }
        }

        [TestMethod]
        public void TransformsOneFile()
        {
            using (new AssertionScope())
            {
                var filename = "minimal.ndjson";
                var testLocation = TestHelpers.CreateTempDirectory();
                try
                {
                    var working = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var sourceFilePath = Path.Combine(working, "SampleData/good", filename);
                    var destinationFilePath = Path.Combine(testLocation, filename);
                    File.Copy(sourceFilePath, destinationFilePath);

                    (int exitCode, string output) = TestHelpers.RunCommandLine("HtmlFormatterCli.exe", destinationFilePath, working);
                    exitCode.Should().Be(0);
                    output.Should().Contain($"Conversion of {destinationFilePath} completed successfully.");
                    var transformedFilePath = Path.Combine(testLocation, Path.GetFileNameWithoutExtension(filename) + ".html");
                    File.Exists(transformedFilePath).Should().BeTrue();
                }
                finally
                {
                    TestHelpers.DeleteTempDirectory(testLocation);
                }
            }
        }
        [TestMethod]
        public void TransformsMultipleFilesInADirectory()
        {
            using (new AssertionScope())
            {
                var testLocation = TestHelpers.CreateTempDirectory();
                try
                {
                    var working = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var sourceFilePath = Path.Combine(working, "SampleData/good");
                    // copy each .ndjson file found in the sourceFilePath to the testLocation
                    var ndjsonFiles = Directory.GetFiles(sourceFilePath, "*.ndjson");
                    foreach (var file in ndjsonFiles)
                    {
                        var destinationFilePath = Path.Combine(testLocation, Path.GetFileName(file));
                        File.Copy(file, destinationFilePath);
                    }

                    (int exitCode, string output) = TestHelpers.RunCommandLine("HtmlFormatterCli.exe", testLocation, working);
                    exitCode.Should().Be(0);
                    output.Should().Contain($"completed successfully.", Exactly.Times(ndjsonFiles.Length));

                    var htmlFiles = Directory.GetFiles(testLocation, "*.html");
                    htmlFiles.Length.Should().Be(ndjsonFiles.Length);
                }
                finally
                {
                    TestHelpers.DeleteTempDirectory(testLocation);
                }
            }
        }

        [TestMethod]
        public void TransformsMultipleFilesGivenOnTheCommandLine()
        {
            using (new AssertionScope())
            {
                var testLocation = TestHelpers.CreateTempDirectory();
                var working = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                try
                {
                    var sourceFilePath = Path.Combine(working, "SampleData/good");
                    // copy each .ndjson file found in the sourceFilePath to the testLocation
                    var ndjsonFiles = Directory.GetFiles(sourceFilePath, "*.ndjson");
                    foreach (var file in ndjsonFiles)
                    {
                        var destinationFilePath = Path.Combine(testLocation, Path.GetFileName(file));
                        File.Copy(file, destinationFilePath);
                    }
                    var listOfNdjsonInDestination = Directory.GetFiles(testLocation, "*.ndjson");
                    var quoted = TestHelpers.QuoteStrings(listOfNdjsonInDestination);
                    var commandLineFileList = String.Join(" ", quoted);
                    (int exitCode, string output) = TestHelpers.RunCommandLine("HtmlFormatterCli.exe", commandLineFileList, working);
                    exitCode.Should().Be(0);
                    output.Should().Contain($"completed successfully.", Exactly.Times(ndjsonFiles.Length));

                    var htmlFiles = Directory.GetFiles(testLocation, "*.html");
                    htmlFiles.Length.Should().Be(ndjsonFiles.Length);
                }
                finally
                {
                    TestHelpers.DeleteTempDirectory(testLocation);
                }
            }
        }

        [TestMethod]
        public void TransformsMultipleFilesFromAGlob()
        {
            using (new AssertionScope())
            {
                var testLocation = TestHelpers.CreateTempDirectory();
                try
                {
                    var working = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var sourceFilePath = Path.Combine(working, "SampleData/good");
                    var ndjsonFiles = Directory.GetFiles(sourceFilePath, "*.ndjson", SearchOption.AllDirectories);
                    var globcommand = $"SampleData/good/**/*.ndjson --outputDirectory {testLocation}";
                    (int exitCode, string output) = TestHelpers.RunCommandLine("HtmlFormatterCli.exe", globcommand, working);
                    exitCode.Should().Be(0);
                    output.Should().Contain($"completed successfully.", Exactly.Times(ndjsonFiles.Length));

                    var htmlFiles = Directory.GetFiles(testLocation, "*.html");
                    htmlFiles.Length.Should().Be(ndjsonFiles.Length);
                }
                finally
                {
                    TestHelpers.DeleteTempDirectory(testLocation);
                }
            }
        }

        [TestMethod]
        public void TransformsMultipleFilesWithMergeOption()
        {
            using (new AssertionScope())
            {
                var testLocation = TestHelpers.CreateTempDirectory();
                var working = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                try
                {
                    // Use real sample files from SampleData/good
                    var sourceDir = Path.Combine(working, "SampleData/good");
                    var file1 = Path.Combine(sourceDir, "minimal.ndjson");
                    var file2 = Path.Combine(sourceDir, "SubDirectory/Hooks.ndjson");
                    var destFile1 = Path.Combine(testLocation, "minimal.ndjson");
                    var destFile2 = Path.Combine(testLocation, "Hooks.ndjson");
                    File.Copy(file1, destFile1);
                    File.Copy(file2, destFile2);
                    var mergedFilename = "merged.html";

                    var commandLine = $"{TestHelpers.QuoteString(destFile1)} {TestHelpers.QuoteString(destFile2)} --mergedFile {TestHelpers.QuoteString(mergedFilename)}  --outputDirectory {TestHelpers.QuoteString(testLocation)}";
                    (int exitCode, string output) = TestHelpers.RunCommandLine("HtmlFormatterCli.exe", commandLine, working);
                    exitCode.Should().Be(0);
                    output.Should().Contain("Conversion of");

                    // Only one HTML file should be produced (from merged ndjson)
                    var htmlFiles = Directory.GetFiles(testLocation, "*.html");
                    htmlFiles.Length.Should().Be(1);
                    var htmlContent = File.ReadAllText(htmlFiles[0]);
                    htmlContent.Should().Contain("Feature"); // Should contain at least one feature
                    htmlContent.Should().Contain("Hook");    // Should contain hook info from Hooks.ndjson
                }
                finally
                {
                    TestHelpers.DeleteTempDirectory(testLocation);
                }
            }
        }
    }
}
