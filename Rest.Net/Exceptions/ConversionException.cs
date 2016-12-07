using System;

namespace Rest.Net.Exceptions
{
    public class ConversionException : Exception
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

        public ConversionException(string type, Exception originalException)
        {
            _message = "Failed converting returned value from server to type " + type;
            _originalException = originalException;
        }
    }
}
