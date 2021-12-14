using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GXDLMSDirector
{
	public class GXValueSerializer : IXmlSerializable
	{
		private object _data;

		public object Data
		{
			get {return _data;}
			set {_data = value;}
		}

		public static implicit operator string(GXValueSerializer o)
		{
			return o.Data.ToString();
		}

		public static implicit operator GXValueSerializer(string o)
		{
			return (o == null) ? null : new GXValueSerializer(o);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public GXValueSerializer()
		{
		}

		public GXValueSerializer(string data)
		{
			_data = data;
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			reader.GetAttribute("xsi:type");
		}

		public void WriteXml(XmlWriter writer)
		{
			Type type = _data.GetType();
			writer.WriteAttributeString("xsi:type", type.FullName);
			new XmlSerializer(type).Serialize(writer, _data);
		}
	}
}
