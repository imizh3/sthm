using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;

namespace STHM.Media
{
	public class ESSocket : ESMedia
	{
		public delegate void ReceivedDataEventHandler(object sender, byte[] data, int leng);

		public string Host;

		public int Port;

		private Socket m_Socket;

		public string MeterId;

		private ManualResetEvent sendDone;

		private ManualResetEvent receiveDone;

		public static int Waittimeout = 5000;

		public static int buffersize = 2048;

		public static int CheckDropConnectionTime = 60;

		private byte[] bytesBuffer;

		private byte[] byteResult;

		private int bytesReceived = 0;

		private bool m_Connected;

		public bool IsBusy;

		private int bytesSent = 0;

		private bool bWaitResultCommand = false;

		private bool bFirstResultCommand = false;

		private System.Timers.Timer CheckDropConnectTimer = null;

		public int RetrySentNum = 3;

		public int RetryNakNum = 9;

		public GPRSModemLine Config
		{
			get
			{
				return (GPRSModemLine)LineConfig;
			}
			set
			{
				LineConfig = value;
			}
		}

		public Socket ClientSocket
		{
			get
			{
				return m_Socket;
			}
			set
			{
				m_Socket = value;
			}
		}

		public bool Connected
		{
			get
			{
				bool flag = false;
				try
				{
					flag = m_Socket.Connected;
				}
				catch (Exception)
				{
					LineConfig.ConnectedStatus = EnumConnectedStatus.Disconnected;
				}
				return m_Connected && flag;
			}
			set
			{
				bool flag = false;
				try
				{
					flag = m_Socket.Connected;
				}
				catch (Exception)
				{
				}
				bool connected = m_Connected;
				m_Connected = value;
				bool flag2;
				if ((flag2 = m_Connected && flag) != connected)
				{
					OnConnectedChanged(this, new EventArgs());
					if (flag2)
					{
						LineConfig.ConnectedStatus = EnumConnectedStatus.Connected;
					}
					else
					{
						LineConfig.ConnectedStatus = EnumConnectedStatus.Disconnected;
					}
				}
			}
		}

		public bool Available
		{
			get
			{
				return Connected && !IsBusy;
			}
		}

		public int BytesReceived
		{
			get
			{
				return bytesReceived;
			}
		}

		public int BytesSent
		{
			get
			{
				return bytesSent;
			}
		}

		public event EventHandler ConnectedChanged;

		public event ReceivedDataEventHandler ReceivedData;

		public ESSocket()
		{
			Init();
		}

		public ESSocket(string host, int port)
		{
			Host = host;
			Port = port;
			Init();
		}

		public ESSocket(Socket socket, string meterid)
		{
			m_Socket = socket;
			MeterId = meterid;
			Config = new GPRSModemLine();
			Config.DeviceId = MeterId;
			Config.SimcardNo = MeterId;
			Config.LineID = MeterId;
			Init();
			m_Connected = socket.Connected;
			StartReceive();
		}

		public ESSocket(Socket socket, GPRSModemLine config)
		{
			m_Socket = socket;
			LineConfig = config;
			Init();
			StartReceive();
		}

		public ESSocket(string sConfig)
		{
			Config = new GPRSModemLine(sConfig);
		}

		public void SetSocket(Socket socket)
		{
			Close();
			m_Socket = socket;
			Init();
			StartReceive();
		}

		~ESSocket()
		{
			if (CheckDropConnectTimer != null)
			{
				CheckDropConnectTimer.Stop();
				CheckDropConnectTimer.Close();
			}
		}

		private void CheckDropConnectTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			CheckDropConnectTimer.Stop();
			Connected = false;
		}

		public void Init()
		{
			bytesBuffer = new byte[buffersize];
			byteResult = new byte[10 * buffersize];
			sendDone = new ManualResetEvent(false);
			receiveDone = new ManualResetEvent(false);
			IsBusy = false;
			m_Connected = false;
			if (Config.HeartbeatEnable)
			{
				CheckDropConnectTimer = new System.Timers.Timer(Config.HeartbeatInterval * 1500);
				CheckDropConnectTimer.Elapsed += CheckDropConnectTimer_Elapsed;
			}
			m_Connected = m_Socket.Connected;
		}

		public virtual void OnConnectedChanged(object sender, EventArgs e)
		{
			if (this.ConnectedChanged != null)
			{
				this.ConnectedChanged(sender, e);
			}
		}

