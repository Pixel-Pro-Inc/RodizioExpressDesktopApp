using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RodizioSmartRestuarant.Application.Interfaces;
using RodizioSmartRestuarant.Infrastructure.Configuration;
using RodizioSmartRestuarant.Infrastructure.Helpers;
using MenuItem = RodizioSmartRestuarant.Core.Entities.MenuItem;

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for MenuEditor.xaml
    /// </summary>
    public partial class MenuEditor : Window
    {
        Core.Entities.Aggregates.Menu menu = new Core.Entities.Aggregates.Menu();
        IDataService _dataService;
        IMenuService _menuService;

        public MenuEditor()
        {
            InitializeComponent();
            UpdateMenuView();
        }

        // REFACTOR: Consider putting this in a base window class that we make of our own
        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        async void UpdateMenuView()
        {
            ActivityIndicator.AddSpinner(spinner);

            menuView.Children.Clear();

            Core.Entities.Aggregates.Menu items = await _menuService.GetOnlineMenu(BranchSettings.Instance.branchId);

            menu = items;

            for (int i = 0; i < items.Count; i++)
            {
                menuView.Children.Add(GetPanel(items[i]));
            }

            //Updates with size settings
            Infrastructure.Helpers.Settings.Instance.OnWindowCountChange();

            ActivityIndicator.RemoveSpinner(spinner);
        }

        void UpdateMenuViewSearch(Core.Entities.Aggregates.Menu items)
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
            ActivityIndicator.AddSpinner(spinner);

            Button button = (Button)sender;

            string id = button.Name.Remove(0, 1);

            int numId = Int32.Parse(id);

            for (int i = 0; i < menu.Count; i++)
            {
                if(menu[i].Id == numId)
                {
                    menu[i].Availability = !menu[i].Availability;
                    
                    //UpdateDatabase
                    string branchId = BranchSettings.Instance.branchId;
                    string fullPath = "Menu/" + branchId + "/" + numId;

                    await _dataService.StoreData(fullPath, menu[i]);

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
            Core.Entities.Aggregates.Menu result = _menuService.SearchForQueryString(query, menu);

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

        private void Refresh_Click(object sender, RoutedEventArgs e) => Refresh();

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
