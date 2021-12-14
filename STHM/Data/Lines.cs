using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using STHM.LIB;
namespace STHM.Data
{
    public class Lines
    {
        public Lines() { }
        public Lines(string LineID, string LineType, string Config)
        {
            this.LINE_ID = LineID;
            this.LINE_TYPE = LineType;
            this.CONFIG = Config;
            this.CONNECT = false;
            this.BUSY = false;
            this.CONNECTION_STATUS = "Chưa kết nối";
            this.READING_STATUS = "";
            this.RECONNECT_NUMBER = 0;
        }

        public void ToDataRow(DataRow r){
            try
            {
                List<Lines> listLines = new List<Lines>();
                listLines.Add(this);
                r.ItemArray = listLines.ToDataTable().Rows[0].ItemArray;
            }
            catch
            {
                
            }
        }

        public bool BUSY { get; set; }

        public string CONFIG { get; set; }

        public bool CONNECT { get; set; }

        public string CONNECTION_STATUS { get; set; }

        public string LINE_ID { get; set; }

        public string LINE_TYPE { get; set; }

        public string READING_STATUS { get; set; }

        public int RECONNECT_NUMBER { get; set; }
    }
}

