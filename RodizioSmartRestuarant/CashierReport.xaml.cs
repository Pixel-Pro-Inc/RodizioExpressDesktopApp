using RdKitchenApp.Entities;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
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

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for CashierReport.xaml
    /// </summary>
    public partial class CashierReport : Window
    {
        public CashierReport()
        {
            InitializeComponent();

            GenerateReport();
        }
        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        async void GenerateReport()
        {
            //Name
            cashierName.Text = "Cashier: " + LocalStorage.Instance.user.FullName();

            //Get Orders

            List<Order> orderItems = new List<Order>();
            //Offline include completed orders
            orderItems = (List<Order>)(await FirebaseDataContext.Instance.GetOfflineOrdersCompletedInclusive());

            //Exclude Unpaid Orders
            List<Order> orders = orderItems.Where(o => !o[0].WaitingForPayment).ToList();

            //Cash Orders Summary
            List<Order> cashOrders = new List<Order>();
            cashOrders = GetRelevantOrders("cash", LocalStorage.Instance.user, orders);

            foreach (var order in cashOrders)
            {
                cashOrdersPanel.Children.Add(GetOrderSummaryPanel(order));
            }


            //Cash Orders Total
            float cashTotal = 0;
            foreach (var order in cashOrders)
            {
                for (int i = 0; i < order.Count; i++)
                {
                    cashTotal += float.Parse(order[i].Price);
                }                
            }

            cashOrdersTotal.Text = "Total: BWP " + Formatting.FormatAmountString(cashTotal);

            //Card Orders Summary
            List<Order> cardOrders = new List<Order>();
            cardOrders = GetRelevantOrders("card", LocalStorage.Instance.user, orders);

            foreach (var order in cardOrders)
            {
                cardOrdersPanel.Children.Add(GetOrderSummaryPanel(order));
            }


            //Card Orders Total
            float cardTotal = 0;
            foreach (var order in cardOrders)
            {
                for (int i = 0; i < order.Count; i++)
                {
                    cardTotal += float.Parse(order[i].Price);
                }
            }

            cardOrdersTotal.Text = "Total: BWP " + Formatting.FormatAmountString(cardTotal);
        }

        List<Order> GetRelevantOrders(string paymentMethod, AppUser user, List<Order> allPaidOrders)
        {
            List<Order> relevantOrders = new List<Order>();

            if(paymentMethod.ToLower().Trim() == "cash")
            {
                relevantOrders = allPaidOrders.Where(o => o[0].PaymentMethod.ToLower().Trim() == "cash").ToList();
            }

            if (paymentMethod.ToLower().Trim() == "card")
            {
                relevantOrders = allPaidOrders.Where(o => o[0].PaymentMethod.ToLower().Trim() == "card").ToList();
            }

            return relevantOrders.Where(o => o[0].User.ToLower() == user.FullName().ToLower()).ToList();
        }

        StackPanel GetOrderSummaryPanel(Order order)
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

            float totalPrice = 0;

            foreach (var item in order)
            {
                totalPrice += float.Parse(item.Price);
            }

            TextBlock textBlock_1 = new TextBlock()
            {
                FontSize = 15,
                Text = "BWP " + Formatting.FormatAmountString(totalPrice)
            };

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(textBlock_1);

            return stackPanel;
        }
    }
}
