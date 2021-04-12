module CLIArgs

open Argu
open Domain

type IMLP_CLIArgs =
    | [<Unique>] [<AltCommandLine("-s")>] (*[<Last>]*) Sequence of sequence: string
    | [<Unique>] [<AltCommandLine("-f")>] (*[<Last>]*) InputFile of inputFile: string
    | [<Unique>] [<AltCommandLine("-o")>] OutputFile of outputFile: string 
    | [<Unique>] [<AltCommandLine("-v")>] Verbosity of verbosity: int
    //| [<Unique>] [<AltCommandLine("-l")>] LogFile of logFile: string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Sequence _    -> "A single, one-letter coded amino acid input sequence. Either --sequence (-s) or --inputFile (-f) must be specified."
            | InputFile _   -> "Path to a fasta formatted input file that may contain multiple entries. Either --sequence (-s) or --inputFile (-f) must be specified."
            | OutputFile _  -> "(optional) Path to the desired output file, which will be tab separated (tsv). If not specified, output will be printed to stdout instead."
            | Verbosity _   -> "(optional) The verbosity of the logging process. 0(Silent) | 1(Error) | 2(Warn) | 3(Info) | >=4 : Debug | (default:1)"
            //| LogFile _     -> "(optional) Path to a file to save logs to."