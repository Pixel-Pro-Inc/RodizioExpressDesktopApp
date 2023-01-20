using System;

namespace RodizioSmartRestuarant.Core.Models
{
    /// <summary>
    /// This is a type that will store and transfer the time from the server to the Client so that you can check the if the login time is correct.
    /// </summary>
    public class ServerTime
    {
        public DateTime Time { get; set; }
    }
}
