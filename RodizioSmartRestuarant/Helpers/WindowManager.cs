using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static RodizioSmartRestuarant.Entities.Enums;

namespace RodizioSmartRestuarant.Helpers
{
    public class WindowManager
    {
        public static WindowManager Instance { get; set; }

        public List<Window> openWindows = new List<Window>();

        public WindowManager()
        {
            Instance = this;
        }

        public void CloseAllExcept(Window target)
        {
            for (int i = 0; i < openWindows.Count; i++)
            {
                if (openWindows[i] != target)
                    openWindows[i].Close();
            }

            UpdateList();
        }

        public void CloseAllAndOpen(Window target)
        {
            if (!WindowAlreadyOpen(target))
            {
                for (int i = 0; i < openWindows.Count; i++)
                {
                    openWindows[i].Close();
                }

                target.Show();

                AddToOpenWindows(target);

                return;
            }

            ShowWarning();
        }

        public void Close(Window target)
        {
            target.Close();

            UpdateList();
        }

        public void CloseAndOpen(Window close, Window target)
        {
            if (!WindowAlreadyOpen(target))
            {
                close.Close();

                target.Show();

                AddToOpenWindows(target);

                return;
            }

            ShowWarning();
        }

        public void Open(Window target)
        {
            if (!WindowAlreadyOpen(target))
            {
                target.Show();

                AddToOpenWindows(target);

                return;
            }

            ShowWarning();
                
        }

        bool WindowAlreadyOpen(Window target)
        {
            UpdateList();

            for (int i = 0; i < openWindows.Count; i++)
            {
                if (openWindows[i].GetType() == target.GetType())
                    return true;
            }

            return false;
        }

        void ShowWarning()
        {
            string messageBoxText = "The window is already open close it first to open a new one.";
            string caption = "Warning";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }

        void UpdateList()
        {
            int k = 0;

            while(k == 0)
            {
                for (int i = 0; i < openWindows.Count; i++)
                {
                    if (i < openWindows.Count)
                        if (openWindows[i].GetType() == typeof(LoadingScreen))
                        {
                            if (((LoadingScreen)openWindows[i]).IsClosed)
                            {
                                openWindows.RemoveAt(i);

                                i = 100000;
                            }
                        }

                    if (i < openWindows.Count)
                        if (openWindows[i].GetType() == typeof(Login))
                    {
                        if (((Login)openWindows[i]).IsClosed)
                        {
                            openWindows.RemoveAt(i);

                            i = 100000;
                        }
                    }

                    if (i < openWindows.Count)
                        if (openWindows[i].GetType() == typeof(MenuEditor))
                    {
                        if (((MenuEditor)openWindows[i]).IsClosed)
                        {
                            openWindows.RemoveAt(i);

                            i = 100000;
                        }
                    }

                    if (i < openWindows.Count)
                        if (openWindows[i].GetType() == typeof(NewOrder))
                    {
                        if (((NewOrder)openWindows[i]).IsClosed)
                        {
                            openWindows.RemoveAt(i);

                            i = 100000;
                        }
                    }

                    if (i < openWindows.Count)
                        if (openWindows[i].GetType() == typeof(OrderStatus))
                    {
                        if (((OrderStatus)openWindows[i]).IsClosed)
                        {
                            openWindows.RemoveAt(i);

                            i = 100000;
                        }
                    }

                    if (i < openWindows.Count)
                        if (openWindows[i].GetType() == typeof(POS))
                    {
                        if (((POS)openWindows[i]).IsClosed)
                        {
                            openWindows.RemoveAt(i);

                            i = 100000;
                        }
                    }

                    if (i < openWindows.Count)
                        if (openWindows[i].GetType() == typeof(ReceivePayment))
                    {
                        if (((ReceivePayment)openWindows[i]).IsClosed)
                        {
                            openWindows.RemoveAt(i);

                            i = 100000;
                        }
                    }

                }

                for (int i = 0; i < openWindows.Count; i++)
                {
                    k = 1;

                    if (openWindows[i].GetType() == typeof(LoadingScreen))
                    {
                        if (((LoadingScreen)openWindows[i]).IsClosed)
                        {
                            k = 0;
                        }
                    }

                    if (openWindows[i].GetType() == typeof(Login))
                    {
                        if (((Login)openWindows[i]).IsClosed)
                        {
                            k = 0;
                        }
                    }

                    if (openWindows[i].GetType() == typeof(MenuEditor))
                    {
                        if (((MenuEditor)openWindows[i]).IsClosed)
                        {
                            k = 0;
                        }
                    }

                    if (openWindows[i].GetType() == typeof(NewOrder))
                    {
                        if (((NewOrder)openWindows[i]).IsClosed)
                        {
                            k = 0;
                        }
                    }

                    if (openWindows[i].GetType() == typeof(OrderStatus))
                    {
                        if (((OrderStatus)openWindows[i]).IsClosed)
                        {
                            k = 0;
                        }
                    }

                    if (openWindows[i].GetType() == typeof(POS))
                    {
                        if (((POS)openWindows[i]).IsClosed)
                        {
                            k = 0;
                        }
                    }

                    if (openWindows[i].GetType() == typeof(ReceivePayment))
                    {
                        if (((ReceivePayment)openWindows[i]).IsClosed)
                        {
                            k = 0;
                        }
                    }
                }
                if (openWindows.Count == 0)
                    k = 1;
            }            
        }
        public async void UpdateAllOrderViews(UIChangeSource source)
        {
            var result = await FirebaseDataContext.Instance.GetData("Order/" + BranchSettings.Instance.branchId);

            List<List<OrderItem>> temp = new List<List<OrderItem>>();

            foreach (var item in result)
            {
                List<OrderItem> data = JsonConvert.DeserializeObject<List<OrderItem>>(((JArray)item).ToString());

                temp.Add(data);
            }

            for (int i = 0; i < openWindows.Count; i++)
            {
                if (openWindows[i].GetType() == typeof(POS))
                {
                    ((POS)openWindows[i]).UpdateOrderView(temp, source);
                }

                if (openWindows[i].GetType() == typeof(OrderStatus))
                {
                    ((OrderStatus)openWindows[i]).UpdateScreen(temp);
                }
            }
        }

        void AddToOpenWindows(Window target)
        {
            openWindows.Add(target);

            UpdateList();
        }
    }
}
