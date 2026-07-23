using System;
using System.Collections.Generic;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.Abstractions
{
    public interface ITagRegistry
    {
        void Register(TagDefinition tag);
        bool Unregister(string tagName);
        TagDefinition Resolve(string tagName);
        IReadOnlyList<TagDefinition> GetAll();

        event EventHandler<TagDefinition> TagRegistered;
        event EventHandler<string> TagUnregistered;
    }
}
