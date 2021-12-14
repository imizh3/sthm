using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using STHM.Media;

namespace STHM.Device
{
    public abstract class ESMeterDevice
    {
        public delegate void TaskFunc(TaskInfo para = null);

        private ESMedia _media;

        public DataTable EventLogTypeTable;

        public TaskInfo TaskPara;

        public Dictionary<string, TaskFunc> m_arTaskFunc;

        public EnumWorkingStatus WorkingStatus;
        

        public ESMedia Media
        {
            get
            {
                return _media;
            }
            set
            {
                _media = value;
                _media.LineConfig.WorkingStatusChanged += LineConfig_WorkingStatusChanged;
            }
        }

        public event EventHandler WorkingStatusChanged = null;

        private void LineConfig_WorkingStatusChanged(object sender, EventArgs e)
        {
            OnWorkingStatusChanged(this, e);
        }

        public virtual void OnWorkingStatusChanged(object sender, EventArgs e)
        {
            if (this.WorkingStatusChanged != null)
            {
                this.WorkingStatusChanged(sender, e);
            }
        }

        public abstract void ReadCurrentRegisterValues(TaskInfo para = null);

        public abstract void ReadHistoricalRegister(TaskInfo para = null);

        public abstract void ReadLoadProfile(TaskInfo para = null);

        public abstract void ReadLoadProfile_A0(TaskInfo para = null);

        public abstract void ReadInstrumentationProfile(TaskInfo para = null);

        public abstract void ReadEventLog(TaskInfo para = null);

        public abstract void WriteSyncDateTime(TaskInfo para = null);

        private void Init()
        {
            m_arTaskFunc = new Dictionary<string, TaskFunc>();
            m_arTaskFunc.Add("ReadCurrentRegisterValues", ReadCurrentRegisterValues);
            m_arTaskFunc.Add("ReadHistoricalRegister", ReadHistoricalRegister);
            m_arTaskFunc.Add("ReadLoadProfile", ReadLoadProfile);
            m_arTaskFunc.Add("ReadLoadProfile_A0", ReadLoadProfile_A0);
            m_arTaskFunc.Add("ReadInstrumentationProfile", ReadInstrumentationProfile);
            m_arTaskFunc.Add("ReadEventLog", ReadEventLog);
            m_arTaskFunc.Add("WriteSyncDateTime", WriteSyncDateTime);
        }

        public ESMeterDevice()
        {
            Init();
        }

        public virtual void RunTask(TaskInfo para = null)
        {
            _media.LogBuffer.Length = 0;
            Media.LineConfig.WorkingStatus = EnumWorkingStatus.Working;
            WorkingStatus = EnumWorkingStatus.UnCompleted;
            if (para != null)
            {
                TaskPara = para;
            }
            try
            {
                if (m_arTaskFunc.ContainsKey(TaskPara.Taskname))
                {
                    m_arTaskFunc[TaskPara.Taskname]();
                }
            }
            catch (Exception ex)
            {
                Media.AddLog(MediaLog.MessageType.Error, "ESMeterDevice: RunTask->" + ex.Message);
            }
            finally
            {
                Media.LineConfig.WorkingStatus = WorkingStatus;
            }
        }

        private List<string> SerialToMaDiemDo(string serial)
        {
            List<string> list = new List<string>();
            DataSet dataSet = new DataSet();
            dataSet.ReadXml(Application.StartupPath + "/meterlist.cfg");
            for (int i = 0; i < dataSet.Tables["METER"].Rows.Count; i++)
            {
                if (dataSet.Tables["METER"].Rows[i]["MET_KEY"].ToString().Trim() == serial)
                {
                    list.Add(dataSet.Tables["METER"].Rows[i]["MADDO"].ToString());
                    list.Add(dataSet.Tables["METER"].Rows[i]["Tu"].ToString());
                    list.Add(dataSet.Tables["METER"].Rows[i]["Ti"].ToString());
                    list.Add(dataSet.Tables["METER"].Rows[i]["T"].ToString());
                    break;
                }
            }
            dataSet.Reset();
            dataSet.ReadXml(Application.StartupPath + "/Config.cfg");
            list.Add(dataSet.Tables["Table1"].Rows[0]["ConnectionString"].ToString());
            return list;
        }

