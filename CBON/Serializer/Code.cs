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
        }

        private class TestC1
        {
            ISeDe sede1;
            public TestC1(ISeDe sede1)
            {
                this.sede1 = sede1;
            }

            public static void Use(object obj, SeStack ctx, TestC1 data)
            {
                var self = (TestC2)obj;
                ctx.DoTab();
                var body = ctx.DoObjStart();
                ctx.Append("a ");
                data.sede1.Se(self.a, body);
                ctx.DoFinishObjItem();
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

            obj.se = GenCodesObjectSe(objname, type);

            obj.generated = true;
        }

        private static Action<object, SeStack> GenCodesObjectSe(string objname, Type t)
        {
            var items = new Dictionary<string, ISeDe>();
            GetMembers(items, t);
            var objid = ComObjId(items);
            var dataClass = GenDataClass(objid);


            static void GetMembers(Dictionary<string, ISeDe> items, Type t)
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
                            items.Add(name, sede);
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
                            items.Add(name, sede);
                        }
                    }
                }
            }

            static Dictionary<ISeDe, ulong> ComObjId(Dictionary<string, ISeDe> items)
            {
                ulong i = 0u;
                var objid = new Dictionary<ISeDe, ulong>();

                foreach (var obj in items.Values)
                {
                    if (objid.ContainsKey(obj)) continue;
                    objid.Add(obj, i);
                    i++;
                }
                return objid;
            }
            
            static (Type, ConstructorInfo, (FieldInfo, Type, ulong, string)[]) GenDataClass(Dictionary<ISeDe, ulong> objs)
            {
                var name = $"{Namespace}.CODE.SeDe.Data_{Guid.NewGuid():N}";
                var tb = module.DefineType(name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed, typeof(object));

                var fbs = objs.Select(v => {
                    var t = typeof(ISeDe);
                    var name = $"sede{v.Value}";
                    var fb = tb.DefineField($"sede{v.Value}", typeof(ISeDe), FieldAttributes.Public);
                    return ((FieldInfo)fb, t, v.Value, name);
                }).ToArray();

                var ctor = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, fbs.Select(static v => v.t).ToArray());
                foreach (var (_, _, i, n) in fbs)
                {
                    ctor.DefineParameter((int)i + 1, ParameterAttributes.None, n);
                }
                var il = ctor.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
                foreach (var (fb, _, i, _) in fbs)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_S, (byte)i + 1);
                    il.Emit(OpCodes.Stfld, fb);
                }
                il.Emit(OpCodes.Ret);

                return (tb.CreateType()!, ctor, fbs);
            }

            var se = new DynamicMethod($"{Namespace}.CODE.SeDe.{objname}_Se", typeof(void), new Type[] { typeof(object), typeof(SeStack) }, t, true);
            se.DefineParameter(0, ParameterAttributes.None, "self");
            se.DefineParameter(1, ParameterAttributes.None, "ctx");
            var il = se.GetILGenerator();

            il.Emit(OpCodes.Ldarga_S, (byte)1);
            il.EmitCall(OpCodes.Call, typeof(SeStack).GetMethod(nameof(SeStack.DoTab))!, null);

            //if (cb.Member == 0)
            //{
            //    il.Emit(OpCodes.Ldarga_S, (byte)1);
            //    il.Emit(OpCodes.Ldstr, "{}");
            //    il.EmitCall(OpCodes.Call, typeof(SeStack).GetMethod(nameof(SeStack.Append), new Type[] { typeof(string) })!, null);
            //} 
            //else
            //{
            //    il.Emit(OpCodes.Ldarga_S, (byte)1);
            //    il.Emit(OpCodes.Ldstr, "{");
            //    il.EmitCall(OpCodes.Call, typeof(SeStack).GetMethod(nameof(SeStack.Append), new Type[] { typeof(string) })!, null);

            //    if (cb.Member.HasFlag(CbonMember.Fields))
            //    {
            //        var fields = t.GetFields(BindingFlags.Instance | (cb.Member.HasFlag(CbonMember.Public) ? BindingFlags.Public : 0) | (cb.Member.HasFlag(CbonMember.Private) ? BindingFlags.NonPublic : 0));
            //        foreach (var field in fields)
            //        {
            //            if (field.GetCustomAttribute<CbonIgnoreAttribute>() != null) continue;
            //            if (cb.Member.HasFlag(CbonMember.OptIn)) if (field.GetCustomAttribute<CbonMemberAttribute>() == null && field.GetCustomAttribute<DataContractAttribute>() == null) continue;
            //            var fcb = field.GetCustomAttribute<CbonMemberAttribute>() ?? CbonMemberAttribute.Default;
            //            if (fcb.Ignore || field.GetCustomAttribute<NonSerializedAttribute>() != null) continue;
            //            var name = SeStrQuot(fcb.Name ?? field.Name);

            //            il.Emit(OpCodes.Ldarga_S, (byte)1);
            //            il.Emit(OpCodes.Ldstr, $" {name} : null ");
            //            il.EmitCall(OpCodes.Call, typeof(SeStack).GetMethod(nameof(SeStack.Append), new Type[] { typeof(string) })!, null);
            //        }
            //    }

            //    il.Emit(OpCodes.Ldarga_S, (byte)1);
            //    il.EmitCall(OpCodes.Call, typeof(SeStack).GetMethod(nameof(SeStack.DoTab))!, null);
            //    il.Emit(OpCodes.Ldarga_S, (byte)1);
            //    il.Emit(OpCodes.Ldstr, "}");
            //    il.EmitCall(OpCodes.Call, typeof(SeStack).GetMethod(nameof(SeStack.Append), new Type[] { typeof(string) })!, null);
            //}

            il.Emit(OpCodes.Ret);
            return se.CreateDelegate<Action<object, SeStack>>();
        }

        private static void GenCodesObjectDe(Type type, TypeBuilder tb, MethodBuilder mb)
        {
            var cb = type.GetCustomAttribute<CbonAttribute>() ?? CbonAttribute.Default;
            ILGenerator il = mb.GetILGenerator();

            il.Emit(OpCodes.Ret);
        }

    }
}
