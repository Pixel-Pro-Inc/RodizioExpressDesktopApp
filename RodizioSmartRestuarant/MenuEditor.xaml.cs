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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Helpers;
using MenuItem = RodizioSmartRestuarant.Entities.MenuItem;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for MenuEditor.xaml
    /// </summary>
    public partial class MenuEditor : Window
    {
        List<MenuItem> menuItems = new List<MenuItem>();
        private FirebaseDataContext firebaseDataContext;
        public MenuEditor()
        {
            InitializeComponent();

            firebaseDataContext = FirebaseDataContext.Instance;

            UpdateMenuView();
        }

        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        async void UpdateMenuView()
        {
            menuView.Children.Clear();

            var result = await firebaseDataContext.GetData("Menu/" + BranchSettings.Instance.branchId);

            List<MenuItem> items = new List<MenuItem>();

            foreach (var item in result)
            {
                MenuItem menuItem = JsonConvert.DeserializeObject<MenuItem>(((JObject)item).ToString());

                items.Add(menuItem);
            }

            menuItems = items;

            for (int i = 0; i < items.Count; i++)
            {
                menuView.Children.Add(GetPanel(items[i]));
            }
        }

        void UpdateMenuViewSearch(List<MenuItem> items)
        {
            menuView.Children.Clear();

            for (int i = 0; i < items.Count; i++)
            {
                menuView.Children.Add(GetPanel(items[i]));
            }
        }

        StackPanel GetPanel(MenuItem menuItem)
        {
            StackPanel stackPanel = new StackPanel()
            {
                Margin = new Thickness(10),
                Background = new SolidColorBrush(Colors.White)
            };

            Label label = new Label()
            {
                Content = menuItem.Name
            };

            Button button = new Button()
            {
                Content = menuItem.Availability ? "Mark as unavailable" : "Mark as available",
                Name = "n" + menuItem.Id
            };

            button.Click += AvailablityToggle_Click;

            stackPanel.Children.Add(label);
            stackPanel.Children.Add(button);

            return stackPanel;
        }

        private async void AvailablityToggle_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            string id = button.Name.Remove(0, 1);

            int numId = Int32.Parse(id);

            for (int i = 0; i < menuItems.Count; i++)
            {
                if(menuItems[i].Id == numId)
                {
                    menuItems[i].Availability = !menuItems[i].Availability;
                    
                    //UpdateDatabase
                    string branchId = BranchSettings.Instance.branchId;
                    string fullPath = "Menu/" + branchId + "/" + numId;

                    await firebaseDataContext.EditData(fullPath, menuItems[i]);

                    UpdateMenuView();
                }
            }
        }

        //Search

        int block;
        bool showingResults;

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (block == 0)
            {
                searchBox.IsEnabled = false;
                searchBox.IsReadOnly = true;

                Search(searchBox.Text);
                block = 1;
            }
        }

        private void Search(string query)
        {
            List<MenuItem> result = new SearchMenu().Search(query, menuItems);

            showingResults = true;            

            if (result.Count != 0)
            {
                UpdateMenuViewSearch(result);
                return;
            }

            string messageBoxText = "Your search had 0 results.";
            string caption = "Search";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            block = 0;

            Search(searchBox.Text);
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (showingResults)
            {
                showingResults = false;

                searchBox.Text = "";
                searchBox.IsEnabled = true;
                searchBox.IsReadOnly = false;

                block = 0;

                UpdateMenuView();
            }
        }
    }
}
