using Newtonsoft.Json;

namespace STHM.Media
{
	public class IPPortLine : CommunicationLine
	{
		public string Address;

		public int Port;

		public IPPortLine()
		{
			LineType = "IPPortLine";
		}

        public IPPortLine(string _config)
		{
			LineType = "IPPortLine";
            SetConfig(_config);
		}

		protected override void ParseConfig(string _config)
		{
            IPPortLine iPPortLine = JsonConvert.DeserializeObject<IPPortLine>(_config);
			this.Address = iPPortLine.Address;
			this.Port = iPPortLine.Port;
		}
	}
}
