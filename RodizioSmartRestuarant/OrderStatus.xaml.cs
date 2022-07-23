using RodizioSmartRestuarant.Entities;
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
using System.Speech.Synthesis;
using static RodizioSmartRestuarant.Entities.Enums;
using RodizioSmartRestuarant.Configuration;
using MenuItem = RodizioSmartRestuarant.Entities.MenuItem;
using System.Windows.Media.Effects;
using RodizioSmartRestuarant.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Threading;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for OrderStatus.xaml
    /// </summary>
    public partial class OrderStatus : Window
    {
        public OrderStatus()
        {

        }
        public OrderStatus(List<List<OrderItem>> orders)
        {
            InitializeComponent();

            UpdateScreen(orders);

            //Adds phonenumbers to the phonenumber display of the panel

            if (BranchSettings.Instance == null)
                return;

            var numbers = BranchSettings.Instance.branch.PhoneNumbers;

            for (int i = 0; i < numbers.Count; i++)
            {
                var num = numbers[i];

                phoneNumberText.Text += i == 0 ? " " + num : "/" + num;
            }

            PopulateMenuScrollers();

            //Timer Setup
            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (countShowTime)
                menuItemsShownTimer++;

            if (menuItemsShownTimer >= menuItemsShownThreshold)
                RenderMenuItems();
        }

        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        // This is called also in windowManager so that you don't have to update this window with an event defined here
        public void UpdateScreen(List<List<OrderItem>> orders)
        {
            Dispatcher.BeginInvoke(new Action(() => Logic(orders)));
        }

        public async void CallOutOrders(List<List<OrderItem>> orders)
        {
            //To avoid collection modified exception
            try
            {
                foreach (var order in orders)
                {
                    // @Yewo: What does this do, wait, kana you won't understand the question. What does this aim to do?
                    // @Abel: Since orders are fufilled one orderItem at a time on the tablets there are going to be situations
                    //where the complete order haas not been fufilled by parts of it has
                    //this line makes it so that we skip over orders which have not been fully fufilled

                    if (order.Where(o => o.Fufilled).Count() != order.Count)
                        continue;

                    //We retrieve a serialized list of orders we had called out earlier
                    var calledOutOrders = getCalledOutOrders();

                    //If we had called out the current order earlier we skip over the order
                    if (calledOutOrders.Contains(order[0].OrderNumber))
                        continue;

                    //If we had not called out the current order earlier we add it to the list of called out orders
                    calledOutOrders.Add(order[0].OrderNumber);

                    saveCalledOutOrders(calledOutOrders);

                    //Call Order
                    SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
                    speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);

                    string oNumber = order[0].OrderNumber;
                    oNumber = oNumber.Substring(oNumber.Length - 4, 4);//e.g 2345

                    string oNumberCallOut = "";

                    for (int i = 0; i < oNumber.Length; i++)//Return order number with spaces e.g 2 3 4 5
                    {
                        oNumberCallOut += oNumber[i] + " ";
                    }

                    speechSynthesizer.Speak("Order Number " + oNumberCallOut.Trim());
                    await Task.Delay(5000);
                }
            }
            catch
            {
                ;
            }

            await Task.Delay(5000);
            statusGrid.Visibility = Visibility.Collapsed;
            menuGrid.Visibility = Visibility.Visible;
        }

        List<string> getCalledOutOrders()
        {
            List<object> data = (List<object>)(new SerializedObjectManager().RetrieveData(Directories.CalledOutOrders));
            List<string> calledOutOrders = data == null ? new List<string>() : (List<string>)data[0];

            return calledOutOrders;
        }

        void saveCalledOutOrders(List<string> orderNumbers) => new SerializedObjectManager().SaveOverwriteData(orderNumbers, Directories.CalledOutOrders);

        public void Logic(List<List<OrderItem>> orders)
        {
            ready.Children.Clear();
            inProgress.Children.Clear();

            statusGrid.Visibility = Visibility.Visible;
            menuGrid.Visibility = Visibility.Collapsed;

            for (int i = 0; i < orders.Count; i++)
            {
                if (!orders[i][0].Collected)
                {
                    bool skip = false;
                    foreach (var item in orders[i])
                    {
                        if (!item.Fufilled)
                        {
                            skip = true;
                        }
                    }

                    if (!skip)
                    {
                        ready.Children.Add(GetLabel(orders[i]));
                    }

                    if (skip)
                    {
                        inProgress.Children.Add(GetLabel(orders[i]));
                    }
                }
            }

            // TODO: Get this working so that we can have the fancy shit going on again
            CallOutOrders(orders);
        }

        Label GetLabel(List<OrderItem> order)
        {
            Label label = new Label()
            {
                Margin = new Thickness(10),
                FontWeight = FontWeights.DemiBold,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 50
            };

            string number = order[0].OrderNumber.Substring(order[0].OrderNumber.IndexOf('_') + 1, 4);

            label.Content = "#" + number;

            return label;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            maxButton.Visibility = Visibility.Collapsed;

            WindowStyle = WindowStyle.None;

            WindowState = WindowState.Maximized;
        }

        #region MenuScrollerRegion
        private FirebaseDataContext firebaseDataContext = FirebaseDataContext.Instance;

        List<MenuItem> menuCollection_1;
        List<MenuItem> menuCollection_2;
        async void PopulateMenuScrollers()
        {
            //Get menu items
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

            //Split Menu Items Into Separate Collections

            menuCollection_1 = items.Where(item => item.SubCategory.ToLower().Trim() == "chicken" ||
            item.SubCategory.ToLower().Trim() == "steak" || item.SubCategory.ToLower().Trim() == "lamb").ToList();

            menuCollection_2 = items.Where(item => item.SubCategory.ToLower().Trim() == "platter" ||
            item.SubCategory.ToLower().Trim() == "beef" || item.SubCategory.ToLower().Trim() == "pork").ToList();

            RenderMenuItems();
        }

        int startIndex_1 = 0;
        int startIndex_2 = 0;
        int menuItemsShownTimer = 0, menuItemsShownThreshold = 20;
        bool countShowTime;
        void RenderMenuItems()
        {
            countShowTime = false;
            menuItemsShownTimer = 0;

            menuScroller_1.Children.Clear();
            for (int i = startIndex_1; i < menuCollection_1.Count; i++)
            {
                var menuItem = menuCollection_1[i];

                if (menuCollection_1.Count - startIndex_1 < 7)
                {
                    //Less than 7 items left in the collection
                    int diff = 7 - (menuCollection_1.Count - startIndex_1);

                    if (startIndex_1 - diff >= 0)
                    {
                        startIndex_1 = startIndex_1 - diff;
                        i = startIndex_1;
                    }
                }

                menuScroller_1.Children.Add(menuScrollItem(menuItem));

                if (menuScroller_1.Children.Count == 7 || i == menuCollection_1.Count - 1)
                {
                    if (i != menuCollection_1.Count - 1)
                    {
                        startIndex_1 = i;
                        break;
                    }

                    startIndex_1 = 0;
                    break;
                }
            }

            menuScroller_2.Children.Clear();
            for (int i = startIndex_2; i < menuCollection_2.Count; i++)
            {
                var menuItem = menuCollection_2[i];

                if (menuCollection_2.Count - startIndex_2 < 7)
                {
                    //Less than 7 items left in the collection
                    int diff = 7 - (menuCollection_2.Count - startIndex_2);
                    if (startIndex_2 - diff >= 0)
                    {
                        startIndex_2 = startIndex_2 - diff;
                        i = startIndex_2;
                    }
                }

                menuScroller_2.Children.Add(menuScrollItem(menuItem));

                if (menuScroller_2.Children.Count == 7 || i == menuCollection_2.Count - 1)
                {
                    if (i != menuCollection_2.Count - 1)
                    {
                        startIndex_2 = i;
                        break;
                    }

                    startIndex_2 = 0;
                    break;
                }
            }

            countShowTime = true;
        }
        StackPanel menuScrollItem(MenuItem menuItem)
        {
            StackPanel stackPanel = new StackPanel()
            {
                Margin = new Thickness(20, 40, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Orientation = Orientation.Horizontal,
                Height = 400
            };

            StackPanel stackPanel1 = new StackPanel()
            {
                Width = 210
            };

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(menuItem.ImgUrl, UriKind.Absolute);
            bitmap.EndInit();


            Image image = new Image()
            {
                Source = bitmap,
                Width = 175
            };

            TextBlock textBlock = new TextBlock()
            {
                TextWrapping = TextWrapping.WrapWithOverflow,
                FontSize = 30,
                Margin = new Thickness(0, 10, 0, 0),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Red),
                Text = menuItem.Name
            };

            stackPanel1.Children.Add(image);
            stackPanel1.Children.Add(textBlock);

            if (!string.IsNullOrEmpty(menuItem.Description))
            {
                TextBlock textBlock_1 = new TextBlock()
                {
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    FontSize = 15,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(5, 20, 0, 0),
                    Foreground = new SolidColorBrush(Colors.White),
                    Text = menuItem.Description
                };

                stackPanel1.Children.Add(textBlock_1);
            }

            TextBlock textBlock_2 = new TextBlock()
            {
                TextWrapping = TextWrapping.WrapWithOverflow,
                FontSize = 25,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5, 0, 0, 0),
                Foreground = new SolidColorBrush(Colors.White),
                Text = menuItem.Price
            };

            stackPanel1.Children.Add(textBlock_2);

            Border border = new Border()
            {
                Width = 2,
                Margin = new Thickness(20, 0, 0, 0),
                Background = new SolidColorBrush(Colors.Black),
                Opacity = 0.3
            };

            border.Effect = new DropShadowEffect();

            stackPanel.Children.Add(stackPanel1);
            stackPanel.Children.Add(border);

            return stackPanel;
        }
        #endregion
    }
}