using RodizioSmartRestuarant.Configuration;
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

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for ReceivePayment.xaml
    /// </summary>
    public partial class ReceivePayment : Window
    {
        private List<OrderItem> _order;
        public float total;

        string method;

        POS _pOS;
        public ReceivePayment(List<OrderItem> order, POS pOS)
        {
            _order = order;
            _pOS = pOS;

            InitializeComponent();

            List<Label> labels = GetLabels(order, "item");

            for (int i = 0; i < labels.Count; i++)
            {
                itemsPanel.Children.Add(labels[i]);
            }

            labels = GetLabels(order, "price");

            for (int i = 0; i < labels.Count; i++)
            {
                pricePanel.Children.Add(labels[i]);
            }

            title.Content = "Order number - " + order[0].OrderNumber.Substring(order[0].OrderNumber.IndexOf('_') + 1, 4);

        }
        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }
        //Creates Labels to be added to the xaml
        List<Label> GetLabels(List<OrderItem> order, string _type)
        {
            List<Label> labels = new List<Label>();

            if (_type.Equals("item"))
            {
                labels.Add(new Label() { Content = "Items", FontWeight = FontWeights.Bold });
                foreach (var item in order)
                {
                    labels.Add(new Label() { Content = item.Name });
                }
                labels.Add(new Label() { Content = "Total", FontWeight = FontWeights.Bold });

                return labels;
            }

            float temp = 0;

            labels.Add(new Label() { Content = "Price", FontWeight = FontWeights.Bold });
            foreach (var item in order)
            {
                labels.Add(new Label() { Content = item.Price });

                temp += float.Parse(item.Price);
            }

            SetTotal(temp);

            labels.Add(new Label() { Content = temp.ToString("f2"), FontWeight = FontWeights.Bold });

            return labels;
        }
        //Sets the total variable
        void SetTotal(float t)
        {
            total = t;
        }
        //Completes a transaction
        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            if(changeAmt.Content != null)
            {
                float f = float.Parse(changeAmt.Content.ToString());
                if (f >= 0)
                {
                    foreach (var item in _order)
                    {
                        item.WaitingForPayment = false;
                        item.Purchased = true;
                        item.Preparable = true;
                        item.PaymentMethod = method;
                    }

                    _pOS.OnTransaction(_order[0].OrderNumber, _order);

                    PrintReceipt(_order, BranchSettings.Instance.branch);

                    Close();
                }
            }            
        }
        //An option of whether the customer is using a card
        private void Card_Click(object sender, RoutedEventArgs e)
        {
            amountBox.Text = total.ToString("f2");
            method = "card";
        }
        //An option of whether the customer is using a cash
        private void Cash_Click(object sender, RoutedEventArgs e)
        {
            amountBox.Text = "";
            method = "cash";
        }
        //This event handler calculates and displays the change amount in real time
        private void amountBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            float temp = 0;
            if (float.TryParse(amountBox.Text, out temp))
            {
                float change = temp - total;

                changeAmt.Content = change.ToString("f2");
            }
        }
        //This method prints a receipt
        void PrintReceipt(List<OrderItem> orderItem, Branch branch)
        {
            if(amountBox.Text != null && amountBox.Text != "")
            {
                float change = float.Parse(changeAmt.Content.ToString());
                float amount = float.Parse(amountBox.Text);

                new ReceiptSlip.PrintJob(orderItem, branch, amount, change).Print(BranchSettings.Instance.printerName);
            }            
        }

        //OverLoads for promotion desired. Update the xaml if we are including it and use these 
        public void ApplyDiscount(string promoCode) => total *= 1 + Waiver.GetpromoPercent(promoCode);
        public void ApplyDiscount(double amount) => total -= LocalStorage.Instance.user == _order[0].employee ? (float)amount : 0; //the zero needs to be replaced with error message
        public void RemoveDiscount(string promoCode) => total /= 1 + Waiver.GetpromoPercent(promoCode);
        public void RemoveDiscount(double amount, OrderItem order) => total += Int32.Parse(order.Price) * order.Quantity >= total + amount ? (float)amount : 0;

    }
}
