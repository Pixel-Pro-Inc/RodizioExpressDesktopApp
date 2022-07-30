using System;
using System.Runtime.Serialization;
using RodizioSmartRestuarant.Extensions;
using RodizioSmartRestuarant.Interfaces;

namespace RodizioSmartRestuarant.Exceptions
{
    /// <summary>
    /// This will be thrown most probably in <see cref="IOfflineDataService.GetOfflineData{Entity}(string)"/> when trying to get data from the local storage comes up with nothing.
    /// It can also be if you simply feed it incorrect data such as in <see cref="SerializedConvertExtensions.FromSerializedToObject{T}(object)"/> and the object can't be transformed from object to List of list of IDictionary
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