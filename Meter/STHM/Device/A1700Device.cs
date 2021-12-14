using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using STHM.Media;

namespace STHM.Device
{
	public class A1700Device : ESMeterDevice
	{
		private string m_A1700Version;

		private char BaudRate = '5';

		private string FirmwareVirsion = "002.03";

		private StringBuilder outbuffer = new StringBuilder();

		private Dictionary<string, string> m_arUnitRate;

		private string m_UnitRateMask;

		public string MeterID
		{
			get
			{
				return TaskPara.MeterID;
			}
		}

		public string OutStationNumber
		{
			get
			{
				return TaskPara.OutStationNumber;
			}
		}

		public string Password
		{
			get
			{
				return TaskPara.Password;
			}
		}

		public string TaskName
		{
			get
			{
				return TaskPara.Taskname;
			}
		}

		public int SetNumber
		{
			get
			{
				return TaskPara.SetNumber;
			}
			set
			{
				TaskPara.SetNumber = value;
			}
		}

		public DateTime StartDate
		{
			get
			{
				return TaskPara.StartDate;
			}
		}

		public bool IsFromDate
		{
			get
			{
				return TaskPara.IsFromDate;
			}
		}

		public A1700Device()
		{
			Init();
		}

		private void Init()
		{
			m_arTaskFunc.Add("ReadCummulativeTotalValues", ReadCummulativeTotalValues);
			m_arUnitRate = new Dictionary<string, string>(3);
			m_arUnitRate.Add("00", "No Source");
			m_arUnitRate.Add("01", "Import Wh");
			m_arUnitRate.Add("02", "Export Wh");
		}

		private string create_ACK_Request_Msg()
		{
			StringBuilder stringBuilder = new StringBuilder("01");
			stringBuilder.Insert(1, BaudRate);
			stringBuilder.Insert(0, '\u0006');
			stringBuilder.Append('\r');
			stringBuilder.Append('\n');
			return stringBuilder.ToString();
		}

		private string parse_ACK_Anser(string buffer)
		{
			int num = buffer.IndexOf('(');
			int num2 = buffer.LastIndexOf(')');
			string result = "";
			if (num > 0 && num2 > num)
			{
				result = buffer.Substring(num + 1, num2 - num - 1);
			}
			return result;
		}

		[DllImport("Encryptdll.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		private static extern int ENCRYPT([MarshalAs(UnmanagedType.LPArray)] byte[] EncryptedPassword, [MarshalAs(UnmanagedType.LPArray)] byte[] seed, [MarshalAs(UnmanagedType.LPArray)] byte[] passs);

		private string getEncryptPass(string seed, string pass)
		{
			ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
			byte[] bytes = aSCIIEncoding.GetBytes(seed);
			byte[] bytes2 = aSCIIEncoding.GetBytes(pass);
			byte[] array = new byte[16];
			ENCRYPT(array, bytes, bytes2);
			return aSCIIEncoding.GetString(array);
		}

		private string createACK_Answer_MSG(string epass)
		{
			StringBuilder stringBuilder = new StringBuilder("P2");
			stringBuilder.Insert(0, '\u0001');
			stringBuilder.Append('\u0002');
			stringBuilder.Append('(');
			stringBuilder.Append(epass);
			stringBuilder.Append(')');
			stringBuilder.Append('\u0003');
			char bCC = getBCC(stringBuilder.ToString());
			stringBuilder.Append(bCC);
			return stringBuilder.ToString();
		}

		private string create_Data_Command_MSG(string command, string code)
		{
			StringBuilder stringBuilder = new StringBuilder(command);
			stringBuilder.Insert(0, '\u0001');
			stringBuilder.Append('\u0002');
			stringBuilder.Append(code);
			stringBuilder.Append('\u0003');
			char bCC = getBCC(stringBuilder.ToString());
			stringBuilder.Append(bCC);
			return stringBuilder.ToString();
		}

		private char getBCC(string sData)
		{
			byte b = 0;
			byte[] bytes = Encoding.ASCII.GetBytes(sData);
			if (bytes != null && bytes.Length != 0)
			{
				for (int i = 1; i < bytes.Length; i++)
				{
					b = (byte)(b ^ bytes[i]);
				}
			}
			return (char)b;
		}

		private static DateTime convertFromUnixTimestamp(double timestamp)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timestamp);
		}

		private string create_Logoff_Meter_Msg()
		{
			StringBuilder stringBuilder = new StringBuilder("B0");
			stringBuilder.Insert(0, '\u0001');
			stringBuilder.Append('\u0003');
			stringBuilder.Append("q");
			return stringBuilder.ToString();
		}

		private bool parse_Identification_Msg(string idmsg)
		{
			if (idmsg.Length < 15)
			{
				return false;
			}
			BaudRate = idmsg[4];
			FirmwareVirsion = idmsg.Substring(10, 5);
			FirmwareVirsion = FirmwareVirsion.Insert(4, ".");
			m_A1700Version = "-A";
			detectVersion();
			return true;
		}

