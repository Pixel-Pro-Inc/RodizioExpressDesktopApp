using System;
using System.Runtime.Serialization;

namespace RodizioSmartRestuarant.Extensions
{
    /// <summary>
    /// This should be thrown in the <see cref="JsonConvertExtensions"/> when trying to convert a list of objects/ object to the appropriate type
    /// </summary>
    [Serializable]
    public class FailedToConvertFromJson : Exception
    {
        public FailedToConvertFromJson() { }
        /// <summary>
        /// This should be thrown in the <see cref="JsonConvertExtensions"/> when trying to convert a list of objects/ object to the appropriate type
        /// </summary>
        public FailedToConvertFromJson(string message) : base(message) { }
        /// <summary>
        /// This should be thrown in the <see cref="JsonConvertExtensions"/> when trying to convert a list of objects/ object to the appropriate type
        /// </summary>
        public FailedToConvertFromJson(string message, Exception inner) : base(message, inner) { }
        protected FailedToConvertFromJson(
          SerializationInfo info,
          StreamingContext context) : base(info, context) { }
    }

}