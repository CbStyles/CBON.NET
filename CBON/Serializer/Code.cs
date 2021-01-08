using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CbStyles.Cbon.Serializer
{

    [Serializable]
    public class SerializerTypeException : Exception
    {
        public SerializerTypeException() { }
        public SerializerTypeException(Type type): base($"<{type.FullName}> is not serializable") { }
        public SerializerTypeException(string message) : base(message) { }
        public SerializerTypeException(string message, Exception inner) : base(message, inner) { }
        protected SerializerTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    internal static partial class Codes
    {
        internal static ConcurrentDictionary<Type, ISeDe> codes = new ConcurrentDictionary<Type, ISeDe>();

        internal static string Namespace = $"{nameof(CbStyles)}.{nameof(Cbon)}.{nameof(Cbon.Serializer)}";

        public static ISeDe GetCode(Type type)
        {
            SeDeObj? obj = null;
            var r = codes.GetOrAdd(type, _ => {
                CheckType(type);
                obj = new SeDeObj();
                return obj;
            });
            if (obj != null)
            {
                GenCodes(type, obj);
            } 
            else
            {
                if (r is SeDeObj o)
                {
                    while (!o.generated) { }
                }
            }
            return r;
        }

        private static ISeDe GetCodeInner(Type type)
        {
            SeDeObj? obj = null;
            var r = codes.GetOrAdd(type, _ => {
                CheckType(type);
                obj = new SeDeObj();
                return obj;
            });
            if (obj != null)
            {
                GenCodes(type, obj);
            }
            return r;
        }

        private static void CheckType(Type type)
        {
            if (type.IsPointer) throw new SerializerTypeException("Does not support pointer type");
            if (type.IsCOMObject) throw new SerializerTypeException("Does not support COM Object");
            if (type.IsPrimitive) throw new NotImplementedException("never");
            if (typeof(DateTime).IsAssignableFrom(type)) throw new NotImplementedException("never");
            if (typeof(Guid).IsAssignableFrom(type)) throw new NotImplementedException("never");
            if (typeof(string).IsAssignableFrom(type)) throw new NotImplementedException("never");
            if (!type.IsSerializable
                && type.GetCustomAttribute<CbonAttribute>() == null
                && type.GetCustomAttribute<CbonEnumAttAttribute>() == null
                && type.GetCustomAttribute<CbonUnionAttribute>() == null
                ) throw new SerializerTypeException(type);
            if (type.GetCustomAttribute<CbonUnionAttribute>() == null)
            {
                if (type.IsAbstract) throw new SerializerTypeException("Cannot de/serialize abstract class");
                if (type.IsAbstract) throw new SerializerTypeException("Cannot de/serialize interface");
            }
            if (type.IsByRefLike) throw new SerializerTypeException("Does not support ref struct");
        }

        private static void GenCodes(Type type, SeDeObj obj)
        {
            //todo other

            GenCodesObject(type, obj);
        }

        private class TestC2
        {
            public string a;
            public string b { get; set; }
        }

        private class TestC1
        {
            ISeDe sede1;
            ISeDe sede2;

            public TestC1(ISeDe sede1, ISeDe sede2)
            {
                this.sede1 = sede1;
                this.sede2 = sede2;
            }

            public static void Use(object obj, SeStack ctx, TestC1 data)
            {
                var self = (TestC2)obj;
                ctx.DoTab();
                var body = ctx.DoObjStart();

                body.DoTab();
                body.Append("a ");
                data.sede1.Se(self.a, body);
                body.DoFinishObjItem();

                body.DoTab();
                body.Append("b ");
                data.sede2.Se(self.b, body);
                body.DoFinishObjItem();

                ctx.DoObjEnd();
            }
        }

        private static ModuleBuilder GenModule()
        {
            var name = $"CBONCODE_{Guid.NewGuid():N}";
            var ab = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.Run);
            var mb = ab.DefineDynamicModule(name);
            return mb;
        }

        internal static readonly ModuleBuilder module = GenModule();

        internal class SeDeObj : ISeDe
        {
            public Func<CbVal, object?>? de;
            public Action<object, SeStack>? se;
            public bool generated = false;

            public object? De(CbVal ast) => de!(ast);

            public void Se(object self, SeStack ctx) => se!(self, ctx);
        }

        private static void GenCodesObject(Type type, SeDeObj obj)
        {
            var objname = $"{Guid.NewGuid():N}.{type.FullName}_{type.GUID:N}";

            GenCodesObject(objname, type, obj);

            obj.generated = true;
        }

        private static void GenCodesObject(string objname, Type t, SeDeObj obj)
        {
            var items = new Dictionary<string, (ISeDe obj, Action<ILGenerator> ldfld)>();
            GetMembers(items, t);
            var objIds = ComObjId(items);
            var dataClass = GenDataClass(objIds);
            var dataObj = BuildDataClass(dataClass.classT, dataClass.ci, dataClass.objs);
            var se = GenSeFunc(objname, t, dataClass.classT, items, objIds, dataClass.fis, dataObj);
            obj.se = se;


            static void GetMembers(Dictionary<string, (ISeDe obj, Action<ILGenerator> ldfld)> items, Type t)
            {
                var cb = t.GetCustomAttribute<CbonAttribute>() ?? CbonAttribute.Default;
                if (cb.Member != 0)
                {
                    if (cb.Member.HasFlag(CbonMember.Fields))
                    {
                        var fields = t.GetFields(BindingFlags.Instance | (cb.Member.HasFlag(CbonMember.Public) ? BindingFlags.Public : 0) | (cb.Member.HasFlag(CbonMember.Private) ? BindingFlags.NonPublic : 0));
                        foreach (var field in fields)
                        {
                            if (field.GetCustomAttribute<CbonIgnoreAttribute>() != null) continue;
                            if (cb.Member.HasFlag(CbonMember.OptIn)) if (field.GetCustomAttribute<CbonMemberAttribute>() == null && field.GetCustomAttribute<DataContractAttribute>() == null) continue;
                            var fcb = field.GetCustomAttribute<CbonMemberAttribute>() ?? CbonMemberAttribute.Default;
                            if (fcb.Ignore || field.GetCustomAttribute<NonSerializedAttribute>() != null) continue;
                            var name = SeStrQuot(fcb.Name ?? field.Name);
                            var sede = GetCodeInner(field.FieldType);
                            items.Add(name, (sede, (ILGenerator il) => {
                                il.Emit(OpCodes.Ldfld, field);
                            }));
                        }
                    }
                    if (cb.Member.HasFlag(CbonMember.Properties))
                    {
                        var props = t.GetProperties(BindingFlags.Instance | (cb.Member.HasFlag(CbonMember.Public) ? BindingFlags.Public : 0) | (cb.Member.HasFlag(CbonMember.Private) ? BindingFlags.NonPublic : 0));
                        foreach (var prop in props)
                        {
                            if (!prop.CanRead) continue;
                            if (prop.GetCustomAttribute<CbonIgnoreAttribute>() != null) continue;
                            if (cb.Member.HasFlag(CbonMember.OptIn)) if (prop.GetCustomAttribute<CbonAttribute>() == null && prop.GetCustomAttribute<DataContractAttribute>() == null) continue;
                            var pcb = prop.GetCustomAttribute<CbonMemberAttribute>() ?? CbonMemberAttribute.Default;
                            if (pcb.Ignore || prop.GetCustomAttribute<NonSerializedAttribute>() != null) continue;
                            var name = SeStrQuot(pcb.Name ?? prop.Name);
                            var sede = GetCodeInner(prop.PropertyType);
                            items.Add(name, (sede, (ILGenerator il) => {
                                il.Emit(OpCodes.Callvirt, prop.GetMethod!);
                            }));
                        }
                    }
                }
            }

            static Dictionary<ISeDe, ulong> ComObjId(Dictionary<string, (ISeDe obj, Action<ILGenerator> ldfld)> items)
            {
                ulong i = 0u;
                var objid = new Dictionary<ISeDe, ulong>();

                foreach (var (obj, _) in items.Values)
                {
                    if (objid.ContainsKey(obj)) continue;
                    objid.Add(obj, i);
                    i++;
                }
                return objid;
            }
            
            static (Type classT, ConstructorInfo ci, Dictionary<ulong, (FieldInfo f, Type t, string name)> fis, ISeDe[] objs) GenDataClass(Dictionary<ISeDe, ulong> objs)
            {
                var name = $"{Namespace}.CODE.SeDe.Data_{Guid.NewGuid():N}";
                var tb = module.DefineType(name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed, typeof(object));

                var fbs = objs.Select(v => {
                    var t = typeof(ISeDe);
                    var name = $"sede{v.Value}";
                    var fb = tb.DefineField($"sede{v.Value}", typeof(ISeDe), FieldAttributes.Public);
                    return (fb, t, v.Value, name, v.Key);
                }).ToArray();

                var ctor = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, fbs.Select(static v => v.t).ToArray());
                foreach (var (_, _, i, n, _) in fbs)
                {
                    ctor.DefineParameter((int)i + 1, ParameterAttributes.None, n);
                }
                var il = ctor.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
                foreach (var (fb, _, i, _, _) in fbs)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_S, (byte)i + 1);
                    il.Emit(OpCodes.Stfld, fb);
                }
                il.Emit(OpCodes.Ret);

                var typ = tb.CreateType()!;
                var fis = fbs.ToDictionary(v => v.Value, v => (typ.GetField(v.fb.Name)!, v.t, v.name));
                var args = fbs.Select(v => v.Key).ToArray()!;

                return (tb.CreateType()!, ctor, fis, args);
            }

            static object BuildDataClass(Type classT, ConstructorInfo ci, IEnumerable<ISeDe> objs)
            {
                var ctor = classT.GetConstructor(ci.GetParameters().Select(v => v.ParameterType).ToArray())!;
                var obj = ctor.Invoke(objs.ToArray());
                return obj;
            }

            static Action<object, SeStack> GenSeFunc(string objname, Type t, Type dataT, Dictionary<string, (ISeDe obj, Action<ILGenerator> ldfld)> fs, Dictionary<ISeDe, ulong> objIds, Dictionary<ulong, (FieldInfo f, Type t, string name)> fis, object dataObj)
            {
                var se = new DynamicMethod($"{Namespace}.CODE.SeDe.{objname}_Se", typeof(void), new Type[] { typeof(object), typeof(SeStack), dataT }, t, true);
                se.DefineParameter(0, ParameterAttributes.None, "obj");
                se.DefineParameter(1, ParameterAttributes.None, "ctx");
                se.DefineParameter(2, ParameterAttributes.None, "data"); // dataT
                var il = se.GetILGenerator();

                il.DeclareLocal(t);                 // self
                il.DeclareLocal(typeof(SeStack));   // body

                // XXX self = (XXX)obj
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, t);
                il.Emit(OpCodes.Stloc_0);

                // ctx.DoTab();
                il.Emit(OpCodes.Ldarga_S, (byte)1);
                il.EmitCall(OpCodes.Call, typeof(SeStack).GetMethod(nameof(SeStack.DoTab))!, null);

                //// body = DoObjStart();
                il.Emit(OpCodes.Ldarga_S, (byte)1);
                il.Emit(OpCodes.Call, typeof(SeStack).GetMethod(nameof(SeStack.DoObjStart))!);
                il.Emit(OpCodes.Stloc_1);

                foreach (var (key, (obj, ldfld)) in fs)
                {
                    // body.DoTab();
                    il.Emit(OpCodes.Ldloca_S, (byte)1);
                    il.Emit(OpCodes.Call, typeof(SeStack).GetMethod(nameof(SeStack.DoTab))!);

                    // body.Append($"{f.key} ");
                    il.Emit(OpCodes.Ldloca_S, (byte)1);
                    il.Emit(OpCodes.Ldstr, $"{key} ");
                    il.Emit(OpCodes.Call, typeof(SeStack).GetMethod(nameof(SeStack.Append), new Type[] { typeof(string) })!);

                    var ofi = objIds[obj];
                    var fi = fis[ofi];

                    // data.seden.Se(self.xxx, body);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Ldfld, fi.f);
                    il.Emit(OpCodes.Ldloc_0);
                    ldfld(il);
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Callvirt, typeof(ISeDe).GetMethod(nameof(ISeDe.Se))!);

                    // data.DoFinishObjItem();
                    il.Emit(OpCodes.Ldloca_S, (byte)1);
                    il.Emit(OpCodes.Call, typeof(SeStack).GetMethod(nameof(SeStack.DoFinishObjItem))!);
                }

                // ctx.DoObjEnd();
                il.Emit(OpCodes.Ldarga_S, (byte)1);
                il.Emit(OpCodes.Call, typeof(SeStack).GetMethod(nameof(SeStack.DoObjEnd))!);

                il.Emit(OpCodes.Ret);

                var d = se.CreateDelegate(typeof(Action<,,>).MakeGenericType(typeof(object), typeof(SeStack), dataT));
                return (object obj, SeStack ctx) =>
                {
                    d.DynamicInvoke(obj, ctx, dataObj);
                };
            }

        }

        private static void GenCodesObjectDe(Type type, TypeBuilder tb, MethodBuilder mb)
        {
            var cb = type.GetCustomAttribute<CbonAttribute>() ?? CbonAttribute.Default;
            ILGenerator il = mb.GetILGenerator();

            il.Emit(OpCodes.Ret);
        }

    }
}