		private string detectVersion()
		{
			if (FirmwareVirsion.Substring(0, 3) == "030")
			{
				m_A1700Version = "-5";
			}
			else
			{
				m_A1700Version = "-A";
			}
			return m_A1700Version;
		}

		protected bool LogonMeter()
		{
			bool result = false;
			base.Media.AddLog(MediaLog.MessageType.Normal, "Log on");
			string text = "/?" + OutStationNumber + "!\r\n";
			string text2 = base.Media.SendMessage(text);
			text2 = text2.Replace("?", string.Empty);
			if (text2.Length == 0 || text2[0] == '\u0015')
			{
				base.Media.AddLog(MediaLog.MessageType.Error, "not answer: " + text);
			}
			else if (parse_Identification_Msg(text2))
			{
				string text3 = create_ACK_Request_Msg();
				text2 = base.Media.SendMessage(text3);
				if (text2.Length == 0 || text2[0] == '\u0015')
				{
					base.Media.AddLog(MediaLog.MessageType.Error, "not answer: " + text3);
				}
				else
				{
					string seed = parse_ACK_Anser(text2);
					string encryptPass = getEncryptPass(seed, Password);
					text3 = createACK_Answer_MSG("0000000000000000");
					text2 = base.Media.SendMessage(text3);
					if (text2.Length == 0)
					{
						base.Media.AddLog(MediaLog.MessageType.Error, "not answer: " + text3);
					}
					else if (text2[0] != '\u0006')
					{
						base.Media.AddLog(MediaLog.MessageType.Error, "Not ACK" + text3);
					}
					else
					{
						text3 = createACK_Answer_MSG(encryptPass);
						text2 = base.Media.SendMessage(text3);
						if (text2.Length == 0)
						{
							base.Media.AddLog(MediaLog.MessageType.Error, "not answer: " + text3);
						}
						else if (text2[0] != '\u0006')
						{
							base.Media.AddLog(MediaLog.MessageType.Error, "Not ACK" + text3);
						}
						else
						{
							base.Media.AddLog(MediaLog.MessageType.Warning, "ACK. correct password!");
							result = true;
						}
					}
				}
			}
			return result;
		}

		protected bool LogonMeterJob()
		{
			bool flag = false;
			int num = 0;
			while (num++ < 3 && !flag)
			{
				flag = LogonMeter();
			}
			if (flag)
			{
				string msg = create_Data_Command_MSG("R1", "798001(10)");
				string dataset = base.Media.SendMessage(msg);
				dataset = PacketParser.ParseMeterSerial(dataset);
				if (TaskPara.MeterID == "")
				{
					TaskPara.MeterID = dataset.Trim('-');
				}
			}
			else
			{
				WorkingStatus = EnumWorkingStatus.UnCompleted;
			}
			return flag;
		}

		private void ReadCummulativeTotal(StringBuilder outbuffer)
		{
			string msg = create_Data_Command_MSG("R1", "507001(40)");
			string dataSetFromDataMsg = PacketParser.GetDataSetFromDataMsg(base.Media.SendMessage(msg));
			msg = create_Data_Command_MSG("R1", "507002(10)");
			dataSetFromDataMsg += PacketParser.GetDataSetFromDataMsg(base.Media.SendMessage(msg));
			outbuffer.AppendLine("Cumulative totals,");
			outbuffer.AppendLine(",,,");
			outbuffer.AppendLine(",Value,Unit");
			string[] array = new string[10] { "Import Wh", "Export Wh", "Q1 varh", "Q2 varh", "Q3 varh", "Q4 varh", "VAh", "CD1", "CD2", "CD3" };
			for (int i = 0; i < 10; i++)
			{
				string reverse = dataSetFromDataMsg.Substring(i * 16, 16);
				outbuffer.AppendLine("," + PacketParser.ParseRegisterValue(reverse) + "," + array[i] + ",");
			}
		}

		private Dictionary<string, string> ReadCummulativeTotal()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string msg = create_Data_Command_MSG("R1", "507001(40)");
			string dataSetFromDataMsg = PacketParser.GetDataSetFromDataMsg(base.Media.SendMessage(msg));
			msg = create_Data_Command_MSG("R1", "507002(10)");
			dataSetFromDataMsg += PacketParser.GetDataSetFromDataMsg(base.Media.SendMessage(msg));
			string[] array = new string[10] { "ImportWh", "ExportWh", "Q1varh", "Q2varh", "Q3varh", "Q4varh", "VAh", "CD1", "CD2", "CD3" };
			for (int i = 0; i < 10; i++)
			{
				string reverse = dataSetFromDataMsg.Substring(i * 16, 16);
				dictionary.Add(array[i], PacketParser.ParseRegisterValue(reverse));
			}
			return dictionary;
		}

