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
            //If there is no server it returns false but it does initialize a client
            // REFACTOR: Consider renaming the method or extracting logic, its been poorly named. Why create a client anyways but return false, It will be 
            //miss leading in the future

            if (TCPClient.CreateClient())
                return;

            TCPClient.client = null;

            //Try start a server 
            // UPDATE: It doesn't try to update here but in CreateClient, this is the wrong place for the above comment

           //Tags this client as Server
           // REFACTOR: Consider making a check here to find if this worked properly, AFTER the server has been created. 
            identity.isServer = true;            

            TCPServer server = new TCPServer();
            //Makes this client the server
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
