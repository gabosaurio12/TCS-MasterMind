using System;

namespace MasterMind_Server.Exceptions
{
    internal class AvatarOperationException : Exception
    {
        public AvatarOperationException(ErrorMessages reason) : base(reason.ToString())
        {
            Reason = reason;
        }

        public ErrorMessages Reason { get; private set; }
    }
}
