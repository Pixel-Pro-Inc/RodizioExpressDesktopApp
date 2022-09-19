﻿using RodizioSmartRestuarant.Infrastructure;
using RodizioSmartRestuarant.Infrastructure.Helpers;
using System.Threading.Tasks;
using System.Windows;

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
            //To Allow For The Window To Open
            await Task.Delay(5000);

            if (TCPClient.CreateClient())
            {
                WindowManager.Instance.CloseAndOpen(this, new Login());
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
