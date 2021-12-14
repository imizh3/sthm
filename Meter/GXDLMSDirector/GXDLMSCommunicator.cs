#define DEBUG
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using Gurux.Common;
using Gurux.DLMS;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.DLMS.Objects;
using Gurux.Net;
using Gurux.Serial;
using GXDLMS.Common;
using GXDLMS.ManufacturerSettings;
using STHM.Device;
using STHM.Media;

namespace GXDLMSDirector
{
    internal class Password
    {
        public static string Key = "Gurux Ltd.";
    }
    public delegate void ReadEventHandler(Gurux.DLMS.Objects.GXDLMSObject sender, int index, string nametask, int set, DateTime starttime);
    public delegate void StatusEventHandler(object sender, DeviceState status);
    public delegate void ProgressEventHandler(object sender, string description, int current, int maximium);
    public delegate void DataReceivedEventHandler(object sender, byte[] data, int set, DateTime starttime);
	public class GXDLMSCommunicator
	{
		private class GXAttributeRead
		{
			public PropertyInfo Info;

			public GXDLMSAttribute Attribute;

			public GXAttributeRead(PropertyInfo info, GXDLMSAttribute attribute)
			{
				Info = info;
				Attribute = attribute;
			}
		}

		private delegate void ClearProfileGenericDataEventHandler();

		public ESMedia m_ESMedia;

		public bool m_bESMediaType = false;

		private TaskInfo tf;

		private StringBuilder outdataString = new StringBuilder();

		internal DateTime LastTransaction = DateTime.MinValue;

		internal DateTime ConnectionStartTime;

		internal GXDLMSDevice Parent;

		public IGXMedia Media = null;

		internal GXDLMSClient m_Cosem;

		[XmlIgnore]
		internal Dictionary<GXDLMSObject, GXDLMSObjectCollection> DeviceColumns = new Dictionary<GXDLMSObject, GXDLMSObjectCollection>();

		[XmlIgnore]
		private Dictionary<GXObisCode, object> Values = new Dictionary<GXObisCode, object>();

		[XmlIgnore]
		private GXObisCode ItemReading;

		public ProgressEventHandler OnProgress;

		public ReadEventHandler OnBeforeRead;

		public ReadEventHandler OnAfterRead;

		private GXDLMSProfileGeneric CurrentProfileGeneric;

		private string MeterID = "";

		private string NameTask = "";

		private int i = 1;

		private List<string> listRows = new List<string>();

		public bool UseLogicalNameReferencing
		{
			get
			{
				return m_Cosem.UseLogicalNameReferencing;
			}
		}

		public object ClientID
		{
			get
			{
				return m_Cosem.ClientID;
			}
		}

		public object ServerID
		{
			get
			{
				return m_Cosem.ServerID;
			}
		}

		public event DataReceivedEventHandler OnDataReceived;

		private void AddLog(MediaLog.MessageType type, string msg)
		{
			if (m_bESMediaType)
			{
				m_ESMedia.AddLog(type, msg);
			}
			else
			{
				MediaLog.DisplayData(type, msg);
			}
		}

		public GXDLMSCommunicator(GXDLMSDevice parent, IGXMedia media)
		{
			Parent = parent;
			Media = media;
			m_Cosem = new GXDLMSClient();
		}

		public byte[] SNRMRequest()
		{
			return m_Cosem.SNRMRequest();
		}

		public void ParseUAResponse(byte[] data)
		{
			m_Cosem.ParseUAResponse(data);
		}

		public byte[][] AARQRequest()
		{
			return m_Cosem.AARQRequest(null);
		}

		public void ParseAAREResponse(byte[] data)
		{
			m_Cosem.ParseAAREResponse(data);
		}

		public byte[] Read(object data, ObjectType type, int AttributeOrdinal)
		{
			LastTransaction = DateTime.Now;
			if (data is GXDLMSObject)
			{
				GXDLMSObject gXDLMSObject = data as GXDLMSObject;
				data = gXDLMSObject.Name;
			}
			byte[] array = m_Cosem.Read(data, type, AttributeOrdinal)[0];
			GXLogWriter.WriteLog("Reading object " + data.ToString() + " from interface " + type, array);
			return array;
		}

		public byte[] MethodRequest(object name, ObjectType type, int methodIndex, int set, DateTime starttime)
		{
			return ReadDataBlock(m_Cosem.MethodRequest(name, type, methodIndex), "", set, starttime);
		}

		public byte[] DisconnectRequest()
		{
			byte[] array = m_Cosem.DisconnectRequest();
			PacketParser.ByteToHex(array);
			return array;
		}

