using RodizioSmartRestuarant.Core.Entities;

namespace RodizioSmartRestuarant.Infrastructure
{
    public class LocalStorage
    {
        public static LocalStorage Instance { get; set; }
        public AppUser user { get; set; }

        public NetworkIdentity networkIdentity { get; set; }

        public LocalStorage()
        {
            Instance = this;
        }
    }
}
