using System;

namespace Ddon.Socket.Session
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SocketApiAttribute : Attribute
    {
        public string? Template { get; set; }

        public SocketApiAttribute(string? template = null)
        {
            Template = template;
        }
    }
}
