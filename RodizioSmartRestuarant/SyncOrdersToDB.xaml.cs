using RodizioSmartRestuarant.Application.Interfaces;
using RodizioSmartRestuarant.Core.Entities.Aggregates;
using RodizioSmartRestuarant.Infrastructure;
using RodizioSmartRestuarant.Infrastructure.Helpers;
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
        IDataService _dataService;
        IOrderService _orderService;
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
                orderItems = (List<Order>)await _orderService.GetOfflineOrdersCompletedInclusive();

                _dataService.startedSyncing = true;

                await _dataService.SyncDataEndOfDay(orderItems);
            }            

            App.Instance.ShutdownApp();
        }
    }
}
