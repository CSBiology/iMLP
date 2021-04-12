module API

open NLog
open Domain

let singleSequencePrediction (logger:NLog.Logger) (args:SingleSequencePredictionArgs) : iMLPResult = raise (new System.NotImplementedException())

let handlesingleSequencePredictionResult (logger:NLog.Logger) (outputKind:OutputKind) (result:iMLPResult) : unit = raise (new System.NotImplementedException())

let fastaFilePrediction (logger:NLog.Logger) (args:FastaFilePredictionArgs) : iMLPResult [] = raise (new System.NotImplementedException())

let handlefastaFilePredictionResult (logger:NLog.Logger) (outputKind:OutputKind) (results:iMLPResult []) : unit = raise (new System.NotImplementedException())
    

let iMLP_API =
    API.create
        singleSequencePrediction
        handlesingleSequencePredictionResult
        fastaFilePrediction
        handlefastaFilePredictionResult