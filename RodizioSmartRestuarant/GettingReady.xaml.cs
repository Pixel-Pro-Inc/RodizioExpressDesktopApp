using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for GettingReady.xaml
    /// </summary>
    public partial class GettingReady : Window
    {
        public static GettingReady Instance { get; set; }

        private static Mutex _mutex = null;
        public GettingReady()
        {
            InitializeComponent();

            Instance = this;

            StartFunction();
        }
        private async void StartFunction()
        {
            await Task.Delay(5000);

            await Dispatcher.BeginInvoke(new Action(() => App.Instance.Config_StartUp()));
        }
    }
}
