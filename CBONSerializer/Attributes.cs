#nullable enable
using System;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CbStyles.Cbon
{
    /// <summary>
    /// Control member serialization
    /// </summary>
    [Flags]
    public enum CbonMember : byte
    {
        /// <summary>
        /// Do not serialize this object
        /// </summary>
        None =          0b_00_00_00,
        /// <summary>
        /// Only members marked with <see cref="CbonAttribute"/> or <see cref="DataContractAttribute"/> are serialized
        /// </summary>
        OptIn =         0b_00_00_01,
        /// <summary>
        /// Will serialize fields
        /// </summary>
        Fields =        0b_00_01_00,
        /// <summary>
        /// Will serialize properties
        /// </summary>
        Properties =    0b_00_10_00,
        /// <summary>
        /// Will serialize fields and properties
        /// </summary>
        Member = Fields | Properties,
        /// <summary>
        /// Will serialize public member
        /// </summary>
        Public =        0b_01_00_00,
        /// <summary>
        /// Will serialize private member
        /// </summary>
        Private =       0b_10_00_00,
        /// <summary>
        /// Serialize all public fields and properties by default
        /// </summary>
        Default = Member | Public,
        /// <summary>
        /// Default with OptIn
        /// </summary>
        Opt = Default | OptIn,
        /// <summary>
        /// Serialize all member
        /// </summary>
        All = Default | Private,
        /// <summary>
        /// All with OptIn
        /// </summary>
        AllOpt = All | OptIn,
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    public sealed class CbonAttribute : Attribute
    {
        internal static readonly CbonAttribute Default = new CbonAttribute();

        /// <summary>
        /// <para>Control member serialization</para>
        /// <para>Only for Class and Struct</para>
        /// </summary>
        public CbonMember Member { get; set; } = CbonMember.Default;

        /// <summary>
        /// <para>Will use enum name instead of enum value</para>
        /// <para>Only for Enum</para>
        /// </summary>
        public bool Union { get; set; } = false;

        /// <summary>
        /// <para>The name for Field or Property</para>
        /// <para>Only for Field and Property and Enum Variant</para>
        /// </summary>
        public string? Name { get; set; } = null;
        /// <summary>
        /// <para>Only for Field, Property, Class, Struct</para>
        /// <para>If it does not exist during deserialization, an error will be thrown</para>
        /// </summary>
        public bool Required { get; set; } = false;
        /// <summary>
        /// <para>This Field or Property will be ignored</para>
        /// <para>Only for Field and Property</para>
        /// </summary>
        public bool Ignore { get; set; } = false;

        public CbonAttribute() { }

        /// <summary>
        /// <para>Only for Field and Property and Enum Variant</para>
        /// </summary>
        /// <param name="name">The name for Field or Property</param>
        public CbonAttribute(string name) => Name = name;

        /// <summary>
        /// <para>Only for Class and Struct</para>
        /// </summary>
        /// <param name="member">Control member serialization</param>
        public CbonAttribute(CbonMember member) => Member = member;

    }

    /// <summary>
    /// <para>This Field or Property will be ignored</para>
    /// <para>Equivalent to <see cref="CbonAttribute"/> { ignore = true }</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class CbonIgnoreAttribute : Attribute { }

    public class CbonUnionError : Exception
    {
        public CbonUnionError(string message) : base(message) { }
    }

    /// <summary>
    /// <para>For enum will use enum name instead of enum value</para>
    /// <para>For abstract class and interface will use items and serialize to <code>(TagName)ItemData</code></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class CbonUnionAttribute : Attribute
    {
        /// <summary>
        /// <para>Union items</para>
        /// <para>Only for Class and Interface</para>
        /// </summary>
        public Type[] Items { get; set; } = Type.EmptyTypes;

        private static readonly ConditionalWeakTable<Type, Dictionary<string, Type>> CheckItemsTemp = new ConditionalWeakTable<Type, Dictionary<string, Type>>();

        internal Dictionary<string, Type> CheckItems(Type self)
        {
            if (CheckItemsTemp.TryGetValue(self, out var tmp)) return tmp;
            if (Items.Length == 0) throw new CbonUnionError($"Union cannot have 0 variants on <{self.FullName}>");
            var names = new Dictionary<string, Type>();
            var types = new HashSet<string>();
            foreach (var item in Items)
            {
                var cbui = item.GetCustomAttribute<CbonUnionItemAttribute>();
                if (cbui == null) throw new CbonUnionError($"The union variant <{item.FullName}> must have [CbonUnionItem] attribute");
                var itemName = cbui.TagName ?? item.Name;
                var fullname = item.FullName;
                var sameType = types.Contains(itemName);
                if (sameType) throw new CbonUnionError($"union cannot have 2 same variants on <{self.FullName}> of <{fullname}>");
                if (names.TryGetValue(itemName, out var sameName)) throw new CbonUnionError($"The union variant <{fullname}> has the same tag name as <{sameName.FullName}> on <{self.FullName}>");
                if (!self.IsAssignableFrom(item)) throw new CbonUnionError($"the variant <{item.FullName}> not assignable to the union <{self.FullName}>");
                types.Add(itemName);
                names.Add(itemName, item);
            }
            CheckItemsTemp.Add(self, names);
            return names;
        }

        public CbonUnionAttribute() { }

        /// <summary>
        /// <para>Only for Class and Interface</para>
        /// </summary>
        /// <param name="items">Union items</param>
        public CbonUnionAttribute(params Type[] items) => Items = items;
    }

    /// <summary>
    /// <para>OnlyFor UnionAbstractClass items or UnionInterface items, not for Enum</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class CbonUnionItemAttribute : Attribute
    {
        public string? TagName { get; set; } = null;

        public CbonUnionItemAttribute() { }

        public CbonUnionItemAttribute(string tagName) => TagName = tagName;
    }

}
