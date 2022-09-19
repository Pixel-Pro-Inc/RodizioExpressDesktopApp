using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace RodizioSmartRestuarant.Infrastructure.Helpers
{
    public class Notification
    {
        public Notification(string title, string message)
        {
            var _notifyIcon = new NotifyIcon();
            // Extracts your app's icon and uses it as notify icon
            _notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            // Hides the icon when the notification is closed
            _notifyIcon.BalloonTipClosed += (s, e) => _notifyIcon.Visible = false;

            _notifyIcon.Visible = true;
            // Shows a notification with specified message and title
            _notifyIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
        }
    }
}