		public byte[] DisconnectedModeRequest()
		{
			byte[] array = m_Cosem.DisconnectedModeRequest();
			GXLogWriter.WriteLog("Disconnected Mode request", array);
			return array;
		}

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

		public static string ByteToHex(byte[] comByte)
		{
			int num = -1;
			num = comByte.Length;
			StringBuilder stringBuilder = new StringBuilder(num * 3);
			for (int i = 0; i < num; i++)
			{
				stringBuilder.Append(Convert.ToString(comByte[i], 16).PadLeft(2, '0').PadRight(3, ' '));
			}
			return stringBuilder.ToString().ToUpper();
		}

		public byte[] ReadDLMSPacketES(byte[] data)
		{
			if (data == null)
			{
				return null;
			}
			object eop = (byte)126;
			ReceiveParameters<byte[]> receiveParameters = new ReceiveParameters<byte[]>
			{
				AllData = true,
				Eop = eop,
				Count = 5,
				WaitTime = Parent.WaitTime * 1000
			};
			AddLog(MediaLog.MessageType.Outgoing, "ReadDLMSPacketES send data: " + ByteToHex(data) + " [" + data.Length + "]\n");
			m_ESMedia.CustomCheckEndPacket = CheckEndPacket;
			receiveParameters.Reply = m_ESMedia.SendMessage(data);
			if (receiveParameters.Reply == null)
			{
				Thread.Sleep(50);
				receiveParameters.Reply = m_ESMedia.SendMessage(data);
			}
			Thread.Sleep(50);
			AddLog(MediaLog.MessageType.Incoming, "ReadDLMSPacketES reply data: " + ByteToHex(receiveParameters.Reply) + " [" + receiveParameters.Reply.Length + "]\n");
			m_ESMedia.CustomCheckEndPacket = null;
			GXLogWriter.WriteLog("Reveived data", receiveParameters.Reply);
			int num = receiveParameters.Reply[2];
			if (num != 20)
			{
			}
			if (num + 2 < receiveParameters.Reply.Length)
			{
				List<byte> list = new List<byte>(receiveParameters.Reply);
				int num2 = 0;
				bool flag = false;
				do
				{
					num2 = list.IndexOf(253, num2);
					if (num2 >= 0)
					{
						if (list[num2 + 1] == 238)
						{
							list.RemoveAt(num2 + 1);
							list[num2] = 254;
							flag = true;
						}
						else if (list[num2 + 1] == 237)
						{
							list.RemoveAt(num2 + 1);
							flag = true;
						}
						num2++;
					}
				}
				while (num2 >= 0);
				if (flag)
				{
					receiveParameters.Reply = list.ToArray();
				}
			}
			object obj = null;
			try
			{
				obj = m_Cosem.CheckReplyErrors(data, receiveParameters.Reply);
			}
			catch (Exception)
			{
			}
			if (obj != null)
			{
				object[,] array = (object[,])obj;
				int num3 = (int)array[0, 0];
				if (num3 == -1)
				{
					throw new Exception(array[0, 1].ToString());
				}
				throw new GXDLMSException(num3);
			}
			return receiveParameters.Reply;
		}

		public bool CheckEndPacket(byte[] data, int count)
		{
			byte[] array = new byte[count];
			Buffer.BlockCopy(data, 0, array, 0, count);
			return m_Cosem.IsDLMSPacketComplete(array);
		}

		public byte[] ReadDLMSPacket(byte[] data)
		{
			if (data == null)
			{
				return null;
			}
			if (m_bESMediaType)
			{
				return ReadDLMSPacketES(data);
			}
			object eop = (byte)126;
			if (Parent.UseIEC47 && Media is GXNet && !Parent.UseRemoteSerial)
			{
				eop = null;
			}
			int num = 0;
			bool flag = false;
			ReceiveParameters<byte[]> receiveParameters = new ReceiveParameters<byte[]>
			{
				AllData = true,
				Eop = eop,
				Count = 5,
				WaitTime = Parent.WaitTime * 1000
			};
			AddLog(MediaLog.MessageType.Outgoing, "ReadDLMSPacket send data: " + PacketParser.ByteToHex(data) + "\n");
			lock (Media.Synchronous)
			{
				Media.Send(data, null);
				ByteToHex(data);
				while (!flag && num != 3)
				{
					if (!(flag = Media.Receive(receiveParameters)))
					{
						if (++num == 3)
						{
							string text = "Failed to receive reply from the device in given time.";
							GXLogWriter.WriteLog(text, receiveParameters.Reply);
							throw new Exception(text);
						}
						if (receiveParameters.Eop == null)
						{
							receiveParameters.Count = 1;
						}
						Debug.WriteLine("Data send failed. Try to resend " + num + "/3");
						Media.Send(data, null);
					}
				}
				while (!m_Cosem.IsDLMSPacketComplete(receiveParameters.Reply))
				{
					if (receiveParameters.Eop == null)
					{
						receiveParameters.Count = 1;
					}
					if (!Media.Receive(receiveParameters))
					{
						if (++num == 3)
						{
							string text2 = "Failed to receive reply from the device in given time.";
							GXLogWriter.WriteLog(text2, receiveParameters.Reply);
							throw new Exception(text2);
						}
						Debug.WriteLine("Data send failed. Try to resend " + num + "/3");
					}
				}
			}
			GXLogWriter.WriteLog("Reveived data", receiveParameters.Reply);
			AddLog(MediaLog.MessageType.Incoming, "ReadDLMSPacket received data: " + PacketParser.ByteToHex(receiveParameters.Reply) + "\n");
			object obj = null;
			try
			{
				obj = m_Cosem.CheckReplyErrors(data, receiveParameters.Reply);
				if (obj != null)
				{
				}
			}
			catch
			{
			}
			if (obj != null)
			{
				object[,] array = (object[,])obj;
				int num2 = (int)array[0, 0];
				if (num2 == -1)
				{
					throw new Exception(array[0, 1].ToString());
				}
				throw new GXDLMSException(num2);
			}
			return receiveParameters.Reply;
		}

