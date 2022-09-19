using RodizioSmartRestuarant.Infrastructure.Helpers;
using System;
using System.Windows;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        public Settings()
        {
            InitializeComponent();
            float? sizeValue = Infrastructure.Helpers.Settings.Instance.properties.displaySize;

            displaySizeSlider.Value = sizeValue == 0 || sizeValue == null? displaySizeSlider.Value: (double)sizeValue;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Infrastructure.Helpers.Settings.Instance.ChangeDisplaySize((float)displaySizeSlider.Value);
            WindowManager.Instance.Close(this);
        }
    }
}
