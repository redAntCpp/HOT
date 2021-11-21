using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using HOT.lib;
using HOT.Config;
using System.Data.SqlClient;
using System.Threading;

namespace HOT.View
{
    /// <summary>
    /// UC_SearchData.xaml 的交互逻辑  HOT.View.UC_SearchData
    /// </summary>
    public partial class UC_SearchData : UserControl
    {
        private readonly SqlServerHelper ssh_OP;
        private readonly SqlServerHelper ssh_GetData;
        private readonly SqlServerHelper ssh_HOT;
        //private MyCustomControlLibrary.MMessageBox mb;
        public UC_SearchData()
        {
            InitializeComponent();
            //创建连接器
            ssh_OP = new SqlServerHelper
            {
                ConnetionString = HOTConfig.GetConfig().GetHIS_OPConnetionString()
            };
            ssh_GetData = new SqlServerHelper
            {
                ConnetionString = HOTConfig.GetConfig().GetGetDataConnetionString()
            };
            ssh_HOT = new SqlServerHelper
            {
                ConnetionString = HOTConfig.GetConfig().GetHOTConnetionString()
            };
           // mb = new MyCustomControlLibrary.MMessageBox();
        }


        //查询数据，返回查询结果
        //查本地数据库是否含有数据，就诊码的方式进行查询
        public void getData()
        {
            try
            {
                if (txt_search_input.Text == "")
                {/*
                    MyCustomControlLibrary.MMessageBox.ShowAlert(
                               "就诊码不允许为空！", //提示内容
                                Orientation.Horizontal,
                                null, //图标
                               "#3ca9fe", //画笔颜色
                                false,
                                true,
                                10);
                    */
                    MessageBox.Show("就诊码不允许为空!");
                }
                else {
                   
                    if (getPatientData()) //患者信息无误才继续查数据
                    {
                        SqlParameter[] par = { new SqlParameter("@RegisterID", txt_search_input.Text) };
                        DataSet qry = ssh_GetData.ExecSelectStoredProcedure("checkReceipt", par);
                        if (qry.Tables.Count == 1 && qry.Tables[0].Rows.Count == 0)
                        {
                            // MessageBox.Show("查不到该患者的收据记录！");
                            
                            MyCustomControlLibrary.MMessageBox.ShowAlert(
                                "Success!", //提示内容
                                 Orientation.Horizontal,
                                 null, //图标
                                "#3ca9fe", //画笔颜色
                                 false,
                                 true,
                                 3); //
                            txt_search_input.Text = "";
                        }
                        else
                        {
                            dataGrid.DataContext = qry.Tables[0];

                            //同时查操作记录
                            string sqlStr = "SELECT b.UserName,b.deptName,a.OperationTime,a.OperationCount,a.Others,a.itemName, a.registerid " +
                                         "FROM[dbo].[t_ReceiptCheckTrack] a,T_User b " +
                                         "where a.UserID = b.Userid " +
                                         "and a.registerid = @RegisterID";
                            SqlParameter[] par2 = { new SqlParameter("@RegisterID", txt_search_input.Text) };
                            DataSet qry2 = ssh_HOT.SelectData(sqlStr, par2);
                            if (qry2.Tables.Count == 1 && qry2.Tables[0].Rows.Count == 0)
                            {
                                //MessageBox.Show("该患者没有操作记录！");
                                MyCustomControlLibrary.MMessageBox.MClosed();
                                MyCustomControlLibrary.MMessageBox.ShowAlert(
                                "该患者没有操作记录！", //提示内容
                                 Orientation.Horizontal,
                                 null, //图标
                                "#3ca9fe", //画笔颜色
                                 false,
                                 true,
                                 3); //
                                txt_search_input.Text = "";
                            }
                            else
                            {
                                // lbl_Info.Visibility = Visibility.Collapsed;
                                dataGrid2.DataContext = qry2.Tables[0];
                                MyCustomControlLibrary.MMessageBox.MClosed();
                                txt_search_input.Text = "";
                            }
                        }



                    }
                }
               
            }
            catch (Exception ex)
            {
                MyCustomControlLibrary.MMessageBox.MClosed();
                MessageBox.Show(ex.Message);
                
            }
            finally
            {
                MyCustomControlLibrary.MMessageBox.MClosed();
            }
            
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Log.AddTrack("HOT.View.UC_SearchData.dataGrid_MouseDoubleClick", "Begin");
            DataRowView drv = (DataRowView)dataGrid.SelectedItem;
            if (drv != null)
            {
                Log.AddTrack("HOT.View.UC_SearchData.dataGrid_MouseDoubleClick", "查询数据是否存在");
                string ReceiptCheckID = GetReceiptCheckID(drv["OPAccountListID"].ToString());
                if (ReceiptCheckID == "")
                {
                    Log.AddTrack("HOT.View.UC_SearchData.dataGrid_MouseDoubleClick", "查询数据不存在，开始转移");
                    if (MoveDataFromOP(drv))
                    {
                        ReceiptCheckID = GetReceiptCheckID(drv["OPAccountListID"].ToString());
                        MessageWin.EditAccountList ea = new MessageWin.EditAccountList(ReceiptCheckID);
                        ea.ShowDialog();
                    }
                }
                else
                {
                    Log.AddTrack("HOT.View.UC_SearchData.dataGrid_MouseDoubleClick", "数据已存在，无需转移。");
                    MessageWin.EditAccountList ea = new MessageWin.EditAccountList(ReceiptCheckID);
                    ea.ShowDialog();
                }
            }
        }

