using System;

namespace STHM.Device
{
    public class TaskInfo : ICloneable
    {
        public bool isAuto = true;
        public int Taskid = 0;

        public string MeterID = "";

        public string OutStationNumber = "";

        public string UserID = "";

        public string Password = "";

        public string DeviceType = "";

        public string Taskname = "";

        public int SetNumber = 1;

        public DateTime StartDate;

        public bool IsFromDate;

        public string DataLinkOSN = "";

        public string VersionEDMI = "";

        public string ExportFileCompany = "";

        public bool checkCurrentValuesVCGM = false;

        public TaskInfo()
        {
            SetNumber = 1;
            IsFromDate = false;
            StartDate = DateTime.Now;
            
        }

        public object Clone()
        {
            return new TaskInfo
            {
                isAuto = isAuto,
                Taskid = Taskid,
                MeterID = MeterID,
                OutStationNumber = OutStationNumber,
                UserID = UserID,
                Password = Password,
                DeviceType = DeviceType,
                Taskname = Taskname,
                SetNumber = SetNumber,
                StartDate = StartDate,
                IsFromDate = IsFromDate,
                DataLinkOSN = DataLinkOSN,
                VersionEDMI = VersionEDMI,
                ExportFileCompany = ExportFileCompany,
                checkCurrentValuesVCGM = checkCurrentValuesVCGM
            };
        }
    }
}
