﻿using System;

namespace Ddon.Domain.Specifications.Exceptions
{
    public class SelectorNotFoundException : Exception
    {
        private const string message = "The specification must have Selector defined.";

        public SelectorNotFoundException() : base(message)
        {
        }

        public SelectorNotFoundException(Exception innerException) : base(message, innerException)
        {
        }
    }
}
