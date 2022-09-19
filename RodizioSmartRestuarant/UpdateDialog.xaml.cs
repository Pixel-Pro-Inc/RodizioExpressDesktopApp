using Squirrel;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for UpdateDialog.xaml
    /// </summary>
    public partial class UpdateDialog : Window
    {
        public UpdateDialog()
        {
            InitializeComponent();
            StartUpdate();
        }

        public async void StartUpdate()
        {
            await Task.Delay(5000);
            // TRACK: Added GC to make sure updateManager Is Disposed to avoid Mutex Leaks
            // @Yewo: Okay here you need to explain how Mutex leaks occur and how that is a bad thing
            try
            {
                using (var updateManager = await UpdateManager.GitHubUpdateManager(@"https://github.com/Pixel-Pro-Inc/RodizioExpressDesktopApp"))
                {
                    await updateManager.UpdateApp();
                }

                GC.WaitForFullGCComplete();

                message.Content = "We have successfully installed the updates. You need to restart this computer";
                closeButton.Visibility = Visibility.Visible;
            }
            catch
            {
                message.Content = "We were unable to update the app you need to close and reopen the app";
                closeButton.Visibility = Visibility.Visible;
            }   
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            //Application.Current.Shutdown();
            System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
        }
    }
}
