using System;
using System.Management;
using System.Net.NetworkInformation;

namespace STHMServer
{
	public static class SystemInfo
	{
		public static bool UseBaseBoardManufacturer;

		public static bool UseBaseBoardProduct;

		public static bool UseBiosManufacturer;

		public static bool UseBiosVersion;

		public static bool UseDiskDriveSignature;

		public static bool UseMac;

		public static bool UsePhysicalMediaSerialNumber;

		public static bool UseProcessorID;

		public static bool UseVideoControllerCaption;

		public static bool UseWindowsSerialNumber;

		public static string GetSystemInfo(string SoftwareName)
		{
			SoftwareName = "";
			if (UseProcessorID)
			{
				SoftwareName += RunQuery("Processor", "ProcessorId");
			}
			if (UseBaseBoardProduct)
			{
				SoftwareName += RunQuery("BaseBoard", "Product");
			}
			if (UseBaseBoardManufacturer)
			{
				SoftwareName += RunQuery("BaseBoard", "Manufacturer");
			}
			if (UseDiskDriveSignature)
			{
				SoftwareName += RunQuery("DiskDrive where InterfaceType != 'USB'", "Signature");
			}
			if (UseVideoControllerCaption)
			{
				SoftwareName += RunQuery("VideoController", "Caption");
			}
			if (UsePhysicalMediaSerialNumber)
			{
				SoftwareName += RunQuery("PhysicalMedia", "SerialNumber");
			}
			if (UseBiosVersion)
			{
				SoftwareName += RunQuery("BIOS", "Version");
			}
			if (UseWindowsSerialNumber)
			{
				SoftwareName += RunQuery("OperatingSystem", "SerialNumber");
			}
			if (UseMac)
			{
				SoftwareName += leerMACaddress();
			}
			SoftwareName = RemoveUseLess(SoftwareName);
			if (SoftwareName.Length < 25)
			{
				return SoftwareName.ToUpper();
			}
			return SoftwareName.ToUpper();
		}

		public static string leerMACaddress()
		{
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			string str = "";
			NetworkInterface[] array = allNetworkInterfaces;
			NetworkInterface[] array2 = array;
			foreach (NetworkInterface interface2 in array2)
			{
				interface2.GetIPProperties();
				byte[] addressBytes = interface2.GetPhysicalAddress().GetAddressBytes();
				for (int i = 0; i < addressBytes.Length; i++)
				{
					str += addressBytes[i].ToString("X2");
					if (i != addressBytes.Length - 1)
					{
						str += "-";
					}
				}
			}
			return str;
		}

		private static string RemoveUseLess(string st)
		{
			for (int i = st.Length - 1; i >= 0; i--)
			{
				char ch = char.ToUpper(st[i]);
				if ((ch < 'A' || ch > 'Z') && (ch < '0' || ch > '9'))
				{
					st = st.Remove(i, 1);
				}
			}
			return st;
		}

		private static string RunQuery(string TableName, string MethodName)
		{
			ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from Win32_" + TableName);
			foreach (ManagementObject obj2 in searcher.Get())
			{
				try
				{
					return obj2[MethodName].ToString();
				}
				catch (Exception)
				{
				}
			}
			return "";
		}
	}
}
