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
using System.Windows.Shapes;
using STHM.Data;
using STHM.LIB;


namespace STHM
{
    /// <summary>
    /// Interaction logic for controlConfig.xaml
    /// </summary>
    public partial class controlConfig : UserControl
    {
        private Config config;
        private Common common;
        public bool IsChanged { get; set; }
        public controlConfig()
        {
            InitializeComponent();
            this.Loaded += controlConfig_Loaded;
            common = new Common();
        }

        void controlConfig_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();
            LoadFileFormat();
            IsChanged = false;
            //using (var db = new HtsmContext())
            //{
            //    config = db.Configs.Where(d => d.Ma_NM == PublicValue.MaNM).FirstOrDefault();
            //}
            this.LoadDataControl();
        }
        public void LoadConfig()
        {
            try
            {
                this.config = new Config();
                this.config.ReadConfigFromFile(PublicValue.configFilename);
            }
            catch (Exception)
            {
                //common.ShowDialog("Không lấy được thông tin cấu hình!");
            }
        }
        private void LoadFileFormat()
        {
            cboformatfile.Items.Clear();
            cboformatfile.Items.Add(".CSV");
            cboformatfile.Items.Add(".TXT");
            cboformatfile.SelectedIndex = 0;
        }

        private void LoadDataControl()
        {
            txtScheduleInterval.EditValue = config.ScheduleInterval;
            txtScheduleDelay.EditValue = config.ScheduleDelay;
            chkAutoRun.IsChecked = config.AutoRun;
            chkOpenFile.IsChecked = config.OpenFile;
            chkCommunicationLog.IsChecked = config.ShowCommunicationLog;
            txtScheduleExportFilePath.Text = config.ScheduleExportFilePath;
            txtManualExportFilePath.Text = config.ManualExportFilePath;
            txtExportDataOldPath.Text = config.ExportDataOldPath;
            cboformatfile.EditValue = config.formatfile;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.config.ScheduleInterval = (int)txtScheduleInterval.EditValue;
                this.config.ScheduleDelay = (int)txtScheduleDelay.EditValue;
                this.config.AutoRun = (bool)chkAutoRun.IsChecked;
                this.config.OpenFile = (bool)chkOpenFile.IsChecked;
                this.config.ShowCommunicationLog = (bool)chkCommunicationLog.IsChecked;
                this.config.ScheduleExportFilePath = txtScheduleExportFilePath.Text;
                this.config.ManualExportFilePath = txtManualExportFilePath.Text;
                this.config.ExportDataOldPath = txtExportDataOldPath.Text;
                this.config.ExportDataCompany = txtExportDataOldPath.Text;
                this.config.formatfile = (string)cboformatfile.EditValue;
                this.config.WriteConfig(PublicValue.configFilename);
                IsChanged = true;
                common.ShowDialog("Lưu cấu hình thành công!");
            }
            catch (Exception ex)
            {
                common.ShowDialog("Lỗi: " + ex.Message);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private void txtScheduleExportFilePath_DefaultButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowser.SelectedPath = config.ScheduleExportFilePath;
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                config.ScheduleExportFilePath = folderBrowser.SelectedPath;
                txtScheduleExportFilePath.Text = config.ScheduleExportFilePath;
            }
        }

        private void txtManualExportFilePath_DefaultButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowser.SelectedPath = config.ManualExportFilePath;
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                config.ManualExportFilePath = folderBrowser.SelectedPath;
                txtManualExportFilePath.Text = config.ManualExportFilePath;
            }
        }

        private void txtExportDataOldPath_DefaultButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowser.SelectedPath = config.ExportDataOldPath;
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                config.ExportDataOldPath = folderBrowser.SelectedPath;
                txtExportDataOldPath.Text = config.ExportDataOldPath;
            }
        }
    }
}
