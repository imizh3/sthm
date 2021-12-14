using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace STHM
{
    public class Common
    {
        public DataSet ReadXMLFile(string xmlFilePath)
        {
            DataSet set = new DataSet();
            if (File.Exists(xmlFilePath))
            {
                set.ReadXml(xmlFilePath);
            }
            return set;
        }

        public DateTime RoundDown(DateTime dt, TimeSpan d)
        {
            long num = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - num);
        }

        public DateTime RoundToNearest(DateTime dt, TimeSpan d)
        {
            long num = dt.Ticks % d.Ticks;
            return ((num > (d.Ticks / 2L)) ? this.RoundUp(dt, d) : this.RoundDown(dt, d));
        }

        public DateTime RoundUp(DateTime dt, TimeSpan d)
        {
            long num = (d.Ticks - (dt.Ticks % d.Ticks)) % d.Ticks;
            return new DateTime(dt.Ticks + num);
        }

        public bool SaveXML(DataSet ds, string xmlFilePath)
        {
            try
            {
                ds.WriteXml(xmlFilePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public DialogResult ShowConfirm(string message)
        {
            return MessageBox.Show(message, "STHM", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        public void ShowDialog(string message)
        {
            MessageBox.Show(message, "STHM", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
    }
}
