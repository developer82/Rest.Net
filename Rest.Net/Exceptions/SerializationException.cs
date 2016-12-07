using System;

namespace Rest.Net.Exceptions
{
    public class SerializationException : Exception
    {
        private string _message;
        private Exception _originalException;

        public override string Message
        {
            get { return _message; }
        }

        public override Exception GetBaseException()
        {
            return _originalException;
        }

        public SerializationException(string type, Exception originalException)
        {
            _message = "The serialization of the server response failed for type " + type;
            _originalException = originalException;
        }
    }
}
