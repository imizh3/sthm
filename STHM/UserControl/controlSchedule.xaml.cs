using System;
using System.Collections.Generic;
using System.Data;
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
using STHM.LIB;

namespace STHM
{
    /// <summary>
    /// Interaction logic for controlLineMeter.xaml
    /// </summary>
    public partial class controlSchedule : UserControl
    {
        DataTable DT_Line;
        DataTable DT_Meter;
        DataTable dsSchedule;
        Common common = new Common();
        ActionType action = ActionType.Add;
        DataTable listDelete ;
        public bool IsChanged { get; set; }

        public controlSchedule()
        {
            InitializeComponent();
            cboCOMMLINE.SelectedIndexChanged += cboCOMMLINE_SelectedIndexChanged;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            //grThongtin.IsEnabled = false;
            EnableControl(false);
            this.action = ActionType.Load;
            IsChanged = false;
            listDelete = null;
            LoadTaskname();
            LoadCommLine();
            LoadSchedule();
            listDelete = dsSchedule.Clone();
        }

        private void EnableControl(bool state)
        {
            txtID.IsReadOnly = !state;
            chkENABLED.IsReadOnly = !state;
            cboTASKNAME.IsReadOnly = !state;
            dtpSTARTTIME.IsReadOnly = !state;
            dtpSTARTDATE.IsReadOnly = !state;
            udSETNUMBER.IsReadOnly = !state;
            txtEXCEPTFROM.IsReadOnly = !state;
            txtEXCEPTTO.IsReadOnly = !state;
            udPERIODVALUE.IsReadOnly = !state;
            cboCOMMLINE.IsReadOnly = !state;
            chkAllMeter.IsReadOnly = !state;
            cboMETER.IsReadOnly = !state;
            txtOUTSTATIONNUMBER.IsReadOnly = !state;
            txtDatalinkOSN.IsReadOnly = !state;
            txtPASSWORD.IsReadOnly = !state;
            txtMETTYPE.IsReadOnly = !state;
        }

