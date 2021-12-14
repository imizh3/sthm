using System;
using System.Net.Sockets;
using System.Threading;

namespace STHM.Media
{
	public class ESNet : ESMedia
	{
		private TcpClient tcpClient = null;

		private NetworkStream stm = null;

		public IPPortLine Config
		{
			get
			{
				return (IPPortLine)LineConfig;
			}
			set
			{
				LineConfig = value;
			}
		}

		public ESNet()
		{
		}

		public ESNet(string host, string port)
		{
			Config = new IPPortLine();
			Config.Address = host;
			Config.Port = Convert.ToInt32(port);
		}

		public ESNet(string sConfig)
		{
			Config = new IPPortLine(sConfig);
		}

		public override byte[] SendMessage(byte[] msg)
		{
			stm.Write(msg, 0, msg.Length);
			Thread.Sleep(200);
			byte[] array = new byte[2024];
			int num = 0;
			int num2 = 0;
			bool flag = true;
			int num3 = 0;
			stm.ReadTimeout = 1000;
			while (flag)
			{
				num2 = 0;
				if (stm.DataAvailable)
				{
					num2 = stm.Read(array, num, 1024 - num);
				}
				else
				{
					Thread.Sleep(500);
				}
				num += num2;
				if (num2 == 0)
				{
					num3++;
					if (num3 == 3)
					{
						break;
					}
				}
				else
				{
					num3 = 0;
					flag = !OnCheckEndPacket(array, num);
				}
				if (flag && num2 == 0)
				{
					Thread.Sleep(100);
				}
			}
			byte[] array2 = new byte[num];
			Buffer.BlockCopy(array, 0, array2, 0, num);
			return array2;
		}

		public override void PostMessage(byte[] msg)
		{
			stm.Write(msg, 0, msg.Length);
		}

		public override bool Open()
		{
			try
			{
				Close();
				LineConfig.WorkingStatus = EnumWorkingStatus.NotWorking;
				AddLog(MediaLog.MessageType.Normal, string.Format("Connecting to {0}:{1}...", Config.Address, Config.Port));
				tcpClient = new TcpClient();
				tcpClient.SendTimeout = 2000;
				tcpClient.ReceiveTimeout = 2000;
				IAsyncResult asyncResult = tcpClient.BeginConnect(Config.Address, Config.Port, null, null);
				bool result;
				if (!(result = asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2.0))))
				{
					AddLog(MediaLog.MessageType.Normal, "Not Connected");
					LineConfig.ConnectedStatus = EnumConnectedStatus.NotConnected;
					LineConfig.WorkingStatus = EnumWorkingStatus.UnCompleted;
					tcpClient.Close();
					return result;
				}
				AddLog(MediaLog.MessageType.Normal, "Connected");
				stm = tcpClient.GetStream();
				bool connected;
				if (connected = tcpClient.Connected)
				{
					LineConfig.ConnectedStatus = EnumConnectedStatus.Connected;
				}
				else
				{
					LineConfig.ConnectedStatus = EnumConnectedStatus.NotConnected;
				}
				LineConfig.WorkingStatus = EnumWorkingStatus.NotWorking;
				return connected;
			}
			catch (Exception)
			{
				AddLog(MediaLog.MessageType.Normal, "Not Connected");
				LineConfig.ConnectedStatus = EnumConnectedStatus.NotConnected;
				LineConfig.WorkingStatus = EnumWorkingStatus.UnCompleted;
				tcpClient.Close();
				return false;
			}
		}

		public override void Close()
		{
			if (tcpClient == null || tcpClient.Client == null || !tcpClient.Connected)
			{
				return;
			}
			AddLog(MediaLog.MessageType.Normal, tcpClient.Client.RemoteEndPoint.ToString() + " closing...");
			try
			{
				tcpClient.Close();
				tcpClient = null;
				if (stm != null)
				{
					stm.Close();
					stm.Dispose();
					stm = null;
				}
				LineConfig.ConnectedStatus = EnumConnectedStatus.Disconnected;
				AddLog(MediaLog.MessageType.Normal, "Closed");
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}
}
