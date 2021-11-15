﻿using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Helpers;
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

namespace RodizioSmartRestuarant
{
    /// <summary>
    /// Interaction logic for LoadingScreen.xaml
    /// </summary>
    public partial class LoadingScreen : Window
    {
        public LoadingScreen()
        {
            InitializeComponent();

            AppInitialization();

            NextPage();
        }

        void AppInitialization()
        {
            BranchSettings.Instance = new BranchSettings();
            LocalStorage.Instance = new LocalStorage();
        }

        private void LoadingScreen_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        async void NextPage()
        {
            await Task.Delay(3000);

            Window current = this;
            Window next = new Login();

            current.Hide();

            next.Show();
            current.Close();
        }
    }
}