        private int GiaTriG(string _chuKy)
        {
            int result = 0;
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("G", typeof(string));
            dataTable.Columns.Add("THOIGIAN", typeof(string));
            for (int i = 0; i < 48; i++)
            {
                DataRow row = dataTable.NewRow();
                dataTable.Rows.Add(row);
                dataTable.Rows[i]["G"] = "G" + (i + 1);
            }
            dataTable.Rows[0]["THOIGIAN"] = "0:30";
            dataTable.Rows[1]["THOIGIAN"] = "1:00";
            dataTable.Rows[2]["THOIGIAN"] = "1:30";
            dataTable.Rows[3]["THOIGIAN"] = "2:00";
            dataTable.Rows[4]["THOIGIAN"] = "2:30";
            dataTable.Rows[5]["THOIGIAN"] = "3:00";
            dataTable.Rows[6]["THOIGIAN"] = "3:30";
            dataTable.Rows[7]["THOIGIAN"] = "4:00";
            dataTable.Rows[8]["THOIGIAN"] = "4:30";
            dataTable.Rows[9]["THOIGIAN"] = "5:00";
            dataTable.Rows[10]["THOIGIAN"] = "5:30";
            dataTable.Rows[11]["THOIGIAN"] = "6:00";
            dataTable.Rows[12]["THOIGIAN"] = "6:30";
            dataTable.Rows[13]["THOIGIAN"] = "7:00";
            dataTable.Rows[14]["THOIGIAN"] = "7:30";
            dataTable.Rows[15]["THOIGIAN"] = "8:00";
            dataTable.Rows[16]["THOIGIAN"] = "8:30";
            dataTable.Rows[17]["THOIGIAN"] = "9:00";
            dataTable.Rows[18]["THOIGIAN"] = "9:30";
            dataTable.Rows[19]["THOIGIAN"] = "10:00";
            dataTable.Rows[20]["THOIGIAN"] = "10:30";
            dataTable.Rows[21]["THOIGIAN"] = "11:00";
            dataTable.Rows[22]["THOIGIAN"] = "11:30";
            dataTable.Rows[23]["THOIGIAN"] = "12:00";
            dataTable.Rows[24]["THOIGIAN"] = "12:30";
            dataTable.Rows[25]["THOIGIAN"] = "13:00";
            dataTable.Rows[26]["THOIGIAN"] = "13:30";
            dataTable.Rows[27]["THOIGIAN"] = "14:00";
            dataTable.Rows[28]["THOIGIAN"] = "14:30";
            dataTable.Rows[29]["THOIGIAN"] = "15:00";
            dataTable.Rows[30]["THOIGIAN"] = "15:30";
            dataTable.Rows[31]["THOIGIAN"] = "16:00";
            dataTable.Rows[32]["THOIGIAN"] = "16:30";
            dataTable.Rows[33]["THOIGIAN"] = "17:00";
            dataTable.Rows[34]["THOIGIAN"] = "17:30";
            dataTable.Rows[35]["THOIGIAN"] = "18:00";
            dataTable.Rows[36]["THOIGIAN"] = "18:30";
            dataTable.Rows[37]["THOIGIAN"] = "19:00";
            dataTable.Rows[38]["THOIGIAN"] = "19:30";
            dataTable.Rows[39]["THOIGIAN"] = "20:00";
            dataTable.Rows[40]["THOIGIAN"] = "20:30";
            dataTable.Rows[41]["THOIGIAN"] = "21:00";
            dataTable.Rows[42]["THOIGIAN"] = "21:30";
            dataTable.Rows[43]["THOIGIAN"] = "22:00";
            dataTable.Rows[44]["THOIGIAN"] = "22:30";
            dataTable.Rows[45]["THOIGIAN"] = "23:00";
            dataTable.Rows[46]["THOIGIAN"] = "23:30";
            dataTable.Rows[47]["THOIGIAN"] = "24:00";
            for (int j = 0; j < dataTable.Rows.Count; j++)
            {
                if (dataTable.Rows[j]["THOIGIAN"].ToString() == _chuKy)
                {
                    result = j + 1;
                    break;
                }
            }
            return result;
        }

