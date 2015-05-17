using System;
using System.Runtime.Serialization;

namespace Zazz.Core.Exceptions
{
    [Serializable]
    public class InvalidUserWithPromoterType : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidUserWithPromoterType()
        {}

        public InvalidUserWithPromoterType(string message) : base(message)
        {}

        public InvalidUserWithPromoterType(string message, Exception inner) : base(message, inner)
        {}

        protected InvalidUserWithPromoterType(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {}
    }
}