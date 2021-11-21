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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HOT.View
{
    /// <summary>
    /// UC_ShowWeb.xaml 的交互逻辑
    /// </summary>
    public partial class UC_ShowWeb : UserControl
    {
        public UC_ShowWeb()
        {
            InitializeComponent();
            loading.Visibility = Visibility.Visible;
            
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Wb_showWeb.Source = 
            MessageBox.Show("begin");
            Uri start = new Uri("http://192.168.161.65:89/article/123.html");
            Wb_showWeb.Navigate(start);
            //MessageBox.Show("end");
            
        }

        private async Task LoadDoc()
        {
            
            await Task.Run(
                  () =>
                  {
                      Wb_showWeb.Source = new Uri("http://192.168.161.65:89/article/123.html");
                  }
                );
            
        }

        private void Wb_showWeb_LoadCompleted(object sender, NavigationEventArgs e)
        {
            MessageBox.Show("end");
        }
    }
}
