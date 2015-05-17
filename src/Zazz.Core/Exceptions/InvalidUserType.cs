using System;
using System.Runtime.Serialization;

namespace Zazz.Core.Exceptions
{
    [Serializable]
    public class InvalidUserType : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidUserType()
        {}

        public InvalidUserType(string message) : base(message)
        {}

        public InvalidUserType(string message, Exception inner) : base(message, inner)
        {}

        protected InvalidUserType(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {}
    }
}