		private void ReadRateValues(StringBuilder outbuffer)
		{
			outbuffer.AppendLine("Rates,,");
			outbuffer.AppendLine(",,,");
			outbuffer.AppendLine("Register,Value,Unit,");
			Dictionary<string, string> dictionary = new Dictionary<string, string>(3);
			dictionary.Add("00", "No Source");
			dictionary.Add("01", "Import Wh");
			dictionary.Add("02", "Export Wh");
			int num = 16;
			if (m_A1700Version == "-5")
			{
				num = 32;
			}
			string code = string.Format("667001({0:x})", num);
			code = create_Data_Command_MSG("R1", code);
			string dataSetFromDataMsg = PacketParser.GetDataSetFromDataMsg(base.Media.SendMessage(code));
			int num2 = num / 8;
			int num3 = 5275648;
			StringBuilder stringBuilder = new StringBuilder(num2 * 128);
			string datamsg;
			for (int i = 1; i <= num2; i++)
			{
				int num4 = ++num3;
				code = num4.ToString("X") + "(40)";
				code = create_Data_Command_MSG("R1", code);
				datamsg = base.Media.SendMessage(code);
				stringBuilder.Append(PacketParser.GetDataSetFromDataMsg(datamsg));
			}
			datamsg = stringBuilder.ToString();
			for (int j = 1; j <= num; j++)
			{
				string reverse = datamsg.Substring((j - 1) * 16, 16);
				try
				{
					outbuffer.AppendLine(j + "," + PacketParser.ParseRegisterValue(reverse) + "," + dictionary[dataSetFromDataMsg.Substring((j - 1) * 2, 2)] + ",");
				}
				catch (Exception)
				{
					outbuffer.AppendLine(j + "," + PacketParser.ParseRegisterValue(reverse) + "," + dataSetFromDataMsg.Substring((j - 1) * 2, 2) + ",");
				}
			}
		}

		private void ReadMURegister(StringBuilder outbuffer)
		{
			outbuffer.AppendLine("Multi-utility registers,,,");
			outbuffer.AppendLine(",,,");
			outbuffer.AppendLine(",Value,Unit,");
			string msg = create_Data_Command_MSG("R1", "516001(20)");
			string dataSetFromDataMsg = PacketParser.GetDataSetFromDataMsg(base.Media.SendMessage(msg));
			for (int i = 1; i <= 4; i++)
			{
				string reverse = dataSetFromDataMsg.Substring((i - 1) * 16, 16);
				outbuffer.AppendLine("," + PacketParser.ParseRegisterValue(reverse) + ",MU " + i);
			}
		}

		private void ReadCummulativeMaximumDemands(StringBuilder outbuffer)
		{
			outbuffer.AppendLine("Cummulative Maximum demands,,,");
			outbuffer.AppendLine(",,,");
			outbuffer.AppendLine("Register,Value,Unit,");
			Dictionary<string, string> dictionary = new Dictionary<string, string>(3);
			dictionary.Add("00", "No Source");
			dictionary.Add("01", "Import W");
			dictionary.Add("02", "Export W");
			string msg = create_Data_Command_MSG("R1", "509001(40)");
			string dataSetFromDataMsg = PacketParser.GetDataSetFromDataMsg(base.Media.SendMessage(msg));
			msg = create_Data_Command_MSG("R1", "509002(08)");
			dataSetFromDataMsg += PacketParser.GetDataSetFromDataMsg(base.Media.SendMessage(msg));
			for (int i = 1; i <= 8; i++)
			{
				string reverse = dataSetFromDataMsg.Substring((i - 1) * 18, 16);
				string text = dataSetFromDataMsg.Substring((i - 1) * 18 + 16, 2);
				try
				{
					outbuffer.AppendLine(i + "," + PacketParser.ParseRegisterValue(reverse) + "," + dictionary[text] + ",");
				}
				catch (Exception)
				{
					outbuffer.AppendLine(i + "," + PacketParser.ParseRegisterValue(reverse) + "," + text + ",");
				}
			}
		}

		private string ReadPhaseABC(string codeW1, string codeR1, string codeR1END)
		{
			string msg = create_Data_Command_MSG("W1", codeW1);
			string dataSetFromDataMsg = PacketParser.GetDataSetFromDataMsg(base.Media.SendMessage(msg));
			for (int i = 0; i < i + 1; i++)
			{
				msg = create_Data_Command_MSG("R1", codeR1);
				dataSetFromDataMsg = PacketParser.GetDataSetFromDataMsg(base.Media.SendMessage(msg));
				int startIndex = dataSetFromDataMsg.Length - 2;
				string text = dataSetFromDataMsg.Substring(startIndex, 1);
				if (text.ToString() == "8")
				{
					break;
				}
			}
			msg = create_Data_Command_MSG("R1", codeR1END);
			return PacketParser.GetDataSetFromDataMsg(base.Media.SendMessage(msg));
		}

