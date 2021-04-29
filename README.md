# iMLP
codebase and dotnet CLI tool for prediction of iMTS-Ls 

## Installation

### Dotnet tool

### Docker

### Published binaries

## Usage

```
USAGE: imlp [--help] [--sequence <sequence>] [--inputfile <inputFile>] [--outputfile <outputFile>]
            [--plotdirectory <plotDirectory>] [--proteinheaderregex <proteinHeaderRegex>] [--verbosity <verbosity>]

OPTIONS:

    --sequence, -s <sequence>
                          A single, one-letter coded amino acid input sequence. Either --sequence (-s) or --inputFile
                          (-f) must be specified.
    --inputfile, -f <inputFile>
                          Path to a fasta formatted input file that may contain multiple entries. Either --sequence
                          (-s) or --inputFile (-f) must be specified.
    --outputfile, -o <outputFile>
                          (optional) Path to the desired output file, which will be tab separated (tsv). If not
                          specified, output will be printed to stdout instead.
    --plotdirectory, -p <plotDirectory>
                          (optional) Path to a directory to save plots to.
    --proteinheaderregex, -pr <proteinHeaderRegex>
                          (optional) Regular expression to extract protein names from fasta headers for the naming of
                          plot output files. if not provided, plot files will be 'protein_{i}.html', where i is the
                          index in the input.
    --verbosity, -v <verbosity>
                          (optional) The verbosity of the logging process. 0(Silent) | 1(Error) | 2(Warn) | 3(Info) |
                          >=4 : Debug | (default:1)
    --help                display this list of options.
```

## Development