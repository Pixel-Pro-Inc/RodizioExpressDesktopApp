using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.CustomBaseClasses.BaseClasses;
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
        // @Yewo: What made you think to do this, makking a window manager. Cause it feels, kinda unlike you *laughing emoji*
        public static WindowManager Instance { get; set; }

        public List<BaseWindow> openWindows = new List<BaseWindow>();

        public WindowManager()
        {
            Instance = this;
        }

        public void CloseAllExcept(BaseWindow target)
        {
            // REFACTOR: We need to extract this code cause it is used alot in this cs file
            for (int i = 0; i < openWindows.Count; i++)
            {
                if (openWindows[i] != target)
                    openWindows[i].Close();
            }

            UpdateList();
        }

        public void CloseAllAndOpen(BaseWindow target)
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

        public void Close(BaseWindow target)
        {
            target.Close();

            UpdateList();
        }

        public void CloseAndOpen(BaseWindow close, BaseWindow target)
        {
            if (!WindowAlreadyOpen(target))
            {
                close.Close();

                target.Show();

                AddToOpenWindows(target);

                return;
            }

            if (target is ReceivePayment)
            {
                for (int i = 0; i < openWindows.Count; i++)
                {
                    if (openWindows[i].GetType() == target.GetType())
                        Close(openWindows[i]);
                }

                close.Close();

                target.Show();

                AddToOpenWindows(target);

                return;
            }

            ShowWarning();
        }

        public void Open(BaseWindow target)
        {
            if (!WindowAlreadyOpen(target))
            {
                target.Show();

                AddToOpenWindows(target);

                return;
            }

            if(target is ReceivePayment)
            {
                for (int i = 0; i < openWindows.Count; i++)
                {
                    if (openWindows[i].GetType() == target.GetType())
                        Close(openWindows[i]);
                }

                target.Show();

                AddToOpenWindows(target);

                return;
            }

            ShowWarning();
                
        }

        bool WindowAlreadyOpen(BaseWindow target)
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

        // REFACTOR: There is prolly some syntax we can find out that can do a better job than a million if statements.
        // We can prolly extract the method and have it use generic types and set the generic parameter to be within 'Window' or something 
        // more specific we want so we don't have to do this every time a new window is generated and also, well we don't have to maintain a million
        // if statements
        public void UpdateList()
        {
            while(true)
            {
                //Check closed windows that are still in the list
                for (int i = 0; i < openWindows.Count; i++)
                {
                    if (!openWindows[i].IsClosed)
                        continue;

                    //Remove closed windows 
                    openWindows.RemoveAt(i);
                    break;
                }

                //Exit the loop if there are no more closed windows
                if (openWindows.Where(w => w.IsClosed).ToList().Count == 0)
                    break;
            }

            Settings.Instance.OnWindowCountChange();
            OnScreenKeyboard.AttachEventHandler();
        }
        int changeCount = 0;
        public async void UpdateAllOrderViews()
        {
            if(BranchSettings.Instance.branchId != null)
            {
                bool pOSOpen = false;

                for (int i = 0; i < openWindows.Count; i++)
                {
                    if (openWindows[i].GetType() == typeof(POS))
                    {
                        pOSOpen = true;
                        break;
                    }
                }

                if (!pOSOpen)
                    return;

                var result = await FirebaseDataContext.Instance.GetData_Online("Order/" + BranchSettings.Instance.branchId);                                

                List<List<OrderItem>> temp = new List<List<OrderItem>>();

                foreach (var item in result)
                {
                    // We don't need to declare this variable but I'll just leave it
                    List<OrderItem> data = JsonConvert.DeserializeObject<List<OrderItem>>(((JArray)item).ToString());

                    temp.Add(data);
                }

                // @Yewo: Why the need to delete orders from the database?
                foreach (var item in temp)
                {
                    await FirebaseDataContext.Instance.DeleteData("Order/" + BranchSettings.Instance.branchId + "/" + item[0].OrderNumber);//Delete all downloaded orders from DB
                }

                for (int i = 0; i < openWindows.Count; i++)
                {
                    if (openWindows[i].GetType() == typeof(POS))
                    {
                        ((POS)openWindows[i]).UpdateOrderView(temp);
                        await Task.Delay(500);//Waiting for the method called in the dispatcher to conclude
                    }

                    List<List<OrderItem>> ordersUpdated = new List<List<OrderItem>>();
                    ordersUpdated = (List<List<OrderItem>>)(await FirebaseDataContext.Instance.GetOfflineOrdersCompletedInclusive());                    

                    if (openWindows[i].GetType() == typeof(OrderStatus))
                    {
                        var unCollected = ordersUpdated.Where(o => !o[0].Collected).ToList();

                        ((OrderStatus)openWindows[i]).UpdateScreen(unCollected.Where(o => !o[0].MarkedForDeletion).ToList());
                    }
                }
            }
            changeCount++;
        }

        public async void UpdateAllOrderViews_Offline()
        {
            if (BranchSettings.Instance.branchId != null)
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
                        ((POS)openWindows[i]).UpdateOrderView(temp);
                    }

                    if (openWindows[i].GetType() == typeof(OrderStatus))
                    {
                        ((OrderStatus)openWindows[i]).UpdateScreen(temp);
                    }
                }

                //if (TCPClient.client != null)
                    //TCPClient.refreshing = false;

            }
            changeCount++;
        }

        void AddToOpenWindows(BaseWindow target)
        {
            openWindows.Add(target);

            UpdateList();
        }

        public List<BaseWindow> GetAllOpenWindows()
        {
            return openWindows;
        }
    }
}
