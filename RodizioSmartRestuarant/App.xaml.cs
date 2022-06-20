using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static App Instance { get; set; }
        public bool isInitialSetup { get; set; }
        private static Mutex _mutex = null;
        // we need this to send the SMS but I don't have time to configure a centralized httpClient ( if it is even needed)
        private static readonly HttpClient client = new HttpClient();

        public App()
        {
            const string appName = "RodizioSmartRestuarant";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);


            if (!createdNew)
            {
                //App Instance Already Running 
                Application.Current.Shutdown();
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");           

            Instance = this;
        }

        // TODO: Put this in an ERRORloggger
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string folder = new SerializedObjectManager().savePath(Entities.Enums.Directories.Error);

            string fileName = "error_log.txt";

            // TODO: Have the smpt server send the error messages to our email, But we basically need to test if the line below works. It wasn't done in the API
            //SendErrorlogSMS(e.ExceptionObject.ToString());

            // if the file doesn't exists
            if(!File.Exists(folder + "/" + fileName))
            {
                Directory.CreateDirectory(folder);

                FileStream fileStream = File.Create(folder + "/" + fileName);
                fileStream.Close();
            }            

            File.SetAttributes(folder + "/" + fileName, FileAttributes.Normal);

            File.WriteAllText(folder + "/" + fileName, System.DateTime.Now.ToString() + "_" + e.ExceptionObject.ToString());


        }
        async void SendErrorlogSMS(string errorlog)=> await client.PostAsync("https://rodizioexpress.com/api/sms/send/errorlogging/" + errorlog, null);

        public void Config_StartUp()
        {
            new StartUp(this);
        }

        public void ShowKeyboard()=> Dispatcher.BeginInvoke(new Action(() => Logic()));
        static void ShowWarning(string msg)
        {
            string messageBoxText = msg;
            string caption = "Warning";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }
        public void ShutdownApp() => Dispatcher.BeginInvoke(new Action(() => CloseApp()));
        // TODO: You should have it so that when ever the POS is closed it will pull up the login page. Then it should fire this method if closeApp is hit
        void CloseApp()
        {
            // This makes sure that only when there are no windows but one, will it close the application. 
            if (Application.Current.Windows.Count==1)
            {
                Application.Current.Shutdown();
            }
            //It should warn the user that they haven't closed the window and remove it. 
            else
            {
                //Error Message
                MessageBoxResult messageBoxResult = MessageBox.Show("Please close all the windows to close the application.", "Some windows are open", System.Windows.MessageBoxButton.OK);
                if (messageBoxResult == MessageBoxResult.OK)
                {
                    WindowManager.Instance.CloseAllAndOpen(new Login());
                }
            }

            // 

        }

        // REFACTOR: Please name this more appropriatly
        // @Yewo: look up
        void Logic()
        {
            Process process = Process.Start(new ProcessStartInfo(
            ((Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\osk.exe"))));
        }

    }
}
