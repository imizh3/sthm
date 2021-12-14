using System;
using System.Text;

namespace STHMServer
{
	public static class Encryption
	{
		public static string Boring(string st)
		{
			for (int i = 0; i < st.Length; i++)
			{
				int startIndex = i * Convert.ToUInt16(st[i]);
				startIndex %= st.Length;
				char ch = st[i];
				st = st.Remove(i, 1);
				st = st.Insert(startIndex, ch.ToString());
			}
			return st;
		}

		private static char ChangeChar(char ch, int[] EnCode)
		{
			ch = char.ToUpper(ch);
			if (ch >= 'A' && ch <= 'H')
			{
				return Convert.ToChar(Convert.ToInt16(ch) + 2 * EnCode[0]);
			}
			if (ch >= 'I' && ch <= 'P')
			{
				return Convert.ToChar(Convert.ToInt16(ch) - EnCode[2]);
			}
			if (ch >= 'Q' && ch <= 'Z')
			{
				return Convert.ToChar(Convert.ToInt16(ch) - EnCode[1]);
			}
			if (ch >= '0' && ch <= '4')
			{
				return Convert.ToChar(Convert.ToInt16(ch) + 5);
			}
			if (ch >= '5' && ch <= '9')
			{
				return Convert.ToChar(Convert.ToInt16(ch) - 5);
			}
			return '0';
		}

		public static string ConvertToLetterDigit(string st)
		{
			StringBuilder builder = new StringBuilder();
			foreach (char ch in st)
			{
				if (!char.IsLetterOrDigit(ch))
				{
					builder.Append(Convert.ToInt16(ch).ToString());
				}
				else
				{
					builder.Append(ch);
				}
			}
			return builder.ToString();
		}

		public static string InverseByBase(string st, int MoveBase)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < st.Length; i += MoveBase)
			{
				int num = ((i + MoveBase <= st.Length - 1) ? MoveBase : (st.Length - i));
				builder.Append(InverseString(st.Substring(i, num)));
			}
			return builder.ToString();
		}

		public static string InverseString(string st)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = st.Length - 1; i >= 0; i--)
			{
				builder.Append(st[i]);
			}
			return builder.ToString();
		}

		public static string MakePassword(string st, string Identifier)
		{
			int[] enCode = new int[Identifier.Length];
			st = Boring(st);
			for (int i = 0; i < Identifier.Length; i++)
			{
				int moveBase = Convert.ToInt32(Identifier[i].ToString(), 10);
				st = InverseByBase(st, moveBase);
			}
			StringBuilder builder = new StringBuilder();
			string text = st;
			string text2 = text;
			foreach (char ch2 in text2)
			{
				builder.Append(ChangeChar(ch2, enCode));
			}
			return builder.ToString();
		}
	}
}
