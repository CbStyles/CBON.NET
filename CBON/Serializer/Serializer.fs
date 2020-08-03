module CbStyle.Cbon.Serializer
open CbStyle.Cbon.Deserialize
open CbStyle.Cbon.Parser

let DeserializeArray<'T>(code: string) = 
    let ast = Parser.parser (Reader.reader code)
    de_arr<'T> ast