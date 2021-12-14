using System;
using Newtonsoft.Json;

namespace STHM.Media
{
	public abstract class CommunicationLine
	{
		[JsonIgnore]
		public string LineID;

		[JsonIgnore]
		public string LineType;

		[JsonIgnore]
		protected string Config;

		[JsonIgnore]
		protected EnumConnectedStatus m_Connected = EnumConnectedStatus.NotConnected;

		[JsonIgnore]
		protected EnumWorkingStatus m_Working = EnumWorkingStatus.NotWorking;

		[JsonIgnore]
		public EnumConnectedStatus ConnectedStatus
		{
			get
			{
				return m_Connected;
			}
			set
			{
				if (m_Connected != value)
				{
					m_Connected = value;
					OnConnectedChanged(this, new EventArgs());
				}
			}
		}

		[JsonIgnore]
		public EnumWorkingStatus WorkingStatus
		{
			get
			{
				return m_Working;
			}
			set
			{
				if (m_Working != value)
				{
					m_Working = value;
					OnWorkingStatusChanged(this, new EventArgs());
				}
			}
		}

		public event EventHandler ConnectedChanged = null;

		public event EventHandler WorkingStatusChanged = null;

		public void SetConfig(string sConfig)
		{
			Config = sConfig;
			ParseConfig(sConfig);
		}

		protected abstract void ParseConfig(string sConfig);

		public virtual string GetConfig()
		{
			return JsonConvert.SerializeObject(this);
		}

		public virtual void OnConnectedChanged(object sender, EventArgs e)
		{
			EventHandler connectedChanged = this.ConnectedChanged;
			if (connectedChanged != null)
			{
				connectedChanged(sender, e);
			}
		}

		public virtual void OnWorkingStatusChanged(object sender, EventArgs e)
		{
			EventHandler workingStatusChanged = this.WorkingStatusChanged;
			if (workingStatusChanged != null)
			{
				workingStatusChanged(sender, e);
			}
		}
	}
}