        private DataSet Loadprofile(string _sourceFileName)
        {
            int num = 0;
            try
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("NGAY", typeof(string));
                dataTable.Columns.Add("SERIALID", typeof(string));
                dataTable.Columns.Add("MaDDo", typeof(string));
                dataTable.Columns.Add("Kenh", typeof(string));
                for (int i = 1; i < 49; i++)
                {
                    dataTable.Columns.Add("G" + i, typeof(double));
                }
                DataSet dataSet = new DataSet();
                if (!string.IsNullOrEmpty(_sourceFileName))
                {
                    List<string> list = new List<string>();
                    string value = "";
                    string value2 = "";
                    string text = "";
                    bool flag = false;
                    bool flag2 = false;
                    bool flag3 = true;
                    bool flag4 = false;
                    bool flag5 = false;
                    bool flag6 = false;
                    bool flag7 = false;
                    bool flag8 = false;
                    int num2 = 1;
                    int num3 = 0;
                    int num4 = 0;
                    double num5 = 1.0;
                    string[] array = Regex.Split(_sourceFileName, "\r\n");
                    DateTime dateTime = DateTime.Now;
                    DateTime dateTime2 = DateTime.Now;
                    int num6 = 0;
                    for (int j = 0; j < array.Length; j++)
                    {
                        num = j;
                        if (!flag5)
                        {
                            string[] array2 = array[1].Split(',');
                            text = array2[1].Trim();
                            for (int k = 2; k < array.Length; k++)
                            {
                                string[] array3 = array[k].Split(',');
                                if (array3.Length >= 2 || array.Length > 3)
                                {
                                    if (array3[0].ToString() == "E4" && Convert.ToDateTime(array3[1].ToString()).Date <= TaskPara.StartDate.Date && TaskPara.IsFromDate)
                                    {
                                        flag5 = true;
                                        j = k;
                                    }
                                    else if (array3[0].ToString() == "E4" && Convert.ToDateTime(array3[1].ToString()).Date <= DateTime.Now.Date.AddDays(-TaskPara.SetNumber) && !TaskPara.IsFromDate)
                                    {
                                        flag5 = true;
                                        j = k;
                                    }
                                }
                                else
                                {
                                    array[k] = string.Concat("E4,", TaskPara.StartDate.Date, ",");
                                    flag5 = true;
                                    j = k;
                                }
                            }
                        }
                        string[] array4 = array[j].Split(',');
                        if (!(array4[0].ToString() != ""))
                        {
                            continue;
                        }
                        if (flag3)
                        {
                            list = SerialToMaDiemDo(text);
                            if (list.Count > 0)
                            {
                                value = list[0];
                                Convert.ToDouble(list[1]);
                                Convert.ToDouble(list[2]);
                                num5 = Convert.ToDouble(list[3]);
                                flag3 = false;
                            }
                        }
                        if (array4[0].ToString() == "E4")
                        {
                            if (flag6)
                            {
                                num3 += 4;
                            }
                            value2 = array4[1];
                            string text2 = array4[2];
                            num6 = ((text2 == "033F99") ? 8 : ((!(text2 == "030399")) ? 4 : 4));
                            flag2 = true;
                            flag = false;
                            num4 = 0;
                            num2 = 1;
                            for (int l = 0; l < 4; l++)
                            {
                                DataRow row = dataTable.NewRow();
                                dataTable.Rows.Add(row);
                            }
                            for (int m = 0; m < dataTable.Rows.Count; m += 4)
                            {
                                dataTable.Rows[m]["Kenh"] = "KwhGiao";
                                dataTable.Rows[m + 1]["Kenh"] = "KwhNhan";
                                dataTable.Rows[m + 2]["Kenh"] = "KvarhGiao";
                                dataTable.Rows[m + 3]["Kenh"] = "KvarhNhan";
                            }
                            dataTable.Rows[num3]["NGAY"] = value2;
                            dataTable.Rows[num3 + 1]["NGAY"] = value2;
                            dataTable.Rows[num3 + 2]["NGAY"] = value2;
                            dataTable.Rows[num3 + 3]["NGAY"] = value2;
                            dataTable.Rows[num3]["SERIALID"] = text;
                            dataTable.Rows[num3 + 1]["SERIALID"] = text;
                            dataTable.Rows[num3 + 2]["SERIALID"] = text;
                            dataTable.Rows[num3 + 3]["SERIALID"] = text;
                            dataTable.Rows[num3]["MaDDo"] = value;
                            dataTable.Rows[num3 + 1]["MaDDo"] = value;
                            dataTable.Rows[num3 + 2]["MaDDo"] = value;
                            dataTable.Rows[num3 + 3]["MaDDo"] = value;
                            flag6 = true;
                            string item = (from x in array.ToList()
                                           where x.StartsWith("E8")
                                           select x).LastOrDefault();
                            int num7 = array.ToList().IndexOf(item);
                            if (num7 > 0)
                            {
                                string[] array5 = array[num7].Split(',');
                                DateTime dateTime3 = Convert.ToDateTime(array5[1]);
                                DateTime date = Convert.ToDateTime(value2).Date;
                                if (dateTime3 == date)
                                {
                                    j = num7 - 1;
                                }
                            }
                        }
                        else
                        {
                            if (array4[0].ToString() == "STHM Meter Reading")
                            {
                                continue;
                            }
                            if (array4[0].ToString() == "From unit")
                            {
                                text = array4[1].Trim().ToString();
                                continue;
                            }
                            if (array4[0].ToString() == "E8")
                            {
                                dateTime2 = Convert.ToDateTime(array4[1]);
                                flag8 = true;
                                continue;
                            }
                            if (flag8)
                            {
                                string chuKyTime = "";
                                int minute = dateTime2.Minute;
                                if (dateTime2.Minute < 30)
                                {
                                    chuKyTime = dateTime2.Hour + ":30";
                                }
                                if (dateTime2.Minute >= 30 && dateTime2.Second >= 0)
                                {
                                    chuKyTime = dateTime2.Hour + 1 + ":00";
                                }
                                int num8 = GiaTriG(chuKyTime);
                                if (array4[0].IndexOf('.') == 0)
                                {
                                    array4[0] = array4[0].Insert(0, "0");
                                }
                                if (array4[1].IndexOf('.') == 0)
                                {
                                    array4[1] = array4[1].Insert(0, "0");
                                }
                                if (array4[2].IndexOf('.') == 0)
                                {
                                    array4[2] = array4[2].Insert(0, "0");
                                }
                                if (array4[3].IndexOf('.') == 0)
                                {
                                    array4[3] = array4[3].Insert(0, "0");
                                }
                                dataTable.Rows[num3]["G" + num8] = Convert.ToDouble(array4[0].Trim().ToString()) * 0.5 * num5;
                                dataTable.Rows[num3 + 1]["G" + num8] = Convert.ToDouble(array4[1].Trim().ToString()) * 0.5 * num5;
                                dataTable.Rows[num3 + 2]["G" + num8] = Convert.ToDouble(array4[num6 - 2].Trim().ToString()) * 0.5 * num5;
                                dataTable.Rows[num3 + 3]["G" + num8] = Convert.ToDouble(array4[num6 - 1].Trim().ToString()) * 0.5 * num5;
                                num4 = num8 + 1;
                                num2++;
                                flag8 = false;
                                continue;
                            }
                            if (array4[0].ToString() == "EA")
                            {
                                dateTime = Convert.ToDateTime(array4[1]);
                                flag4 = true;
                                continue;
                            }
                            if (flag4)
                            {
                                if (array4.Length <= 3)
                                {
                                    flag4 = false;
                                    continue;
                                }
                                string chuKyTime2 = "";
                                int minute2 = dateTime.Minute;
                                if (dateTime.Minute < 30)
                                {
                                    chuKyTime2 = dateTime.Hour + ":30";
                                }
                                if (dateTime.Minute >= 30 && dateTime.Second >= 0)
                                {
                                    chuKyTime2 = dateTime.Hour + 1 + ":00";
                                }
                                int num9 = GiaTriG(chuKyTime2);
                                if (array4[0].IndexOf('.') == 0)
                                {
                                    array4[0] = array4[0].Insert(0, "0");
                                }
                                if (array4[1].IndexOf('.') == 0)
                                {
                                    array4[1] = array4[1].Insert(0, "0");
                                }
                                if (array4[2].IndexOf('.') == 0)
                                {
                                    array4[2] = array4[2].Insert(0, "0");
                                }
                                if (array4[3].IndexOf('.') == 0)
                                {
                                    array4[3] = array4[3].Insert(0, "0");
                                }
                                if (num9 >= num4)
                                {
                                    if (num9 != num4)
                                    {
                                        dataTable.Rows[num3]["G" + num9] = Convert.ToDouble(array4[0].Trim().ToString()) * 0.5 * num5;
                                        dataTable.Rows[num3 + 1]["G" + num9] = Convert.ToDouble(array4[1].Trim().ToString()) * 0.5 * num5;
                                        dataTable.Rows[num3 + 2]["G" + num9] = Convert.ToDouble(array4[num6 - 2].Trim().ToString()) * 0.5 * num5;
                                        dataTable.Rows[num3 + 3]["G" + num9] = Convert.ToDouble(array4[num6 - 1].Trim().ToString()) * 0.5 * num5;
                                        num4 = num9 + 1;
                                        num2++;
                                    }
                                    else
                                    {
                                        dataTable.Rows[num3]["G" + num9] = Convert.ToDouble(array4[0].Trim().ToString()) * 0.5 * num5 + Convert.ToDouble((dataTable.Rows[num3]["G" + num9].ToString() == "") ? ((object)0) : dataTable.Rows[num3]["G" + num9]);
                                        dataTable.Rows[num3 + 1]["G" + num9] = Convert.ToDouble(array4[1].Trim().ToString()) * 0.5 * num5 + Convert.ToDouble((dataTable.Rows[num3 + 1]["G" + num9].ToString() == "") ? ((object)0) : dataTable.Rows[num3 + 1]["G" + num9]);
                                        dataTable.Rows[num3 + 2]["G" + num9] = Convert.ToDouble(array4[num6 - 2].Trim().ToString()) * 0.5 * num5 + Convert.ToDouble((dataTable.Rows[num3 + 2]["G" + num9].ToString() == "") ? ((object)0) : dataTable.Rows[num3 + 2]["G" + num9]);
                                        dataTable.Rows[num3 + 3]["G" + num9] = Convert.ToDouble(array4[num6 - 1].Trim().ToString()) * 0.5 * num5 + Convert.ToDouble((dataTable.Rows[num3 + 3]["G" + num9].ToString() == "") ? ((object)0) : dataTable.Rows[num3 + 3]["G" + num9]);
                                        num4 = num9;
                                    }
                                }
                                else
                                {
                                    for (int n = 0; n < num4 - num9; n++)
                                    {
                                        string[] array6 = array[j + n].Split(',');
                                        if (array6[0].IndexOf('.') == 0)
                                        {
                                            array6[0] = array6[0].Insert(0, "0");
                                        }
                                        if (array6[1].IndexOf('.') == 0)
                                        {
                                            array6[1] = array6[1].Insert(0, "0");
                                        }
                                        if (array6[2].IndexOf('.') == 0)
                                        {
                                            array6[2] = array6[2].Insert(0, "0");
                                        }
                                        if (array6[3].IndexOf('.') == 0)
                                        {
                                            array6[3] = array6[3].Insert(0, "0");
                                        }
                                        dataTable.Rows[num3]["G" + (num9 + n)] = Convert.ToDouble(array6[0].Trim().ToString()) * 0.5 * num5 + Convert.ToDouble(dataTable.Rows[num3]["G" + (num9 + n)].ToString().Trim());
                                        dataTable.Rows[num3 + 1]["G" + (num9 + n)] = Convert.ToDouble(array6[1].Trim().ToString()) * 0.5 * num5 + Convert.ToDouble(dataTable.Rows[num3 + 1]["G" + (num9 + n)].ToString().Trim());
                                        dataTable.Rows[num3 + 2]["G" + (num9 + n)] = Convert.ToDouble(array6[num6 - 2].Trim().ToString()) * 0.5 * num5 + Convert.ToDouble(dataTable.Rows[num3 + 2]["G" + (num9 + n)].ToString().Trim());
                                        dataTable.Rows[num3 + 3]["G" + (num9 + n)] = Convert.ToDouble(array6[num6 - 1].Trim().ToString()) * 0.5 * num5 + Convert.ToDouble(dataTable.Rows[num3 + 3]["G" + (num9 + n)].ToString().Trim());
                                    }
                                    j = j + num4 - num9 - 1;
                                    num2 = num4;
                                }
                                flag4 = false;
                                continue;
                            }
                            if (array4[0].ToString() == "E5")
                            {
                                string chuKyTime3 = "";
                                for (int num10 = j + 1; num10 < array.Length; num10++)
                                {
                                    string[] array7 = array[num10 - 1].Split(',');
                                    if (array7[0] != "E6" || array7[0] != "E5")
                                    {
                                        string[] array8 = array[num10 - 1].Split(',');
                                        DateTime dateTime4 = default(DateTime);
                                        dateTime4 = Convert.ToDateTime(array8[1]);
                                        if (dateTime4.Minute < 30)
                                        {
                                            chuKyTime3 = dateTime4.Hour + ":30";
                                        }
                                        if (dateTime4.Minute >= 30 && dateTime4.Second >= 0)
                                        {
                                            chuKyTime3 = dateTime4.Hour + 1 + ":00";
                                        }
                                        num4 = GiaTriG(chuKyTime3);
                                        j = num10 - 1;
                                        flag = true;
                                        break;
                                    }
                                }
                                continue;
                            }
                            if (array4[0].ToString() == "E6")
                            {
                                string chuKyTime4 = "";
                                for (int num11 = j + 1; num11 < array.Length; num11++)
                                {
                                    string[] array9 = array[num11 - 1].Split(',');
                                    if (array9[0] != "E6" || array9[0] != "E5")
                                    {
                                        string[] array10 = array[num11 - 1].Split(',');
                                        DateTime dateTime5 = default(DateTime);
                                        dateTime5 = Convert.ToDateTime(array10[1]);
                                        if (dateTime5.Minute < 30)
                                        {
                                            chuKyTime4 = dateTime5.Hour + ":30";
                                        }
                                        if (dateTime5.Minute >= 30 && dateTime5.Second >= 0)
                                        {
                                            chuKyTime4 = dateTime5.Hour + 1 + ":00";
                                        }
                                        num4 = GiaTriG(chuKyTime4);
                                        j = num11 - 1;
                                        flag = true;
                                        break;
                                    }
                                }
                                continue;
                            }
                            if (array4[0].ToString() == "")
                            {
                                break;
                            }
                            if (flag)
                            {
                                if (num4 <= 48)
                                {
                                    if (flag2)
                                    {
                                        dataTable.Rows[num3]["NGAY"] = value2;
                                        dataTable.Rows[num3 + 1]["NGAY"] = value2;
                                        dataTable.Rows[num3 + 2]["NGAY"] = value2;
                                        dataTable.Rows[num3 + 3]["NGAY"] = value2;
                                        dataTable.Rows[num3]["SERIALID"] = text;
                                        dataTable.Rows[num3 + 1]["SERIALID"] = text;
                                        dataTable.Rows[num3 + 2]["SERIALID"] = text;
                                        dataTable.Rows[num3 + 3]["SERIALID"] = text;
                                        dataTable.Rows[num3]["MaDDo"] = value;
                                        dataTable.Rows[num3 + 1]["MaDDo"] = value;
                                        dataTable.Rows[num3 + 2]["MaDDo"] = value;
                                        dataTable.Rows[num3 + 3]["MaDDo"] = value;
                                        flag2 = false;
                                    }
                                    if (flag7)
                                    {
                                        if (array4[0].IndexOf('.') == 0)
                                        {
                                            array4[0] = array4[0].Insert(0, "0");
                                        }
                                        if (array4[1].IndexOf('.') == 0)
                                        {
                                            array4[1] = array4[1].Insert(0, "0");
                                        }
                                        if (array4[2].IndexOf('.') == 0)
                                        {
                                            array4[2] = array4[2].Insert(0, "0");
                                        }
                                        if (array4[3].IndexOf('.') == 0)
                                        {
                                            array4[3] = array4[3].Insert(0, "0");
                                        }
                                        dataTable.Rows[num3]["G" + num4] = Convert.ToDouble(array4[0].Trim().ToString()) * 0.5 * num5 + Convert.ToDouble(dataTable.Rows[num3]["G" + num4].ToString().Trim());
                                        dataTable.Rows[num3 + 1]["G" + num4] = Convert.ToDouble(array4[1]) * 0.5 * num5 + Convert.ToDouble(dataTable.Rows[num3 + 1]["G" + num4].ToString().Trim());
                                        dataTable.Rows[num3 + 2]["G" + num4] = Convert.ToDouble(array4[num6 - 2]) * 0.5 * num5 + Convert.ToDouble(dataTable.Rows[num3 + 2]["G" + num4].ToString().Trim());
                                        dataTable.Rows[num3 + 3]["G" + num4] = Convert.ToDouble(array4[num6 - 1]) * 0.5 * num5 + Convert.ToDouble(dataTable.Rows[num3 + 3]["G" + num4].ToString().Trim());
                                        num4++;
                                        flag7 = false;
                                        continue;
                                    }
                                    if (array4[0].IndexOf('.') == 0)
                                    {
                                        array4[0] = array4[0].Insert(0, "0");
                                    }
                                    dataTable.Rows[num3]["G" + num4] = Convert.ToDouble(array4[0].Trim().ToString()) * 0.5 * num5;
                                    if (array4[1].IndexOf('.') == 0)
                                    {
                                        array4[1] = array4[1].Insert(0, "0");
                                    }
                                    dataTable.Rows[num3 + 1]["G" + num4] = Convert.ToDouble(array4[1].Trim().ToString()) * 0.5 * num5;
                                    if (array4[2].IndexOf('.') == 0)
                                    {
                                        array4[2] = array4[2].Insert(0, "0");
                                    }
                                    dataTable.Rows[num3 + 2]["G" + num4] = Convert.ToDouble(array4[num6 - 2].Trim().ToString()) * 0.5 * num5;
                                    if (array4[3].IndexOf('.') == 0)
                                    {
                                        array4[3] = array4[3].Insert(0, "0");
                                    }
                                    dataTable.Rows[num3 + 3]["G" + num4] = Convert.ToDouble(array4[num6 - 1].Trim().ToString()) * 0.5 * num5;
                                }
                            }
                            else if (num2 <= 48)
                            {
                                if (flag2)
                                {
                                    dataTable.Rows[num3]["NGAY"] = value2;
                                    dataTable.Rows[num3 + 1]["NGAY"] = value2;
                                    dataTable.Rows[num3 + 2]["NGAY"] = value2;
                                    dataTable.Rows[num3 + 3]["NGAY"] = value2;
                                    dataTable.Rows[num3]["SERIALID"] = text;
                                    dataTable.Rows[num3 + 1]["SERIALID"] = text;
                                    dataTable.Rows[num3 + 2]["SERIALID"] = text;
                                    dataTable.Rows[num3 + 3]["SERIALID"] = text;
                                    dataTable.Rows[num3]["MaDDo"] = value;
                                    dataTable.Rows[num3 + 1]["MaDDo"] = value;
                                    dataTable.Rows[num3 + 2]["MaDDo"] = value;
                                    dataTable.Rows[num3 + 3]["MaDDo"] = value;
                                    flag2 = false;
                                }
                                if (array4[0].IndexOf('.') == 0)
                                {
                                    array4[0] = array4[0].Insert(0, "0");
                                }
                                dataTable.Rows[num3]["G" + num2] = Convert.ToDouble(array4[0]) * 0.5 * num5;
                                if (array4[1].IndexOf('.') == 0)
                                {
                                    array4[1] = array4[1].Insert(0, "0");
                                }
                                dataTable.Rows[num3 + 1]["G" + num2] = Convert.ToDouble(array4[1]) * 0.5 * num5;
                                if (array4[2].IndexOf('.') == 0)
                                {
                                    array4[2] = array4[2].Insert(0, "0");
                                }
                                dataTable.Rows[num3 + 2]["G" + num2] = Convert.ToDouble(array4[num6 - 2]) * 0.5 * num5;
                                if (array4[3].IndexOf('.') == 0)
                                {
                                    array4[3] = array4[3].Insert(0, "0");
                                }
                                dataTable.Rows[num3 + 3]["G" + num2] = Convert.ToDouble(array4[num6 - 1]) * 0.5 * num5;
                            }
                            num2++;
                            num4++;
                        }
                    }
                    for (int num12 = 1; num12 < 49; num12++)
                    {
                        for (int num13 = 0; num13 < 4; num13++)
                        {
                            if (dataTable.Rows.Count <= 0)
                            {
                                break;
                            }
                            if (dataTable.Rows == null)
                            {
                                break;
                            }
                            if (DateTime.Compare(Convert.ToDateTime(dataTable.Rows[num13]["NGAY"]).Date, DateTime.Now.Date) != 0 && dataTable.Rows[num13]["G" + num12].ToString() == "")
                            {
                                dataTable.Rows[num13]["G" + num12] = 0;
                            }
                        }
                    }
                }
                for (int num14 = 0; num14 < dataTable.Rows.Count; num14++)
                {
                    for (int num15 = 0; num15 < dataTable.Columns.Count; num15++)
                    {
                        if (DateTime.Compare(Convert.ToDateTime(dataTable.Rows[num14]["NGAY"]).Date, DateTime.Now.Date) != 0 && dataTable.Rows[num14][num15].ToString() == "")
                        {
                            dataTable.Rows[num14][num15] = 0;
                        }
                    }
                }
                dataSet.Tables.Add(dataTable);
                dataSet.Tables[0].TableName = "LOADPROFILE";
                return dataSet;
            }
            catch (Exception ex)
            {
                string value3 = string.Concat(DateTime.Now.ToString(), ex, num.ToString());
                string path = Path.Combine(Application.StartupPath, "Log_Services.txt");
                if (File.Exists(path))
                {
                    StreamWriter streamWriter = new StreamWriter(path, true);
                    streamWriter.WriteLine(value3);
                    streamWriter.Close();
                }
                else
                {
                    File.Create(path);
                    StreamWriter streamWriter2 = new StreamWriter(path);
                    streamWriter2.WriteLine(value3);
                    streamWriter2.Close();
                }
                return null;
            }
        }