		private void InitializeIEC()
		{
			GXManufacturer gXManufacturer = Parent.Manufacturers.FindByIdentification(Parent.Manufacturer);
			if (gXManufacturer == null)
			{
				throw new Exception("Unknown manufacturer " + Parent.Manufacturer);
			}
			if (m_bESMediaType)
			{
				return;
			}
			byte b = 10;
			Media.Open();
			Thread.Sleep(500);
			if (Media == null || Parent.StartProtocol != StartProtocolType.IEC)
			{
				return;
			}
			string text = "/?!\r\n";
			if (Parent.HDLCAddressing == HDLCAddressType.SerialNumber)
			{
				text = string.Concat("/?", Parent.PhysicalAddress, "!\r\n");
			}
			GXLogWriter.WriteLog("HDLC sending:" + text);
			ReceiveParameters<string> receiveParameters = new ReceiveParameters<string>
			{
				Eop = b,
				WaitTime = Parent.WaitTime * 1000
			};
			lock (Media.Synchronous)
			{
				Media.Send(text, null);
				if (!Media.Receive(receiveParameters))
				{
					ReadDLMSPacket(DisconnectRequest());
					text = "Failed to receive reply from the device in given time.";
					GXLogWriter.WriteLog(text);
					throw new Exception(text);
				}
				if (receiveParameters.Reply == text)
				{
					receiveParameters.Reply = null;
					if (!Media.Receive(receiveParameters))
					{
						ReadDLMSPacket(DisconnectRequest());
						text = "Failed to receive reply from the device in given time.";
						GXLogWriter.WriteLog(text);
						throw new Exception(text);
					}
				}
			}
			GXLogWriter.WriteLog("HDLC received: " + receiveParameters.Reply);
			if (receiveParameters.Reply[0] != '/')
			{
				receiveParameters.WaitTime = 100;
				Media.Receive(receiveParameters);
				throw new Exception("Invalid responce.");
			}
			receiveParameters.Reply.Substring(1, 3);
			char c = receiveParameters.Reply[4];
			int num;
			switch (c)
			{
			default:
				throw new Exception("Unknown baud rate.");
			case '0':
				num = 300;
				break;
			case '1':
				num = 600;
				break;
			case '2':
				num = 1200;
				break;
			case '3':
				num = 2400;
				break;
			case '4':
				num = 4800;
				break;
			case '5':
				num = 9600;
				break;
			case '6':
				num = 19200;
				break;
			}
			GXLogWriter.WriteLog("BaudRate is : " + num);
			byte[] array = new byte[6] { 6, 0, 0, 0, 13, 10 };
			array[1] = 50;
			array[2] = (byte)c;
			array[3] = 50;
			byte[] array2 = array;
			GXLogWriter.WriteLog("Moving to mode E.", array2);
			lock (Media.Synchronous)
			{
				Media.Send(array2, null);
				receiveParameters.Reply = null;
				receiveParameters.WaitTime = 500;
				if (!Media.Receive(receiveParameters))
				{
					ReadDLMSPacket(DisconnectRequest());
					text = "Failed to receive reply from the device in given time.";
					GXLogWriter.WriteLog(text);
					throw new Exception(text);
				}
			}
		}

		private void InitNet()
		{
			try
			{
				if (Parent.UseRemoteSerial)
				{
					InitializeIEC();
				}
				else
				{
					Media.Open();
				}
			}
			catch (Exception ex)
			{
				if (Media != null)
				{
					Media.Close();
				}
				throw ex;
			}
		}

