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
using System.Data;
using System.Xml;
using System.IO;
using STHMServer;

namespace STHM
{
    /// <summary>
    /// Interaction logic for frm_Register.xaml
    /// </summary>
    public partial class frm_License : Window
    {
        private string _Pass;
        private string[] _PassMoreDays;
        public int _Runed;
        public int DaysRestore;
        public int NumRestore;
        public bool isLicensed = false;
        private License license = new License();
        public TrialMaker.RunTypes type { get; set; }
        public frm_License(string BaseString, string Password, int DaysToEnd, int Runed, string info, string[] passMoreDays)
        {
            InitializeComponent();
            txtIDNumber.Text = BaseString;
            _Pass = Password;
            _Runed = Runed;
            lbDaysLeft.Content = DaysToEnd + " Day(s)";
            if (DaysToEnd <= 0 || Runed <= 0)
            {
                lbDaysLeft.Content = "Finished";
                btnOK.IsEnabled = false;
            }
            _PassMoreDays = passMoreDays;
            //frm_Register frm = new frm_Register();
            //frm.ShowDialog();
        }
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (_Pass == txtLiceseKey.Text)
            {
                MessageBox.Show("Đăng kí bản quyền thành công!", "Thông báo", MessageBoxButton.OK);
                type = TrialMaker.RunTypes.Licensed;
                isLicensed = true;
                this.DialogResult = true;
                this.Hide();
            }
            else if (isMoreDays())
            {
                MessageBox.Show("Kích hoạt thành công, bạn có " + DaysRestore + " ngày dùng thử", "Thông báo", MessageBoxButton.OK);
                type = TrialMaker.RunTypes.Trial;
                isLicensed = false;
                this.DialogResult = false;
            }
            else
            {
                MessageBox.Show("Đăng kí không thành công", "Thông báo", MessageBoxButton.OK);
            }
        }

        private void btnStartFreeTrial_Click(object sender, RoutedEventArgs e)
        {
            this.Height = 400;
            lbDaysLeft.Visibility = System.Windows.Visibility.Visible;
            lbFreeTrialLeft.Visibility = System.Windows.Visibility.Visible;
            btnOK.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            type = TrialMaker.RunTypes.Trial;
            isLicensed = true;
            this.Hide();
        }
        private bool isMoreDays()
        {
            try
            {
                if (_PassMoreDays != null)
                {
                    string text = txtLiceseKey.Text;
                    int startIndex = (text.Length - 2) / 2;
                    string s = text.Substring(startIndex, 2);
                    DaysRestore = int.Parse(s);
                    text = text.Remove((text.Length - 2) / 2, 2);
                    int num2 = 0;
                    if (_Runed == 50)
                    {
                        _Runed = 0;
                    }
                    string[] passMoreDays = _PassMoreDays;
                    string[] array = passMoreDays;
                    foreach (string str3 in array)
                    {
                        if (str3 == text && num2 == _Runed)
                        {
                            NumRestore = num2 + 1;
                            return true;
                        }
                        num2++;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtIDNumber.Text);
        }
    }
}
