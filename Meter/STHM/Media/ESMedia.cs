using System.Text;

namespace STHM.Media
{
	public abstract class ESMedia
	{
		public MediaLog Log = null;

		public StringBuilder LogBuffer = null;

		public CommunicationLine LineConfig;

		public CheckEndPacketFunc CustomCheckEndPacket = null;

		public virtual string SendMessage(string msg)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(msg);
			AddLog(MediaLog.MessageType.Outgoing, msg + " [" + bytes.Length + "] ");
			byte[] arr = null;
			int num = 0;
            while (arr == null)
			{
                arr = SendMessage(bytes);
				num++;
				if (num == 3)
				{
					break;
				}
			}
            string str = Encoding.ASCII.GetString(arr, 0, arr.Length);
            // @
            AddLog(MediaLog.MessageType.Incoming, str + " [" + arr.Length + "] ");
            return str;
		}

		public abstract byte[] SendMessage(byte[] msg);

		public virtual void PostMessage(string msg)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(msg);
			PostMessage(bytes);
		}

		public abstract void PostMessage(byte[] msg);

		public abstract void Close();

		public abstract bool Open();

		public ESMedia()
		{
			Log = new MediaLog();
			LogBuffer = new StringBuilder();
		}

		public void AddLog(MediaLog.MessageType type, string msg)
		{
			if (Log != null)
			{
				Log.AddLog(type, msg);
				return;
			}
			msg = MediaLog.CreateLog(msg);
			LogBuffer.Append(msg + "\r\n");
		}

		protected bool EndPacket(byte[] buffer, int bufferCount)
		{
			bool rs = false;
            if (bufferCount > 0)
			{
                if (buffer[bufferCount - 1] == 6 || buffer[bufferCount - 1] == 10)
				{
                    rs = true;
				}
                if (bufferCount > 1 && buffer[bufferCount - 2] == 3)
				{
                    rs = true;
				}
			}
            return rs;
		}

		protected bool EndPacket(string buffer)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(buffer);
			return EndPacket(bytes, bytes.Length);
		}

		public virtual bool OnCheckEndPacket(byte[] buffer, int count)
		{
			if (CustomCheckEndPacket != null)
			{
				return CustomCheckEndPacket(buffer, count);
			}
			return EndPacket(buffer, count);
		}
	}
}
