using System;
using System.Runtime.Serialization;

namespace Zazz.Core.Exceptions
{
    [Serializable]
    public class InvalidEmailException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidEmailException()
        {}

        public InvalidEmailException(string message) : base(message)
        {}

        public InvalidEmailException(string message, Exception inner) : base(message, inner)
        {}

        protected InvalidEmailException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {}
    }
}