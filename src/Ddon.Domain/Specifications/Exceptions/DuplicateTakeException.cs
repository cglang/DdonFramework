﻿using System;

namespace Ddon.Domain.Specifications.Exceptions
{
    public class DuplicateTakeException : Exception
    {
        private const string message = "Duplicate use of Take(). Ensure you don't use both Paginate() and Take() in the same specification!";

        public DuplicateTakeException() : base(message)
        {
        }

        public DuplicateTakeException(Exception innerException) : base(message, innerException)
        {
        }
    }
}
