using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Helpers;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static App Instance { get; set; }
        public bool isInitialSetup { get; set; }
        private static Mutex _mutex = null;
        // we need this to send the SMS but I don't have time to configure a centralized httpClient ( if it is even needed)
        private static readonly HttpClient client = new HttpClient();

        public App()
        {
            const string appName = "RodizioSmartRestuarant";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            // @Yewo: I need further clarifaction on this one, 
            if (!createdNew)
            {
                //App Instance Already Running //UPDATE: Does this mean that it shuts down the entire application or just that thread?
                Application.Current.Shutdown();
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");           

            Instance = this;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string folder = new SerializedObjectManager().savePath(Entities.Enums.Directories.Error);

            string fileName = $"error_log[{DateTime.Now.ToString()}].txt";

            // TODO: Have the smpt server send the error messages to our email, But we basically need to test if the line below works
            // SendErrorlogSMS(e.ExceptionObject.ToString());

            // if the file doesn't exists
            if(!File.Exists(folder + "/" + fileName))
            {
                Directory.CreateDirectory(folder);

                FileStream fileStream = File.Create(folder + "/" + fileName);
                fileStream.Close();
            }            

            File.SetAttributes(folder + "/" + fileName, FileAttributes.Normal);

            File.WriteAllText(folder + "/" + fileName, System.DateTime.Now.ToString() + "_" + e.ExceptionObject.ToString());

            SendEmail(e);
        }

        async void SendEmail(UnhandledExceptionEventArgs e)
        {
            var apiKey = "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";

            var client = new SendGridClient(apiKey);

            string userCode = "Rodizio Express Error Logger";

            var recipients = new List<string>() { "pixelprocompanyco@gmail.com",
                "yewotheu123456789@gmail.com","apexmachine2@gmail.com"};

            string _subject = "POS Terminal Error" + System.DateTime.Now.ToString();

            foreach (var reciepient in recipients)
            {
                var from = new EmailAddress("corecommunications2022@gmail.com", userCode);
                var subject = _subject;
                var to = new EmailAddress(reciepient);
                var plainTextContent = _subject;
                var htmlContent = System.DateTime.Now.ToString() + "_" + e.ExceptionObject.ToString();
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                await client.SendEmailAsync(msg).ConfigureAwait(false);
            }
        }
        async void SendErrorlogSMS(string errorlog)=> await client.PostAsync("https://rodizioexpress.com/api/sms/send/errorlogging/" + errorlog, null);

        public void Config_StartUp()
        {
            new StartUp(this);
        }

        public void ShowKeyboard()=> Dispatcher.BeginInvoke(new Action(() => Logic()));
        static void ShowWarning(string msg)
        {
            string messageBoxText = msg;
            string caption = "Warning";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }
        public void ShutdownApp() => Dispatcher.BeginInvoke(new Action(() => CloseApp()));
        void CloseApp() => Application.Current.Shutdown();

        // REFACTOR: Please name this more appropriatly
        // @Yewo: look up
        void Logic()
        {
            Process process = Process.Start(new ProcessStartInfo(
            ((Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\osk.exe"))));
        }

    }
}
