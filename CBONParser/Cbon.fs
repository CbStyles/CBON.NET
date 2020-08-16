namespace CbStyles.Cbon
open CbStyles.Cbon.Utils

type CbVal = 
    | Bool of bool
    | Num of decimal
    | Str of string
    | Arr of CbVal MutList
    | Obj of MutMap<string, CbVal>
    | Union of Tag: string * Value: CbVal