		private List<string> ParsePhaseABCTotal(string outstrRead)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < outstrRead.Length; i += 14)
			{
				string text = outstrRead.Substring(i, 14);
				if (text.ToString() == "00000000000000")
				{
					list.Add("0.00");
				}
				else if (text.Substring(0, 4).ToString() == "00FF")
				{
					list.Add("Not available");
				}
				else if (text.Substring(0, 2).ToString() == "80")
				{
					text = text.Substring(2, 12);
					list.Add("-" + text.Insert(8, ".").ToString());
				}
				else if (text.Substring(0, 2).ToString() == "01")
				{
					text = text.Substring(2, 12);
					list.Add(text.Insert(8, ".").ToString());
				}
				else if (text.Substring(0, 2).ToString() == "04")
				{
					text = text.Substring(2, 12);
					list.Add(text.Insert(8, ".").ToString());
				}
				else if (text.Substring(0, 2).ToString() == "03")
				{
					text = text.Substring(2, 12);
					list.Add(text.Insert(8, ".").ToString());
				}
				else if (text.Substring(0, 2).ToString() == "02")
				{
					text = text.Substring(2, 12);
					list.Add(text.Insert(8, ".").ToString());
				}
				else
				{
					list.Add(text.Insert(10, ".").ToString());
				}
			}
			return list;
		}

		public void ReadCurrentCSTUCTHOI_VCGM(StringBuilder outbuffer)
		{
			string outstrRead = ReadPhaseABC("605001(1C2C4C0C)", "605001(04)", "606001(1C)");
			List<string> list = ParsePhaseABCTotal(outstrRead);
			outbuffer.AppendLine("," + list[3] + ",");
		}

		public void ReadCurrentUIF(StringBuilder outbuffer)
		{
			outbuffer.AppendLine("Instantaneous Value,");
			outbuffer.AppendLine(",,,");
			outbuffer.AppendLine(",Phase A, Phase B, Phase C, Total");
			string outstrRead = ReadPhaseABC("605001(1A2A4A00)", "605001(04)", "606001(1C)");
			List<string> list = ParsePhaseABCTotal(outstrRead);
			outbuffer.AppendLine("," + list[0] + "," + list[1] + "," + list[2] + ",,Amps,");
			outstrRead = ReadPhaseABC("605001(1B2B4B00)", "605001(04)", "606001(1C)");
			list = ParsePhaseABCTotal(outstrRead);
			outbuffer.AppendLine("," + list[0] + "," + list[1] + "," + list[2] + ",,Volts,");
			outstrRead = ReadPhaseABC("605001(1C2C4C0C)", "605001(04)", "606001(1C)");
			list = ParsePhaseABCTotal(outstrRead);
			outbuffer.AppendLine("," + list[0] + "," + list[1] + "," + list[2] + "," + list[3] + ",Active Power(kW),");
			outstrRead = ReadPhaseABC("605001(1D2D4D0D)", "605001(04)", "606001(1C)");
			list = ParsePhaseABCTotal(outstrRead);
			outbuffer.AppendLine("," + list[0] + "," + list[1] + "," + list[2] + "," + list[3] + ",Reactive Power(kvar),");
			outstrRead = ReadPhaseABC("605001(1E2E4E0E)", "605001(04)", "606001(1C)");
			list = ParsePhaseABCTotal(outstrRead);
			outbuffer.AppendLine("," + list[0] + "," + list[1] + "," + list[2] + "," + list[3] + ",Apparent Power(kVA),");
			outstrRead = ReadPhaseABC("605001(13234303)", "605001(04)", "606001(1C)");
			list = ParsePhaseABCTotal(outstrRead);
			outbuffer.AppendLine("," + list[0] + "," + list[1] + "," + list[2] + "," + list[3] + ",Power factor,");
			outstrRead = ReadPhaseABC("605001(18284800)", "605001(04)", "606001(1C)");
			list = ParsePhaseABCTotal(outstrRead);
			outbuffer.AppendLine("," + list[0] + "," + list[1] + "," + list[2] + ",,Frequency(Hz),");
			outstrRead = ReadPhaseABC("605001(19294900)", "605001(04)", "606001(1C)");
			list = ParsePhaseABCTotal(outstrRead);
			outbuffer.AppendLine("," + list[0] + "," + list[1] + "," + list[2] + ",,Phase Angle(deg),");
			outstrRead = ReadPhaseABC("605001(07000000)", "605001(04)", "606001(1C)");
			string text = outstrRead.Substring(0, 14);
			if (text.Substring(0, 4).ToString() == "00FF")
			{
				outbuffer.AppendLine(",,,,Non available,Phase Rotation,");
			}
			else if (text.ToString() == "00000000000001")
			{
				outbuffer.AppendLine(",,,,A->B->C,Phase Rotation,");
			}
			else if (text.ToString() == "00000000000000")
			{
				outbuffer.AppendLine(",,,,Non available,Phase Rotation,");
			}
		}

		protected void LogoffMeter()
		{
			string msg = create_Logoff_Meter_Msg();
			base.Media.PostMessage(msg);
			base.Media.AddLog(MediaLog.MessageType.Normal, msg);
			base.Media.AddLog(MediaLog.MessageType.Normal, "Logoff");
		}

		public override void ReadCurrentRegisterValues(TaskInfo para = null)
		{
			if (para != null)
			{
				TaskPara = para;
			}
			if (LogonMeterJob())
			{
				DateTime dateTime = ReadMeterClock();
				outbuffer.Length = 0;
				outbuffer.AppendLine("STHM Meter Reading,");
				outbuffer.AppendLine("From unit, " + MeterID + "," + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + "," + dateTime.ToString("MM/dd/yyyy HH:mm:ss") + ",");
				if (!TaskPara.checkCurrentValuesVCGM)
				{
					ReadCummulativeTotal(outbuffer);
					ReadRateValues(outbuffer);
					ReadMURegister(outbuffer);
					ReadCummulativeMaximumDemands(outbuffer);
					ReadCurrentUIF(outbuffer);
				}
				else
				{
					outbuffer.Length = 0;
					outbuffer.AppendLine(", " + MeterID + "," + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + "," + dateTime.ToString("MM/dd/yyyy HH:mm:ss") + ",");
					ReadCurrentCSTUCTHOI_VCGM(outbuffer);
				}
				LogoffMeter();
				WorkingStatus = EnumWorkingStatus.Completed;
			}
		}

		public override void ReadLoadProfile(TaskInfo para = null)
		{
			if (para != null)
			{
				TaskPara = para;
			}
			int setNumber = SetNumber;
			if (IsFromDate)
			{
				setNumber = DateTime.Now.Subtract(StartDate).Days + 1;
				if (setNumber <= 0)
				{
					setNumber = 1;
					SetNumber = 1;
				}
				ReadLoadProfile(setNumber, SetNumber);
			}
			else
			{
				ReadLoadProfile(SetNumber);
			}
		}

		public override void ReadLoadProfile_A0(TaskInfo para = null)
		{
			if (para != null)
			{
				TaskPara = para;
			}
			int setNumber = SetNumber;
			if (IsFromDate)
			{
				setNumber = DateTime.Now.Subtract(StartDate).Days + 1;
				if (setNumber <= 0)
				{
					setNumber = 1;
					SetNumber = 1;
				}
				ReadLoadProfile(setNumber, SetNumber);
			}
			else
			{
				ReadLoadProfile(SetNumber);
			}
		}

		private void ReadHistoricalRegisterValuesSet(int iset, StringBuilder outbuffer)
		{
			int num = 5517313;
			int num2 = 0;
			string text = "(27)";
			int num3 = 16;
			if (m_A1700Version == "-A")
			{
				num2 = 10;
				text = "(27)";
			}
			else if (m_A1700Version == "-5")
			{
				num2 = 14;
				text = "(1F)";
				num3 = 32;
			}
			StringBuilder stringBuilder = new StringBuilder(num2 * 128);
			num = 5517312 + (iset - 1) * num2;
			int num4;
			string code;
			string datamsg;
			for (int i = 1; i < num2; i++)
			{
				num4 = ++num;
				code = num4.ToString("X") + "(40)";
				code = create_Data_Command_MSG("R1", code);
				datamsg = base.Media.SendMessage(code);
				stringBuilder.Append(PacketParser.GetDataSetFromDataMsg(datamsg));
			}
			num4 = ++num;
			code = num4.ToString("X") + text;
			code = create_Data_Command_MSG("R1", code);
			datamsg = base.Media.SendMessage(code);
			stringBuilder.Append(PacketParser.GetDataSetFromDataMsg(datamsg));
			datamsg = stringBuilder.ToString();
			outbuffer.AppendLine("Historical data set:, " + iset + ",");
			outbuffer.AppendLine(",,,");
			outbuffer.AppendLine("Cumulative totals,");
			outbuffer.AppendLine(",,,");
			outbuffer.AppendLine(",Value,Unit");
			string[] array = new string[10] { "Import Wh", "Export Wh", "Q1 varh", "Q2 varh", "Q3 varh", "Q4 varh", "VAh", "CD1", "CD2", "CD3" };
			int num5 = 0;
			for (int j = 0; j < 10; j++)
			{
				string reverse = datamsg.Substring(num5, 16);
				outbuffer.AppendLine("," + PacketParser.ParseRegisterValue(reverse) + "," + array[j] + ",");
				num5 += 16;
			}
			outbuffer.AppendLine("Rates,,");
			outbuffer.AppendLine(",,,");
			outbuffer.AppendLine("Register,Value,Unit,");
			for (int k = 1; k <= num3; k++)
			{
				string reverse2 = datamsg.Substring(num5, 16);
				outbuffer.AppendLine(k + "," + PacketParser.ParseRegisterValue(reverse2) + "," + m_arUnitRate[m_UnitRateMask.Substring((k - 1) * 2, 2)] + ",");
				num5 += 16;
			}
			outbuffer.AppendLine("Multi-utility registers,,,");
			outbuffer.AppendLine(",,,");
			outbuffer.AppendLine(",Value,Unit,");
			for (int l = 1; l <= 4; l++)
			{
				string reverse3 = datamsg.Substring(num5, 16);
				num5 += 16;
				outbuffer.AppendLine("," + PacketParser.ParseRegisterValue(reverse3) + ",MU " + l);
			}
			outbuffer.AppendLine("Cummulative Maximum demands,,,");
			outbuffer.AppendLine(",,,");
			outbuffer.AppendLine("Register,Value,Unit,");
			Dictionary<string, string> dictionary = new Dictionary<string, string>(3);
			dictionary.Add("00", "No Source");
			dictionary.Add("01", "Import W");
			dictionary.Add("02", "Export W");
			for (int m = 1; m <= 8; m++)
			{
				string reverse4 = datamsg.Substring(num5, 16);
				string text2 = datamsg.Substring(num5 + 16, 2);
				num5 += 18;
				try
				{
					outbuffer.AppendLine(m + "," + PacketParser.ParseRegisterValue(reverse4) + "," + dictionary[text2] + ",");
				}
				catch (Exception)
				{
					outbuffer.AppendLine(m + "," + PacketParser.ParseRegisterValue(reverse4) + "," + text2 + ",");
				}
			}
			num5 = datamsg.Length - 30;
			outbuffer.AppendLine("Billing event details,");
			outbuffer.AppendLine(",,,");
			string s = datamsg.Substring(num5, 2);
			num5 += 2;
			outbuffer.AppendLine("Billing reset number:," + int.Parse(s, NumberStyles.HexNumber) + ",,");
			num5 += 2;
			s = datamsg.Substring(num5, 8);
			num5 += 8;
			DateTime dateTime = PacketParser.parseUTCDateTime(s);
			s = datamsg.Substring(num5, 8);
			num5 += 8;
			DateTime dateTime2 = PacketParser.parseUTCDateTime(s);
			num5 += 2;
			s = datamsg.Substring(num5, 8);
			num5 += 8;
			outbuffer.AppendLine("Time of billing reset:," + PacketParser.parseUTCDateTime(s).ToString("MM/dd/yyyy HH:mm:ss") + ",,");
			outbuffer.AppendLine("Billing period start date," + dateTime.ToString("MM/dd/yyyy HH:mm:ss") + ",,");
			outbuffer.AppendLine("Billing period end date," + dateTime2.ToString("MM/dd/yyyy HH:mm:ss") + ",,");
			outbuffer.AppendLine(",,,,");
			WorkingStatus = EnumWorkingStatus.Completed;
		}

		public override void ReadHistoricalRegister(TaskInfo para = null)
		{
			if (!LogonMeterJob())
			{
				return;
			}
			create_Export_Header();
			outbuffer.AppendLine("Historical Data,");
			int num = 16;
			if (m_A1700Version == "-5")
			{
				num = 32;
			}
			string code = string.Format("667001({0:x})", num);
			code = create_Data_Command_MSG("R1", code);
			m_UnitRateMask = PacketParser.GetDataSetFromDataMsg(base.Media.SendMessage(code));
			if (SetNumber == -1)
			{
				SetNumber = 12;
				if (m_A1700Version == "-5")
				{
					SetNumber = 36;
				}
			}
			ReadHistoricalRegisterValues(SetNumber);
		}

		private void ReadHistoricalRegisterValues(int setcount)
		{
			for (int i = 1; i <= setcount; i++)
			{
				ReadHistoricalRegisterValuesSet(i, outbuffer);
			}
			LogoffMeter();
			WorkingStatus = EnumWorkingStatus.Completed;
		}

		private void ReadLoadProfile(int daycount)
		{
			if (!LogonMeterJob())
			{
				return;
			}
			outbuffer.Length = 0;
			outbuffer.AppendLine("STHM Meter Reading,");
			outbuffer.AppendLine("From unit, " + MeterID + "," + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ",");
			StringBuilder stringBuilder = null;
			string text = daycount.ToString("X").PadLeft(4, '0');
			text = text.Substring(2, 2) + text.Substring(0, 2);
			text = "551001(" + text + ")";
			text = create_Data_Command_MSG("W1", text);
			string text2 = base.Media.SendMessage(text);
			if (text2.Length != 0 && text2[0] == '\u0006')
			{
				text = create_Data_Command_MSG("R1", "551001(02)");
				text2 = base.Media.SendMessage(text);
				int num = int.Parse(PacketParser.GetDataSetFromDataMsg(text2), NumberStyles.HexNumber);
				int num2 = 5570560;
				stringBuilder = new StringBuilder(num * 128);
				for (int i = 1; i <= num; i++)
				{
					int num3 = ++num2;
					text = num3.ToString("X") + "(40)";
					text = create_Data_Command_MSG("R1", text);
					text2 = base.Media.SendMessage(text);
					stringBuilder.Append(PacketParser.GetDataSetFromDataMsg(text2));
					Thread.Sleep(50);
				}
			}
			LogoffMeter();
			if (stringBuilder != null)
			{
				PacketParser.ParseLoadProfileData(outbuffer, stringBuilder);
			}
			WorkingStatus = EnumWorkingStatus.Completed;
		}

		private void ReadLoadProfile(int dayfromnow, int daycount)
		{
			if (!LogonMeterJob())
			{
				return;
			}
			outbuffer.Length = 0;
			outbuffer.AppendLine("STHM Meter Reading,");
			outbuffer.AppendLine("From unit, " + MeterID + "," + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ",");
			StringBuilder stringBuilder = null;
			string text = dayfromnow.ToString("X").PadLeft(4, '0');
			text = text.Substring(2, 2) + text.Substring(0, 2);
			text = "551001(" + text + ")";
			text = create_Data_Command_MSG("W1", text);
			string text2 = base.Media.SendMessage(text);
			if (text2.Length == 0)
			{
				base.Media.AddLog(MediaLog.MessageType.Error, "not answer: " + text);
			}
			else if (text2[0] != '\u0006')
			{
				base.Media.AddLog(MediaLog.MessageType.Error, "Not ACK" + text);
			}
			else
			{
				text = create_Data_Command_MSG("R1", "551001(02)");
				text2 = base.Media.SendMessage(text);
				int num = int.Parse(PacketParser.GetDataSetFromDataMsg(text2), NumberStyles.HexNumber);
				int num2 = 5570560;
				stringBuilder = new StringBuilder(num * 128);
				for (int i = 1; i <= num; i++)
				{
					int num3 = ++num2;
					text = num3.ToString("X") + "(40)";
					text = create_Data_Command_MSG("R1", text);
					text2 = base.Media.SendMessage(text);
					text2 = PacketParser.GetDataSetFromDataMsg(text2);
					stringBuilder.Append(text2);
					Thread.Sleep(50);
				}
			}
			LogoffMeter();
			if (stringBuilder != null)
			{
				PacketParser.ParseLoadProfileData(outbuffer, stringBuilder, dayfromnow);
			}
			WorkingStatus = EnumWorkingStatus.Completed;
		}

		public DateTime ReadMeterClock()
		{
			string msg = create_Data_Command_MSG("R1", "861001(07)");
			string datamsg = base.Media.SendMessage(msg);
			datamsg = PacketParser.GetDataSetFromDataMsg(datamsg);
			DateTime result;
			try
			{
				return PacketParser.Parse_Meter_Clock(datamsg);
			}
			catch (Exception)
			{
				result = new DateTime(0L);
			}
			return result;
		}

		protected void create_Export_Header()
		{
			outbuffer.Length = 0;
			outbuffer.AppendLine("STHM Meter Reading,");
			outbuffer.AppendLine("From unit, " + MeterID + "," + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ",,");
		}

		public void ReadCummulativeTotalValues(TaskInfo para = null)
		{
			if (para != null)
			{
				TaskPara = para;
			}
			if (LogonMeterJob())
			{
				create_Export_Header();
				ReadCummulativeTotal(outbuffer);
				LogoffMeter();
				WorkingStatus = EnumWorkingStatus.Completed;
			}
		}

		public CumulativeTotalsData ReadCummulativeTotalData(TaskInfo para = null)
		{
			if (para != null)
			{
				TaskPara = para;
			}
			CumulativeTotalsData result = null;
			if (!LogonMeterJob())
			{
				return result;
			}
			DateTime meterclock = ReadMeterClock();
			result = new CumulativeTotalsData(MeterID, DateTime.Now);
			result.meterclock = meterclock;
			result.dictData = ReadCummulativeTotal();
			LogoffMeter();
			return result;
		}

		public override void ReadInstrumentationProfile(TaskInfo para = null)
		{
			if (para != null)
			{
				TaskPara = para;
			}
			int setNumber = SetNumber;
			if (IsFromDate)
			{
				setNumber = DateTime.Now.Subtract(StartDate).Days + 1;
				ReadInstrumentationProfile(setNumber, SetNumber);
			}
			else
			{
				ReadInstrumentationProfile(SetNumber);
			}
		}

		private void ReadInstrumentationProfile(int daycount)
		{
			if (!LogonMeterJob() || m_A1700Version == "-A")
			{
				return;
			}
			create_Export_Header();
			StringBuilder stringBuilder = null;
			string text = daycount.ToString("X").PadLeft(4, '0');
			text = text.Substring(2, 2) + text.Substring(0, 2);
			text = "556001(" + text + ")";
			text = create_Data_Command_MSG("W1", text);
			string text2 = base.Media.SendMessage(text);
			if (text2.Length != 0 && text2[0] == '\u0006')
			{
				text = create_Data_Command_MSG("R1", "556001(02)");
				text2 = base.Media.SendMessage(text);
				int num = int.Parse(PacketParser.GetDataSetFromDataMsg(text2), NumberStyles.HexNumber);
				int num2 = 5591040;
				stringBuilder = new StringBuilder(num * 128);
				for (int i = 1; i <= num; i++)
				{
					int num3 = ++num2;
					text = num3.ToString("X") + "(40)";
					text = create_Data_Command_MSG("R1", text);
					text2 = base.Media.SendMessage(text);
					stringBuilder.Append(PacketParser.GetDataSetFromDataMsg(text2));
					Thread.Sleep(50);
				}
			}
			LogoffMeter();
			if (stringBuilder != null)
			{
				PacketParser.ParseInstrumentationProfileData(outbuffer, stringBuilder);
			}
			WorkingStatus = EnumWorkingStatus.Completed;
		}

		private void ReadInstrumentationProfile(int dayfromnow, int daycount)
		{
			if (!LogonMeterJob())
			{
				return;
			}
			if (m_A1700Version == "-A")
			{
				LogoffMeter();
				WorkingStatus = EnumWorkingStatus.Completed;
				return;
			}
			create_Export_Header();
			StringBuilder stringBuilder = null;
			string text = dayfromnow.ToString("X").PadLeft(4, '0');
			text = text.Substring(2, 2) + text.Substring(0, 2);
			text = "556001(" + text + ")";
			text = create_Data_Command_MSG("W1", text);
			string text2 = base.Media.SendMessage(text);
			if (text2.Length == 0)
			{
				base.Media.AddLog(MediaLog.MessageType.Error, "not answer: " + text);
			}
			else if (text2[0] != '\u0006')
			{
				base.Media.AddLog(MediaLog.MessageType.Error, "Not ACK" + text);
			}
			else
			{
				text = create_Data_Command_MSG("R1", "556001(02)");
				text2 = base.Media.SendMessage(text);
				int num = int.Parse(PacketParser.GetDataSetFromDataMsg(text2), NumberStyles.HexNumber);
				int num2 = 5591040;
				stringBuilder = new StringBuilder(num * 128);
				int num3 = 0;
				for (int i = 1; i <= num; i++)
				{
					int num4 = ++num2;
					text = num4.ToString("X") + "(40)";
					text = create_Data_Command_MSG("R1", text);
					text2 = base.Media.SendMessage(text);
					text2 = PacketParser.GetDataSetFromDataMsg(text2);
					stringBuilder.Append(text2);
					if (text2.IndexOf("E4") >= 0)
					{
						num3++;
						if (num3 == daycount + 1)
						{
							break;
						}
					}
					Thread.Sleep(50);
				}
			}
			LogoffMeter();
			if (stringBuilder != null)
			{
				PacketParser.ParseInstrumentationProfileData(outbuffer, stringBuilder, daycount);
			}
			WorkingStatus = EnumWorkingStatus.Completed;
		}

		public override void ReadEventLog(TaskInfo para = null)
		{
			throw new NotImplementedException();
		}

		public override void WriteSyncDateTime(TaskInfo para = null)
		{
			if (para != null)
			{
				TaskPara = para;
			}
			para = TaskPara;
			WriteSyncDateTime(PacketParser.StringDateTimeHexWrite(para.StartDate));
		}

		private void WriteSyncDateTime(string dtimeString)
		{
			if (LogonMeterJob())
			{
				string msg = create_Data_Command_MSG("R1", "795001(08)");
				msg = base.Media.SendMessage(msg);
				dtimeString = "861001(" + dtimeString + ")";
				dtimeString = create_Data_Command_MSG("W1", dtimeString);
				msg = base.Media.SendMessage(dtimeString);
				LogoffMeter();
				WorkingStatus = EnumWorkingStatus.Completed;
			}
		}

		public override void Export2CSV(string pathSchedule, string pathmanual, string exportcompany, bool CheckpathManual, bool bOpen = false)
		{
			Export2CSV(pathSchedule, pathmanual, exportcompany, CheckpathManual, outbuffer.ToString(), bOpen);
		}
	}
}
