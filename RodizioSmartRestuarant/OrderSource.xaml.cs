using RodizioSmartRestuarant.Infrastructure.Helpers;
using System;
using System.Windows;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for OrderSource.xaml
    /// </summary>
    public partial class OrderSource : Window
    {
        int block = 0;
        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }
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

            WindowManager.Instance.CloseAndOpen(this, new NewOrder("walkin"));
        }
        //Call
        private void C_Button_Click(object sender, RoutedEventArgs e)
        {
            if (block != 0)
                return;

            block = 1;

            WindowManager.Instance.CloseAndOpen(this, new NewOrder("call"));
        }
        //Delivery
        private void D_Button_Click(object sender, RoutedEventArgs e)
        {
            if (block != 0)
                return;

            block = 1;

            WindowManager.Instance.CloseAndOpen(this, new NewOrder("delivery"));
        }
    }
}
