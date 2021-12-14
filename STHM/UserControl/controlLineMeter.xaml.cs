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
    public partial class controlLineMeter : UserControl
    {
        DataTable DT_Line;
        DataTable DT_Meter;
        Common common = new Common();
        public bool IsChanged { get; set; }
        public controlLineMeter()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            IsChanged = false;
        }

        private void LoadData()
        {
            try
            {
                using (var db = new HtsmContext())
                {
                    DT_Line = db.Lines.OrderBy(d => d.LINE_ID).ToDataTable();
                    DT_Meter = db.Meters.OrderBy(d => d.MET_ID).ToDataTable();
                    dtgDanhsachkenh.ItemsSource = DT_Line;
                    dtgDanhsachcongto.ItemsSource = DT_Meter;
                    //dtgDanhsachcongto.group
                }
            }
            catch { }
        }

        private void btnAddLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frm_Line frm = new frm_Line(ActionType.Add, new Lines(), DT_Line);
                frm.ShowDialog();
                if (frm.IsChanged == true)
                {
                    IsChanged = frm.IsChanged;
                }
                DT_Line.AcceptChanges();
            }
            catch { }
        }

        private void btnEditLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dtgDanhsachkenh.SelectedItem == null) return;
                DataRowView rvLine = (DataRowView)dtgDanhsachkenh.SelectedItem;
                if (rvLine != null)
                {
                    Lines line = new Lines(rvLine["LINE_ID"].ToString(), rvLine["LINE_TYPE"].ToString(), rvLine["CONFIG"].ToString());
                    frm_Line frm = new frm_Line(ActionType.Edit, line, DT_Line);
                    frm.ShowDialog();
                    if (frm.IsChanged == true)
                    {
                        IsChanged = frm.IsChanged;
                    }
                    DT_Line.AcceptChanges();
                }
            }
            catch { }
        }

        private void btnDeleteLine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dtgDanhsachkenh.SelectedItem == null) return;
                DataRowView rvLine = (DataRowView)dtgDanhsachkenh.SelectedItem;
                if (rvLine != null)
                {
                    if (this.common.ShowConfirm("Xóa kênh truyền thông sẽ làm mất liên kết công tơ vơi kênh này.Bạn có muốn xóa không?") == System.Windows.Forms.DialogResult.Yes)
                    {
                        string str = rvLine["LINE_ID"].ToString();
                        using (var db = new HtsmContext())
                        {
                            db.Lines.Remove(db.Lines.Find(str));
                            db.SaveChanges();
                            DataRow[] rowArray = this.DT_Line.Select("LINE_ID='" + str + "'");
                            this.DT_Line.Rows.Remove(rowArray[0]);
                            this.DT_Line.AcceptChanges();
                            IsChanged = true;
                        }

                    }
                }
            }
            catch
            {
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private void btnAddMeter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frm_Meter frm = new frm_Meter(ActionType.Add, new Meter(), DT_Meter);
                frm.ShowDialog();
                if (frm.IsChanged == true)
                {
                    IsChanged = frm.IsChanged;
                }
            }
            catch { }
        }

        private void btnEditMeter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dtgDanhsachcongto.SelectedItem == null) return;
                DataRowView rvMeter = (DataRowView)dtgDanhsachcongto.SelectedItem;
                if (rvMeter != null)
                {
                    using (var db = new HtsmContext())
                    {
                        Meter meter = db.Meters.Find(rvMeter["MET_ID"].ToString());
                        frm_Meter frm = new frm_Meter(ActionType.Edit, meter, DT_Meter);
                        frm.ShowDialog();
                        if (frm.IsChanged == true)
                        {
                            IsChanged = frm.IsChanged;
                        }
                    }
                }
            }
            catch { }
        }

        private void btnDeleteMeter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dtgDanhsachcongto.SelectedItem == null) return;
                DataRowView rvMeter = (DataRowView)dtgDanhsachcongto.SelectedItem;
                if (rvMeter != null)
                {
                    if (this.common.ShowConfirm("Bạn có muốn xóa không?") == System.Windows.Forms.DialogResult.Yes)
                    {
                        string str = rvMeter["MET_ID"].ToString();
                        using (var db = new HtsmContext())
                        {
                            db.Meters.Remove(db.Meters.Find(str));
                            db.SaveChanges();
                            DataRow[] rowArray = this.DT_Meter.Select("MET_ID='" + str + "'");
                            this.DT_Meter.Rows.Remove(rowArray[0]);
                            this.DT_Meter.AcceptChanges();
                            IsChanged = true;
                        }

                    }
                }
            }
            catch
            {
            }
        }
    }
}
