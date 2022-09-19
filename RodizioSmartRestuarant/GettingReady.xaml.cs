using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for GettingReady.xaml
    /// </summary>
    public partial class GettingReady : Window
    {
        public static GettingReady Instance { get; set; }
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
