// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.IO
open System.Text
open System.Text.RegularExpressions
open Argu
open NLog
open NLog.Targets
open CLIArgs
open Domain
open API

[<EntryPoint>]
let main argv =

    let config = new NLog.Config.LoggingConfiguration()
    let logconsole = new NLog.Targets.ColoredConsoleTarget("logconsole")
    let helpMessageheader = """
iMLP
----
    
iMLP is a cli tool for prediction of iMTS-L propensities of proteins of interest.

Learn more about iMTS-L here: [LINK]

"""
    try 
        let parser = ArgumentParser.Create<IMLP_CLIArgs>("imlp",helpTextMessage=helpMessageheader)
        let results = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true) 
        
        let verbosity = 
            results.GetResult (IMLP_CLIArgs.Verbosity, defaultValue = 1)
            |> logLevelofInt

        config.AddRule(verbosity,LogLevel.Off,logconsole)
        NLog.LogManager.Configuration <- config

        let logger = NLog.LogManager.GetCurrentClassLogger()

        logger.Debug($"Log level: {verbosity}")
        logger.Debug($"parsed arguments: {results.GetAllResults()}")

        let outputKind =
            match ((results.TryGetResult IMLP_CLIArgs.OutputFile),((results.TryGetResult IMLP_CLIArgs.PlotDirectory))) with
            | Some outputFile, Some plotDirectory -> OutputKind.FileAndPlots (outputFile,plotDirectory)
            | Some outputFile, None -> OutputKind.File outputFile
            | None, Some plotDirectory -> OutputKind.PlotsOnly plotDirectory
            | None,None ->  OutputKind.STDOut

        logger.Debug($"Output Kind: {outputKind}")

        let fileNameHandler = 
            match (results.TryGetResult IMLP_CLIArgs.ProteinHeaderRegex) with
            | Some pattern -> 
                logger.Debug($"using custom protein header regex {pattern}")
                let headerRegex = new Regex(pattern)
                fun (index:int) (header:string) -> 
                    if headerRegex.Match(header).Success then
                        headerRegex.Match(header).Value
                    else 
                        logger.Warn($"{header} has produced no matches based on input regex {pattern}. Using protein_{index} as filename.")
                        $"protein_{index}"
            | None ->
                fun (index:int) (header:string) -> $"protein_{index}"
                

        match ((results.TryGetResult(IMLP_CLIArgs.Sequence)),((results.TryGetResult(IMLP_CLIArgs.InputFile)))) with

        | Some _, Some _ -> failwith "Only one of --sequence (-s) or --inputFile (-f) may be defined at once."
        | Some sequence, None -> 
                
            logger.Debug($"Input sequence: {sequence}")

            let apiArgs = 
                SingleSequencePredictionArgs.create
                    sequence
                    outputKind
                    fileNameHandler

            logger.Debug($"apiArgs:\r\n{apiArgs}")

            API.singleSequencePrediction logger apiArgs
            |> API.handlesingleSequencePredictionResult logger apiArgs

        | None, Some file -> 

            logger.Debug(sprintf "Input file: %s" file)
                
            let apiArgs = 
                FastaFilePredictionArgs.create
                    file
                    outputKind
                    fileNameHandler

            logger.Debug($"apiArgs:\r\n{apiArgs}")
                
            API.fastaFilePrediction logger apiArgs
            |> API.handlefastaFilePredictionResult logger apiArgs

        | _ -> failwith "No input provided."

        0

    with 
    | :? Argu.ArguParseException as e ->
        match e.ErrorCode with
        | ErrorCode.HelpText ->
            printfn "%s" e.Message
            0
        | _ ->
            NLog.LogManager.Configuration <- config
            let logger = NLog.LogManager.GetCurrentClassLogger()
            logger.Error(e.Message)
            -1
    | e as exn ->
        NLog.LogManager.Configuration <- config
        let logger = NLog.LogManager.GetCurrentClassLogger()
        logger.Error(e.Message)
        -1