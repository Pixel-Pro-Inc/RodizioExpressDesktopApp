using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Data;
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
using RodizioSmartRestuarant.Helpers;
using RodizioSmartRestuarant.Entities.Aggregates;
using RodizioSmartRestuarant.Interfaces;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for SyncOrdersToDB.xaml
    /// </summary>
    public partial class SyncOrdersToDB : Window
    {
        IDataService _dataService;
        public SyncOrdersToDB(IDataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;

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
                List<Order> orderItems = new List<Order>();

                //Offline include completed orders
                orderItems = (List<Order>)await _dataService.GetOfflineOrdersCompletedInclusive();

                _dataService.startedSyncing = true;

                await _dataService.SyncDataEndOfDay(orderItems);
            }            

            App.Instance.ShutdownApp();
        }
    }
}
