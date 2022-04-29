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

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for OrderStatus.xaml
    /// </summary>
    public partial class OrderStatus : Window
    {
        public OrderStatus(List<List<OrderItem>> orders)
        {
            InitializeComponent();

            UpdateScreen(orders);
        }

        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        public void UpdateScreen(List<List<OrderItem>> orders)
        {
            Dispatcher.BeginInvoke(new Action(() => Logic(orders)));
        }

        public async void CallOutOrders(List<List<OrderItem>> orders)
        {
            foreach (var order in orders)
            {
                if (order.Where(o => o.Fufilled).Count() != order.Count)
                    continue;

                if (LocalStorage.Instance.ordersCalledOut.Contains(order[0].OrderNumber))
                    continue;

                LocalStorage.Instance.ordersCalledOut.Add(order[0].OrderNumber);

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

        public void Logic(List<List<OrderItem>> orders)
        {
            ready.Children.Clear();
            inProgress.Children.Clear();

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
    }
}
