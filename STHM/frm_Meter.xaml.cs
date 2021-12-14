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
using System.Windows.Shapes;
using STHM.Data;
using STHM.LIB;
using System.Data;
namespace STHM
{
    /// <summary>
    /// Interaction logic for frmMeter.xaml
    /// </summary>
    public partial class frm_Meter : Window
    {
        private ActionType action;
        private DataTable source;
        private Meter meter;
        private Common common;
        private List<Lines> Lines;
        public bool IsChanged { get; set; }
        public frm_Meter(ActionType action,Meter meter, DataTable source)
        {
            InitializeComponent();
            this.action = action;
            this.meter = meter;
            this.source = source;
            this.Loaded += frm_Meter_Loaded;
            this.common = new Common();
            using (var db = new HtsmContext())
            {
                this.Lines = db.Lines.ToList();
            }
        }

        void frm_Meter_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetBaudrateValues(this.action);
            this.SetMetType(this.action);
            this.SetLineList(this.action);
            if (this.action == ActionType.Edit)
            {
                this.txtMetID.IsReadOnly = true;
                this.txtMetID.Text = this.meter.MET_ID;
                this.txtMetKey.Text = this.meter.MET_KEY;
                this.txtMaDDO.Text = this.meter.DATALINK_OSN;
                this.txtTenDDO.Text = this.meter.TENDDO;
                this.txtProgPW.Text = this.meter.PROG_PW;
                this.txtOutstation.Text = this.meter.OUT_STATION_NUMBER;
                this.cboBaudrate.EditValue = this.meter.BAUD_RATE;
                this.cboMetType.EditValue = this.meter.MET_TYPE;
                this.udTU.EditValue = this.meter.TU;
                this.cboLine.EditValue = this.meter.COMM_LINE;
            }
            else if (this.action == ActionType.Add)
            {
                this.txtMetID.Text = PublicValue.GET_MATUDONG_INT(this.GetType().Name, this.GetType().Name, false).ToString();
                this.txtProgPW.Text = "M_KH_DOC";
                this.txtMaDDO.Text = "0";
                this.udTU.EditValue = 1;
            }
            IsChanged = false;
            this.txtMetKey.Focus();

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new HtsmContext())
                {
                    if (((((this.txtMetID.Text.Trim() == "") || (this.txtMetKey.Text.Trim() == "")) || ((this.txtMaDDO.Text.Trim() == "") || (this.txtTenDDO.Text.Trim() == ""))) || ((this.txtProgPW.Text.Trim() == ""))) || (this.txtOutstation.Text == ""))
                    {
                        this.common.ShowDialog("Phải nhập hết tất cả các giá trị!");
                        this.txtMetID.Focus();
                    }
                    else
                    {
                        if (this.action == ActionType.Add)
                        {
                            if (this.txtMetID.Text.Trim() == PublicValue.GET_MATUDONG_INT(this.GetType().Name, this.GetType().Name, false).ToString())
                                this.txtMetID.Text = PublicValue.GET_MATUDONG_INT(this.GetType().Name, this.GetType().Name, true).ToString();
                            string metid = this.txtMetID.Text.Trim();
                            string metkey = this.txtMetKey.Text.Trim();
                            if (db.Meters.Find(metid) != null)
                            {
                                this.common.ShowDialog("ID công tơ bị trùng, Hãy nhập ID khác!");
                                this.txtMetID.Text = PublicValue.GET_MATUDONG_INT(this.GetType().Name, this.GetType().Name, false).ToString();
                                this.txtMetKey.Focus();
                                return;
                            }
                            if (db.Meters.FirstOrDefault(d=>d.MET_KEY==metkey) != null)
                            {
                                this.common.ShowDialog("Mã công tơ bị trùng, hãy nhập mã khác!");
                                this.txtMetKey.SelectAll();
                                this.txtMetKey.Focus();
                                return;
                            }

                            this.meter = new Meter();
                            this.meter.MET_ID = this.txtMetID.Text.Trim();
                            this.meter.MET_KEY = this.txtMetKey.Text.Trim();
                            this.meter.MADDO = this.txtMaDDO.Text.Trim();
                            this.meter.TENDDO = this.txtTenDDO.Text.Trim();
                            this.meter.DATALINK_OSN = this.txtMaDDO.Text.Trim();
                            this.meter.PROG_PW = this.txtProgPW.Text.Trim();
                            this.meter.OUT_STATION_NUMBER = this.txtOutstation.Text.Trim();
                            this.meter.BAUD_RATE = int.Parse(this.cboBaudrate.Text);
                            this.meter.REG_NUM = 0;
                            this.meter.MET_TYPE = this.cboMetType.Text;
                            this.meter.TU = (int)this.udTU.EditValue;
                            this.meter.TI = this.meter.TU;
                            this.meter.T = this.meter.TU;
                            this.meter.COMM_LINE = this.cboLine.Text;
                            db.Meters.Add(this.meter);
                            db.SaveChanges();
                            DataRow rNew = this.source.NewRow();
                            this.meter.ToDataRow(rNew);
                            this.source.Rows.Add(rNew);
                        }
                        else if (this.action == ActionType.Edit)
                        {
                            this.meter = db.Meters.Find(this.txtMetID.Text.Trim());
                            this.meter.MET_ID = this.txtMetID.Text.Trim();
                            this.meter.MET_KEY = this.txtMetKey.Text.Trim();
                            this.meter.MADDO = this.txtMaDDO.Text.Trim();
                            this.meter.TENDDO = this.txtTenDDO.Text.Trim();
                            this.meter.DATALINK_OSN = this.txtMaDDO.Text.Trim();
                            this.meter.PROG_PW = this.txtProgPW.Text.Trim();
                            this.meter.OUT_STATION_NUMBER = this.txtOutstation.Text.Trim();
                            this.meter.BAUD_RATE = int.Parse(this.cboBaudrate.Text);
                            this.meter.REG_NUM = 0;
                            this.meter.MET_TYPE = this.cboMetType.Text;
                            this.meter.TU = (int)this.udTU.EditValue;
                            this.meter.TI = this.meter.TU;
                            this.meter.T = this.meter.TU;
                            this.meter.COMM_LINE = this.cboLine.Text;
                            db.Entry(this.meter).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            DataRow rUpdate = this.source.AsEnumerable().FirstOrDefault(d => d["MET_ID"].ToString() == this.meter.MET_ID);
                            this.meter.ToDataRow(rUpdate);
                        }
                        this.source.AcceptChanges();
                        IsChanged = true;
                        this.Close();
                    }
                }
            }
            catch
            {

            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void SetBaudrateValues(ActionType action)
        {
            this.cboBaudrate.Items.Clear();
            this.cboBaudrate.Items.Add("300");
            this.cboBaudrate.Items.Add("600");
            this.cboBaudrate.Items.Add("1200");
            this.cboBaudrate.Items.Add("2400");
            this.cboBaudrate.Items.Add("4800");
            this.cboBaudrate.Items.Add("9600");
            this.cboBaudrate.Items.Add("19200");
            this.cboBaudrate.Items.Add("38400");
            this.cboBaudrate.Items.Add("57600");
            this.cboBaudrate.Items.Add("115200");
            if (action == ActionType.Add)
            {
                this.cboBaudrate.Text = "9600";
            }
            else
            {
                this.cboBaudrate.Text = this.meter.BAUD_RATE.ToString();
            }
        }

        public void SetLineList(ActionType action)
        {
            if (this.Lines != null && this.Lines.Count > 0)
            {
                this.cboLine.ItemsSource = this.Lines;
                this.cboLine.DisplayMember = "LINE_ID";
                this.cboLine.ValueMember = "LINE_ID";
                if (action == ActionType.Add)
                {
                    this.cboLine.SelectedIndex = 0;
                }
                else
                {
                    this.cboLine.Text = this.meter.COMM_LINE;
                }
            }
        }

        public void SetMetType(ActionType action)
        {
            this.cboMetType.Items.Clear();
            this.cboMetType.Items.Add("A1700");
            this.cboMetType.Items.Add("LandisGyr");
            if (action == ActionType.Add)
            {
                this.cboMetType.SelectedIndex = 0;
            }
            else
            {
                this.cboMetType.Text = this.meter.MET_TYPE;
            }
        }
    }
}
