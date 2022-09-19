using RodizioSmartRestuarant.Infrastructure.Helpers;
using System;
using System.Net.Http;
using System.Windows;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for ResetPasswordScreen.xaml
    /// </summary>
    public partial class ResetPasswordScreen : Window
    {
        public bool IsClosed { get; private set; }
        private static readonly HttpClient client = new HttpClient();

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        public ResetPasswordScreen()
        {
            InitializeComponent();
        }

        int block = 0;

        string token = "";

        private async void Request_Click(object sender, RoutedEventArgs e)
        {
            ActivityIndicator.AddSpinner(spinner);

            if (string.IsNullOrEmpty(accountInfo.Text))
            {
                ShowWarning("You have to enter a username or phonenumber");
                ActivityIndicator.RemoveSpinner(spinner);
                return;
            }

            if(block == 0)
            {
                block = 1;

                var responseMessage = await client.PostAsync("https://rodizioexpress.com/api/account/forgotpassword/desktop/" + accountInfo.Text, null);
                token = await responseMessage.Content.ReadAsStringAsync();

                if (token.Length == 7)
                {
                    //Success
                    block = 0;
                    Step1.Visibility = Visibility.Collapsed;
                    Step2.Visibility = Visibility.Visible;
                    ActivityIndicator.RemoveSpinner(spinner);
                    return;
                }

                ShowWarning("The username or phonenumber you entered was incorrect. try again");
                block = 0;
            }

            ActivityIndicator.RemoveSpinner(spinner);
        }

        int block1 = 0;
        private async void Reset_Click(object sender, RoutedEventArgs e)
        {
            ActivityIndicator.AddSpinner(spinner);
            if (string.IsNullOrEmpty(resetToken.Text) && string.IsNullOrEmpty(newPassword.Text))
            {
                ShowWarning("You cannot leave these fields empty");
                ActivityIndicator.RemoveSpinner(spinner);
                return;
            }

            if(resetToken.Text.ToLower() != token.ToLower())
            {
                ShowWarning("The token you entered is not correct");
                ActivityIndicator.RemoveSpinner(spinner);
                return;
            }

            if (!PasswordIsValid(newPassword.Text))
            {
                ShowWarning("The password you entered is invalid. Passwords must be atleast 6 characters and cannot contain spaces");
                ActivityIndicator.RemoveSpinner(spinner);
                return;
            }

            if(block1 == 0)
            {
                block1 = 1;

                await client.PostAsync("https://rodizioexpress.com/api/account/forgotpassword/successful/" + accountInfo.Text + "/" + newPassword.Text, null);

                block1 = 0;

                ActivityIndicator.RemoveSpinner(spinner);

                ShowSuccess("Your password was reset successfully. Login with your new password");

                WindowManager.Instance.CloseAndOpen(this, new Login());
            }
        }
        bool PasswordIsValid(string pass)
        {
            if (pass.Length < 6)
                return false;

            if (pass.Contains(" "))
                return false;

            return true;
        }
        void ShowWarning(string msg)
        {
            string messageBoxText = msg;
            string caption = "Warning";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }

        void ShowSuccess(string msg)
        {
            string messageBoxText = msg;
            string caption = "Success";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.None;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }
    }
}
