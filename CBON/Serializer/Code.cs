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

        internal static string Namespace = $"{nameof(CbStyles)}.{nameof(CbStyles.Cbon)}.{nameof(CbStyles.Cbon.Serializer)}";

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

    }
}
