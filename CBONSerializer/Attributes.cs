#nullable enable
using System;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

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
        /// Serialize all member
        /// </summary>
        All = Default | Private,
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    public sealed class CbonAttribute : Attribute
    {
        public static readonly CbonAttribute Default = new CbonAttribute();

        /// <summary>
        /// <para>If there are extra items on deserialize, an error will be thrown</para>
        /// <para>Only for Class and Struct</para>
        /// </summary>
        public bool Strict { get; set; } = false;
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
        /// <para>Only for Field and Property</para>
        /// </summary>
        public string? Name { get; set; } = null;
        /// <summary>
        /// <para>Only for Field and Property</para>
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
        /// <para>Only for Field and Property</para>
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

    /// <summary>
    /// <para>For enum will use enum name instead of enum value</para>
    /// <para>For abstract class and interface will use items and serialize to <code>(TagName)ItemData</code></para>
    /// <para>For [<see cref="StructLayoutAttribute"/>(<see cref="LayoutKind.Explicit"/>)] struct while need tagis and all item need <see cref="CbonUnionTag"/></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class CbonUnionAttribute : Attribute
    {
        /// <summary>
        /// <para>Union items</para>
        /// <para>Only for Class and Interface</para>
        /// </summary>
        public Type[] Items { get; set; } = Type.EmptyTypes;

        /// <summary>
        /// <para>Tag field name</para>
        /// <para>Only for Union Struct</para>
        /// </summary>
        public string? TagIs { get; set; } = null;

        public CbonUnionAttribute() { }

        /// <summary>
        /// <para>Only for Class and Interface</para>
        /// </summary>
        /// <param name="items">Union items</param>
        public CbonUnionAttribute(params Type[] items) => Items = items;

        /// <summary>
        /// <para>Only for Union Struct</para>
        /// </summary>
        /// <param name="tagIs">Tag field name</param>
        public CbonUnionAttribute(string tagIs) => TagIs = tagIs;
    }

    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class CbonUnionItemAttribute : Attribute
    {
        public string? TagName { get; set; } = null;

        public Type Belong { get; set; }

        public CbonUnionItemAttribute(Type belong) => Belong = belong;

        public CbonUnionItemAttribute(Type belong, string tagName) : this(belong) => TagName = tagName;
    }

    /// <summary>
    /// Set tag name
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class CbonUnionTagAttribute : Attribute
    {
        public string TagName { get; set; }

        public CbonUnionTagAttribute(string tagName) => TagName = tagName;
    }

}
