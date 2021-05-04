# iMLP

The imlp cli tool predicts iMTS-L propensity profiles for proteins of interest.

<!-- TOC -->

- [General](#general)
- [Installation](#installation)
    - [Best way for your OS](#best-way-for-your-os)
    - [Dotnet tool](#dotnet-tool)
        - [External dependencies on linux](#external-dependencies-on-linux)
        - [External dependencies on windows](#external-dependencies-on-windows)
    - [Docker](#docker)
    - [Published binaries](#published-binaries)
- [Usage](#usage)
- [Development](#development)
    - [build](#build)
    - [test](#test)
    - [publish self contained libraries](#publish-self-contained-libraries)
    - [docker](#docker)

<!-- /TOC -->

## General

## Installation

### Best way for your OS

Due to the problems arising with CNTK external dependency installation, we offer several containerized/self contained alternatives to the dotnet tool, 
which due to limitations by design will not work in a self contained manner.

- For linux distros and mac, we recommend to use the [docker container](#docker), which will take care of any external dependency management.
- For windows users, we recommend to use the self contained binaries published [here]() or the docker container

### Dotnet tool

imlp is packaged as dotnet tool. To use it:
- install the [.NET SDK](https://dotnet.microsoft.com/download) for your system
- run either `dotnet tool install imlp` (after initializing a local manifest via `dotnet new tool-manifest`) for a local or `dotnet tool install -g imlp` for a global installation
- you can now run the tool via `dotnet imlp ...` (for a local installation) or `imlp ...` (for a global installation)

#### External dependencies on linux

CNTK has some external dependencies that can not be published with the packaged tool.

- the necessary cntk binaries can be downloaded [here](https://cntk.azurewebsites.net/BinaryDrop/CNTK-2-7-Linux-64bit-CPU-Only.tar.gz)
- for installation of OpenMPI, please follow [this guide]()
- finally, apply this fix:
    - navigate to the location where you unpacked the cntk binaries
    - run `cp ./cntk/lib/Cntk.Core.CSBinding-2.7.so ./cntk/lib/libCntk.Core.CSBinding-2.7.dll`

all steps can be seen executed in out [Dockerfile](./Dockerfile)

#### External dependencies on windows

CNTK has some external dependencies that can not be published with the packaged tool.

Download the CNTK package here: https://cntk.ai/dlwc-2.7.html and add the extracted ./cntk/cntk folder to your path variable.
    
### Docker

A Dockerfile can be downloaded under [releases]() or found [here](./Dockerfile). It takes care of setting up the necessary native dependencies of CNTK. Due to that, the resulting image will be quite large (~7GB). Build the dockerfile by running `docker build . -t imlp`
imlp is installed as local tool usable under `/data` via `dotnet imlp` in the container.

### Published binaries

Download links to self-contained binaries can be found under [releases](). Linux users have to apply the same fixes as laid out under the [dotnet tool section](#external-dependencies-on-linux)

## Usage

```shell
USAGE: imlp [--help] [--sequence <sequence>] [--inputfile <inputFile>] [--outputfile <outputFile>] [--model <plant|nonplant>] [--plotdirectory <plotDirectory>] [--proteinheaderregex <proteinHeaderRegex>]
            [--verbosity <verbosity>]

OPTIONS:

    --sequence, -s <sequence>
                          A single, one-letter coded amino acid input sequence. Either --sequence (-s) 
                          or --inputFile (-f) must be specified.
    --inputfile, -f <inputFile>
                          Path to a fasta formatted input file that may contain multiple entries. Either 
                          --sequence (-s) or --inputFile (-f) must be specified.
    --outputfile, -o <outputFile>
                          (optional) Path to the desired output file, which will be tab separated (tsv). 
                          If not specified, output will be printed to stdout instead.
    --model, -m <plant|nonplant>
                          (optional) Model to use for prediction. Choose the one that is closest to your 
                          organism of interest: Either 'plant' or 'nonplant'. (default:nonplant)
    --plotdirectory, -p <plotDirectory>
                          (optional) Path to a directory to save plots to.
    --proteinheaderregex, -pr <proteinHeaderRegex>
                          (optional) Regular expression to extract protein names from fasta headers for 
                          the naming of plot output files. if not provided, plot files will be 'protein_{i}.html', 
                          where i is the index in the input.
    --verbosity, -v <verbosity>
                          (optional) The verbosity of the logging process. 0(Silent) | 1(Error) | 2(Warn) | 3(Info) 
                          | >=4 : Debug | (default:1)
    --help                display this list of options.
```

## Development

### build 

- `dotnet tool restore` (once)
- `dotnet fake build`

### test

- `dotnet tool restore` (once)
- `dotnet fake build -t testpackagedtool` (You need to have external dependencies ([win](#external-dependencies-on-windows)/[linux](#external-dependencies-on-linux)) installed)

### publish self contained libraries

- `dotnet tool restore` (once)
- `dotnet fake build -t publishbinaries` will publish both `win-x64` and `linux-x64` binaries. 

### docker

- set the correct imlp version in the containers `IMLP_VERSION` argument
- `docker build . -t imlp`