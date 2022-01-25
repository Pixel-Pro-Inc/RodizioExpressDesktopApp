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
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using MenuItem = RodizioSmartRestuarant.Entities.MenuItem;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for NewOrderPO.xaml
    /// </summary>
    public partial class NewOrderPO : Window
    {
        public NewOrderPO()
        {
            InitializeComponent();
        }


        private FirebaseDataContext firebaseDataContext;
        List<OrderItem> order = new List<OrderItem>();

        private void DrinksButtonClicked(object sender, RoutedEventArgs e)
        {
            string cat = "Drinks";
            UpdateOrderView(cat);
        }
        private void SidesButton_Clickeddf(object sender, RoutedEventArgs e)
        {
            string cat = "Sides";
            UpdateOrderView(cat);
        }
        private void MeatButton_Clicked(object sender, RoutedEventArgs e)
        {
            string cat = "Meats";
            UpdateOrderView(cat);
        }
        private void DesertsButtonClicked(object sender, RoutedEventArgs e)
        {
            string cat = "Deserts";
            UpdateOrderView(cat);
        }
        private void CompleteOrderClicked(object sender, RoutedEventArgs e)
        {
            foreach (var item in order)
            {
                // var data = await firebaseDataContext.StoreData("Orders/", item, item);
                Console.Error.WriteLine(" Make an order");
            }

            Window current = this;
            Window next = new Login();

            current.Hide();

            next.Show();

            current.Close();
        }
        private void Additem_Click(object sender, RoutedEventArgs e)
        {
           // order= 
        }
        StackPanel GetPanel(List<MenuItem> items)
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

            //I am thinking a for each to iterate through all the items
            string x = items[0].Name; //this is supposed to be the menu item name
            x = x.Substring(x.IndexOf('_') + 1, 4);
            Label label = new Label()
            {
                FontWeight = FontWeights.DemiBold,
                Margin = new Thickness(0, 0, 50, 0),
                Content = x 
            };
            stackPanel1.Children.Add(label);

            bool isWeighted=false; // Needs to be a toggle
            int y = items[0].Quantity;
            if (isWeighted)
            {
                Label label1 = new Label()
                {
                    FontWeight = FontWeights.DemiBold,
                    Margin = new Thickness(0, 0, 50, 0),
                    Content = y  // This is supposed to be the amount in weight
                };
                stackPanel1.Children.Add(label1);
            }

            Button additemButton = new Button()
            {
                Background = new SolidColorBrush(Colors.OrangeRed),
                Width = 110,
                Foreground = new SolidColorBrush(Colors.White),
                Content = "Add Item",
                Name = items[0].Name
            };
            additemButton.Click += Additem_Click;
            stackPanel1.Children.Add(additemButton);

            string price= items[0].Price; //this is supposed to be the menu item amount
            x = x.Substring(x.IndexOf('_') + 1, 4);
            Label Itemamount = new Label()
            {
                FontWeight = FontWeights.DemiBold,
                Margin = new Thickness(0, 0, 50, 0),
                Content = price
            };
            stackPanel1.Children.Add(Itemamount);


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

        
        async void UpdateOrderView(string category)
        {
            orderViewer.Children.Clear();
            //
            var result = await firebaseDataContext.GetData("Menu/" + BranchSettings.Instance.branchId); // This is obviously wrong but something like this should happen
            foreach (List<MenuItem> item in result) //I am hoping that it downloads category lists, but i know its not that easy
            {
                foreach (var atrciile in item)
                {
                    if (atrciile.Category == category) orderViewer.Children.Add(GetPanel(item));
                }
                
            }
        }

        /*
          void AddOrder(string name,)
        {
            OrderItem desire = new OrderItem
            {
                Name = name,
                Price=
               
            };
        }
         */

    }
}
