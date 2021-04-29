# iMLP

The imlp cli tool predicts iMTS-L propensity profiles for proteins of interest.

<!-- TOC -->

- [General](#general)
- [Installation](#installation)
    - [Dotnet tool](#dotnet-tool)
        - [Dependencies on linux](#dependencies-on-linux)
    - [Docker](#docker)
    - [Published binaries](#published-binaries)
- [Usage](#usage)
- [Development](#development)

<!-- /TOC -->

## General

## Installation

### Dotnet tool

imlp is packaged as dotnet tool. To use it:
- install the [.NET SDK]() for your system
- run either `dotnet tool install imlp` for a local or `dotnet tool install -g imlp` for a global installation
- you can now run the tool via `dotnet imlp ...`

#### Dependencies on linux

The CNTK nuget package sadly only works ootb on windows. On linux, several native dependencies must be installed and be added to environment variables. The necessary libraries are the CNTK binaries themselves and OpenMPI. 

- the necessary cntk binaries can be downloaded [here](https://cntk.azurewebsites.net/BinaryDrop/CNTK-2-7-Linux-64bit-CPU-Only.tar.gz)
- for installation of OpenMPI, please follow [this guide]()
- finally, apply this fix:
    - navigate to the location where you unpacked the cntk binaries
    - run `cp ./cntk/lib/Cntk.Core.CSBinding-2.7.so ./cntk/lib/libCntk.Core.CSBinding-2.7.dll`
    
### Docker

A Dockerfile can be downloaded under [releases]() or found [here](). It takes care of setting up the necessary native dependencies of CNTK. Due to that, the resulting image will be quite large (~7GB). Build the dockerfile by running `docker build . -t imlp`

### Published binaries

Download links to self-contained binaries can be found under [releases](). Linux users have to apply the same fixes as laid out under the [dotnet tool section](#dotnet-tool)

## Usage

```shell
USAGE: imlp [--help] [--sequence <sequence>] [--inputfile <inputFile>] [--outputfile <outputFile>] [--model <plant|nonplant>] [--plotdirectory <plotDirectory>] [--proteinheaderregex <proteinHeaderRegex>]
            [--verbosity <verbosity>]

OPTIONS:

    --sequence, -s <sequence>
                          A single, one-letter coded amino acid input sequence. Either --sequence (-s) or --inputFile (-f) must be specified.
    --inputfile, -f <inputFile>
                          Path to a fasta formatted input file that may contain multiple entries. Either --sequence (-s) or --inputFile (-f) must be specified.
    --outputfile, -o <outputFile>
                          (optional) Path to the desired output file, which will be tab separated (tsv). If not specified, output will be printed to stdout instead.
    --model, -m <plant|nonplant>
                          (optional) Model to use for prediction. Choose the one that is closest to your organism of interest: Either 'plant' or 'nonplant'. (default:nonplant)
    --plotdirectory, -p <plotDirectory>
                          (optional) Path to a directory to save plots to.
    --proteinheaderregex, -pr <proteinHeaderRegex>
                          (optional) Regular expression to extract protein names from fasta headers for the naming of plot output files. if not provided, plot files will be 'protein_{i}.html', where i is the
                          index in the input.
    --verbosity, -v <verbosity>
                          (optional) The verbosity of the logging process. 0(Silent) | 1(Error) | 2(Warn) | 3(Info) | >=4 : Debug | (default:1)
    --help                display this list of options.
```

## Development