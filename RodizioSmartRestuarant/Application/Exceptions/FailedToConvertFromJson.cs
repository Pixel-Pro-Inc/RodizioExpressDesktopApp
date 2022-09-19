using System;
using System.Runtime.Serialization;
using RodizioSmartRestuarant.Application.Extensions;
using RodizioSmartRestuarant.Application.Interfaces;

namespace RodizioSmartRestuarant.Application.Exceptions
{
    /// <summary>
    /// This should be thrown in the <see cref="JsonConvertExtensions"/> when trying to convert a list of objects/ object to the appropriate type
    /// <para> If thrown, please check that you have called the right method for the result, or used the right source to convert.</para>
    /// <para> Know errors can be: When you try to fire,
    /// <see cref="IFirebaseServices.GetData{T}(string)"/> with a type param of <see cref="Order"/></para>
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