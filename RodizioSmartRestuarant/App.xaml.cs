using RodizioSmartRestuarant.Infrastructure.Configuration;
using RodizioSmartRestuarant.Infrastructure.Helpers;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Windows;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
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

            // @Yewo: I need further clarifaction on this one, 
            if (!createdNew)
            {
                //App Instance Already Running //UPDATE: Does this mean that it shuts down the entire application or just that thread?
                System.Windows.Application.Current.Shutdown();
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");           

            Instance = this;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string folder = new SerializedObjectManager().savePath(RodizioSmartRestuarant.Core.Entities.Enums.Directories.Error);

            string fileName = "error_log.txt";

            // TODO: Have the smpt server send the error messages to our email, But we basically need to test if the line below works
            // SendErrorlogSMS(e.ExceptionObject.ToString());

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
        void CloseApp() => System.Windows.Application.Current.Shutdown();

        // REFACTOR: Please name this more appropriatly
        // @Yewo: look up
        void Logic()
        {
            Process process = Process.Start(new ProcessStartInfo(
            ((Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\osk.exe"))));
        }

    }
}
