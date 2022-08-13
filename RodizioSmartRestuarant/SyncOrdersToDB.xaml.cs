using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for SyncOrdersToDB.xaml
    /// </summary>
    public partial class SyncOrdersToDB : Window
    {
        public SyncOrdersToDB()
        {
            InitializeComponent();

            SyncData();
        }

        //Sync Data Then Close App
        async void SyncData()
        {
            await Task.Delay(5000);
            // @Yewo  what you think? In case the connection is not there we can avoid the lost of data when we implement the deleting option... 
            // Note: This is just an idea, it is by no means a real solution but we can discuss no problem
            //ActivityIndicator.AddSpinner(spinner);
            //while (!await (new ConnectionChecker().CheckConnection()))
            //{
            //    ActivityIndicator.StartTimer();
            //}
            //ActivityIndicator.RemoveSpinner(spinner);

            //If you are the server and internet is available sync data
            if (LocalStorage.Instance.networkIdentity.isServer && (await (new ConnectionChecker()).CheckConnection()))
            {
                List<List<OrderItem>> orderItems = new List<List<OrderItem>>();

                //Offline include completed orders
                orderItems = (List<List<OrderItem>>)(await FirebaseDataContext.Instance.GetOfflineOrdersCompletedInclusive());

                FirebaseDataContext.Instance.startedSyncing = true;

                await FirebaseDataContext.Instance.SyncDataEndOfDay(orderItems);
            }            

            App.Instance.ShutdownApp();
        }
    }
}
