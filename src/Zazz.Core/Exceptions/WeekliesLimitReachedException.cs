using System;
using System.Runtime.Serialization;

namespace Zazz.Core.Exceptions
{
    [Serializable]
    public class WeekliesLimitReachedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public WeekliesLimitReachedException()
        {}

        public WeekliesLimitReachedException(string message) : base(message)
        {}

        public WeekliesLimitReachedException(string message, Exception inner) : base(message, inner)
        {}

        protected WeekliesLimitReachedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {}
    }
}