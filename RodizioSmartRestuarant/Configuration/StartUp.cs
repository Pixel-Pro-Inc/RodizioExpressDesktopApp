using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Configuration
{
    public static class StartUp
    {
        public static void Initialize()
        {
            LocalStorage.Instance = new LocalStorage();
            InitNetworking();

            BranchSettings.Instance = new BranchSettings();            
            WindowManager.Instance = new WindowManager();           

            if (FirebaseDataContext.Instance == null)
                FirebaseDataContext.Instance = new FirebaseDataContext();

            new Helpers.Settings();

            ActivityIndicator.StartTimer();            
        }

        static void InitNetworking()
        {
            LocalStorage.Instance.networkIdentity = new Entities.NetworkIdentity("desktop", false);
            Entities.NetworkIdentity identity = LocalStorage.Instance.networkIdentity;

            TCPClient.CreateClient(identity.ipAddress);

            if(TCPClient.ConnectToServer())
                return;

            identity.isServer = true;

            TCPClient.client = null;

            TCPServer.CreateServer(identity.serverAddress);
        }
    }
}
