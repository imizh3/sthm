using System;
using System.IO;
using System.Windows.Forms;
using STHM;
using STHM.LIB;

namespace STHMServer
{
	public class TrialMaker
	{
		public enum RunTypes
		{
			Trial,
			Licensed,
			Expired,
			UnKnown,
			Freeware
		}
        private License license = new License();

		private string _BaseString;

		private string _BaseStringOld = "";

		private int _DefDays;

		private string _HideFilePath;

		private string _Identifier;

		private string _Pass1 = "";

		private string _Pass2 = "";

		private string _Pass3 = "";

		private string _Password;

		private string _RegFilePath;

		private int _Runed;

		private string _SoftName;

		private string _Text;

        private string _HardwareProfile;
		public string HideFilePath
		{
			get
			{
				return _HideFilePath;
			}
			set
			{
				_HideFilePath = value;
			}
		}

		public string RegFilePath
		{
			get
			{
				return _RegFilePath;
			}
			set
			{
				_RegFilePath = value;
			}
		}

		public byte[] TripleDESKey
		{
			get
			{
				return FileReadWrite.key;
			}
			set
			{
				FileReadWrite.key = value;
			}
		}

		public bool UseBaseBoardManufacturer
		{
			get
			{
				return SystemInfo.UseBiosManufacturer;
			}
			set
			{
				SystemInfo.UseBiosManufacturer = value;
			}
		}

		public bool UseBaseBoardProduct
		{
			get
			{
				return SystemInfo.UseBaseBoardProduct;
			}
			set
			{
				SystemInfo.UseBaseBoardProduct = value;
			}
		}

		public bool UseBiosManufacturer
		{
			get
			{
				return SystemInfo.UseBiosManufacturer;
			}
			set
			{
				SystemInfo.UseBiosManufacturer = value;
			}
		}

		public bool UseBiosVersion
		{
			get
			{
				return SystemInfo.UseBiosVersion;
			}
			set
			{
				SystemInfo.UseBiosVersion = value;
			}
		}

		public bool UseDiskDriveSignature
		{
			get
			{
				return SystemInfo.UseDiskDriveSignature;
			}
			set
			{
				SystemInfo.UseDiskDriveSignature = value;
			}
		}

		public bool UsePhysicalMediaSerialNumber
		{
			get
			{
				return SystemInfo.UsePhysicalMediaSerialNumber;
			}
			set
			{
				SystemInfo.UsePhysicalMediaSerialNumber = value;
			}
		}

		public bool UseProcessorID
		{
			get
			{
				return SystemInfo.UseProcessorID;
			}
			set
			{
				SystemInfo.UseProcessorID = value;
			}
		}

		public bool UseVideoControllerCaption
		{
			get
			{
				return SystemInfo.UseVideoControllerCaption;
			}
			set
			{
				SystemInfo.UseVideoControllerCaption = value;
			}
		}

		public bool UseWindowsSerialNumber
		{
			get
			{
				return SystemInfo.UseWindowsSerialNumber;
			}
			set
			{
				SystemInfo.UseWindowsSerialNumber = value;
			}
		}

		public TrialMaker(string SoftwareName, string RegFilePath, string HideFilePath, string Text, int TrialDays, int TrialRunTimes, string Identifier)
		{
			_SoftName = SoftwareName;
			_Identifier = Identifier;
			SetDefaults();
			_DefDays = TrialDays;
			_Runed = TrialRunTimes;
			_RegFilePath = RegFilePath;
			_HideFilePath = HideFilePath;
			_Text = Text;
		}

