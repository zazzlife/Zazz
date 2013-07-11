﻿using System;
using System.Runtime.Serialization;

namespace Zazz.Core.Exceptions
{
    [Serializable]
    public class NotEnoughPointsException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public NotEnoughPointsException()
        {}

        public NotEnoughPointsException(string message) : base(message)
        {}

        public NotEnoughPointsException(string message, Exception inner) : base(message, inner)
        {}

        protected NotEnoughPointsException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {}
    }
}