        private void LoadData()
        {
            try
            {
                using (var db = new HtsmContext())
                {
                    DT_Line = db.Lines.OrderBy(d => d.LINE_ID).ToDataTable();
                    DT_Meter = db.Meters.OrderBy(d => d.MET_ID).Select(d => new MeterItem
                    {
                        BAUD_RATE = d.BAUD_RATE,
                        COMM_LINE = d.COMM_LINE,
                        DATALINK_OSN = d.DATALINK_OSN,
                        MADDO = d.MADDO,
                        MET_ID = d.MET_ID,
                        MET_KEY = d.MET_KEY,
                        MET_TYPE = d.MET_TYPE,
                        OUT_STATION_NUMBER = d.OUT_STATION_NUMBER,
                        PROG_PW = d.PROG_PW,
                        REG_NUM = d.REG_NUM,
                        T = d.T,
                        TU = d.TU,
                        TI = d.TI,
                        TENDDO = d.TENDDO,
                        Tenhienthi = d.MET_KEY + "-" + d.TENDDO

                    }).ToDataTable();
                }
            }
            catch { }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

            if (action == ActionType.Load)
            {
                //grThongtin.IsEnabled = true;
                EnableControl(true);
                dtgDanhsach.IsEnabled = false;
                btnAdd.Content = "Ghi";
                action = ActionType.Add;
                btnEdit.IsEnabled = false;
                btnDelete.IsEnabled = false;
                btnSave.IsEnabled = false;
                btnDiscard.IsEnabled = true;
                chkAllMeter.IsEnabled = true;
                ClearControlValue();
                cboMETER_SelectedIndexChanged(new object(), null);
                int maxid = 0;
                try
                {
                    if (dsSchedule.Rows.Count > 0)
                    {
                        maxid = dsSchedule.AsEnumerable().Max((DataRow a) => a.Field<int>("F_ID"));
                    }
                }
                catch
                {
                    maxid = 0;
                }
                txtID.Text = (maxid + 1).ToString();
                return;
            }
            try
            {
                if (!(bool)chkAllMeter.IsChecked)
                {
                    DataRow dr2 = dsSchedule.NewRow();
                    string fid = txtID.Text.Trim();
                    if (fid == "" || txtEXCEPTFROM.Text.Trim() == "" || txtEXCEPTTO.Text.Trim() == "" || cboCOMMLINE.Text.Trim() == "" || cboMETER.Text.Trim() == "")
                    {
                        common.ShowDialog("Phải nhập giá trị của tất cả các trường!");
                        return;
                    }
                    int duplicate = 0;
                    if (dsSchedule.Rows.Count > 0)
                    {
                        duplicate = dsSchedule.AsEnumerable().Count((DataRow a) => a.Field<int>("F_ID").ToString() == fid);
                    }
                    if (duplicate > 0)
                    {
                        txtID.SelectAll();
                        txtID.Focus();
                        common.ShowDialog("ID lịch bị trùng, hãy nhập ID khác!");
                        return;
                    }
                    dr2["F_ID"] = fid;
                    dr2["ENABLED"] = chkENABLED.IsChecked;
                    dr2["TASK_NAME"] = cboTASKNAME.EditValue;
                    dr2["STARTDATE"] = ReadStartDatetime(cboTASKNAME.EditValue.ToString());
                    dr2["SET_NUMBER"] = udSETNUMBER.EditValue;
                    dr2["F_READING_STATUS"] = 0;
                    dr2["F_PERIOD_VALUE"] = udPERIODVALUE.EditValue;
                    dr2["F_MET_KEY"] = cboMETER.EditValue;
                    dr2["LINE_ID"] = cboCOMMLINE.Text;
                    dr2["OUT_STATION_NUMBER"] = txtOUTSTATIONNUMBER.Text;
                    dr2["DATALINK_OSN"] = txtDatalinkOSN.Text;
                    dr2["PROG_PW"] = txtPASSWORD.Text;
                    dr2["MET_TYPE"] = txtMETTYPE.Text;
                    dr2["F_EXCEPTION"] = chkISEXCEPTION.IsChecked;
                    dr2["F_EXCEPT_FROM"] = txtEXCEPTFROM.Text;
                    dr2["F_EXCEPT_TO"] = txtEXCEPTTO.Text;
                    dr2["F_READING_COUNT"] = 0;
                    dr2["TASKNAME_DISPLAY"] = cboTASKNAME.Text;
                    dsSchedule.Rows.Add(dr2);
                    goto IL_09aa;
                }
                int maxid2 = 0;
                if (dsSchedule != null && dsSchedule.Rows.Count > 0)
                {
                    maxid2 = dsSchedule.AsEnumerable().Max((DataRow r) => r.Field<int>("F_ID"));
                }
                int nextid = maxid2 + 1;
                DataRow[] rows = DT_Meter.Select("COMM_LINE='" + cboCOMMLINE.Text + "'");
                DataRow[] array = rows;
                DataRow[] array2 = array;
                foreach (DataRow row in array2)
                {
                    DataRow dr = dsSchedule.NewRow();
                    dr["F_ID"] = nextid;
                    dr["ENABLED"] = chkENABLED.IsChecked;
                    dr["TASK_NAME"] = cboTASKNAME.EditValue.ToString();
                    dr["STARTDATE"] = ReadStartDatetime(cboTASKNAME.EditValue.ToString());
                    dr["SET_NUMBER"] = udSETNUMBER.EditValue;
                    dr["F_READING_STATUS"] = 0;
                    dr["F_PERIOD_VALUE"] = udPERIODVALUE.EditValue;
                    dr["F_MET_KEY"] = row["MET_KEY"].ToString();
                    dr["LINE_ID"] = row["COMM_LINE"].ToString();
                    dr["OUT_STATION_NUMBER"] = row["OUT_STATION_NUMBER"].ToString();
                    dr["DATALINK_OSN"] = row["DATALINK_OSN"].ToString();
                    dr["PROG_PW"] = row["PROG_PW"].ToString();
                    dr["MET_TYPE"] = row["MET_TYPE"].ToString();
                    dr["F_EXCEPTION"] = chkISEXCEPTION.IsChecked;
                    dr["F_EXCEPT_FROM"] = txtEXCEPTFROM.Text;
                    dr["F_EXCEPT_TO"] = txtEXCEPTTO.Text;
                    dr["F_READING_COUNT"] = 0;
                    dr["TASKNAME_DISPLAY"] = cboTASKNAME.Text;
                    dsSchedule.Rows.Add(dr);
                    nextid++;
                }
                goto IL_09aa;
            IL_09aa:
                dsSchedule.AcceptChanges();
                dtgDanhsach.ItemsSource = dsSchedule;
                btnAdd.Content = "Thêm";
                action = ActionType.Load;
                btnEdit.IsEnabled = true;
                btnDelete.IsEnabled = true;
                btnSave.IsEnabled = true;
                dtgDanhsach.IsEnabled = true;
                //grThongtin.IsEnabled = false;
                EnableControl(false);
                btnDiscard.IsEnabled = false;
                chkAllMeter.IsEnabled = false;
            }
            catch (Exception)
            {
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (action == ActionType.Load)
            {
                //grThongtin.IsEnabled = true;
                EnableControl(true);
                dtgDanhsach.IsEnabled = false;
                btnEdit.Content = "Ghi";
                action = ActionType.Edit;
                btnAdd.IsEnabled = false;
                btnDelete.IsEnabled = false;
                btnSave.IsEnabled = false;
                btnDiscard.IsEnabled = true;
                txtID.IsReadOnly = true;
                return;
            }
            try
            {
                DataRowView rv = (DataRowView)dtgDanhsach.SelectedItem;
                string fid = txtID.Text.Trim();
                DataRow[] dr = dsSchedule.Select("F_ID='" + fid + "'");
                dr[0]["ENABLED"] = chkENABLED.IsChecked;
                dr[0]["TASK_NAME"] = cboTASKNAME.EditValue;
                dr[0]["STARTDATE"] = dtpSTARTDATE.EditValue;
                dr[0]["SET_NUMBER"] = udSETNUMBER.EditValue;
                dr[0]["F_PERIOD_VALUE"] = udPERIODVALUE.EditValue;
                dr[0]["F_MET_KEY"] = cboMETER.EditValue;
                dr[0]["LINE_ID"] = cboCOMMLINE.Text;
                dr[0]["OUT_STATION_NUMBER"] = txtOUTSTATIONNUMBER.Text;
                dr[0]["DATALINK_OSN"] = txtDatalinkOSN.Text;
                dr[0]["PROG_PW"] = txtPASSWORD.Text;
                dr[0]["MET_TYPE"] = txtMETTYPE.Text;
                dr[0]["F_EXCEPTION"] = chkISEXCEPTION.IsChecked;
                dr[0]["F_EXCEPT_FROM"] = txtEXCEPTFROM.Text;
                dr[0]["F_EXCEPT_TO"] = txtEXCEPTTO.Text;
                dr[0]["TASKNAME_DISPLAY"] = cboTASKNAME.Text;
                dsSchedule.AcceptChanges();
                dtgDanhsach.ItemsSource = dsSchedule;
                btnEdit.Content = "Sửa";
                action = ActionType.Load;
                btnAdd.IsEnabled = true;
                btnDelete.IsEnabled = true;
                btnSave.IsEnabled = true;
                dtgDanhsach.IsEnabled = true;
                //grThongtin.IsEnabled = false;
                EnableControl(false);
                btnDiscard.IsEnabled = false;
                txtID.IsReadOnly = false;
            }
            catch
            {
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                foreach (DataRowView info in this.dtgDanhsach.SelectedItems)
                {
                    string str = info["F_ID"].ToString();
                    DataRow[] rowArray = this.dsSchedule.Select("F_ID='" + str + "'");
                    DataRow delete = listDelete.NewRow();
                    delete.ItemArray = rowArray[0].ItemArray;
                    listDelete.Rows.Add(delete);
                    this.dsSchedule.Rows.Remove(rowArray[0]);
                    IsChanged = true;
                }
                this.dsSchedule.AcceptChanges();
                this.dtgDanhsach.ItemsSource = this.dsSchedule;
                this.IsEmptySchedule();
            }
            catch
            {
            }
        }

        private void LoadTaskname()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("ReadCurrentRegisterValues", "Thông số công tơ");
            data.Add("ReadHistoricalRegister", "Chỉ số chốt");
            data.Add("ReadLoadProfile", "Phụ tải công suất");
            data.Add("ReadLoadProfile_A0", "Phụ tải sản lượng");
            data.Add("ReadInstrumentationProfile", "Thông số lịch sử");
            cboTASKNAME.ItemsSource = data;
            cboTASKNAME.DisplayMember = "Value";
            cboTASKNAME.ValueMember = "Key";
            cboTASKNAME.SelectedIndex = 0;
        }

