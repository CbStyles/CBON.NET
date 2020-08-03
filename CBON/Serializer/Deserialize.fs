namespace CbStyle.Cbon
open CbStyle.Cbon.Utils
open CbStyle.Cbon.Parser
open System.Linq
open System
open System.Runtime.Serialization

//type DeserializeOptions =
//    |

type DeserializeError(msg) = inherit System.Exception(msg)
type DeserializeTypeError(typename, tname) =inherit DeserializeError("Cannot deserialize <" + typename + "> to " + tname)


module internal rec Deserialize =
    let de_arr<'T> (ast: CbAst MutList) = 
        if ast.Count = 0 then new MutList<'T>() else
        let t = typeof<'T>
        if not t.IsSerializable then raise (DeserializeError "This type cannot be deserialized")
        if t.IsInterface then raise (DeserializeError "Cannot deserialize interface")
        if t.IsAbstract then raise (DeserializeError "Cannot deserialize abstract class")
        if t.IsCOMObject then raise (DeserializeError "Cannot deserialize COM object")
        if t.IsPointer then raise (DeserializeError "Cannot deserialize pointer")
        if t.IsPrimitive then Enumerable.Select(ast, fun v -> de_rimitive t v) |> Enumerable.ToList else
        //let obj = FormatterServices.GetUninitializedObject(t)
        failwith "todo"
    let de_rimitive<'T> (t: Type) (ast: CbAst) : 'T = 
        match ast with
        | Bool v -> 
            if typeof<bool>.IsAssignableFrom(t) then downcast (v :> obj)
            else raise (DeserializeTypeError("bool", t.FullName))
        | Num v ->
            if typeof<int8>.IsAssignableFrom(t) then downcast (v.I8() :> obj)
            else if typeof<int16>.IsAssignableFrom(t) then downcast (v.I16() :> obj)
            else if typeof<int32>.IsAssignableFrom(t) then downcast (v.I32() :> obj)
            else if typeof<int64>.IsAssignableFrom(t) then downcast (v.I64() :> obj)
            else if typeof<uint8>.IsAssignableFrom(t) then downcast (v.U8() :> obj)
            else if typeof<uint16>.IsAssignableFrom(t) then downcast (v.U16() :> obj)
            else if typeof<uint32>.IsAssignableFrom(t) then downcast (v.U32() :> obj)
            else if typeof<uint64>.IsAssignableFrom(t) then downcast (v.U64() :> obj)
            else if typeof<float32>.IsAssignableFrom(t) then downcast (v.F32() :> obj)
            else if typeof<float>.IsAssignableFrom(t) then downcast (v.F64() :> obj)
            else if typeof<decimal>.IsAssignableFrom(t) then downcast (v.F128() :> obj)
            else raise (DeserializeTypeError("number", t.FullName))
        | Hex v ->
            if typeof<int8>.IsAssignableFrom(t) then downcast (v.I8() :> obj)
            else if typeof<int16>.IsAssignableFrom(t) then downcast (v.I16() :> obj)
            else if typeof<int32>.IsAssignableFrom(t) then downcast (v.I32() :> obj)
            else if typeof<int64>.IsAssignableFrom(t) then downcast (v.I64() :> obj)
            else if typeof<uint8>.IsAssignableFrom(t) then downcast (v.U8() :> obj)
            else if typeof<uint16>.IsAssignableFrom(t) then downcast (v.U16() :> obj)
            else if typeof<uint32>.IsAssignableFrom(t) then downcast (v.U32() :> obj)
            else if typeof<uint64>.IsAssignableFrom(t) then downcast (v.U64() :> obj)
            else if typeof<float32>.IsAssignableFrom(t) then downcast (v.F32() :> obj)
            else if typeof<float>.IsAssignableFrom(t) then downcast (v.F64() :> obj)
            else if typeof<decimal>.IsAssignableFrom(t) then downcast (v.F128() :> obj)
            else raise (DeserializeTypeError("integer", t.FullName))
        | Str v ->
            if typeof<char>.IsAssignableFrom(t) && v.Length = 1 then downcast (v.[0] :> obj)
            else raise (DeserializeTypeError("string", t.FullName))
        | Null -> raise (DeserializeTypeError("null", t.FullName))
        | Arr _ -> raise (DeserializeTypeError("array", t.FullName))
        | Obj _ -> raise (DeserializeTypeError("object", t.FullName))