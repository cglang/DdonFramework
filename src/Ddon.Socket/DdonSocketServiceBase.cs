﻿using Ddon.ConvenientSocket.Extra;
using System.Diagnostics.CodeAnalysis;

namespace Ddon.Socket
{
    public class DdonSocketServiceBase
    {
        [AllowNull]
        public DdonSocketConnectionCore Connection { get; set; }

        [AllowNull]
        public DdonSocketHead Head { get; set; }
    }
}
