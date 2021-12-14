using System.Collections.Generic;

namespace GXDLMSDirector
{
	public class GXObisCodeGraphItemCollection : List<GXObisCodeGraphItem>
	{
		public GXObisCodeGraphItem Find(string logicalName)
		{
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					GXObisCodeGraphItem current = enumerator.Current;
					if (string.Compare(current.LogicalName, logicalName, true) == 0)
					{
						return current;
					}
				}
			}
			return null;
		}
	}
}
