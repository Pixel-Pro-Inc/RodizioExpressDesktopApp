using RodizioSmartRestuarant.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");           

            Instance = this;
        }
        public void Config_StartUp()
        {
            new StartUp(this);
        }

        public void ShowKeyboard()
        {
            Dispatcher.BeginInvoke(new Action(() => Logic()));
        }

        public void ShutdownApp()
        {
            Dispatcher.BeginInvoke(new Action(() => CloseApp()));
        }

        void Logic()
        {
            Process process = Process.Start(new ProcessStartInfo(
            ((Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\osk.exe"))));
        }

        void CloseApp()
        {
            Application.Current.Shutdown();
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
