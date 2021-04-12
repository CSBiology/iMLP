module Domain
open NLog

type OutputKind =
    | STDOut
    | File of string

let logLevelofInt (i:int) =
    match i with
    | 0 -> LogLevel.Off
    | 1 -> LogLevel.Error
    | 2 -> LogLevel.Warn
    | 3 -> LogLevel.Info
    | _ -> LogLevel.Debug


type SingleSequencePredictionArgs = {
    Sequence    : string
    OutputKind  : OutputKind
} with
    static member create sequence outputKind =
        {
            Sequence    = sequence
            OutputKind  = outputKind
        }

type FastaFilePredictionArgs = {
    FilePath    : string
    OutputKind  : OutputKind
} with
    static member create filePath outputKind =
        {
            FilePath    = filePath
            OutputKind  = outputKind
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
    HandleSingleSequencePredictionResult : NLog.Logger -> OutputKind -> iMLPResult -> unit
    FastaFilePrediction : NLog.Logger -> FastaFilePredictionArgs -> iMLPResult []
    HandleFastaFilePredictionResult : NLog.Logger -> OutputKind -> iMLPResult [] -> unit
} with
    static member create singleSequencePrediction singleHandler fastaFilePrediction fastaHandler =
        {
            SingleSequencePrediction                = singleSequencePrediction
            HandleSingleSequencePredictionResult    = singleHandler
            FastaFilePrediction                     = fastaFilePrediction
            HandleFastaFilePredictionResult         = fastaHandler
        }
