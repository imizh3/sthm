namespace STHM.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using STHM.LIB;

    public class Meter
    {
        public Meter()
        {
        }

        public Meter(string MetID, string metkey, string maddo, string tenddo, string DatalinkOSN, string progpw, string outstation, int baudrate, int regnum, string mettype, int tu, int ti, int t, string commline)
        {
            this.MET_ID = MetID;
            this.MET_KEY = metkey;
            this.MADDO = maddo;
            this.TENDDO = tenddo;
            this.DATALINK_OSN = DatalinkOSN;
            this.PROG_PW = progpw;
            this.OUT_STATION_NUMBER = outstation;
            this.BAUD_RATE = baudrate;
            this.REG_NUM = regnum;
            this.MET_TYPE = mettype;
            this.TU = tu;
            this.TI = ti;
            this.T = t;
            this.COMM_LINE = commline;
        }

        public void ToDataRow(DataRow r)
        {
            try
            {
                List<Meter> listMeter = new List<Meter>();
                listMeter.Add(this);
                r.ItemArray = listMeter.ToDataTable().Rows[0].ItemArray;
            }
            catch
            {

            }
        }

        public int BAUD_RATE { get; set; }

        public string COMM_LINE { get; set; }

        public string DATALINK_OSN { get; set; }

        public string MADDO { get; set; }

        public string MET_ID { get; set; }

        public string MET_KEY { get; set; }

        public string MET_TYPE { get; set; }

        public string OUT_STATION_NUMBER { get; set; }

        public string PROG_PW { get; set; }

        public int REG_NUM { get; set; }

        public int T { get; set; }

        public string TENDDO { get; set; }

        public int TI { get; set; }

        public int TU { get; set; }
    }
}

