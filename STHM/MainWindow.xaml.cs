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
using STHM.Data;
using System.Windows.Forms;
using STHM.Media;
using STHM.Device;
using System.Data;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;
using System.Diagnostics;
using Excel = Microsoft.Office.Interop.Excel;
using STHM.LIB;
using DevExpress.Xpf.Charts;
using System.Xml.Linq;

namespace STHM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum EnumLineStatus
        {
            NOTCONNECTED,
            CONNECTED,
            DISCONNECTED
        }

        public event EventHandler TaskCompleted;

        private Common common = new Common();

        private Random random;

        private int tempIndex;

        private Dictionary<string, Queue<TaskInfo>> TaskBuffer = new Dictionary<string, Queue<TaskInfo>>();

        private Dictionary<string, ESSocket> dictConnectedLine = new Dictionary<string, ESSocket>();

        private Dictionary<string, ESMedia> dictMedia;

        private Dictionary<string, DateTime> dictUncompletedSchedule = new Dictionary<string, DateTime>();

        private DataSet dsSchedule = null;

        private Dictionary<int, Schedule> dictScheduleList = null;

        private bool reloadScheduleFlag = false;

        private DataTable dtConnectedLine;

        private Socket serverSocket;

        private Config config = new Config();

        private byte[] byteData = new byte[1024];

        public Dictionary<string, string> dictEvent;

        private Thread licenseExp;

        private bool bForceClose = false;

        private System.Windows.Forms.Timer timerSchedule;

        private System.Windows.Forms.Timer timerTaskQueue;

        //public System.Windows.Forms.RichTextBox rtbLog;
        DataTable DT_SL_PGiao = new DataTable();
        DataTable DT_SL_PNhan = new DataTable();
        DataTable DT_SL_QGiao = new DataTable();
        DataTable DT_SL_QNhan = new DataTable();
        DataTable DT_CS_PGiao = new DataTable();
        DataTable DT_CS_PNhan = new DataTable();
        DataTable DT_CS_QGiao = new DataTable();
        DataTable DT_CS_QNhan = new DataTable();
        DateTime ngaybieudo;
        LineSeries2D highlightedSeries = null;
        bool isAuto = false;

        public MainWindow(Config config)
        {
            InitializeComponent();
            this.config = config;
            this.config.ExportDataCompany = this.config.ExportDataOldPath;
            this.Loaded += MainWindow_Loaded;
            timerSchedule = new System.Windows.Forms.Timer();
            timerTaskQueue = new System.Windows.Forms.Timer();
            timerSchedule.Enabled = true;
            timerSchedule.Interval = 60000;
            timerSchedule.Tick += new System.EventHandler(timerSchedule_Tick);
            timerTaskQueue.Enabled = true;
            timerTaskQueue.Interval = 3000;
            timerTaskQueue.Tick += new System.EventHandler(timerTaskQueue_Tick);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTaskname();
            LoadConnectedLine();
            LoadConfig();
            //SetPermissions(config.UserLoggedIn);
            if (btnStartSchedule.Content.Equals("DỪNG LỊCH TỰ ĐỘNG"))
            {
                timerSchedule.Start();
                btnStartSchedule.Background = new SolidColorBrush(Colors.Red);
            }
            dtpickerStartfrom.EditValue = DateTime.Now;

            if (!DT_SL_PGiao.Columns.Contains("CK")) DT_SL_PGiao.Columns.Add("CK", typeof(string));
            if (!DT_SL_PGiao.Columns.Contains("W")) DT_SL_PGiao.Columns.Add("W", typeof(string));
            DT_SL_PNhan = DT_SL_PGiao.Clone();
            DT_SL_QGiao = DT_SL_PGiao.Clone();
            DT_SL_QNhan = DT_SL_PGiao.Clone();

            DT_CS_PGiao = DT_SL_PGiao.Clone();
            DT_CS_PNhan = DT_SL_PGiao.Clone();
            DT_CS_QGiao = DT_SL_PGiao.Clone();
            DT_CS_QNhan = DT_SL_PGiao.Clone();
        }


        void rtbLog_TextChanged(object sender, EventArgs e)
        {
            try
            {
                System.Windows.Forms.RichTextBox r = (System.Windows.Forms.RichTextBox)sender;
                if (r.InvokeRequired)
                {
                    r.BeginInvoke((MethodInvoker)delegate
                    {
                        rtbLog_TextChanged(sender, e);
                    });
                    return;
                }
                else
                {
                    DisplayLog(r.Text);
                }
            }
            catch (Exception ex)
            {
                common.ShowDialog(ex.Message);
            }
        }

        private void DisplayLog(string strLog)
        {
            if (!rtbLog1.Dispatcher.CheckAccess())
            {
                rtbLog1.Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    DisplayLog(strLog);
                });
                return;
            }
            rtbLog1.Document.Blocks.Clear();
            rtbLog1.AppendText(strLog);
            rtbLog1.ScrollToEnd();
        }

        private void LoadTaskname()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("ReadLoadProfile", "Phụ tải công suất");
            data.Add("ReadLoadProfile_A0", "Phụ tải sản lượng");
            cboTASKNAME.ItemsSource = data;
            cboTASKNAME.DisplayMember = "Value";
            cboTASKNAME.ValueMember = "Key";
            cboTASKNAME.SelectedIndex = 0;
        }
        private void LoadConnectedLine()
        {
            using (var db = new Data.HtsmContext())
            {

                this.dtConnectedLine = this.JoinMeterWithCommLine(db.Lines.ToDataTable(), db.Meters.ToDataTable());
                DataSet line = new DataSet();
                line.Tables.Add(db.Lines.ToDataTable());
                DataSet meter = new DataSet();
                DataTable DT_METER = db.Meters.ToDataTable();
                DT_METER.TableName = "METER";
                meter.Tables.Add(DT_METER);
                this.common.SaveXML(line, PublicValue.linepath);
                this.common.SaveXML(meter, PublicValue.meterpath);
            }
        }

        public DataTable JoinMeterWithCommLine(DataTable table1, DataTable table2)
        {
            DataTable dtResult = new DataTable();
            if (table1 == null || table1.Rows.Count <= 0)
            {
                return dtResult;
            }
            if (table2 == null || table2.Rows.Count <= 0)
            {
                return dtResult;
            }
            try
            {
                dtResult.Columns.Add("LINE_ID", typeof(string));
                dtResult.Columns.Add("LINE_TYPE", typeof(string));
                dtResult.Columns.Add("CONFIG", typeof(string));
                dtResult.Columns.Add("CONNECTION_STATUS", typeof(string));
                dtResult.Columns.Add("READING_STATUS", typeof(string));
                dtResult.Columns.Add("RECONNECT_NUMBER", typeof(string));
                dtResult.Columns.Add("MET_ID", typeof(string));
                dtResult.Columns.Add("MET_KEY", typeof(string));
                dtResult.Columns.Add("MADDO", typeof(string));
                dtResult.Columns.Add("TENDDO", typeof(string));
                dtResult.Columns.Add("DATALINK_OSN", typeof(string));
                dtResult.Columns.Add("PROG_PW", typeof(string));
                dtResult.Columns.Add("OUT_STATION_NUMBER", typeof(string));
                dtResult.Columns.Add("BAUD_RATE", typeof(string));
                dtResult.Columns.Add("REG_NUM", typeof(string));
                dtResult.Columns.Add("MET_TYPE", typeof(string));
                IEnumerable<DataRow> result = from datarow1 in table1.AsEnumerable()
                                              join datarow2 in table2.AsEnumerable() on (string)datarow1["LINE_ID"] equals (string)datarow2["COMM_LINE"]
                                              select dtResult.LoadDataRow(new object[16]
					{
						(string)datarow1["LINE_ID"],
						(string)datarow1["LINE_TYPE"],
						(string)datarow1["CONFIG"],
						(string)datarow1["CONNECTION_STATUS"],
						(string)datarow1["READING_STATUS"],
						(string)datarow1["RECONNECT_NUMBER"].ToString(),
						(string)datarow2["MET_ID"],
						(string)datarow2["MET_KEY"],
						(string)datarow2["MADDO"],
						(string)datarow2["TENDDO"],
						(string)datarow2["DATALINK_OSN"],
						(string)datarow2["PROG_PW"],
						(string)datarow2["OUT_STATION_NUMBER"],
						(string)datarow2["BAUD_RATE"].ToString(),
						(string)datarow2["REG_NUM"].ToString(),
						(string)datarow2["MET_TYPE"]
					}, fAcceptChanges: false);
                dtResult = result.CopyToDataTable();
            }
            catch
            {
                return dtResult;
            }
            return dtResult;
        }

        private void btnSetup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frm_SetupMangager fromSetup = new frm_SetupMangager();
                fromSetup.ShowDialog();
                if (fromSetup.IsChanged)
                {
                    LoadConnectedLine();
                    UpdateGrid(dtConnectedLine);
                    BuildMediaAndTask(dtConnectedLine);
                }
            }
            catch
            {

            }
        }

        private void btnStartSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (btnStartSchedule.Content.Equals("CHẠY LỊCH TỰ ĐỘNG"))
            {
                btnStartSchedule.Background = new SolidColorBrush(Colors.Red);
                btnStartSchedule.Content = "DỪNG LỊCH TỰ ĐỘNG";
                timerSchedule.Start();
            }
            else
            {
                btnStartSchedule.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFA3C3EC");
                btnStartSchedule.Content = "CHẠY LỊCH TỰ ĐỘNG";
                ClearSchedule();
            }
        }

        private void ClearSchedule()
        {
            try
            {
                timerSchedule.Stop();
                timerTaskQueue.Stop();
                foreach (Queue<TaskInfo> taskqueue in TaskBuffer.Values)
                {
                    int taskcount = taskqueue.Count;
                    for (int i = 0; i < taskcount; i++)
                    {
                        TaskInfo para = taskqueue.Dequeue();
                        if (para.Taskid == 0)
                        {
                            taskqueue.Enqueue(para);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog("ClearSchedule() error:" + ex.Message);
            }
            finally
            {
                reloadScheduleFlag = true;
                timerTaskQueue.Start();
            }
        }

        private void AddLog(string slog)
        {
            if (!rtbLog1.Dispatcher.CheckAccess())
            {
                rtbLog1.Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    AddLog(slog);
                });
                return;
            }
            if (rtbLog1.Document.Blocks.Count == 100)
            {
                rtbLog1.Document.Blocks.Clear();
            }
            rtbLog1.AppendText(slog);
            rtbLog1.ScrollToEnd();
            MediaLog.DisplayData(MediaLog.MessageType.Normal, slog);
        }


        private void timerSchedule_Tick(object sender, EventArgs e)
        {
            timerSchedule.Stop();
            if (dsSchedule == null || dsSchedule.Tables.Count <= 0 || reloadScheduleFlag)
            {
                dsSchedule = new DataSet();
                dictScheduleList = new Dictionary<int, Schedule>();
                try
                {
                    //string scheduleFilename = Application.StartupPath + "\\Schedule.cfg";
                    //if (File.Exists(scheduleFilename))
                    //{
                    //    dsSchedule.ReadXml(scheduleFilename);
                    //}
                    using (var db = new HtsmContext())
                    {
                        dsSchedule.Tables.Add(db.Schedules.ToDataTable());
                    }
                    if (dsSchedule == null || dsSchedule.Tables.Count <= 0)
                    {
                        timerSchedule.Start();
                        return;
                    }
                }
                catch (Exception ex2)
                {
                    AddLog("LoadSchedule() Error: " + ex2.Message);
                    timerSchedule.Start();
                    return;
                }
                foreach (DataRow dr in dsSchedule.Tables[0].Rows)
                {
                    string lineid5 = dr["LINE_ID"].ToString();
                    Schedule schedule = new Schedule();
                    schedule.F_ID = int.Parse(dr["F_ID"].ToString());
                    schedule.ENABLED = bool.Parse(dr["ENABLED"].ToString());
                    schedule.TASK_NAME = dr["TASK_NAME"].ToString();
                    schedule.STARTDATE = DateTime.Parse(dr["STARTDATE"].ToString());
                    schedule.SET_NUMBER = int.Parse((dr["SET_NUMBER"].ToString() == "") ? "0" : dr["SET_NUMBER"].ToString());
                    schedule.F_PERIOD_VALUE = int.Parse(dr["F_PERIOD_VALUE"].ToString());
                    schedule.F_READING_STATUS = int.Parse(dr["F_READING_STATUS"].ToString());
                    schedule.F_MET_KEY = dr["F_MET_KEY"].ToString();
                    schedule.PROG_PW = dr["PROG_PW"].ToString().Trim();
                    schedule.OUT_STATION_NUMBER = dr["OUT_STATION_NUMBER"].ToString().Trim();
                    schedule.MET_TYPE = dr["MET_TYPE"].ToString();
                    schedule.F_EXCEPTION = bool.Parse(dr["F_EXCEPTION"].ToString());
                    schedule.F_EXCEPT_FROM = DateTime.Parse(dr["F_EXCEPT_FROM"].ToString());
                    schedule.F_EXCEPT_TO = DateTime.Parse(dr["F_EXCEPT_TO"].ToString());
                    schedule.F_READING_COUNT = int.Parse(dr["F_READING_COUNT"].ToString());
                    schedule.DATALINK_OSN = dr["DATALINK_OSN"].ToString();
                    if (schedule.F_EXCEPT_FROM > schedule.F_EXCEPT_TO)
                    {
                        schedule.F_EXCEPT_TO = schedule.F_EXCEPT_TO.AddDays(1.0);
                    }
                    dictScheduleList[schedule.F_ID] = schedule;
                }
                reloadScheduleFlag = false;
            }
            try
            {
                if (dictScheduleList.Count <= 0)
                {
                    return;
                }
                DateTime currentDate = DateTime.Now;
                foreach (KeyValuePair<int, Schedule> schedule2 in dictScheduleList)
                {
                    if (!schedule2.Value.ENABLED)
                    {
                        continue;
                    }
                    string linetype;
                    switch (schedule2.Value.TASK_NAME)
                    {
                        case "ReadLoadProfile":
                            if (schedule2.Value.F_READING_STATUS == 1)
                            {
                                break;
                            }
                            if (schedule2.Value.F_EXCEPTION)
                            {
                                if ((!(currentDate < schedule2.Value.F_EXCEPT_FROM) && !(currentDate > schedule2.Value.F_EXCEPT_TO)) || !(currentDate >= schedule2.Value.STARTDATE))
                                {
                                    break;
                                }
                                string lineid2;
                                GetLineInfo(schedule2.Value.F_MET_KEY, out lineid2, out linetype);
                                int setNumberTemp2 = schedule2.Value.SET_NUMBER;
                                if (schedule2.Value.STARTDATE < DateTime.Now)
                                {
                                    int setNumber2 = DateTime.Now.Subtract(schedule2.Value.STARTDATE).Days + 1;
                                    if (setNumber2 > schedule2.Value.SET_NUMBER)
                                    {
                                        schedule2.Value.SET_NUMBER = setNumber2;
                                    }
                                }
                                addScheduleTask2Queue(lineid2, schedule2.Value);
                                schedule2.Value.SET_NUMBER = setNumberTemp2;
                            }
                            else
                            {
                                if (!(currentDate >= schedule2.Value.STARTDATE))
                                {
                                    break;
                                }
                                string lineid;
                                GetLineInfo(schedule2.Value.F_MET_KEY, out lineid, out linetype);
                                int setNumberTemp = schedule2.Value.SET_NUMBER;
                                if (schedule2.Value.STARTDATE < DateTime.Now)
                                {
                                    int setNumber = DateTime.Now.Subtract(schedule2.Value.STARTDATE).Days + 1;
                                    if (setNumber > schedule2.Value.SET_NUMBER)
                                    {
                                        schedule2.Value.SET_NUMBER = setNumber;
                                    }
                                }
                                addScheduleTask2Queue(lineid, schedule2.Value);
                                schedule2.Value.SET_NUMBER = setNumberTemp;
                            }
                            break;
                        case "ReadCurrentRegisterValues":
                            if (currentDate >= schedule2.Value.STARTDATE && schedule2.Value.F_READING_STATUS != 1)
                            {
                                string lineid3;
                                GetLineInfo(schedule2.Value.F_MET_KEY, out lineid3, out linetype);
                                addScheduleTask2Queue(lineid3, schedule2.Value);
                                schedule2.Value.F_READING_STATUS = 1;
                            }
                            break;
                        case "ReadHistoricalRegister":
                            if (currentDate >= schedule2.Value.STARTDATE && schedule2.Value.F_READING_STATUS != 1)
                            {
                                string lineid4;
                                GetLineInfo(schedule2.Value.F_MET_KEY, out lineid4, out linetype);
                                addScheduleTask2Queue(lineid4, schedule2.Value);
                                schedule2.Value.F_READING_STATUS = 1;
                            }
                            break;
                        case "ReadLoadProfile_A0":
                            if (currentDate >= schedule2.Value.STARTDATE && schedule2.Value.F_READING_STATUS != 1)
                            {
                                string lineid6;
                                GetLineInfo(schedule2.Value.F_MET_KEY, out lineid6, out linetype);
                                int setNumberTemp3 = schedule2.Value.SET_NUMBER;
                                int setNumber3 = DateTime.Now.Subtract(schedule2.Value.STARTDATE).Days;
                                if (setNumber3 > schedule2.Value.SET_NUMBER)
                                {
                                    schedule2.Value.SET_NUMBER = setNumber3;
                                }
                                else
                                {
                                    schedule2.Value.STARTDATE = currentDate.Date.Add(new TimeSpan(schedule2.Value.STARTDATE.Hour, schedule2.Value.STARTDATE.Minute, schedule2.Value.STARTDATE.Second)).Subtract(new TimeSpan(1, 0, 0, 0));
                                }
                                schedule2.Value.IS_FROM_STARTDATE = true;
                                addScheduleTask2Queue(lineid6, schedule2.Value);
                                schedule2.Value.F_READING_STATUS = 1;
                                schedule2.Value.SET_NUMBER = setNumberTemp3;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog("timerSchedule_Tick() error: " + ex.Message);
            }
            finally
            {
                timerSchedule.Start();
            }
        }

        private void GetLineInfo(string meterid, out string lineid, out string linetype)
        {
            lineid = "";
            linetype = "";
            try
            {
                DataRow[] drs = dtConnectedLine.Select("MET_KEY = '" + meterid + "'");
                DataRow[] array = drs;
                DataRow[] array2 = array;
                DataRow[] array3 = array2;
                foreach (DataRow dr in array3)
                {
                    if (dr != null)
                    {
                        lineid = dr["LINE_ID"].ToString();
                        linetype = dr["LINE_TYPE"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog("GetLineInfo() error: " + ex.Message);
            }
        }
        private void addScheduleTask2Queue(string lineid, Schedule schedule)
        {
            if (!dictMedia.ContainsKey(lineid))
            {
                return;
            }
            TaskInfo para = createTaskFromSchedule(schedule);
            bool bfind = false;
            foreach (TaskInfo ti in TaskBuffer[lineid])
            {
                if (ti.MeterID == para.MeterID && ti.Taskname == para.Taskname)
                {
                    bfind = true;
                    break;
                }
            }
            if (!bfind)
            {
                TaskBuffer[lineid].Enqueue(para);
            }
        }

        private TaskInfo createTaskFromSchedule(Schedule schedule)
        {
            TaskInfo para = new TaskInfo();
            para.Taskid = schedule.F_ID;
            para.OutStationNumber = schedule.OUT_STATION_NUMBER;
            para.Password = schedule.PROG_PW;
            para.Taskname = schedule.TASK_NAME;
            para.MeterID = schedule.F_MET_KEY;
            para.SetNumber = schedule.SET_NUMBER;
            para.StartDate = schedule.STARTDATE;
            para.IsFromDate = schedule.IS_FROM_STARTDATE;
            para.DeviceType = schedule.MET_TYPE;
            para.DataLinkOSN = schedule.DATALINK_OSN;
            return para;
        }

        public void LoadConfig()
        {
            if (!base.Dispatcher.CheckAccess())
            {
                base.Dispatcher.Invoke((MethodInvoker)delegate
                {
                    LoadConfig();
                });
                return;
            }
            try
            {
                timerSchedule.Interval = config.ScheduleInterval;
                timerSchedule.Stop();
                //fRead.StartFrom = DateTime.Now;
                stbTaikhoan.Content = "Tài khoản đăng nhập: " + config.UserLoggedIn;
                //rgrbProfile.IsChecked = true;
                if (config.AutoRun)
                {
                    btnStart_Click(null, null);
                }
                BuildGridHeader(config.IsPCVersion);
                if (!config.IsPCVersion)
                {
                    btnStartSchedule_Click(null, null);
                }
                UpdateGrid(dtConnectedLine);
                BuildMediaAndTask(dtConnectedLine);
            }
            catch (Exception ex)
            {
                AddLog("LoadConfig() Error: " + ex.Message);
            }
        }

        public void BuildGridHeader(bool isPCVersion)
        {
            //ColumnGroupsViewDefinition columnGroupsView = new ColumnGroupsViewDefinition();
            //columnGroupsView.ColumnGroups.Add(new GridViewColumnGroup("Kênh truyền thông"));
            //columnGroupsView.ColumnGroups[0].Rows.Add(new GridViewColumnGroupRow());
            //columnGroupsView.ColumnGroups[0].Rows[0].Columns.Add(dtgCommLines.Columns["LINE_ID"]);
            //columnGroupsView.ColumnGroups[0].Rows[0].Columns.Add(dtgCommLines.Columns["LINE_TYPE"]);
            //columnGroupsView.ColumnGroups[0].Rows[0].Columns.Add(dtgCommLines.Columns["CONFIG"]);
            //columnGroupsView.ColumnGroups[0].Rows[0].Columns.Add(dtgCommLines.Columns["CONNECTION_STATUS"]);
            //columnGroupsView.ColumnGroups[0].Rows[0].Columns.Add(dtgCommLines.Columns["READING_STATUS"]);
            //columnGroupsView.ColumnGroups[0].Rows[0].Columns.Add(dtgCommLines.Columns["RECONNECT_NUMBER"]);
            //columnGroupsView.ColumnGroups[0].Rows[0].Columns.Add(dtgCommLines.Columns["MET_ID"]);
            //columnGroupsView.ColumnGroups[0].Rows[0].Columns.Add(dtgCommLines.Columns["MET_KEY"]);
            //columnGroupsView.ColumnGroups[0].Rows[0].Columns.Add(dtgCommLines.Columns["MADDO"]);
            //columnGroupsView.ColumnGroups[0].Rows[0].Columns.Add(dtgCommLines.Columns["TENDDO"]);
            //columnGroupsView.ColumnGroups[0].Rows[0].Columns.Add(dtgCommLines.Columns["DATALINK_OSN"]);
            //columnGroupsView.ColumnGroups[0].Rows[0].Columns.Add(dtgCommLines.Columns["PROG_PW"]);
            //columnGroupsView.ColumnGroups[0].Rows[0].Columns.Add(dtgCommLines.Columns["OUT_STATION_NUMBER"]);
            //columnGroupsView.ColumnGroups[0].Rows[0].Columns.Add(dtgCommLines.Columns["BAUD_RATE"]);
            //columnGroupsView.ColumnGroups[0].Rows[0].Columns.Add(dtgCommLines.Columns["REG_NUM"]);
            //columnGroupsView.ColumnGroups[0].Rows[0].Columns.Add(dtgCommLines.Columns["MET_TYPE"]);
            //dtgCommLines.Columns["TENDDO"].IsVisible = isPCVersion;
            //dtgCommLines.Columns["DATALINK_OSN"].IsVisible = isPCVersion;
        }

        public void UpdateGrid(DataTable dtSource)
        {
            if (!base.Dispatcher.CheckAccess())
            {
                base.Dispatcher.Invoke((MethodInvoker)delegate
                {
                    UpdateGrid(dtSource);
                });
            }
            else
            {
                dtgCommLines.ItemsSource = dtSource;
            }
        }

        public void BuildMediaAndTask(DataTable dtSource)
        {
            if (dtSource != null)
            {
                this.TaskBuffer.Clear();
                try
                {
                    this.dictMedia = new Dictionary<string, ESMedia>();
                    string[] columnNames = new string[] { "LINE_ID", "LINE_TYPE", "CONFIG" };
                    DataTable table = dtSource.DefaultView.ToTable(true, columnNames);
                    foreach (DataRow row in table.Rows)
                    {
                        string key = row["LINE_ID"].ToString();
                        string str2 = row["LINE_TYPE"].ToString();
                        string sConfig = row["CONFIG"].ToString();
                        ESMedia media = null;
                        string str4 = str2;
                        if (!(str4 == "GPRSModemLine"))
                        {
                            if (str4 == "IPPortLine")
                            {
                                goto Label_00E9;
                            }
                            if (str4 == "SerialPortLine")
                            {
                                goto Label_00F4;
                            }
                        }
                        else
                        {
                            media = new ESSocket(sConfig);
                        }
                        goto Label_00FF;
                    Label_00E9:
                        media = new ESNet(sConfig);
                        goto Label_00FF;
                    Label_00F4:
                        media = new ESSerial(sConfig);
                    Label_00FF:
                        media.LineConfig.LineID = key;
                        this.dictMedia.Add(key, media);
                        media.LineConfig.ConnectedChanged += new EventHandler(this.LineConfig_ConnectedChanged);
                        this.TaskBuffer.Add(key, new Queue<TaskInfo>());
                    }
                    this.timerTaskQueue.Start();
                }
                catch (Exception exception)
                {
                    this.AddLog("BuildMediaAndTask() Error: " + exception.Message);
                }
            }
        }

        private void LineConfig_ConnectedChanged(object sender, EventArgs e)
        {
            CommunicationLine line = (CommunicationLine)sender;
            string lineType = line.LineType;
            if (!(lineType == "IPPortLine"))
            {
                if (lineType == "GPRSModemLine")
                {
                    GPRSModemLine line3 = (GPRSModemLine)sender;
                    this.SetConnectionStatus(line3.LineID, line3.ConnectedStatus);
                }
                else if (lineType == "SerialPortLine")
                {
                    SerialPortLine line4 = (SerialPortLine)sender;
                    this.SetConnectionStatus(line4.LineID, line4.ConnectedStatus);
                }
            }
            else
            {
                IPPortLine line2 = (IPPortLine)sender;
                this.SetConnectionStatus(line2.LineID, line2.ConnectedStatus);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                StartServer(config.Port);
                SetControlStatus(0);
            }
            catch (Exception ex)
            {
                AddLog("Không khởi tạo được kết nối: " + ex.Message);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                timerSchedule.Stop();
                btnStartSchedule.Content = "CHẠY LỊCH TỰ ĐỘNG";
                StopServer();
                SetControlStatus(1);
            }
            catch (Exception ex)
            {
                AddLog("Không đóng được kết nối: " + ex.Message);
            }
        }

        public void SetControlStatus(int status)
        {
            //switch (status)
            //{
            //    case 0:
            //        btnStart.Enabled = false;
            //        btnStop.Enabled = true;
            //        mnuNotifyStart.Enabled = false;
            //        mnuNotifyStop.Enabled = true;
            //        break;
            //    case 1:
            //        btnStart.Enabled = true;
            //        btnStop.Enabled = false;
            //        mnuNotifyStart.Enabled = true;
            //        mnuNotifyStop.Enabled = false;
            //        break;
            //}
        }

        public void StartServer(int port)
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);
                serverSocket.Bind(ipEndPoint);
                serverSocket.Listen(100);
                serverSocket.BeginAccept(OnAccept, null);
            }
            catch (Exception ex)
            {
                AddLog("StartServer() Error: " + ex.Message);
            }
        }

        public void StopServer()
        {
            try
            {
                if (dictMedia == null || dictMedia.Count <= 0)
                {
                    return;
                }
                foreach (KeyValuePair<string, ESMedia> item in dictMedia)
                {
                    if (item.Value.LineConfig.LineType == "GPRSModemLine")
                    {
                        ESSocket socket = (ESSocket)item.Value;
                        socket.ReceivedData -= OnConnection_ReceivedData;
                        socket.Close();
                    }
                }
                serverSocket.Close();
                serverSocket = null;
                dictConnectedLine.Clear();
                for (int i = 0; i < dtConnectedLine.Rows.Count; i++)
                {
                    string LineID = dtConnectedLine.Rows[i]["LINE_ID"].ToString();
                    dtConnectedLine.Rows[i]["CONNECTION_STATUS"] = GetLineStatusFromEnum(EnumLineStatus.NOTCONNECTED);
                }
                dtgCommLines.ItemsSource = dtConnectedLine;
            }
            catch (Exception ex)
            {
                AddLog("StopServer() Error: " + ex.Message);
            }
        }


        private void OnAccept(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = serverSocket.EndAccept(ar);
                serverSocket.BeginAccept(OnAccept, null);
                clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, OnReceive, clientSocket);
            }
            catch (Exception)
            {
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = (Socket)ar.AsyncState;
                int leng = clientSocket.EndReceive(ar);
                AddLog(string.Format("{0:hh:mm:ss.fff} Hello packet: {1}", DateTime.Now, PacketParser.ByteToHex(byteData, leng)));
                if (leng > 4)
                {
                    string deviceID = PacketParser.ByteToHex(new byte[4]
					{
						byteData[3],
						byteData[2],
						byteData[1],
						byteData[0]
					}).Replace(" ", "");
                    string meterID = "";
                    int i = Array.IndexOf(byteData, (byte)0, 4);
                    if (i > 0)
                    {
                        meterID = Encoding.ASCII.GetString(byteData, 4, i - 4);
                    }
                    string dynamicIP = "";
                    dynamicIP = dynamicIP + Convert.ToInt32(byteData[leng - 7]) + ".";
                    dynamicIP = dynamicIP + Convert.ToInt32(byteData[leng - 6]) + ".";
                    dynamicIP = dynamicIP + Convert.ToInt32(byteData[leng - 5]) + ".";
                    dynamicIP += Convert.ToInt32(byteData[leng - 4]);
                    string CSQ = ((leng >= 23) ? Encoding.ASCII.GetString(byteData, leng - 2, 2) : "99");
                    int baudrate = GetBaudrateFromMeter(meterID);
                    AddMeter(deviceID, meterID, dynamicIP, CSQ, clientSocket, baudrate);
                }
            }
            catch (Exception)
            {
            }
        }

        public void AddMeter(string deviceid, string lineid, string dynamicIP, string CSQ, Socket client, int baudrate = 9600)
        {
            try
            {
                if (!base.Dispatcher.CheckAccess())
                {
                    base.Dispatcher.BeginInvoke((MethodInvoker)delegate
                    {
                        AddMeter(deviceid, lineid, dynamicIP, CSQ, client, baudrate);
                    });
                    return;
                }
                ESSocket newConnection = new ESSocket(client, lineid);
                newConnection.ReceivedData += OnConnection_ReceivedData;
                newConnection.ConnectedChanged += OnConnection_ConnectedChanged;
                if (dictConnectedLine.ContainsKey(lineid))
                {
                    ESSocket oldConnection = dictConnectedLine[lineid];
                    try
                    {
                        oldConnection.ClientSocket.Close();
                    }
                    catch
                    {
                    }
                    dictConnectedLine[lineid] = newConnection;
                    dictConnectedLine[lineid].Connected = false;
                }
                else
                {
                    dictConnectedLine.Add(lineid, newConnection);
                }
                for (int i = 0; i < dtConnectedLine.Rows.Count; i++)
                {
                    string LineID = dtConnectedLine.Rows[i]["LINE_ID"].ToString();
                    int LineReconnectNumber = int.Parse(dtConnectedLine.Rows[i]["RECONNECT_NUMBER"].ToString());
                    if (LineID == lineid)
                    {
                        LineReconnectNumber++;
                        dtConnectedLine.Rows[i]["CONNECTION_STATUS"] = GetLineStatusFromEnum(EnumLineStatus.CONNECTED);
                        dtConnectedLine.Rows[i]["RECONNECT_NUMBER"] = LineReconnectNumber;
                        dtgCommLines.ItemsSource = dtConnectedLine;
                        break;
                    }
                }
                AddLog(string.Format("{0:hh:mm:ss.fff}] Meter ID/Sim Card No: {1} - Device ID: {2} - Dynamic IP: {3} - CSQ: {4}", DateTime.Now, lineid, deviceid, 3, CSQ));
            }
            catch (Exception ex)
            {
                AddLog("AddMeter() error: " + ex.Message);
            }
        }

        private void OnConnection_ReceivedData(object sender, byte[] data, int leng)
        {
            AddLog(string.Format("{0:hh:mm:ss.fff} ReceivedDataHandler {1}: {2}", DateTime.Now, ((Socket)sender).RemoteEndPoint.ToString(), Encoding.ASCII.GetString(data, 0, leng)));
        }
        private void OnConnection_ConnectedChanged(object sender, EventArgs e)
        {
            try
            {
                ESSocket socket = (ESSocket)sender;
                SetConnectionStatus(socket.MeterId, socket.Connected ? EnumLineStatus.CONNECTED : EnumLineStatus.NOTCONNECTED);
            }
            catch
            {
            }
        }

        private void SetConnectionStatus(string lineid, EnumLineStatus linestatus)
        {
            if (!base.Dispatcher.CheckAccess())
            {
                dtgCommLines.Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    SetConnectionStatus(lineid, linestatus);
                });
                return;
            }
            else
            {
                for (int i = 0; i < this.dtConnectedLine.Rows.Count; i++)
                {
                    string str = this.dtConnectedLine.Rows[i]["LINE_ID"].ToString();
                    int num2 = int.Parse(this.dtConnectedLine.Rows[i]["RECONNECT_NUMBER"].ToString());
                    if (str == lineid)
                    {
                        this.dtConnectedLine.Rows[i]["CONNECTION_STATUS"] = this.GetLineStatusFromEnum(linestatus);
                        this.dtConnectedLine.AcceptChanges();
                        this.dtgCommLines.ItemsSource = this.dtConnectedLine;
                        break;
                    }
                }
            }
        }

        private void SetConnectionStatus(string lineid, EnumConnectedStatus lineStatus)
        {
            if (!base.Dispatcher.CheckAccess())
            {
                base.Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    SetConnectionStatus(lineid, lineStatus);
                });
                return;
            }
            else
            {
                for (int i = 0; i < this.dtConnectedLine.Rows.Count; i++)
                {
                    if (this.dtConnectedLine.Rows[i]["LINE_ID"].ToString() == lineid)
                    {
                        this.dtConnectedLine.Rows[i]["CONNECTION_STATUS"] = this.GetConnectionStatusFromEnum(lineStatus);
                        this.dtConnectedLine.AcceptChanges();
                    }
                }
                this.dtgCommLines.ItemsSource = this.dtConnectedLine;
            }
        }
        private string GetConnectionStatusFromEnum(EnumConnectedStatus status)
        {
            string str2;
            if (1 == 0)
            {
            }
            if (status != EnumConnectedStatus.Connected)
            {
                if (status == EnumConnectedStatus.Disconnected)
                {
                    str2 = "Ngắt kết nối";
                }
                else
                {
                    str2 = "Chưa kết nối";
                }
            }
            else
            {
                str2 = "Đã kết nối";
            }
            if (1 == 0)
            {
            }
            string str = str2;
            return str;
        }
        private string GetLineStatusFromEnum(EnumLineStatus status)
        {
            string str2;
            if (1 == 0)
            {
            }
            if (status != EnumLineStatus.CONNECTED)
            {
                if (status == EnumLineStatus.DISCONNECTED)
                {
                    str2 = "Ngắt kết nối";
                }
                else
                {
                    str2 = "Chưa kết nối";
                }
            }
            else
            {
                str2 = "Đã kết nối";
            }
            if (1 == 0)
            {
            }
            string str = str2;
            return str;
        }
        public int GetBaudrateFromMeter(string meterID)
        {
            int baudrate = 9600;
            if (dtConnectedLine != null && dtConnectedLine.Rows.Count > 0)
            {
                foreach (DataRow dr in dtConnectedLine.Rows)
                {
                    if (dr["MET_KEY"].ToString().Trim() == meterID)
                    {
                        baudrate = int.Parse(dr["BAUD_RATE"].ToString());
                        break;
                    }
                }
            }
            return baudrate;
        }
        public void OnSend(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndSend(ar);
            }
            catch (Exception)
            {
            }
        }
        private void timerTaskQueue_Tick(object sender, EventArgs e)
        {
            //lstTaskQueue.Items.Clear();
            //foreach (KeyValuePair<string, Queue<TaskInfo>> item in TaskBuffer)
            //{
            //    List<TaskInfo> lti = item.Value.ToList();
            //    foreach (TaskInfo ti in lti)
            //    {
            //        lstTaskQueue.Items.Add(string.Concat(ti.StartDate, " - ", item.Key, " - ", ti.Taskid, " - ", ti.Taskname, " - ", ti.MeterID));
            //    }
            //}
            foreach (KeyValuePair<string, Queue<TaskInfo>> item2 in TaskBuffer)
            {
                if (item2.Value.Count <= 0)
                {
                    continue;
                }
                TaskInfo s = item2.Value.First();
                if (dictMedia[item2.Key].LineConfig.WorkingStatus == EnumWorkingStatus.Working)
                {
                    continue;
                }
                if (dictUncompletedSchedule.ContainsKey(item2.Key))
                {
                    if (Math.Round(DateTime.Now.Subtract(dictUncompletedSchedule[item2.Key]).TotalSeconds) < (double)config.ScheduleDelay)
                    {
                        continue;
                    }
                    dictUncompletedSchedule.Remove(item2.Key);
                }
                item2.Value.Dequeue();
               
                RunTask(s, config.OpenFile);
                Thread.Sleep(500);
            }
        }
        public bool RunTask(TaskInfo para, bool bOpenExportFile = false)
        {
            lock (this)
            {
                string lineid = "";
                string linetype = "";
                GetLineInfo(para.MeterID, out lineid, out linetype);
                para.ExportFileCompany = lineid;
                if (!dictMedia.ContainsKey(lineid))
                {
                    SaveTryCatch("Lỗi 1: " + para.MeterID + "/" + lineid);
                    return false;
                }
                ESMedia eSMedia = dictMedia[lineid];
                ESMeterDevice eSMeterDevice = null;
                if (eSMedia.LineConfig.WorkingStatus == EnumWorkingStatus.Working)
                {
                    return false;
                }
                if (linetype != "GPRSModemLine")
                {
                    try
                    {
                        if (!eSMedia.Open())
                        {
                            SaveTryCatch("Lỗi 2: " + para.MeterID + "/" + lineid);
                            if (!dictUncompletedSchedule.ContainsKey(lineid))
                            {
                                dictUncompletedSchedule.Add(lineid, DateTime.Now);
                            }
                            else
                            {
                                dictUncompletedSchedule[lineid] = DateTime.Now;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        eSMedia.LineConfig.ConnectedStatus = EnumConnectedStatus.NotConnected;
                        eSMedia.LineConfig.WorkingStatus = EnumWorkingStatus.NotWorking;
                    }
                }
                if (para.DeviceType == "A1700")
                {
                    eSMeterDevice = new A1700Device();
                }
                else if (para.DeviceType == "LandisGyr")
                {
                    ESDLMSDevice eSDLMSDevice = new ESDLMSDevice(config.ShowCommunicationLog);
                    LoadDictEvent();
                    eSDLMSDevice.DictEvent = dictEvent;
                    eSDLMSDevice.ObisProfilePath = Path.Combine(System.Windows.Forms.Application.StartupPath, "LANDISANDGYR");
                    eSDLMSDevice.m_UseESMedia = true;
                    eSMeterDevice = eSDLMSDevice;
                }
                eSMeterDevice.TaskPara = para;
                if (!config.ShowCommunicationLog)
                {
                    eSMedia.Log = null;
                }
                else
                {
                    eSMedia.Log = new MediaLog();
                }
                eSMeterDevice.Media = eSMedia;
                eSMeterDevice.WorkingStatusChanged += device_WorkingStatusChanged;
                if (eSMedia.LineConfig.ConnectedStatus != EnumConnectedStatus.Connected)
                {
                    eSMeterDevice.WorkingStatus = EnumWorkingStatus.UnCompleted;
                    UpdateScheduleList(eSMeterDevice);
                    SaveTryCatch("Lỗi 3: " + para.MeterID + "/" + lineid);
                    return false;
                }
                if (config.ShowCommunicationLog)
                {
                    string name = "Device ID: " + para.MeterID;
                    System.Windows.Forms.RichTextBox rtbLog = new System.Windows.Forms.RichTextBox();
                    rtbLog.TextChanged += rtbLog_TextChanged;
                    eSMeterDevice.Media.Log.RTBLog = rtbLog;
                    //return true;
                }                
                Thread thread = new Thread(RunTask);
                thread.Start(eSMeterDevice);
                return true;
            }
        }

        private void LoadDictEvent()
        {
            dictEvent = new Dictionary<string, string>();
            List<string> keys = new List<string>();
            DataSet dataset = new DataSet();
            dataset.ReadXml(System.Windows.Forms.Application.StartupPath + "\\" + "LANDISANDGYR" + "\\" + "EventDictionary.gxc");
            DataTable dt= dataset.Tables[0];
            foreach (DataRow row in dt.Rows)
            {
                dictEvent.Add(row["EventKey"].ToString(), row["EventValue"].ToString());
            }
        }

        private void MainWindow_TaskCompleted(object sender, EventArgs e)
        {
            this.OnTaskCompleted(sender, e);
        }
        public virtual void OnTaskCompleted(object obj, EventArgs e)
        {
            if (this.TaskCompleted != null)
            {
                this.TaskCompleted(obj, e);
            }
        }
        private void device_WorkingStatusChanged(object sender, EventArgs e)
        {
            if (!base.Dispatcher.CheckAccess())
            {
                base.Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    device_WorkingStatusChanged(sender, e);
                });
                return;
            }
            else
            {
                string str2;
                string str3;
                ESMeterDevice device = (ESMeterDevice)sender;
                string meterID = device.TaskPara.MeterID;
                ESMedia media = device.Media;
                this.SetReadingStatus(meterID, media.LineConfig.WorkingStatus);
                bool flag = media.LineConfig.WorkingStatus == EnumWorkingStatus.Completed;
                this.GetLineInfo(device.TaskPara.MeterID, out str3, out str2);
                this.UpdateScheduleList(device);
                if ((str2 != "GPRSModemLine") && (media.LineConfig.WorkingStatus != EnumWorkingStatus.Working))
                {
                    device.WorkingStatusChanged -= new EventHandler(this.device_WorkingStatusChanged);
                    media.Close();
                }
            }
        }

        public void RunTask(object obj)
        {
            try
            {
                ESMeterDevice eSMeterDevice = (ESMeterDevice)obj;
                SetConnectionBusy(eSMeterDevice.Media, bBusy: true);
                string text = "";
                text = ((!config.IsPCVersion) ? ("Đọc số liệu " + eSMeterDevice.TaskPara.Taskname + " công tơ  " + eSMeterDevice.TaskPara.OutStationNumber) : ("Đọc số liệu " + eSMeterDevice.TaskPara.Taskname + " công tơ " + eSMeterDevice.TaskPara.MeterID));
                AddLog(text);
                try
                {
                    eSMeterDevice.RunTask();
                    if (eSMeterDevice.WorkingStatus == EnumWorkingStatus.Completed)
                    {
                        if (eSMeterDevice.TaskPara.isAuto)
                        {
                            eSMeterDevice.Export2CSV(config.ScheduleExportFilePath, config.ScheduleExportFilePath, config.ExportDataCompany, eSMeterDevice.TaskPara.isAuto);
                        }
                        else
                        {
                            eSMeterDevice.Export2CSV(config.ManualExportFilePath, config.ManualExportFilePath, config.ExportDataCompany, eSMeterDevice.TaskPara.isAuto);
                        }
                        
                        eSMeterDevice.Export2CSV(@"C:\Temp\STHM\DATA\CSV", @"C:\Temp\STHM\DATA\CSV", @"C:\Temp\STHM\DATA\CSV", true);
                     
                    }
                    string text2 = System.Windows.Forms.Application.StartupPath + "\\Log\\" + DateTime.Now.ToString("yyyy-MM") + "\\" + DateTime.Now.ToString("yyyy-MM-dd");
                    if (!Directory.Exists(text2))
                    {
                        Directory.CreateDirectory(text2);
                    }
                    string path = string.Format("{0}\\{1}_{2}_{3:yyyyMMdd_hhmmss}.log", text2, eSMeterDevice.TaskPara.MeterID, eSMeterDevice.TaskPara.Taskname, DateTime.Now);
                    File.WriteAllText(path, eSMeterDevice.Media.LogBuffer.ToString());
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (!dictUncompletedSchedule.ContainsKey(eSMeterDevice.Media.LineConfig.LineID))
                    {
                        dictUncompletedSchedule.Add(eSMeterDevice.Media.LineConfig.LineID, DateTime.Now);
                    }
                    else
                    {
                        dictUncompletedSchedule[eSMeterDevice.Media.LineConfig.LineID] = DateTime.Now;
                    }
                }
                SetConnectionBusy(eSMeterDevice.Media, bBusy: false);
                AddLog("Kết thúc");
            }
            catch (Exception ex)
            {
                common.ShowDialog(ex.Message);
            }
        }
        private void SetReadingStatus(string meterid, EnumWorkingStatus workingStatus)
        {
            if (!base.Dispatcher.CheckAccess())
            {
                base.Dispatcher.BeginInvoke((MethodInvoker)delegate
                {
                    SetReadingStatus(meterid, workingStatus);
                });
            }
            else
            {
                for (int i = 0; i < dtConnectedLine.Rows.Count; i++)
                {
                    string MeterID = dtConnectedLine.Rows[i]["MET_KEY"].ToString();
                    if (MeterID == meterid)
                    {
                        dtConnectedLine.Rows[i]["READING_STATUS"] = GetReadingStatusFromEnum(workingStatus);
                        dtConnectedLine.AcceptChanges();
                    }
                }
                dtgCommLines.ItemsSource = dtConnectedLine;
            }
        }
        private string GetReadingStatusFromEnum(EnumWorkingStatus status)
        {
            string str2;
            if (1 == 0)
            {
            }
            switch (status)
            {
                case EnumWorkingStatus.NotWorking:
                    str2 = "";
                    break;

                case EnumWorkingStatus.Completed:
                    str2 = "Kết thúc";
                    break;

                case EnumWorkingStatus.UnCompleted:
                    str2 = "Đọc lỗi";
                    break;

                default:
                    str2 = "Đang đọc";
                    break;
            }
            if (1 == 0)
            {
            }
            string str = str2;
            return str;
        }
        private void SaveTryCatch(string exeption)
        {
            string strLogfile = System.Windows.Forms.Application.StartupPath;
            strLogfile = Path.Combine(strLogfile, "Log_STHM.txt");
            if (File.Exists(strLogfile))
            {
                StreamWriter file2 = new StreamWriter(strLogfile, append: true);
                file2.WriteLine(exeption);
                file2.Close();
            }
            else
            {
                File.Create(strLogfile);
                StreamWriter file = new StreamWriter(strLogfile, append: true);
                file.WriteLine(exeption);
                file.Close();
            }
        }

        private void SetConnectionBusy(ESMedia Media, bool bBusy)
        {
            try
            {
                if (Media.GetType() == typeof(ESSocket))
                {
                    ESSocket socket = (ESSocket)Media;
                    socket.IsBusy = bBusy;
                }
            }
            catch (Exception)
            {
            }
        }

        public void UpdateScheduleList(ESMeterDevice device)
        {
            try
            {
                if (device.TaskPara.Taskid == 0)
                {
                    return;
                }
                string lineid = device.Media.LineConfig.LineID;
                DateTime RoundToNearestDate = new Common().RoundToNearest(DateTime.Now, new TimeSpan(0, dictScheduleList[device.TaskPara.Taskid].F_PERIOD_VALUE, 0));
                EnumWorkingStatus workstatus = device.Media.LineConfig.WorkingStatus;
                dictScheduleList[device.TaskPara.Taskid].F_READING_STATUS = (int)workstatus;
                switch (workstatus)
                {
                    case EnumWorkingStatus.Working:
                        break;
                    case EnumWorkingStatus.Completed:
                        dictScheduleList[device.TaskPara.Taskid].F_READING_COUNT = 0;
                        switch (device.TaskPara.Taskname)
                        {
                            case "ReadCurrentRegisterValues":
                                {
                                    DateTime dtimeupdate = dictScheduleList[device.TaskPara.Taskid].STARTDATE;
                                    Schedule scheduleupdate = dictScheduleList[device.TaskPara.Taskid];
                                    scheduleupdate.STARTDATE = dtimeupdate.AddDays(1.0);
                                    scheduleupdate.F_READING_COUNT = 0;
                                    scheduleupdate.F_READING_STATUS = (int)workstatus;
                                    dictScheduleList.Remove(device.TaskPara.Taskid);
                                    dictScheduleList.Add(device.TaskPara.Taskid, scheduleupdate);
                                    SaveScheduleFile(dsSchedule, dictScheduleList[device.TaskPara.Taskid].STARTDATE, (int)workstatus, dictScheduleList[device.TaskPara.Taskid]);
                                    SaveLogSchedule(EnumWorkingStatus.Completed, dictScheduleList[device.TaskPara.Taskid]);
                                    break;
                                }
                            case "ReadLoadProfile":
                                {
                                    DateTime NextTime = RoundToNearestDate.Add(new TimeSpan(0, dictScheduleList[device.TaskPara.Taskid].F_PERIOD_VALUE, 0));
                                    dictScheduleList[device.TaskPara.Taskid].STARTDATE = NextTime;
                                    SaveScheduleFile(dsSchedule, NextTime, (int)workstatus, dictScheduleList[device.TaskPara.Taskid]);
                                    SaveLogSchedule(EnumWorkingStatus.Completed, dictScheduleList[device.TaskPara.Taskid]);
                                    break;
                                }
                            case "ReadLoadProfile_A0":
                                dictScheduleList[device.TaskPara.Taskid].STARTDATE = DateTime.Now.Date.Add(new TimeSpan(1, dictScheduleList[device.TaskPara.Taskid].STARTDATE.Hour, dictScheduleList[device.TaskPara.Taskid].STARTDATE.Minute, dictScheduleList[device.TaskPara.Taskid].STARTDATE.Second));
                                SaveScheduleFile(dsSchedule, dictScheduleList[device.TaskPara.Taskid].STARTDATE, (int)workstatus, dictScheduleList[device.TaskPara.Taskid]);
                                SaveLogSchedule(EnumWorkingStatus.Completed, dictScheduleList[device.TaskPara.Taskid]);
                                break;
                            case "ReadHistoricalRegister":
                                {
                                    DateTime dtime = dictScheduleList[device.TaskPara.Taskid].STARTDATE.AddMonths(1).Date;
                                    dtime = new DateTime(dtime.Year, dtime.Month, 1, dtime.Hour, dtime.Minute, dtime.Second);
                                    dictScheduleList[device.TaskPara.Taskid].STARTDATE = dtime;
                                    SaveScheduleFile(dsSchedule, dictScheduleList[device.TaskPara.Taskid].STARTDATE, (int)workstatus, dictScheduleList[device.TaskPara.Taskid]);
                                    SaveLogSchedule(EnumWorkingStatus.Completed, dictScheduleList[device.TaskPara.Taskid]);
                                    break;
                                }
                        }
                        break;
                    case EnumWorkingStatus.UnCompleted:
                        dictScheduleList[device.TaskPara.Taskid].F_READING_COUNT++;
                        if (dictScheduleList[device.TaskPara.Taskid].F_READING_COUNT == 3)
                        {
                            dictScheduleList[device.TaskPara.Taskid].F_READING_COUNT = 0;
                            switch (device.TaskPara.Taskname)
                            {
                                case "ReadCurrentRegisterValues":
                                    dictScheduleList[device.TaskPara.Taskid].STARTDATE = dictScheduleList[device.TaskPara.Taskid].STARTDATE.Add(new TimeSpan(24, 0, 0));
                                    SaveScheduleFile(dsSchedule, dictScheduleList[device.TaskPara.Taskid].STARTDATE, (int)workstatus, dictScheduleList[device.TaskPara.Taskid]);
                                    SaveLogSchedule(EnumWorkingStatus.UnCompleted, dictScheduleList[device.TaskPara.Taskid]);
                                    break;
                                case "ReadLoadProfile":
                                    {
                                        DateTime NextTime2 = RoundToNearestDate.Add(new TimeSpan(0, dictScheduleList[device.TaskPara.Taskid].F_PERIOD_VALUE, 0));
                                        dictScheduleList[device.TaskPara.Taskid].STARTDATE = NextTime2;
                                        SaveScheduleFile(dsSchedule, NextTime2, (int)workstatus, dictScheduleList[device.TaskPara.Taskid]);
                                        SaveLogSchedule(EnumWorkingStatus.UnCompleted, dictScheduleList[device.TaskPara.Taskid]);
                                        break;
                                    }
                                case "ReadLoadProfile_A0":
                                    SaveLogSchedule(EnumWorkingStatus.UnCompleted, dictScheduleList[device.TaskPara.Taskid]);
                                    break;
                                case "ReadHistoricalRegister":
                                    dictScheduleList[device.TaskPara.Taskid].STARTDATE = dictScheduleList[device.TaskPara.Taskid].STARTDATE.AddMonths(1).Date;
                                    SaveScheduleFile(dsSchedule, dictScheduleList[device.TaskPara.Taskid].STARTDATE, (int)workstatus, dictScheduleList[device.TaskPara.Taskid]);
                                    SaveLogSchedule(EnumWorkingStatus.UnCompleted, dictScheduleList[device.TaskPara.Taskid]);
                                    break;
                            }
                        }
                        if (!dictUncompletedSchedule.ContainsKey(lineid))
                        {
                            dictUncompletedSchedule.Add(lineid, DateTime.Now);
                        }
                        else
                        {
                            dictUncompletedSchedule[lineid] = DateTime.Now;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                string messagetrycatch = DateTime.Now.ToString() + " UpdateScheduleList : " + ex.ToString();
                SaveTryCatch(messagetrycatch);
            }
        }

        private void SaveScheduleFile(DataSet ds, DateTime currentdate, int workstatus, Schedule s)
        {
            if (s.F_READING_STATUS == 1)
            {
                return;
            }
            try
            {
                //string scheduleFilename = System.Windows.Forms.Application.StartupPath + "\\Schedule.cfg";
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    int F_ID = int.Parse(ds.Tables[0].Rows[i]["F_ID"].ToString());
                    if (F_ID == s.F_ID)
                    {
                        ds.Tables[0].Rows[i]["STARTDATE"] = currentdate;
                        ds.Tables[0].Rows[i]["F_READING_STATUS"] = workstatus;
                        ds.Tables[0].Rows[i]["F_READING_COUNT"] = s.F_READING_COUNT;
                        using (var db = new HtsmContext())
                        {
                            Schedule update = db.Schedules.Find(s.F_ID);
                            if (update != null)
                            {
                                update.STARTDATE = currentdate;
                                update.F_READING_STATUS = workstatus;
                                update.F_READING_COUNT = s.F_READING_COUNT;
                                db.Entry(update).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChangesAsync();
                            }
                        }
                    }
                }
                lock (this)
                {
                    //dsSchedule.WriteXml(scheduleFilename);
                    Thread.Sleep(250);
                }
            }
            catch (Exception ex)
            {
                AddLog("SaveScheduleFile() Error: " + ex.Message);
            }
        }

        private void SaveLogSchedule(EnumWorkingStatus workstatus, Schedule s)
        {
            if (s.F_READING_STATUS == 1)
            {
                return;
            }
            string strNow = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            string logSchedulePath = System.Windows.Forms.Application.StartupPath + "\\Log Schedule\\" + DateTime.Now.ToString("yyyy-MM");
            string logScheduleName = logSchedulePath + "\\" + strNow;
            if (!Directory.Exists(logSchedulePath))
            {
                Directory.CreateDirectory(logSchedulePath);
            }
            try
            {
                string logContent = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]");
                logContent = logContent + " Người thao tác: " + config.UserLoggedIn;
                logContent = logContent + " Mã lịch: " + s.F_ID;
                logContent = logContent + ", Thông số: " + s.TASK_NAME;
                logContent = logContent + ", Serial công tơ: " + s.F_MET_KEY;
                logContent = logContent + ", Outstation number: " + s.OUT_STATION_NUMBER;
                logContent = logContent + ", Mã Đ.Đo: " + s.DATALINK_OSN;
                logContent = logContent + ", Tình trạng đọc: " + GetReadingStatusFromEnum(workstatus);
                logContent += "\r\n";
                lock (this)
                {
                    File.AppendAllText(logScheduleName, logContent);
                    AddLog(logContent);
                }
            }
            catch (Exception ex)
            {
                AddLog("SaveLogSchedule() Error: " + ex.Message);
            }
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            frm_ReadData fRead = new frm_ReadData();
            fRead.ShowDialog();
            if (!fRead.IsRun) return;
            if (fRead.Taskname.Equals("ReadLoadProfile_A0"))
            {
                TimeSpan time = new TimeSpan(0, 0, 30, 0);
                if (DateTime.Now.TimeOfDay <= time)
                {
                    if (fRead.StartFrom.Day >= DateTime.Now.AddDays(-1).Day)
                    {
                        fRead.StartFrom = DateTime.Now.AddDays(-2);
                    }
                }
                else
                {
                    if (fRead.StartFrom.Day > DateTime.Now.AddDays(-1).Day)
                    {
                        fRead.StartFrom = DateTime.Now.AddDays(-1);
                    }
                }
            }
            foreach (DataRowView selectedLine in dtgCommLines.SelectedItems)
            {
                string lineid = selectedLine["LINE_ID"].ToString();
                string linetype = selectedLine["LINE_TYPE"].ToString();
                TaskInfo para = new TaskInfo();
                para.isAuto = false;
                string devicetype = selectedLine["MET_TYPE"].ToString();
                string meterid = selectedLine["MET_KEY"].ToString();
                para.OutStationNumber = selectedLine["OUT_STATION_NUMBER"].ToString();
                para.Password = selectedLine["PROG_PW"].ToString();
                para.Taskname = "";
                para.MeterID = meterid;
                para.DeviceType = devicetype;
                para.DataLinkOSN = selectedLine["DATALINK_OSN"].ToString();
                para.ExportFileCompany = lineid;
                if (fRead.Taskname.Equals("ReadCurrentRegisterValues"))
                {
                    para.Taskname = "ReadCurrentRegisterValues";
                    para.StartDate = fRead.StartFrom;
                }
                else if (fRead.Taskname.Equals("ReadHistoricalRegister"))
                {
                    para.Taskname = "ReadHistoricalRegister";
                    para.StartDate = fRead.StartFrom;
                    para.SetNumber = fRead.udNumDays;
                }
                else if (fRead.Taskname.Equals("ReadLoadProfile"))
                {
                    para.Taskname = "ReadLoadProfile";
                    para.SetNumber = fRead.udNumDays;
                    para.StartDate = fRead.StartFrom;
                    para.IsFromDate = true;
                }
                else if (fRead.Taskname.Equals("ReadLoadProfile_A0"))
                {
                    para.Taskname = "ReadLoadProfile_A0";
                    para.SetNumber = fRead.udNumDays;
                    para.StartDate = fRead.StartFrom;
                    para.IsFromDate = true;
                }
                else if (fRead.Taskname.Equals("ReadInstrumentationProfile"))
                {
                    para.Taskname = "ReadInstrumentationProfile";
                    para.SetNumber = fRead.udNumDays;
                    para.StartDate = fRead.StartFrom;
                    para.IsFromDate = true;
                }
                //else if (rgrbEventLog.IsChecked)
                //{
                //    para.Taskname = "ReadEventLog";
                //    para.SetNumber = (int)udNumDays.Value;
                //    if (chkStartfrom.Checked)
                //    {
                //        para.StartDate = fRead.StartFrom;
                //        para.IsFromDate = true;
                //    }
                //    else
                //    {
                //        para.StartDate = DateTime.Now;
                //    }
                //}
                try {
                    if (TaskBuffer.ContainsKey(lineid))
                    {
                        TaskInfo[] TaskBufferItems = TaskBuffer[lineid].ToArray();
                        TaskBuffer[lineid].Clear();
                        TaskBuffer[lineid].Enqueue(para);
                        TaskInfo[] array = TaskBufferItems;
                        TaskInfo[] array2 = array;
                        TaskInfo[] array3 = array2;
                        foreach (TaskInfo item in array3)
                        {
                            TaskBuffer[lineid].Enqueue(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Thông báo!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }
            }
        }

        private void BtExportData_Click(object sender, RoutedEventArgs e)
        {
            string filepathExportData = config.ExportDataOldPath;
            if (!Directory.Exists(filepathExportData))
            {
                Directory.CreateDirectory(filepathExportData);
            }

            //frm_ReadData fRead = new frm_ReadData();
            //fRead.Title = "Xem Số Liệu";
            //fRead.btnRun.Content = "Xem số liệu";
            //fRead.ShowDialog();
            //if (!fRead.IsRun) return;

            //foreach (DataRowView indexselected in dtgCommLines.SelectedItems)
            //{
            //    string DatalinkOS = indexselected["DATALINK_OSN"].ToString();
            //    if (fRead.Taskname.Equals("ReadLoadProfile"))
            //    {
            //        //if (chkStartfrom.Checked)
            //        //{
            //        string str = "";
            //        string ngay = "";
            //        string thang2 = "";
            //        for (int i = 1; i <= fRead.udNumDays; i++)
            //        {
            //            str = fRead.StartFrom.AddDays(i - 1).Year.ToString().Substring(fRead.StartFrom.AddDays(i - 1).Year.ToString().Length - 1, 1);
            //            ngay = ((fRead.StartFrom.AddDays(i - 1).Day >= 10) ? fRead.StartFrom.AddDays(i - 1).Day.ToString() : ("0" + fRead.StartFrom.AddDays(i - 1).Day));
            //            thang2 = ((fRead.StartFrom.AddDays(i - 1).Month >= 10) ? fRead.StartFrom.AddDays(i - 1).Month.ToString() : ("0" + fRead.StartFrom.AddDays(i - 1).Month));
            //            string exportfilename2 = ngay + thang2 + str + DatalinkOS;
            //            if (File.Exists(System.Windows.Forms.Application.StartupPath + "\\DataBase\\" + exportfilename2 + ".sthm"))
            //            {
            //                File.Copy(System.Windows.Forms.Application.StartupPath + "\\DataBase\\" + exportfilename2 + ".sthm", filepathExportData + "\\" + exportfilename2 + config.formatfile, overwrite: true);
            //            }
            //        }
            //        continue;
            //    }
            //    else
            //    {
            //        if (!fRead.Taskname.Equals("ReadLoadProfile_A0"))
            //        {
            //            continue;
            //        }
            //        //if (chkStartfrom.Checked)
            //        //{
            //        string str4 = "";
            //        string ngay4 = "";
            //        string thang4 = "";
            //        for (int l = 1; l <= fRead.udNumDays; l++)
            //        {
            //            str4 = fRead.StartFrom.AddDays(l - 1).Year.ToString().Substring(fRead.StartFrom.AddDays(l - 1).Year.ToString().Length - 1, 1);
            //            ngay4 = ((fRead.StartFrom.AddDays(l - 1).Day >= 10) ? fRead.StartFrom.AddDays(l - 1).Day.ToString() : ("0" + fRead.StartFrom.AddDays(l - 1).Day));
            //            thang4 = ((fRead.StartFrom.AddDays(l - 1).Month >= 10) ? fRead.StartFrom.AddDays(l - 1).Month.ToString() : ("0" + fRead.StartFrom.AddDays(l - 1).Month));
            //            string exportfilename4 = ngay4 + thang4 + str4 + DatalinkOS;
            //            if (File.Exists(System.Windows.Forms.Application.StartupPath + "\\DataBase\\" + exportfilename4 + ".sthm"))
            //            {
            //                File.Copy(System.Windows.Forms.Application.StartupPath + "\\DataBase\\" + exportfilename4 + ".sthm", filepathExportData + "\\" + exportfilename4 + config.formatfile, overwrite: true);
            //            }
            //        }
            //        continue;
            //    }
            //}
            Process.Start("explorer.exe", filepathExportData);
        }

        private void btnExitApplication_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult d = (MessageBoxResult)common.ShowConfirm("Bạn chắc chắn muốn thoát hoàn toàn phần mềm không?");
            if (d == MessageBoxResult.Yes)
                System.Windows.Application.Current.Shutdown();
        }

        private void dtpickerStartfrom_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            ngaybieudo = DateTime.Parse(dtpickerStartfrom.EditValue.ToString());
        }

        private void LoadChar()
        {
            try
            {
                AddLog("Hiển thị biểu đồ!");

            }
            catch (Exception ex)
            {
                AddLog(ex.Message);
            }
        }

        private void dtgCommLines_SelectedItemChanged(object sender, DevExpress.Xpf.Grid.SelectedItemChangedEventArgs e)
        {

        }

        private static DataTable ConverArraytoDatatable_SL(Array conver, int columCount, int rowCount)
        {
            DataTable dtConver = new DataTable();
            try
            {
                //thêm tên cột
                for (int j = 1; j <= columCount; j++)
                {
                    if (j == 1)
                    {
                        dtConver.Columns.Add("Ngày", typeof(string));
                    }
                    else
                        if (j == 2)
                        {
                            dtConver.Columns.Add("Tên", typeof(string));
                        }
                        else
                        {
                            dtConver.Columns.Add((j - 2).ToString(), typeof(string));
                        }
                }
                for (int i = 1; i <= rowCount; i++)
                {
                    DataRow dr = dtConver.NewRow();
                    for (int j = 0; j < columCount; j++)
                    {
                        if (conver.GetValue(i, j + 1) != null)
                            dr[j] = conver.GetValue(i, j + 1).ToString();
                        else
                            dr[j] = "";
                    }
                    dtConver.Rows.Add(dr);
                    dtConver.AcceptChanges();

                }


                return dtConver;
            }
            catch
            {
                dtConver = null;
                return dtConver;
            }
        }
        private static DataTable ConverArraytoDatatable_CS(Array conver, int columCount, int rowCount, DateTime _ngaybieudo)
        {
            DataTable dtConver = new DataTable();
            try
            {
                //thêm tên cột
                for (int j = 1; j <= columCount; j++)
                {
                    if (j == 1)
                    {
                        dtConver.Columns.Add("PGiao", typeof(string));
                    }
                    else if (j == 2)
                    {
                        dtConver.Columns.Add("PNhan", typeof(string));
                    }
                    else if (j == 3)
                    {
                        dtConver.Columns.Add("QGiao", typeof(string));
                    }
                    else if (j == 4)
                    {
                        dtConver.Columns.Add("QNhan", typeof(string));
                    }
                    else if (j == 5)
                    {
                        dtConver.Columns.Add("X", typeof(string));
                    }
                }
                for (int i = 3; i < rowCount; i++)
                {
                    if (conver.GetValue(i, 1).ToString().Equals("E4"))
                    {
                        try
                        {
                            DateTime ngayDuLieu = DateTime.Parse(conver.GetValue(i, 2).ToString());
                        }
                        catch { continue; }
                        if (DateTime.Parse(conver.GetValue(i, 2).ToString()).Date == _ngaybieudo.Date)
                        {
                            for (int r = i + 1; r < rowCount; r++)
                            {
                                if (conver.GetValue(r, 1).ToString().Contains("E")) break;
                                DataRow dr = dtConver.NewRow();
                                for (int j = 0; j < columCount; j++)
                                {
                                    if (conver.GetValue(r, j + 1) != null)
                                        dr[j] = conver.GetValue(r, j + 1).ToString();
                                    else
                                        dr[j] = "";
                                }
                                dtConver.Rows.Add(dr);
                                dtConver.AcceptChanges();
                            }
                        }
                    }
                }
                return dtConver;
            }
            catch
            {
                dtConver = null;
                return dtConver;
            }
        }

        private void btnXem_Click(object sender, RoutedEventArgs e)
        {
            diagram1.Series.Clear();
            string filepathExportData = "C:\\Temp\\STHM\\DATA\\CSV";
            if (!Directory.Exists(filepathExportData))
            {
                Directory.CreateDirectory(filepathExportData);
            }
            string loaibieudo = cboTASKNAME.EditValue.ToString();
            foreach (DataRowView indexselected in dtgCommLines.SelectedItems)
            {
                string DatalinkOS = indexselected["DATALINK_OSN"].ToString();
                if (loaibieudo.Equals("ReadLoadProfile"))
                {
                    string path = filepathExportData + "\\" + indexselected["LINE_ID"] + "\\" + "PHU TAI CONG SUAT" + "\\" + ngaybieudo.ToString("yyyyMMdd")
                        + "\\" + indexselected["MET_KEY"];
                    if (!Directory.Exists(path))
                    {
                        return;
                    }
                    string[] files = System.IO.Directory.GetFiles(path, "*.csv");
                    foreach (string s in files)
                    {
                        System.IO.FileInfo fi = null;
                        try
                        {
                            fi = new System.IO.FileInfo(s);
                            string fName = fi.Name;
                            if (!fName.Contains(indexselected["MET_KEY"] + "_" + loaibieudo + "_" + DateTime.Now.ToString("yyyyMMdd")))
                            {
                                files = files.Where(value => value != s).ToArray();
                            }
                        }
                        catch (System.IO.FileNotFoundException ex)
                        {
                            Console.WriteLine(ex.Message);
                            continue;
                        }
                    }
                    if (files.Count() == 0) continue;
                    string smax = files[0];
                    for (int i = 1; i < files.Count(); i++)
                    {
                        System.IO.FileInfo fi = null;
                        try
                        {
                            fi = new System.IO.FileInfo(files[i]);
                            string fName = fi.Name;
                            if (!fName.Contains(indexselected["MET_KEY"] + "_" + loaibieudo + "_" + DateTime.Now.ToString("yyyyMMdd"))) continue;
                            if (String.Compare(smax, files[i], true) < 0)
                            {
                                smax = files[i];
                            }
                        }
                        catch (System.IO.FileNotFoundException ex)
                        {
                            Console.WriteLine(ex.Message);
                            continue;
                        }
                    }
                    // string filename = indexselected["MET_KEY"] + "_" + loaibieudo + "_" + DateTime.Now.ToString("yyyyMMdd") + "_" + DateTime.Now.TimeOfDay.ToString("hhmmss");
                    string filepath = smax;
                    LoadChart(filepath, indexselected["MET_KEY"].ToString());
                }
                else if (loaibieudo.Equals("ReadLoadProfile_A0"))
                {
                    string str = "";
                    string ngay = "";
                    string thang2 = "";
                    str = ngaybieudo.Year.ToString().Substring(ngaybieudo.Year.ToString().Length - 1, 1);
                    ngay = ((ngaybieudo.Day >= 10) ? ngaybieudo.Day.ToString() : ("0" + ngaybieudo.Day));
                    thang2 = ((ngaybieudo.Month >= 10) ? ngaybieudo.Month.ToString() : ("0" + ngaybieudo.Month));
                    string exportfilename2 = ngay + thang2 + str + DatalinkOS;
                    string filepath = filepathExportData + "\\" + exportfilename2 + config.formatfile;
                    LoadChart(filepath, indexselected["MET_KEY"].ToString());
                }
            }
        }
        private void LoadChart(string _path_excel, string macongto)
        {
            if (!File.Exists(_path_excel))
            {
                //System.Windows.MessageBox.Show("Không có dữ liệu.", "Thông báo!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            LoadChar();
            DT_SL_PGiao.Clear();
            DT_SL_PNhan.Clear();
            DT_SL_QGiao.Clear();
            DT_SL_QNhan.Clear();
            DT_CS_PGiao.Clear();
            DT_CS_PNhan.Clear();
            DT_CS_QGiao.Clear();
            DT_CS_QNhan.Clear();

            DataTable dtGiaodichct = new DataTable();
            Excel.Application xlApp = new Excel.Application();
            xlApp.Visible = true;
            try
            {
                xlApp.Width = 1;
                xlApp.Height = 1;
                xlApp.Top = 10000;
            }
            catch { }
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(_path_excel); ;
            xlApp.Visible = false;
            Excel.Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range ranger = xlWorksheet.UsedRange;
            try
            {
                string loaibieudo = cboTASKNAME.EditValue.ToString();
                if (loaibieudo.Equals("ReadLoadProfile_A0"))
                {
                    dtGiaodichct = ConverArraytoDatatable_SL(ranger.Value, ranger.Columns.Count, ranger.Rows.Count);
                    foreach (DataRow item in dtGiaodichct.Rows)
                    {
                        var itemArray = item.ItemArray;

                        if (itemArray[1].Equals("KwhGiao"))
                        {
                            for (int i = 2; i < itemArray.Count(); i++)
                            {
                                DataRow dr = DT_SL_PGiao.NewRow();
                                dr[0] = i - 1;
                                dr[1] = itemArray[i];
                                DT_SL_PGiao.Rows.Add(dr);
                                DT_SL_PGiao.AcceptChanges();
                            }
                        }
                        else if (item[1].Equals("KwhNhan"))
                        {
                            for (int i = 2; i < itemArray.Count(); i++)
                            {
                                DataRow dr = DT_SL_PNhan.NewRow();
                                dr[0] = i - 1;
                                dr[1] = itemArray[i];
                                DT_SL_PNhan.Rows.Add(dr);
                                DT_SL_PNhan.AcceptChanges();
                            }
                        }
                        else if (item[1].Equals("KvarhGiao"))
                        {
                            for (int i = 2; i < itemArray.Count(); i++)
                            {
                                DataRow dr = DT_SL_QGiao.NewRow();
                                dr[0] = i - 1;
                                dr[1] = itemArray[i];
                                DT_SL_QGiao.Rows.Add(dr);
                                DT_SL_QGiao.AcceptChanges();
                            }
                        }
                        else if (item[1].Equals("KvarhNhan"))
                        {
                            for (int i = 2; i < itemArray.Count(); i++)
                            {
                                DataRow dr = DT_SL_QNhan.NewRow();
                                dr[0] = i - 1;
                                dr[1] = itemArray[i];
                                DT_SL_QNhan.Rows.Add(dr);
                                DT_SL_QNhan.AcceptChanges();
                            }
                        }
                        else continue;
                    }
                    
                    LineSeries2D lineSeriesPG = new LineSeries2D();
                    diagram1.Series.Add(lineSeriesPG);
                    lineSeriesPG.Brush = new SolidColorBrush(Colors.Red);
                    lineSeriesPG.ArgumentDataMember = "CK";
                    lineSeriesPG.ValueDataMember = "W";
                    lineSeriesPG.MarkerVisible = true;
                    lineSeriesPG.MarkerSize = 8;
                    lineSeriesPG.Tag = "PG";
                    lineSeriesPG.ToolTipEnabled = true;
                    lineSeriesPG.ToolTipPointPattern = "Công tơ {S} - {A} : {V}";
                    lineSeriesPG.DisplayName = macongto;
                    lineSeriesPG.Visible = (bool)chPGiao.IsChecked;
                    lineSeriesPG.DataSource = DT_SL_PGiao;

                    LineSeries2D lineSeriesPN = new LineSeries2D();
                    diagram1.Series.Add(lineSeriesPN);
                    lineSeriesPN.Brush = new SolidColorBrush(Colors.Blue);
                    lineSeriesPN.ArgumentDataMember = "CK";
                    lineSeriesPN.ValueDataMember = "W";
                    lineSeriesPN.MarkerVisible = true;
                    lineSeriesPN.MarkerSize = 8;
                    lineSeriesPN.Tag = "PN";
                    lineSeriesPN.ToolTipEnabled = true;
                    lineSeriesPN.ToolTipPointPattern = "Công tơ {S} - {A} : {V}";
                    lineSeriesPN.DisplayName = macongto;
                    lineSeriesPN.Visible = (bool)chPNhan.IsChecked;
                    lineSeriesPN.DataSource = DT_SL_PNhan;

                    LineSeries2D lineSeriesQG = new LineSeries2D();
                    diagram1.Series.Add(lineSeriesQG);
                    lineSeriesQG.Brush = new SolidColorBrush(Colors.Orange);
                    lineSeriesQG.ArgumentDataMember = "CK";
                    lineSeriesQG.ValueDataMember = "W";
                    lineSeriesQG.MarkerVisible = true;
                    lineSeriesQG.MarkerSize = 8;
                    lineSeriesQG.Tag = "QG";
                    lineSeriesQG.ToolTipEnabled = true;
                    lineSeriesQG.ToolTipPointPattern = "Công tơ {S} - {A} : {V}";
                    lineSeriesQG.DisplayName = macongto;
                    lineSeriesQG.Visible = (bool)chQGiao.IsChecked;
                    lineSeriesQG.DataSource = DT_SL_QGiao;

                    LineSeries2D lineSeriesQN = new LineSeries2D();
                    diagram1.Series.Add(lineSeriesQN);
                    lineSeriesQN.Brush = new SolidColorBrush(Colors.Green);
                    lineSeriesQN.ArgumentDataMember = "CK";
                    lineSeriesQN.ValueDataMember = "W";
                    lineSeriesQN.MarkerVisible = true;
                    lineSeriesQN.MarkerSize = 8;
                    lineSeriesQN.Tag = "QN";
                    lineSeriesQN.ToolTipEnabled = true;
                    lineSeriesQN.ToolTipPointPattern = "Công tơ {S} - {A} : {V}";
                    lineSeriesQN.DisplayName = macongto;
                    lineSeriesQN.Visible = (bool)chQNhan.IsChecked;
                    lineSeriesQN.DataSource = DT_SL_QNhan;

                    if (diagram1.AxisX.VisualRange == null)
                        diagram1.AxisX.VisualRange = new Range();
                    diagram1.AxisX.VisualRange.SetAuto();
                    if (diagram1.AxisX.WholeRange == null)
                        diagram1.AxisX.WholeRange = new Range();
                    diagram1.AxisX.WholeRange.SetAuto();

                    if (diagram1.AxisY.VisualRange == null)
                        diagram1.AxisY.VisualRange = new Range();
                    diagram1.AxisY.VisualRange.SetAuto();
                    if (diagram1.AxisY.WholeRange == null)
                        diagram1.AxisY.WholeRange = new Range();
                    diagram1.AxisY.WholeRange.SetAuto(); 

                     
                }
                else if (loaibieudo.Equals("ReadLoadProfile"))
                {
                    dtGiaodichct = ConverArraytoDatatable_CS(ranger.Value, ranger.Columns.Count, ranger.Rows.Count, ngaybieudo);
                    int count = 1;
                    foreach (DataRow item in dtGiaodichct.Rows)
                    {
                        DataRow dr1 = DT_CS_PGiao.NewRow();
                        dr1[0] = count;
                        dr1[1] = item["PGiao"];
                        DT_CS_PGiao.Rows.Add(dr1);
                        DT_CS_PGiao.AcceptChanges();

                        DataRow dr2 = DT_CS_PNhan.NewRow();
                        dr2[0] = count;
                        dr2[1] = item["PNhan"];
                        DT_CS_PNhan.Rows.Add(dr2);
                        DT_CS_PNhan.AcceptChanges();

                        DataRow dr3 = DT_CS_QGiao.NewRow();
                        dr3[0] = count;
                        dr3[1] = item["QGiao"];
                        DT_CS_QGiao.Rows.Add(dr3);
                        DT_CS_QGiao.AcceptChanges();

                        DataRow dr4 = DT_CS_QNhan.NewRow();
                        dr4[0] = count;
                        dr4[1] = item["QNhan"];
                        DT_CS_QNhan.Rows.Add(dr4);
                        DT_CS_QNhan.AcceptChanges();

                        count++;
                    }
                    diagram1.ZoomOut(null);
                    LineSeries2D lineSeriesPG = new LineSeries2D();
                    diagram1.Series.Add(lineSeriesPG);
                    lineSeriesPG.Brush = new SolidColorBrush(Colors.Red);
                    lineSeriesPG.ArgumentDataMember = "CK";
                    lineSeriesPG.ValueDataMember = "W";
                    lineSeriesPG.MarkerVisible = true;
                    lineSeriesPG.MarkerSize = 8;
                    lineSeriesPG.Tag = "PG";
                    lineSeriesPG.ToolTipEnabled = true;
                    lineSeriesPG.ToolTipPointPattern = "Công tơ {S} - {A} : {V}";
                    lineSeriesPG.DisplayName = macongto;
                    lineSeriesPG.Visible = (bool)chPGiao.IsChecked;
                    lineSeriesPG.DataSource = DT_CS_PGiao;


                    LineSeries2D lineSeriesPN = new LineSeries2D();
                    diagram1.Series.Add(lineSeriesPN);
                    lineSeriesPN.Brush = new SolidColorBrush(Colors.Blue);
                    lineSeriesPN.ArgumentDataMember = "CK";
                    lineSeriesPN.ValueDataMember = "W";
                    lineSeriesPN.MarkerVisible = true;
                    lineSeriesPN.MarkerSize = 8;
                    lineSeriesPN.Tag = "PN";
                    lineSeriesPN.ToolTipEnabled = true;
                    lineSeriesPN.ToolTipPointPattern = "Công tơ {S} - {A} : {V}";
                    lineSeriesPN.DisplayName = macongto;
                    lineSeriesPN.Visible = (bool)chPNhan.IsChecked;
                    lineSeriesPN.DataSource = DT_CS_PNhan;

                    LineSeries2D lineSeriesQG = new LineSeries2D();
                    diagram1.Series.Add(lineSeriesQG);
                    lineSeriesQG.Brush = new SolidColorBrush(Colors.Orange);
                    lineSeriesQG.ArgumentDataMember = "CK";
                    lineSeriesQG.ValueDataMember = "W";
                    lineSeriesQG.MarkerVisible = true;
                    lineSeriesQG.MarkerSize = 8;
                    lineSeriesQG.Tag = "QG";
                    lineSeriesQG.ToolTipEnabled = true;
                    lineSeriesQG.ToolTipPointPattern = "Công tơ {S} - {A} : {V}";
                    lineSeriesQG.DisplayName = macongto;
                    lineSeriesQG.Visible = (bool)chQGiao.IsChecked;
                    lineSeriesQG.DataSource = DT_CS_QGiao;

                    LineSeries2D lineSeriesQN = new LineSeries2D();
                    diagram1.Series.Add(lineSeriesQN);
                    lineSeriesQN.Brush = new SolidColorBrush(Colors.Green);
                    lineSeriesQN.ArgumentDataMember = "CK";
                    lineSeriesQN.ValueDataMember = "W";
                    lineSeriesQN.MarkerVisible = true;
                    lineSeriesQN.MarkerSize = 8;
                    lineSeriesQN.Tag = "QN";
                    lineSeriesQN.ToolTipEnabled = true;
                    lineSeriesQN.ToolTipPointPattern = "Công tơ {S} - {A} : {V}";
                    lineSeriesQN.DisplayName = macongto;
                    lineSeriesQN.Visible = (bool)chQNhan.IsChecked;
                    lineSeriesQN.DataSource = DT_CS_QNhan;

                    if (diagram1.AxisX.VisualRange == null)
                        diagram1.AxisX.VisualRange = new Range();
                    diagram1.AxisX.VisualRange.SetAuto();
                    if (diagram1.AxisX.WholeRange == null)
                        diagram1.AxisX.WholeRange = new Range();
                    diagram1.AxisX.WholeRange.SetAuto();

                    if (diagram1.AxisY.VisualRange == null)
                        diagram1.AxisY.VisualRange = new Range();
                    diagram1.AxisY.VisualRange.SetAuto();
                    if (diagram1.AxisY.WholeRange == null)
                        diagram1.AxisY.WholeRange = new Range();
                    diagram1.AxisY.WholeRange.SetAuto(); 
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Thông báo!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                //xlWorksheet.Delete();
                xlWorkbook.Close(null, null, null);
                xlApp.Workbooks.Close();
                xlApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorksheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkbook);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(xlApp);
                xlWorksheet = null;
                xlWorkbook = null;
                GC.Collect();
            }
        }

        private void chPGiao_Checked(object sender, RoutedEventArgs e)
        {
            if (chart1.Diagram == null) return;
            foreach (LineSeries2D ls in chart1.Diagram.Series)
            {
                if (ls.Tag == "PG")
                {
                    ls.Visible = true;
                }
            }
        }

        private void chPGiao_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chart1.Diagram == null) return;
            foreach (LineSeries2D ls in chart1.Diagram.Series)
            {
                if (ls.Tag == "PG")
                {
                    ls.Visible = false;
                }
            }
        }

        private void chPNhan_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chart1.Diagram == null) return;
            foreach (LineSeries2D ls in chart1.Diagram.Series)
            {
                if (ls.Tag == "PN")
                {
                    ls.Visible = false;
                }
            }
        }

        private void chPNhan_Checked(object sender, RoutedEventArgs e)
        {
            if (chart1.Diagram == null) return;
            foreach (LineSeries2D ls in chart1.Diagram.Series)
            {
                if (ls.Tag == "PN")
                {
                    ls.Visible = true;
                }
            }
        }

        private void chQGiao_Checked(object sender, RoutedEventArgs e)
        {
            if (chart1.Diagram == null) return;
            foreach (LineSeries2D ls in chart1.Diagram.Series)
            {
                if (ls.Tag == "QG")
                {
                    ls.Visible = true;
                }
            }
        }

        private void chQGiao_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chart1.Diagram == null) return;
            foreach (LineSeries2D ls in chart1.Diagram.Series)
            {
                if (ls.Tag == "QG")
                {
                    ls.Visible = false;
                }
            }
        }

        private void chQNhan_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chart1.Diagram == null) return;
            foreach (LineSeries2D ls in chart1.Diagram.Series)
            {
                if (ls.Tag == "QN")
                {
                    ls.Visible = false;
                }
            }
        }

        private void chQNhan_Checked(object sender, RoutedEventArgs e)
        {
            if (chart1.Diagram == null) return;
            foreach (LineSeries2D ls in chart1.Diagram.Series)
            {
                if (ls.Tag == "QN")
                {
                    ls.Visible = true;
                }
            }
        }

        private void chart1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ChartHitInfo info = chart1.CalcHitInfo(e.GetPosition(chart1));
            if (info.InSeries)
            {
                LineSeries2D series = info.Series as LineSeries2D;
                if (series == highlightedSeries)
                    return;
                if (highlightedSeries != null)
                    highlightedSeries.LineStyle = new LineStyle() { Thickness = 3 };
                highlightedSeries = series;
                series.LineStyle = new LineStyle() { Thickness = 5 };
            }
            else
            {
                if (highlightedSeries != null)
                {
                    highlightedSeries.LineStyle = new LineStyle() { Thickness = 3 };
                    highlightedSeries = null;
                }
            }
        }

        private void cboTASKNAME_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            if (cboTASKNAME.EditValue == "ReadLoadProfile_A0")
            {
                dtpickerStartfrom.MaxValue = DateTime.Now.AddDays(-1);
                dtpickerStartfrom.EditValue = DateTime.Now.AddDays(-1);
            }
            else
            {
                dtpickerStartfrom.MaxValue = new DateTime(DateTime.Now.Year, 12, 31);
                dtpickerStartfrom.EditValue = DateTime.Now;
            }
        }

    }
}
