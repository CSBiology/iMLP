module Domain
open NLog

open System.IO
open System.Reflection

let assembly = Assembly.GetExecutingAssembly()
let resnames = assembly.GetManifestResourceNames();

let private plantModelBuffer = 
    match Array.tryFind (fun (r:string) -> r.Contains("IMTS_Plant_AraMaizeRice.model")) resnames with
    | Some path -> 
        use stream = assembly.GetManifestResourceStream(path)
        let length = int stream.Length
        use bReader = new BinaryReader(stream)
        bReader.ReadBytes(length)

    | _ -> failwithf "could not plant load model from embedded ressources, check package integrity"

let private nonPlantModelBuffer = 
    match Array.tryFind (fun (r:string) -> r.Contains("IMTS_nonPlant_HumanMouseYeast.model")) resnames with
    | Some path -> 
        use stream = assembly.GetManifestResourceStream(path)
        let length = int stream.Length
        use bReader = new BinaryReader(stream)
        bReader.ReadBytes(length)

    | _ -> failwithf "could not plant load model from embedded ressources, check package integrity"


type Model =
    | Plant
    | NonPlant

    ///returns a byte array from the ressource stream. Either reads the original models from the manifest resources or reads the custom model from the given path
    static member getModelBuffer (model: Model) =
        let assembly = Assembly.GetExecutingAssembly()
        let resnames = assembly.GetManifestResourceNames();
        match model with
        | Plant     -> plantModelBuffer
        | NonPlant  -> nonPlantModelBuffer

type OutputKind =
    | STDOut
    | File of outputFile:string 
    | PlotsOnly of plotDirectory:string
    | FileAndPlots of outputFile:string*plotDirectory:string

let logLevelofInt (i:int) =
    match i with
    | 0 -> LogLevel.Off
    | 1 -> LogLevel.Error
    | 2 -> LogLevel.Warn
    | 3 -> LogLevel.Info
    | _ -> LogLevel.Debug

type SingleSequencePredictionArgs = {
    Sequence        : string
    OutputKind      : OutputKind
    Model           : Model
    FileNameHandler : int -> string -> string
} with
    static member create sequence outputKind model fileNameHandler =
        {
            Sequence        = sequence
            OutputKind      = outputKind
            Model           = model
            FileNameHandler = fileNameHandler
        }

type FastaFilePredictionArgs = {
    FilePath        : string
    OutputKind      : OutputKind
    Model           : Model
    FileNameHandler : int -> string -> string
} with
    static member create filePath outputKind model fileNameHandler =
        {
            FilePath        = filePath
            OutputKind      = outputKind
            Model           = model
            FileNameHandler = fileNameHandler
        }

type iMLPResult = {
    Header          : string
    Sequence        : string
    PropensityScores: float []
} with
    static member create header sequence scores = 
        {
            Header          = header
            Sequence        = sequence
            PropensityScores= scores
        }

    static member toCSV (includeHeader:bool) (separator:char) (r:iMLPResult) =

        let scoresString = (r.PropensityScores |> Array.fold (fun acc elem -> if acc = "" then string elem else sprintf "%s; %f" acc elem) "")
        let csv = $"{r.Header}{separator}{r.Sequence}{separator}{scoresString}"
        if includeHeader then 
            let header = $"Header{separator}Sequence{separator}iMTS-L_Propensity_Scores"
            $"{header}\r\n{csv}"
        else
            csv
       
    static member seqToCSV (includeHeader:bool) (separator:char) (results:seq<iMLPResult>) =
        if (not includeHeader) then
            results
            |> Seq.map (fun r -> iMLPResult.toCSV includeHeader separator r) 
        else
            results
            |> Seq.mapi (fun i r ->
                if i = 0 then
                    iMLPResult.toCSV true separator r
                else    
                    iMLPResult.toCSV false separator r
            )
        |> String.concat "\r\n"

type API = {
    SingleSequencePrediction : NLog.Logger -> SingleSequencePredictionArgs -> iMLPResult
    HandleSingleSequencePredictionResult : NLog.Logger -> SingleSequencePredictionArgs -> iMLPResult -> unit
    FastaFilePrediction : NLog.Logger -> FastaFilePredictionArgs -> iMLPResult []
    HandleFastaFilePredictionResult : NLog.Logger -> FastaFilePredictionArgs -> iMLPResult [] -> unit
} with
    static member create singleSequencePrediction singleHandler fastaFilePrediction fastaHandler =
        {
            SingleSequencePrediction                = singleSequencePrediction
            HandleSingleSequencePredictionResult    = singleHandler
            FastaFilePrediction                     = fastaFilePrediction
            HandleFastaFilePredictionResult         = fastaHandler
        }