		public void UpdateManufactureSettings(string id, string HDLCadress)
		{
			if (!string.IsNullOrEmpty(Parent.Manufacturer) && string.Compare(Parent.Manufacturer, id, true) != 0)
			{
				throw new Exception("Manufacturer type does not match. Manufacturer is " + id + " and it should be " + Parent.Manufacturer + ".");
			}
			GXManufacturer gXManufacturer = Parent.Manufacturers.FindByIdentification(id);
			if (gXManufacturer == null)
			{
				throw new Exception("Unknown manufacturer " + id);
			}
			Parent.Manufacturer = gXManufacturer.Identification;
			m_Cosem.Authentication = Parent.Authentication;
			m_Cosem.InterfaceType = InterfaceType.General;
			if (!string.IsNullOrEmpty(Parent.Password))
			{
				m_Cosem.Password = CryptHelper.Decrypt(Parent.Password, Password.Key);
			}
			m_Cosem.UseLogicalNameReferencing = gXManufacturer.UseLogicalNameReferencing;
			if (!Parent.UseRemoteSerial && Media is GXNet && gXManufacturer.UseIEC47)
			{
				m_Cosem.InterfaceType = InterfaceType.Net;
				m_Cosem.ClientID = Convert.ToUInt16(Parent.ClientID);
				m_Cosem.ServerID = Convert.ToUInt16((Parent.LogicalAddress << 9) | Convert.ToUInt16(Parent.PhysicalAddress));
				return;
			}
			if (Parent.HDLCAddressing == HDLCAddressType.Custom)
			{
				m_Cosem.ClientID = Parent.ClientID;
			}
			else
			{
				m_Cosem.ClientID = (byte)((uint)(Convert.ToByte(Parent.ClientID) << 1) | 1u);
			}
			string formula = null;
			GXServerAddress server = gXManufacturer.GetServer(Parent.HDLCAddressing);
			if (server != null)
			{
				formula = HDLCadress;
			}
			m_Cosem.ServerID = GXManufacturer.CountServerAddress(Parent.HDLCAddressing, formula, Parent.PhysicalAddress, Parent.LogicalAddress);
		}

		private void InitSerial()
		{
			try
			{
				InitializeIEC();
			}
			catch (Exception ex)
			{
				AddLog(MediaLog.MessageType.Error, ex.ToString());
				if (Media != null)
				{
					Media.Close();
				}
				throw ex;
			}
		}

		private void InitTerminal()
		{
			try
			{
				InitializeIEC();
			}
			catch (Exception ex)
			{
				if (Media != null)
				{
					Media.Close();
				}
				throw ex;
			}
		}

		public void InitializeConnection()
		{
			if (string.IsNullOrEmpty(Parent.Manufacturer))
			{
			}
			if (Media is GXSerial)
			{
				AddLog(MediaLog.MessageType.Normal, "Initializing serial connection.\n");
				InitSerial();
				ConnectionStartTime = DateTime.Now;
			}
			else
			{
				if (!(Media is GXNet))
				{
					GXLogWriter.WriteLog("Unknown media type.");
					throw new Exception("Unknown media type.");
				}
				GXLogWriter.WriteLog("Initializing Network connection.");
				InitNet();
				Thread.Sleep(500);
			}
			byte[] data = null;
			byte[] array = SNRMRequest();
			if (array != null)
			{
				ByteToHex(array);
				data = ReadDLMSPacket(array);
				AddLog(MediaLog.MessageType.Incoming, "Send SNRM request: \n");
				ParseUAResponse(data);
				AddLog(MediaLog.MessageType.Incoming, "Parsing UA reply succeeded!\n");
			}
			byte[][] array2 = AARQRequest();
			byte[][] array3 = array2;
			foreach (byte[] data2 in array3)
			{
				AddLog(MediaLog.MessageType.Incoming, "Send AARQ request:\n");
				data = ReadDLMSPacket(data2);
			}
			try
			{
				ParseAAREResponse(data);
			}
			catch (Exception ex)
			{
				ReadDLMSPacket(DisconnectRequest());
				throw ex;
			}
			AddLog(MediaLog.MessageType.Incoming, "Parsing AARE reply succeeded!\n");
			Parent.KeepAliveStart();
		}

