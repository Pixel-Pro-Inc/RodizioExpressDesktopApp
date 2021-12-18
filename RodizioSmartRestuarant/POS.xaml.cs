using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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
    /// Interaction logic for POS.xaml
    /// </summary>
    public partial class POS : Window
    {
        static List<List<OrderItem>> orders = new List<List<OrderItem>>();
        private FirebaseDataContext firebaseDataContext;
        private bool showingResults;

        public POS()
        {
            InitializeComponent();

            firebaseDataContext = FirebaseDataContext.Instance;

            OnStart();                        
        }

        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        async void OnStart()
        {
            var result = await firebaseDataContext.GetData("Order/" + BranchSettings.Instance.branchId);            

            List<List<OrderItem>> temp = new List<List<OrderItem>>();

            foreach (var item in result)
            {
                List<OrderItem> data = JsonConvert.DeserializeObject<List<OrderItem>>(((JArray)item).ToString());

                temp.Add(data);
            }

            UpdateOrderView(temp, UIChangeSource.Addition);          
        }

        public async void OnTransaction(string orderNumber, List<OrderItem> order)
        {
            string n = orderNumber;

            foreach (var item in order)
            {
                string branchId = BranchSettings.Instance.branchId;
                string fullPath = "Order/" + branchId + "/" + n + "/" + item.Id.ToString();

                await firebaseDataContext.EditData(fullPath, item);
            }            
        }

        public void UpdateOrderView(List<List<OrderItem>> temp, UIChangeSource source)
        {
            switch (source)
            {
                case UIChangeSource.Addition:
                    for (int i = 0; i < temp.Count; i++)
                    {
                        var item = temp[i];

                        if (i >= orderViewer.Children.Count)
                        {
                            if (!item[0].Collected)
                            {
                                orders.Add(item);

                                orderViewer.Children.Add(GetPanel(item));
                            }
                        }
                    }
                    break;
                case UIChangeSource.Edit:
                    //Replace Edited Items
                    for (int i = 0; i < temp.Count; i++)
                    {
                        var item = temp[i];

                        if (item != orders[i])
                        {
                            orders[i] = item;

                            orderViewer.Children[i] = GetPanel(item);
                        }
                    }

                    //Hide Collected Items
                    for (int i = 0; i < temp.Count; i++)
                    {
                        var item = temp[i];

                        if (item[0].Collected)
                        {
                            orders.RemoveAt(i);

                            orderViewer.Children.RemoveAt(i);
                        }
                    }
                    break;
                default:
                    break;
            }            

            UpdateOrderCount();
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

            if (!items[0].Purchased)
            {
                Button button = new Button()
                {
                    Background = new SolidColorBrush(Colors.OrangeRed),
                    Width = 110,
                    Margin = new Thickness(20,0,0,0),
                    Foreground = new SolidColorBrush(Colors.White),
                    Content = "Confirm Payment",
                    Name = "o" + items[0].OrderNumber.Replace('-', 'e')
                };

                button.Click += Payment_Click;

                stackPanel1.Children.Add(button);
            }

            if (items[0].Purchased && items[0].Fufilled)
            {
                Button button = new Button()
                {
                    Background = new SolidColorBrush(Colors.DeepSkyBlue),
                    Width = 110,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(20, 0, 0, 0),
                    Content = "Confirm Collection",
                    Name = "o" + items[0].OrderNumber.Replace('-', 'e')
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
                    Content = items[i].Name
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
                    Content = items[i].Price
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
                        Content = total.ToString("f2")
                    };

                    stackPanel5.Children.Add(label2);
                }
            }

            stackPanel3.Children.Add(stackPanel4);
            stackPanel3.Children.Add(stackPanel5);

            return stackPanel;
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            StackPanel mainPanel = (StackPanel)(((StackPanel)button.Parent).Parent);

            StackPanel panel = (StackPanel)mainPanel.Children[1];

            if(panel.Visibility == Visibility.Collapsed)
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

        private async void Collection_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            for (int i = 0; i < orders.Count; i++)
            {
                string n = button.Name.Replace('e', '-');
                n = n.Remove(0, 1);

                if (orders[i][0].OrderNumber == n)
                {
                    foreach (var item in orders[i])
                    {
                        item.Collected = true;
                    }

                    foreach (var item in orders[i])
                    {
                        string branchId = BranchSettings.Instance.branchId;
                        string fullPath = "Order/" + branchId + "/" + orders[i][0].OrderNumber + "/" + item.Id.ToString();

                        await firebaseDataContext.EditData(fullPath, item);
                    }
                }
            }
        }

        void UpdateOrderCount()
        {
            activeOrdersCount.Content = orderViewer.Children.Count + " - Active Orders";

            FirebaseDataContext.Instance.count1 = 1;
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

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.Instance.CloseAllAndOpen(new LoadingScreen());
        }

        private void Statuses_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.Instance.Open(new OrderStatus(orders));
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.Instance.Open(new MenuEditor());
        }

        private void NewOrder_Click(object sender, RoutedEventArgs e)
        {
            WindowManager.Instance.Open(new NewOrder());
        }
        private void CancelButton_Clicked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
            //Receipt Maybe
        }
        private void NewOrderButton_Clicked(object sender, RoutedEventArgs e)
        {
            Window current = this;
            Window next = new NewOrderPO();

            current.Hide();

            next.Show();
            //I dont want to close it but its not for any real reason
        }
        private void LogoutButton_Clicked(object sender, RoutedEventArgs e)
        {
            LocalStorage.Instance.user = null;

            Window current = this;
            Window next = new Login();

            current.Hide();

            next.Show();

            current.Close();
        }

    }
}
