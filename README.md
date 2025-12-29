# Cucumber.HtmlFormatter.CLI

A command-line tool to convert NDJSON files (Cucumber messages) into HTML reports.

## Usage

```
HtmlFormatterCli.exe [inputFiles] [--outputDirectory <dir>] [--mergedFile <fileName>]
```

### Arguments

- `inputFiles`  
  One or more NDJSON files, directories, or glob patterns to convert.  
  - Directories will be searched recursively for `.ndjson` files.
  - Glob patterns (e.g., `**/*.ndjson`) are supported.

### Options

- `--outputDirectory <dir>`  
  The directory where output HTML files will be written.  
  If not specified, output files are created in the same directory as their input files.

- `--mergedFile <fileName>`  
  If specified, all input NDJSON files are merged into a single NDJSON stream before conversion.  
  The result is a single HTML file (with name `fileName.html`) containing the merged report.

## Examples

Convert a single NDJSON file:

```
HtmlFormatterCli.exe results.ndjson
```

Convert all NDJSON files in a directory:

```
HtmlFormatterCli.exe path/to/dir
```

Convert using a glob pattern:

```
HtmlFormatterCli.exe "results/**/*.ndjson"
```

Convert and specify an output directory:

```
HtmlFormatterCli.exe results.ndjson --outputDirectory reports/
```

Merge multiple NDJSON files into a single HTML report:

```
HtmlFormatterCli.exe results1.ndjson results2.ndjson --mergedFile mergedReport
```

## Exit Codes

- `0` - Success
- `-1` - Error occurred during processing

## Notes

- The tool expects input files to be in Cucumber NDJSON message format.
- When using `--mergedFile`, only one HTML file will be produced.