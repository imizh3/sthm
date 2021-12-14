using System.IO.Ports;
using Newtonsoft.Json;

namespace STHM.Media
{
	public class SerialPortLine : CommunicationLine
	{
		public int baudRate = 9600;

		public int dataBits = 7;

		public Parity parity = Parity.Even;

		public StopBits stopBits = StopBits.One;

		public string portName = "COM1";

		public SerialPortLine()
		{
			LineType = "SerialPortLine";
		}

		public SerialPortLine(string sConfig)
		{
			LineType = "SerialPortLine";
			SetConfig(sConfig);
		}

		protected override void ParseConfig(string sConfig)
		{
			SerialPortLine serialPortLine = JsonConvert.DeserializeObject<SerialPortLine>(sConfig);
			baudRate = serialPortLine.baudRate;
			parity = serialPortLine.parity;
			stopBits = serialPortLine.stopBits;
			dataBits = serialPortLine.dataBits;
			portName = serialPortLine.portName;
		}
	}
}
