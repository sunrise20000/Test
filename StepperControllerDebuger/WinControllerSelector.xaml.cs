﻿using System;
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

namespace StepperControllerDebuger
{
    /// <summary>
    /// WinControllerSelector.xaml 的交互逻辑
    /// </summary>
    public partial class WinControllerSelector : Window
    {
        public WinControllerSelector()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.HidSN = cmbDeviceList.SelectedItem.ToString();

            this.Hide();

            MainWindow win = new MainWindow();
            win.ShowDialog();

            Application.Current.Shutdown();

        }

        private void cmbDeviceList_DropDownOpened(object sender, EventArgs e)
        {
            string[] devices = IrixiStepperControllerHelper.IrixiMotionController.GetDeviceList();
            cmbDeviceList.ItemsSource = devices;
        }
    }
}
