using System;
using System.Configuration;
using System.Runtime.Serialization;

namespace RodizioSmartRestuarant.Exceptions
{
    /// <summary>
    /// Thrown when the .exe file you provided does not have any connectionStrings section Note, it should already be set so most prolly happens when you have <see cref="ConfigurationManager.OpenExeConfiguration(string)"/> open the wrong file.
    /// It is most likely to be thrown in <see cref="ConnectionStringManager.GetConnectionString(string)"/>
    /// </summary>
    [Serializable]
    public class NoConnectionStringSectionFound : Exception
    {
        public NoConnectionStringSectionFound()
        {
        }

        public NoConnectionStringSectionFound(string message) : base(message)
        {
        }

        public NoConnectionStringSectionFound(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoConnectionStringSectionFound(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}