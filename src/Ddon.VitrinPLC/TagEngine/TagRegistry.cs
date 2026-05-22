using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.TagEngine
{
    public sealed class TagRegistry : ITagRegistry
    {
        private readonly ConcurrentDictionary<string, TagDefinition> _tags = new(StringComparer.OrdinalIgnoreCase);

        public void Register(TagDefinition tag)
        {
            ArgumentNullException.ThrowIfNull(tag);
            if (!_tags.TryAdd(tag.Name, tag))
                throw new InvalidOperationException($"Tag '{tag.Name}' 已注册，请检查是否重复定义。");
        }

        public TagDefinition Resolve(string tagName)
        {
            if (_tags.TryGetValue(tagName, out var def)) return def;
            throw new KeyNotFoundException($"Tag '{tagName}' 未注册。请先调用 MapTag() 或 Register()。");
        }

        public IReadOnlyList<TagDefinition> GetAll() =>
            new List<TagDefinition>(_tags.Values).AsReadOnly();
    }
}
