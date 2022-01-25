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
            AuthSecret = "KIxlMLOIsiqVrQmM0V7pppI1Ao67UPZv5jOdU0QJ",
            BasePath = "https://rodizoapp-default-rtdb.firebaseio.com/"
        };

        IFirebaseClient client;
        public async Task<bool> CheckConnection()
        {
            bool result = true;

            /*try
            {
                client = new FireSharp.FirebaseClient(config);

                FirebaseResponse response = await client.GetAsync("Branch/" + BranchSettings.Instance.branchId);
            }
            catch(Exception ex)
            {
                result = false;
            }*/

            FirebaseDataContext.Instance.ToggleConnectionStatus(result);
            
            return result;
        }
    }
}
