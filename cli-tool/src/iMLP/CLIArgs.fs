module CLIArgs

open Argu
open Domain

type OrganismModel = 
    | Plant = 1
    | NonPlant = 2

type IMLP_CLIArgs =
    | [<Unique>] [<AltCommandLine("-s")>] (*[<Last>]*) Sequence of sequence: string
    | [<Unique>] [<AltCommandLine("-f")>] (*[<Last>]*) InputFile of inputFile: string
    | [<Unique>] [<AltCommandLine("-o")>] OutputFile of outputFile: string 
    | [<Unique>] [<AltCommandLine("-m")>] Model of OrganismModel 
    | [<Unique>] [<AltCommandLine("-p")>] PlotDirectory of plotDirectory: string
    | [<Unique>] [<AltCommandLine("-pr")>] ProteinHeaderRegex of proteinHeaderRegex: string
    | [<Unique>] [<AltCommandLine("-v")>] Verbosity of verbosity: int
    //| [<Unique>] [<AltCommandLine("-l")>] LogFile of logFile: string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Sequence _            -> "A single, one-letter coded amino acid input sequence. Either --sequence (-s) or --inputFile (-f) must be specified."
            | InputFile _           -> "Path to a fasta formatted input file that may contain multiple entries. Either --sequence (-s) or --inputFile (-f) must be specified."
            | OutputFile _          -> "(optional) Path to the desired output file, which will be tab separated (tsv). If not specified, output will be printed to stdout instead."
            | Model _               -> "(optional) Model to use for prediction. Choose the one that is closest to your organism of interest: Either 'plant' or 'nonplant'. (default:nonplant)"
            | PlotDirectory _       -> "(optional) Path to a directory to save plots to."
            | ProteinHeaderRegex _  -> "(optional) Regular expression to extract protein names from fasta headers for the naming of plot output files. if not provided, plot files will be 'protein_{i}.html', where i is the index in the input."
            | Verbosity _           -> "(optional) The verbosity of the logging process. 0(Silent) | 1(Error) | 2(Warn) | 3(Info) | >=4 : Debug | (default:1)"
            //| LogFile _     -> "(optional) Path to a file to save logs to."