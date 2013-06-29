using System;
using System.Runtime.Serialization;

namespace Zazz.Core.Exceptions
{
    [Serializable]
    public class InvalidFollowException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidFollowException()
        { }

        public InvalidFollowException(string message)
            : base(message)
        { }

        public InvalidFollowException(string message, Exception inner)
            : base(message, inner)
        { }

        protected InvalidFollowException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        { }
    }
}
