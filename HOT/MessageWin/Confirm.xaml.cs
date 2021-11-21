using HOT.Config;
using HOT.lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;


namespace HOT.MessageWin
{
    /// <summary>
    /// Confirm.xaml 的交互逻辑
    /// </summary>
    public partial class Confirm : Window
    {
        private int RestCount;//相减后的剩余次数
        private int ReceiptCheckID;//记录id
        private readonly SqlServerHelper ssh_GetData;
        private readonly SqlServerHelper ssh_HOT;
        private int ExcuteCount;//本次执行次数
        private string Others;//备注
        private string itemName;
        private int registerid;


        public Confirm()
        {
            InitializeComponent();
        }
        
        public Confirm(string inparam)
        {
            InitializeComponent();
            JObject ConfirmMessage = (JObject)JsonConvert.DeserializeObject(inparam);
            this.RestCount = Convert.ToInt32(ConfirmMessage["restCount"].ToString());
            this.ReceiptCheckID =Convert.ToInt32(ConfirmMessage["ReceiptCheckID"].ToString());
            this.ExcuteCount = Convert.ToInt32(ConfirmMessage["ExcuteCount"].ToString());
            this.Others = ConfirmMessage["Others"].ToString();
            this.itemName = ConfirmMessage["itemName"].ToString();
            this.registerid = Convert.ToInt32(ConfirmMessage["RegisterID"].ToString());

            ssh_GetData = new SqlServerHelper
            {
                ConnetionString = HOTConfig.GetConfig().GetGetDataConnetionString()
            };
            ssh_HOT = new SqlServerHelper
            {
                ConnetionString = HOTConfig.GetConfig().GetHOTConnetionString()
            };

        }
        private void Btn_CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void main_toolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Log.AddTrack("HOT.MessageWin.Confirm.OK_Click", "Begin");
            try
            {
                string userNo_input = Name.Text.Trim();
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
                Log.AddTrack("HOT.MessageWin.Confirm.OK_Click", "加密后的密码为 " + passWordEncrypt);
                //组织入参
                JsonDocument inParam = new JsonDocument();
                JsonElement userNo = new JsonElement("userNo", userNo_input);
                JsonElement passWord = new JsonElement("passWord", passWordEncrypt);
                inParam.add(userNo);
                inParam.add(passWord);
                //调用登录接口
                string body = inParam.innerText;
                string postUrl = "http://192.168.161.65/HOTApi/Login";
                Log.AddTrack("HOT.MessageWin.Confirm.OK_Click", "开始调用登录接口：" + body);
                string outParam = HttpService.Post(body, postUrl, false, 6);
                Log.AddTrack("HOT.MessageWin.Confirm.OK_Click", "登录接口返回：" + outParam);
                //解析返回的json结果：
                JObject loginMessage = (JObject)JsonConvert.DeserializeObject(outParam);
                if (loginMessage == null)
                {
                    Log.AddError("HOT.MessageWin.Confirm.OK_Click", "接口调用异常，返回为空");
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
                        //MessageBox.Show("登录成功！");
                        string Userid = loginMessage["UserInfo"]["Userid"].ToString();//获取操作人
                        Log.AddTrack("HOT.MessageWin.Confirm.OK_Click", "开始扣除执行次数...");
                        string sqlStr = "update tReceiptCheck SET RestExcutCount = @RestCount where ReceiptCheckID = @ReceiptCheckID";
                        SqlParameter[] par = { 
                            new SqlParameter("@ReceiptCheckID", ReceiptCheckID) ,
                            new SqlParameter("@RestCount",RestCount)
                        };
                        int i = ssh_GetData.changeData(sqlStr, par);
                        if(i > 0)
                        {
                            Log.AddTrack("HOT.MessageWin.Confirm.OK_Click", "扣除成功!开始记录操作者日志...");
                            sqlStr = "INSERT INTO [dbo].[t_ReceiptCheckTrack] " +
                                     "([UserID], [ReceiptCheckID], [OperationTime], [OperationCount], [Others],[itemName],[registerID]) " +
                                     "VALUES (@UserID, @ReceiptCheckID, @OperationTime, @OperationCount,@Ohters,@itemName,@registerid);";
                            SqlParameter[] par2 =
                            {
                                new SqlParameter("@UserID",Userid),
                                new SqlParameter("@ReceiptCheckID",ReceiptCheckID),
                                new SqlParameter("@OperationTime",DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")),
                                new SqlParameter("@OperationCount",ExcuteCount),
                                new SqlParameter("@Ohters",Others),
                                new SqlParameter("@itemName",itemName),
                                new SqlParameter("@registerid",registerid)
                                
                            };
                            int j = ssh_HOT.changeData(sqlStr, par2);
                            if(j > 0)
                            {
                                Log.AddTrack("HOT.MessageWin.Confirm.OK_Click", "记录操作者日志成功！");
                                MessageBox.Show("执行成功");
                                //this.DialogResult = true;
                                this.Close();
                            }
                            else
                            {
                                Log.AddError("HOT.MessageWin.Confirm.OK_Click", "记录操作者日志失败！");
                            }
                        }
                        else
                        {
                            
                            Log.AddError("HOT.MessageWin.Confirm.OK_Click", "执行失败!");
                        }
                        
                    }
                }
                Log.AddTrack("HOT.MessageWin.Confirm.OK_Click", "End");
            }
            catch (Exception ex)
            {
                MessageBox.Show("HOT.MessageWin.Confirm.OK_Click 执行异常：" + ex.Message);
                Log.AddError("HOT.MessageWin.Confirm.OK_Click", "执行异常：" + ex.Message);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Owner.Close();
        }
    }
}
