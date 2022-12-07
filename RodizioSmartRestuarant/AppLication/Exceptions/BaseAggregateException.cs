using System;
using System.Runtime.Serialization;

namespace RodizioSmartRestuarant.AppLication.Exceptions
{
    [Serializable]
    internal class BaseAggregateException : Exception
    {
        /// <summary>
        /// These errors are formed when you don't put in valid values in the elements of a Aggregate type or it had trouble taking information from all the elements
        /// into usable data
        /// </summary>
        public BaseAggregateException()
        {
        }

        public BaseAggregateException(string message) : base(message)
        {
        }

        public BaseAggregateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BaseAggregateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}