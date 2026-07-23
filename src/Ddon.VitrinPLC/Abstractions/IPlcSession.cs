using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.Abstractions
{
    public interface IPlcSession
    {
        IPlcMemoryMirror Mirror { get; }
        IReadOnlyList<TagDefinition> Tags { get; }
        T Get<T>(string tagName);
        Task<WriteResult> SetAsync<T>(string tagName, T value, CancellationToken ct = default);
        IDisposable Subscribe<T>(string tagName, Action<T> handler);
        IDisposable Subscribe<T>(string tagName, Action<T, T> onChanged);
        void AddTag(TagDefinition tag);
        bool RemoveTag(string tagName);
    }
}
