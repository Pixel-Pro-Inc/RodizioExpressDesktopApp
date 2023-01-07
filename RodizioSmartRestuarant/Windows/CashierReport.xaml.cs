using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.CustomBaseClasses.BaseClasses;
using RodizioSmartRestuarant.Data;
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
using OrderItem = RodizioSmartRestuarant.Entities.OrderItem;
using RodizioSmartRestuarant.Core.Entities.Aggregates;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for CashierReport.xaml
    /// </summary>
    public partial class CashierReport : BaseWindow
    {
        public CashierReport()
        {
            InitializeComponent();

            GenerateReport();
        }

        float cardTotal = 0;
        float cashTotal = 0;
        async void GenerateReport()
        {
            //Name
            cashierName.Text = "Cashier: " + LocalStorage.Instance.user.FullName();

            //Get Orders

            List<Order> orderItems = new List<Order>();
            //Offline include completed orders
            orderItems = (List<Order>)(await FirebaseDataContext.Instance.GetOfflineOrdersCompletedInclusive());

            //Exclude Unpaid Orders
            List<Order> orders = orderItems.Where(o => o[0].Purchased).ToList();

            foreach (var order in orders)
            {
                if(order.First().OrderPayments.Card != 0)
                {
                    cardOrdersPanel.Children.Add(GetOrderSummaryPanel(order, order.First().OrderPayments.Card));
                    cardTotal += order.First().OrderPayments.Card;
                }

                if (order.First().OrderPayments.Cash != 0)
                {
                    cashOrdersPanel.Children.Add(GetOrderSummaryPanel(order, order.First().OrderPayments.Cash));
                    cashTotal += order.First().OrderPayments.Cash;
                }
            }

            cardOrdersTotal.Text = "Total: BWP " + Formatting.FormatAmountString(cardTotal);
            cashOrdersTotal.Text = "Total: BWP " + Formatting.FormatAmountString(cashTotal);
        }

        StackPanel GetOrderSummaryPanel(Order order, float amount)
        {
            StackPanel stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#EEEEEE"),
                Height = 30
            };

            TextBlock textBlock = new TextBlock()
            {
                Margin = new Thickness(0,0,50,0),
                FontSize = 15,
                Text = order[0].OrderNumber
            };

            TextBlock textBlock_1 = new TextBlock()
            {
                FontSize = 15,
                Text = "BWP " + Formatting.FormatAmountString(amount)
            };

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(textBlock_1);

            return stackPanel;
        }
    }
}
