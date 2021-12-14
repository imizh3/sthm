using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace STHM.Data
{
    public class Schedule
    {
        public string DATALINK_OSN { get; set; }

        public bool ENABLED { get; set; }

        public DateTime F_EXCEPT_FROM { get; set; }

        public DateTime F_EXCEPT_TO { get; set; }

        public bool F_EXCEPTION { get; set; }

        public int F_ID { get; set; }

        public string F_MET_KEY { get; set; }

        public int F_PERIOD_VALUE { get; set; }

        public int F_READING_COUNT { get; set; }

        public int F_READING_STATUS { get; set; }

        public bool IS_FROM_STARTDATE { get; set; }

        public string LINE_ID { get; set; }

        public string LINE_TYPE { get; set; }

        public string MET_KEY { get; set; }

        public string MET_TYPE { get; set; }

        public string OUT_STATION_NUMBER { get; set; }

        public string PROG_PW { get; set; }

        public int SET_NUMBER { get; set; }

        public DateTime STARTDATE { get; set; }

        public int TASK_ID { get; set; }

        public string TASK_NAME { get; set; }
    }
}

