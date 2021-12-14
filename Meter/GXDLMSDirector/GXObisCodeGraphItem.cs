namespace GXDLMSDirector
{
	public class GXObisCodeGraphItem
	{
		public string LogicalName { get; set; }

		public GXGraphItemCollection GraphItems { get; set; }

		public GXObisCodeGraphItem()
		{
			GraphItems = new GXGraphItemCollection();
		}
	}
}
