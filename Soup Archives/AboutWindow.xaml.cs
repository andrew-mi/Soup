﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Soup_Archives
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            Closing += AboutWindow_Closing;
        }

        private void AboutWindow_Closing(object sender, CancelEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) => System.Diagnostics.Process.Start(e.Uri.ToString());


        #region Window Titlebar
        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(params string[] PropertyNames)
        {
            foreach (string PropertyName in PropertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e) =>
            Application.Current.Shutdown();

        private void CanExecute(object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = true;

        private void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (TitleBar.IsMouseDirectlyOver)
                {
                    DragMove();
                }
            }
        }
        #endregion
    }
}
