using System;
using System.Runtime.Serialization;

namespace RodizioSmartRestuarant.Services
{
   
    [Serializable]
    internal class NoNetworkException : Exception
    {
        /// <summary>
        /// The only reason I have this is cause I was lazy to put up proper logic prompt out that there is o network so you can't store online
        /// </summary>
        public NoNetworkException()
        {
        }

        public NoNetworkException(string message) : base(message)
        {
        }

        public NoNetworkException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoNetworkException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}