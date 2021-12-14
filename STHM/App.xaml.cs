using STHMServer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using STHM.Data;
using STHM.LIB;

namespace STHM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private bool _isExit;
        private const string Guid = "250C5597-BA73-40DF-B2CF-DD644F674834";
        static readonly Mutex Mutex = new Mutex(true, "{" + Guid + "}");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //check phan mem chay ngam!
            if (!Mutex.WaitOne(TimeSpan.Zero, true))
            {
                //MessageBox.Show("Already running.", "Startup Warning");
                MessageBox.Show("Chương trình đang chạy", "STHM", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                Application.Current.Shutdown();
            }
            else
            {
                TrialMaker maker = new TrialMaker("STHM", System.AppDomain.CurrentDomain.BaseDirectory + "Register.reg", System.AppDomain.CurrentDomain.BaseDirectory + "STHMSetp.dbf", "", 10, 10, "252513251");
                maker.TripleDESKey = new byte[24]
            {
                100, 134, 65, 1, 20, 38, 71, 98, 67, 12,
                76, 88, 11, 9, 14, 76, 54, 21, 87, 123,
                233, 6, 199, 5
            };
                TrialMaker.RunTypes types = maker.ShowDialog();
                int a = maker.DaysToEnd();
                bool flag2 = false;
                switch (types)
                {
                    case TrialMaker.RunTypes.Trial:
                        if (a != 0)
                        {
                            flag2 = true;
                        }
                        break;
                    case TrialMaker.RunTypes.Licensed:
                        flag2 = true;
                        break;
                    default:
                        Application.Current.Shutdown();
                        return;
                }
                if (flag2)
                {
                    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                    AppDomain.CurrentDomain.TypeResolve += CurrentDomain_TypeResolve;
                    GC.Collect();
                    frm_Login login = new frm_Login();
                    login.ShowDialog();
                    if (login.isLogin)
                    {
                        Config config = new Config();
                        try
                        {
                            config.ReadConfigFromFile(PublicValue.configFilename);
                        }
                        catch
                        {
                            Common com = new Common();
                            com.ShowDialog("Không lấy được thông tin cấu hình!");
                            if (MessageBox.Show("Tạo file cấu hình mặc định?", "Xác Nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                login.config.WriteConfig(PublicValue.configFilename);
                            }
                        }
                        config.UserLoggedIn = login.txtTaiKhoan.Text.Trim();

                        MainWindow = new MainWindow(config);
                        MainWindow.Closing += MainWindow_Closing;

                        _notifyIcon = new System.Windows.Forms.NotifyIcon();
                        _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
                        _notifyIcon.Icon = STHM.Properties.Resources.app;
                        _notifyIcon.Visible = true;

                        CreateContextMenu();
                        ShowMainWindow();
                        login.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Chương trình hết hạn sử dụng", "STHM Server", MessageBoxButton.OK);

                }

            }
        }

        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip =
              new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Màn hình chính").Click += (s, e) => ShowMainWindow();
            _notifyIcon.ContextMenuStrip.Items.Add("Thoát").Click += (s, e) => ExitApplication();
        }

        private void ExitApplication()
        {
            if (MessageBox.Show("Bạn chắc chắn muốn thoát hoàn toàn phần mềm không?", "STHM", MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _isExit = true;
                MainWindow.Close();
                Application.Current.Shutdown();
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
        }

        private void ShowMainWindow()
        {
            if (MainWindow.IsVisible)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    MainWindow.WindowState = WindowState.Normal;
                }
                MainWindow.Activate();
            }
            else
            {
                MainWindow.Show();
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
                MainWindow.Hide(); // A hidden window can be shown again, a closed one not
            }
        }
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string[] typeinfo = args.Name.Split(',');
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly[] array = assemblies;
            foreach (Assembly it in array)
            {
                if (it.GetName().Name.Equals(typeinfo[0]) || it.GetName().FullName.Equals(typeinfo[0]))
                {
                    return it;
                }
            }
            string[] tmp = args.Name.Split(',');
            string path = "";
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                path = Path.Combine("/usr", "lib");
                path = Path.Combine(path, tmp[0].ToLower().Replace(".", "-"));
                if (Directory.Exists(path))
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    FileInfo[] files = di.GetFiles(tmp[0] + ".dll");
                    int num = 0;
                    if (num < files.Length)
                    {
                        FileInfo fi = files[num];
                        Trace.WriteLine("CurrentDomain_AssemblyResolve: Returning assembly from(3):" + fi.FullName);
                        return Assembly.LoadFile(fi.FullName);
                    }
                }
            }
            return null;
        }

        private static Assembly CurrentDomain_TypeResolve(object sender, ResolveEventArgs args)
        {
            string ns = "";
            int pos = args.Name.LastIndexOf('.');
            if (pos != -1)
            {
                ns = args.Name.Substring(0, pos);
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                Assembly[] array = assemblies;
                foreach (Assembly assembly2 in array)
                {
                    if (assembly2.GetName().Name == ns && assembly2.GetType(args.Name, throwOnError: false, ignoreCase: true) != null)
                    {
                        return assembly2;
                    }
                }
            }
            Assembly[] assemblies2 = AppDomain.CurrentDomain.GetAssemblies();
            Assembly[] array2 = assemblies2;
            foreach (Assembly assembly in array2)
            {
                Type[] types = assembly.GetTypes();
                Type[] array3 = types;
                foreach (Type type in array3)
                {
                    if (type.Name == args.Name || type.FullName == args.Name)
                    {
                        return assembly;
                    }
                }
            }
            try
            {
                return Assembly.LoadFrom(ns + ".dll");
            }
            catch
            {
            }
            return null;
        }
    }
}