		public override bool Open()
		{
			try
			{
				Close();
				m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				m_Socket.Connect(Host, Port);
				IsBusy = false;
				m_Connected = m_Socket.Connected;
				StartReceive();
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		public override void Close()
		{
			if (CheckDropConnectTimer != null)
			{
				CheckDropConnectTimer.Stop();
				CheckDropConnectTimer.Close();
				CheckDropConnectTimer = null;
			}
			if (m_Socket != null)
			{
				try
				{
					Connected = false;
					m_Socket.Close();
				}
				catch (Exception)
				{
				}
				finally
				{
					m_Socket = null;
				}
			}
			Connected = false;
		}

		public override byte[] SendMessage(byte[] msg)
		{
			bytesReceived = 0;
			if (Send(msg) == msg.Length)
			{
				Receive();
			}
			if (bytesReceived > 0)
			{
				byte[] array = new byte[bytesReceived];
				Buffer.BlockCopy(byteResult, 0, array, 0, BytesReceived);
				return array;
			}
			return null;
		}

		public override string SendMessage(string msg)
		{
			string text = "";
			int num = 0;
			int num2 = 0;
			while (num < RetrySentNum || num2 < RetryNakNum)
			{
				AddLog(MediaLog.MessageType.Outgoing, msg);
				if (Send(Encoding.ASCII.GetBytes(msg)) == msg.Length)
				{
					Receive();
					text = Encoding.ASCII.GetString(byteResult, 0, bytesReceived);
				}
				AddLog(MediaLog.MessageType.Incoming, text);
				if (text == "")
				{
					num2 = 0;
					if (++num == RetrySentNum)
					{
						Connected = false;
						m_Socket.Close();
						throw new Exception("ESSocket::SendMessage(string msg):not received data");
					}
					continue;
				}
				if (text[0] != '\u0015')
				{
					break;
				}
				num = 0;
				if (++num2 == RetryNakNum)
				{
					Connected = false;
					m_Socket.Close();
					throw new Exception("ESSocket::SendMessage(string msg):statement return NAK");
				}
			}
			return text;
		}

		public override void PostMessage(byte[] msg)
		{
			m_Socket.Send(msg);
		}

		private int Send(byte[] data, int leng = 0)
		{
			if (!m_Socket.Connected)
			{
				return 0;
			}
			sendDone.Reset();
			bytesSent = 0;
			if (leng == 0)
			{
				leng = data.Length;
			}
			SetCheckDropTimer(false);
			try
			{
				m_Socket.BeginSend(data, 0, leng, SocketFlags.None, OnSend, m_Socket);
				sendDone.WaitOne(Waittimeout);
			}
			catch (Exception ex)
			{
				Connected = false;
				sendDone.Set();
				throw new Exception("Error ESSocketMedia::Send(byte[] data, int leng = 0): " + ex.Message);
			}
			finally
			{
				SetCheckDropTimer();
			}
			return bytesSent;
		}

		private void Receive()
		{
			if (m_Socket.Connected)
			{
				lock (this)
				{
					bWaitResultCommand = true;
					bFirstResultCommand = true;
					bytesReceived = 0;
					receiveDone.Reset();
				}
				receiveDone.WaitOne(Waittimeout);
			}
		}

		private void OnSend(IAsyncResult ar)
		{
			try
			{
				Socket socket = (Socket)ar.AsyncState;
				bytesSent = socket.EndSend(ar);
				Connected = socket.Connected;
				lock (this)
				{
					sendDone.Set();
				}
			}
			catch (Exception ex)
			{
				Connected = false;
				AddLog(MediaLog.MessageType.Error, "Error ESSocketMedia:OnSend(): " + ex.Message);
				sendDone.Set();
			}
		}

		public virtual void OnReceivedData(byte[] data, int leng)
		{
			if (this.ReceivedData != null)
			{
				this.ReceivedData(m_Socket, data, leng);
			}
		}

		public void StartReceive()
		{
			m_Socket.BeginReceive(bytesBuffer, 0, bytesBuffer.Length, SocketFlags.None, OnReceive, m_Socket);
		}

		private void SetCheckDropTimer(bool bStart = true)
		{
			if (CheckDropConnectTimer != null)
			{
				if (bStart)
				{
					CheckDropConnectTimer.Start();
				}
				else
				{
					CheckDropConnectTimer.Stop();
				}
			}
		}

		private bool CheckHeartbeatString(int dataleng)
		{
			bool result = false;
			if (Config.HeartbeatEnable && dataleng == Config.HeartbeatString.Length)
			{
				for (int i = 0; i < Config.HeartbeatString.Length && bytesBuffer[i] == Config.HeartbeatString[i]; i++)
				{
				}
				result = true;
			}
			return result;
		}

		private int RemoveHeartbeatStr(int dataleng)
		{
			int num = dataleng;
			if (Config.HeartbeatEnable && dataleng >= Config.HeartbeatString.Length)
			{
				bool flag = true;
				for (int i = 0; i < Config.HeartbeatString.Length; i++)
				{
					if (bytesBuffer[i] != Config.HeartbeatString[i])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					num -= Config.HeartbeatString.Length;
				}
			}
			return num;
		}

		private void EndWaitResultComnmandStatus()
		{
			receiveDone.Set();
			bWaitResultCommand = false;
		}

		private void OnReceive(IAsyncResult ar)
		{
			try
			{
				SetCheckDropTimer(false);
				Socket socket = (Socket)ar.AsyncState;
				if (socket != null)
				{
					Connected = socket.Connected;
				}
				else
				{
					Connected = false;
				}
				if (Connected)
				{
					int num = socket.EndReceive(ar);
					if (num > 0)
					{
						OnReceivedData(bytesBuffer, num);
						if (bWaitResultCommand)
						{
							int num2 = num;
							if (bFirstResultCommand)
							{
								num2 = RemoveHeartbeatStr(num);
							}
							else if (CheckHeartbeatString(num))
							{
								num2 = 0;
							}
							bFirstResultCommand = false;
							if (num2 > 0)
							{
								Buffer.BlockCopy(bytesBuffer, num - num2, byteResult, bytesReceived, num2);
								bytesReceived += num2;
								if (OnCheckEndPacket(byteResult, bytesReceived))
								{
									EndWaitResultComnmandStatus();
								}
							}
						}
					}
					if (socket.Connected)
					{
						socket.BeginReceive(bytesBuffer, 0, bytesBuffer.Length, SocketFlags.None, OnReceive, socket);
					}
					else
					{
						EndWaitResultComnmandStatus();
					}
					SetCheckDropTimer();
				}
				else
				{
					EndWaitResultComnmandStatus();
				}
			}
			catch (Exception)
			{
				Connected = false;
				EndWaitResultComnmandStatus();
			}
		}
	}
}
