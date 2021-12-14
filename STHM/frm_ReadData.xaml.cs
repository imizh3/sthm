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
    public partial class frm_ReadData : Window
    {
        private ActionType _Action = ActionType.Load;
        public bool IsRun = false;
        public string Taskname;
        public int udNumDays;
        public DateTime StartFrom;
        private Common common = new Common();
        public frm_ReadData()
        {
            InitializeComponent();
            this.Loaded += frm_ReadData_Loaded;
        }

        void frm_ReadData_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTaskname();
            txtNumDays.EditValue = 1;
            dtpickerStartfrom.EditValue = DateTime.Now;
            cboTASKNAME.Focus();
        }

        private void LoadTaskname()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("ReadCurrentRegisterValues", "Thông số công tơ");
            data.Add("ReadHistoricalRegister", "Chỉ số chốt");
            data.Add("ReadLoadProfile", "Phụ tải công suất");
            data.Add("ReadLoadProfile_A0", "Phụ tải sản lượng");
            cboTASKNAME.ItemsSource = data;
            cboTASKNAME.DisplayMember = "Value";
            cboTASKNAME.ValueMember = "Key";
            cboTASKNAME.SelectedIndex = 0;
        }

        private void btnDong_Click(object sender, RoutedEventArgs e)
        {
            IsRun = false;
            this.Close();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsRun = true;
                Taskname = cboTASKNAME.EditValue.ToString();
                udNumDays = txtNumDays.EditValue != null ? int.Parse(txtNumDays.EditValue.ToString()) : 0;
                StartFrom = (DateTime)dtpickerStartfrom.EditValue;
                this.Close();
            }
            catch (Exception ex)
            {
                IsRun = false;
                common.ShowDialog(ex.Message);
            }
        }
    }
}
