using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RodizioSmartRestuarant.AppLication.DTOs;
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Core.Entities.Aggregates;
using RodizioSmartRestuarant.CustomBaseClasses.BaseClasses;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Extensions;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
using Formatting = RodizioSmartRestuarant.Helpers.Formatting;
using MenuItem = RodizioSmartRestuarant.Entities.MenuItem;

namespace RodizioSmartRestuarant.Windows
{
    /// <summary>
    /// Interaction logic for LocationSelection.xaml
    /// </summary>
    public partial class LocationSelection : BaseWindow
    {
        private readonly string APIDomain;
        private readonly string _Phonenumber;
        private List<MenuItem> Menu = new List<MenuItem>();
        private Order _order;
        private readonly string _source;
        public LocationSelection(string Phonenumber, Order order, string source)
        {
            _Phonenumber = Phonenumber;
            _source = source;
            _order = order;
            APIDomain = ((List<object>)new SerializedObjectManager()
                .RetrieveData(Directories.APIDomain)).First().ToString();

            InitializeComponent();

            AssignLocationList();

            AssignMenuItems();
        }

        private async void AssignMenuItems()
        {
            var result = await OfflineDataContext.GetData(Enums.Directories.Menu);

            Menu = new List<MenuItem>();

            foreach (var item in (List<IDictionary<string, object>>)result)
            {
                Menu.Add(item.ToObject<MenuItem>());
            }
        }

        private async void AssignLocationList()
        {
            lvDataBinding.ItemsSource = await GetCustomerLocations(_Phonenumber);
        }

        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            AssignLocationList();
        }

        private void New_Location_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.Instance.CloseAndOpen(this, new LocationForm(_Phonenumber, _order, _source));
        }

        private void Select_Location_Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            SelectLocation((StackPanel)button.Parent);
        }

        private async void SelectLocation(StackPanel stackPanel)
        {
            string addressName = ((Label)stackPanel.Children[0]).Content.ToString();

            List<Location> locations = await GetCustomerLocations(_Phonenumber);

            int locationIndex = (locations.Where(location => location.AddressName == addressName)).First().Index;
            //Check if order is in range
            var distanceRequestResult = await GetDeliveryDistance(
                new DistanceRequestDto()
                {
                    FirstLocation = locations[locationIndex],
                    SecondLocation= BranchSettings.Instance.branch.Location
                });

            if (distanceRequestResult == null)
                return;

            if (distanceRequestResult.Distance > BranchSettings.Instance.branch.DeliveryRadius)
            {
                ShowError($"Sorry delivery orders must be at most {BranchSettings.Instance.branch.DeliveryRadius} {distanceRequestResult.Units} away.");
                return;
            }

            //Assign to order
            foreach (var item in _order)
            {
                item.DeliveryOrder = true;
                item.LocationIndex = locationIndex;
            }

            //Add Delivery Fee
            MenuItem deliveryFee = Menu.Where(menuItem => menuItem.Name.ToLower() == "delivery fee").First();
            OrderItem firstOrderItem = _order[0];

            _order.Add(new OrderItem()
            {
                Collected = false,
                Description = deliveryFee.Description,
                Fufilled = false,
                Name = deliveryFee.Name,
                Preparable = false,
                Price = Formatting.FormatAmountString((float.Parse(deliveryFee.Price) * 1f)),
                OrderPayments = firstOrderItem.OrderPayments,
                Purchased = false,
                Category = deliveryFee.Category,
                Quantity = 1,
                Reference = "till",
                PrepTime = Int32.Parse(deliveryFee.prepTime),
                SubCategory = deliveryFee.SubCategory,
                OrderNumber = firstOrderItem.OrderNumber,
                OrderDateTime = firstOrderItem.OrderDateTime,
                Index = _order.Count,
                ID = firstOrderItem.ID,
            });

            POS pos = new POS();

            //Proceed to next
            if (_source.ToLower() == "walkin")
            {
                WindowManager.Instance.CloseAndOpen(this, new ReceivePayment(_order, (POS)pos));
                return;
            }

            //Add Unpaid Order
            if (_source.ToLower() == "call")
            {
                CreateUnpaidOrder((POS)pos);
                return;
            }
        }

        private async Task<DistanceResultDto> GetDeliveryDistance(DistanceRequestDto distanceRequestDto)
        {
            using (var requestMessage =
            new HttpRequestMessage(HttpMethod.Post, $"{APIDomain}api/location/getdistancebetweenlocations"))
            {
                var content = JsonContent.Create(distanceRequestDto, 
                    new MediaTypeHeaderValue("application/json"));

                //Here we set the content of the request message with the object we just created for the amount block
                requestMessage.Content = content;

                try
                {
                    var response = await new HttpClient().SendAsync(requestMessage);

                    var resultString = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<DistanceResultDto>(resultString);
                }
                catch(Exception ex)
                {
                    ShowError("An error occured while trying to get the distance. Please try again.");
                    WindowManager.Instance.CloseAndOpen(this, new LocationSelection(_Phonenumber, _order, _source));

                    return null;
                }
            }
        }

        private void CreateUnpaidOrder(POS _pOS)
        {
            foreach (var item in _order)
            {
                item.Purchased = false;
                item.User = LocalStorage.Instance.user.FullName();
                item.Preparable = true;
                item.Reference = "Call";
            }

            _pOS.OnTransaction(_order[0].OrderNumber, _order);

            WindowManager.Instance.Close(this);
        }

        private async Task<List<Location>> GetCustomerLocations(string Phonenumber)
        {
            //Get the data
            //Primarily get local stored data
            //Update data if new location created
            var result = await OfflineDataContext.GetData(Enums.Directories.Customers);

            var Customers = new List<Customer>();

            foreach (var item in (List<IDictionary<string, object>>)result)
            {
                Customers.Add(item.ToObject<Customer>());
            }

            var Locations = Customers.Where(customer => customer.PhoneNumber == Phonenumber).First().Locations;

            return Locations;
        }
    }
}
