using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STHM.LIB
{
    public class License
    {
        public enum HardwareProfileComponents
        {
            ComputerModel,
            VolumeSerial,
            CpuId,
            MemoryCapacity
        }

        public byte[] key;

        public byte[] IV;

        private string licensefile;

        private string configFile;

        private const int CryptoCycle = 3;

        public bool TrialVersion;

        public int days = 0;

        public string daysRemain;

        public License()
        {
            key = Encoding.ASCII.GetBytes("esolutions123456esolutions123456");
            IV = Encoding.ASCII.GetBytes("ABCDEFGH12345678");
            licensefile = System.AppDomain.CurrentDomain.BaseDirectory + "License.lic";
            configFile = System.AppDomain.CurrentDomain.BaseDirectory + "Config.cfg";
        }

        public bool CheckExpireDate(DataSet dsExpired)
        {
            bool isExpired = false;
            try
            {
                DateTime lastRunningDate = DateTime.Parse(Cryptography.Decrypt(dsExpired.Tables[0].Rows[0]["key3"].ToString(), key, IV, 3));
                DateTime expiredDate = DateTime.Parse(Cryptography.Decrypt(dsExpired.Tables[0].Rows[0]["key4"].ToString(), key, IV, 3));
                string daysRemain = Cryptography.Decrypt(dsExpired.Tables[0].Rows[0]["key5"].ToString(), key, IV, 3);
                if (daysRemain == "Unexpired")
                {
                    isExpired = false;
                    TrialVersion = false;
                }
                else
                {
                    TrialVersion = true;
                    try
                    {
                        days = int.Parse(daysRemain);
                        if (days <= 0)
                        {
                            isExpired = true;
                        }
                        else if (expiredDate.Subtract(lastRunningDate).Days != days)
                        {
                            isExpired = true;
                        }
                        else
                        {
                            DateTime currentDate = DateTime.Now.Date;
                            if (currentDate < lastRunningDate)
                            {
                                isExpired = true;
                            }
                            else
                            {
                                TimeSpan timeSpan = expiredDate.Subtract(currentDate);
                                if (timeSpan.Days <= 0)
                                {
                                    isExpired = true;
                                }
                                else
                                {
                                    isExpired = false;
                                    dsExpired.Tables[0].Rows[0]["key3"] = Cryptography.Encrypt(currentDate.ToShortDateString(), key, IV, 3);
                                    dsExpired.Tables[0].Rows[0]["key5"] = Cryptography.Encrypt(timeSpan.Days.ToString(), key, IV, 3);
                                    dsExpired.WriteXml(licensefile);
                                }
                            }
                        }
                    }
                    catch
                    {
                        isExpired = true;
                    }
                }
            }
            catch
            {
                isExpired = true;
            }
            return isExpired;
        }

        public bool CheckLicense()
        {
            bool isLicense = false;
            if (!File.Exists(licensefile))
            {
                isLicense = false;
            }
            else
            {
                try
                {
                    DataSet dsLicense = new DataSet();
                    dsLicense.ReadXml(licensefile);
                    string strLicenseStatus = Cryptography.Decrypt(dsLicense.Tables[0].Rows[0]["key1"].ToString(), key, IV, 3);
                    string strComputerIDFromFile = Cryptography.Decrypt(dsLicense.Tables[0].Rows[0]["key2"].ToString(), key, IV, 3);
                    string strComputerIDDirect = GetMaNM();
                    isLicense = ((strComputerIDFromFile == strComputerIDDirect) ? true : false);
                    if (isLicense)
                    {
                        isLicense = !CheckExpireDate(dsLicense);
                    }
                }
                catch (Exception)
                {
                    isLicense = false;
                }
            }
            return isLicense;
        }

        public string GetHardwareProfileEncrypted()
        {
            return Cryptography.Encrypt(GetHardwareProfile(), key, IV, 3);
        }

        public string GetMaNM()
        {
            string manm = Getstring();
            DataSet dsconfig = new DataSet();
            dsconfig.ReadXml(configFile);
            return dsconfig.Tables[0].Rows[0]["MaNM"].ToString();
        }

        private static string Getstring()
        {
            return "";
        }

        public string GetHardwareProfile()
        {
            Dictionary<string, string> retval = new Dictionary<string, string>
            {
                {
                    HardwareProfileComponents.ComputerModel.ToString(),
                    GetComputerModel()
                },
                {
                    HardwareProfileComponents.VolumeSerial.ToString(),
                    GetVolumeSerial()
                },
                {
                    HardwareProfileComponents.CpuId.ToString(),
                    GetCpuId()
                },
                {
                    HardwareProfileComponents.MemoryCapacity.ToString(),
                    GetMemoryAmount()
                }
            };
            string hardwareprofile = "";
            foreach (string item in retval.Values)
            {
                hardwareprofile += item;
            }
            return hardwareprofile;
        }

        private static string GetComputerModel()
        {
            ManagementObjectSearcher s1 = new ManagementObjectSearcher("select * from Win32_ComputerSystem");
            using (ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = s1.Get().GetEnumerator())
            {
                if (managementObjectEnumerator.MoveNext())
                {
                    ManagementObject oReturn = (ManagementObject)managementObjectEnumerator.Current;
                    return oReturn["Model"].ToString().Trim();
                }
            }
            return string.Empty;
        }

        private static string GetMemoryAmount()
        {
            ManagementObjectSearcher s1 = new ManagementObjectSearcher("select * from Win32_PhysicalMemory");
            using (ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = s1.Get().GetEnumerator())
            {
                if (managementObjectEnumerator.MoveNext())
                {
                    ManagementObject oReturn = (ManagementObject)managementObjectEnumerator.Current;
                    return oReturn["Capacity"].ToString().Trim();
                }
            }
            return string.Empty;
        }

        private static string GetVolumeSerial()
        {
            ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"c:\"");
            disk.Get();
            string volumeSerial = disk["VolumeSerialNumber"].ToString();
            disk.Dispose();
            return volumeSerial;
        }

        private static string GetCpuId()
        {
            ManagementClass managClass = new ManagementClass("win32_processor");
            ManagementObjectCollection managCollec = managClass.GetInstances();
            using (ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = managCollec.GetEnumerator())
            {
                if (managementObjectEnumerator.MoveNext())
                {
                    ManagementObject managObj = (ManagementObject)managementObjectEnumerator.Current;
                    return managObj.Properties["ProcessorID"].Value.ToString();
                }
            }
            return string.Empty;
        }
    }
}
