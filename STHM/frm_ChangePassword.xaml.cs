using STHM.Data;
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

namespace STHM
{
    /// <summary>
    /// Interaction logic for frm_ChangePassword.xaml
    /// </summary>
    public partial class frm_ChangePassword : Window
    {
        private HtsmContext _db = new HtsmContext();
        private string userName;
        public frm_ChangePassword(string _userName)
        {
            InitializeComponent();
            userName = _userName;
            txtTaiKhoan.Text = _userName;
        }

        private void btnThayDoi_Click(object sender, RoutedEventArgs e)
        {
            string oldPass = txtMatKhauCu.Text;
            string newPass = txtMatKhauMoi.Text;
            Nguoidung checkUser = _db.Nguoidungs.Find(userName);
            if (checkUser == null)
            {
                MessageBox.Show("Sai tên người dùng");
                txtTaiKhoan.Focus();
            }
            else
            {
                //pass để trống
                if(checkUser.Password == "")
                {
                        if (newPass == checkUser.Password)
                        {
                            MessageBox.Show("Mật khẩu mới trùng với mật khẩu cũ!");
                            txtMatKhauMoi.Focus();
                            return;
                        }
                        else
                        {
                            if (MessageBox.Show("Đổi mật khẩu ?", "Đổi mật khẩu", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                            {
                                    checkUser.Password = LIB.Password.EnPassword(newPass);
                                    _db.Entry<Nguoidung>(checkUser).State = System.Data.Entity.EntityState.Modified;
                                    _db.SaveChanges();
                                    MessageBox.Show("Thay đổi mật khẩu thành công!");
                                    this.Close();
                            }
                            else
                            {
                                txtMatKhauMoi.Focus();
                            }
                        }
                }
                else
                {
                    if (LIB.Password.DePassword(checkUser.Password) != oldPass)
                    {
                        MessageBox.Show("Mật khẩu cũ không đúng!");
                        txtMatKhauCu.Focus();
                        return;
                    }
                    else
                    {
                        if (newPass == LIB.Password.DePassword(checkUser.Password))
                        {
                            MessageBox.Show("Mật khẩu mới trùng với mật khẩu cũ!");
                            txtMatKhauMoi.Focus();
                            return;
                        }
                        else
                        {
                            if (MessageBox.Show("Đổi mật khẩu ?", "Đổi mật khẩu", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                            {
                                try {
                                    //cho phép pass để trống
                                if (newPass != "")
                                {
                                    checkUser.Password = LIB.Password.EnPassword(newPass);
                                    _db.Entry<Nguoidung>(checkUser).State = System.Data.Entity.EntityState.Modified;
                                    _db.SaveChanges();
                                    Nguoidung user12121 = _db.Nguoidungs.Find(userName);
                                    MessageBox.Show("Thay đổi mật khẩu thành công!");
                                    this.Close();
                                }
                                else
                                {
                                    checkUser.Password = newPass;
                                    _db.Entry<Nguoidung>(checkUser).State = System.Data.Entity.EntityState.Modified;
                                    _db.SaveChanges();
                                    MessageBox.Show("Thay đổi mật khẩu thành công!");
                                    this.Close();
                                }
                                }catch(Exception ex){
                                    MessageBox.Show("Thay đổi mật khẩu không thành công do lỗi: "+ex.Message);
                                    txtMatKhauMoi.Focus();
                                    return;
                                }
                                
                            }
                            else
                            {
                                txtMatKhauMoi.Focus();
                            }
                        }
                    }
                }
            }
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
