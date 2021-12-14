using System.Collections.Generic;

namespace GXDLMSDirector
{
	public class GXGraphItemCollection : List<GXGraphItem>
	{
		public GXGraphItem Find(string name, int attributeIndex)
		{
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					GXGraphItem current = enumerator.Current;
					if (string.Compare(current.LogicalName, name, true) == 0 && current.AttributeIndex == attributeIndex)
					{
						return current;
					}
				}
			}
			return null;
		}
	}
}