        private void LoadCommLine()
        {
            try
            {
                cboCOMMLINE.ItemsSource = DT_Line;
                cboCOMMLINE.ValueMember = "LINE_ID";
                cboCOMMLINE.DisplayMember = "LINE_ID";
                cboCOMMLINE.SelectedIndex = 0;
                LoadMeter(cboCOMMLINE.EditValue != null ? cboCOMMLINE.EditValue.ToString() : "");
            }
            catch
            {
                return;
            }
            cboCOMMLINE.SelectedIndex = 0;
            LoadMeter(cboCOMMLINE.EditValue != null ? cboCOMMLINE.EditValue.ToString() : "");
        }

        private void LoadMeter(string linename)
        {
            cboMETER.SelectedIndexChanged -= cboMETER_SelectedIndexChanged;
            DataTable dtMeter = DT_Meter;
            dtMeter.DefaultView.RowFilter = "COMM_LINE='" + linename + "'";
            cboMETER.Items.Clear();
            cboMETER.ItemsSource = dtMeter;
            cboMETER.ValueMember = "MET_KEY";
            cboMETER.DisplayMember = "Tenhienthi";
            cboMETER.SelectedIndexChanged += cboMETER_SelectedIndexChanged;
            cboMETER.SelectedIndex = 0;
        }

