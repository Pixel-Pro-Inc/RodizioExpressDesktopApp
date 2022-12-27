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
using System.Windows.Media;
using MenuItem = RodizioSmartRestuarant.Entities.MenuItem;
using RodizioSmartRestuarant.Data;
using Formatting = RodizioSmartRestuarant.Helpers.Formatting;
using RodizioSmartRestuarant.CustomBaseClasses.BaseClasses;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for NewOrder.xaml
    /// </summary>
    public partial class NewOrder : BaseWindow
    {
        List<MenuItem> menuItems = new List<MenuItem>();
        List<OrderItem> orders = new List<OrderItem>();
        private FirebaseDataContext firebaseDataContext;
        private string _source;

        string lastSelectedFlavour = "None";
        string lastSelectedMeatTemp = "Well Done";
        string lastSelectedSauce = "Lemon & Garlic";

        bool optForSMS = true;
        public NewOrder(string source)
        {
            InitializeComponent();

            _source = source;

            if (_source.ToLower() != "walkin")
                checkbox.Visibility = Visibility.Collapsed;

            firebaseDataContext = FirebaseDataContext.Instance;

            UpdateMenuView();
        }

        // TODO: New order bug
        /*
         Say you want to remove an order placed, there is no functionality to remove it, you just have to start the order all afresh

         */
        async void UpdateMenuView()
        {
            ActivityIndicator.AddSpinner(spinner);

            menuView.Children.Clear();

            var result = await firebaseDataContext.GetData_Online("Menu/" + BranchSettings.Instance.branchId);

            List<MenuItem> items = new List<MenuItem>();

            foreach (var item in result)
            {
                if (item != null)
                {
                    MenuItem menuItem = JsonConvert.DeserializeObject<MenuItem>(((JObject)item).ToString());

                    items.Add(menuItem);
                }                
            }

            menuItems = items;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Availability)
                    menuView.Children.Add(GetPanel(items[i]));
            }

            //Updates with size settings
            RodizioSmartRestuarant.Helpers.Settings.Instance.OnWindowCountChange();

            ActivityIndicator.RemoveSpinner(spinner);
        }

        void UpdateMenuViewSearch(List<MenuItem> items)
        {
            menuView.Children.Clear();

            for (int i = 0; i < items.Count; i++)
            {
                menuView.Children.Add(GetPanel(items[i]));
            }

            //Updates with size settings
            RodizioSmartRestuarant.Helpers.Settings.Instance.OnWindowCountChange();
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

            if (menuItem.Category != "Meat" || menuItem.Price != "0.00")
            {
                Label label1 = new Label()
                {
                    Content = "BWP " + Formatting.FormatAmountString(float.Parse(menuItem.Price))
                };

                stackPanel.Children.Add(label1);
            }

            if (menuItem.Category == "Meat" && menuItem.Price == "0.00")
            {
                Label label1 = new Label()
                {
                    Content = "Minimum price = " + Formatting.FormatAmountString(menuItem.MinimumPrice)
                };

                TextBox textBox = new TextBox()
                {
                    Name = "r" + menuItem.ID
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

            //Quantity Counter
            Label label3 = new Label()
            {
                Content = "Quantity"
            };

            TextBox textBox1 = new TextBox()
            {
                Name = "q" + menuItem.ID,
                Text = "1"
            };

            textBox1.TextChanged += Quantity_TextChanged;
            stackPanel.Children.Add(label3);
            stackPanel.Children.Add(textBox1);

            //Dropdown Flavours
            if(menuItem.Flavours != null)
            {
                if(menuItem.Flavours.Count > 0)
                {
                    Label label3_1 = new Label()
                    {
                        Content = "Flavour"
                    };

                    ComboBox comboBox = new ComboBox();
                    comboBox.Items.Add((new ComboBoxItem()).Content = (new TextBlock()).Text = "None");

                    foreach (var flavour in menuItem.Flavours)
                    {
                        comboBox.Items.Add((new ComboBoxItem()).Content = (new TextBlock()).Text = flavour);
                    }

                    comboBox.SelectionChanged += Flavour_SelectionChanged;
                    
                    stackPanel.Children.Add(label3_1);
                    stackPanel.Children.Add(comboBox);
                }
            }

            //Dropdown Meat Temp
            if (menuItem.MeatTemperatures != null)
            {
                if (menuItem.MeatTemperatures.Count > 0)
                {
                    Label label3_1 = new Label()
                    {
                        Content = "Readiness"
                    };

                    ComboBox comboBox = new ComboBox();

                    for (int i = menuItem.MeatTemperatures.Count - 1; i >= 0; i--)
                    {
                        var meatTemp = menuItem.MeatTemperatures[i];

                        comboBox.Items.Add((new ComboBoxItem()).Content = (new TextBlock()).Text = meatTemp);
                    }

                    comboBox.SelectionChanged += MeatTemperature_SelectionChanged;

                    stackPanel.Children.Add(label3_1);
                    stackPanel.Children.Add(comboBox);
                }
            }

            //Dropdown Sauces
            if (menuItem.Sauces != null)
            {
                if (menuItem.Sauces.Count > 0)
                {
                    if(menuItem.SubCategory != "Platter")
                    {
                        Label label3_1 = new Label()
                        {
                            Content = "Sauces"
                        };

                        ComboBox comboBox = new ComboBox();

                        for (int i = 0; i < menuItem.Sauces.Count; i++)
                        {
                            var sauce = menuItem.Sauces[i];

                            comboBox.Items.Add((new ComboBoxItem()).Content = (new TextBlock()).Text = sauce);
                        }

                        comboBox.SelectionChanged += Sauce_SelectionChanged;

                        stackPanel.Children.Add(label3_1);
                        stackPanel.Children.Add(comboBox);
                    }                    
                }
            }

            Button button = new Button()
            {
                Content = "Add",
                Name = "n" + menuItem.ID
            };

            button.Click += Add_Click;

            if (menuItem.Category == "Meat")
            {
                if(menuItem.Price != "0.00")
                {
                    button.Visibility = Visibility.Visible;
                }
                else
                {
                    button.Visibility = Visibility.Collapsed;
                }
            }
            
            stackPanel.Children.Add(button);

            //Updates with size settings
            RodizioSmartRestuarant.Helpers.Settings.Instance.OnWindowCountChange();

            if (!menuItem.Availability)
                stackPanel.Visibility = Visibility.Collapsed;

            return stackPanel;
        }

        private void Sauce_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lastSelectedSauce = (((ComboBox)sender).SelectedItem).ToString();
        }

        private void MeatTemperature_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lastSelectedMeatTemp = (((ComboBox)sender).SelectedItem).ToString();
        }

        private void Flavour_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lastSelectedFlavour = (((ComboBox)sender).SelectedItem).ToString();
        }

        int lastQuantity = 1;

        private void Quantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            float inputNum = 0;

            if (textBox.Text != "" && textBox.Text != null)
                if (float.TryParse(textBox.Text, out inputNum))
                {
                    string id = textBox.Name.Remove(0, 1);

                    for (int i = 0; i < menuItems.Count; i++)
                    {
                        if (menuItems[i].ID == id)
                        {
                            if (inputNum >= 1)
                            {
                                lastQuantity = (int)inputNum;
                                return;
                            }

                            StackPanel stack1 = (StackPanel)textBox.Parent;
                            ((Button)stack1.Children[stack1.Children.Count - 1]).Visibility = Visibility.Collapsed;
                        }
                    }
                }
        }

        void UpdateOrderView()
        {
            orderView.Children.Clear();

            for (int i = 0; i < orders.Count; i++)
            {
                orders[i].Index = i;

                orderView.Children.Add(GetStackPanel(orders[i], i));
            }

            //Updates with size settings
            RodizioSmartRestuarant.Helpers.Settings.Instance.OnWindowCountChange();

            UpdateTotal();
            UpdateOrderPrepTime(orders);
            CheckChanged();
        }

        StackPanel GetStackPanel(OrderItem orderItem, int index)
        {
            StackPanel mainStackPanel = new StackPanel()
            {
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#343434"),
            };

            StackPanel stackPanel = new StackPanel()
            {
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#343434"),
                Orientation = Orientation.Horizontal
            };

            Label label = new Label()
            {
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(5,0,10,0),
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth= 200,
                Content = orderItem.Name + " x " + orderItem.Quantity
            };

            Label label1 = new Label()
            {
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 10, 0),
                Width = 80,
                Content = orderItem.Weight != null? orderItem.Weight + " grams" : "- grams"
            };

            float x = float.Parse(orderItem.Price);

            Label label2 = new Label()
            {
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 10, 0),
                Width = 80,
                Content ="BWP " + Formatting.FormatAmountString(x)
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

            StackPanel orderPropertiesStackPanel = new StackPanel()
            {
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#343434"),
                Orientation = Orientation.Horizontal
            };

            bool condition = true;

            if (orderItem.SubCategory != "Chicken" && orderItem.SubCategory != "Platter")
                condition = false;

            if (orderItem.SubCategory == "Platter" && !orderItem.Name.ToLower().Contains("chicken"))
                condition = false;

            Label label_1 = new Label()
            {
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(5, 2, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 200,
                Content = condition? $"Flavour : " + orderItem.Flavour : "Flavour : None",
                Visibility = condition? Visibility.Visible : Visibility.Collapsed
            };

            condition = true;

            if (orderItem.SubCategory != "Steak")
                condition = false;

            orderPropertiesStackPanel.Children.Add(label_1);

            Label label_2 = new Label()
            {
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(5, 2, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 200,
                Content = condition? $"Readiness : " + orderItem.MeatTemperature : "Readiness : None",
                Visibility = condition? Visibility.Visible : Visibility.Collapsed
            };

            orderPropertiesStackPanel.Children.Add(label_2);

            condition = true;

            if (orderItem.Category != "Meat")
                condition = false;

            Label label_3 = new Label()
            {
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(5, 2, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 200,
                Content = condition? $"Sauces : " + orderItem.Sauces == null? "Sauces : None" : "Sauces : " + Formatting.FormatListToString(orderItem.Sauces) : "Sauces : None",
                Visibility = condition ? Visibility.Visible : Visibility.Collapsed
            };

            orderPropertiesStackPanel.Children.Add(label_3);

            //Updates with size settings
            RodizioSmartRestuarant.Helpers.Settings.Instance.OnWindowCountChange();

            mainStackPanel.Children.Add(stackPanel);
            mainStackPanel.Children.Add(orderPropertiesStackPanel);

            return mainStackPanel;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string id = button.Name.Remove(0, 1);

            for (int i = 0; i < orders.Count; i++)
            {
                if (orders[i].Index == Int32.Parse(id))
                {
                    orders.RemoveAt(i);

                    UpdateOrderView();

                    CheckChanged();

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

                    for (int i = 0; i < menuItems.Count; i++)
                    {
                        if (menuItems[i].ID == id)
                        {
                            if(inputNum >= menuItems[i].MinimumPrice)
                            {
                                StackPanel stack = (StackPanel)textBox.Parent;
                                ((Label)stack.Children[3]).Content = "Weight = " + menuItems[i].Rate * inputNum + " grams";

                                ((Button)stack.Children[stack.Children.Count - 1]).Visibility = Visibility.Visible;

                                return;
                            }

                            StackPanel stack1 = (StackPanel)textBox.Parent;
                            ((Button)stack1.Children[stack1.Children.Count - 1]).Visibility = Visibility.Collapsed;
                        }
                    }
                }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            string id = button.Name.Remove(0, 1);

            for (int i = 0; i < menuItems.Count; i++)
            {
                if (menuItems[i].ID == id)
                {
                    if(menuItems[i].Category == "Meat" && menuItems[i].Price == "0.00")
                    {
                        string price = ((TextBox)((StackPanel)button.Parent).Children[2]).Text;
                        string weight = (float.Parse(price) * menuItems[i].Rate).ToString("f2") + " grams";

                        orders.Add(new OrderItem()
                        {
                            Collected = false,
                            Description = menuItems[i].Description,
                            Fufilled = false,
                            Name = menuItems[i].Name,
                            Preparable = false,
                            Price = Formatting.FormatAmountString((float.Parse(price) * (float)lastQuantity)),
                            Purchased = false,
                            WaitingForPayment = true,
                            PaymentMethod = "",
                            Quantity = lastQuantity,
                            Reference = "till",
                            Category = menuItems[i].Category,
                            Weight = weight,
                            PrepTime = Int32.Parse(menuItems[i].prepTime),
                            Flavour = lastSelectedFlavour,
                            MeatTemperature = lastSelectedMeatTemp,
                            Sauces = menuItems[i].SubCategory != "Platter"? new List<string>() { lastSelectedSauce } : menuItems[i].Sauces,
                            SubCategory = menuItems[i].SubCategory
                        });
                    }
                    else if (menuItems[i].Category == "Meat" && menuItems[i].Price != "0.00")
                    {
                        orders.Add(new OrderItem()
                        {
                            Collected = false,
                            Description = menuItems[i].Description,
                            Fufilled = false,
                            Name = menuItems[i].Name,
                            Preparable = false,
                            Price = Formatting.FormatAmountString((float.Parse(menuItems[i].Price) * (float)lastQuantity)),
                            Purchased = false,
                            WaitingForPayment = true,
                            PaymentMethod = "",
                            Quantity = lastQuantity,
                            Reference = "till",
                            Category = menuItems[i].Category,
                            Weight = menuItems[i].Weight,
                            PrepTime = Int32.Parse(menuItems[i].prepTime),
                            Flavour = lastSelectedFlavour,
                            MeatTemperature = lastSelectedMeatTemp,
                            Sauces = menuItems[i].SubCategory != "Platter" ? new List<string>() { lastSelectedSauce } : menuItems[i].Sauces,
                            SubCategory = menuItems[i].SubCategory
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
                            Price = Formatting.FormatAmountString((float.Parse(menuItems[i].Price) * (float)lastQuantity)),
                            PaymentMethod = "",
                            Purchased = false,
                            Category = menuItems[i].Category,
                            WaitingForPayment = true,
                            Quantity = lastQuantity,
                            Reference = "till",
                            PrepTime = Int32.Parse(menuItems[i].prepTime),
                            Flavour = lastSelectedFlavour,
                            MeatTemperature = lastSelectedMeatTemp,
                            Sauces = menuItems[i].SubCategory != "Platter" ? new List<string>() { lastSelectedSauce } : menuItems[i].Sauces,
                            SubCategory = menuItems[i].SubCategory
                        });
                    }

                    lastSelectedFlavour = "None";
                    lastSelectedMeatTemp = "Well Done";
                    lastSelectedSauce = "Lemon & Garlic";

                    lastQuantity = 1;

                    UpdateOrderView();
                }
            }

            CheckChanged();
        }

        async void ConfirmOrder(List<OrderItem> orderItems)
        {
            //Activity Indicator
            ActivityIndicator.AddSpinner(spinner);

            string x = await GetOrderNum(BranchSettings.Instance.branchId);

            string d = DateTime.Now.Day.ToString("00") + "/" + DateTime.Now.Month.ToString("00") + "/"
                    + DateTime.Now.Year.ToString("0000");

            for (int i = 0; i < orderItems.Count; i++)
            {
                var orderItem = orderItems[i];

                orderItem.OrderNumber = d + "_" + x;
                orderItem.OrderNumber = orderItem.OrderNumber.Replace('/', '-');

                orderItem.OrderDateTime = DateTime.UtcNow;

                if (optForSMS)
                    orderItem.PhoneNumber = phoneNumber.Text;

                if (!optForSMS)
                    orderItem.PhoneNumber = "";

                orderItem.Index = i;
                orderItem.ID = DateTime.UtcNow.Ticks.ToString();
                //await firebaseDataContext.StoreData("Order/" + BranchSettings.Instance.branchId + "/" + orderItem.OrderNumber + "/" + orderItem.Id, orderItem);
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

            ActivityIndicator.RemoveSpinner(spinner);

            if (_source.ToLower() == "walkin") 
            {
                WindowManager.Instance.CloseAndOpen(this, new ReceivePayment(orderItems, (POS)pos));
                return;
            }
            //Add Unpaid Order
            if (_source.ToLower() == "call")
            {
                CreateUnpaidOrder(orderItems, (POS)pos);
                return;
            }            
        }

        private void CreateUnpaidOrder(List<OrderItem> _order, POS _pOS)
        {
            List<OrderItem> orderItems = new List<OrderItem>();
            foreach (var item in _order)
            {
                orderItems.Add(new OrderItem()
                {
                    Index = item.Index,
                    Name = item.Name,
                    Category = item.Category,
                    Description = item.Description,
                    Reference = item.Reference,
                    Price = item.Price,
                    Weight = item.Weight,
                    Fufilled = item.Fufilled,
                    Purchased = false,
                    Preparable = item.Preparable,
                    WaitingForPayment = true,
                    Quantity = item.Quantity,
                    PhoneNumber = item.PhoneNumber,
                    OrderNumber = item.OrderNumber,
                    //Add changes to OrderItem model here as well
                    OrderDateTime = item.OrderDateTime,
                    Collected = item.Collected,
                    User = LocalStorage.Instance.user.FullName(),
                    PrepTime = item.PrepTime,
                    Flavour = lastSelectedFlavour,
                    MeatTemperature = lastSelectedMeatTemp,
                    Sauces = item.SubCategory != "Platter" ? new List<string>() { lastSelectedSauce } : item.Sauces,
                    SubCategory = item.SubCategory
                });
            }

            lastSelectedFlavour = "None";
            lastSelectedMeatTemp = "Well Done";
            lastSelectedSauce = "Lemon & Garlic";

            foreach (var item in orderItems)
            {
                item.WaitingForPayment = true;
                item.Purchased = false;
                item.Preparable = true;
                item.Reference = "Call";
            }

            _pOS.OnTransaction(_order[0].OrderNumber, orderItems);

            WindowManager.Instance.Close(this);
        }

        public async Task<string> GetOrderNum(string branchId)
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

            // NOTE: Offline Made Orders will always start with 0 to avoid them matching an order made online
            string candidateNumber = (new Random().Next(1, 1000)).ToString("0000");

            //Check Against Other Order Numbers For The Day
            List<string> orderNums = new List<string>();

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
                    orderNums.Add(number);
                }
            }

            while (orderNums.Contains(candidateNumber))
            {
                candidateNumber = (new Random().Next(1, 1000)).ToString("0000");
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

            total.Content = Formatting.FormatAmountString(totalAmt);
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

        // @Yewo: What does block do, not sure whats happening here
        int block1 = 0;
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if(block1 == 0)
            {
                ConfirmOrder(orders);
                block1 = 1;
            }            
        }
        private void UpdateOrderPrepTime(List<OrderItem> orderItems)
        {
            int orderTime = 0;
            foreach (var item in orderItems)
            {
                if (item.PrepTime > orderTime)
                    orderTime = item.PrepTime;
            }

            orderPrepTime.Content = $"Order will be ready in: {orderTime} minutes";
        }

        private void phoneNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckChanged();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            optForSMS = true;

            CheckChanged();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            optForSMS = false;

            CheckChanged();
        }

        private void CheckChanged()
        {
            if (confirmButton == null)
                return;

            if (!optForSMS)
            {
                phoneNumberEntry.Visibility = Visibility.Collapsed;

                if (orders.Count > 0)
                {
                    confirmButton.Visibility = Visibility.Visible;                    

                    return;
                }
            }

            if (optForSMS)
                phoneNumberEntry.Visibility = Visibility.Visible;

            if (!string.IsNullOrEmpty(phoneNumber.Text))
                if (phoneNumber.Text.Length == 8 && orders.Count > 0)
                {
                    confirmButton.Visibility = Visibility.Visible;

                    return;
                }

            confirmButton.Visibility = Visibility.Collapsed;
        }
    }
}