		public void InitializeConnectionES(string HDLCadress)
		{
			if (!string.IsNullOrEmpty(Parent.Manufacturer))
			{
				UpdateManufactureSettings(Parent.Manufacturer, HDLCadress);
			}
			byte[] data = null;
			byte[] array = SNRMRequest();
			if (array != null)
			{
				ByteToHex(array);
				data = ReadDLMSPacket(array);
				AddLog(MediaLog.MessageType.Incoming, "Send SNRM request: \n");
				ParseUAResponse(data);
				AddLog(MediaLog.MessageType.Incoming, "Parsing UA reply succeeded!\n");
			}
			byte[][] array2 = AARQRequest();
			byte[][] array3 = array2;
			foreach (byte[] data2 in array3)
			{
				AddLog(MediaLog.MessageType.Incoming, "Send AARQ request: \n");
				data = ReadDLMSPacket(data2);
			}
			try
			{
				ParseAAREResponse(data);
			}
			catch (Exception ex)
			{
				ReadDLMSPacket(DisconnectRequest());
				throw ex;
			}
			AddLog(MediaLog.MessageType.Incoming, "Parsing AARE reply succeeded!\n");
			Parent.KeepAliveStart();
		}

		private void NotifyProgress(string description, int current, int maximium)
		{
			if (OnProgress != null)
			{
				OnProgress(this, description, current, maximium);
			}
		}

		private byte[] ReadDataBlock(byte[] data, string text, int set, DateTime starttime)
		{
			return ReadDataBlock(data, text, 1.0, set, starttime);
		}

		private void OnProfileGenericDataReceived(object sender, byte[] data, int set, DateTime starttime)
		{
			try
			{
				Array array = (Array)m_Cosem.TryGetValue(data);
				if (array != null)
				{
					ShowRows(array);
					if (OnAfterRead != null)
					{
						OnAfterRead(CurrentProfileGeneric, 2, NameTask, set, starttime);
					}
					Application.DoEvents();
				}
			}
			catch (Exception)
			{
			}
		}

		public void loadIDTask(string meterid, string nametask)
		{
			MeterID = meterid;
			NameTask = nametask;
		}

		public List<string> resultRows()
		{
			return listRows;
		}

		private void ShowRows(Array rows)
		{
			DataType dataType = DataType.None;
			DataType dataType2 = DataType.None;
			foreach (object[] row in rows)
			{
				if (row == null)
				{
					continue;
				}
				DataRow dataRow = CurrentProfileGeneric.Data.NewRow();
				object obj = null;
				for (int i = 0; i < CurrentProfileGeneric.Columns.Count; i++)
				{
					string text = "";
					if (Parent.Extension != null)
					{
						obj = row[i];
					}
					else
					{
						int num = 0;
						IGXDLMSColumnObject iGXDLMSColumnObject = CurrentProfileGeneric.Columns[i];
						foreach (GXDLMSObject item in DeviceColumns[CurrentProfileGeneric])
						{
							IGXDLMSColumnObject iGXDLMSColumnObject2 = item;
							if (item.ObjectType != CurrentProfileGeneric.Columns[i].ObjectType || !(item.LogicalName == CurrentProfileGeneric.Columns[i].LogicalName) || iGXDLMSColumnObject2.SelectedAttributeIndex != iGXDLMSColumnObject.SelectedAttributeIndex)
							{
								num++;
								continue;
							}
							obj = row[num];
							text = item.LogicalName;
							break;
						}
					}
					if (obj == null)
					{
						continue;
					}
					double num2 = 1.0;
					object obj2 = CurrentProfileGeneric.Columns[i];
					if (obj2 is GXDLMSRegister)
					{
						num2 = ((GXDLMSRegister)obj2).Scaler;
					}
					if (obj2 is GXDLMSDemandRegister)
					{
						num2 = ((GXDLMSDemandRegister)obj2).Scaler;
					}
					IGXDLMSColumnObject iGXDLMSColumnObject3 = CurrentProfileGeneric.Columns[i];
					dataType2 = CurrentProfileGeneric.Columns[i].GetUIDataType(iGXDLMSColumnObject3.SelectedAttributeIndex);
					dataType = CurrentProfileGeneric.Columns[i].GetDataType(iGXDLMSColumnObject3.SelectedAttributeIndex);
					if (dataType2 == DataType.None)
					{
						dataType2 = dataType;
					}
					if (!obj.GetType().IsArray && num2 != 1.0)
					{
						if (dataType == DataType.None)
						{
							dataRow[i] = Convert.ToDouble(obj) * num2;
							List<string> list = listRows;
							string text2 = text;
							object obj3 = dataRow[i];
							list.Add(text2 + "," + ((obj3 != null) ? obj3.ToString() : null));
						}
						else
						{
							dataRow[i] = Convert.ToDouble(GXHelpers.ConvertFromDLMS(obj, DataType.None, dataType, true)) * num2;
							List<string> list2 = listRows;
							string text3 = text;
							object obj4 = dataRow[i];
							list2.Add(text3 + "," + ((obj4 != null) ? obj4.ToString() : null));
						}
					}
					else
					{
						dataRow[i] = GXHelpers.ConvertFromDLMS(obj, dataType, dataType2, true);
						List<string> list3 = listRows;
						string text4 = text;
						object obj5 = dataRow[i];
						list3.Add(text4 + "," + ((obj5 != null) ? obj5.ToString() : null));
					}
				}
				CurrentProfileGeneric.Data.Rows.InsertAt(dataRow, CurrentProfileGeneric.Data.Rows.Count);
			}
		}

