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
    }
}