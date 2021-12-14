using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace GXDLMSDirector
{
	public class GXGraphItem
	{
		[XmlIgnore]
		public Color Color { get; set; }

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public int Argb
		{
			get
			{
				return Color.ToArgb();
			}
			set
			{
				if (value == 0)
				{
					Color = Color.Empty;
				}
				else
				{
					Color = Color.FromArgb(value);
				}
			}
		}

		[DefaultValue(true)]
		public bool Enabled { get; set; }

		[Browsable(false)]
		public string LogicalName { get; set; }

		[Browsable(false)]
		public int AttributeIndex { get; set; }

		public GXGraphItem()
		{
			Enabled = true;
		}
	}
}