        private void TabItemButtonOne_Checked(object sender, RoutedEventArgs e)
        {

        }


        private bool MoveDataFromOP(DataRowView drv)
        {
           // DataRowView drv = (DataRowView)dataGrid.SelectedItem;
            try
            {
                if (drv != null)
                {
                    Log.AddTrack("HOT.View.UC_SearchData.MoveDataFromOP", "Begin");
                    string OPAccountListID = drv["OPAccountListID"].ToString();
                    int RestExcutCount;//剩余执行次数
                    Log.AddTrack("HOT.View.UC_SearchData.MoveDataFromOP", "数据不存在，准备转移数据topaccountlist => trecepitcheck");
                    RestExcutCount = Convert.ToInt32(drv["Quantity"]);
                    string sqlStr2 = "INSERT INTO [tReceiptCheck] " +
                                     "([RegisterID], [ReceiptID], [name], [Quantity], [FeeKindName], " +
                                     "[DepartmentName], [RestExcutCount], [OPAccountListID]) " +
                                     "VALUES(@RegisterID, @ReceiptID, @name, @Quantity, @FeeKindName, @DepartmentName, @RestExcutCount, @OPAccountListID); ";
                    SqlParameter[] par2 = {
                        //可能报错，调试注意
                        new SqlParameter("@RegisterID", drv["RegisterID"]) ,
                        new SqlParameter("@ReceiptID",drv["ReceiptID"]),
                        new SqlParameter("@name",drv["name"]),
                        new SqlParameter("@Quantity",drv["Quantity"]),
                        new SqlParameter("@FeeKindName",drv["FeeKindName"]),
                        new SqlParameter("@DepartmentName",drv["DepartmentName"]),
                        //初次的剩余次数 = 总次数
                        new SqlParameter("@RestExcutCount",RestExcutCount),
                        new SqlParameter("@OPAccountListID",drv["OPAccountListID"])
                };
                    int i = ssh_GetData.changeData(sqlStr2, par2);
                    if (i > 0)
                    {
                        Log.AddTrack("HOT.View.UC_SearchData.MoveDataFromOP", "数据转移成功");
                       // MessageBox.Show("数据转移成功");
                        Log.AddTrack("HOT.View.UC_SearchData.MoveDataFromOP", "End");
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("数据转移失败");
                        Log.AddTrack("HOT.View.UC_SearchData.MoveDataFromOP", "数据转移失败");
                        return false;
                    }
                }
                else
                {
                    
                    return false;
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("数据转移异常：" + e.Message);
                Log.AddError("HOT.View.UC_SearchData.MoveDataFromOP", "数据转移异常：" + e.Message);
                throw e;
            }
        }

        //显示编辑窗口
        private void ShowEditWin(object sender, RoutedEventArgs e)
        {
            Log.AddTrack("HOT.View.UC_SearchData.ShowEditWin", "Begin");
            DataRowView drv = (DataRowView)dataGrid.SelectedItem;
            if (drv != null)
            {
                Log.AddTrack("HOT.View.UC_SearchData.ShowEditWin", "查询数据是否存在");
                string ReceiptCheckID = GetReceiptCheckID(drv["OPAccountListID"].ToString());               
                if (ReceiptCheckID == "")
                {
                    Log.AddTrack("HOT.View.UC_SearchData.ShowEditWin", "查询数据不存在，开始转移");
                    if (MoveDataFromOP(drv))
                    {
                        ReceiptCheckID = GetReceiptCheckID(drv["OPAccountListID"].ToString());
                        MessageWin.EditAccountList ea = new MessageWin.EditAccountList(ReceiptCheckID);
                        ea.ShowDialog();
                    }
                }
                else
                {
                    Log.AddTrack("HOT.View.UC_SearchData.ShowEditWin", "数据已存在，无需转移。");
                    MessageWin.EditAccountList ea = new MessageWin.EditAccountList(ReceiptCheckID);
                    ea.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("请选择要编辑的数据");
            }

        }

        private string GetReceiptCheckID(string OPAccountListID)
        {
            string sqlstr = "SELECT ReceiptCheckID from tReceiptCheck where OPAccountListID = @OPAccountListID";
            SqlParameter[] par = { new SqlParameter("@OPAccountListID", OPAccountListID) };
            DataSet qry = ssh_GetData.SelectData(sqlstr, par);
            string ReceiptCheckID;
            if (qry.Tables.Count == 1 && qry.Tables[0].Rows.Count == 0)
            {
                ReceiptCheckID = ""; //没有数据
            }
            else
            {
                ReceiptCheckID = qry.Tables[0].Rows[0]["ReceiptCheckID"].ToString();

            }
            return ReceiptCheckID;
        }

        private void ImageButtonTwo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void GetData_Click(object sender, RoutedEventArgs e)
        {
            getData();
        }



        private void txt_search_input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                GetData_Click(sender, e);
            }
        }

