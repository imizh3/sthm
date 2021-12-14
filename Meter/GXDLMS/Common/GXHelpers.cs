using System;
using System.Text;
using Gurux.DLMS;

namespace GXDLMS.Common
{
	public class GXHelpers
	{
		public static object ConvertFromDLMS(object data, DataType from, DataType type, bool arrayAsString)
		{
			switch (type)
			{
			default:
				if (from == DataType.OctetString && type == DataType.OctetString)
				{
					if (data is byte[])
					{
						string text = "";
						byte[] array = (byte[])data;
						if (array.Length == 0)
						{
							data = string.Empty;
						}
						else
						{
							byte[] array2 = array;
							byte[] array3 = array2;
							foreach (byte b in array3)
							{
								text = text + b + ".";
							}
							data = text.Substring(0, text.Length - 1);
						}
					}
				}
				else if (from == DataType.OctetString && type == DataType.String)
				{
					if (data is string)
					{
						return data;
					}
					if (data is byte[])
					{
						byte[] bytes = (byte[])data;
						data = Encoding.ASCII.GetString(bytes);
					}
				}
				else
				{
					DataType dataType = type;
					DataType dataType2 = dataType;
					if (dataType2 == DataType.DateTime)
					{
						if (data is byte[])
						{
							return GXDLMSClient.ChangeType((byte[])data, DataType.DateTime);
						}
						return data;
					}
					if (dataType2 == DataType.Date)
					{
						if (data is DateTime)
						{
							return data;
						}
						if (data is string)
						{
							return data;
						}
						if (!data.GetType().IsArray || ((Array)data).Length < 5)
						{
							throw new Exception("DateTime conversion failed. Invalid DLMS format.");
						}
						return GXDLMSClient.ChangeType((byte[])data, DataType.Date);
					}
					if (data is byte[])
					{
						data = ((type != DataType.String) ? ToHexString(data) : Encoding.ASCII.GetString((byte[])data));
					}
					else if (data is Array)
					{
						data = ArrayToString(data);
					}
				}
				return data;
			case DataType.Array:
				return data;
			case DataType.None:
				if (arrayAsString && data != null && data.GetType().IsArray)
				{
					data = GetArrayAsString(data);
				}
				return data;
			}
		}

		public static byte[] StringToByteArray(string hexString)
		{
			bool flag = hexString.Contains(".");
			if (string.IsNullOrEmpty(hexString))
			{
				return null;
			}
			string[] array = hexString.Split(flag ? '.' : ' ');
			byte[] array2 = new byte[array.Length];
			int num = -1;
			string[] array3 = array;
			string[] array4 = array3;
			foreach (string value in array4)
			{
				array2[++num] = Convert.ToByte(value, flag ? 10 : 16);
			}
			return array2;
		}

		public static string ToHexString(object data)
		{
			string text = string.Empty;
			if (data is Array)
			{
				Array array = (Array)data;
				for (long num = 0L; num != array.Length; num++)
				{
					long num2 = Convert.ToInt64(array.GetValue(num));
					text = text + Convert.ToString(num2, 16) + " ";
				}
				return text.TrimEnd();
			}
			return Convert.ToString(Convert.ToInt64(data), 16);
		}

		private static string ArrayToString(object data)
		{
			string text = "";
			if (data is Array)
			{
				Array array = (Array)data;
				for (long num = 0L; num != array.Length; num++)
				{
					object value = array.GetValue(num);
					text = ((!(value is Array)) ? (text + "{ " + Convert.ToString(value) + " }") : (text + "{ " + ArrayToString(value) + " }"));
				}
			}
			return text;
		}

		public static string GetArrayAsString(object data)
		{
			Array array = (Array)data;
			string text = null;
			foreach (object item in array)
			{
				text = ((text != null) ? (text + ", ") : "{");
				text = ((item == null || !item.GetType().IsArray) ? (text + Convert.ToString(item)) : (text + GetArrayAsString(item)));
			}
			if (text == null)
			{
				return "";
			}
			return text + "}";
		}

		public static string ConvertDLMS2String(object data)
		{
			if (data is DateTime)
			{
				DateTime dateTime = (DateTime)data;
				if (dateTime == DateTime.MinValue)
				{
					return "";
				}
				return dateTime.ToString();
			}
			if (data is byte[])
			{
				return BitConverter.ToString((byte[])data).Replace("-", " ");
			}
			return Convert.ToString(data);
		}

		public static Type FromDLMSDataType(DataType type)
		{
			switch (type)
			{
			case DataType.Boolean:
				return typeof(bool);
			case DataType.Int32:
				return typeof(int);
			case DataType.UInt32:
				return typeof(uint);
			case DataType.String:
				return typeof(string);
			case DataType.Int8:
				return typeof(short);
			case DataType.Int16:
				return typeof(short);
			case DataType.UInt8:
				return typeof(byte);
			case DataType.UInt16:
				return typeof(ushort);
			case DataType.Array:
			case DataType.CompactArray:
				return typeof(byte[]);
			case DataType.Int64:
				return typeof(long);
			case DataType.UInt64:
				return typeof(ulong);
			default:
				return null;
			case DataType.Float32:
				return typeof(float);
			case DataType.Float64:
				return typeof(double);
			case DataType.DateTime:
			case DataType.Date:
			case DataType.Time:
				return typeof(DateTime);
			}
		}

		public static bool IsNumeric(DataType type)
		{
			switch (type)
			{
			default:
				return false;
			case DataType.Int32:
			case DataType.UInt32:
			case DataType.String:
			case DataType.Int8:
			case DataType.Int16:
			case DataType.UInt8:
			case DataType.UInt16:
			case DataType.Int64:
			case DataType.UInt64:
			case DataType.Float32:
			case DataType.Float64:
				return true;
			}
		}

		public static DataType GetDLMSDataType(Type type)
		{
			if (type == typeof(int))
			{
				return DataType.Int32;
			}
			if (type == typeof(uint))
			{
				return DataType.UInt32;
			}
			if (type == typeof(string))
			{
				return DataType.String;
			}
			if (type == typeof(byte))
			{
				return DataType.UInt8;
			}
			if (type == typeof(sbyte))
			{
				return DataType.Int8;
			}
			if (type == typeof(short))
			{
				return DataType.Int16;
			}
			if (type == typeof(ushort))
			{
				return DataType.UInt16;
			}
			if (type == typeof(long))
			{
				return DataType.Int64;
			}
			if (type == typeof(ulong))
			{
				return DataType.UInt64;
			}
			if (type == typeof(float))
			{
				return DataType.Float32;
			}
			if (type == typeof(double))
			{
				return DataType.Float64;
			}
			if (type == typeof(DateTime))
			{
				return DataType.DateTime;
			}
			if (type == typeof(bool) || type == typeof(bool))
			{
				return DataType.Boolean;
			}
			throw new Exception("Failed to convert data type to DLMS data type. Unknown data type.");
		}
	}
}
