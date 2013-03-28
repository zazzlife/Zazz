using System;
using System.Runtime.Serialization;

namespace Zazz.Core.Exceptions
{
    [Serializable]
    public class OAuthAccountNotFoundException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public OAuthAccountNotFoundException()
        {}

        public OAuthAccountNotFoundException(string message) : base(message)
        {}

        public OAuthAccountNotFoundException(string message, Exception inner) : base(message, inner)
        {}

        protected OAuthAccountNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {}
    }
}