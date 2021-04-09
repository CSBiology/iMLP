module Domain

type OutputKind =
    | STDOut
    | File of string

type SingleSequencePredictionArgs = {
    Sequence: string
    OutputKind: OutputKind
} with
    static member create sequence outputKind =
        {
            Sequence    = sequence
            OutputKind  = outputKind
        }

type FastaFilePredictionArgs = {
    FilePath: string
    OutputKind: OutputKind
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

type API = {
    SingleSequencePrediction : SingleSequencePredictionArgs -> iMLPResult
    FastaFilePrediction : FastaFilePredictionArgs ->  iMLPResult []
} with
    static member create singleSequencePrediction fastaFilePrediction =
        {
            SingleSequencePrediction    = singleSequencePrediction
            FastaFilePrediction         = fastaFilePrediction
        }
