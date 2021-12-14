using Microsoft.Win32;
using STHM.LIB;
using STHMServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using System.Xml;

namespace STHM
{
    /// <summary>
    /// Interaction logic for frm_Register.xaml
    /// </summary>
    public partial class frm_Register : Window
    {
        License license = new License();
        private string _BaseString;
        private string _Password;
        private string _Identifier = "252513251";
        private string _SoftName ="STHM";

        public frm_Register()
        {
            InitializeComponent();
            txtComputerID.Text = "";
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtSerial.Text);
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            MakeBaseString();
            MakePassword();
            txtSerial.Text = _Password;
        }
        private void MakePassword()
        {
            _Password = Encryption.MakePassword(_BaseString, _Identifier);
        }

        private void MakeBaseString()
        {
            _BaseString = Encryption.Boring(Encryption.InverseByBase(SystemInfo.GetSystemInfo(_SoftName), 10));
        }
    }
}
