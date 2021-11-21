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
using System.Windows.Shapes;

namespace HOT
{
    /// <summary>
    /// HomeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();
           // Expander_Dictionary.Visibility = Visibility.Collapsed;
           // PatientCaseInfo.Visibility = Visibility.Collapsed;

        }
        private void main_toolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            e.Handled = true;
        }

        //窗口操作  
        private void Btn_MinWindow_Click(object sender, RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        private void Btn_MaxWindow_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void Btn_CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //展示主页
        private void tab_HomePage_checked(object sender, RoutedEventArgs e)
        {

        }

        //传输助手
        private void tab_File_checked(object sender, RoutedEventArgs e)
        {

        }

        private void tab_Chat_checked(object sender, RoutedEventArgs e)
        {
           // grid_mainScreen.Children.Clear();
           // grid_mainScreen.Children.Add(new View.UC_ShowWeb());
        }

        private void tab_OPDepartment_checked(object sender, RoutedEventArgs e)
        {

        }

        private void tab_OPPatient_checked(object sender, RoutedEventArgs e)
        {

        }

        private void tab_MedicareContents_checked(object sender, RoutedEventArgs e)
        {

        }

        private void tab_Others_checked(object sender, RoutedEventArgs e)
        {

        }

        private void tab_Write_checked(object sender, RoutedEventArgs e)
        {

        }

        private void tab_OPDressing_checked(object sender, RoutedEventArgs e)
        {
            grid_mainScreen.Children.Clear();
            grid_mainScreen.Children.Add(new View.UC_SearchData());
        }

        private void tab_Interface_checked(object sender, RoutedEventArgs e)
        {

        }

        private void tab_OPPatientCase_checked(object sender, RoutedEventArgs e)
        {
            grid_mainScreen.Children.Clear();
            grid_mainScreen.Children.Add(new View.UC_PatientCaseInfo());
        }
    }
}
