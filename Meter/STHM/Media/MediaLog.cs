using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace STHM.Media
{
	public class MediaLog
	{
		public enum MessageType
		{
			Incoming,
			Outgoing,
			Normal,
			Warning,
			Error
		}

		public static RichTextBox DisplayWindow = null;

		private static Color[] MessageColor = new Color[5]
		{
			Color.Blue,
			Color.Green,
			Color.Black,
			Color.Orange,
			Color.Red
		};

		public RichTextBox RTBLog;

		[STAThread]
		public static void DisplayData(MessageType type, string msg)
		{
			if (type == MessageType.Error || DisplayWindow == null)
			{
				return;
			}
			if (DisplayWindow.InvokeRequired)
			{
				DisplayWindow.BeginInvoke((MethodInvoker)delegate
				{
					DisplayData(type, msg);
				});
				return;
			}
			try
			{
				DisplayWindow.SelectedText = string.Empty;
				DisplayWindow.SelectionFont = new Font(DisplayWindow.SelectionFont, FontStyle.Bold);
				DisplayWindow.SelectionColor = MessageColor[(int)type];
				string text = msg;
				text = CreateLog(msg);
				DisplayWindow.AppendText(text);
				DisplayWindow.ScrollToCaret();
			}
			catch (Exception)
			{
			}
		}

		public void AddLog(MessageType type, string msg)
		{
			if (type == MessageType.Error)
			{
				return;
			}
			if (RTBLog != null)
			{
				if (RTBLog.InvokeRequired)
				{
					RTBLog.BeginInvoke((MethodInvoker)delegate
					{
						AddLog(type, msg);
					});
					return;
				}
				try
				{
					RTBLog.SelectedText = string.Empty;
					RTBLog.SelectionFont = new Font(RTBLog.SelectionFont, FontStyle.Bold);
					RTBLog.SelectionColor = MessageColor[(int)type];
					string text = msg;
					text = CreateLog(msg);
					RTBLog.AppendText(text);
					RTBLog.ScrollToCaret();
				}
				catch (Exception)
				{
				}
			}
			else
			{
				DisplayData(type, msg);
			}
		}

		public static string CreateLog(string msg)
		{
			StringBuilder stringBuilder = new StringBuilder(msg);
			string oldValue = new string('\u0006', 1);
			stringBuilder = stringBuilder.Replace(oldValue, "<ACK>");
			oldValue = new string('\u0015', 1);
			stringBuilder = stringBuilder.Replace(oldValue, "<NAK>");
			oldValue = new string('\u0002', 1);
			stringBuilder = stringBuilder.Replace(oldValue, "<STX>");
			oldValue = new string('\u0003', 1);
			stringBuilder = stringBuilder.Replace(oldValue, "<ETX>");
			oldValue = new string('\u0001', 1);
			stringBuilder = stringBuilder.Replace(oldValue, "<SOH>");
			oldValue = new string('\r', 1);
			stringBuilder = stringBuilder.Replace(oldValue, "<CR>");
			oldValue = new string('\n', 1);
			stringBuilder = stringBuilder.Replace(oldValue, "<LF>");
			oldValue = stringBuilder.ToString();
			return oldValue + "\n";
		}
	}
}
