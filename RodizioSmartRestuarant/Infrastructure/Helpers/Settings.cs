using RodizioSmartRestuarant.Application.Extensions;
using RodizioSmartRestuarant.Core.Entities;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static RodizioSmartRestuarant.Core.Entities.Enums;

namespace RodizioSmartRestuarant.Infrastructure.Helpers
{
    public class Settings
    {
        public static Settings Instance { get; set; }

        public SettingsProperties properties { get; set; }

        public Settings()
        {
            Instance = this;

            SettingsProperties settings = new SerializedObjectManager().RetrieveData(Directories.Settings) != null? ((IDictionary<string, object>)((List<object>)new SerializedObjectManager().RetrieveData(Directories.Settings))[0]).ToObject<SettingsProperties>(): new SettingsProperties();

            Instance.properties = settings;
        }

        public void OnWindowCountChange() => WindowScaleChange();

        private void WindowScaleChange() //either opened new or closed
        {
            float displaySize = properties.displaySize;

            List<Window> windows = WindowManager.Instance.GetAllOpenWindows();
            foreach (var window in windows)
            {
                if (!(window is POS || window is MenuEditor || window is NewOrder))
                    continue;

                //Adjust Label Sizes
                foreach (var lb in FindVisualChildren<Label>(window))
                {
                    if (lb.TryFindResource("Font") == null)
                        lb.Resources.Add("Font", lb.FontSize);

                    if (lb.TryFindResource("Width") == null)
                        lb.Resources.Add("Width", lb.Width);

                    lb.FontSize = (double)lb.FindResource("Font") * (double)displaySize;
                    lb.Width = (double)lb.FindResource("Width") * (double)displaySize;
                }
                //Adjust Button Sizes
                foreach (var butt in FindVisualChildren<Button>(window))
                {
                    if (butt.Style != new Button().Style)
                        continue;

                    if (butt.TryFindResource("Width") == null)
                        butt.Resources.Add("Width", butt.Width);

                    butt.Width = (double)butt.FindResource("Width") * (double)displaySize;

                    if (butt.TryFindResource("Font") == null)
                        butt.Resources.Add("Font", butt.FontSize);

                    butt.FontSize = (double)butt.FindResource("Font") * (double)displaySize;
                }
            }

            //Save new Settings
            IDictionary<string, object> keyValuePairs = properties.AsDictionary();

            new SerializedObjectManager().SaveOverwriteData(keyValuePairs, Directories.Settings);
        }

        public void ChangeDisplaySize(float newSize)
        {
            properties.displaySize = newSize;

            WindowScaleChange();
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
                yield return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                if (child != null && child is T)
                    yield return (T)child;

                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }
    }    
}
