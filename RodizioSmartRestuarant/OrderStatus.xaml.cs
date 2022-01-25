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
        }

        Label GetLabel(List<OrderItem> order)
        {
            Label label = new Label()
            {
                Margin = new Thickness(10), 
                FontWeight = FontWeights.DemiBold,
                Foreground = new SolidColorBrush(Colors.White), 
                FontSize = 25
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
