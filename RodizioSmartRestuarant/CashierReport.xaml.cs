using RdKitchenApp.Entities;
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

        float cardTotal = 0;
        float cashTotal = 0;
        async void GenerateReport()
        {
            //Name
            cashierName.Text = "Cashier: " + LocalStorage.Instance.user.FullName();

            //Get Orders

            List<List<OrderItem>> orderItems = new List<List<OrderItem>>();
            //Offline include completed orders
            orderItems = (List<List<OrderItem>>)(await FirebaseDataContext.Instance.GetOfflineOrdersCompletedInclusive());

            //Exclude Unpaid Orders
            List<List<OrderItem>> orders = orderItems.Where(o => !o[0].WaitingForPayment).ToList();

            //Cash Orders Summary
            List<List<OrderItem>> cashOrders = new List<List<OrderItem>>();
            cashOrders = GetRelevantOrders("cash", LocalStorage.Instance.user, orders);

            foreach (var order in cashOrders)
            {
                cashOrdersPanel.Children.Add(GetOrderSummaryPanel(order));
            }


            //Cash Orders Total            
            foreach (var order in cashOrders)
            {
                for (int i = 0; i < order.Count; i++)
                {
                    cashTotal += float.Parse(order[i].Price);
                }                
            }

            cashOrdersTotal.Text = "Total: BWP " + Formatting.FormatAmountString(cashTotal);

            //Card Orders Summary
            List<List<OrderItem>> cardOrders = new List<List<OrderItem>>();
            cardOrders = GetRelevantOrders("card", LocalStorage.Instance.user, orders);

            foreach (var order in cardOrders)
            {
                cardOrdersPanel.Children.Add(GetOrderSummaryPanel(order));
            }


            //Card Orders Total            
            foreach (var order in cardOrders)
            {
                for (int i = 0; i < order.Count; i++)
                {
                    cardTotal += float.Parse(order[i].Price);
                }
            }

            cardOrdersTotal.Text = "Total: BWP " + Formatting.FormatAmountString(cardTotal);

            //Split Orders Summary
            List<List<OrderItem>> splitOrders = new List<List<OrderItem>>();
            splitOrders = GetRelevantOrders("split", LocalStorage.Instance.user, orders);

            foreach (var order in splitOrders)
            {
                cardOrdersPanel.Children.Add(GetOrderSummaryPanel(order, "card"));
                cashOrdersPanel.Children.Add(GetOrderSummaryPanel(order, "cash"));

                cardTotal += float.Parse(order[0].payments[1]);
                cashTotal += float.Parse(order[0].payments[0]);
            }


            //Split Orders Total
            foreach (var order in cardOrders)
            {
                for (int i = 0; i < order.Count; i++)
                {
                    cardTotal += float.Parse(order[i].Price);
                }
            }

            cardOrdersTotal.Text = "Total: BWP " + Formatting.FormatAmountString(cardTotal);
            cashOrdersTotal.Text = "Total: BWP " + Formatting.FormatAmountString(cashTotal);
        }

        List<List<OrderItem>> GetRelevantOrders(string paymentMethod, AppUser user, List<List<OrderItem>> allPaidOrders)
        {
            List<List<OrderItem>> relevantOrders = allPaidOrders.Where(o => o[0].PaymentMethod.ToLower().Trim() == paymentMethod.ToLower().Trim()).ToList();

            return relevantOrders.Where(o => o[0].User.ToLower() == user.FullName().ToLower()).ToList();
        }

        StackPanel GetOrderSummaryPanel(List<OrderItem> order, string method = null)
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

            if (!order[0].SplitPayment)
            {
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
            }
            else
            {
                TextBlock textBlock_1 = new TextBlock()
                {
                    FontSize = 15,
                    Text = "BWP " + Formatting.FormatAmountString(method == "cash"? float.Parse(order[0].payments[0]) : float.Parse(order[0].payments[1]))
                };

                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(textBlock_1);
            }

            return stackPanel;
        }
    }
}