        private void cboMETER_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            if (cboMETER.EditValue != null)
            {
                DataRow[] rowArray = this.DT_Meter.Select("MET_KEY='" + this.cboMETER.EditValue.ToString() + "'");
                if (rowArray.Length > 0)
                {
                    this.txtOUTSTATIONNUMBER.Text = rowArray[0]["OUT_STATION_NUMBER"].ToString();
                    this.txtPASSWORD.Text = rowArray[0]["PROG_PW"].ToString();
                    this.txtMETTYPE.Text = rowArray[0]["MET_TYPE"].ToString();
                    this.txtDatalinkOSN.Text = rowArray[0]["DATALINK_OSN"].ToString();
                }
            }
        }

        void cboCOMMLINE_SelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            if (this.cboCOMMLINE.EditValue != null)
            {
                this.LoadMeter(this.cboCOMMLINE.EditValue.ToString());
            }
        }

        private void LoadSchedule()
        {
            using (var db = new HtsmContext())
            {
                dsSchedule = db.Schedules.ToDataTable();
            }
            if (dsSchedule != null)
            {
                dsSchedule.Columns.Add("TASKNAME_DISPLAY", typeof(string));
                for (int i = 0; i < dsSchedule.Rows.Count; i++)
                {
                    if (dsSchedule.Rows[i]["TASK_NAME"].ToString() == "ReadCurrentRegisterValues")
                    {
                        dsSchedule.Rows[i]["TASKNAME_DISPLAY"] = "Thông số công tơ";
                    }
                    if (dsSchedule.Rows[i]["TASK_NAME"].ToString() == "ReadHistoricalRegister")
                    {
                        dsSchedule.Rows[i]["TASKNAME_DISPLAY"] = "Chỉ số chốt";
                    }
                    if (dsSchedule.Rows[i]["TASK_NAME"].ToString() == "ReadLoadProfile")
                    {
                        dsSchedule.Rows[i]["TASKNAME_DISPLAY"] = "Phụ tải công suất";
                    }
                    if (dsSchedule.Rows[i]["TASK_NAME"].ToString() == "ReadLoadProfile_A0")
                    {
                        dsSchedule.Rows[i]["TASKNAME_DISPLAY"] = "Phụ tải sản lượng";
                    }
                    if (dsSchedule.Rows[i]["TASK_NAME"].ToString() == "ReadInstrumentationProfile")
                    {
                        dsSchedule.Rows[i]["TASKNAME_DISPLAY"] = "Thông số lịch sử";
                    }
                }
                dtgDanhsach.ItemsSource = dsSchedule;
            }
            IsEmptySchedule();
        }

        private void IsEmptySchedule()
        {
            try
            {
                if (dsSchedule.Rows.Count <= 0)
                {
                    btnEdit.IsEnabled = false;
                    btnDelete.IsEnabled = false;
                }
            }
            catch (Exception)
            {
                btnEdit.IsEnabled = false;
                btnDelete.IsEnabled = false;
            }
        }

        private void ClearControlValue()
        {
            txtID.Text = "";
            chkENABLED.IsChecked = true;
            cboTASKNAME.SelectedIndex = 0;
            dtpSTARTDATE.EditValue = DateTime.Now;
            dtpSTARTTIME.EditValue = DateTime.Now;
            udSETNUMBER.EditValue = 1;
            udPERIODVALUE.EditValue = 30;
            cboCOMMLINE.SelectedIndex = 0;
            chkISEXCEPTION.IsChecked = false;
            txtEXCEPTFROM.Text = "00:00";
            txtEXCEPTTO.Text = "07:00";
            txtDatalinkOSN.Text = "001";
        }
        private DateTime ReadStartDatetime(string tasknamevalue)
        {
            DateTime startdate = DateTime.Now;
            DateTime dtpk = (DateTime)dtpSTARTDATE.EditValue;
            int days = (int)(((DateTime)dtpSTARTTIME.EditValue).Date - startdate.Date).TotalDays;
            dtpk = new DateTime(startdate.Year, startdate.Month, startdate.Day, dtpk.Hour, dtpk.Minute, dtpk.Second);
            switch (tasknamevalue)
            {
                case "ReadLoadProfile":
                    startdate = dtpk;
                    break;
                case "ReadCurrentRegisterValues":
                    startdate = dtpk;
                    startdate = startdate.AddDays(days);
                    break;
                case "ReadHistoricalRegister":
                    startdate = dtpk;
                    startdate = startdate.AddDays(days);
                    break;
                case "ReadLoadProfile_A0":
                    startdate = dtpk;
                    startdate = startdate.AddDays(days);
                    break;
            }
            return startdate;
        }

        private void dtgDanhsach_CurrentItemChanged(object sender, DevExpress.Xpf.Grid.CurrentItemChangedEventArgs e)
        {

        }

        private void dtgDanhsach_SelectedItemChanged(object sender, DevExpress.Xpf.Grid.SelectedItemChangedEventArgs e)
        {
            try
            {
                DataRowView currentRow = (DataRowView)dtgDanhsach.SelectedItem;
                this.txtID.Text = currentRow["F_ID"].ToString();
                this.chkENABLED.IsChecked = bool.Parse(currentRow["ENABLED"].ToString());
                this.cboTASKNAME.EditValue = currentRow["TASK_NAME"].ToString();
                this.dtpSTARTDATE.EditValue = new DateTime?(DateTime.Parse(currentRow["STARTDATE"].ToString()));
                this.udSETNUMBER.EditValue = decimal.Parse(currentRow["SET_NUMBER"].ToString());
                this.udPERIODVALUE.EditValue = decimal.Parse(currentRow["F_PERIOD_VALUE"].ToString());
                this.cboMETER.EditValue = currentRow["F_MET_KEY"].ToString();
                this.cboCOMMLINE.EditValue = currentRow["LINE_ID"].ToString();
                this.txtOUTSTATIONNUMBER.Text = currentRow["OUT_STATION_NUMBER"].ToString();
                this.txtDatalinkOSN.Text = currentRow["DATALINK_OSN"].ToString();
                this.txtPASSWORD.Text = currentRow["PROG_PW"].ToString();
                this.txtMETTYPE.Text = currentRow["MET_TYPE"].ToString();
                this.chkISEXCEPTION.IsChecked = bool.Parse(currentRow["F_EXCEPTION"].ToString());
                this.txtEXCEPTFROM.EditValue = currentRow["F_EXCEPT_FROM"].ToString();
                this.txtEXCEPTTO.EditValue = currentRow["F_EXCEPT_TO"].ToString();
            }
            catch
            {
                this.ClearControlValue();
            }
        }

        private void btnDiscard_Click(object sender, RoutedEventArgs e)
        {
            //grThongtin.IsEnabled = false;
            EnableControl(false);
            btnAdd.IsEnabled = true;
            btnEdit.IsEnabled = true;
            btnDelete.IsEnabled = true;
            btnSave.IsEnabled = true;
            dtgDanhsach.IsEnabled = true;
            btnDiscard.IsEnabled = false;
            action = ActionType.Load;
            btnAdd.Content = "Thêm";
            btnEdit.Content = "Sửa";
            txtID.IsReadOnly = false;
            IsEmptySchedule();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dssave = dsSchedule.Copy();
                foreach (DataRow rUpdate in dssave.Rows)
                {
                    using (var db = new HtsmContext())
                    {
                        int fid = (int)rUpdate["F_ID"];
                        Schedule add = db.Schedules.Find(fid);
                        if (add != null)
                        {
                            add.F_ID = (int)rUpdate["F_ID"];
                            add.ENABLED = (bool)rUpdate["ENABLED"];
                            add.TASK_NAME = (string)rUpdate["TASK_NAME"];
                            add.STARTDATE = (DateTime)rUpdate["STARTDATE"];
                            add.SET_NUMBER = (int)rUpdate["SET_NUMBER"];
                            add.F_READING_STATUS = (int)rUpdate["F_READING_STATUS"];
                            add.F_PERIOD_VALUE = (int)rUpdate["F_PERIOD_VALUE"];
                            add.F_MET_KEY = (string)rUpdate["F_MET_KEY"];
                            add.LINE_ID = (string)rUpdate["LINE_ID"];
                            add.OUT_STATION_NUMBER = (string)rUpdate["OUT_STATION_NUMBER"];
                            add.DATALINK_OSN = (string)rUpdate["DATALINK_OSN"];
                            add.PROG_PW = (string)rUpdate["PROG_PW"];
                            add.MET_TYPE = (string)rUpdate["MET_TYPE"];
                            add.F_EXCEPTION = (bool)rUpdate["F_EXCEPTION"];
                            add.F_EXCEPT_FROM = (DateTime)rUpdate["F_EXCEPT_FROM"];
                            add.F_EXCEPT_TO = (DateTime)rUpdate["F_EXCEPT_TO"];
                            add.F_READING_COUNT = (int)rUpdate["F_READING_COUNT"];
                            db.Entry(add).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            add = new Schedule();
                            add.F_ID = (int)rUpdate["F_ID"];
                            add.ENABLED = (bool)rUpdate["ENABLED"];
                            add.TASK_NAME = (string)rUpdate["TASK_NAME"];
                            add.STARTDATE = (DateTime)rUpdate["STARTDATE"];
                            add.SET_NUMBER = (int)rUpdate["SET_NUMBER"];
                            add.F_READING_STATUS = (int)rUpdate["F_READING_STATUS"];
                            add.F_PERIOD_VALUE = (int)rUpdate["F_PERIOD_VALUE"];
                            add.F_MET_KEY = (string)rUpdate["F_MET_KEY"];
                            add.LINE_ID = (string)rUpdate["LINE_ID"];
                            add.OUT_STATION_NUMBER = (string)rUpdate["OUT_STATION_NUMBER"];
                            add.DATALINK_OSN = (string)rUpdate["DATALINK_OSN"];
                            add.PROG_PW = (string)rUpdate["PROG_PW"];
                            add.MET_TYPE = (string)rUpdate["MET_TYPE"];
                            add.F_EXCEPTION = (bool)rUpdate["F_EXCEPTION"];
                            add.F_EXCEPT_FROM = (DateTime)rUpdate["F_EXCEPT_FROM"];
                            add.F_EXCEPT_TO = (DateTime)rUpdate["F_EXCEPT_TO"];
                            add.F_READING_COUNT = (int)rUpdate["F_READING_COUNT"];
                            db.Schedules.Add(add);
                            db.SaveChanges();
                        }
                    }
                }
                foreach (DataRow row in listDelete.Rows)
                {
                    using (var db = new HtsmContext())
                    {
                        try
                        {
                            int str = (int)row["F_ID"];
                            Schedule delete = db.Schedules.Find(str);
                            if (delete != null)
                            {
                                db.Schedules.Remove(delete);
                                db.SaveChanges();
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                listDelete = null;
                listDelete = dsSchedule.Clone();
                IsChanged = true;
                common.ShowDialog("Lưu thông tin lịch thành công!");
            }
            catch (Exception ex)
            {
                common.ShowDialog("Không lưu được thông tin lịch: " + ex.Message);
            }
        }
    }

    public class MeterItem
    {
        public string Tenhienthi
        {
            get;
            set;
        }
        public void ToDataRow(DataRow r)
        {
            try
            {
                List<MeterItem> listMeter = new List<MeterItem>();
                listMeter.Add(this);
                r.ItemArray = listMeter.ToDataTable().Rows[0].ItemArray;
            }
            catch
            {

            }
        }

        public int BAUD_RATE { get; set; }

        public string COMM_LINE { get; set; }

        public string DATALINK_OSN { get; set; }

        public string MADDO { get; set; }

        public string MET_ID { get; set; }

        public string MET_KEY { get; set; }

        public string MET_TYPE { get; set; }

        public string OUT_STATION_NUMBER { get; set; }

        public string PROG_PW { get; set; }

        public int REG_NUM { get; set; }

        public int T { get; set; }

        public string TENDDO { get; set; }

        public int TI { get; set; }

        public int TU { get; set; }
    }
}
