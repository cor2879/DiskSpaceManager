using System;

namespace DiskSpaceManager
{
    public class ValidationException
        : Exception
    {
        public ValidationException(string message)
            : base(message)
        { }
    }
}