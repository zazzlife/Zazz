using System;
using System.Runtime.Serialization;

namespace Zazz.Core.Exceptions
{
    [Serializable]
    public class AlreadyLikedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public AlreadyLikedException()
        {}

        public AlreadyLikedException(string message) : base(message)
        {}

        public AlreadyLikedException(string message, Exception inner) : base(message, inner)
        {}

        protected AlreadyLikedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {}
    }
}