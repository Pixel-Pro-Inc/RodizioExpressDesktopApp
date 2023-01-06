using RodizioSmartRestuarant.Core.Entities.Aggregates;
using RodizioSmartRestuarant.CustomBaseClasses.BaseClasses;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Helpers;
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
using static RodizioSmartRestuarant.Entities.Enums;

namespace RodizioSmartRestuarant.Windows
{
    /// <summary>
    /// Interaction logic for LocationForm.xaml
    /// </summary>
    public partial class LocationForm : BaseWindow
    {
        private readonly string _phonenumber;
        private readonly string _source;
        private Order _order;
        public LocationForm(string phonenumber, Order order, string source)
        {
            _phonenumber = phonenumber;
            _source = source;
            _order = order;

            string APIDomain = ((List<object>)new SerializedObjectManager()
                .RetrieveData(Directories.APIDomain)).First().ToString();

            InitializeComponent();

            myBrowser.Loaded += delegate
            {
                myBrowser.Source = new Uri($"{APIDomain}location-creation?phonenumber={_phonenumber}");
            };
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            CloseLogic();
        }

        private async void CloseLogic()
        {
            //Update local customers
            await FirebaseDataContext.Instance.UpdateCustomers();

            WindowManager.Instance.CloseAndOpen(this, new LocationSelection(_phonenumber, _order, _source));
        }
    }
}
