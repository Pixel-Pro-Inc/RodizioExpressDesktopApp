using RodizioSmartRestuarant.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
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
        public App()
        {
            Instance = this;
            StartUp.Initialize();
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
    }
}