		private int CheckHideFile()
		{
			string[] strArray = FileReadWrite.ReadFile(_HideFilePath).Split(';');
			_BaseStringOld = strArray[3];
			if (strArray.Length == 5 && _BaseString == strArray[4])
			{
				if (Convert.ToInt32(strArray[1]) <= 0)
				{
					_DefDays = 0;
					return 0;
				}
				DateTime time = new DateTime(Convert.ToInt64(strArray[0]));
				if (time == DateTime.Now)
				{
					_DefDays = 0;
					return _DefDays;
				}
				long days2 = (DateTime.Now.Date - time.Date).Days;
				int num3 = Convert.ToInt32(strArray[1]);
				_Runed = Convert.ToInt32(strArray[2]);
				days2 = Math.Abs(days2);
				_DefDays = num3 - Convert.ToInt32(days2);
			}
			else if (strArray.Length == 4)
			{
				if (Convert.ToInt32(strArray[1]) <= 0)
				{
					_DefDays = 0;
					return 0;
				}
				DateTime time2 = new DateTime(Convert.ToInt64(strArray[0]));
				if (time2 == DateTime.Now)
				{
					_DefDays = 0;
					return _DefDays;
				}
				long days = (DateTime.Now.Date - time2.Date).Days;
				int num2 = Convert.ToInt32(strArray[1]);
				_Runed = Convert.ToInt32(strArray[2]);
				days = Math.Abs(days);
				_DefDays = num2 - Convert.ToInt32(days);
			}
			return _DefDays;
		}

		public bool CheckRegister()
		{
			string str = FileReadWrite.ReadFile(_RegFilePath);
			return _Password == str;
		}

		public int DaysToEnd()
		{
			FileInfo info = new FileInfo(_HideFilePath);
			if (!info.Exists)
			{
				MakeHideFile();
				return _DefDays;
			}
			return CheckHideFile();
		}

		private void MakeBaseString()
		{
            _BaseString = Encryption.Boring(Encryption.InverseByBase(SystemInfo.GetSystemInfo(_SoftName), 10));
		}

		private void MakeHideFile()
		{
			object obj2 = DateTime.Now.Ticks + ";";
			string data = string.Concat(obj2, _DefDays, ";", _Runed, ";", _BaseStringOld, ";", _BaseString);
			FileReadWrite.WriteFile(_HideFilePath, data);
		}

		private void MakePassword()
		{
			_Password = Encryption.MakePassword(_BaseString, _Identifier);
		}

		private void MakeRegFile()
		{
			FileReadWrite.WriteFile(_RegFilePath, _Password);
		}

		private string[] newsDays()
		{
			try
			{
				_Pass1 = Encryption.MakePassword(_BaseString, "2358777");
				_Pass2 = Encryption.MakePassword(_BaseString, "2345678");
				_Pass3 = Encryption.MakePassword(_BaseString, "34353637");
				return new string[3] { _Pass1, _Pass2, _Pass3 };
			}
			catch
			{
				return null;
			}
		}

		private void SetDefaults()
		{
			SystemInfo.UseBaseBoardManufacturer = false;
			SystemInfo.UseBaseBoardProduct = false;
			SystemInfo.UseBiosManufacturer = false;
			SystemInfo.UseBiosVersion = true;
			SystemInfo.UseDiskDriveSignature = true;
			SystemInfo.UsePhysicalMediaSerialNumber = false;
			SystemInfo.UseProcessorID = true;
			SystemInfo.UseVideoControllerCaption = false;
			SystemInfo.UseWindowsSerialNumber = false;
			SystemInfo.UseMac = false;
			MakeBaseString();
			MakePassword();
		}

		public RunTypes ShowDialog()
		{
			if (CheckRegister())
			{
				return RunTypes.Licensed;
			}
			string[] passMoreDays = newsDays();
			frm_License licence = new frm_License(_BaseString, _Password, DaysToEnd(), _Runed, _Text, passMoreDays);
            licence.ShowDialog();
			MakeHideFile();
			switch (licence.isLicensed)
			{
                case true:
				if (licence.type == RunTypes.Licensed)
				{
					MakeRegFile();
				}
				return licence.type;
                case false:
				_Runed = licence.NumRestore;
				_DefDays = licence.DaysRestore;
				MakeHideFile();
				return licence.type;
			default:
				return RunTypes.Expired;
			}
		}

		public int TrialPeriodDays()
		{
			return _DefDays;
		}
	}
}
