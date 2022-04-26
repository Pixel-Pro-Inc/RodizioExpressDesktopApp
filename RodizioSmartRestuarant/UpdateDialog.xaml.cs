using RodizioSmartRestuarant.Helpers;
using Squirrel;
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
    /// Interaction logic for UpdateDialog.xaml
    /// </summary>
    public partial class UpdateDialog : Window
    {
        UpdateManager manager;
        public UpdateDialog()
        {
            InitializeComponent();
            StartUpdate();
        }

        public async void StartUpdate()
        {
            await Task.Delay(5000);
            await UpdateManager.GitHubUpdateManager(@"https://github.com/Pixel-Pro-Inc/RodizioExpressDesktopApp");

            await manager.UpdateApp();

            message.Content = "We have successfully installed the updates you need to close and reopen the app";
            closeButton.Visibility = Visibility.Visible;
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
