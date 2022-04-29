using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Helpers;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Net.Http;
using System.Reflection;
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

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for POS.xaml
    /// </summary>
    public partial class POS : Window
    {
        private static readonly HttpClient client = new HttpClient();
        public List<List<OrderItem>> orders = new List<List<OrderItem>>();
        private FirebaseDataContext firebaseDataContext;
        private bool showingResults;

        public POS()
        {
            InitializeComponent();

            firebaseDataContext = FirebaseDataContext.Instance;

            OnStart();
            
            welcomeMsg.Text = "Welcome, " + LocalStorage.Instance.user.FullName();
        }

        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        async void OnStart()
        {
            string version = "";
            try
            {
                using (var manager = await UpdateManager.GitHubUpdateManager(@"https://github.com/Pixel-Pro-Inc/RodizioExpressDesktopApp"))
                {
                    version = manager.CurrentlyInstalledVersion().ToString();
                }

                GC.WaitForFullGCComplete();
            }
            catch
            {
                version = "null";
            }

            versionText.Text = "Version : " + version;

            //ActivityIndicator.AddSpinner(spinner);

            var resultOnline = await firebaseDataContext.GetData_Online("Order/" + BranchSettings.Instance.branchId);

            //Offline include completed orders
            List<List<OrderItem>> tempOffline = (List<List<OrderItem>>)(await firebaseDataContext.GetOfflineOrdersCompletedInclusive());

            //Online orders //Check to see if completed ones are included
            List<List<OrderItem>> tempOnline = new List<List<OrderItem>>();

            foreach (var item in resultOnline)
            {
                List<OrderItem> data = new List<OrderItem>();

                data = JsonConvert.DeserializeObject<List<OrderItem>>(((JArray)item).ToString());

                tempOnline.Add(data);
            }

            //Compare online orders with offline orders(Completed Order Inclusive)

            //All Orders
            List<List<OrderItem>> temp = new List<List<OrderItem>>();

            //Primarily take offline over online
            List<string> offlineOrderNumbers = GetOrderNumbers(tempOffline);

            temp = tempOffline;

            foreach (var item in tempOnline)
            {
                //if doesn't contain add to temp
                if (!(offlineOrderNumbers.Contains(item[0].OrderNumber)))
                {
                    temp.Add(item);
                }
            }

            //Delete Downloaded Orders From DB To Avoid Re-Downloading
            foreach (var item in temp)
            {
                await FirebaseDataContext.Instance.DeleteData("Order/" + BranchSettings.Instance.branchId + "/" + item[0].OrderNumber);//Delete all downloaded orders from DB
            }

            UpdateOrderView(temp);
        }
        /// <summary>
        /// Logic for compiling and displaying relevant orders
        /// </summary>
        public void UpdateOrderView(List<List<OrderItem>> data, UIChangeSource? source = null)
        {
            ActivityIndicator.AddSpinner(spinner);
            this.Dispatcher.Invoke(() =>
            {

                List<List<OrderItem>> temp = data;//.Where(o => !o[0].Collected && !o[0].MarkedForDeletion).ToList();

                temp = Formatting.ChronologicalOrderList(temp);

                List<string> orderNumbers = GetCurrentOrderNumbers();
                orderViewer.Children.Clear();
                List<int> indexesToChange = new List<int>();
                List<int> indexesOfNewValues = new List<int>();


                foreach (var item in temp)//New Updates
                {
                    if (!orderNumbers.Contains(item[0].OrderNumber))
                    {
                        //Addition of new order
                        orders.Add(item);
                        continue;//Continue so we dont trigger part 2
                    }

                    if (orderNumbers.Contains(item[0].OrderNumber))
                    {
                        //Edit / Completed order
                        foreach (var _item in orders)
                        {
                            //Loop through current orders find match and update it
                            if (_item[0].OrderNumber == item[0].OrderNumber)
                            {
                                indexesToChange.Add(orders.IndexOf(_item));

                                indexesOfNewValues.Add(temp.IndexOf(item));
                            }
                        }
                    }
                }

                for (int i = 0; i < indexesToChange.Count; i++)
                {
                    orders[indexesToChange[i]] = temp[indexesOfNewValues[i]];
                }

                orders = orders.Where(o => !o[0].Collected).ToList();

                orders = Formatting.ChronologicalOrderList(orders);

                foreach (var order in orders)
                {
                    orderViewer.Children.Add(GetPanel(order));
                }

                #region Old Update UI/Orders Code
                //if (source == null)
                //source = GetUIChangeSource(temp);

                //temp = Formatting.ChronologicalOrderList(temp);
                /*
                switch (source)
                {
                    case UIChangeSource.Addition:
                        for (int i = 0; i < temp.Count; i++)
                        {
                            var item = temp[i];

                            if (!orderNumbers.Contains(temp[i][0].OrderNumber))
                            {
                                orderNumbers.Add(temp[i][0].OrderNumber);

                                if (!item[0].Collected || !item[0].MarkedForDeletion)
                                {
                                    orders.Add(item);

                                    orderViewer.Children.Add(GetPanel(item));
                                }
                            }
                        }
                        ActivityIndicator.RemoveSpinner(spinner);
                        break;
                    case UIChangeSource.Edit:
                        //Hide Collected/Cancelled Items
                        for (int i = 0; i < orders.Count; i++)
                        {
                            var item = orders[i];

                            if (item[0].Collected || item[0].MarkedForDeletion)
                            {
                                orders.RemoveAt(i);

                                orderViewer.Children.RemoveAt(i);
                            }
                        }

                        //Replace Edited Items
                        for (int i = 0; i < temp.Count; i++)
                        {
                            var item = temp[i];

                            List<List<OrderItem>> ord = orders.Where(o => o[0].OrderNumber == item[0].OrderNumber).ToArray().ToList();

                            if (OrderItemChanged(item, ord[0], i))
                            {
                                orders[i] = item;

                                orderViewer.Children.Insert(i, GetPanel(item));
                                orderViewer.Children.RemoveAt(i + 1);

                                continue;
                            }

                            ord = orders.Where(o => o[0].OrderNumber == item[0].OrderNumber).ToArray().ToList();

                            if (item.Count != ord[0].Count)
                            {
                                orders[i] = item;

                                orderViewer.Children.Insert(i, GetPanel(item));
                                orderViewer.Children.RemoveAt(i + 1);
                            }
                        }

                        ActivityIndicator.RemoveSpinner(spinner);
                        break;
                    case UIChangeSource.StartUp:
                        orderViewer.Children.Clear();
                        orders.Clear();

                        for (int i = 0; i < temp.Count; i++)
                        {
                            var item = temp[i];

                            if (!item[0].Collected || !item[0].MarkedForDeletion)
                            {
                                orders.Add(item);

                                orderViewer.Children.Add(GetPanel(item));
                            }
                        }

                        ActivityIndicator.RemoveSpinner(spinner);
                        break;
                    case UIChangeSource.Deletion:
                        List<string> orderNumbersNew = new List<string>();

                        foreach (var item in temp)
                        {
                            if (!orderNumbersNew.Contains(item[0].OrderNumber))
                                orderNumbersNew.Add(item[0].OrderNumber);
                        }

                        foreach (var item in orderNumbers)
                        {
                            if (!orderNumbersNew.Contains(item))
                            {
                                int i = orderNumbers.IndexOf(item);

                                orders.RemoveAt(i);

                                orderViewer.Children.RemoveAt(i);
                            }
                        }
                        ActivityIndicator.RemoveSpinner(spinner);
                        break;
                    case UIChangeSource.Search:
                        for (int i = 0; i < temp.Count; i++)
                        {
                            var item = temp[i];

                            if (!item[0].Collected || !item[0].MarkedForDeletion)
                            {
                                orders.Add(item);

                                orderViewer.Children.Add(GetPanel(item));
                            }
                        }
                        ActivityIndicator.RemoveSpinner(spinner);
                        break;
                }
                */
                #endregion

                ActivityIndicator.RemoveSpinner(spinner);

                //Update with size settings
                RodizioSmartRestuarant.Helpers.Settings.Instance.OnWindowCountChange();

                UpdateOrderCount();

                //Update Locally Stored Data
                //We use the variable orders since it has been updated to include all the latest information by the code above
                firebaseDataContext.ResetLocalData(orders);
                //Update Offline Client With Server Data
            });
        }

        bool OrderItemChanged(List<OrderItem> itemsNew, List<OrderItem> itemsOld, int index)
        {
            string newItem = itemsNew[0].OrderNumber;
            string oldItem = itemsOld[0].OrderNumber;

            if (itemsNew.Count == itemsOld.Count)
                for (int i = 0; i < itemsNew.Count; i++)
                {
                    if (itemsNew[i].Fufilled != itemsOld[i].Fufilled || itemsNew[i].Purchased != itemsOld[i].Purchased)
                        return true;
                }

            return false;
        }

        public List<string> GetCurrentOrderNumbers()
        {
            List<string> orderNumbers = new List<string>();

            foreach (var item in orders)
            {
                if (!orderNumbers.Contains(item[0].OrderNumber))
                    orderNumbers.Add(item[0].OrderNumber);
            }

            return orderNumbers;
        }
        List<string> GetOrderNumbers(List<List<OrderItem>> orderItems)
        {
            List<string> orderNumbers = new List<string>();

            foreach (var item in orderItems)
            {
                if (!orderNumbers.Contains(item[0].OrderNumber))
                    orderNumbers.Add(item[0].OrderNumber);
            }

            return orderNumbers;
        }
        List<string> GetOrderNumbers_4Digit(List<List<OrderItem>> orderItems)
        {
            List<string> orderNumbers = new List<string>();

            foreach (var item in orderItems)
            {
                if (!orderNumbers.Contains(item[0].OrderNumber))
                    orderNumbers.Add(item[0].OrderNumber.Substring(item[0].OrderNumber.Length - 4, 4));
            }

            return orderNumbers;
        }

        UIChangeSource GetUIChangeSource(List<List<OrderItem>> Orders)
        {
            //Rethink this method nigga
            // TODO: Rethink this method nigga
            List<string> orderNumbers = GetCurrentOrderNumbers();

            if (orderNumbers.Count == 0 || ContainsOnlyCollectedOrder(this.orders)) return UIChangeSource.StartUp; //Started POS Up

            List<List<OrderItem>> order = Orders;//Updated Order List

            List<List<OrderItem>> _orders = order.Where(o => !o[0].Collected && !o[0].MarkedForDeletion).ToList();
            //the 'orders' mentioned here are the global orders stored locally  in the POS object

            var updatedOrders = GetOrderNumbers_4Digit(_orders);
            var currentOrders = GetOrderNumbers_4Digit(orders);

            //If updatedOrders contains some offline made orders then UIChangeSource Was Not Online Change
            //Offline Orders Alway Start with a zero so if the list of updated order numbers
            //contains a zero in it then updated orders is inclusive of offline orders
            if (updatedOrders.Where(uO => uO[0] == '0').Count() > 0)
            {
                foreach (var orderNum in currentOrders)
                {
                    if (!updatedOrders.Contains(orderNum))
                        return UIChangeSource.Deletion;
                }
            }


            if (_orders.Count < orders.Count) return UIChangeSource.Addition;//UIChangeSource.Deletion; //if new list has less items than current list deletion occurred

            int count = _orders.Where(o => !orderNumbers.Contains(o[0].OrderNumber) && !o[0].Collected && !o[0].MarkedForDeletion).Count();

            if (count > 0) return UIChangeSource.Addition;

            return UIChangeSource.Edit;
        }

        bool ContainsOnlyCollectedOrder(List<List<OrderItem>> orderItems)
        {
            int count = 0;

            foreach (var orderItem in orderItems)
            {
                count += orderItem.Where(o => o.Collected).ToList().Count();
            }

            return count == orderItems.Count ? true : false;
        }

        StackPanel GetPanel(List<OrderItem> items)
        {
            StackPanel stackPanel = new StackPanel()
            {
                Margin = new Thickness(0, 5, 0, 0),
                Background = new SolidColorBrush(Colors.White)
            };

            StackPanel stackPanel1 = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            string x = items[0].OrderNumber;
            x = x.Substring(x.IndexOf('_') + 1, 4);

            Label label = new Label()
            {
                FontWeight = FontWeights.DemiBold,
                Margin = new Thickness(0, 0, 10, 0),
                Width = 150,
                Content = "Order Number - " + x
            };

            Label label1 = new Label()
            {
                FontWeight = FontWeights.DemiBold,
                Margin = new Thickness(0, 0, 10, 0),
                Width = 80,
                Content = items[0].PhoneNumber
            };

            stackPanel1.Children.Add(label);
            stackPanel1.Children.Add(label1);

            if (true)//!items[0].Purchased)
            {
                Button button = new Button()
                {
                    Background = new SolidColorBrush(Colors.OrangeRed),
                    Width = 110,
                    Margin = new Thickness(20, 0, 0, 0),
                    Foreground = new SolidColorBrush(Colors.White),
                    Content = "Confirm Payment",
                    Name = "o" + items[0].OrderNumber.Replace('-', 'e'),
                    Visibility = items[0].Purchased ? Visibility.Hidden : Visibility.Visible
                };

                button.Click += Payment_Click;

                stackPanel1.Children.Add(button);
            }

            if (true)//items[0].Purchased && items[0].Fufilled)
            {
                Button button = new Button()
                {
                    Background = new SolidColorBrush(Colors.DeepSkyBlue),
                    Width = 110,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(20, 0, 0, 0),
                    Content = "Confirm Collection",
                    Name = "o" + items[0].OrderNumber.Replace('-', 'e'),
                    Visibility = !(items.Where(itm => itm.Purchased).Count() == items.Count && items.Where(itm => itm.Fufilled).Count() == items.Count) ? Visibility.Hidden : Visibility.Visible
                };

                button.Click += Collection_Click;

                stackPanel1.Children.Add(button);
            }

            Button button1 = new Button()
            {
                Background = new SolidColorBrush(Colors.OrangeRed),
                Width = 60,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(20, 0, 0, 0),
                Content = "View",
                Name = "o" + items[0].OrderNumber.Replace('-', 'e')
            };

            button1.Click += View_Click;

            stackPanel1.Children.Add(button1);

            Button button2 = new Button()
            {
                Background = new SolidColorBrush(Colors.Red),
                Width = 110,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(20, 0, 0, 0),
                Content = "Cancel Order",
                IsEnabled = true, //false, //Hidden for now while I work on offline variant
                Visibility = Visibility.Hidden, // items[0].Purchased || !FirebaseDataContext.Instance.connected ? Visibility.Hidden : Visibility.Visible,
                Name = "c" + items[0].OrderNumber.Replace('-', 'e')
            };

            button2.Click += CancelOrder_Click;

            stackPanel1.Children.Add(button2);

            stackPanel.Children.Add(stackPanel1);

            StackPanel stackPanel2 = new StackPanel()
            {
                Visibility = Visibility.Collapsed
            };

            stackPanel.Children.Add(stackPanel2);

            StackPanel stackPanel3 = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            stackPanel2.Children.Add(stackPanel3);

            StackPanel stackPanel4 = new StackPanel();

            for (int i = 0; i < items.Count; i++)
            {
                if (i == 0)
                {
                    Label label2 = new Label()
                    {
                        FontWeight = FontWeights.Bold,
                        Content = "Item"
                    };

                    stackPanel4.Children.Add(label2);
                }

                Label label3 = new Label()
                {
                    Content = items[i].Name//.Substring(0, items[i].Name.IndexOf(" x") + 1),
                };

                stackPanel4.Children.Add(label3);

                if (i == items.Count - 1)
                {
                    Label label2 = new Label()
                    {
                        FontWeight = FontWeights.Bold,
                        Content = "Total"
                    };

                    stackPanel4.Children.Add(label2);
                }
            }

            StackPanel stackPanel4_1 = new StackPanel();

            for (int i = 0; i < items.Count; i++)
            {
                if (i == 0)
                {
                    Label label2 = new Label()
                    {
                        FontWeight = FontWeights.Bold,
                        Content = "Units"
                    };

                    stackPanel4_1.Children.Add(label2);
                }

                Label label3 = new Label()
                {
                    Content = items[i].Quantity
                };

                stackPanel4_1.Children.Add(label3);
            }

            StackPanel stackPanel4_2 = new StackPanel();

            for (int i = 0; i < items.Count; i++)
            {
                if (i == 0)
                {
                    Label label2 = new Label()
                    {
                        FontWeight = FontWeights.Bold,
                        Content = "Unit Price"
                    };

                    stackPanel4_2.Children.Add(label2);
                }

                Label label3 = new Label()
                {
                    Content = Formatting.FormatAmountString(float.Parse(items[i].Price) / (float)items[i].Quantity)
                };

                stackPanel4_2.Children.Add(label3);
            }

            StackPanel stackPanel4_3 = new StackPanel();

            for (int i = 0; i < items.Count; i++)
            {
                if (i == 0)
                {
                    Label label2 = new Label()
                    {
                        FontWeight = FontWeights.Bold,
                        Content = "Flavour"
                    };

                    stackPanel4_3.Children.Add(label2);
                }

                Label label3 = new Label()
                {
                    Content = "None"
                };

                if (items[i].SubCategory != "Chicken" && items[i].SubCategory != "Platter")
                {
                    stackPanel4_3.Children.Add(label3);
                    continue;
                }

                if (items[i].SubCategory == "Platter" && !items[i].Name.ToLower().Contains("chicken"))
                {
                    stackPanel4_3.Children.Add(label3);
                    continue;
                }


                label3 = new Label()
                {
                    Content = items[i].Flavour == "None" ? "None" : items[i].Flavour
                };

                stackPanel4_3.Children.Add(label3);
            }

            StackPanel stackPanel4_4 = new StackPanel();

            for (int i = 0; i < items.Count; i++)
            {
                if (i == 0)
                {
                    Label label2 = new Label()
                    {
                        FontWeight = FontWeights.Bold,
                        Content = "Readiness"
                    };
                    stackPanel4_4.Children.Add(label2);
                }

                Label label3 = new Label()
                {
                    Content = "None"
                };

                if (items[i].SubCategory != "Steak")
                {
                    stackPanel4_4.Children.Add(label3);
                    continue;
                }

                label3 = new Label()
                {
                    Content = items[i].MeatTemperature
                };

                stackPanel4_4.Children.Add(label3);
            }

            StackPanel stackPanel4_5 = new StackPanel();

            for (int i = 0; i < items.Count; i++)
            {
                if (i == 0)
                {
                    Label label2 = new Label()
                    {
                        FontWeight = FontWeights.Bold,
                        Content = "Sauces"
                    };
                    stackPanel4_5.Children.Add(label2);
                }

                Label label3 = new Label()
                {
                    Content = "None"
                };

                if (items[i].Category != "Meat")
                {
                    stackPanel4_5.Children.Add(label3);
                    continue;
                }

                label3 = new Label()
                {
                    Content = items[i].Sauces == null ? "None" : Formatting.FormatListToString(items[i].Sauces)
                };

                stackPanel4_5.Children.Add(label3);
            }

            StackPanel stackPanel5 = new StackPanel();

            for (int i = 0; i < items.Count; i++)
            {
                if (i == 0)
                {
                    Label label2 = new Label()
                    {
                        FontWeight = FontWeights.Bold,
                        Content = "Price"
                    };

                    stackPanel5.Children.Add(label2);
                }

                Label label3 = new Label()
                {
                    Content = Formatting.FormatAmountString(float.Parse(items[i].Price)),
                };

                stackPanel5.Children.Add(label3);

                if (i == items.Count - 1)
                {
                    float total = 0;

                    foreach (var item in items)
                    {
                        total += float.Parse(item.Price);
                    }

                    Label label2 = new Label()
                    {
                        FontWeight = FontWeights.Bold,
                        Content = Formatting.FormatAmountString(total),
                    };

                    stackPanel5.Children.Add(label2);
                }
            }

            stackPanel3.Children.Add(stackPanel4);
            stackPanel3.Children.Add(stackPanel4_1);
            stackPanel3.Children.Add(stackPanel4_2);
            stackPanel3.Children.Add(stackPanel4_3);
            stackPanel3.Children.Add(stackPanel4_4);
            stackPanel3.Children.Add(stackPanel4_5);
            stackPanel3.Children.Add(stackPanel5);

            return stackPanel;
        }

        //Order Functions

        public async void OnTransaction(string orderNumber, List<OrderItem> order)
        {
            ActivityIndicator.AddSpinner(spinner);

            string n = orderNumber;

            foreach (var item in order)
            {
                string branchId = BranchSettings.Instance.branchId;
                string fullPath = "Order/" + branchId + "/" + n + "/" + item.Id.ToString();

                if (TCPServer.Instance != null)
                    await firebaseDataContext.StoreData(fullPath, item);
            }

            if (TCPServer.Instance == null)
                await firebaseDataContext.StoreData("Order/", order);
        }

        private async void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string n = button.Name.Replace('e', '-');
            n = n.Remove(0, 1);

            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to cancel order #" + n.Remove(0, 11) + "?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                ActivityIndicator.AddSpinner(spinner);
                for (int i = 0; i < orders.Count; i++)
                {
                    if (orders[i][0].OrderNumber == n)
                    {
                        SendSMS(orders[i][0].PhoneNumber, n.Remove(0, 11));

                        await firebaseDataContext.CancelOrder(orders[i]);
                    }
                }
            }
        }

        private async void Collection_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            ActivityIndicator.AddSpinner(spinner);

            for (int i = 0; i < orders.Count; i++)
            {
                string n = button.Name.Replace('e', '-');
                n = n.Remove(0, 1);

                if (orders[i][0].OrderNumber == n)
                {
                    foreach (var item in orders[i])
                    {
                        item.Collected = true;
                        item.User = LocalStorage.Instance.user.FullName();
                    }

                    string branchId = "";
                    string fullPath = "";

                    branchId = BranchSettings.Instance.branchId;
                    fullPath = "Order/" + branchId;

                    foreach (var item in orders[i])
                    {
                        if (TCPServer.Instance != null)
                            await firebaseDataContext.StoreData(fullPath, item);
                    }

                    if (TCPServer.Instance == null)
                        await firebaseDataContext.StoreData("Order/", orders[i]);

                    return;
                }
            }
        }

        async void SendSMS(string phoneNumber, string orderNumber)
        {
            await client.PostAsync("https://rodizioexpress.com/api/sms/send/cancel/" + phoneNumber + "/" + orderNumber, null);
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            StackPanel mainPanel = (StackPanel)(((StackPanel)button.Parent).Parent);

            StackPanel panel = (StackPanel)mainPanel.Children[1];

            if (panel.Visibility == Visibility.Collapsed)
            {
                panel.Visibility = Visibility.Visible;
                button.Content = "Hide";
            }
            else
            {
                panel.Visibility = Visibility.Collapsed;
                button.Content = "View";
            }
        }

        void UpdateOrderCount()
        {
            activeOrdersCount.Text = orderViewer.Children.Count + " - Active Orders";
        }

        private void Payment_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            for (int i = 0; i < orders.Count; i++)
            {
                string n = button.Name.Replace('e', '-');
                n = n.Remove(0, 1);
                if (orders[i][0].OrderNumber == n)
                {
                    WindowManager.Instance.Open(new ReceivePayment(orders[i], this));
                }
            }
        }
        int block = 0;
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (block == 0)
            {
                searchBox.IsEnabled = false;
                searchBox.IsReadOnly = true;

                Search(searchBox.Text);
                block = 1;
            }
        }

        private void Search(string query)
        {
            List<List<OrderItem>> result = new SearchOrders().Search(query, orders);

            showingResults = true;

            orders.Clear();
            orderViewer.Children.Clear();

            if (result.Count != 0)
            {
                UpdateOrderView(result, UIChangeSource.Search);
                return;
            }

            string messageBoxText = "Your search had 0 results.";
            string caption = "Search";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            orders.Clear();
            orderViewer.Children.Clear();

            block = 0;

            Search(searchBox.Text);
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (showingResults)
            {
                showingResults = false;

                searchBox.Text = "";
                searchBox.IsEnabled = true;
                searchBox.IsReadOnly = false;

                block = 0;

                orders.Clear();
                orderViewer.Children.Clear();

                OnStart(); //Since We need to reload all orders
            }
        }

        private void Cashier_Report_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.Instance.Open(new CashierReport());
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.Instance.CloseAllAndOpen(new Login());
        }

        private void Statuses_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.Instance.Open(new OrderStatus(orders));
        }

        private async void Menu_Click(object sender, RoutedEventArgs e)
        {
            if (!await FirebaseDataContext.Instance.connectionChecker.CheckConnection())
            {
                ShowWarning("You need to be online or on the server computer to access the menu page.");
                return;
            }

            WindowManager.Instance.Open(new MenuEditor());
        }

        private void NewOrder_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.Instance.Open(new OrderSource());
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.Instance.Open(new Settings());
        }

        void ShowWarning(string msg)
        {
            string messageBoxText = msg;
            string caption = "Warning";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }
    }
}