		public static void OpenFile(string filePath)
		{
			Process process = new Process();
			process.StartInfo.UseShellExecute = true;
			process.StartInfo.FileName = filePath;
			process.Start();
		}

		internal byte[] ReadDataBlock(byte[] data, string text, double multiplier, int set, DateTime starttime)
		{
			if (Parent.InactivityMode == InactivityMode.ReopenActive && Media is GXSerial && DateTime.Now.Subtract(ConnectionStartTime).TotalSeconds > 40.0)
			{
				Parent.Disconnect();
				Parent.InitializeConnection();
			}
			byte[] array = ReadDLMSPacket(data);
			PacketParser.ByteToHex(array);
			object obj = m_Cosem.CheckReplyErrors(data, array);
			if (obj != null)
			{
				object[,] array2 = (object[,])obj;
				int num = (int)array2[0, 0];
				if (num == -1)
				{
					throw new GXDLMSException(array2[0, 1].ToString());
				}
				throw new GXDLMSException(num);
			}
			byte[] data2 = null;
			RequestTypes requestTypes = m_Cosem.GetDataFromPacket(array, ref data2);
			if (this.OnDataReceived != null)
			{
				this.OnDataReceived(this, data2, set, starttime);
			}
			if ((requestTypes & (RequestTypes)3) != 0)
			{
				int maxProgressStatus = m_Cosem.GetMaxProgressStatus(data2);
				NotifyProgress(text, 1, maxProgressStatus);
				while (requestTypes != 0)
				{
					while ((requestTypes & RequestTypes.Frame) != 0)
					{
						data = m_Cosem.ReceiverReady(RequestTypes.Frame);
						AddLog(MediaLog.MessageType.Incoming, "Get next frame: \n");
						array = ReadDLMSPacket(data);
						RequestTypes dataFromPacket = m_Cosem.GetDataFromPacket(array, ref data2);
						if (this.OnDataReceived != null)
						{
							this.OnDataReceived(this, data2, set, starttime);
						}
						if ((dataFromPacket & RequestTypes.Frame) != 0)
						{
							int currentProgressStatus = m_Cosem.GetCurrentProgressStatus(data2);
							NotifyProgress(text, (int)(multiplier * (double)currentProgressStatus), maxProgressStatus);
							continue;
						}
						requestTypes &= (RequestTypes)(-3);
						break;
					}
					if (Parent.InactivityMode == InactivityMode.ReopenActive && Media is GXSerial && DateTime.Now.Subtract(ConnectionStartTime).TotalSeconds > 40.0)
					{
						Parent.Disconnect();
						Parent.InitializeConnection();
					}
					if ((requestTypes & RequestTypes.DataBlock) != 0)
					{
						data = m_Cosem.ReceiverReady(RequestTypes.DataBlock);
						AddLog(MediaLog.MessageType.Incoming, "Get Next Data block: \n");
						array = ReadDLMSPacket(data);
						requestTypes = m_Cosem.GetDataFromPacket(array, ref data2);
						if (this.OnDataReceived != null)
						{
							this.OnDataReceived(this, data2, set, starttime);
						}
						int currentProgressStatus2 = m_Cosem.GetCurrentProgressStatus(data2);
						NotifyProgress(text, (int)(multiplier * (double)currentProgressStatus2), maxProgressStatus);
					}
				}
			}
			return data2;
		}

		public GXDLMSObjectCollection GetObjects(int set, DateTime starttime)
		{
			GXLogWriter.WriteLog("--- Collecting objects. ---");
			byte[] data;
			try
			{
				NotifyProgress("Collecting objects", 0, 1);
				data = ReadDataBlock(m_Cosem.GetObjects(), "Collecting objects", set, starttime);
			}
			catch (Exception ex)
			{
				throw new Exception("GetObjects failed. " + ex.Message);
			}
			GXDLMSObjectCollection gXDLMSObjectCollection = m_Cosem.ParseObjects(data, true);
			NotifyProgress("Collecting objects", gXDLMSObjectCollection.Count, gXDLMSObjectCollection.Count);
			GXLogWriter.WriteLog("--- Collecting " + gXDLMSObjectCollection.Count + " objects. ---");
			return gXDLMSObjectCollection;
		}

