using System;
using System.Runtime.Serialization;

namespace RodizioSmartRestuarant.Application.Exceptions
{
    /// <summary>
    /// This happens cause to get data you have to put in the correct type parameter. But some data doesn't really have a type so they have been left blank
    /// <para> To prevent errors occuring further down I made this exception. When logic if figured out on how to work for those data types they will be included</para>
    /// </summary>
    [Serializable]
    internal class InCorrectDirectory : Exception
    {
        public InCorrectDirectory()
        {
        }

        /// <summary>
        /// This happens cause to get data you have to put in the correct type parameter. The type parameter to chose is acquired using GetDirectory(fullPath). But some data doesn't really have a type so they have been left blank
        /// <para> To prevent errors occuring further down I made this exception. When logic is figured out on how to work for those data types they will be included</para>
        /// </summary>
        public InCorrectDirectory(string message) : base(message)
        {
        }

        public InCorrectDirectory(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InCorrectDirectory(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}