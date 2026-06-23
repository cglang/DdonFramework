using System;

namespace Ddon.VitrinPLC.Models
{
    /// <summary>
    /// Tag 定义
    /// </summary>
    public sealed class TagDefinition
    {
        public string Name { get; init; }

        public string Address { get; init; }

        public PlcDataType Type { get; init; }

        /// <summary>
        /// 仅限 String 类型使用
        /// </summary>
        public int StringLength { get; init; } = 0;

        public TagDefinition(string name, string address, PlcDataType type, int stringLength = 0)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tag name cannot be empty.", nameof(name));
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("Tag address cannot be empty.", nameof(address));
            Name = name;
            Address = address;
            Type = type;
            StringLength = stringLength;
        }

        public override string ToString() => $"[{Type}] {Name} @ {Address}";
    }
}
