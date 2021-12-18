using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Helpers;
using RodizioSmartRestuarant.Entities;
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
using MenuItem = RodizioSmartRestuarant.Entities.MenuItem;
using RodizioSmartRestuarant.Data;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for NewOrder.xaml
    /// </summary>
    public partial class NewOrder : Window
    {
        List<MenuItem> menuItems = new List<MenuItem>();
        List<OrderItem> orders = new List<OrderItem>();
        private FirebaseDataContext firebaseDataContext;
        public NewOrder()
        {
            InitializeComponent();

            firebaseDataContext = FirebaseDataContext.Instance;

            UpdateMenuView();
        }

        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        async void UpdateMenuView()
        {
            menuView.Children.Clear();

            var result = await firebaseDataContext.GetData("Menu/" + BranchSettings.Instance.branchId);

            List<MenuItem> items = new List<MenuItem>();

            foreach (var item in result)
            {
                MenuItem menuItem = JsonConvert.DeserializeObject<MenuItem>(((JObject)item).ToString());

                items.Add(menuItem);
            }

            menuItems = items;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Availability)
                    menuView.Children.Add(GetPanel(items[i]));
            }
        }

        void UpdateMenuViewSearch(List<MenuItem> items)
        {
            menuView.Children.Clear();

            for (int i = 0; i < items.Count; i++)
            {
                menuView.Children.Add(GetPanel(items[i]));
            }
        }

        StackPanel GetPanel(MenuItem menuItem)
        {
            StackPanel stackPanel = new StackPanel()
            {
                Margin = new Thickness(10),
                Background = new SolidColorBrush(Colors.White)
            };

            Label label = new Label()
            {
                Content = menuItem.Name
            };

            stackPanel.Children.Add(label);

            if (menuItem.Category != "Meat")
            {
                Label label1 = new Label()
                {
                    Content = "BWP " + menuItem.Price
                };

                stackPanel.Children.Add(label1);
            }

            if (menuItem.Category == "Meat")
            {
                Label label1 = new Label()
                {
                    Content = "Minimum price = " + menuItem.MinimumPrice
                };

                TextBox textBox = new TextBox()
                {
                    Name = "r" + menuItem.Id
                };

                textBox.TextChanged += Price_TextChanged;

                Label label2 = new Label()
                {
                    Content = "Weight = " + 0 + " grams"
                };

                stackPanel.Children.Add(label1);
                stackPanel.Children.Add(textBox);
                stackPanel.Children.Add(label2);
            }

            Button button = new Button()
            {
                Content = "Add",
                Name = "n" + menuItem.Id
            };

            button.Click += Add_Click;

            if (menuItem.Category == "Meat")
            {
                button.Visibility = Visibility.Collapsed;
            }
            
            stackPanel.Children.Add(button);

            return stackPanel;
        }

        void UpdateOrderView()
        {
            orderView.Children.Clear();

            for (int i = 0; i < orders.Count; i++)
            {
                orders[i].Id = i;

                orderView.Children.Add(GetStackPanel(orders[i], i));
            }

            UpdateTotal();
        }

        StackPanel GetStackPanel(OrderItem orderItem, int index)
        {
            StackPanel stackPanel = new StackPanel()
            {
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#343434"),
                Orientation = Orientation.Horizontal
            };

            TextBlock label = new TextBlock()
            {
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(5,0,30,0),
                Width = 100,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.WrapWithOverflow,
                Text = orderItem.Name
            };

            Label label1 = new Label()
            {
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 30, 0),
                Width = 80,
                Content = orderItem.Weight != null? orderItem.Weight + " grams": "- grams"
            };

            float x = float.Parse(orderItem.Price);

            Label label2 = new Label()
            {
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 30, 0),
                Width = 80,
                Content ="BWP " + x.ToString("f2")
            };

            Button button = new Button()
            {
                Foreground = new SolidColorBrush(Colors.White),
                Background = new SolidColorBrush(Colors.OrangeRed),
                Content = "Remove",
                Name = "a" + index
            };

            stackPanel.Children.Add(label);
            stackPanel.Children.Add(label1);
            stackPanel.Children.Add(label2);
            stackPanel.Children.Add(button);

            button.Click += Remove_Click;

            return stackPanel;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string id = button.Name.Remove(0, 1);

            int numId = Int32.Parse(id);

            for (int i = 0; i < orders.Count; i++)
            {
                if (orders[i].Id == numId)
                {
                    orders.RemoveAt(i);

                    UpdateOrderView();

                    return;
                }
            }
        }

        private void Price_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            float inputNum = 0;

            if (textBox.Text != "" && textBox.Text != null)
                if (float.TryParse(textBox.Text, out inputNum))
                {
                    string id = textBox.Name.Remove(0, 1);

                    int numId = Int32.Parse(id);

                    for (int i = 0; i < menuItems.Count; i++)
                    {
                        if (menuItems[i].Id == numId)
                        {
                            if(inputNum >= menuItems[i].MinimumPrice)
                            {
                                StackPanel stack = (StackPanel)textBox.Parent;
                                ((Label)stack.Children[3]).Content = "Weight = " + menuItems[i].Rate * inputNum + " grams";

                                ((Button)stack.Children[4]).Visibility = Visibility.Visible;

                                return;
                            }

                            StackPanel stack1 = (StackPanel)textBox.Parent;
                            ((Button)stack1.Children[4]).Visibility = Visibility.Collapsed;
                        }
                    }
                }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            string id = button.Name.Remove(0, 1);

            int numId = Int32.Parse(id);

            for (int i = 0; i < menuItems.Count; i++)
            {
                if (menuItems[i].Id == numId)
                {
                    if(menuItems[i].Category == "Meat")
                    {
                        string price = ((TextBox)((StackPanel)button.Parent).Children[2]).Text;
                        string weight = (float.Parse(price) * menuItems[i].Rate).ToString();

                        orders.Add(new OrderItem()
                        {
                            Collected = false,
                            Description = menuItems[i].Description,
                            Fufilled = false,
                            Name = menuItems[i].Name,
                            Preparable = false,
                            Price = price,
                            Purchased = false,
                            WaitingForPayment = true,
                            Quantity = 1,
                            Reference = "till",
                            Weight = weight
                        });
                    }
                    else
                    {
                        orders.Add(new OrderItem()
                        {
                            Collected = false,
                            Description = menuItems[i].Description,
                            Fufilled = false,
                            Name = menuItems[i].Name,
                            Preparable = false,
                            Price = menuItems[i].Price,
                            Purchased = false,
                            WaitingForPayment = true,
                            Quantity = 1,
                            Reference = "till",
                        });
                    }                    

                    UpdateOrderView();
                }
            }
        }

        async void ConfirmOrder(List<OrderItem> orderItems)
        {
            int x = await GetOrderNum(BranchSettings.Instance.branchId);

            string d = DateTime.Now.Day.ToString("00") + "/" + DateTime.Now.Month.ToString("00") + "/"
                    + DateTime.Now.Year.ToString("0000");

            for (int i = 0; i < orderItems.Count; i++)
            {
                var orderItem = orderItems[i];

                orderItem.OrderNumber = d + "_" + x;
                orderItem.OrderNumber = orderItem.OrderNumber.Replace('/', '-');

                orderItem.PhoneNumber = phoneNumber.Text;

                orderItem.Id = i;

                await firebaseDataContext.StoreData("Order/" + BranchSettings.Instance.branchId + "/" + orderItem.OrderNumber + "/" + orderItem.Id, orderItem);
            }            

            phoneNumber.Text = "";

            Window pos = null;

            for (int i = 0; i < WindowManager.Instance.openWindows.Count; i++)
            {
                string xs = WindowManager.Instance.openWindows[i].GetType().FullName;

                if (WindowManager.Instance.openWindows[i].GetType() == typeof(POS))
                {
                    pos = WindowManager.Instance.openWindows[i];
                }                
            }

            if (pos == null)
                pos = new POS();            

            WindowManager.Instance.CloseAndOpen(this, new ReceivePayment(orderItems, (POS)pos));

            UpdateOrderView();
        }

        public async Task<int> GetOrderNum(string branchId)
        {
            var response = await firebaseDataContext.GetData("Order/" + branchId);
            List<OrderItem> orders = new List<OrderItem>();

            foreach (var item in response)
            {
                OrderItem[] data = JsonConvert.DeserializeObject<OrderItem[]>(((JArray)item).ToString());

                for (int i = 0; i < data.Length; i++)
                {
                    orders.Add(data[i]);
                }
            }

            int candidateNumber = new Random().Next(1000, 9999);

            //Check Against Other Order Numbers For The Day
            List<int> orderNums = new List<int>();

            for (int i = 0; i < orders.Count; i++)
            {
                //Only 4 digit numbers

                string orderNum = orders[i].OrderNumber;

                int n = orderNum.IndexOf('_');

                string date = orderNum.Remove(n, 5);

                string number = orderNum.Remove(0, n + 1);

                string dateToday =
                    DateTime.Now.Day.ToString("00") + "-"
                    + DateTime.Now.Month.ToString("00") + "-"
                    + DateTime.Now.Year.ToString("0000");

                if (date == dateToday)
                {
                    orderNums.Add((Int32.Parse(number)));
                }
            }

            while (orderNums.Contains(candidateNumber))
            {
                candidateNumber = new Random().Next(1000, 9999);
            }

            return candidateNumber;
        }

        void UpdateTotal()
        {
            float totalAmt = 0;

            for (int i = 0; i < orders.Count; i++)
            {
                totalAmt += float.Parse(orders[i].Price);
            }

            total.Content = totalAmt.ToString("f2");
        }

        //Search

        int block;
        bool showingResults;

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
            List<MenuItem> result = new SearchMenu().Search(query, menuItems);

            showingResults = true;

            if (result.Count != 0)
            {
                UpdateMenuViewSearch(result);
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

                UpdateMenuView();
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            ConfirmOrder(orders);
        }

        private void phoneNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (phoneNumber.Text != null)
                if (phoneNumber.Text.Length == 8)
                {
                    confirmButton.Visibility = Visibility.Visible;

                    return;
                }

            confirmButton.Visibility = Visibility.Collapsed;

        }
    }
}