        private bool getPatientData()
        {
            Log.AddTrack("HOT.View.UC_SearchData.getPatientData", "Begin");
                try
                {
                    string sqlStr = "select a.registerdate,a.seqno,b.patientname from op..tregister a,op..tpatient b "
                                  + "where a.patientid = b.patientid and a.RegisterID = @RegisterID";
                    SqlParameter[] par = { new SqlParameter("@RegisterID", txt_search_input.Text.Trim()) };
                    DataSet qry = ssh_OP.SelectData(sqlStr, par);
                    //查询不到数据
                    if (qry.Tables.Count == 1 && qry.Tables[0].Rows.Count == 0)
                    {
                    //MessageBox.Show("找不到患者基本信息");
                    
                    MyCustomControlLibrary.MMessageBox.ShowAlert(
                            "找不到患者基本信息", //提示内容
                             Orientation.Horizontal,
                             null, //图标
                            "#3ca9fe", //画笔颜色
                             false,
                             true,
                             3);

                        Log.AddError("HOT.View.UC_SearchData.getPatientData", "找不到患者的挂号信息：registerid：" + txt_search_input.Text);
                        Log.AddTrack("HOT.View.UC_SearchData.getPatientData", "End");
                       
                        return false;
                    }
                    else
                    {
                        string registerdate = qry.Tables[0].Rows[0]["registerdate"].ToString();
                        DateTime dt = Convert.ToDateTime(registerdate);
                        registerdate = dt.ToString("yyyy/MM/dd");
                        lb_RegisterDate.Content = registerdate;
                        string seqNo = qry.Tables[0].Rows[0]["seqno"].ToString();
                        lb_SeqNo.Content = seqNo;
                        string PatientName = qry.Tables[0].Rows[0]["patientname"].ToString();
                        lb_PatientName.Content = PatientName;
                        Log.AddTrack("HOT.View.UC_SearchData.getPatientData", "End");
                        return true;
                    }
                }
                catch (Exception e)
                {
                
                MessageBox.Show(e.Message);
                    Log.AddError("HOT.View.UC_SearchData.getPatientData", "获取患者挂号信息异常：" + e.StackTrace);
                    return false;
                }
            
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(TBC_data.SelectedIndex == 1)
            {
                //加载页面2的操作
            }
        }

        private void GetReceiptTrack(string registerid)
        {

        }

        private void dataGrid_MouseMove(object sender, MouseEventArgs e)
        {
            //if()
        }
    }
}
