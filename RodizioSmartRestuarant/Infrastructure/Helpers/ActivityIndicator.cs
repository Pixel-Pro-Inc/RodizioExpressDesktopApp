using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace RodizioSmartRestuarant.Infrastructure.Helpers
{
    public static class ActivityIndicator
    {
        public static void StartTimer()
        {
            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            dispatcherTimer.Start();
        }

        public static void AddSpinner(Grid background)
        {
            if (!indicators.Contains((Image)background.Children[1]))
            {
                background.Visibility = System.Windows.Visibility.Visible;

                indicators.Add((Image)background.Children[1]);
            }                
        }

        public static void RemoveSpinner(Grid background)
        {
            background.Visibility = System.Windows.Visibility.Collapsed;

            if (indicators.Contains((Image)background.Children[1]))
                indicators.Remove((Image)background.Children[1]);
        }

        static int numOfSpins = 1;

        static List<Image> indicators = new List<Image>();

        // REFACTOR: This amougst others are have the same name and similar purpose, we have to consider having overloads of a single method (that takes advantage of 'base' syntax)
        private static void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            int spinAngle = numOfSpins * 5;

            numOfSpins++;

            if (numOfSpins == 72)
                numOfSpins = 1;

            RotateTransform rotateTransform = new RotateTransform(spinAngle);

            foreach (var indicator in indicators)
            {
                if (indicator != null)
                    indicator.RenderTransform = rotateTransform;
            }            
        }
    }
}
