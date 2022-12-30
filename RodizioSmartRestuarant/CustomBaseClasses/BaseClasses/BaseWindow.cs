using RodizioSmartRestuarant.Helpers;
using System;
using System.Windows;

namespace RodizioSmartRestuarant.CustomBaseClasses.BaseClasses
{
    /// <summary>
    /// Custom subclass for the windows base class
    /// Adds Custom Property
    /// </summary>
    public class BaseWindow : Window
    {
        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            WindowManager.Instance.UpdateList();
            IsClosed = true;
        }

        protected void ShowWarning(string msg)
        {
            string messageBoxText = msg;
            string caption = "Warning";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }
        protected void ShowError(string msg)
        {
            string messageBoxText = msg;
            string caption = "Error";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Error;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }

        protected void ShowSuccess(string msg)
        {
            string messageBoxText = msg;
            string caption = "Success";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.None;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }
    }
}