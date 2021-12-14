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
using STHM.LIB;
using STHM.Data;
using STHM.Media;
using System.Data;

namespace STHM
{
    /// <summary>
    /// Interaction logic for frm_Line.xaml
    /// </summary>
    public partial class frm_Line : Window
    {
        private ActionType _Action = ActionType.Add;
        private Lines _Line;
        private DataTable _Source;
        private Common common;
        public bool IsChanged { get; set; }
        public frm_Line(ActionType action, Lines line, DataTable source)
        {
            InitializeComponent();
            this.Loaded += frm_Line_Loaded;
            _Action = action;
            _Line = line;
            _Source = source;
            this.common = new Common();
        }

        void frm_Line_Loaded(object sender, RoutedEventArgs e)
        {
            if (_Action == ActionType.Add)
            {
                ClearValue();
                txtId.EditValue = PublicValue.GET_MATUDONG_INT(this.GetType().Name, this.GetType().Name, false);
                txtIpAddress.Focus();
            }
            else if (_Action == ActionType.Edit)
            {
                txtId.EditValue = _Line.LINE_ID;
                txtId.IsReadOnly = true;
                IPPortLine line = new IPPortLine(this._Line.CONFIG);
                txtIpAddress.Text = line.Address;
                txtPort.EditValue = line.Port;
                txtIpAddress.Focus();
            }
            IsChanged = false;
        }

        public void ClearValue()
        {
            txtId.Clear();
            txtIpAddress.EditValue = "127.0.0.0";
            txtPort.EditValue = "2101";
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new HtsmContext())
                {
                    string config = new IPPortLine { Address = this.txtIpAddress.Text.Trim(), Port = int.Parse(this.txtPort.Text) }.GetConfig();
                    if (_Action == ActionType.Add)
                    {
                        DataRow rNew = _Source.NewRow();
                        if (txtId.Text == PublicValue.GET_MATUDONG_INT(this.GetType().Name, this.GetType().Name, false).ToString())
                            txtId.Text = PublicValue.GET_MATUDONG_INT(this.GetType().Name, this.GetType().Name, true).ToString();
                        Lines check = db.Lines.Find(txtId.Text);
                        if (check != null)
                        {
                            common.ShowDialog("ID Kênh truyền thông bị trùng, Hãy nhập Id khác");
                            txtId.Text = PublicValue.GET_MATUDONG_INT(this.GetType().Name, this.GetType().Name, false).ToString();
                            txtId.Focus();
                            return;
                        }
                        _Line = new Lines(txtId.Text, "IPPortLine", config);
                        db.Lines.Add(_Line);
                        db.SaveChanges();
                        _Line.ToDataRow(rNew);
                        _Source.Rows.Add(rNew);
                    }
                    else if (_Action == ActionType.Edit)
                    {
                        _Line = db.Lines.Find(_Line.LINE_ID);
                        _Line.CONFIG = config;
                        _Line.LINE_TYPE = "IPPortLine";
                        db.Entry(_Line).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        DataRow rUpdate = _Source.AsEnumerable().FirstOrDefault(d => d["LINE_ID"].ToString() == _Line.LINE_ID);
                        _Line.ToDataRow(rUpdate);
                    }
                    _Source.AcceptChanges();
                    IsChanged = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void btnDong_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
