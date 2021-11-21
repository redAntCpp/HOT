using HOT.Config;
using HOT.lib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HOT.MessageWin
{
    /// <summary>
    /// EditAccountList.xaml 的交互逻辑
    /// </summary>
    public partial class EditAccountList : Window
    {
        private readonly SqlServerHelper ssh_GetData;
        private string ReceiptCheckID;
        private string RegisterID;
        public EditAccountList()
        {
            InitializeComponent();
        }
        public EditAccountList(string ReceiptCheckID)
        {
            InitializeComponent();
            this.ReceiptCheckID = ReceiptCheckID;
            ssh_GetData = new SqlServerHelper
            {
                ConnetionString = HOTConfig.GetConfig().GetGetDataConnetionString()
            };
            string sqlStr = "select ReceiptCheckID,RegisterID, name,RestExcutCount,DepartmentName from [dbo].[tReceiptCheck] " +
                            "where ReceiptCheckID = @ReceiptCheckID";
            SqlParameter[] par = { new SqlParameter("@ReceiptCheckID", ReceiptCheckID) };
            DataTable dt = ssh_GetData.SelectData(sqlStr, par).Tables[0];
            this.Name.Text = dt.Rows[0]["name"].ToString();
            this.RestExcuteCount.Text = dt.Rows[0]["RestExcutCount"].ToString();
            this.ExcuteDepartment.Text = dt.Rows[0]["DepartmentName"].ToString();
            RegisterID = dt.Rows[0]["RegisterID"].ToString();
        }
        public EditAccountList(string itemName, string RestExcuteCount, string itemExcuteDepartment)
        {
            InitializeComponent();
            this.Name.Text = itemName;
            this.RestExcuteCount.Text = RestExcuteCount;
            this.ExcuteDepartment.Text = itemExcuteDepartment;



        }

        private void Btn_CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void main_toolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }


        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if(txt_ExcuteCount.Text == "")
            {
                MessageBox.Show("执行次数不允许为空");
            }
            else
            {
                //int restCount = Convert.ToInt32(RestExcuteCount.Text.Trim())
                int restCount = Convert.ToInt32(RestExcuteCount.Text.Trim()) - Convert.ToInt32(txt_ExcuteCount.Text.Trim());
                if(restCount < 0)
                {
                    MessageBox.Show("执行次数不允许大于剩余次数！");
                }
                else
                {
                    //参数数量太多，使用json转换
                    JsonDocument jd = new JsonDocument();
                    JsonElement RestCount = new JsonElement("restCount", restCount.ToString());
                    JsonElement ExcuteCount = new JsonElement("ExcuteCount", txt_ExcuteCount.Text.Trim());
                    JsonElement itemName = new JsonElement("itemName", Name.Text);
                    JsonElement other = new JsonElement("Others", txt_Others.Text.Trim());
                    JsonElement ID = new JsonElement("ReceiptCheckID", ReceiptCheckID);
                    JsonElement Registerid = new JsonElement("RegisterID", RegisterID);
                    jd.add(RestCount);
                    jd.add(ExcuteCount);
                    jd.add(itemName);
                    jd.add(other);
                    jd.add(ID);
                    jd.add(Registerid);

                    MessageWin.Confirm cf = new MessageWin.Confirm(jd.innerText);
                    cf.Owner = this;
                    cf.ShowDialog();                  
                }
            }
        }

        private void txt_ExcuteCount_KeyDown(object sender, KeyEventArgs e)
        {
            //if(!char.IsDigit(e.KeyChar))
        }

        private void txt_ExcuteCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //在文本输入之前，检测是否为数字
            //InputMethod.IsInputMethodEnabled="False" 屏蔽输入法
            //使用正则表达式
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);                
        }
    }
}
