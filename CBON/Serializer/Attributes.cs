using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbStyles.Cbon.Serializer
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
        None = 0b_00_00_00,
        /// <summary>
        /// Only members marked with <see cref="CbonAttribute"/> or <see cref="DataContractAttribute"/> are serialized
        /// </summary>
        OptIn = 0b_00_00_01,
        /// <summary>
        /// Will serialize fields
        /// </summary>
        Fields = 0b_00_01_00,
        /// <summary>
        /// Will serialize properties
        /// </summary>
        Properties = 0b_00_10_00,
        /// <summary>
        /// Will serialize fields and properties
        /// </summary>
        Member = Fields | Properties,
        /// <summary>
        /// Will serialize public member
        /// </summary>
        Public = 0b_01_00_00,
        /// <summary>
        /// Will serialize private member
        /// </summary>
        Private = 0b_10_00_00,
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

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class CbonAttribute : Attribute
    {
        internal static readonly CbonAttribute Default = new CbonAttribute();

        /// <summary>
        /// <para>Control member serialization</para>
        /// </summary>
        public CbonMember Member { get; set; } = CbonMember.Default;

        public CbonAttribute() { }

        /// <param name="member">Control member serialization</param>
        public CbonAttribute(CbonMember member) => Member = member;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class CbonMemberAttribute : Attribute
    {
        internal static readonly CbonMemberAttribute Default = new CbonMemberAttribute();
        /// <summary>
        /// <para>The name for Field/Property</para>
        /// </summary>
        public string? Name { get; set; } = null;
        /// <summary>
        /// <para>If it does not exist during deserialization, an error will be thrown</para>
        /// </summary>
        public bool Required { get; set; } = false;
        /// <summary>
        /// <para>This Field/Property will be ignored</para>
        /// </summary>
        public bool Ignore { get; set; } = false;
        /// <summary>
        /// <para>If this Field/Property is an integer type, it will be serialized into hexadecimal</para>
        /// </summary>
        public bool Hex { get; set; } = false;

        public CbonMemberAttribute() { }

        /// <param name="name">The name for Field or Property</param>
        public CbonMemberAttribute(string name) => Name = name;
    }

    /// <summary>
    /// <para>This Field or Property will be ignored</para>
    /// <para>Equivalent to <see cref="CbonMemberAttribute"/> { Ignore = true }</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class CbonIgnoreAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = true)]
    public sealed class CbonEnumAttAttribute : Attribute {
        /// <summary>
        /// <para>Serialize this enum in hexadecimal</para>
        /// </summary>
        public bool Hex { get; set; } = false;
        /// <summary>
        /// <para>Serialize this enum by name</para>
        /// </summary>
        public bool Union { get; set; } = false;
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
        public Dictionary<string, Type> Items { get; set; } = new Dictionary<string, Type>();

        public CbonUnionAttribute() { }

        /// <summary>
        /// <para>Only for Class and Interface</para>
        /// </summary>
        /// <param name="items">Union items</param>
        public CbonUnionAttribute(Dictionary<string, Type> items) => Items = items;

        /// <summary>
        /// <para>Only for Class and Interface</para>
        /// </summary>
        /// <param name="items">Union items</param>
        public CbonUnionAttribute(params (string name, Type type)[] items) => Items = items.ToDictionary(t => t.name, t => t.type);

    }
}
