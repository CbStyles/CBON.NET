namespace CbStyle.Cbon
open CbStyle.Cbon.Utils
open CbStyle.Cbon.Parser
open System.Linq
open System
open System.Runtime.Serialization
open System.Reflection

//type DeserializeOptions =
//    |

type DeserializeError(msg) = inherit System.Exception(msg)
type DeserializeTypeError(typename, tname) =inherit DeserializeError("Cannot deserialize <" + typename + "> to " + tname)


module internal rec Deserialize =
    let check_de<'T> (t: Type) = 
        if not t.IsSerializable then raise (DeserializeError "This type cannot be deserialized")
        if t.IsInterface then raise (DeserializeError "Cannot deserialize interface")
        if t.IsAbstract then raise (DeserializeError "Cannot deserialize abstract class")
        if t.IsCOMObject then raise (DeserializeError "Cannot deserialize COM object")
        if t.IsPointer then raise (DeserializeError "Cannot deserialize pointer")
    let arr_de<'T> (t: Type) (ast: CbAst MutList) : 'T MutList = 
        if ast.Count = 0 then new MutList<'T>() else
        Enumerable.Select(ast, fun v -> item_de t v) |> Enumerable.ToList
    let item_de<'T> (t: Type) (ast: CbAst) : 'T = downcast (item_de_obj t ast)
    let item_de_obj (t: Type) (ast: CbAst) : obj =
        if t.IsPrimitive then de_rimitive t ast else
        if typeof<string>.IsAssignableFrom(t) then de_str t ast else
        match ast with
        | Obj v -> de_obj t v
        | Arr v -> failwith "todo"
        | Null -> null
        | Str _ -> raise (DeserializeTypeError("string", t.FullName))
        | Num _ -> raise (DeserializeTypeError("number", t.FullName))
        | Hex _ -> raise (DeserializeTypeError("integer", t.FullName))
        | Bool _ -> raise (DeserializeTypeError("bool", t.FullName))
    let de_str<'T> (t: Type) (ast: CbAst) : obj=
        match ast with
        | Str v -> v :> obj
        | Bool v -> (if v then "true" else "false") :> obj
        | Num v -> v.raw :> obj
        | Hex v -> v.raw :> obj
        | Null -> null :> obj
        | Arr _ -> raise (DeserializeTypeError("array", t.FullName))
        | Obj _ -> raise (DeserializeTypeError("object", t.FullName))
    let de_rimitive<'T> (t: Type) (ast: CbAst) : obj = 
        match ast with
        | Bool v -> 
            if typeof<bool>.IsAssignableFrom(t) then v :> obj
            else raise (DeserializeTypeError("bool", t.FullName))
        | Num v ->
            if typeof<int8>.IsAssignableFrom(t) then v.I8() :> obj
            else if typeof<int16>.IsAssignableFrom(t) then v.I16() :> obj
            else if typeof<int32>.IsAssignableFrom(t) then v.I32() :> obj
            else if typeof<int64>.IsAssignableFrom(t) then v.I64() :> obj
            else if typeof<uint8>.IsAssignableFrom(t) then v.U8() :> obj
            else if typeof<uint16>.IsAssignableFrom(t) then v.U16() :> obj
            else if typeof<uint32>.IsAssignableFrom(t) then v.U32() :> obj
            else if typeof<uint64>.IsAssignableFrom(t) then v.U64() :> obj
            else if typeof<float32>.IsAssignableFrom(t) then v.F32() :> obj
            else if typeof<float>.IsAssignableFrom(t) then v.F64() :> obj
            else if typeof<decimal>.IsAssignableFrom(t) then v.F128() :> obj
            else if typeof<bool>.IsAssignableFrom(t) then 
                if v.raw = "0" then false :> obj
                else if v.raw = "1" then true :> obj
                else raise (DeserializeTypeError("number", t.FullName))
            else raise (DeserializeTypeError("number", t.FullName))
        | Hex v ->
            if typeof<int8>.IsAssignableFrom(t) then v.I8() :> obj
            else if typeof<int16>.IsAssignableFrom(t) then v.I16() :> obj
            else if typeof<int32>.IsAssignableFrom(t) then v.I32() :> obj
            else if typeof<int64>.IsAssignableFrom(t) then v.I64() :> obj
            else if typeof<uint8>.IsAssignableFrom(t) then v.U8() :> obj
            else if typeof<uint16>.IsAssignableFrom(t) then v.U16() :> obj
            else if typeof<uint32>.IsAssignableFrom(t) then v.U32() :> obj
            else if typeof<uint64>.IsAssignableFrom(t) then v.U64() :> obj
            else if typeof<float32>.IsAssignableFrom(t) then float32 (v.I32()) :> obj
            else if typeof<float>.IsAssignableFrom(t) then float (v.I64()) :> obj
            else if typeof<decimal>.IsAssignableFrom(t) then decimal (v.I64()) :> obj
            else if typeof<bool>.IsAssignableFrom(t) then 
                if v.raw = "0" then false :> obj
                else if v.raw = "1" then true :> obj
                else raise (DeserializeTypeError("number", t.FullName))
            else raise (DeserializeTypeError("integer", t.FullName))
        | Str v ->
            if typeof<char>.IsAssignableFrom(t) && v.Length = 1 then v.[0] :> obj
            else raise (DeserializeTypeError("string", t.FullName))
        | Null -> raise (DeserializeTypeError("null", t.FullName))
        | Arr _ -> raise (DeserializeTypeError("array", t.FullName))
        | Obj _ -> raise (DeserializeTypeError("object", t.FullName))
    let de_obj<'T> (t: Type) (ast: MutMap<string, CbAst>) : obj =
        let constructor = t.GetConstructor(BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.NonPublic, null, Type.EmptyTypes, [||])
        let obj = 
            if constructor = null then
                let cts = t.GetConstructors(BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.NonPublic)
                if cts.Length = 0 then FormatterServices.GetUninitializedObject(t)
                else raise (DeserializeError ("Cannot construct this type : " + t.FullName)) 
            else constructor.Invoke([||])
        let fields = t.GetFields(BindingFlags.Instance ||| BindingFlags.Public)
        for field in fields do
            let has, v = ast.TryGetValue(field.Name)
            if has then
                let ft = field.FieldType
                let va = item_de_obj ft v
                field.SetValue(obj, va)
        let props = t.GetProperties(BindingFlags.Instance ||| BindingFlags.Public)
        for prop in props do
            let has, v = ast.TryGetValue(prop.Name)
            if prop.CanWrite && has then
                let ft = prop.PropertyType
                let va = item_de_obj ft v
                prop.SetValue(obj, va)
        obj