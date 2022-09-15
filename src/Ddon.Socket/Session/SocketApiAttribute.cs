using System;

namespace Ddon.Socket.Session
{
    public class SocketApiAttribute : Attribute
    {
        public string? Template { get; set; }

        public SocketApiAttribute(string? template = null)
        {
            Template = template;
        }
    }
}
