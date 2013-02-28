using System;
using System.Runtime.Serialization;

namespace Zazz.Core.Exceptions
{
    [Serializable]
    public class EmailNotExistsException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public EmailNotExistsException()
        {
        }

        public EmailNotExistsException(string message) : base(message)
        {
        }

        public EmailNotExistsException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EmailNotExistsException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}