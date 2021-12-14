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
using System.IO;
namespace STHM
{
    /// <summary>
    /// Interaction logic for frm_Login.xaml
    /// </summary>
    public partial class frm_Login : Window
    {
        private HtsmContext _db = new HtsmContext();
        public bool isLogin = false;
        public Config config = new Config();
        private bool isHaveConfig = false;
        public frm_Login()
        {
            InitializeComponent();
            try
            {
                config.ReadConfigFromFile(PublicValue.configFilename);
                if (config.RememberLogin == 1)
                {
                    txtTaiKhoan.Text = config.User;
                    txtMatKhau.Text =LIB.Password.DePassword(config.Password);
                    ceLuuMatKhau.IsChecked = Convert.ToBoolean(config.RememberLogin);
                }
                isHaveConfig = true;
            }
            catch
            {
                if (MessageBox.Show("Tạo file cấu hình mặc định?", "Xác Nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    config.WriteConfig(PublicValue.configFilename);
                }
                isHaveConfig = false;
            }
            txtTaiKhoan.Focus();
        }

        private void btnDangNhap_Click(object sender, RoutedEventArgs e)
        {
            //Nguoidung checkUser = _db.Nguoidungs.Where(d => d.UserName == userName && d.Password == passWord).FirstOrDefault();
            _db = new HtsmContext();
            string userName = txtTaiKhoan.Text;
            string passWord = txtMatKhau.Text;
            if (_db.Nguoidungs.Find(userName) == null)
            {
                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu !");
            }
            else
            {
                if (_db.Nguoidungs.Find(userName).Password == "")
                {
                    //check pass để trống
                    if (_db.Nguoidungs.Find(userName).Password != passWord)
                    {
                        MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu !");
                    }
                    else
                    {
                        isLogin = true;
                        this.Hide();
                        return;
                    }
                }
                else
                {
                    string a = LIB.Password.DePassword(_db.Nguoidungs.Find(userName).Password);
                    if (LIB.Password.DePassword(_db.Nguoidungs.Find(userName).Password) != passWord)
                    {
                        MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu !");
                    }
                    else
                    {
                        isLogin = true;
                        if (ceLuuMatKhau.IsChecked == true)
                        {
                            config.User = txtTaiKhoan.Text;
                            config.Password = LIB.Password.EnPassword(txtMatKhau.Text.Trim());
                            config.RememberLogin = 1;
                        }
                        else
                        {
                            config.User = "";
                            config.Password = "";
                            config.RememberLogin = 0;
                        }
                        config.WriteConfig(PublicValue.configFilename);
                        this.Hide();
                        return;
                    }
                }
            }
        }

        private void btnDoiMatKhau_Click(object sender, RoutedEventArgs e)
        {
            Nguoidung checkUser = _db.Nguoidungs.Find(txtTaiKhoan.Text);
            if (checkUser != null)
            {
                frm_ChangePassword frm = new frm_ChangePassword(checkUser.UserName);
                frm.Show();
            }
            else
            {
                return;
            }
        }

        private void btnThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnDangKy_Click(object sender, RoutedEventArgs e)
        {
            frm_Register register = new frm_Register();
            register.Show();
        }

        private void txtTaiKhoan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtMatKhau.Focus();
            }
        }

        private void txtMatKhau_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnDangNhap.Focus();
            }
        }

        private void ceLuuMatKhau_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnDangNhap.Focus();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
