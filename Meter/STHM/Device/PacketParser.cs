using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace STHM.Device
{
	public static class PacketParser
	{
		public static Dictionary<int, char> m_arBaudrate2Id = new Dictionary<int, char>
		{
			{ 300, '0' },
			{ 600, '1' },
			{ 1200, '2' },
			{ 2400, '3' },
			{ 4800, '4' },
			{ 9600, '5' },
			{ 19200, '6' }
		};

		public static byte[] HexToByte(string msg)
		{
			msg = msg.Replace(" ", "");
			byte[] array = new byte[msg.Length / 2];
			for (int i = 0; i < msg.Length; i += 2)
			{
				array[i / 2] = Convert.ToByte(msg.Substring(i, 2), 16);
			}
			return array;
		}

		public static string ByteToHex(byte[] comByte, int leng = -1)
		{
			if (leng == -1)
			{
				leng = comByte.Length;
			}
			StringBuilder stringBuilder = new StringBuilder(leng * 3);
			for (int i = 0; i < leng; i++)
			{
				stringBuilder.Append(Convert.ToString(comByte[i], 16).PadLeft(2, '0').PadRight(3, ' '));
			}
			return stringBuilder.ToString().ToUpper();
		}

		public static string GetDataSetFromDataMsg(string datamsg, bool bBCCcheck = false)
		{
			try
			{
				int num = datamsg.LastIndexOf(')');
				if (num == -1)
				{
					return datamsg;
				}
				return datamsg.Substring(2, num - 2);
			}
			catch
			{
				return null;
			}
		}

		public static string ParseMeterSerial(string dataset)
		{
			dataset = GetDataSetFromDataMsg(dataset);
			byte[] bytes = HexToByte(dataset);
			return Encoding.ASCII.GetString(bytes);
		}

		public static string ParseUIF(string strvalue)
		{
			return strvalue.Substring(0, 14);
		}

		public static string ParseLoadProfileNumberValue(string value)
		{
			string text = value.Substring(0, 5);
			int num = value[5] - 48;
			switch (num)
			{
			default:
				try
				{
					return text.Insert(num - 1, ".");
				}
				catch (Exception)
				{
					return value;
				}
			case 7:
				return text + "0";
			case 0:
				return ".0" + text;
			}
		}

		public static DateTime parseUTCDateTime(string utcstrreverse)
		{
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0);
			DateTime dateTime2 = new DateTime(2012, 10, 31, 0, 0, 0);
			string text = "";
			for (int num = 3; num >= 0; num--)
			{
				text += utcstrreverse.Substring(num * 2, 2);
			}
			int num2 = int.Parse(text, NumberStyles.HexNumber);
			return dateTime.AddSeconds(num2);
		}

		public static string parseEventChangePeriod(string period)
		{
			string utcstrreverse = period.Substring(2, 8);
			string text = parseUTCDateTime(utcstrreverse).ToString("MM/dd/yyyy HH:mm:ss");
			return period.Substring(0, 2) + "," + text;
		}

		private static void chanelLoadprofile(string sscode, out int chanel)
		{
			int num = 0;
			switch (sscode)
			{
			case "033F":
				num = 8;
				break;
			case "1303":
				num = 4;
				break;
			case "0303":
				num = 4;
				break;
			case "0001":
				num = 1;
				break;
			}
			chanel = num;
		}

		public static void ParseLoadProfileData(StringBuilder outbuffer, StringBuilder DataBlock, int Day2Parse)
		{
			int num = -1;
			int chanel = 0;
			while (DataBlock.Length > 0)
			{
				string text = DataBlock.ToString(0, 2);
				if (text == "FF")
				{
					break;
				}
				if (text[0] == 'E')
				{
					if (text[1] == '4' || text[1] == '8')
					{
						if (text[1] == '4' && ++num == Day2Parse)
						{
							break;
						}
						string text2 = DataBlock.ToString(0, 16);
						DataBlock.Remove(0, 16);
						outbuffer.AppendLine(parseEventChangePeriod(text2) + "," + text2.Substring(10, 6) + ",");
						chanelLoadprofile(text2.Substring(10, 4), out chanel);
					}
					else
					{
						string period = DataBlock.ToString(0, 10);
						DataBlock.Remove(0, 10);
						outbuffer.AppendLine(parseEventChangePeriod(period) + ",");
					}
				}
				else
				{
					DataBlock.Remove(0, 2);
					for (int i = 0; i < chanel; i++)
					{
						string value = DataBlock.ToString(0, 6);
						DataBlock.Remove(0, 6);
						outbuffer.Append(ParseLoadProfileNumberValue(value) + ",");
					}
					outbuffer.AppendLine(text + ",");
				}
			}
		}

		public static void ParseLoadProfileData(StringBuilder outbuffer, StringBuilder DataBlock)
		{
			int chanel = 0;
			while (DataBlock.Length > 0)
			{
				string text = DataBlock.ToString(0, 2);
				if (text == "FF")
				{
					break;
				}
				if (text[0] == 'E')
				{
					if (text[1] == '4' || text[1] == '8')
					{
						string text2 = DataBlock.ToString(0, 16);
						DataBlock.Remove(0, 16);
						outbuffer.AppendLine(parseEventChangePeriod(text2) + "," + text2.Substring(10, 6) + ",");
						chanelLoadprofile(text2.Substring(10, 4), out chanel);
					}
					else
					{
						string period = DataBlock.ToString(0, 10);
						DataBlock.Remove(0, 10);
						outbuffer.AppendLine(parseEventChangePeriod(period) + ",");
					}
				}
				else
				{
					DataBlock.Remove(0, 2);
					for (int i = 0; i < chanel; i++)
					{
						string value = DataBlock.ToString(0, 6);
						DataBlock.Remove(0, 6);
						outbuffer.Append(ParseLoadProfileNumberValue(value) + ",");
					}
					outbuffer.AppendLine(text + ",");
				}
			}
		}

		public static string ParseRegisterValue(string reverse, int unit = 3)
		{
			int num = reverse.Length / 2;
			string text = "";
			for (int num2 = num; num2 > 0; num2--)
			{
				text += reverse.Substring((num2 - 1) * 2, 2);
			}
			if (text.Length > unit)
			{
				text = text.Insert(text.Length - unit, ".");
				if (Convert.ToDecimal(text) > 0m)
				{
					text = Convert.ToDecimal(text).ToString("### ### ### ###.######");
				}
			}
			return text;
		}

		public static void Export2CSV(string path, StringBuilder outbuffer, bool bOpen = false)
		{
			if (outbuffer.Length != 0)
			{
				if (File.Exists(path))
				{
					File.Delete(path);
				}
				File.WriteAllText(path, outbuffer.ToString());
				if (bOpen)
				{
					OpenFile(path);
				}
			}
		}

		public static void OpenFile(string filePath)
		{
			Process process = new Process();
			process.StartInfo.UseShellExecute = true;
			process.StartInfo.FileName = filePath;
			process.Start();
		}

		public static char BaudRate2ACKId(int baud)
		{
			char result = '0';
			if (m_arBaudrate2Id.ContainsKey(baud))
			{
				result = m_arBaudrate2Id[baud];
			}
			return result;
		}

		public static string StringDateTimeHexWrite(DateTime dtimeWrite)
		{
			string text = "";
			text = ((dtimeWrite.Second <= 9) ? ("0" + dtimeWrite.Second) : dtimeWrite.Second.ToString());
			text = ((dtimeWrite.Minute <= 9) ? (text + "0" + dtimeWrite.Minute) : (text + dtimeWrite.Minute));
			text = ((dtimeWrite.Hour <= 9) ? (text + "0" + dtimeWrite.Hour) : (text + dtimeWrite.Hour));
			int day = DateTime.Now.Day;
			text += day + 40;
			int month = DateTime.Now.Month;
			text += month + 80;
			if (DateTime.Now.Year > 2050)
			{
				return text + "00" + (DateTime.Now.Year - 1900);
			}
			return text + "00" + (DateTime.Now.Year - 2000);
		}

		public static DateTime Parse_Meter_Clock(string sMeterClock)
		{
			int second = Convert.ToInt32(sMeterClock.Substring(0, 2));
			int minute = Convert.ToInt32(sMeterClock.Substring(2, 2));
			int hour = Convert.ToInt32(sMeterClock.Substring(4, 2));
			string text = sMeterClock.Substring(6, 2);
			string text2 = sMeterClock.Substring(8, 2);
			int month = int.Parse(text2[0].ToString(), NumberStyles.HexNumber) % 2 * 10 + (text2[1] - 48);
			int num = Convert.ToInt32(sMeterClock.Substring(sMeterClock.Length - 2, 2));
			num = ((num <= 50) ? (num + 2000) : (num + 1900));
			int num2 = text[1] - 48;
			text = string.Format("{0}0", text[0]);
			int num3 = int.Parse(text, NumberStyles.HexNumber);
			string value = ((num3 - 64 * (num % 4)) / 16).ToString() + num2;
			int day = Convert.ToInt32(value);
			return new DateTime(num, month, day, hour, minute, second, 0);
		}

		public static void ParseInstrumentationProfileData(StringBuilder outbuffer, StringBuilder DataBlock)
		{
			int num = 7;
			while (DataBlock.Length > 0)
			{
				string text = DataBlock.ToString(0, 2);
				if (text == "FF")
				{
					break;
				}
				if (text[0] == 'E')
				{
					if (text[1] == '4' || text[1] == '8')
					{
						string text2 = DataBlock.ToString(0, 10);
						DataBlock.Remove(0, 10);
						string utcstrreverse = text2.Substring(2, 8);
						string text3 = parseUTCDateTime(utcstrreverse).ToString("MM/dd/yyyy HH:mm:ss");
						utcstrreverse = text2.Substring(0, 2) + "," + text3;
						outbuffer.AppendLine(utcstrreverse + ",030399,");
						text2 = DataBlock.ToString(0, 24);
						DataBlock.Remove(0, 24);
						if (text2 == "000000000000000000000000")
						{
							DataBlock.Remove(0, 56);
						}
						if (text2 == "001B002B004B000000000000")
						{
							num = 3;
							DataBlock.Remove(0, 20);
						}
						if (text2 == "001B002B004B001A002A004A")
						{
							num = 7;
							DataBlock.Remove(0, 20);
						}
					}
					else if (text[1] == 'F')
					{
						DataBlock.ToString(2, 74);
						DataBlock.Remove(0, 76);
					}
					else
					{
						string period = DataBlock.ToString(0, 10);
						DataBlock.Remove(0, 10);
						outbuffer.AppendLine(parseEventChangePeriod(period) + ",");
					}
				}
				else
				{
					DataBlock.Remove(0, 2);
					for (int i = 0; i < num; i++)
					{
						string value = DataBlock.ToString(0, 8);
						DataBlock.Remove(0, 8);
						outbuffer.Append(parseInstrumentationProfileNumberValue(value) + ",");
					}
					outbuffer.AppendLine(text + ",");
				}
			}
		}

		public static void ParseInstrumentationProfileData(StringBuilder outbuffer, StringBuilder DataBlock, int Day2Parse)
		{
			string text = "";
			int num = -1;
			int num2 = 0;
			while (DataBlock.Length > 0)
			{
				string text2 = DataBlock.ToString(0, 2);
				if (text2 == "FF")
				{
					break;
				}
				if (text2[0] == 'E')
				{
					if (text2[1] == '4' || text2[1] == '8')
					{
						if (text2[1] == '4' && ++num == Day2Parse)
						{
							break;
						}
						string text3 = DataBlock.ToString(0, 10);
						DataBlock.Remove(0, 10);
						text = text3.Substring(2, 8);
						string text4 = parseUTCDateTime(text).ToString("MM/dd/yyyy HH:mm:ss");
						text = text3.Substring(0, 2) + "," + text4;
						outbuffer.AppendLine(text + ",030399,");
						text3 = DataBlock.ToString(0, 34);
						DataBlock.Remove(0, 34);
						string text5 = "";
						for (int i = 2; i < 34; i += 4)
						{
							switch (text3.Substring(i, 4))
							{
							case "0330":
								num2++;
								text5 += "PF,";
								break;
							case "0D30":
								num2++;
								text5 += "KVAR,";
								break;
							case "4B30":
								num2++;
								text5 += "VC,";
								break;
							case "1A00":
								num2++;
								text5 += "IA,";
								break;
							case "4A00":
								num2++;
								text5 += "IC,";
								break;
							case "2B00":
								num2++;
								text5 += "VB,";
								break;
							case "4B00":
								num2++;
								text5 += "VC,";
								break;
							case "0C00":
								num2++;
								text5 += "KW,";
								break;
							case "0300":
								num2++;
								text5 += "PF,";
								break;
							case "1B00":
								num2++;
								text5 += "VA,";
								break;
							default:
								text5 += ",";
								break;
							case "2A00":
								num2++;
								text5 += "IB,";
								break;
							}
						}
						outbuffer.AppendLine(text5 + "Flags");
						DataBlock.Remove(0, 10);
					}
					else if (text2[1] == 'F')
					{
						DataBlock.ToString(2, 74);
						DataBlock.Remove(0, 76);
					}
					else
					{
						string period = DataBlock.ToString(0, 10);
						DataBlock.Remove(0, 10);
						outbuffer.AppendLine(parseEventChangePeriod(period) + ",");
					}
					continue;
				}
				DataBlock.Remove(0, 2);
				for (int j = 0; j < num2; j++)
				{
					try
					{
						text = DataBlock.ToString(0, 8);
					}
					catch (Exception)
					{
					}
					DataBlock.Remove(0, 8);
					outbuffer.Append(parseInstrumentationProfileNumberValue(text) + ",");
				}
				outbuffer.AppendLine(text2 + ",");
			}
		}

		public static string parseInstrumentationProfileNumberValue(string value)
		{
			string text = value.Substring(2, 6);
			string text2 = value.Substring(0, 2);
			try
			{
				if (text2[0] == '0' && text2 != "00")
				{
					int startIndex = 0x30 ^ text2[1];
					text = text.Insert(startIndex, ".");
				}
				else if (text2[0] != '8' || text2[1] == '0')
				{
					string text3;
					switch (text2)
					{
					default:
						text3 = text2 + "_" + text;
						break;
					case "80":
						text3 = text.Insert(0, "0.");
						break;
					case "40":
						text3 = text.Insert(0, "0.");
						break;
					case "A0":
						text3 = text.Insert(0, "-0.");
						break;
					case "10":
						text3 = text.Insert(0, "0.");
						break;
					case "00":
						text3 = text.Insert(0, "0.");
						break;
					}
					text = text3;
				}
				else
				{
					int startIndex2 = 0x30 ^ text2[1];
					text = "-" + text.Insert(startIndex2, ".");
				}
			}
			catch (Exception)
			{
			}
			return text;
		}
	}
}
