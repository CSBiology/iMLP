module API

open System
open System.IO
open NLog
open Domain
open FSharpAux
open FSharpAux.IO
open BioFSharp
open BioFSharp.IO
open Plotly.NET

module Prediction =

    let predictIMTSLPropensityForSequence (inputSequence: FastA.FastaItem<string>) =
        [|1.;2.;3.|]

module Plots =
    
    let xAxis title (zeroline : bool)=
        Axis.LinearAxis.init
            (
                Title=title,
                Showgrid=false,
                Showline=true,
                Mirror=StyleParam.Mirror.All,
                Zeroline=zeroline,
                Tickmode=StyleParam.TickMode.Auto,
                Ticks= StyleParam.TickOptions.Inside,
                Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=18),
                Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=18)
            )

    let yAxis title =
        Axis.LinearAxis.init
            (
                Title=title,
                Showgrid=false,
                Showline=true,
                Mirror=StyleParam.Mirror.All,
                Tickmode=StyleParam.TickMode.Auto,
                Ticks= StyleParam.TickOptions.Inside,
                Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=18.),
                Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=18.)
            )
            
    
    let insideLegend () =
        Legend.init(
            X = 0.02,
            Y = -0.98,
            TraceOrder = StyleParam.TraceOrder.Normal,
            BGColor = "rgba(222, 235, 247, 0.6)",
            BorderColor = "rgb(68, 84, 106)",
            Borderwidth = 2
        )
        
    
    let layout = fun () ->
        let la = 
            Layout.init(Paper_bgcolor="rgba(0,0,0,0)",Plot_bgcolor="white")
        la?legend <- insideLegend ()
        la
        
    
    let csbDarkBlue = FSharpAux.Colors.fromRgb 68 84 106

    let csbOrange = FSharpAux.Colors.fromRgb 237 125 49
    
    let plotPropensity (name:string) (scores: float array) =
        let vals = 
            scores
            |> Array.mapi (fun i x -> (i+1,x))
        [
            Chart.SplineArea(vals,Color = "rgb(237, 125, 49, 0.9)",Name = "Propensity Score",Width = 2.5)

        ]
        |> Chart.Combine
        |> Chart.withY_Axis(xAxis "Score" true)
        |> Chart.withX_Axis(yAxis "Index of AminoAcid")
        |> Chart.withTitle(name)
        |> Chart.withLayout(Layout.init(Paper_bgcolor="rgba(0,0,0,0)",Plot_bgcolor="white"))
        |> Chart.withSize(600.,600.)

module FastaPreprocessing =

    ///Returns the input sequence without characters that cant be used as input for targetP
    let filterIllegalCharacters (logger:NLog.Logger) (fsa:FastA.FastaItem<string>) =

        let containsIllegalCharacter = fsa .Sequence |> String.exists(fun aa ->  (aa = '*' || aa = '-' ))

        if containsIllegalCharacter then logger.Warn("Input sequence contains one or more illegal characters (* or -) and will be filtered.")

        {fsa with 
            Sequence = 
                fsa.Sequence 
                |> String.filter (fun aa ->  
                    not (aa = '*' || aa = '-' )
                )
        }

let singleSequencePrediction (logger:NLog.Logger) (args:SingleSequencePredictionArgs) : iMLPResult = 

    let header = "Protein from STDIN"

    let preprocessedFSA = 
        FastA.createFastaItem
            header
            args.Sequence
        |> FastaPreprocessing.filterIllegalCharacters logger

    let prediction = 
        preprocessedFSA
        |> Prediction.predictIMTSLPropensityForSequence

    let result = 
        iMLPResult.create  
            preprocessedFSA.Header
            preprocessedFSA.Sequence
            prediction

    logger.Debug($"Successful prediction for {header} : {preprocessedFSA.Sequence}")

    result

let handlesingleSequencePredictionResult (logger:NLog.Logger) (args:SingleSequencePredictionArgs) (result:iMLPResult) : unit = 
    
    match args.OutputKind with
    | OutputKind.STDOut -> 
        printfn $"{result |> iMLPResult.toCSV true '\t'}"

    | OutputKind.PlotsOnly plotDir -> 
        ()
        printfn $"{result |> iMLPResult.toCSV true '\t'}"

    | OutputKind.File outFile -> 
        result 
        |> iMLPResult.toCSV true '\t'
        |> fun resString -> File.WriteAllText(outFile,resString)
        logger.Debug( $"prediction result written to ${outFile}")

    | OutputKind.FileAndPlots (outFile,plotDir) -> 
        logger.Debug( $"prediction result written to ${outFile}")
        logger.Debug( $"plot written to ${plotDir}")

let fastaFilePrediction (logger:NLog.Logger) (args:FastaFilePredictionArgs) : iMLPResult [] = raise (new System.NotImplementedException())

let handlefastaFilePredictionResult (logger:NLog.Logger) (args:FastaFilePredictionArgs) (results:iMLPResult []) : unit = raise (new System.NotImplementedException())
    

let iMLP_API =
    API.create
        singleSequencePrediction
        handlesingleSequencePredictionResult
        fastaFilePrediction
        handlefastaFilePredictionResult