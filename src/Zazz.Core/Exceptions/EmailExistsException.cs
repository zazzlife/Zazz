using System;
using System.Runtime.Serialization;

namespace Zazz.Core.Exceptions
{
    [Serializable]
    public class EmailExistsException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public EmailExistsException()
        {
        }

        public EmailExistsException(string message) : base(message)
        {
        }

        public EmailExistsException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EmailExistsException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}