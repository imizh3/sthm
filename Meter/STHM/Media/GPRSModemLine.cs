using System.Net;
using Newtonsoft.Json;

namespace STHM.Media
{
	public class GPRSModemLine : CommunicationLine
	{
		public string DeviceId = "12345678";

		public string SimcardNo = "0123456789A";

		public bool HeartbeatEnable = true;

		public int HeartbeatInterval = 60;

		public string HeartbeatString = new string(new char[1] { 'þ' });

		[JsonIgnore]
		public string CSQ;

		[JsonIgnore]
		public EndPoint RemoteEndPoint;

		public GPRSModemLine()
		{
			LineType = "GPRSModemLine";
		}

		public GPRSModemLine(string _config)
		{
			LineType = "GPRSModemLine";
            SetConfig(_config);
		}

        protected override void ParseConfig(string _config)
		{
            GPRSModemLine gPRSModemLine = JsonConvert.DeserializeObject<GPRSModemLine>(_config);
			this.DeviceId = gPRSModemLine.DeviceId;
            this.SimcardNo = gPRSModemLine.SimcardNo;
            this.HeartbeatEnable = gPRSModemLine.HeartbeatEnable;
            this.HeartbeatInterval = gPRSModemLine.HeartbeatInterval;
            this.HeartbeatString = gPRSModemLine.HeartbeatString;
		}
	}
}
