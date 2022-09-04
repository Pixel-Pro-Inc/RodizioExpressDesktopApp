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
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : BaseWindow
    {
        public Settings()
        {
            InitializeComponent();
            float? sizeValue = Helpers.Settings.Instance.properties.displaySize;

            displaySizeSlider.Value = sizeValue == 0 || sizeValue == null? displaySizeSlider.Value: (double)sizeValue;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Helpers.Settings.Instance.ChangeDisplaySize((float)displaySizeSlider.Value);
            WindowManager.Instance.Close(this);
        }
    }
}
