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
open AminoAcids
open CNTK
open System.Collections.Generic

module Directory =
    let ensure path =
        if not (Directory.Exists(path)) then Directory.CreateDirectory(path) |> ignore

module Prediction =

    type targetPOut = {
        QID         : int
        AA          : AminoAcid
        AAIdx       : int
        TargetPScore: float 
        }
    
    let aminoAcidSetStandard =
        [
            AminoAcid.Ala
            AminoAcid.Cys
            AminoAcid.Asp
            AminoAcid.Glu
            AminoAcid.Phe
            AminoAcid.Gly
            AminoAcid.His
            AminoAcid.Ile
            AminoAcid.Lys
            AminoAcid.Leu
            AminoAcid.Met
            AminoAcid.Asn
            //AminoAcid.Pyl
            AminoAcid.Pro
            AminoAcid.Gln
            AminoAcid.Arg
            AminoAcid.Ser
            AminoAcid.Thr
            //AminoAcid.Sel
            AminoAcid.Val
            AminoAcid.Trp
            AminoAcid.Tyr
   
            AminoAcid.Xaa
            AminoAcid.Asx
            AminoAcid.Sel
            AminoAcid.Glx
       
        ]

    
    let aminoAcidToVectorIdx = 
        aminoAcidSetStandard        
        |> List.mapi (fun i x -> x,i)
        |> Map.ofList
          
    let predict (modelBuffer:byte[]) featureData =

        let device = DeviceDescriptor.CPUDevice
    
        let PeptidePredictor : Function = 
            Function.Load(modelBuffer, device)
    
        let x' = 
            PeptidePredictor.Parameters()
            |> Seq.toList
            |> fun x -> x.[x.Length-1].Shape
    
        ///////////Input 
        let inputVar: Variable = 
            PeptidePredictor.Arguments.Item 0
    
        let inputShape = inputVar.Shape
    
        let CNCRepresentationOf (protein:targetPOut []) =
            /// 
            let rowIndices            = new ResizeArray<int>()
            /// new word // should in my case be 1
            let colStarts             = new ResizeArray<int>()
            let nonZeroValues         = new ResizeArray<float32>()
            protein
            |> Array.iteri (fun i x -> 
                            nonZeroValues.Add (float32 1.)
                            rowIndices.Add x.AAIdx
                            colStarts.Add i
                            )
            colStarts.Add protein.Length
            let rowIndices    = rowIndices |> Array.ofSeq
            let nonZeroValues = nonZeroValues |> Array.ofSeq
            let colStarts = colStarts |> Array.ofSeq
            Value.CreateSequence<float32>(24,protein.Length,colStarts,rowIndices,nonZeroValues,device)
  
        //inputShape    
        let inputValues = CNCRepresentationOf featureData
    
        let inputMap = new Dictionary<Variable,Value>()
        inputMap.Add(inputVar,inputValues)
    
        let outputVar : Variable = PeptidePredictor.Output
    
        let outputMap = new Dictionary<Variable,Value>()
        outputMap.Add(outputVar,null)
    
        PeptidePredictor.Evaluate(inputMap,outputMap,device)
    
        let outputValues = outputMap.[outputVar]
    
        let preds = 
            outputValues.GetDenseData<float32>(outputVar)
            |> Seq.concat
            |> Array.ofSeq
    
    
        let xF,yF = 
            Array.mapi (fun (i) x -> i,x.TargetPScore) (featureData) 
            |> Array.unzip
    
        let xP,yP = 
            Array.mapi (fun (i) x -> i,float x) (preds) 
            |> Array.unzip
       
        yF, yP
    
    let bioSeqToInput (input: BioSeq.BioSeq<AminoAcids.AminoAcid>) :targetPOut[] = 
            input 
            |> Seq.choose (fun x -> 
                try
                let idx = aminoAcidToVectorIdx.[x]
                {
                QID          = 0
                AA           = x 
                AAIdx        = idx 
                TargetPScore = nan
                }
                |> Some
                with
                | _-> None
                )
            |> Array.ofSeq
    
    let predictFinal (modelBuffer:byte[]) (sequence:string) = 
        let bsequence = BioSeq.ofAminoAcidString sequence
        let _, predictedTraces = predict modelBuffer (bioSeqToInput bsequence) 
        predictedTraces

    let predictIMTSLPropensityForSequence (modelBuffer:byte[]) (inputSequence: FastA.FastaItem<string>) =
        predictFinal modelBuffer inputSequence.Sequence

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
        try
            preprocessedFSA
            |> Prediction.predictIMTSLPropensityForSequence (args.Model |> Model.getModelBuffer)

        with e as exn ->
            logger.Error(e)
            failwith ""

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
        result.PropensityScores
        |> Plots.plotPropensity $"iMTS-L propensity profile of {result.Sequence}"
        |> Chart.SaveHtmlAs $"{plotDir}/protein_0"

        printfn $"{result |> iMLPResult.toCSV true '\t'}"

    | OutputKind.File outFile -> 
        result 
        |> iMLPResult.toCSV true '\t'
        |> fun resString -> File.WriteAllText(outFile,resString)
        logger.Debug( $"prediction result written to ${outFile}")

    | OutputKind.FileAndPlots (outFile,plotDir) -> 
        result 
        |> iMLPResult.toCSV true '\t'
        |> fun resString -> File.WriteAllText(outFile,resString)

        logger.Debug( $"prediction result written to ${outFile}")

        result.PropensityScores
        |> Plots.plotPropensity $"iMTS-L propensity profile of {result.Sequence}"
        |> Chart.SaveHtmlAs $"{plotDir}/protein_0"
        printfn $"{result |> iMLPResult.toCSV true '\t'}"

        logger.Debug( $"plot written to ${plotDir}")

