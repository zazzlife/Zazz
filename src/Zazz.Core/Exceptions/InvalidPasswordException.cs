using System;
using System.Runtime.Serialization;

namespace Zazz.Core.Exceptions
{
    [Serializable]
    public class InvalidPasswordException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidPasswordException()
        {
        }

        public InvalidPasswordException(string message) : base(message)
        {
        }

        public InvalidPasswordException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidPasswordException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}