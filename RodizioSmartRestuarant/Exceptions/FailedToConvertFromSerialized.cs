using System;
using System.Runtime.Serialization;
using RodizioSmartRestuarant.Interfaces;

namespace RodizioSmartRestuarant.Exceptions
{
    /// <summary>
    /// This will be thrown most probably in <see cref="IOfflineDataService.GetOfflineData{Entity}(string)"/> when trying to get data from the local storage comes up with nothing
    /// </summary>
    [Serializable]
    public class FailedToConvertFromSerialized : Exception
    {
        public FailedToConvertFromSerialized()
        {
        }

        public FailedToConvertFromSerialized(string message) : base(message)
        {
        }

        public FailedToConvertFromSerialized(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FailedToConvertFromSerialized(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}