using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace RodizioSmartRestuarant.Helpers
{
    public static class OnScreenKeyboard
    {
        public static void AttachEventHandler()
        {
            List<Window> windows = WindowManager.Instance.GetAllOpenWindows();
            foreach (var window in windows)
            {
                //Adjust Text Boxes Sizes
                foreach (var tb in FindVisualChildren<TextBox>(window))
                {
                    tb.IsMouseCapturedChanged += Element_IsKeyboardFocusedChanged;
                }
                //Adjust Watermark Text Boxes
                foreach (var wtb in FindVisualChildren<Xceed.Wpf.Toolkit.WatermarkTextBox>(window))
                {
                    wtb.IsMouseCapturedChanged += Element_IsKeyboardFocusedChanged;
                }
                //Adjust Password Boxes
                foreach (var pb in FindVisualChildren<PasswordBox>(window))
                {
                    pb.IsMouseCapturedChanged += Element_IsKeyboardFocusedChanged;
                }
            }
        }

        private static void Element_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CheckKeyboard();
        }

        public static void CheckKeyboard()
        {
            List<string> keyboards = new List<string>();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select Name from Win32_Keyboard");

            foreach (ManagementObject keyboard in searcher.Get())
            {
                if (!keyboard.GetPropertyValue("Name").Equals(""))
                {
                    keyboards.Add(keyboard.GetPropertyValue("Name").ToString());
                }
            }

            if (keyboards.Count != 0)
                return;

            App app = App.Instance;

            app.ShowKeyboard();                        
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