        private void InsertCSTUCTHOI(string content)
        {
            try
            {
                string[] array = Regex.Split(content, "\r\n");
                string text = "";
                string text2 = "";
                string text3 = "";
                string text4 = "";
                string text5 = "";
                string text6 = "";
                string[] array2 = array[0].Split(',');
                text = array2[1].Trim();
                text2 = array2[2].Trim();
                text3 = array2[3];
                array2 = array[1].Split(',');
                text4 = array2[1].Trim();
                List<string> list = SerialToMaDiemDo(text);
                text4 = (Convert.ToDouble(text4) * Convert.ToDouble(list[3])).ToString();
                text5 = list[0];
                text6 = list[4];
                list.Clear();
                list.Add(text6);
                list.Add(text2);
                list.Add(text3);
                list.Add(text5);
                list.Add(text4);
                insertDB(list);
            }
            catch
            {
            }
        }

        private void insertDB(List<string> listDD)
        {
            try
            {
                CultureInfo cultureInfo = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                DateTime dateTime = default(DateTime);
                using (SqlConnection sqlConnection = new SqlConnection(listDD[0]))
                {
                    SqlDataReader sqlDataReader = null;
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand("P_CSTUCTHOI_InsertUpdate", sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    dateTime = DateTime.ParseExact(listDD[1], "MM/dd/yyyy HH:mm:ss", cultureInfo.DateTimeFormat);
                    sqlCommand.Parameters.Add(new SqlParameter("@Ngay", dateTime));
                    dateTime = DateTime.ParseExact(listDD[2], "MM/dd/yyyy HH:mm:ss", cultureInfo.DateTimeFormat);
                    sqlCommand.Parameters.Add(new SqlParameter("@MeterClock", dateTime));
                    sqlCommand.Parameters.Add(new SqlParameter("@MaDDo", listDD[3]));
                    sqlCommand.Parameters.Add(new SqlParameter("@ACTIVEPOWER", Convert.ToDecimal(listDD[4])));
                    sqlDataReader = sqlCommand.ExecuteReader();
                    sqlCommand.Parameters.Clear();
                    sqlDataReader.Close();
                    sqlConnection.Close();
                }
            }
            catch
            {
            }
        }

        private DataTable dtresult(DataTable dt, DateTime starttime, int setnumber)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (Convert.ToDateTime(dt.Rows[i]["NGAY"]).Date < starttime || Convert.ToDateTime(dt.Rows[i]["NGAY"]).Date > starttime.AddDays(setnumber - 2) || Convert.ToDateTime(dt.Rows[i]["NGAY"]).Date == DateTime.Now.Date)
                {
                    dt.Rows[i].Delete();
                    dt.AcceptChanges();
                    i--;
                }
            }
            for (int j = 0; j < setnumber - 1; j++)
            {
                bool flag = false;
                if (j != 0)
                {
                    starttime = starttime.AddDays(1.0);
                }
                if (starttime >= DateTime.Now.Date)
                {
                    break;
                }
                if (dt.Rows.Count == 0)
                {
                    flag = false;
                }
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    if (starttime == Convert.ToDateTime(dt.Rows[k]["NGAY"]).Date)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    continue;
                }
                for (int l = 0; l < 4; l++)
                {
                    int count = dt.Rows.Count;
                    DataRow row = dt.NewRow();
                    dt.Rows.Add(row);
                    if (l == 0)
                    {
                        dt.Rows[count]["NGAY"] = starttime;
                        dt.Rows[count]["SERIALID"] = "";
                        dt.Rows[count]["MaDDo"] = "";
                        dt.Rows[count]["Kenh"] = "KwhGiao";
                        for (int m = 1; m < 49; m++)
                        {
                            dt.Rows[count]["G" + m] = 0;
                        }
                    }
                    if (l == 1)
                    {
                        dt.Rows[count]["NGAY"] = starttime;
                        dt.Rows[count]["SERIALID"] = "";
                        dt.Rows[count]["MaDDo"] = "";
                        dt.Rows[count]["Kenh"] = "KwhNhan";
                        for (int n = 1; n < 49; n++)
                        {
                            dt.Rows[count]["G" + n] = 0;
                        }
                    }
                    if (l == 2)
                    {
                        dt.Rows[count]["NGAY"] = starttime;
                        dt.Rows[count]["SERIALID"] = "";
                        dt.Rows[count]["MaDDo"] = "";
                        dt.Rows[count]["Kenh"] = "KvarhGiao";
                        for (int num = 1; num < 49; num++)
                        {
                            dt.Rows[count]["G" + num] = 0;
                        }
                    }
                    if (l == 3)
                    {
                        dt.Rows[count]["NGAY"] = starttime;
                        dt.Rows[count]["SERIALID"] = "";
                        dt.Rows[count]["MaDDo"] = "";
                        dt.Rows[count]["Kenh"] = "KvarhNhan";
                        for (int num2 = 1; num2 < 49; num2++)
                        {
                            dt.Rows[count]["G" + num2] = 0;
                        }
                    }
                }
            }
            return dt;
        }

        private string AddlabelData(string[] AllLines)
        {
            try
            {
                List<string> list = new List<string>();
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                bool flag4 = false;
                DateTime dateTime = DateTime.Now;
                DateTime dateTime2 = DateTime.Now;
                DateTime dateTime3 = DateTime.Now;
                DateTime dateTime4 = DateTime.Now;
                int num = 0;
                list.Add(AllLines[0]);
                list.Add(AllLines[1]);
                for (int i = 2; i < AllLines.Length; i++)
                {
                    string[] array = AllLines[i].Split(',');
                    string text = array[0];
                    if (text != null && text.Length == 0)
                    {
                        continue;
                    }
                    switch (text)
                    {
                        case "E5":
                            {
                                dateTime = DateTime.Parse(array[1]);
                                int minute = dateTime.Minute;
                                if (minute > num && !flag3)
                                {
                                    int minute2 = dateTime.Minute;
                                    if (minute2 > num)
                                    {
                                        dateTime4 = dateTime4.Date.AddHours(dateTime.Hour + 1);
                                    }
                                }
                                if (minute < num && !flag3)
                                {
                                    dateTime4 = dateTime4.Date.AddHours(dateTime.Hour).AddMinutes(num);
                                }
                                flag2 = true;
                                continue;
                            }
                        case "E6":
                            dateTime2 = DateTime.Parse(array[1]);
                            flag3 = true;
                            continue;
                        case "E4":
                            if (array[2] == "030399" && !flag)
                            {
                                string[] array2 = Regex.Split(array[2], "0303");
                                if (array2[1] == "99")
                                {
                                    num = 30;
                                }
                                list.Add("StartDate,EndDate,Import kW,Export kW,Import kvar,Export kvar,flag");
                                flag = true;
                            }
                            dateTime4 = DateTime.Parse(array[1]);
                            continue;
                        case "EA":
                            dateTime3 = DateTime.Parse(array[1]);
                            flag4 = true;
                            continue;
                        case "A8":
                            continue;
                    }
                    if (flag2 && flag3)
                    {
                        list.Add(dateTime4.ToString() + "," + dateTime4.AddMinutes(num).ToString() + "," + AllLines[i]);
                        dateTime4 = dateTime4.AddMinutes(num);
                        flag2 = false;
                        flag3 = false;
                    }
                    else if (flag3)
                    {
                        list.Add(dateTime4.ToString() + "," + dateTime2.ToString() + "," + AllLines[i]);
                        dateTime4 = dateTime4.AddMinutes(num);
                        flag3 = false;
                    }
                    else if (flag2)
                    {
                        int minutes = (dateTime - dateTime4).Minutes;
                        if (minutes > 0)
                        {
                            list.Add(dateTime4.ToString() + "," + dateTime.ToString() + "," + AllLines[i]);
                            dateTime4 = dateTime4.AddMinutes(num);
                        }
                        if (minutes < 0)
                        {
                            list.Add(dateTime.ToString() + "," + dateTime4.ToString() + "," + AllLines[i]);
                        }
                        flag2 = false;
                    }
                    else if (flag4)
                    {
                        if (dateTime3.Minute > 30)
                        {
                            list.Add(dateTime3.ToString() + "," + dateTime4.ToString() + "," + AllLines[i]);
                        }
                        if (dateTime3.Minute <= 30)
                        {
                            list.Add(dateTime4.ToString() + "," + dateTime3.ToString() + "," + AllLines[i]);
                        }
                        flag4 = false;
                    }
                    else
                    {
                        list.Add(dateTime4.ToString() + "," + dateTime4.AddMinutes(num).ToString() + "," + AllLines[i]);
                        dateTime4 = dateTime4.AddMinutes(num);
                    }
                }
                string text2 = "";
                for (int j = 0; j < list.Count; j++)
                {
                    text2 = text2 + list[j] + "\r\n";
                }
                return text2;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void SaveFileSend(DataTable dt, string Osdatalink, string path)
        {
            try
            {
                int num = TaskPara.SetNumber;
                if (TaskPara.DeviceType.ToString() == "LandisGyr")
                {
                    num = TaskPara.SetNumber - 1;
                }
                if (TaskPara.IsFromDate)
                {
                    DateTime date = TaskPara.StartDate.Date;
                    dt = dtresult(dt, date, num + 1);
                }
                else
                {
                    DateTime starttime = DateTime.Now.Date.AddDays(-num - 1);
                    if (TaskPara.DeviceType.ToString() == "LandisGyr")
                    {
                        starttime = DateTime.Now.Date.AddDays(-num);
                    }
                    dt = dtresult(dt, starttime, num + 1);
                }
                if (dt.Rows.Count <= 0)
                {
                    return;
                }
                for (int i = 0; i < dt.Rows.Count; i += 4)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    string text = "";
                    string text2 = Convert.ToDateTime(dt.Rows[i]["NGAY"]).ToString("dd-MM-yy") + ",S102C003";
                    string text3 = "";
                    string text4 = "";
                    string text5 = Convert.ToDateTime(dt.Rows[i]["NGAY"]).Year.ToString().Substring(Convert.ToDateTime(dt.Rows[i]["NGAY"]).Year.ToString().Length - 1, 1);
                    text3 = ((Convert.ToDateTime(dt.Rows[i]["NGAY"]).Day >= 10) ? Convert.ToDateTime(dt.Rows[i]["NGAY"]).Day.ToString() : ("0" + Convert.ToDateTime(dt.Rows[i]["NGAY"]).Day));
                    text4 = ((Convert.ToDateTime(dt.Rows[i]["NGAY"]).Month >= 10) ? Convert.ToDateTime(dt.Rows[i]["NGAY"]).Month.ToString() : ("0" + Convert.ToDateTime(dt.Rows[i]["NGAY"]).Month));
                    string text6 = text3 + text4 + text5 + Osdatalink;
                    for (int j = 1; j < 49; j++)
                    {
                        text2 += ",0";
                    }
                    for (int k = i; k < i + 4; k++)
                    {
                        text = Convert.ToDateTime(dt.Rows[k]["NGAY"]).ToString("dd-MM-yy") + "," + dt.Rows[k]["Kenh"].ToString();
                        for (int l = 1; l < 49; l++)
                        {
                            text = (string.IsNullOrEmpty(dt.Rows[k]["G" + l].ToString()) ? (text + ",") : (text + "," + Math.Round(Convert.ToDecimal(dt.Rows[k]["G" + l]), 4)));
                        }
                        if (k == i + 2)
                        {
                            for (int m = 0; m < 5; m++)
                            {
                                stringBuilder.AppendLine(text2);
                            }
                            stringBuilder.AppendLine(text);
                        }
                        else if (k == i + 3)
                        {
                            stringBuilder.AppendLine(text);
                            for (int n = 0; n < 5; n++)
                            {
                                stringBuilder.AppendLine(text2);
                            }
                        }
                        else
                        {
                            stringBuilder.AppendLine(text);
                        }
                    }
                    if (!Directory.Exists(Application.StartupPath + "\\DataBase"))
                    {
                        Directory.CreateDirectory(Application.StartupPath + "\\DataBase");
                    }
                    if (File.Exists(Application.StartupPath + "\\DataBase\\" + text6 + ".sthm"))
                    {
                        File.Delete(Application.StartupPath + "\\DataBase\\" + text6 + ".sthm");
                    }
                    File.WriteAllText(Application.StartupPath + "\\DataBase\\" + text6 + ".sthm", stringBuilder.ToString());
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    if (File.Exists(path + "\\" + text6 + ".CSV"))
                    {
                        File.Delete(path + "\\" + text6 + ".CSV");
                    }
                    File.WriteAllText(path + "\\" + text6 + ".CSV", stringBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                string path2 = Path.Combine(Application.StartupPath, "Log_Services.txt");
                if (File.Exists(path2))
                {
                    StreamWriter streamWriter = new StreamWriter(path2, true);
                    streamWriter.WriteLine(string.Concat(ex, "xin chao save file send"));
                    streamWriter.Close();
                }
                else
                {
                    File.Create(path2);
                    StreamWriter streamWriter2 = new StreamWriter(path2, true);
                    streamWriter2.WriteLine(ex);
                    streamWriter2.Close();
                }
            }
        }

        public abstract void Export2CSV(string pathSchedule, string pathmanual, string pathexportcompany, bool CheckpathManual, bool bOpen = false);

        public virtual void Export2CSV(string pathSchedule, string pathmanual, string pathexportcompany, bool CheckpathManual, string content, bool bOpen = false)
        {
            Regex.Split(content, "\r\n");
            string savePath = "";
            if (!CheckpathManual)
            {
                savePath = pathSchedule;
            }
            else savePath = pathmanual;
            if (TaskPara.Taskname == "ReadLoadProfile_A0")
            {
                DataSet dataSet = Loadprofile(content);
                SaveFileSend(dataSet.Tables[0], TaskPara.DataLinkOSN, savePath);
                return;
            }
            string text = "";
            string text2 = TaskPara.StartDate.ToString("yyyyMMdd") + "\\" + TaskPara.MeterID + "\\";
            switch (TaskPara.Taskname)
            {
                case "ReadInstrumentationProfile":
                    text = savePath + "\\" + TaskPara.ExportFileCompany + "\\THONG SO LICH SU\\";
                    break;
                case "ReadLoadProfile":
                    text = savePath + "\\" + TaskPara.ExportFileCompany + "\\PHU TAI CONG SUAT\\";
                    break;
                case "ReadHistoricalRegister":
                    text = savePath + "\\" + TaskPara.ExportFileCompany + "\\CHI SO CHOT\\";
                    break;
                case "ReadCurrentRegisterValues":
                    text = savePath + "\\" + TaskPara.ExportFileCompany + "\\THONG SO VAN HANH\\";
                    break;
            }
            if (content.Length != 0)
            {
                if (!Directory.Exists(text + text2))
                {
                    Directory.CreateDirectory(text + text2);
                }
                string text3 = string.Format("{0}{1}_{2}_{3}.csv", text + text2, TaskPara.MeterID, TaskPara.Taskname, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                if (File.Exists(text3))
                {
                    File.Delete(text3);
                }
                File.WriteAllText(text3, content);
                if (bOpen)
                {
                    OpenFile(text3);
                }
            }
        }

        private Dictionary<string, string> StrHistorical()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("STHM Meter Reading", ",");
            dictionary.Add("From unit, " + TaskPara.MeterID + "," + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + ",", ",");
            return dictionary;
        }

        private string convertContentEDMI(string contentbuffer)
        {
            StringBuilder stringBuilder = new StringBuilder();
            contentbuffer = Regex.Replace(contentbuffer, "[]{\"}[]", string.Empty);
            stringBuilder.AppendLine("Historical Data,");
            string[] array = contentbuffer.Split(',');
            int num = 1;
            bool flag = false;
            string[] array2 = array;
            string[] array3 = array2;
            foreach (string text in array3)
            {
                string[] array4 = text.Split(':');
                if (array4[0].Trim() == "record_num")
                {
                    stringBuilder.AppendLine("Historical data set:," + num + "\r\n");
                    stringBuilder.AppendLine("Cumulative totals,\r\n");
                    stringBuilder.AppendLine(",Value,Unit,");
                }
                if (array4[0].Trim() == "DateTimeHistorical")
                {
                    Convert.ToDateTime(array4[1]).ToString();
                }
                if (array4[0].Trim() == "Content")
                {
                    flag = true;
                }
                else if (!flag)
                {
                }
            }
            return stringBuilder.ToString();
        }

        public static void OpenFile(string filePath)
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = filePath;
            process.Start();
        }
    }
}
