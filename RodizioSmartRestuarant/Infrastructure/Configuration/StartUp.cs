using RodizioSmartRestuarant.Application.Interfaces;
using RodizioSmartRestuarant.Infrastructure.Helpers;
using System.Threading.Tasks;
using System.Windows;

namespace RodizioSmartRestuarant.Infrastructure.Configuration
{
    public class StartUp
    {
        private App _app;
        public static StartUp Instance { get; set; }

        IDataService _dataService;

        public StartUp()
        {
            // NOTE: Intended to allow initialization without networking to be called alone
        }

        public StartUp(App app)
        {
            Instance = this;

            StartMethod(app);
        }
        async void StartMethod(App app)
        {
            if (!( await Initialize_WithNetworking(app)))
            {
                //Error Message
                MessageBoxResult messageBoxResult = MessageBox.Show("We were unable to connect to the local server. Please make sure its on and connected to the LAN before restarting this application again.", "Connection Failure", System.Windows.MessageBoxButton.OK);
                if (messageBoxResult == MessageBoxResult.OK)
                {
                    System.Windows.Application.Current.Shutdown();
                }
            }
        }
        public async Task<bool> Initialize_WithNetworking(App app)
        {
            _app = app;            

            LocalStorage.Instance = new LocalStorage();            

            if (!InitNetworking())
                return false;

            BranchSettings.Instance = new BranchSettings();

            BranchSettings.Instance.Init();

            if (app.isInitialSetup)
                await _dataService.UpdateOfflineData();

            ActivityIndicator.StartTimer();

            WindowManager.Instance.CloseAndOpen(GettingReady.Instance, new Login());

            return true;
        }

        //Intended for initial start up only
        public void Initialize_Networking_Exclusive()
        {
            WindowManager.Instance = new WindowManager();

            LocalStorage.Instance = new LocalStorage();

            BranchSettings.Instance = new BranchSettings();

            new Helpers.Settings();

            ActivityIndicator.StartTimer();
        }

        public bool InitNetworking()
        {
            LocalStorage.Instance.networkIdentity = new Core.Entities.NetworkIdentity("desktop", false);
            Core.Entities.NetworkIdentity identity = LocalStorage.Instance.networkIdentity;

            //Check local area network connectivity
            if (!(new ConnectionChecker()).CheckLAN())
            {
                //Error Message
                MessageBoxResult messageBoxResult = MessageBox.Show("Please connect to a local area network and restart the application.", "Connection Failure", System.Windows.MessageBoxButton.OK);
                if (messageBoxResult == MessageBoxResult.OK)
                {
                    System.Windows.Application.Current.Shutdown();
                }
            }

            if (!LocalIP.GetIsPrefferedTCPServer())
            {
                //Try to connect to server
                //If there is no server it returns false but it does initialize a client
                // REFACTOR: Consider renaming the method or extracting logic, its been poorly named. Why create a client anyways but return false, It will be 
                //miss leading in the future
                if (TCPClient.CreateClient())
                    return true;

                TCPClient.client = null;

                //Display Failure To Connect Message With Advice
                return false;
            }

            //Try start a server

            identity.isServer = true;

            TCPServer server = new TCPServer();
            //Makes this client the server
            identity.serverIP = server.CreateServer();

            return true;
        }

        void ShowWarning(string msg)
        {
            string messageBoxText = msg;
            string caption = "Warning";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }
    }
}
