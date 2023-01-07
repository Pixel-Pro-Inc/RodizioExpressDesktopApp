using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Core.Entities.Aggregates;
using RodizioSmartRestuarant.CustomBaseClasses.BaseClasses;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for ReceivePayment.xaml
    /// </summary>
    public partial class ReceivePayment : BaseWindow
    {
        private Order _order;
        public float total;

        string method;

        POS _pOS;
        public ReceivePayment(Order order, POS pOS)
        {
            _order = order;
            _pOS = pOS;

            InitializeComponent();

            List<Label> labels = GetLabels(order, "item");

            for (int i = 0; i < labels.Count; i++)
            {
                itemsPanel.Children.Add(labels[i]);
            }

            labels = GetLabels(order, "units");

            for (int i = 0; i < labels.Count; i++)
            {
                unitsPanel.Children.Add(labels[i]);
            }

            labels = GetLabels(order, "unitPrice");

            for (int i = 0; i < labels.Count; i++)
            {
                unitPricePanel.Children.Add(labels[i]);
            }

            labels = GetLabels(order, "price");

            for (int i = 0; i < labels.Count; i++)
            {
                pricePanel.Children.Add(labels[i]);
            }

            title.Content = "Order number - " + order[0].OrderNumber.Substring(order[0].OrderNumber.IndexOf('_') + 1, 4);

            totalPriceView.Text = "Total : " + Formatting.FormatAmountString(total);


            //Updates with size settings
            RodizioSmartRestuarant.Helpers.Settings.Instance.OnWindowCountChange();
        }
        //Creates Labels to be added to the xaml
        List<Label> GetLabels(Order order, string _type)
        {
            List<Label> labels = new List<Label>();

            if (_type.Equals("item"))
            {
                labels.Add(new Label() { Content = "Items", FontWeight = FontWeights.Bold });
                foreach (var item in order)
                {
                    labels.Add(new Label() { Content = item.Name });//.Substring(0, item.Name.IndexOf(" x") + 1) });
                }
                labels.Add(new Label() { Content = "Total", FontWeight = FontWeights.Bold });

                return labels;
            }
            if (_type.Equals("units"))
            {
                labels.Add(new Label() { Content = "Units", FontWeight = FontWeights.Bold });
                foreach (var item in order)
                {
                    labels.Add(new Label() { Content = item.Quantity });
                }

                return labels;
            }
            if (_type.Equals("unitPrice"))
            {
                labels.Add(new Label() { Content = "Unit Price", FontWeight = FontWeights.Bold });
                foreach (var item in order)
                {
                    labels.Add(new Label() { Content = Formatting.FormatAmountString(float.Parse(item.Price) / (float)item.Quantity) });
                }

                return labels;
            }


            float temp = 0;

            labels.Add(new Label() { Content = "Price", FontWeight = FontWeights.Bold });
            foreach (var item in order)
            {
                labels.Add(new Label() { Content = Formatting.FormatAmountString(float.Parse(item.Price)) });

                temp += float.Parse(item.Price);
            }

            SetTotal(temp);

            labels.Add(new Label() { Content = Formatting.FormatAmountString(temp), FontWeight = FontWeights.Bold });

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
            if (changeAmt.Content == null) return;

            float f = float.Parse(changeAmt.Content.ToString());
            if (f < 0) return;

            foreach (var item in _order)
            {
                item.User = LocalStorage.Instance.user.FullName();
            }

            for (int i = 0; i < _order.Count; i++)
            {
                _order[i].Purchased = true;
                _order[i].Preparable = true;
                if (method == "split")
                {
                    _order[i].OrderPayments = new Payments()
                    {
                        Cash = float.Parse(cashBox.Text),
                        Card = float.Parse(cardBox.Text)
                    };

                    continue;
                }

                if (method == "cash")
                    _order[i].OrderPayments = new Payments()
                    {
                        Cash = (float)_order.Price
                    };

                if (method == "card")
                    _order[i].OrderPayments = new Payments()
                    {
                        Card = (float)_order.Price
                    };
            }

            _pOS.OnTransaction(_order.OrderNumber, _order);

            PrintReceipt(_order, BranchSettings.Instance.branch);

            options.Visibility = Visibility.Collapsed;

            //Close();
        }
        //An option of whether the customer is using a card
        private void Card_Click(object sender, RoutedEventArgs e)
        {
            if(total < BranchSettings.Instance.branch.MinimumCardPaymentAmount)
            {
                ShowError("Card payments must be atleast " 
                    + BranchSettings.Instance.branch.Currency + " " 
                    + BranchSettings.Instance.branch.MinimumCardPaymentAmount);

                return;
            }

            amountBox.Text = Formatting.FormatAmountString(total);
            method = "card";
            amountPanel.Visibility = Visibility.Visible;
            splitPayView.Visibility = Visibility.Collapsed;

            amountBox.IsEnabled = false;
        }
        //An option of whether the customer is using a cash
        private void Cash_Click(object sender, RoutedEventArgs e)
        {
            amountBox.Text = "";
            method = "cash";
            amountPanel.Visibility = Visibility.Visible;
            splitPayView.Visibility = Visibility.Collapsed;

            amountBox.IsEnabled = true;
        }
        //An option of whether the customer is using a cash
        private void Split_Click(object sender, RoutedEventArgs e)
        {
            method = "split";
            amountPanel.Visibility = Visibility.Collapsed;
            splitPayView.Visibility = Visibility.Visible;

            amountBox.IsEnabled = false;
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
        private void splitBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            float tempCard = 0;
            float tempCash = 0;
            if (float.TryParse(cashBox.Text, out tempCash))
            {
                if (float.TryParse(cardBox.Text, out tempCard))
                {
                    float change = (tempCash + tempCard) - total;

                    changeAmt.Content = change.ToString("f2");
                }
            }
        }
        //This method prints a receipt
        void PrintReceipt(Order orderItem, Branch branch)
        {
            if (amountBox.Text != null && amountBox.Text != "")
            {
                float change = float.Parse(changeAmt.Content.ToString());
                float amount = float.Parse(amountBox.Text);

                new ReceiptSlip.PrintJob(orderItem, branch, amount, change).Print(BranchSettings.Instance.printerName);
            }
            else
            {
                if (method == "split")
                {
                    float change = float.Parse(changeAmt.Content.ToString());
                    float amount = float.Parse(cashBox.Text) + float.Parse(cardBox.Text);

                    new ReceiptSlip.PrintJob(orderItem, branch, amount, change).Print(BranchSettings.Instance.printerName);
                }
            }
        }

        //OverLoads for promotion desired. Update the xaml if we are including it and use these 
        // NOTE: logic for promotions and discounts
        public void ApplyDiscount(string promoCode) => total *= 1 + Waiver.GetpromoPercent(promoCode);
        public void ApplyDiscount(double amount) => total -= LocalStorage.Instance.user.FullName() == _order[0].User ? (float)amount : 0; //the zero needs to be replaced with error message
        public void RemoveDiscount(string promoCode) => total /= 1 + Waiver.GetpromoPercent(promoCode);
        public void RemoveDiscount(double amount, OrderItem order) => total += Int32.Parse(order.Price) * order.Quantity >= total + amount ? (float)amount : 0;

    }
}