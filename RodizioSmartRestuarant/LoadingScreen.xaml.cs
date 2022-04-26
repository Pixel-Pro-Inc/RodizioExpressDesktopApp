using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Helpers;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for LoadingScreen.xaml
    /// </summary>    
    public partial class LoadingScreen : Window
    {
        UpdateManager manager;
        public LoadingScreen()
        {
            InitializeComponent();            
            
            NextPage();
        }
        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        private void LoadingScreen_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        async void NextPage()
        {
            try
            {
                //Check For Updates
                manager = await UpdateManager.GitHubUpdateManager(@"https://github.com/Pixel-Pro-Inc/RodizioExpressDesktopApp");

                var updateInfo = await manager.CheckForUpdate();

                if (updateInfo.ReleasesToApply.Count > 0)
                {
                    //Show Update Dialog
                    WindowManager.Instance = new WindowManager();
                    WindowManager.Instance.CloseAndOpen(this, new UpdateDialog());
                    return;
                }

            }
            catch
            {
                ;
            }
            
            //Some Exists Here Initiailiztion After Mutex Verification

            if (FirstTime())
                new StartUp().Initialize_Networking_Exclusive();

            await Task.Delay(3000);

            App.Instance.isInitialSetup = FirstTime();

            if (!FirstTime())
            {
                WindowManager.Instance = new WindowManager();
                new Helpers.Settings();
                WindowManager.Instance.CloseAndOpen(this, new GettingReady());
            }   

            if (FirstTime())
                WindowManager.Instance.CloseAndOpen(this, new Setup());
        }

        bool FirstTime()
        {
            List<object> data = (List<object>)(new SerializedObjectManager().RetrieveData(Directories.BranchId));
            if (data == null)
                return true;

            return false;
        }
    }
}
