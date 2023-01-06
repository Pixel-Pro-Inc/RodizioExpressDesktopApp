using RodizioSmartRestuarant.CustomBaseClasses.BaseClasses;
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
    /// Interaction logic for OrderSource.xaml
    /// </summary>
    public partial class OrderSource : BaseWindow
    {
        int block = 0;
        public OrderSource()
        {
            InitializeComponent();
        }
        //Walk in
        private void W_Button_Click(object sender, RoutedEventArgs e)
        {
            if (block != 0)
                return;

            block = 1;

            MessageBoxResult result = 
                MessageBox.Show("Would you like to deliver this order?", "Order Prompt",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                WindowManager.Instance.CloseAndOpen(this, new NewOrder("walkin", true));
            }
            else
            {
                WindowManager.Instance.CloseAndOpen(this, new NewOrder("walkin"));
            }
        }
        //Call
        private void C_Button_Click(object sender, RoutedEventArgs e)
        {
            if (block != 0)
                return;

            block = 1;

            MessageBoxResult result =
                MessageBox.Show("Would you like to deliver this order?", "Order Prompt",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                WindowManager.Instance.CloseAndOpen(this, new NewOrder("call", true));
            }
            else
            {
                WindowManager.Instance.CloseAndOpen(this, new NewOrder("call"));
            }
        }
    }
}
