using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HOT
{
    /// <summary>
    /// TabTestWin.xaml 的交互逻辑
    /// </summary>
    public partial class TabTestWin : Window
    {
        public TabTestWin()
        {
            InitializeComponent();
        }

        private void TabItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
           // grid_ChangeWm.Children.Clear();
           // grid_ChangeWm.Children.Add(new View.UC_ShowData());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //加载用户控件
            //
            grid_ChangeWm.Children.Clear();
            grid_ChangeWm.Children.Add(new View.UC_SearchData());
        }

      
    }
}