		private List<GXAttributeRead> GetOrderedAttributes(GXDLMSObject obj, int attribute)
		{
			PropertyInfo[] array = (from x in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				where Attribute.IsDefined(x, typeof(GXDLMSAttribute), false)
				select x).ToArray();
			SortedList<int, GXAttributeRead> sortedList = new SortedList<int, GXAttributeRead>();
			SortedList<int, GXAttributeRead> sortedList2 = new SortedList<int, GXAttributeRead>();
			PropertyInfo[] array2 = array;
			PropertyInfo[] array3 = array2;
			foreach (PropertyInfo propertyInfo in array3)
			{
				GXDLMSAttribute gXDLMSAttribute = Attribute.GetCustomAttribute(propertyInfo, typeof(GXDLMSAttribute)) as GXDLMSAttribute;
				if (gXDLMSAttribute != null && (obj.GetAccess(gXDLMSAttribute.Index) & AccessMode.Read) != 0 && (attribute == 0 || attribute == gXDLMSAttribute.Index))
				{
					if (gXDLMSAttribute.Order == 0)
					{
						sortedList.Add(gXDLMSAttribute.Index, new GXAttributeRead(propertyInfo, gXDLMSAttribute));
					}
					else
					{
						sortedList2.Add(gXDLMSAttribute.Order, new GXAttributeRead(propertyInfo, gXDLMSAttribute));
					}
				}
				if (attribute != 0)
				{
					break;
				}
			}
			List<GXAttributeRead> list = new List<GXAttributeRead>(sortedList2.Values);
			list.AddRange(sortedList.Values);
			return list;
		}

		private bool IsReadAlready(GXDLMSObject obj, int index)
		{
			object obj2 = obj.GetValues()[index - 1];
			if (obj2 == null)
			{
				return false;
			}
			if (obj2 is DateTime)
			{
				return (DateTime)obj2 != DateTime.MinValue;
			}
			return obj.GetLastReadTime(index) != DateTime.MinValue;
		}

