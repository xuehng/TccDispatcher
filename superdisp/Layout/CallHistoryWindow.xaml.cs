using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using renstech.NET.SupernovaDispatcher.Model;
using renstech.NET.SupernovaDispatcher.Utils;
using MessageWindow = renstech.NET.SupernovaDispatcher.Control.MessageWindow;

namespace renstech.NET.SupernovaDispatcher.Layout
{
    /// <summary>
    /// CallHistoryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CallHistoryWindow : Window
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(CallHistoryWindow));
        SpnvSubSystem _subsystem = null;

        public CallHistoryWindow(SpnvSubSystem system)
        {
            this.InitializeComponent();

            App app = App.Current as App;
            if (app.AppBkBrush != null)
                Background = app.AppBkBrush;

            _subsystem = system;
            _subsystem.CallHisory.ClearUnreadMissing();

            lbxCallHistory.ItemsSource = system.CallHisory.Items;

            system.CallHisory.OnCallHistoryChanged += () => Dispatcher.BeginInvoke(new Action(() => lbxCallHistory.Items.Refresh()));

            btnDelete.IsEnabled = false;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            HistoryItem item = lbxCallHistory.SelectedItem as HistoryItem;
            if (item == null)
            {
                return;
            }

            _subsystem.CallHisory.DeleteCallHistory(item);
        }

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCallHistory.Items.Count == 0)
            {
                return;
            }

            Log.Debug("btnDeleteAll_Click__MessageWindow.ShowDialog BEFORE");

            MessageWindow dialog = new MessageWindow(
                Properties.Resources.IDS_CONFIRM_WINDOW_TITLE,
                Properties.Resources.IDS_CALLHISTORY_DELETE_ALL,
                MessageWindow.ButtonListType.ButtonOkCancel) { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                _subsystem.CallHisory.DeleteAll();
            }

            Log.Debug("btnDeleteAll_Click__MessageWindow.ShowDialog AFTER");
        }

        private void btnCall_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("Protocal Stack Log: (CallHistoryWindow)btnCall_Click, Start");

            HistoryItem item = lbxCallHistory.SelectedItem as HistoryItem;
            if (item == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(item.PartyNumber))
                return;

            DialPrefixType type = _subsystem.PrefixInfo.GetPrefixType(item.PartyNumber);

            if (type != DialPrefixType.DialEavesdrop &&
                 type != DialPrefixType.DialIntercept &&
                 type != DialPrefixType.DialPickup &&
                 type != DialPrefixType.DialThreeway)
            {
                int callId = -1;

                _subsystem.Channels.MakeCall(_subsystem.AccountId, item.PartyNumber, ref callId);

                Log.Debug(String.Format("Protocal Stack Log: (CallHistoryWindow)btnCall_Click, Make Call:{0},{1},{2}", _subsystem.AccountId, item.PartyNumber, callId));
            }

            Log.Debug("Protocal Stack Log: (CallHistoryWindow)btnCall_Click, No Make Call End");
        }

        private void lbxCallHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDelete.IsEnabled = (lbxCallHistory.SelectedItem != null);
        }

        private void BtnExport_OnClick(object sender, RoutedEventArgs args)
        {
            var dlg = new SaveFileDialog();
            dlg.AddExtension = true;
            dlg.DefaultExt = @".csv";
            dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.FileName = "呼叫详单_" + DateTime.Now.ToString("yyyyMMddHHMMss");
            dlg.Filter = @"csv(*.csv)|*.csv";

            Log.Debug("BtnExport_OnClick__SaveFileDialog.ShowDialog BEFORE");

            if (dlg.ShowDialog(this) == true)
            {
                var table = new DataTable();
                table.Columns.Add("主叫姓名");
                table.Columns.Add("主叫号码");
                table.Columns.Add("被叫姓名");
                table.Columns.Add("被叫号码");
                table.Columns.Add("来电/去电");
                table.Columns.Add("是否接通");
                table.Columns.Add("时间");
                //table.Columns.Add("失败原因");

                var items = _subsystem.CallHisory.Items;
                var user = _subsystem.LocalUser;
                foreach (var item in items)
                {
                    var row = table.NewRow();
                    row[0] = item.IsInbound ? item.PartyDispName : user.Name;
                    row[1] = item.IsInbound ? item.PartyNumber : user.Number;
                    row[2] = item.IsInbound ? user.Name : item.PartyDispName;
                    row[3] = item.IsInbound ? user.Number : item.PartyNumber;
                    row[4] = item.IsInbound ? "来电" : "去电";
                    row[5] = item.IsAnswered ? "是" : "否";
                    row[6] = item.DateTime.ToString("G");
                    //row[7] = item.FailureReason;

                    table.Rows.Add(row);
                }

                try
                {
                    using (var writer = new StreamWriter(dlg.FileName, false, Encoding.Default))
                    {
                        int rowCount = table.Rows.Count;
                        int columnCount = table.Columns.Count;

                        //头
                        string[] headList = new string[columnCount];
                        for (int index = 0; index < columnCount; index++)
                        {
                            headList[index] = table.Columns[index].ColumnName;
                        }
                        writer.WriteLine(String.Join(",", headList));

                        //内容
                        for (int iRow = 0; iRow < rowCount; iRow++)
                        {
                            DataRow row = table.Rows[iRow];

                            string[] dataList = new string[columnCount];
                            for (int index = 0; index < columnCount; index++)
                            {
                                dataList[index] = row[index].ToString();
                            }
                            writer.WriteLine(String.Join(",", dataList));
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }
            }

            Log.Debug("BtnExport_OnClick__SaveFileDialog.ShowDialog AFTER");
        }
    }
}