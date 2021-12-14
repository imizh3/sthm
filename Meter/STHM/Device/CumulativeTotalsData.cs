using System;
using System.Collections.Generic;

namespace STHM.Device
{
	public class CumulativeTotalsData
	{
		public string meterserial;

		public DateTime readingserverclock;

		public DateTime meterclock;

		public Dictionary<string, string> dictData;

		public string[] dataName = new string[10] { "ImportWh", "ExportWh", "Q1varh", "Q2varh", "Q3varh", "Q4varh", "VAh", "CD1", "CD2", "CD3" };

		public CumulativeTotalsData(string serial, DateTime readingclock)
		{
			meterserial = serial;
			readingserverclock = readingclock;
			dictData = new Dictionary<string, string>();
		}

		public string GetData(string name)
		{
			if (dictData != null && dictData.ContainsKey(name))
			{
				return dictData[name];
			}
			return "";
		}
	}
}
