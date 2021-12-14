using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Gurux.DLMS;
using Gurux.DLMS.Objects;

namespace GXDLMS.Common
{
	public class GXDLMSObjectSerializer<AbstractType> : IXmlSerializable
	{
		private static Dictionary<Type, XmlSerializer> s = new Dictionary<Type, XmlSerializer>();

		private AbstractType _data;

		public AbstractType Data
		{
			get
			{
				return _data;
			}
			set
			{
				_data = value;
			}
		}

		public static implicit operator AbstractType(GXDLMSObjectSerializer<AbstractType> o)
		{
			return o.Data;
		}

		public static implicit operator GXDLMSObjectSerializer<AbstractType>(AbstractType o)
		{
			return (o == null) ? null : new GXDLMSObjectSerializer<AbstractType>(o);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public GXDLMSObjectSerializer()
		{
		}

		public GXDLMSObjectSerializer(AbstractType data)
		{
			_data = data;
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			string text = reader.GetAttribute("xsi:type");
			if (text == null)
			{
				throw new ArgumentNullException("Unable to Read Xml Data for Abstract Type '" + typeof(AbstractType).Name + "' because no 'type' attribute was specified in the XML.");
			}
			if (text.StartsWith("GXDLMSDirector."))
			{
				text = text.Replace("GXDLMSDirector.", "Gurux.DLMS.Objects.");
			}
			Type type = typeof(GXDLMSClient).Assembly.GetType(text);
			if (type == null)
			{
				type = Type.GetType(text);
			}
			if (type == null)
			{
				throw new InvalidCastException("Unable to Read Xml Data for Abstract Type '" + typeof(AbstractType).Name + "' because the type specified in the XML was not found.");
			}
			if (!type.IsSubclassOf(typeof(AbstractType)))
			{
				throw new InvalidCastException("Unable to Read Xml Data for Abstract Type '" + typeof(AbstractType).Name + "' because the Type specified in the XML differs ('" + type.Name + "').");
			}
			reader.ReadStartElement();
			XmlSerializer xmlSerializer;
			if (!s.ContainsKey(type))
			{
				xmlSerializer = new XmlSerializer(type, GXDLMSClient.GetObjectTypes());
				s[type] = xmlSerializer;
			}
			else
			{
				xmlSerializer = s[type];
			}
			Data = (AbstractType)xmlSerializer.Deserialize(reader);
			reader.ReadEndElement();
		}

		public void WriteXml(XmlWriter writer)
		{
			Type type = _data.GetType();
			writer.WriteAttributeString("xsi:type", type.FullName);
			XmlAttributeOverrides xmlAttributeOverrides = new XmlAttributeOverrides();
			XmlAttributes xmlAttributes = new XmlAttributes();
			xmlAttributes.XmlIgnore = true;
			xmlAttributeOverrides.Add(typeof(GXDLMSObject), "Description", xmlAttributes);
			XmlSerializer xmlSerializer = new XmlSerializer(type, xmlAttributeOverrides);
			xmlSerializer.Serialize(writer, _data);
		}
	}
}