let fastaFilePrediction (logger:NLog.Logger) (args:FastaFilePredictionArgs) : iMLPResult [] = 
    
    let preprocessedFSA = 
        args.FilePath
        |> FastA.fromFile (Array.ofSeq >> String.fromCharArray)
        |> Seq.map (FastaPreprocessing.filterIllegalCharacters logger)

    let results = 
        preprocessedFSA
        |> Seq.map (fun fsa ->
            iMLPResult.create
                fsa.Header
                fsa.Sequence
                (Prediction.predictIMTSLPropensityForSequence (args.Model |> Model.getModelBuffer) fsa)
        )

    logger.Debug($"Successful prediction for {args.FilePath}")

    results
    |> Array.ofSeq

let handlefastaFilePredictionResult (logger:NLog.Logger) (args:FastaFilePredictionArgs) (results:iMLPResult []) : unit = 
    
    match args.OutputKind with
        | OutputKind.STDOut -> 
           printfn $"{results |> iMLPResult.seqToCSV true '\t'}"

        | OutputKind.PlotsOnly plotDir -> 

            Directory.ensure plotDir

            results
            |> Array.iteri (fun i r ->
                r.PropensityScores
                |> Plots.plotPropensity $"iMTS-L propensity profile of {r.Header}"
                |> Chart.SaveHtmlAs ($"{plotDir}/{args.FileNameHandler i r.Header}")
            )

            printfn $"{results |> iMLPResult.seqToCSV true '\t'}"

        | OutputKind.File outFile -> 
            results
            |> iMLPResult.seqToCSV true '\t'
            |> fun resString -> File.WriteAllText(outFile,resString)
            logger.Debug( $"prediction result written to ${outFile}")

        | OutputKind.FileAndPlots (outFile,plotDir) -> 
            
            Directory.ensure plotDir

            results
            |> iMLPResult.seqToCSV true '\t'
            |> fun resString -> File.WriteAllText(outFile,resString)

            logger.Debug( $"prediction result written to ${outFile}")

            results
            |> Array.iteri (fun i r ->
                r.PropensityScores
                |> Plots.plotPropensity $"iMTS-L propensity profile of {r.Header}"
                |> Chart.SaveHtmlAs ($"{plotDir}/{args.FileNameHandler i r.Header}")
            )

            logger.Debug( $"plot written to ${plotDir}")
    

let iMLP_API =
    API.create
        singleSequencePrediction
        handlesingleSequencePredictionResult
        fastaFilePrediction
        handlefastaFilePredictionResult