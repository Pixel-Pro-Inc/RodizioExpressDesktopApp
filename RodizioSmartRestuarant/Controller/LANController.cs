using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Controller;

namespace RodizioSmartRestuarant.Helpers
{
    public class LANController:BaseController
    {
        /*
         Here the method to get data from the LAN will be called.
        
         */
        public LANController():base()
        {
            SendLocalIPAddress();
        }

        public SerializedObjectManager store = new SerializedObjectManager();

        private string GetLocalIPAddress()
        {
            // Get the IP. The information will then be stored locally
            string myIP = Dns.GetHostAddresses(Dns.GetHostName()).ToString();
            store.SaveData(myIP, "localIP");
            return myIP;
        }
        public void SendLocalIPAddress() => _firebaseDataContext.StoreData("tablet" + Tablet.GetTabletNumber().ToString() + "ip", GetLocalIPAddress(), Tablet.GetTabletNumber().ToString());
        public async void GetBranchIPAddress()
        {
            object branchip= await _firebaseDataContext.GetData("BranchIdAddress");
            //store.SaveData(branchip); i dont this line of code is secure


        }

    }
}
