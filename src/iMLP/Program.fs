// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open Argu
open NLog
open CLIArgs
open Domain
open API

[<EntryPoint>]
let main argv =

    let config = new NLog.Config.LoggingConfiguration();
    let logconsole = new NLog.Targets.ConsoleTarget("logconsole");
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

        //let logFile = results.TryGetResult IMLP_CLIArgs.LogFile

        //match logFile with
        //| Some logFile ->
        //    let logfile = new NLog.Targets.FileTarget("logfile")
        //    printfn "%s/logs/${level}.log" __SOURCE_DIRECTORY__
        //    logfile.FileName <- Layouts.Layout.FromString((sprintf "%s/logs/${level}.log" __SOURCE_DIRECTORY__))
        //    config.AddRule(verbosity,LogLevel.Off,logconsole)
        //    NLog.LogManager.Configuration <- config
        //    let logger = NLog.LogManager.GetCurrentClassLogger()
        //    logger.Debug(sprintf "LogFile: %s" logFile)
        // | None -> 
        //    NLog.LogManager.Configuration <- config

        let logger = NLog.LogManager.GetCurrentClassLogger()

        logger.Debug(sprintf "Log level: %A" verbosity)

        let outputKind =
            match results.TryGetResult (IMLP_CLIArgs.OutputFile) with
            | Some f -> OutputKind.File f
            | None ->  OutputKind.STDOut

        logger.Debug(sprintf "Output Kind: %A" outputKind)


        match ((results.TryGetResult(IMLP_CLIArgs.Sequence)),((results.TryGetResult(IMLP_CLIArgs.OutputFile)))) with

        | Some _, Some _ -> failwith ""
        | Some sequence, None -> 
                
            logger.Debug(sprintf "Input sequence: %s" sequence)

            let apiArgs = 
                SingleSequencePredictionArgs.create
                    sequence
                    outputKind

            API.singleSequencePrediction logger apiArgs
            |> API.handlesingleSequencePredictionResult logger outputKind

        | None, Some file -> 

            logger.Debug(sprintf "Input sequence: %s" file)
                
            let apiArgs = 
                FastaFilePredictionArgs.create
                    file
                    outputKind
                
            API.fastaFilePrediction logger apiArgs
            |> API.handlefastaFilePredictionResult logger outputKind

        | _ -> failwith ""

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