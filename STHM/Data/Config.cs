using STHM.LIB;
using System;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace STHM.Data
{
    public class Config
    {
        private const int CryptoCycle = 3;
        private byte[] IV = Encoding.ASCII.GetBytes("12345678STHMSTHM");
        private byte[] key = Encoding.ASCII.GetBytes("sthmsthmsthmsthmsthm123456123456");

        public Config()
        {
            ConnectionString = "Data Source=Htsm.db";
            Port = 2101;
            AutoRun = false;
            OpenFile = false;
            LocalData = true;
            IsPCVersion = false;
            ScheduleInterval = 5000;
            ScheduleDelay = 30;
            CommunicationDelay = 60000;
            ShowCommunicationLog = false;
            ReadingFailResetPercent = 80;
            MonitorLineInterval = 30;
            RememberLogin = 1;
            UserType = 1;
            User = "Admin";
            Password = Cryptography.Encrypt("Admin", key, IV, 3);
            User2 = "operator";
            Password2 = Cryptography.Encrypt("operator", key, IV, 3);
            ScheduleExportFilePath = Application.StartupPath + "\\CSV";
            ManualExportFile = false;
            ManualExportFilePath = ScheduleExportFilePath;
            ExportDataOldPath = ManualExportFilePath;
            ExportDataCompany = ManualExportFilePath;
            formatfile = ".CSV";
            UserLoggedIn = User;
            Ma_NM = "MaNM";
        }

        public Config(string ConnectionString, int Port, bool AutoRun, bool OpenFile, bool LocalData, bool IsPCVersion, int ScheduleInterval, int ScheduleDelay, int CommunicationDelay, bool ShowCommunicationLog, int ReadingFailResetPercent, int MonitorLineInterval, int RememberLogin, int UserType, string User, string Password, string User2, string Password2, string ScheduleExportFilePath, bool ManualExportFile, string ManualExportFilePath, string ExportDataOldPath, string exportDataCompany, string formatfile, string UserLoggedIn, string ma_NM)
        {
            this.ConnectionString = ConnectionString;
            this.Port = Port;
            this.AutoRun = AutoRun;
            this.OpenFile = OpenFile;
            this.LocalData = LocalData;
            this.IsPCVersion = IsPCVersion;
            this.ScheduleInterval = ScheduleInterval;
            this.ScheduleDelay = ScheduleDelay;
            this.CommunicationDelay = CommunicationDelay;
            this.ShowCommunicationLog = ShowCommunicationLog;
            this.ReadingFailResetPercent = ReadingFailResetPercent;
            this.MonitorLineInterval = MonitorLineInterval;
            this.RememberLogin = RememberLogin;
            this.UserType = UserType;
            this.User = User;
            this.Password = Password;
            this.User2 = User2;
            this.Password2 = Password2;
            this.ScheduleExportFilePath = ScheduleExportFilePath;
            this.ManualExportFile = ManualExportFile;
            this.ManualExportFilePath = ManualExportFilePath;
            this.ExportDataOldPath = ExportDataOldPath;
            this.ExportDataCompany = exportDataCompany;
            this.formatfile = formatfile;
            this.UserLoggedIn = UserLoggedIn;
            this.Ma_NM = ma_NM;
        }

        public void ReadConfigFromFile(string configFilename)
        {
            DataSet set = new DataSet();
            set.ReadXml(configFilename);
            DataTable table = set.Tables[0];
            this.ConnectionString = table.Rows[0]["ConnectionString"].ToString();
            this.Port = int.Parse(table.Rows[0]["Port"].ToString());
            this.AutoRun = bool.Parse(table.Rows[0]["AutoRun"].ToString());
            this.OpenFile = bool.Parse(table.Rows[0]["OpenFile"].ToString());
            this.LocalData = bool.Parse(table.Rows[0]["LocalData"].ToString());
            this.IsPCVersion = bool.Parse(table.Rows[0]["IsPCVersion"].ToString());
            this.ScheduleInterval = int.Parse(table.Rows[0]["ScheduleInterval"].ToString());
            this.ScheduleDelay = int.Parse(table.Rows[0]["ScheduleDelay"].ToString());
            this.CommunicationDelay = int.Parse(table.Rows[0]["CommunicationDelay"].ToString());
            this.ShowCommunicationLog = bool.Parse(table.Rows[0]["ShowCommunicationLog"].ToString());
            this.ReadingFailResetPercent = int.Parse(table.Rows[0]["ReadingFailResetPercent"].ToString());
            this.MonitorLineInterval = int.Parse(table.Rows[0]["MonitorLineInterval"].ToString());
            this.RememberLogin = int.Parse(table.Rows[0]["RememberLogin"].ToString());
            this.UserType = int.Parse(table.Rows[0]["UserType"].ToString());
            this.User = table.Rows[0]["User"].ToString();
            if (string.IsNullOrEmpty(table.Rows[0]["Password"].ToString()))
            {
                this.Password = "";
            }
            else this.Password = table.Rows[0]["Password"].ToString();
           
            //this.User2 = table.Rows[0]["User2"].ToString();
            //this.Password2 = Cryptography.Decrypt(table.Rows[0]["Password2"].ToString(), this.key, this.IV, 3);
            this.ScheduleExportFilePath = table.Rows[0]["ScheduleExportFilePath"].ToString();
            this.ManualExportFile = bool.Parse(table.Rows[0]["ManualExportFile"].ToString());
            this.ManualExportFilePath = table.Rows[0]["ManualExportFilePath"].ToString();
            this.ExportDataOldPath = table.Rows[0]["ExportDataOldPath"].ToString();
            this.ExportDataCompany = table.Rows[0]["ExportDataCompany"].ToString();
            this.formatfile = table.Rows[0]["formatfile"].ToString();
            this.Ma_NM = table.Rows[0]["MaNM"].ToString();
        }

        public void ReadConfigFromTable(DataTable dtconfig)
        {
            this.ConnectionString = dtconfig.Rows[0]["ConnectionString"].ToString();
            this.Port = int.Parse(dtconfig.Rows[0]["Port"].ToString());
            this.AutoRun = bool.Parse(dtconfig.Rows[0]["AutoRun"].ToString());
            this.OpenFile = bool.Parse(dtconfig.Rows[0]["OpenFile"].ToString());
            this.LocalData = bool.Parse(dtconfig.Rows[0]["LocalData"].ToString());
            this.IsPCVersion = bool.Parse(dtconfig.Rows[0]["IsPCVersion"].ToString());
            this.ScheduleInterval = int.Parse(dtconfig.Rows[0]["ScheduleInterval"].ToString());
            this.ScheduleDelay = int.Parse(dtconfig.Rows[0]["ScheduleDelay"].ToString());
            this.CommunicationDelay = int.Parse(dtconfig.Rows[0]["CommunicationDelay"].ToString());
            this.ShowCommunicationLog = bool.Parse(dtconfig.Rows[0]["ShowCommunicationLog"].ToString());
            this.ReadingFailResetPercent = int.Parse(dtconfig.Rows[0]["ReadingFailResetPercent"].ToString());
            this.MonitorLineInterval = int.Parse(dtconfig.Rows[0]["MonitorLineInterval"].ToString());
            this.RememberLogin = int.Parse(dtconfig.Rows[0]["RememberLogin"].ToString());
            this.UserType = int.Parse(dtconfig.Rows[0]["UserType"].ToString());
            this.User = dtconfig.Rows[0]["User"].ToString();
            this.Password = Cryptography.Decrypt(dtconfig.Rows[0]["Password"].ToString(), this.key, this.IV, 3);
            this.User2 = dtconfig.Rows[0]["User2"].ToString();
            this.Password2 = Cryptography.Decrypt(dtconfig.Rows[0]["Password2"].ToString(), this.key, this.IV, 3);
            this.ScheduleExportFilePath = dtconfig.Rows[0]["ScheduleExportFilePath"].ToString();
            this.ManualExportFile = bool.Parse(dtconfig.Rows[0]["ManualExportFile"].ToString());
            this.ManualExportFilePath = dtconfig.Rows[0]["ManualExportFilePath"].ToString();
            this.ExportDataOldPath = dtconfig.Rows[0]["ExportDataOldPath"].ToString();
            this.ExportDataCompany = dtconfig.Rows[0]["ExportDataCompany"].ToString();
            this.formatfile = dtconfig.Rows[0]["formatfile"].ToString();
            this.Ma_NM = dtconfig.Rows[0]["MaNM"].ToString();
        }

        public void WriteConfig(string configFilename)
        {
            DataSet set = new DataSet();
            DataTable table = new DataTable();
            table.Columns.Add("ConnectionString", typeof(string));
            table.Columns.Add("Port", typeof(int));
            table.Columns.Add("AutoRun", typeof(bool));
            table.Columns.Add("LocalData", typeof(bool));
            table.Columns.Add("OpenFile", typeof(bool));
            table.Columns.Add("ScheduleInterval", typeof(int));
            table.Columns.Add("IsPCVersion", typeof(bool));
            table.Columns.Add("ScheduleDelay", typeof(int));
            table.Columns.Add("CommunicationDelay", typeof(int));
            table.Columns.Add("ShowCommunicationLog", typeof(bool));
            table.Columns.Add("ReadingFailResetPercent", typeof(int));
            table.Columns.Add("MonitorLineInterval", typeof(int));
            table.Columns.Add("RememberLogin", typeof(int));
            table.Columns.Add("UserType", typeof(int));
            table.Columns.Add("User", typeof(string));
            table.Columns.Add("Password", typeof(string));
            table.Columns.Add("User2", typeof(string));
            table.Columns.Add("Password2", typeof(string));
            table.Columns.Add("ScheduleExportFilePath", typeof(string));
            table.Columns.Add("ManualExportFile", typeof(bool));
            table.Columns.Add("ManualExportFilePath", typeof(string));
            table.Columns.Add("ExportDataOldPath", typeof(string));
            table.Columns.Add("ExportDataCompany", typeof(string));
            table.Columns.Add("formatfile", typeof(string));
            table.Columns.Add("MaNM", typeof(string));
            try
            {
                DataRow row = table.NewRow();
                row["ConnectionString"] = this.ConnectionString;
                row["Port"] = this.Port;
                row["AutoRun"] = this.AutoRun;
                row["LocalData"] = this.LocalData;
                row["OpenFile"] = this.OpenFile;
                row["ScheduleInterval"] = this.ScheduleInterval;
                row["IsPCVersion"] = this.IsPCVersion;
                row["ScheduleDelay"] = this.ScheduleDelay;
                row["CommunicationDelay"] = this.CommunicationDelay;
                row["ShowCommunicationLog"] = this.ShowCommunicationLog;
                row["ReadingFailResetPercent"] = this.ReadingFailResetPercent;
                row["MonitorLineInterval"] = this.MonitorLineInterval;
                row["RememberLogin"] = this.RememberLogin;
                row["UserType"] = this.UserType;
                row["User"] = this.User;
                row["Password"] = this.Password;
                row["User2"] = this.User2;
                row["Password2"] = this.Password2;
                row["ScheduleExportFilePath"] = this.ScheduleExportFilePath;
                row["ManualExportFile"] = this.ManualExportFile;
                row["ManualExportFilePath"] = this.ManualExportFilePath;
                row["ExportDataOldPath"] = this.ExportDataOldPath;
                row["ExportDataCompany"] = this.ExportDataCompany;
                row["formatfile"] = this.formatfile;
                row["MaNM"] = this.Ma_NM;
                table.Rows.Add(row);
                set.Tables.Add(table);
                set.WriteXml(configFilename);
            }
            catch (Exception exception)
            {
                throw new Exception("WriteConfig() error: " + exception.Message);
            }
        }
        public string Mahoa(string str)
        {
            return Cryptography.Encrypt(str, this.key, this.IV, 3);
        }

        public string Giaima(string str)
        {
            return Cryptography.Decrypt(str, this.key, this.IV, 3);
        }
        public bool AutoRun { get; set; }

        public int CommunicationDelay { get; set; }

        public string ConnectionString { get; set; }

        public string ExportDataCompany { get; set; }

        public string ExportDataOldPath { get; set; }

        public string formatfile { get; set; }

        public bool IsPCVersion { get; set; }

        public bool LocalData { get; set; }

        public string Ma_NM { get; set; }

        public bool ManualExportFile { get; set; }

        public string ManualExportFilePath { get; set; }

        public int MonitorLineInterval { get; set; }

        public bool OpenFile { get; set; }

        public string Password { get; set; }

        public string Password2 { get; set; }

        public string PasswordLoggedIn { get; set; }

        public int Port { get; set; }

        public int ReadingFailResetPercent { get; set; }

        public int RememberLogin { get; set; }

        public int ScheduleDelay { get; set; }

        public string ScheduleExportFilePath { get; set; }

        public int ScheduleInterval { get; set; }

        public bool ShowCommunicationLog { get; set; }

        public string User { get; set; }

        public string User2 { get; set; }

        public string UserLoggedIn { get; set; }

        public int UserType { get; set; }
    }
}

