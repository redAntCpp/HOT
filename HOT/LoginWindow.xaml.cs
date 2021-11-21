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
using HOT.lib;
using HOT.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HOT
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            //初始化配置
           // HOTConnetion.ConnetionString = HOTConfig.GetConfig().GetHOTConnetionString();
            OPConnetion.ConnetionString = HOTConfig.GetConfig().GetHIS_OPConnetionString();
        }

        SqlServerHelper HOTConnetion = new SqlServerHelper();
        SqlServerHelper OPConnetion = new SqlServerHelper();
        private void Btn_CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        //登录界面
        private void login_Click(object sender, RoutedEventArgs e)
        {
            LoginHOT();
        }

        private void LoginHOT()
        {

            Log.AddTrack("HOT.LoginWindow.LoginHOT", "Begin");
            try
            {
                Log.AddTrack("HOT.LoginWindow.LoginHOT", "校验用户名密码输入格式...");
                string userNo_input = account.Text.Trim();
                string passWord_input = Password.Password.Trim();
                if (userNo_input == "")
                {
                    MessageBox.Show("用户名不能为空！");
                    return;
                }
                if (passWord_input == "")
                {
                    MessageBox.Show("密码不能为空！");
                    return;
                }
                string passWordEncrypt = Encrypt.MD5EncryptFor64(passWord_input);
                Log.AddTrack("HOT.LoginWindow.LoginHOT", "加密后的密码为 " + passWordEncrypt);
                //组织入参
                JsonDocument inParam = new JsonDocument();
                JsonElement userNo = new JsonElement("userNo", userNo_input);
                JsonElement passWord = new JsonElement("passWord", passWordEncrypt);
                inParam.add(userNo);
                inParam.add(passWord);
                //调用登录接口
                string body = inParam.innerText;
                string postUrl = "http://192.168.161.65/HOTApi/Login";
                Log.AddTrack("HOT.LoginWindow.LoginHOT", "开始调用登录接口：" + body);
                string outParam = HttpService.Post(body, postUrl, false, 6);
                Log.AddTrack("HOT.LoginWindow.LoginHOT", "登录接口返回：" + outParam);
                //解析返回的json结果：
                JObject loginMessage = (JObject)JsonConvert.DeserializeObject(outParam);
                if (loginMessage == null)
                {
                    Log.AddError("HOT.LoginWindow.LoginHOT", "接口调用异常，返回为空");
                    throw new HOTException("登录接口返回超时");//抛出此异常，用于捕获                  
                }
                else
                {
                    if (loginMessage["retCode"].ToString() != "0")
                    {
                        MessageBox.Show(loginMessage["retMsg"].ToString());
                    }
                    else
                    {
                        MessageBox.Show("登录成功！");
                    }
                }
                Log.AddTrack("HOT.LoginWindow.LoginHOT", "End");
            }
            catch (HOTException ex)
            {
                Log.AddError("HOTException：", ex.Message);

                MessageBox.Show("登录异常:" + ex.Message);
            }
        }

        private void account_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Password.Focus();
            }
        }
    }
}
