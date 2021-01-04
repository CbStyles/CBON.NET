using System;
using System.Runtime.Serialization;
using CbStyles.Cbon.SrcPos;

namespace CbStyles.Cbon.Parser
{
    [Serializable]
    internal class ParserError : Exception
    {
        public nuint at;
        public ParserError() { }
        public ParserError(string message) : base(message) { }
        public ParserError(string message, nuint at) : base(message) { this.at = at; }
        public ParserError(string message, Exception inner) : base(message, inner) { }
        protected ParserError(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class ParserException : Exception
    {
        public Pos at;

        public ParserException() { }
        public ParserException(string message) : base(message) { }
        public ParserException(string message, Exception inner) : base(message, inner) { }
        public ParserException(string message, Pos at) : base($"{message} \t at {at}") { this.at = at; }
        public ParserException(string message, Pos at, Exception inner) : base($"{message} \t at {at}", inner) { this.at = at; }
        protected ParserException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

}
