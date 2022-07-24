using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RodizioSmartRestuarant.Interfaces;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private FirebaseDataContext fireBaseDataContext = FirebaseDataContext.Instance;
        IDataService _dataService;
        public Login()
        {
            InitializeComponent();

            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            dispatcherTimer.Start();
        }
        int numOfSpins = 1;
        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;            
        }

        // REFACTOR: This amougst others are have the same name and similar purpose, we have to consider having overloads of a single method (that takes advantage of 'base' syntax)
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            int spinAngle = numOfSpins * 5;

            numOfSpins++;

            if (numOfSpins == 72)
                numOfSpins = 1;

            RotateTransform rotateTransform = new RotateTransform(spinAngle);
            loadingCircle.RenderTransform = rotateTransform;
        }

        private void Signin_Button_Click(object sender, RoutedEventArgs e)
        {
            Signin();
        }

        async void Signin()
        {
            ToggleSpinner();

            errorMsgPassword.Visibility = Visibility.Hidden;
            errorMsgUser.Visibility = Visibility.Hidden;

            string username = usernameField.Text.ToLower();
            string password = passwordField.Password;

            if (username != null && password != null)
                if (username != "" && password != "")
                {
                    // Why create this variable ?
                    var u = await GetUsers();

                    List<AppUser> users = u;
                    for (int i = 0; i < users.Count; i++)
                    {
                        if (users[i].UserName == username)
                        {
                            if (PasswordCheck(users[i], password))
                            {
                                if (users[i].branchId != null)
                                    if (users[i].branchId.Contains(BranchSettings.Instance.branchId))
                                    {
                                        SignedIn(users[i]);
                                        return;
                                    }

                                errorMsgUser.Content = "You are not a staff member of this branch";
                                errorMsgUser.Visibility = Visibility.Visible;

                                ToggleSpinner();

                                return;
                            }

                            errorMsgPassword.Content = "Password is incorrect";
                            errorMsgPassword.Visibility = Visibility.Visible;

                            ToggleSpinner();
                            return;
                        }
                    }

                    errorMsgUser.Content = "Username does not exist";
                    errorMsgUser.Visibility = Visibility.Visible;

                    ToggleSpinner();

                    return;
                }

            if (username == null || username == "")
            {
                errorMsgUser.Content = "You cannot leave this field empty";
                errorMsgUser.Visibility = Visibility.Visible;
            }

            if (password == null || password == "")
            {
                errorMsgPassword.Content = "You cannot leave this field empty";
                errorMsgPassword.Visibility = Visibility.Visible;
            }

            ToggleSpinner();
        }

        void ToggleSpinner()
        {
            if (loadingCircle.Visibility == Visibility.Hidden)
            {
                loadingCircle.Visibility = Visibility.Visible;

                pageContent.Visibility = Visibility.Hidden;
            }
            else
            {
                loadingCircle.Visibility = Visibility.Hidden;
                pageContent.Visibility = Visibility.Visible;
            }
        }

        void SignedIn(AppUser user)
        {
            LocalStorage.Instance.user = user;

            WindowManager.Instance.CloseAndOpen(this, new POS());
        }

        bool PasswordCheck(AppUser user, string password)
        {
            var hmac = new HMACSHA512(user.PasswordSalt);

            Byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                    return false;
            }

            return true;
        }

        async Task<List<AppUser>> GetUsers()
        {
            List<AppUser> users = await _dataService.GetData<AppUser>("Account");

            // REFACTOR: Good place to put a null Guard
            await _dataService.StoreDataOffline("Account/",users);

            return users;
        }

        int block = 0;
        private async void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (block == 0)
            {
                block = 1;

                if (!(await new ConnectionChecker().CheckConnection()))
                {
                    //Message Box
                    ShowWarning();
                    block = 0;
                    return;
                }

                WindowManager.Instance.CloseAndOpen(this, new ResetPasswordScreen());

                block = 0;
            }

        }

        void ShowWarning()
        {
            string messageBoxText = "You cannot reset your password without an internet connection.";
            string caption = "Warning";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.Instance.CloseAllAndOpen(new SyncOrdersToDB());
        }
    }
}