		public void Read(object sender, GXDLMSObject obj, int attribute, int set, DateTime startime)
		{
			Convert.ToString(obj.Description);
			foreach (GXAttributeRead orderedAttribute in GetOrderedAttributes(obj, attribute))
			{
				PropertyInfo info = orderedAttribute.Info;
				GXDLMSAttribute attribute2 = orderedAttribute.Attribute;
				if (obj is GXDLMSProfileGeneric)
				{
					if (Parent.Extension != null)
					{
						GXDLMSObjectCollection gXDLMSObjectCollection;
						if (!DeviceColumns.ContainsKey(obj))
						{
							gXDLMSObjectCollection = Parent.Extension.Refresh(obj as GXDLMSProfileGeneric, this, set, startime);
							if (gXDLMSObjectCollection == null)
							{
								gXDLMSObjectCollection = GetProfileGenericColumns(obj.Name, set, startime);
							}
							DeviceColumns[obj as GXDLMSProfileGeneric] = gXDLMSObjectCollection;
						}
						else
						{
							gXDLMSObjectCollection = DeviceColumns[obj];
						}
						if (Parent.Extension.Read(sender, obj, gXDLMSObjectCollection, orderedAttribute.Attribute.Index, this, NameTask, set, startime))
						{
							continue;
						}
					}
					if (attribute2.Index == 2 || attribute2.Index == 3)
					{
						CurrentProfileGeneric = obj as GXDLMSProfileGeneric;
						if (attribute2.Index == 3)
						{
							if (!DeviceColumns.ContainsKey(obj))
							{
								DeviceColumns[obj] = GetProfileGenericColumns(obj.Name, set, startime);
							}
						}
						else
						{
							if (attribute2.Index != 2)
							{
								continue;
							}
							if (OnBeforeRead != null)
							{
								OnBeforeRead(obj, attribute2.Index, NameTask, set, startime);
							}
							try
							{
								OnDataReceived += OnProfileGenericDataReceived;
								if (CurrentProfileGeneric.AccessSelector != 0)
								{
									byte[] data = m_Cosem.ReadRowsByRange(CurrentProfileGeneric.Name, CurrentProfileGeneric.Columns[0].LogicalName, CurrentProfileGeneric.Columns[0].ObjectType, CurrentProfileGeneric.Columns[0].Version, Convert.ToDateTime(CurrentProfileGeneric.From), Convert.ToDateTime(CurrentProfileGeneric.To));
									ReadDataBlock(data, "Reading profile generic data", 1.0, set, startime);
								}
								else
								{
									byte[] data2 = m_Cosem.ReadRowsByEntry(CurrentProfileGeneric.Name, Convert.ToInt32(CurrentProfileGeneric.From), Convert.ToInt32(CurrentProfileGeneric.To));
									object name = CurrentProfileGeneric.Name;
									ReadDataBlock(data2, "Reading profile generic data " + ((name != null) ? name.ToString() : null), 1.0, set, startime);
								}
							}
							finally
							{
								OnDataReceived -= OnProfileGenericDataReceived;
							}
						}
						continue;
					}
				}
				if (!attribute2.Static || !IsReadAlready(obj, attribute2.Index))
				{
					if (OnBeforeRead != null)
					{
						OnBeforeRead(obj, attribute2.Index, NameTask, set, startime);
					}
					try
					{
						byte[] array = m_Cosem.Read(obj.ToString(), obj.ObjectType, attribute2.Index)[0];
						AddLog(MediaLog.MessageType.Incoming, "Send data: " + PacketParser.ByteToHex(array) + "\n");
						array = ReadDataBlock(array, string.Concat("Read object type ", obj.ObjectType, " index: ", attribute2.Index), set, startime);
						if (obj.GetDataType(attribute2.Index) == DataType.None)
						{
							obj.SetDataType(attribute2.Index, m_Cosem.GetDLMSDataType(array));
						}
						object obj2 = m_Cosem.GetValue(array, obj.ObjectType, obj.LogicalName, attribute2.Index);
						if (obj2 != null)
						{
							if (obj2 is byte[])
							{
								obj2 = GXHelpers.ConvertFromDLMS(obj2, attribute2.Type, attribute2.UIType, true);
							}
							else if (!obj2.GetType().IsArray)
							{
								if (info.PropertyType != typeof(object))
								{
									if (info.PropertyType.IsEnum)
									{
										obj2 = Enum.ToObject(info.PropertyType, Convert.ToInt32(obj2));
									}
									else
									{
										try
										{
											obj2 = Convert.ChangeType(obj2, info.PropertyType);
										}
										catch (Exception)
										{
											continue;
										}
									}
								}
							}
							else if (obj2.GetType().GetElementType() != info.PropertyType && info.PropertyType != typeof(string))
							{
								Type elementType = info.PropertyType.GetElementType();
								if (elementType != typeof(object))
								{
									Array array2 = (Array)obj2;
									Array array3 = Array.CreateInstance(elementType, array2.Length);
									obj2 = array3;
									for (int i = 0; i != array2.Length; i++)
									{
										array3.SetValue(Convert.ChangeType(array2.GetValue(i), elementType), i);
									}
								}
							}
						}
						if (obj2 != null && info.PropertyType != typeof(object) && info.PropertyType != obj2.GetType())
						{
							obj2 = Convert.ChangeType(obj2, info.PropertyType);
						}
						info.SetValue(obj, obj2, null);
						if (OnAfterRead != null)
						{
							OnAfterRead(obj, attribute2.Index, NameTask, set, startime);
						}
						obj.SetLastReadTime(attribute2.Index, DateTime.Now);
						if (attribute != 0)
						{
							return;
						}
					}
					catch (GXDLMSException ex2)
					{
						if (ex2.ErrorCode != 3)
						{
							throw ex2;
						}
						obj.SetAccess(attribute2.Index, AccessMode.NoAccess);
					}
				}
				else
				{
					Debug.WriteLine(string.Format("Object {0} Attribute {1} is already read.", obj.LogicalName, attribute2.Index));
				}
			}
		}

		public object ReadValue(object data, ObjectType interfaceClass, int attributeOrdinal, ref DataType type, int set, DateTime starttime)
		{
			byte[] array = null;
			array = Read(data, interfaceClass, attributeOrdinal);
			array = ReadDataBlock(array, "Read object", set, starttime);
			object value = m_Cosem.GetValue(array);
			if (type == DataType.None)
			{
				type = m_Cosem.GetDLMSDataType(array);
			}
			return value;
		}

		public GXDLMSObjectCollection GetProfileGenericColumns(object name, int set, DateTime starttime)
		{
			byte[] array = ReadDataBlock(Read(name, ObjectType.ProfileGeneric, 3), "Get profile generic columns...", set, starttime);
			if (array == null)
			{
				return null;
			}
			return m_Cosem.ParseColumns(array);
		}

		public void KeepAlive()
		{
			byte[] data = null;
			byte[] array = null;
			byte[] keepAlive = m_Cosem.GetKeepAlive();
			GXLogWriter.WriteLog("Send Keep Alive", keepAlive);
			array = ReadDLMSPacket(keepAlive);
			m_Cosem.GetDataFromPacket(array, ref data);
		}
	}
}
