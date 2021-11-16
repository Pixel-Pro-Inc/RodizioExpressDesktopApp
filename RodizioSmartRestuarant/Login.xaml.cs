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

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private FirebaseDataContext fireBaseDataContext = new FirebaseDataContext();
        public Login()
        {
            InitializeComponent();

            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            int spinAngle = 5;

            RotateTransform rotateTransform = new RotateTransform(spinAngle);
            loadingCircle.RenderTransform = rotateTransform;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
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

            if(username != null && password != null)
                if(username != "" && password != "")
                {
                    List<AppUser> users = await GetUsers();
                    for (int i = 0; i < users.Count; i++)
                    {
                        if(users[i].UserName == username)
                        {
                            if(PasswordCheck(users[i], password))
                            {
                                if(users[i].branchId == BranchSettings.Instance.branchId)
                                {
                                    SignedIn(users[i]);
                                    return;
                                }

                                errorMsgUser.Content = "You are not a member of this branch";
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
            
            if(username == null || username == "")
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

            Window current = this;
            Window next = new POS();

            current.Hide();

            next.Show();

            current.Close();
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
            var result = await fireBaseDataContext.GetData("Account");

            List<AppUser> users = new List<AppUser>();

            foreach (var item in result)
            {
                users.Add((AppUser)item);
            }

            return users;
        }
    }
}
