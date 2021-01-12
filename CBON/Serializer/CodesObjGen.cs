using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CbStyles.Cbon.Serializer
{
    internal static partial class Codes
    {
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
                            var name = SeKey(fcb.Name ?? field.Name);
                            var sede = GetCodeInner(field.FieldType);
                            items.Add(name, (sede, (ILGenerator il) => {
                                il.Emit(OpCodes.Ldfld, field);
                            }
                            ));
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
                            var name = SeKey(pcb.Name ?? prop.Name);
                            var sede = GetCodeInner(prop.PropertyType);
                            items.Add(name, (sede, (ILGenerator il) => {
                                il.Emit(OpCodes.Callvirt, prop.GetMethod!);
                            }
                            ));
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
                var se = new DynamicMethod($"{Namespace}.CODE.SeDe.{objname}_Se", typeof(void), new Type[] { dataT, typeof(object), typeof(SeStack) }, dataT, true);
                se.DefineParameter(0, ParameterAttributes.None, "this"); // dataT
                se.DefineParameter(1, ParameterAttributes.None, "obj");
                se.DefineParameter(2, ParameterAttributes.None, "ctx");
                var il = se.GetILGenerator();

                il.DeclareLocal(t);                 // self
                il.DeclareLocal(typeof(SeStack));   // body

                // XXX self = (XXX)obj
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Castclass, t);
                il.Emit(OpCodes.Stloc_0);

                // ctx.DoTab();
                il.Emit(OpCodes.Ldarga_S, (byte)2);
                il.EmitCall(OpCodes.Call, SeStack.MI_DoTab, null);

                //// body = DoObjStart();
                il.Emit(OpCodes.Ldarga_S, (byte)2);
                il.Emit(OpCodes.Call, SeStack.MI_DoObjStart);
                il.Emit(OpCodes.Stloc_1);

                int len = fs.Count, i = 0;
                foreach (var (key, (obj, ldfld)) in fs)
                {
                    i++;

                    // body.DoTab();
                    il.Emit(OpCodes.Ldloca_S, (byte)1);
                    il.Emit(OpCodes.Call, SeStack.MI_DoTab);

                    // body.DoObjKey($"{f.key}");
                    il.Emit(OpCodes.Ldloca_S, (byte)1);
                    il.Emit(OpCodes.Ldstr, key);
                    il.Emit(OpCodes.Call, SeStack.MI_DoObjKey);

                    var ofi = objIds[obj];
                    var fi = fis[ofi];

                    // body.DoSe(this.seden, self.xxx);
                    il.Emit(OpCodes.Ldloca_S, (byte)1);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, fi.f);
                    il.Emit(OpCodes.Ldloc_0);
                    ldfld(il);
                    il.Emit(OpCodes.Call, SeStack.MI_DoSe<ISeDe>());

                    // data.DoFinishObjItem();
                    il.Emit(OpCodes.Ldloca_S, (byte)1);
                    if (i >= len) il.Emit(OpCodes.Call, SeStack.MI_DoFinishObjItem);
                    else il.Emit(OpCodes.Call, SeStack.MI_DoFinishObjItemBody);
                }

                // ctx.DoObjEnd();
                il.Emit(OpCodes.Ldarga_S, (byte)2);
                il.Emit(OpCodes.Call, SeStack.MI_DoObjEnd);

                il.Emit(OpCodes.Ret);

                var d = se.CreateDelegate(typeof(Action<,>).MakeGenericType(typeof(object), typeof(SeStack)), dataObj);
                return (object obj, SeStack ctx) =>
                {
                    d.DynamicInvoke(obj, ctx);
                };
            }

        }

        private static void GenCodesObjectDe(Type type, TypeBuilder tb, MethodBuilder mb)
        {
            var cb = type.GetCustomAttribute<CbonAttribute>() ?? CbonAttribute.Default;
            ILGenerator il = mb.GetILGenerator();

            il.Emit(OpCodes.Ret);
        }

        private class TestC2
        {
            public int a;
        }

        private class TestC1
        {
            ISeDe sede1;

            public TestC1(ISeDe sede1)
            {
                this.sede1 = sede1;
            }

            public void Use(object obj, SeStack ctx)
            {
                var self = (TestC2)obj;
                ctx.DoTab();
                var body = ctx.DoObjStart();

                body.DoTab();
                body.DoObjKey("a");
                body.Append(self.a.ToString("X"));
                //body.DoSe(sede1, self.a);
                body.DoFinishObjItem();

                ctx.DoObjEnd();
            }
        }
    }
}
