using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        public static void InitNetworking()
        {
            LocalStorage.Instance.networkIdentity = new Entities.NetworkIdentity("desktop", false);
            Entities.NetworkIdentity identity = LocalStorage.Instance.networkIdentity;

            //Check local area network connectivity
            if (LocalIP.ScanNetwork().Count == 0) {
                ShowWarning("Please connect to a local area network and restart the application");

                Application.Current.Shutdown();
                return;            
            }

            //Try to connect to server

            if (TCPClient.CreateClient())
                return;

            TCPClient.client = null;

            //Try start a server

            identity.isServer = true;            

            TCPServer server = new TCPServer();

            identity.serverIP = server.CreateServer();
        }

        static void ShowWarning(string msg)
        {
            string messageBoxText = msg;
            string caption = "Warning";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }
    }
}
