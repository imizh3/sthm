#define DEBUG
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace GXDLMS.Common
{
	internal class Error
	{
		private delegate void ShowErrorEventHandler(IWin32Window owner, Exception Ex);

		private static void OnShowError(IWin32Window owner, Exception Ex)
		{
			MessageBox.Show(owner, Ex.Message);
		}

		public static void ShowError(IWin32Window owner, Exception Ex)
		{
			try
			{
				Debug.WriteLine(Ex.ToString());
			}
			catch
			{
			}
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
			if (!Directory.Exists(empty))
			{
				Directory.CreateDirectory(empty);
			}
			empty = Path.Combine(empty, "LastError.log");
			TextWriter textWriter = File.CreateText(empty);
			textWriter.Write(Ex.ToString());
			textWriter.Close();
			Control control = owner as Control;
			if (control != null && !control.IsDisposed && control.InvokeRequired)
			{
				control.Invoke(new ShowErrorEventHandler(OnShowError), owner, Ex);
			}
			else
			{
				MessageBox.Show(owner, Ex.Message);
			}
		}
	}
}
