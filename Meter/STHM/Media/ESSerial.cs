using System;
using System.IO.Ports;
using System.Threading;

namespace STHM.Media
{
	public class ESSerial : ESMedia
	{
		public SerialPort comPort = null;

		private bool bSync = false;

		private int timeout = 10000;

		private ManualResetEvent m_HasDataEvent = new ManualResetEvent(false);

		public const int m_bytebufferSize = 10240;

		private byte[] m_bytebuffer = new byte[10240];

		private int m_bytebufferCount = 0;

		public SerialPortLine Config
		{
			get
			{
				return (SerialPortLine)LineConfig;
			}
			set
			{
				LineConfig = value;
			}
		}

		public ESSerial(string baud, string par, string sBits, string dBits, string name)
		{
			Config = new SerialPortLine();
			Config.portName = name;
			Config.baudRate = int.Parse(baud);
			Config.dataBits = int.Parse(sBits);
			Config.parity = (Parity)Enum.Parse(typeof(Parity), par);
			Config.stopBits = (StopBits)Enum.Parse(typeof(StopBits), sBits);
			Config.GetConfig();
		}

		public ESSerial(string sConfig)
		{
			Config = new SerialPortLine(sConfig);
		}

		public ESSerial()
		{
			Config = new SerialPortLine();
			Config.portName = "COM2";
		}

		public override void Close()
		{
			if (comPort != null)
			{
				if (comPort.IsOpen)
				{
					comPort.Close();
					AddLog(MediaLog.MessageType.Normal, string.Format("Port {0} closed at {1}", Config.portName, DateTime.Now));
				}
				comPort.Dispose();
				comPort = null;
				LineConfig.ConnectedStatus = EnumConnectedStatus.Disconnected;
			}
		}

		public void SetPortConfig(bool bFromConfig = true)
		{
			if (bFromConfig)
			{
				comPort.BaudRate = Config.baudRate;
				comPort.DataBits = Config.dataBits;
				comPort.StopBits = Config.stopBits;
				comPort.Parity = Config.parity;
				comPort.PortName = Config.portName;
			}
			else
			{
				Config.baudRate = comPort.BaudRate;
				Config.dataBits = comPort.DataBits;
				Config.stopBits = comPort.StopBits;
				Config.parity = comPort.Parity;
				Config.portName = comPort.PortName;
			}
		}

		public override bool Open()
		{
			try
			{
				Close();
				comPort = new SerialPort(Config.portName, Config.baudRate, Config.parity, Config.dataBits, Config.stopBits);
				comPort.Open();
				AddLog(MediaLog.MessageType.Normal, string.Format("Port {0} opened at {1}", Config.portName, DateTime.Now));
				bool isOpen;
				if (isOpen = comPort.IsOpen)
				{
					LineConfig.ConnectedStatus = EnumConnectedStatus.Connected;
				}
				else
				{
					LineConfig.ConnectedStatus = EnumConnectedStatus.NotConnected;
				}
				return isOpen;
			}
			catch (Exception ex)
			{
				AddLog(MediaLog.MessageType.Error, ex.Message);
				LineConfig.ConnectedStatus = EnumConnectedStatus.NotConnected;
				return false;
			}
		}

		private void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			if (!bSync)
			{
				Thread.Sleep(100);
				comPort.ReadExisting();
			}
			else
			{
				comPort.DataReceived -= comPort_DataReceived;
				m_HasDataEvent.Set();
			}
		}

		public override void PostMessage(byte[] msg)
		{
			comPort.Write(msg, 0, msg.Length);
		}

		public override byte[] SendMessage(byte[] buffer)
		{
			try
			{
				if (!comPort.IsOpen)
				{
					comPort.Open();
				}
				bSync = true;
				m_HasDataEvent.Reset();
				comPort.DataReceived += comPort_DataReceived;
				comPort.Write(buffer, 0, buffer.Length);
				comPort.ReadTimeout = timeout;
				m_HasDataEvent.WaitOne(timeout);
				bool flag = true;
				Thread.Sleep(300);
				m_bytebufferCount = 0;
				int num = 0;
				while (flag)
				{
					int num2 = comPort.Read(m_bytebuffer, m_bytebufferCount, 10240 - m_bytebufferCount);
					m_bytebufferCount += num2;
					if (num2 == 0)
					{
						num++;
						if (num == 3)
						{
							break;
						}
					}
					else
					{
						num = 0;
						flag = !OnCheckEndPacket(m_bytebuffer, m_bytebufferCount);
					}
					if (flag && num2 == 0)
					{
						Thread.Sleep(100);
					}
				}
				byte[] array = null;
				if (m_bytebufferCount > 0)
				{
					array = new byte[m_bytebufferCount];
					Array.Copy(m_bytebuffer, array, m_bytebufferCount);
				}
				return array;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
