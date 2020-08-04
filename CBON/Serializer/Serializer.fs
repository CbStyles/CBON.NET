module CbStyle.Cbon.Serializer
open CbStyle.Cbon.Deserialize
open CbStyle.Cbon.Parser

let DeserializeArray<'T>(code: string) = 
    let ast = Parser.parser (Reader.reader code)
    let t = typeof<'T>
    check_de<'T> t
    arr_de<'T> t ast

let Deserialize<'T>(code: string) = 
    let ast = Parser.parser (Reader.reader code)
    let t = typeof<'T>
    check_de<'T> t
    if ast.Count = 0 then raise (DeserializeError "Nothing to deserialize")
    item_de<'T> t ast.[0]