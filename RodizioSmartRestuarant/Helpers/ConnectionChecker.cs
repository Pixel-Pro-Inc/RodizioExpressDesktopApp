using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Data;
using System;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Helpers
{
    public class ConnectionChecker
    {
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "Bjpp5DtGhoP1IllH6CbcD47SNMTgPU2S91EqWNwl",
            BasePath = "https://rodizoapp-default-rtdb.firebaseio.com/"
        };

        IFirebaseClient client;
        public int notifCount = 0;
        public async Task<bool> CheckConnection()
        {
            bool result = true;

            try
            {
                if (!LocalStorage.Instance.networkIdentity.isServer)
                    return false;

                client = new FireSharp.FirebaseClient(config);

                FirebaseResponse response = await client.GetAsync("Branch/" + BranchSettings.Instance.branchId);
            }
            catch(Exception ex)
            {
                result = false;

                if (notifCount != 0)
                {
                    FirebaseDataContext.Instance.ToggleConnectionStatus(result);

                    return result;
                }                    

                notifCount++;
                new Notification("Connectivity", "You're offline. you can continue working offline but you'll miss out on new orders made by customers. Get back online as soon as possible.");
            }

            FirebaseDataContext.Instance.ToggleConnectionStatus(result);
            
            return result;
        }

        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        public bool CheckLAN()
        {
            int desc;
            return InternetGetConnectedState(out desc, 0);
        }
    }
}
