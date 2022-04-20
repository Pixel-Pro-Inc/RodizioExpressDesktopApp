using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for ReconnectingPage.xaml
    /// </summary>
    public partial class ReconnectingPage : Window
    {
        public ReconnectingPage()
        {
            InitializeComponent();

            Reconnect();
        }
        public async void Reconnect()
        {
            if (TCPClient.CreateClient())
            {
                WindowManager.Instance.CloseAndOpen(this, new POS());
                return;
            }

            message_1.Visibility = Visibility.Collapsed;
            message_2.Visibility = Visibility.Visible;

            retry.Visibility = Visibility.Visible;
        }
        private void Reconnect_Button_Click(object sender, RoutedEventArgs e)
        {
            message_1.Visibility = Visibility.Visible;
            message_2.Visibility = Visibility.Collapsed;

            retry.Visibility = Visibility.Collapsed;

            Reconnect();
        }
    }
}
