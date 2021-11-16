using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Data;
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

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for POS.xaml
    /// </summary>
    public partial class POS : Window
    {
        List<List<OrderItem>> orders = new List<List<OrderItem>>();
        private FirebaseDataContext firebaseDataContext;
        public POS()
        {
            InitializeComponent();
            UpdateOrderView();            
        }

        async void UpdateOrderView()
        {
            var result = await firebaseDataContext.GetData("Order/" + BranchSettings.Instance.branchId);

            List<List<OrderItem>> temp = new List<List<OrderItem>>();

            foreach (var item in result)
            {
                temp.Add((List<OrderItem>)item);
            }

            foreach (var item in temp)
            {
                if (!orders.Contains(item))
                {
                    orders.Add(item);

                    orderViewer.Children.Add(GetPanel(item));
                }
            }
        }

        StackPanel GetPanel(List<OrderItem> items)
        {
            StackPanel stackPanel = new StackPanel() 
            {
                Margin = new Thickness(0, 5, 0, 0),
                Background = new SolidColorBrush(Colors.White)
            };

            StackPanel stackPanel1 = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            string x = items[0].OrderNumber;
            x = x.Substring(x.IndexOf('_') + 1, 4);
            Label label = new Label()
            {
                FontWeight = FontWeights.DemiBold,
                Margin = new Thickness(0, 0, 50, 0),
                Content = "Order Number - " + x
            };

            Label label1 = new Label()
            {
                FontWeight = FontWeights.DemiBold,
                Margin = new Thickness(0, 0, 50, 0),
                Content = items[0].PhoneNumber
            };

            stackPanel1.Children.Add(label);
            stackPanel1.Children.Add(label1);

            if (!items[0].Purchased)
            {
                Button button = new Button()
                {
                    Background = new SolidColorBrush(Colors.OrangeRed),
                    Width = 110,
                    Foreground = new SolidColorBrush(Colors.White),
                    Content = "Confirm Payment",
                    Name = items[0].OrderNumber
                };

                button.Click += Payment_Click;

                stackPanel1.Children.Add(button);
            }

            if (items[0].Purchased)
            {
                Button button = new Button()
                {
                    Background = new SolidColorBrush(Colors.OrangeRed),
                    Width = 110,
                    Foreground = new SolidColorBrush(Colors.White),
                    Content = "Confirm Collection",
                    Name = items[0].OrderNumber
                };

                button.Click += Collection_Click;

                stackPanel1.Children.Add(button);
            }

            Button button1 = new Button()
            {
                Background = new SolidColorBrush(Colors.OrangeRed),
                Width = 60,
                Foreground = new SolidColorBrush(Colors.White),
                Content = "View",
                Name = items[0].OrderNumber
            };

            button1.Click += View_Click;

            stackPanel1.Children.Add(button1);

            stackPanel.Children.Add(stackPanel1);

            StackPanel stackPanel2 = new StackPanel() 
            { 
                Visibility = Visibility.Collapsed            
            };

            stackPanel.Children.Add(stackPanel2);

            StackPanel stackPanel3 = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            stackPanel2.Children.Add(stackPanel3);

            StackPanel stackPanel4 = new StackPanel();

            for (int i = 0; i < items.Count; i++)
            {
                if (i == 0)
                {
                    Label label2 = new Label()
                    {
                        FontWeight = FontWeights.Bold,
                        Content = "Item"
                    };

                    stackPanel4.Children.Add(label2);
                }

                Label label3 = new Label()
                {
                    Content = items[i].Name
                };

                stackPanel4.Children.Add(label3);

                if (i == items.Count - 1)
                {
                    Label label2 = new Label()
                    {
                        FontWeight = FontWeights.Bold,
                        Content = "Total"
                    };

                    stackPanel4.Children.Add(label2);
                }
            }

            StackPanel stackPanel5 = new StackPanel();

            for (int i = 0; i < items.Count; i++)
            {
                if (i == 0)
                {
                    Label label2 = new Label()
                    {
                        FontWeight = FontWeights.Bold,
                        Content = "Price"
                    };

                    stackPanel5.Children.Add(label2);
                }

                Label label3 = new Label()
                {
                    Content = items[i].Price
                };

                stackPanel5.Children.Add(label3);

                if (i == items.Count - 1)
                {
                    float total = 0;

                    foreach (var item in items)
                    {
                        total += float.Parse(item.Price);
                    }

                    Label label2 = new Label()
                    {
                        FontWeight = FontWeights.Bold,
                        Content = total
                    };

                    stackPanel5.Children.Add(label2);
                }
            }

            stackPanel3.Children.Add(stackPanel4);
            stackPanel3.Children.Add(stackPanel5);

            return stackPanel;
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            StackPanel mainPanel = (StackPanel)(((StackPanel)button.Parent).Parent);

            StackPanel panel = (StackPanel)mainPanel.Children[1];

            if(panel.Visibility == Visibility.Collapsed)
            {
                panel.Visibility = Visibility.Visible;
                button.Content = "Hide";
            }
            else
            {
                panel.Visibility = Visibility.Collapsed;
                button.Content = "View";
            }
        }

        private void Collection_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
            //Receipt Maybe
        }

        private void Payment_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
            //Receipt Maybe
        }
    }
}
