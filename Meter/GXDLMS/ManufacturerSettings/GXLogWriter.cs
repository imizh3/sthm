#define DEBUG
#define TRACE
using System;
using System.Diagnostics;
using System.IO;
using Gurux.Common;

namespace GXDLMS.ManufacturerSettings
{
	internal class GXLogWriter
	{
		public static string LogPath
		{
			get
			{
				string empty = string.Empty;
				if (Environment.OSVersion.Platform == PlatformID.Unix)
				{
					empty = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
					empty = Path.Combine(empty, ".Gurux");
				}
				else
				{
					empty = ((Environment.OSVersion.Version.Major < 6) ? Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles) : Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
					empty = Path.Combine(empty, "Gurux");
				}
				empty = Path.Combine(empty, "GXDLMSDirector");
				return Path.Combine(empty, "GXDLMSDirector.log");
			}
		}

		public static void WriteLog(string data)
		{
			if (data != null)
			{
				Trace.WriteLine(DateTime.Now.ToLongTimeString() + " " + data.Replace("\r", "<CR>").Replace("\n", "<LF>"));
			}
		}

		public static void WriteLog(string text, byte[] value)
		{
			string text2 = DateTime.Now.ToLongTimeString() + " " + text;
			if (value != null)
			{
				text2 = text2 + "\r\n" + BitConverter.ToString(value).Replace('-', ' ');
			}
			Trace.WriteLine(text2);
		}

		public static void ClearLog()
		{
			foreach (TraceListener listener in Trace.Listeners)
			{
				if (listener is TextWriterTraceListener)
				{
					Trace.Flush();
					listener.Flush();
					if (((TextWriterTraceListener)listener).Writer != null)
					{
						((TextWriterTraceListener)listener).Writer.Close();
					}
					((TextWriterTraceListener)listener).Writer = new StreamWriter(LogPath);
					GXFileSystemSecurity.UpdateFileSecurity(LogPath);
					break;
				}
			}
			Debug.WriteLine("Log created " + DateTime.Now.ToLongTimeString());
		}
	}
}
