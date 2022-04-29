using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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
using static RodizioSmartRestuarant.Entities.Enums;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Setup : Window
    {
        public NetworkInterfaceType networkInterfaceType = NetworkInterfaceType.Wireless80211;
        public bool isPrimaryTCPServer = false;
        public Setup()
        {
            InitializeComponent();
        }

        void Validate(string id, string name)
        {
            if (id == null || id == "")
            {
                ShowWarning("You cannot leave branch code empty");
                return;
            }                

            if (id.Length != 5)
            {
                ShowWarning("Your branch code is supposed to be a 5 digit number");
                return;
            }

            if (!Int32.TryParse(id, out int x))
            {
                ShowWarning("your branch code is only supposed to comprise of numbers");
                return;
            }

            if (name == null || name == "")
            {
                ShowWarning("you cannot leave printer name empty");
                return;
            }

            Next(id, name);
        }

        void ShowWarning(string msg)
        {
            string messageBoxText = msg + ".";
            string caption = "Warning";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }

        async void Next(string id, string name)
        {
            //Activity Indicator
            ActivityIndicator.AddSpinner(spinner);

            new SerializedObjectManager().SaveData(id, Directories.BranchId);
            new SerializedObjectManager().SaveData(name, Directories.PrinterName);
            new SerializedObjectManager().SaveData(networkInterfaceType, Directories.NetworkInterface);
            new SerializedObjectManager().SaveData(isPrimaryTCPServer, Directories.TCPServer);            

            //Close Activity Indicator
            ActivityIndicator.RemoveSpinner(spinner);

            WindowManager.Instance.CloseAndOpen(this, new GettingReady());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Validate(ID.Text, Receipt.Text);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            networkInterfaceType = NetworkInterfaceType.Ethernet;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            networkInterfaceType = NetworkInterfaceType.Wireless80211;
        }

        private void Server_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            isPrimaryTCPServer = true;
        }

        private void Server_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            isPrimaryTCPServer = false;
        }
    }
}
