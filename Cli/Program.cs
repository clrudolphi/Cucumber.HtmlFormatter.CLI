// See https://aka.ms/new-console-template for more information
using Cucumber.Messages;
using Io.Cucumber.Messages.Types;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using Exception = System.Exception;

internal class Program
{
    public static int Main(string[] args)
    {

        var inputFilesArgument = new Argument<string[]>(
            "inputFiles",
            "The NDJSON files or directories to convert.")
        {
            Arity = ArgumentArity.OneOrMore
        };

        var outputDirectoryOption = new Option<string>(
            "--outputDirectory",
            "The output directory. If not specified, the output files will be created in the same directory as the input files.");

        var rootCommand = new RootCommand
        {
            inputFilesArgument,
            outputDirectoryOption
        };

        rootCommand.Description = "Converts NDJSON files to HTML files.";

        rootCommand.SetHandler( (string[] inputFiles, string outputDirectory) =>
        {
            int exitCode = 0;

            // Gather all of the file, directories and patterns into a combined list of files
            List<string> filesToProcess = new();
            foreach (var inputFile in inputFiles)
            {
                try
                {
                    filesToProcess.AddRange(TransformInputPatternToListOfFiles(inputFile));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while processing {inputFile}."
                        + Environment.NewLine +
                        ex.Message);
                    exitCode = -1;
                };
            }
            // iterate over each file and process it
            foreach (var file in filesToProcess.Distinct())
            {
                try
                {
                    var rootFileName = Path.GetFileNameWithoutExtension(file);
                    string filePath = outputDirectory ?? Path.GetDirectoryName(file!)!;
                    var outputFileName = Path.Combine(filePath, rootFileName + ".html");

                    using var outFile = File.Create(outputFileName);
                    using var outputStreamWriter = new StreamWriter(outFile);
                    using var inputStream = File.OpenText(file).BaseStream;

                    var streamSerializerAction = new Action<StreamWriter, Envelope>((sw, e) =>
                    {
                        var s = HtmlFormatter.NdjsonSerializer.Serialize(e);
                        sw.Write(s);
                    });

                    using var ndjsonReader = new NdjsonMessageReader(inputStream, HtmlFormatter.NdjsonSerializer.Deserialize);
                    using var htmlFormatter = new HtmlFormatter.MessagesToHtmlWriter(outputStreamWriter, streamSerializerAction);

                    foreach (var message in ndjsonReader)
                    {
                        if (message == null || EnvelopeIsEmpty(message))
                            throw new InvalidDataException("Empty Envelope or non-Ndjson Json data encountered in {file}.");
                        htmlFormatter.Write(message);
                    }

                    Console.WriteLine($"Conversion of {file} completed successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while processing {file}." +
                        Environment.NewLine +
                        ex.Message);

                    exitCode = -1;
                }
            };

            Environment.ExitCode = exitCode; // IF an error occurred, this will be set to -1
        },
        inputFilesArgument,
        outputDirectoryOption);

        rootCommand.InvokeAsync(args).Wait();
        return Environment.ExitCode;
    }

    private static bool EnvelopeIsEmpty(Envelope message)
    {
        return (message.Attachment == null &&
            message.GherkinDocument == null &&
            message.Hook == null &&
            message.Meta == null &&
            message.ParameterType == null &&
            message.Pickle == null &&
            message.Source == null &&
            message.StepDefinition == null &&
            message.TestCase == null &&
            message.TestCaseFinished == null &&
            message.TestCaseStarted == null &&
            message.TestRunFinished == null &&
            message.TestRunHookFinished == null &&
            message.TestRunHookStarted == null &&
            message.TestRunStarted == null &&
            message.TestStepFinished == null &&
            message.TestStepStarted == null &&
            message.UndefinedParameterType == null &&
            message.ParseError == null);
    }

    private static List<string> TransformInputPatternToListOfFiles(string inputFile)
    {
        var filesToProcess = new List<string>();
        var matcher = new Matcher();

        if (Directory.Exists(inputFile))
        {
            matcher.AddInclude("**/*.ndjson");
            var directoryMatches = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(inputFile)));
            if (directoryMatches.HasMatches)
                filesToProcess.AddRange(directoryMatches.Files.Select(file => Path.Combine(inputFile, file.Path)));
            else
                throw new ArgumentException($"No ndjson files found in Directory: {inputFile}");
        }
        else if (File.Exists(inputFile))
        {
            filesToProcess.Add(inputFile);
        }
        else
        {
            matcher.AddInclude(inputFile);
            var directoryMatches = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(Directory.GetCurrentDirectory())));
            if (directoryMatches.HasMatches)
                filesToProcess.AddRange(directoryMatches.Files.Select(file => Path.Combine(Directory.GetCurrentDirectory(), file.Path)));
            else
                throw new ArgumentException($"File or Directory: {inputFile} not found.");
        }

        return filesToProcess;
    }
}
