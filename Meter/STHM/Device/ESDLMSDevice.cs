using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using Gurux.DLMS;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.DLMS.Objects;
using GXDLMS.Common;
using GXDLMSDirector;
using STHM.Media;
using System.Xml;

namespace STHM.Device
{
	public class ESDLMSDevice : ESMeterDevice
	{
		private Dictionary<string, string> dictEvent;

		public string ObisProfilePath = "";

		public bool m_UseESMedia = false;

		private GXDLMSDeviceCollection Devices;

		private GXManufacturerCollection Manufacturers;

		private List<string> resultList = new List<string>();

		private StringBuilder outstringLandisData = new StringBuilder();

		private object[] parameterdevice;

		private bool debuggingLog = false;

		public Dictionary<string, string> DictEvent
		{
			get
			{
				return dictEvent;
			}
			set
			{
				dictEvent = value;
			}
		}

		public string ObisProfile
		{
			get
			{
				return Path.Combine(ObisProfilePath, TaskPara.Taskname + ".gxc");
			}
		}

		public ESDLMSDevice(bool debuggingLog = false)
		{
			this.debuggingLog = debuggingLog;
		}

		private object[] LoadFile(string path, object physicaladdress)
		{
			object[] result;
            using (TextReader textReader = new StreamReader(path,Encoding.UTF8))
            {
                List<Type> list = new List<Type>(GXDLMSClient.GetObjectTypes());
                list.Add(typeof(GXDLMSAttributeSettings));
                list.Add(typeof(GXDLMSAttribute));
                list.Add(typeof(GXDLMSProfileGeneric));
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(GXDLMSDeviceCollection), list.ToArray());             
                Devices = (GXDLMSDeviceCollection)xmlSerializer.Deserialize(textReader);
                Devices[0].PhysicalAddress = Convert.ToUInt32(physicaladdress);
                object[] array = Devices.ToArray();
                object[] array2 = array;
                result = array2;
            }
			foreach (GXDLMSDevice device in Devices)
			{
				device.Manufacturers = Manufacturers;
				GXManufacturer gXManufacturer = Manufacturers.FindByIdentification(device.Manufacturer);
				if (gXManufacturer != null)
				{
					device.ObisCodes = gXManufacturer.ObisCodes;
					foreach (GXDLMSObject @object in device.Objects)
					{
						if (string.IsNullOrEmpty(@object.Description))
						{
							break;
						}
						@object.UpdateDefaultValueItems();
						if (!(@object is GXDLMSProfileGeneric))
						{
							continue;
						}
						int num = 0;
						foreach (GXDLMSObject column in ((GXDLMSProfileGeneric)@object).Columns)
						{
							DataColumn dataColumn;
							if (column.ObjectType == ObjectType.All)
							{
								num++;
								dataColumn = ((GXDLMSProfileGeneric)@object).Data.Columns.Add("O" + num);
								dataColumn.Caption = column.Description;
								if (string.IsNullOrEmpty(dataColumn.Caption))
								{
									dataColumn.Caption = column.LogicalName;
								}
								IGXDLMSColumnObject iGXDLMSColumnObject = column;
								DataType dataType = column.GetDataType(iGXDLMSColumnObject.SelectedAttributeIndex);
								if (dataType != 0)
								{
									dataColumn.DataType = GXHelpers.FromDLMSDataType(dataType);
								}
								continue;
							}
							GXDLMSObject gXDLMSObject = device.Objects.FindByLN(column.ObjectType, column.LogicalName);
							if (gXDLMSObject == null)
							{
								gXDLMSObject = GXDLMSDevice.ConvertObject2Class(device, column.ObjectType, column.LogicalName);
							}
							num++;
							dataColumn = ((GXDLMSProfileGeneric)@object).Data.Columns.Add("O" + num);
							if (gXDLMSObject != null)
							{
								dataColumn.Caption = gXDLMSObject.LogicalName + " " + gXDLMSObject.Description;
							}
							else
							{
								dataColumn.Caption = column.LogicalName;
							}
							IGXDLMSColumnObject iGXDLMSColumnObject2 = column;
							if ((iGXDLMSColumnObject2.SelectedAttributeIndex & 8) == 0 && (iGXDLMSColumnObject2.SelectedAttributeIndex & 0x10) == 0)
							{
								Type type = GXHelpers.FromDLMSDataType(column.GetDataType(iGXDLMSColumnObject2.SelectedAttributeIndex));
								if (type != null)
								{
									dataColumn.DataType = type;
								}
							}
							if (gXDLMSObject != null)
							{
								column.Description = gXDLMSObject.Description;
							}
							if (gXDLMSObject is GXDLMSRegister)
							{
								GXDLMSRegister gXDLMSRegister = gXDLMSObject as GXDLMSRegister;
								GXDLMSRegister gXDLMSRegister2 = column as GXDLMSRegister;
								gXDLMSRegister.Unit = gXDLMSRegister2.Unit;
								IGXDLMSColumnObject iGXDLMSColumnObject3 = column;
								if (iGXDLMSColumnObject3.SelectedAttributeIndex == 2)
								{
									gXDLMSRegister.Scaler = gXDLMSRegister2.Scaler;
								}
							}
						}
					}
					continue;
				}
				throw new Exception("Load failed. Invalid manufacturer: " + device.Manufacturer);
			}
			return result;
		}

		private void LoadManufacturer(string deviceType)
		{
			Manufacturers = new GXManufacturerCollection();
            //GXManufacturerCollection.ReadManufacturerSettings(Manufacturers);
            if (deviceType == "LandisGyr")
            {
                using (TextReader textReader = new StreamReader(System.Windows.Forms.Application.StartupPath + "\\" + "lgz.obx", Encoding.UTF8))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(GXManufacturer));
                    GXManufacturer manus = (GXManufacturer)xmlSerializer.Deserialize(textReader);
                    Manufacturers.Add(manus);
                }  
            }      
		}

		private bool Connect(object[] parameters)
		{
			bool result = true;
			try
			{
				object obj = parameters[0];
				if (obj is GXDLMSDeviceCollection)
				{
					int count = ((GXDLMSDeviceCollection)obj).Count;
					foreach (GXDLMSDevice item2 in (GXDLMSDeviceCollection)obj)
					{
						if (!item2.Media.IsOpen)
						{
							item2.InitializeConnection();
						}
					}
				}
				else if (obj is GXDLMSDevice)
				{
					if (!((GXDLMSDevice)obj).Media.IsOpen)
					{
						((GXDLMSDevice)obj).InitializeConnection();
					}
				}
				else if (obj is GXDLMSObjectCollection)
				{
					GXDLMSObjectCollection gXDLMSObjectCollection = obj as GXDLMSObjectCollection;
					GXDLMSDeviceCollection gXDLMSDeviceCollection = new GXDLMSDeviceCollection();
					foreach (GXDLMSObject item3 in gXDLMSObjectCollection)
					{
						GXDLMSDevice item = item3.Parent.Parent as GXDLMSDevice;
						if (!gXDLMSDeviceCollection.Contains(item))
						{
							gXDLMSDeviceCollection.Add(item);
						}
					}
					Connect(new object[1] { gXDLMSDeviceCollection });
				}
				else
				{
					GXDLMSObject gXDLMSObject = obj as GXDLMSObject;
					GXDLMSDevice gXDLMSDevice = gXDLMSObject.Parent.Parent as GXDLMSDevice;
					if (!gXDLMSDevice.Media.IsOpen)
					{
						gXDLMSDevice.InitializeConnection();
					}
				}
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception ex2)
			{
				AddLog(MediaLog.MessageType.Outgoing, ex2.Message.ToString());
				return false;
			}
			return result;
		}

		public bool ConnectES(object[] parameters)
		{
			string outStationNumber = TaskPara.OutStationNumber;
			bool result = true;
			try
			{
				object obj = parameters[0];
				if (obj is GXDLMSDeviceCollection)
				{
					int count = ((GXDLMSDeviceCollection)obj).Count;
					foreach (GXDLMSDevice item2 in (GXDLMSDeviceCollection)obj)
					{
						item2.InitializeConnectionES(base.Media, outStationNumber);
					}
				}
				else if (obj is GXDLMSDevice)
				{
					GXDLMSDevice gXDLMSDevice = (GXDLMSDevice)obj;
					gXDLMSDevice.InitializeConnectionES(base.Media, outStationNumber);
				}
				else if (obj is GXDLMSObjectCollection)
				{
					GXDLMSObjectCollection gXDLMSObjectCollection = obj as GXDLMSObjectCollection;
					GXDLMSDeviceCollection gXDLMSDeviceCollection = new GXDLMSDeviceCollection();
					foreach (GXDLMSObject item3 in gXDLMSObjectCollection)
					{
						GXDLMSDevice item = item3.Parent.Parent as GXDLMSDevice;
						if (!gXDLMSDeviceCollection.Contains(item))
						{
							gXDLMSDeviceCollection.Add(item);
						}
					}
					ConnectES(new object[1] { gXDLMSDeviceCollection });
				}
				else
				{
					GXDLMSObject gXDLMSObject = obj as GXDLMSObject;
					GXDLMSDevice gXDLMSDevice2 = gXDLMSObject.Parent.Parent as GXDLMSDevice;
					gXDLMSDevice2.InitializeConnectionES(base.Media, outStationNumber);
				}
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception ex2)
			{
				AddLog(MediaLog.MessageType.Outgoing, ex2.Message.ToString());
				return false;
			}
			return result;
		}

		private void Disconnect(object[] parameters)
		{
			try
			{
				object obj = parameters[0];
				if (obj is GXDLMSDeviceCollection)
				{
					int count = ((GXDLMSDeviceCollection)obj).Count;
					foreach (GXDLMSDevice item2 in (GXDLMSDeviceCollection)obj)
					{
						item2.Disconnect();
					}
				}
				else if (obj is GXDLMSDevice)
				{
					((GXDLMSDevice)obj).Disconnect();
				}
				else if (obj is GXDLMSObjectCollection)
				{
					GXDLMSObjectCollection gXDLMSObjectCollection = obj as GXDLMSObjectCollection;
					GXDLMSDeviceCollection gXDLMSDeviceCollection = new GXDLMSDeviceCollection();
					foreach (GXDLMSObject item3 in gXDLMSObjectCollection)
					{
						GXDLMSDevice item = item3.Parent.Parent as GXDLMSDevice;
						if (!gXDLMSDeviceCollection.Contains(item))
						{
							gXDLMSDeviceCollection.Add(item);
						}
					}
					Disconnect(new object[1] { gXDLMSDeviceCollection });
				}
				else
				{
					GXDLMSObject gXDLMSObject = obj as GXDLMSObject;
					GXDLMSDevice gXDLMSDevice = gXDLMSObject.Parent.Parent as GXDLMSDevice;
					gXDLMSDevice.Disconnect();
				}
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception ex2)
			{
				AddLog(MediaLog.MessageType.Outgoing, ex2.Message.ToString());
			}
		}

		private void DisconnectES(object[] parameters)
		{
			try
			{
				object obj = parameters[0];
				if (obj is GXDLMSDeviceCollection)
				{
					int count = ((GXDLMSDeviceCollection)obj).Count;
					foreach (GXDLMSDevice item2 in (GXDLMSDeviceCollection)obj)
					{
						item2.DisconnectES();
					}
				}
				else if (obj is GXDLMSDevice)
				{
					((GXDLMSDevice)obj).DisconnectES();
				}
				else if (obj is GXDLMSObjectCollection)
				{
					GXDLMSObjectCollection gXDLMSObjectCollection = obj as GXDLMSObjectCollection;
					GXDLMSDeviceCollection gXDLMSDeviceCollection = new GXDLMSDeviceCollection();
					foreach (GXDLMSObject item3 in gXDLMSObjectCollection)
					{
						GXDLMSDevice item = item3.Parent.Parent as GXDLMSDevice;
						if (!gXDLMSDeviceCollection.Contains(item))
						{
							gXDLMSDeviceCollection.Add(item);
						}
					}
					DisconnectES(new object[1] { gXDLMSDeviceCollection });
				}
				else
				{
					GXDLMSObject gXDLMSObject = obj as GXDLMSObject;
					GXDLMSDevice gXDLMSDevice = gXDLMSObject.Parent.Parent as GXDLMSDevice;
					gXDLMSDevice.DisconnectES();
				}
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception ex2)
			{
				AddLog(MediaLog.MessageType.Outgoing, ex2.Message.ToString());
			}
		}

		private void ReadDevices(string meterid, string nametask, int set, DateTime starttime)
		{
			foreach (GXDLMSDevice device in Devices)
			{
				ReadDevice(device, meterid, nametask, set, starttime);
			}
		}

		private void ReadDevice(GXDLMSDevice dev, string meterid, string nametask, int set, DateTime starttime)
		{
			try
			{
				resultList.Clear();
				GXDLMSCommunicator comm = dev.Comm;
				comm.OnBeforeRead = (ReadEventHandler)Delegate.Combine(comm.OnBeforeRead, new ReadEventHandler(OnBeforeRead));
				GXDLMSCommunicator comm2 = dev.Comm;
				comm2.OnAfterRead = (ReadEventHandler)Delegate.Combine(comm2.OnAfterRead, new ReadEventHandler(OnAfterRead));
				dev.KeepAliveStop();
				int count = dev.Objects.Count;
				dev.Comm.loadIDTask(meterid, nametask);
				foreach (GXDLMSObject @object in dev.Objects)
				{
					try
					{
						dev.Comm.Read(this, @object, 0, set, starttime);
						Thread.Sleep(50);
					}
					catch (Exception)
					{
					}
				}
				List<string> list = new List<string>();
				if (nametask != "ReadCurrentRegisterValues")
				{
					resultList = dev.Comm.resultRows();
				}
				if (nametask == "ReadHistoricalRegister")
				{
					for (int i = 0; i < resultList.Count; i++)
					{
						string[] array = resultList[i].Split(',');
						if (array[0] == "0.0.1.0.0.255")
						{
							list.Add(resultList[i]);
						}
					}
					if (list.Count != 0)
					{
						string[] array2 = list[0].Split(',');
						DateTime dateTime = Convert.ToDateTime(array2[1]);
						for (int j = 1; j < list.Count; j++)
						{
							string[] array3 = list[j].Split(',');
							DateTime dateTime2 = Convert.ToDateTime(array3[1]);
							int days = (dateTime - dateTime2).Days;
							if (days > 0 && dateTime.Month == dateTime2.Month && dateTime.Year == dateTime2.Year)
							{
								if (j == 1)
								{
									for (int k = 0; k < resultList.Count; k++)
									{
										if (resultList[k] == list[j])
										{
											resultList.RemoveRange(0, k);
											break;
										}
									}
									dateTime = dateTime2;
									continue;
								}
								string text = "0.0.1.0.0.255," + dateTime.ToString();
								string text2 = "0.0.1.0.0.255," + dateTime2.ToString();
								int num = 0;
								int num2 = 0;
								for (int l = 0; l < resultList.Count; l++)
								{
									if (resultList[l] == text)
									{
										num = l;
									}
									if (resultList[l] == text2)
									{
										num2 = l;
									}
								}
								resultList.RemoveRange(num, num2 - num);
								dateTime = dateTime2;
							}
							else
							{
								dateTime = dateTime2;
							}
						}
						list.Clear();
						for (int m = 0; m < resultList.Count; m++)
						{
							string[] array4 = resultList[m].Split(',');
							if (array4[0] == "0.0.1.0.0.255")
							{
								string text4 = array4[1] + "," + m;
								list.Add(array4[1]);
							}
						}
					}
				}
				if (debuggingLog)
				{
					string text3 = Path.Combine(Application.StartupPath, "Debugging\\");
					if (!Directory.Exists(text3))
					{
						Directory.CreateDirectory(text3);
					}
					string path = string.Format("{0}{1}_{2}_{3}.txt", text3, TaskPara.MeterID, TaskPara.Taskname, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
					if (File.Exists(path))
					{
						File.Delete(path);
					}
					string contents = string.Join("\r\n", resultList.ToArray());
					File.WriteAllText(path, contents);
				}
				ConvertLandisToA1700(resultList, list, nametask, meterid);
			}
			catch (Exception ex2)
			{
				throw ex2;
			}
			finally
			{
				GXDLMSCommunicator comm3 = dev.Comm;
				comm3.OnBeforeRead = (ReadEventHandler)Delegate.Remove(comm3.OnBeforeRead, new ReadEventHandler(OnBeforeRead));
				GXDLMSCommunicator comm4 = dev.Comm;
				comm4.OnAfterRead = (ReadEventHandler)Delegate.Remove(comm4.OnAfterRead, new ReadEventHandler(OnAfterRead));
				dev.KeepAliveStart();
			}
		}

		private List<string> CreateListObisDLMS(string taskname)
		{
			List<string> list = new List<string>();
			if (taskname == "ReadHistoricalRegister")
			{
				list.Add("0.0.1.0.0.255");
				list.Add("1.0.0.1.0.255");
				list.Add("1.1.1.8.0.255");
				list.Add("1.1.2.8.0.255");
				list.Add("1.1.3.8.0.255");
				list.Add("1.1.4.8.0.255");
				list.Add("1.1.1.8.1.255");
				list.Add("1.1.1.8.2.255");
				list.Add("1.1.1.8.3.255");
				list.Add("1.1.2.8.1.255");
				list.Add("1.1.2.8.2.255");
				list.Add("1.1.2.8.3.255");
				list.Add("1.1.1.6.1.255");
				list.Add("1.1.1.6.2.255");
				list.Add("1.1.1.6.3.255");
				list.Add("1.1.1.6.4.255");
				list.Add("1.1.2.6.1.255");
				list.Add("1.1.2.6.2.255");
				list.Add("1.1.2.6.3.255");
				list.Add("1.1.2.6.4.255");
			}
			else if (taskname == "ReadCurrentRegisterValues")
			{
				list.Add("0.0.1.0.0.255");
				list.Add("1.1.1.8.0.255");
				list.Add("1.1.2.8.0.255");
				list.Add("1.1.3.8.0.255");
				list.Add("1.1.4.8.0.255");
				list.Add("1.1.1.8.1.255");
				list.Add("1.1.1.8.2.255");
				list.Add("1.1.1.8.3.255");
				list.Add("1.1.2.8.1.255");
				list.Add("1.1.2.8.2.255");
				list.Add("1.1.2.8.3.255");
				list.Add("1.1.1.6.1.255");
				list.Add("1.1.1.6.2.255");
				list.Add("1.1.1.6.3.255");
				list.Add("1.1.1.6.4.255");
				list.Add("1.1.2.6.1.255");
				list.Add("1.1.2.6.2.255");
				list.Add("1.1.2.6.3.255");
				list.Add("1.1.2.6.4.255");
				list.Add("1.1.31.7.0.255");
				list.Add("1.1.51.7.0.255");
				list.Add("1.1.71.7.0.255");
				list.Add("1.1.32.7.0.255");
				list.Add("1.1.52.7.0.255");
				list.Add("1.1.72.7.0.255");
				list.Add("1.1.35.8.0.255");
				list.Add("1.1.55.8.0.255");
				list.Add("1.1.75.8.0.255");
				list.Add("1.1.16.7.0.255");
				list.Add("1.1.151.7.0.255");
				list.Add("1.1.171.7.0.255");
				list.Add("1.1.191.7.0.255");
				list.Add("1.1.131.7.0.255");
				list.Add("1.1.29.4.0.255");
				list.Add("1.1.49.4.0.255");
				list.Add("1.1.69.4.0.255");
				list.Add("1.1.9.4.0.255");
				list.Add("1.1.33.7.0.255");
				list.Add("1.1.53.7.0.255");
				list.Add("1.1.73.7.0.255");
				list.Add("1.1.13.7.0.255");
				list.Add("1.1.14.7.0.255");
			}
			return list;
		}

		private Dictionary<string, string> ListEventLog()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			DataTable eventLogTypeTable = EventLogTypeTable;
			if (eventLogTypeTable.Rows.Count > 0)
			{
				for (int i = 0; i < eventLogTypeTable.Rows.Count; i++)
				{
					dictionary.Add(eventLogTypeTable.Rows[i]["EVENT_KEY"].ToString(), eventLogTypeTable.Rows[i]["EVENT_NAME"].ToString() + "," + eventLogTypeTable.Rows[i]["EVENT_TYPE_ID"].ToString());
				}
			}
			return dictionary;
		}

		private List<string> DesList(List<string> souList, string tasname)
		{
			List<string> list = new List<string>();
			bool flag = false;
			List<string> list2 = CreateListObisDLMS(tasname);
			switch (tasname)
			{
			case "ReadEventLog":
			{
				List<string> list3 = new List<string>();
				Dictionary<string, string> dictionary = ListEventLog();
				if (souList.Count > 0)
				{
					for (int num3 = 0; num3 < souList.Count; num3++)
					{
						string[] array7 = souList[num3].Split(',');
						foreach (KeyValuePair<string, string> item2 in dictionary)
						{
							uint num4 = Convert.ToUInt32(array7[2]);
							string value = string.Format("{0:x2}", num4);
							uint num5 = Convert.ToUInt32(item2.Key, 16);
							uint num6 = Convert.ToUInt32(value, 16);
							if (num5 == (num5 & num6))
							{
								string item = array7[1] + "," + item2.Key + "," + item2.Value;
								list3.Add(item);
							}
						}
					}
					list2 = list3;
				}
				else
				{
					list2 = souList;
				}
				break;
			}
			case "ReadCurrentRegisterValues":
			{
				for (int m = 0; m < souList.Count; m++)
				{
					string[] array4 = souList[m].Split(',');
					if (!(array4[0].Trim() == "1.1.81.7.6.255"))
					{
						if (array4[0].Trim() == "1.1.81.7.26.255")
						{
							list2.Add("1.1.81.7.4.255");
							list2.Add("1.1.81.7.15.255");
							list2.Add("1.1.81.7.26.255");
							flag = true;
							break;
						}
						continue;
					}
					list2.Add("1.1.81.7.4.255");
					list2.Add("1.1.81.7.5.255");
					list2.Add("1.1.81.7.6.255");
					flag = true;
					break;
				}
				if (!flag)
				{
					list2.Add("1.1.81.7.4.255");
					list2.Add("1.1.81.7.5.255");
					list2.Add("1.1.81.7.6.255");
				}
				for (int n = 0; n < list2.Count; n++)
				{
					for (int num = 0; num < souList.Count; num++)
					{
						string[] array5 = souList[num].Split(',');
						if (array5[0].Trim() == list2[n].Trim())
						{
							list2[n] = souList[num];
							souList.RemoveAt(num);
							break;
						}
					}
				}
				for (int num2 = 0; num2 < list2.Count; num2++)
				{
					string[] array6 = list2[num2].Split(',');
					if (array6.Length < 2)
					{
						list2[num2] += ",0";
					}
				}
				break;
			}
			case "ReadHistoricalRegister":
			{
				for (int i = 0; i < list2.Count; i++)
				{
					for (int j = 0; j < souList.Count; j++)
					{
						string[] array = souList[j].Split(',');
						if (array[0].Trim() == list2[i].Trim())
						{
							list2[i] = souList[j];
							souList.RemoveAt(j);
							break;
						}
					}
					if (i + 1 != list2.Count)
					{
						continue;
					}
					list.AddRange(list2);
					for (int k = 0; k < souList.Count; k++)
					{
						string[] array2 = souList[k].Split(',');
						if (array2[0].Trim() != "0.0.1.0.0.255")
						{
							souList.RemoveAt(k);
							k = -1;
						}
						else if (array2[0].Trim() == "0.0.1.0.0.255")
						{
							list2 = CreateListObisDLMS(tasname);
							i = -1;
							break;
						}
					}
				}
				for (int l = 0; l < list.Count; l++)
				{
					string[] array3 = list[l].Split(',');
					if (array3.Length < 2)
					{
						list[l] += ",0";
					}
				}
				list2 = list;
				break;
			}
			}
			return list2;
		}

		private StringBuilder ConvertLandisToA1700(List<string> strArr, List<string> listdate, string nametask, string meterid)
		{
			int num = 1;
			outstringLandisData.Length = 0;
			outstringLandisData.AppendLine("STHM Meter Reading");
			if (nametask == "ReadHistoricalRegister" && strArr.Count > 0)
			{
				strArr = DesList(strArr, nametask);
				outstringLandisData.AppendLine("From unit," + meterid + "," + DateTime.Now.ToString());
				outstringLandisData.AppendLine("Historical Data");
				string text = "";
				string timedate = "";
				bool flag = false;
				foreach (string item5 in strArr)
				{
					string[] array = item5.Split(',');
					if (array[0] == "0.0.1.0.0.255")
					{
						outstringLandisData.AppendLine("Historical Data set:," + num + "\r\n");
						outstringLandisData.AppendLine("Cumulative totals\r\n");
						outstringLandisData.AppendLine(",Value,Unit");
						num++;
						timedate = array[1];
						continue;
					}
					if (array[0] == "1.0.0.1.0.255")
					{
						text = array[1];
						continue;
					}
					if (array[0] == "1.1.1.8.0.255")
					{
						outstringLandisData.AppendLine("," + array[1] + ",Import Wh");
						continue;
					}
					if (array[0] == "1.1.2.8.0.255")
					{
						outstringLandisData.AppendLine("," + array[1] + ",Export Wh");
						continue;
					}
					if (array[0] == "1.1.3.8.0.255")
					{
						outstringLandisData.AppendLine(",0,Q1 varh");
						outstringLandisData.AppendLine(",0,Q2 varh");
						outstringLandisData.AppendLine(",0,Q3 varh");
						outstringLandisData.AppendLine(",0,Q4 varh");
						outstringLandisData.AppendLine(",0,VAh");
						outstringLandisData.AppendLine("," + array[1] + ",CD1");
						continue;
					}
					if (array[0] == "1.1.4.8.0.255")
					{
						outstringLandisData.AppendLine("," + array[1] + ",CD2");
						outstringLandisData.AppendLine(",0,CD3");
						outstringLandisData.AppendLine("Rates\r\n");
						outstringLandisData.AppendLine("Register,Value,Unit");
						continue;
					}
					if (array[0] == "1.1.1.8.1.255")
					{
						outstringLandisData.AppendLine("1," + array[1] + ",Import Wh");
						continue;
					}
					if (array[0] == "1.1.1.8.2.255")
					{
						outstringLandisData.AppendLine("2," + array[1] + ",Import Wh");
						continue;
					}
					if (array[0] == "1.1.1.8.3.255")
					{
						outstringLandisData.AppendLine("3," + array[1] + ",Import Wh");
						continue;
					}
					if (array[0] == "1.1.2.8.1.255")
					{
						outstringLandisData.AppendLine("4," + array[1] + ",Export Wh");
						continue;
					}
					if (array[0] == "1.1.2.8.2.255")
					{
						outstringLandisData.AppendLine("5," + array[1] + ",Export Wh");
						continue;
					}
					if (array[0] == "1.1.2.8.3.255")
					{
						outstringLandisData.AppendLine("6," + array[1] + ",Export Wh");
						for (int i = 7; i < 17; i++)
						{
							outstringLandisData.AppendLine(i + ",0,no Source");
						}
						outstringLandisData.AppendLine("Multi-utility registers\r\n");
						outstringLandisData.AppendLine(",Value,Unit");
						outstringLandisData.AppendLine(",0,MU 1");
						outstringLandisData.AppendLine(",0,MU 2");
						outstringLandisData.AppendLine(",0,MU 3");
						outstringLandisData.AppendLine(",0,MU 4");
						outstringLandisData.AppendLine("Cummulative Maximum demands\r\n");
						outstringLandisData.AppendLine("Register,Value,Unit");
						continue;
					}
					if (array[0] == "1.1.1.6.1.255")
					{
						outstringLandisData.AppendLine("1," + array[1] + ",Import W");
						continue;
					}
					if (array[0] == "1.1.1.6.2.255")
					{
						outstringLandisData.AppendLine("2," + array[1] + ",Import W");
						continue;
					}
					if (array[0] == "1.1.1.6.3.255")
					{
						outstringLandisData.AppendLine("3," + array[1] + ",Import W");
						continue;
					}
					if (array[0] == "1.1.1.6.4.255")
					{
						outstringLandisData.AppendLine("4," + array[1] + ",Import W");
						continue;
					}
					if (array[0] == "1.1.2.6.1.255")
					{
						outstringLandisData.AppendLine("5," + array[1] + ",Export W");
						continue;
					}
					if (array[0] == "1.1.2.6.2.255")
					{
						outstringLandisData.AppendLine("6," + array[1] + ",Export W");
						continue;
					}
					if (array[0] == "1.1.2.6.3.255")
					{
						outstringLandisData.AppendLine("7," + array[1] + ",Export W");
						continue;
					}
					if (array[0] == "1.1.2.6.4.255")
					{
						outstringLandisData.AppendLine("8," + array[1] + ",Export W");
						flag = true;
					}
					if (flag)
					{
						outstringLandisData.AppendLine("Billing event details\r\n");
						outstringLandisData.AppendLine("Billing reset number:," + text);
						outstringLandisData.AppendLine("Time of billing reset:," + timedate);
						int num2 = listdate.FindIndex((string item) => item == timedate.ToString());
						if (listdate.Count - num2 > 1)
						{
							outstringLandisData.AppendLine("Billing period start date," + listdate[num2 + 1]);
						}
						else
						{
							outstringLandisData.AppendLine("Billing period start date," + Convert.ToDateTime(timedate).AddMonths(-1).ToString());
						}
						outstringLandisData.AppendLine("Billing period end date," + timedate);
						flag = false;
					}
				}
			}
			if (nametask == "ReadLoadProfile")
			{
				outstringLandisData.AppendLine("From unit," + meterid + "," + DateTime.Now.ToString());
				List<string> list = ReverseList(strArr);
				new List<string>();
				List<string> list2 = new List<string>();
				DateTime date = DateTime.Now.Date;
				DateTime dateTime = DateTime.Now.Date;
				list2.Clear();
				List<string> list3 = new List<string>();
				for (int j = 0; j < list.Count; j++)
				{
					string[] array2 = list[j].Split(',');
					string[] array3 = string.Join(",", array2, 2, array2.Length - 2).Split(',');
                    string test = string.Join(",", array3, 1, array3.Length - 1);
					string item2 = string.Join(",", array3, 1, array3.Length - 1) + "," + array3[0] + "," + array2[1];
					list3.Add(item2);
				}
				int num3 = 0;
				while (num3 < list3.Count && num3 != list3.Count - 1)
				{
					string[] array4 = list3[num3].Split(',');
					if (num3 == 0)
					{
						date = DateTime.Parse(array4[array4.Length - 1]).Date;
						dateTime = DateTime.Parse(array4[array4.Length - 1]).Date;
						if (DateTime.Parse(array4[array4.Length - 1]).ToShortTimeString() == "12:00 AM")
						{
							num3++;
							continue;
						}
						list2.Add(string.Concat("E4,", date, ",99"));
					}
					date = DateTime.Parse(array4[array4.Length - 1]).Date;
					if (dateTime != date)
					{
						if (!(DateTime.Parse(array4[array4.Length - 1]).ToShortTimeString() != "12:00 AM"))
						{
							num3++;
							dateTime = date;
							continue;
						}
						list2.Add(string.Concat("E4,", date, ",99"));
						dateTime = date;
					}
					if (DictEvent["E4"].Contains(array4[array4.Length - 2]))
					{
						list2.Add("E4," + array4[array4.Length - 1] + ",99");
					}
					else if (DictEvent["E6"].Contains(array4[array4.Length - 2]))
					{
						list2.Add("E6," + array4[array4.Length - 1]);
					}
					else if (DictEvent["E5"].Contains(array4[array4.Length - 2]))
					{
						list2.Add("E5," + array4[array4.Length - 1]);
					}
					string[] array5 = list3[num3].Split(',');
					DateTime value = DateTime.Parse(array5[array5.Length - 1]);
					DateTime date2 = DateTime.Parse(array5[array5.Length - 1]).Date;
					for (int k = num3 + 1; k < list3.Count; k++)
					{
						string[] array6 = list3[k].Split(',');
						string value2 = array6[array6.Length - 2];
						DateTime dateTime2 = DateTime.Parse(array6[array6.Length - 1]);
						DateTime date3 = DateTime.Parse(array6[array6.Length - 1]).Date;
						int minutes = dateTime2.Subtract(value).Minutes;
						if (!DictEvent["NORMAL"].Contains(value2))
						{
							if (DictEvent["E6"].Contains(value2))
							{
								list2.Add("E6," + dateTime2.ToString());
								num3 = k;
							}
							else if (DictEvent["E5"].Contains(value2))
							{
								if (minutes >= 30)
								{
									double[] currentline = StringToDouble(5, array5, 0, 5);
									double[] nextline = StringToDouble(5, list3[k - 1].Split(','), 0, 5);
									list2.Add(string.Join(",", CalculatePower(currentline, nextline)));
									num3 = k;
									break;
								}
								list2.Add("E5," + dateTime2.ToString());
								num3 = k;
							}
							else if (date3 != date2)
							{
								double[] currentline2 = StringToDouble(5, array5, 0, 5);
								double[] nextline2 = StringToDouble(5, list3[k - 1].Split(','), 0, 5);
								list2.Add(string.Join(",", CalculatePower(currentline2, nextline2)));
								num3 = k;
								break;
							}
							continue;
						}
						double[] currentline3 = StringToDouble(5, array5, 0, 5);
						double[] nextline3 = StringToDouble(5, array6, 0, 5);
						list2.Add(string.Join(",", CalculatePower(currentline3, nextline3)));
						num3 = k;
						break;
					}
				}
				foreach (string item6 in list2)
				{
					outstringLandisData.AppendLine(item6);
				}
			}
			if (nametask == "ReadLoadProfile_A0")
			{
				outstringLandisData.AppendLine("From unit," + meterid + "," + DateTime.Now.ToString());
				List<string> list4 = ReverseList(strArr);
				new List<string>();
				List<string> list5 = new List<string>();
				DateTime date4 = DateTime.Now.Date;
				DateTime dateTime3 = DateTime.Now.Date;
				list5.Clear();
				List<string> list6 = new List<string>();
				for (int l = 0; l < list4.Count; l++)
				{
					string[] array7 = list4[l].Split(',');
					string[] array8 = string.Join(",", array7, 2, array7.Length - 2).Split(',');
					string item3 = string.Join(",", array8, 1, array8.Length - 1) + "," + array8[0] + "," + array7[1];
					list6.Add(item3);
				}
				int num4 = 0;
				while (num4 < list6.Count && num4 != list6.Count - 1)
				{
					string[] array9 = list6[num4].Split(',');
					if (num4 == 0)
					{
						date4 = DateTime.Parse(array9[array9.Length - 1]);
						dateTime3 = DateTime.Parse(array9[array9.Length - 1]).Date;
						if (DateTime.Parse(array9[array9.Length - 1]).ToShortTimeString() == "12:00 AM")
						{
							num4++;
							continue;
						}
						list5.Add(string.Concat("E4,", date4, ",030399"));
					}
					date4 = DateTime.Parse(array9[array9.Length - 1]).Date;
					if (dateTime3 != date4)
					{
						if (!(DateTime.Parse(array9[array9.Length - 1]).ToShortTimeString() != "12:00 AM"))
						{
							num4++;
							dateTime3 = date4;
							continue;
						}
						list5.Add(string.Concat("E4,", date4, ",030399"));
						dateTime3 = date4;
					}
					bool flag2 = false;
					if (DictEvent["E4"].Contains(array9[array9.Length - 2]))
					{
						list5.Add("E4," + array9[array9.Length - 1] + ",030399");
					}
					else if (DictEvent["E6"].Contains(array9[array9.Length - 2]))
					{
						list5.Add("E6," + array9[array9.Length - 1]);
					}
					else if (DictEvent["E5"].Contains(array9[array9.Length - 2]))
					{
						list5.Add("E5," + array9[array9.Length - 1]);
						flag2 = true;
					}
					string[] array10 = list6[num4].Split(',');
					DateTime value3 = DateTime.Parse(array10[array10.Length - 1]);
					DateTime date5 = DateTime.Parse(array10[array10.Length - 1]).Date;
					for (int m = num4 + 1; m < list6.Count; m++)
					{
						string[] array11 = list6[m].Split(',');
						string value4 = array11[array11.Length - 2];
						DateTime dateTime4 = DateTime.Parse(array11[array11.Length - 1]);
						DateTime date6 = DateTime.Parse(array11[array11.Length - 1]).Date;
						double totalMinutes = dateTime4.Subtract(value3).TotalMinutes;
						if (!DictEvent["NORMAL"].Contains(value4))
						{
							if (DictEvent["E6"].Contains(value4))
							{
								list5.Add("E6," + dateTime4.ToString());
								if (m == list6.Count - 1)
								{
									double[] currentline4 = StringToDouble(5, array10, 0, 5);
									double[] nextline4 = StringToDouble(5, list6[m].Split(','), 0, 5);
									list5.Add(string.Join(",", CalculatePower(currentline4, nextline4)));
									num4 = m;
								}
								continue;
							}
							if (DictEvent["E5"].Contains(value4))
							{
								if (totalMinutes < 30.0)
								{
									list5.Add("E5," + dateTime4.ToString());
									continue;
								}
								double[] currentline5 = StringToDouble(5, array10, 0, 5);
								double[] nextline5 = StringToDouble(5, list6[m - 1].Split(','), 0, 5);
								list5.Add(string.Join(",", CalculatePower(currentline5, nextline5)));
								num4 = m;
								break;
							}
							if (!(date6 != date5))
							{
								if (flag2)
								{
									double[] currentline6 = StringToDouble(5, array10, 0, 5);
									double[] nextline6 = StringToDouble(5, list6[m].Split(','), 0, 5);
									list5.Add(string.Join(",", CalculatePower(currentline6, nextline6)));
									num4 = m;
									break;
								}
								continue;
							}
							double[] currentline7 = StringToDouble(5, array10, 0, 5);
							double[] nextline7 = StringToDouble(5, list6[m - 1].Split(','), 0, 5);
							list5.Add(string.Join(",", CalculatePower(currentline7, nextline7)));
							num4 = m;
							break;
						}
						double[] currentline8 = StringToDouble(5, array10, 0, 5);
						double[] nextline8 = StringToDouble(5, array11, 0, 5);
						list5.Add(string.Join(",", CalculatePower(currentline8, nextline8)));
						num4 = m;
						break;
					}
				}
				foreach (string item7 in list5)
				{
					outstringLandisData.AppendLine(item7);
				}
			}
			if (nametask == "ReadCurrentRegisterValues")
			{
				strArr = DesList(strArr, nametask);
				string text2 = "";
				for (int n = 0; n < strArr.Count; n++)
				{
					string[] array12 = strArr[n].Split(',');
					if (array12[0] == "0.0.1.0.0.255")
					{
						outstringLandisData.AppendLine("From unit," + meterid + "," + DateTime.Now.ToString() + "," + array12[1]);
						outstringLandisData.AppendLine("Cumulative totals\r\n");
						outstringLandisData.AppendLine(",Value,Unit");
					}
					else if (array12[0] == "1.1.1.8.0.255")
					{
						outstringLandisData.AppendLine("," + Convert.ToDouble(array12[1]) + ",Import Wh");
					}
					else if (array12[0] == "1.1.2.8.0.255")
					{
						outstringLandisData.AppendLine("," + Convert.ToDouble(array12[1]) + ",Export Wh");
					}
					else if (array12[0] == "1.1.3.8.0.255")
					{
						outstringLandisData.AppendLine(",0,Q1 varh");
						outstringLandisData.AppendLine(",0,Q2 varh");
						outstringLandisData.AppendLine(",0,Q3 varh");
						outstringLandisData.AppendLine(",0,Q4 varh");
						outstringLandisData.AppendLine(",0,VAh");
						outstringLandisData.AppendLine("," + Convert.ToDouble(array12[1]) + ",CD1");
					}
					else if (array12[0] == "1.1.4.8.0.255")
					{
						outstringLandisData.AppendLine("," + Convert.ToDouble(array12[1]) + ",CD2");
						outstringLandisData.AppendLine(",0,CD3");
						outstringLandisData.AppendLine("Rates\r\n");
						outstringLandisData.AppendLine("Register,Value,Unit");
					}
					else if (array12[0] == "1.1.1.8.1.255")
					{
						outstringLandisData.AppendLine("1," + Convert.ToDouble(array12[1]) + ",Import Wh");
					}
					else if (array12[0] == "1.1.1.8.2.255")
					{
						outstringLandisData.AppendLine("2," + Convert.ToDouble(array12[1]) + ",Import Wh");
					}
					else if (array12[0] == "1.1.1.8.3.255")
					{
						outstringLandisData.AppendLine("3," + Convert.ToDouble(array12[1]) + ",Import Wh");
					}
					else if (array12[0] == "1.1.2.8.1.255")
					{
						outstringLandisData.AppendLine("4," + Convert.ToDouble(array12[1]) + ",Export Wh");
					}
					else if (array12[0] == "1.1.2.8.2.255")
					{
						outstringLandisData.AppendLine("5," + Convert.ToDouble(array12[1]) + ",Export Wh");
					}
					else if (array12[0] == "1.1.2.8.3.255")
					{
						outstringLandisData.AppendLine("6," + Convert.ToDouble(array12[1]) + ",Export Wh");
						for (int num5 = 7; num5 < 17; num5++)
						{
							outstringLandisData.AppendLine(num5 + ",0,no Source");
						}
						outstringLandisData.AppendLine("Multi-utility registers\r\n");
						outstringLandisData.AppendLine(",Value,Unit");
						outstringLandisData.AppendLine(",0,MU 1");
						outstringLandisData.AppendLine(",0,MU 2");
						outstringLandisData.AppendLine(",0,MU 3");
						outstringLandisData.AppendLine(",0,MU 4");
						outstringLandisData.AppendLine("Cummulative Maximum demands\r\n");
						outstringLandisData.AppendLine("Register,Value,Unit");
					}
					else if (array12[0] == "1.1.1.6.1.255")
					{
						outstringLandisData.AppendLine("1," + Convert.ToDouble(array12[1]) + ",Import W");
					}
					else if (array12[0] == "1.1.1.6.2.255")
					{
						outstringLandisData.AppendLine("2," + Convert.ToDouble(array12[1]) + ",Import W");
					}
					else if (array12[0] == "1.1.1.6.3.255")
					{
						outstringLandisData.AppendLine("3," + Convert.ToDouble(array12[1]) + ",Import W");
					}
					else if (array12[0] == "1.1.1.6.4.255")
					{
						outstringLandisData.AppendLine("4," + Convert.ToDouble(array12[1]) + ",Import W");
					}
					else if (array12[0] == "1.1.2.6.1.255")
					{
						outstringLandisData.AppendLine("5," + Convert.ToDouble(array12[1]) + ",Export W");
					}
					else if (array12[0] == "1.1.2.6.2.255")
					{
						outstringLandisData.AppendLine("6," + Convert.ToDouble(array12[1]) + ",Export W");
					}
					else if (array12[0] == "1.1.2.6.3.255")
					{
						outstringLandisData.AppendLine("7," + Convert.ToDouble(array12[1]) + ",Export W");
					}
					else if (array12[0] == "1.1.2.6.4.255")
					{
						outstringLandisData.AppendLine("8," + Convert.ToDouble(array12[1]) + ",Export W");
						outstringLandisData.AppendLine("Instantaneous Value\r\n");
						outstringLandisData.AppendLine(",Phase A,Phase B,Phase C,Total");
					}
					else if (array12[0] == "1.1.31.7.0.255")
					{
						text2 = text2 + "," + array12[1];
					}
					else if (array12[0] == "1.1.51.7.0.255")
					{
						text2 = text2 + "," + array12[1];
					}
					else if (array12[0] == "1.1.71.7.0.255")
					{
						text2 = text2 + "," + array12[1] + ",,Amps";
						outstringLandisData.AppendLine(text2);
						text2 = "";
					}
					else if (array12[0] == "1.1.32.7.0.255")
					{
						text2 = "," + array12[1];
					}
					else if (array12[0] == "1.1.52.7.0.255")
					{
						text2 = text2 + "," + array12[1];
					}
					else if (array12[0] == "1.1.72.7.0.255")
					{
						text2 = text2 + "," + array12[1] + ",,Volts";
						outstringLandisData.AppendLine(text2);
						text2 = "";
					}
					else if (array12[0] == "1.1.35.8.0.255")
					{
						text2 = "," + array12[1];
					}
					else if (array12[0] == "1.1.55.8.0.255")
					{
						text2 = text2 + "," + array12[1];
					}
					else if (array12[0] == "1.1.75.8.0.255")
					{
						text2 = text2 + "," + array12[1];
					}
					else if (array12[0] == "1.1.16.7.0.255")
					{
						text2 = text2 + "," + array12[1] + ",Active Power(kW)";
						outstringLandisData.AppendLine(text2);
						text2 = "";
					}
					else if (array12[0] == "1.1.151.7.0.255")
					{
						text2 = text2 + "," + array12[1];
					}
					else if (array12[0] == "1.1.171.7.0.255")
					{
						text2 = text2 + "," + array12[1];
					}
					else if (array12[0] == "1.1.191.7.0.255")
					{
						text2 = text2 + "," + array12[1];
					}
					else if (array12[0] == "1.1.131.7.0.255")
					{
						text2 = text2 + "," + array12[1] + ",Reactive Power(kvar)";
						outstringLandisData.AppendLine(text2);
						text2 = "";
					}
					else if (array12[0] == "1.1.29.4.0.255")
					{
						text2 = text2 + "," + array12[1];
					}
					else if (array12[0] == "1.1.49.4.0.255")
					{
						text2 = text2 + "," + array12[1];
					}
					else if (array12[0] == "1.1.69.4.0.255")
					{
						text2 = text2 + "," + array12[1];
					}
					else if (array12[0] == "1.1.9.4.0.255")
					{
						text2 = text2 + "," + array12[1] + ",Apparent Power(kVA)";
						outstringLandisData.AppendLine(text2);
						text2 = "";
					}
					else if (array12[0] == "1.1.33.7.0.255")
					{
						text2 = ((!(array12[1] == "2")) ? (text2 + "," + array12[1]) : (text2 + ",not available"));
					}
					else if (array12[0] == "1.1.53.7.0.255")
					{
						text2 = ((!(array12[1] == "2")) ? (text2 + "," + array12[1]) : (text2 + ",not available"));
					}
					else if (array12[0] == "1.1.73.7.0.255")
					{
						text2 = ((!(array12[1] == "2")) ? (text2 + "," + array12[1]) : (text2 + ",not available"));
					}
					else if (array12[0] == "1.1.13.7.0.255")
					{
						if (array12[1] == "2")
						{
							text2 += ",not available,Power factor";
							outstringLandisData.AppendLine(text2);
							text2 = "";
						}
						else
						{
							text2 = text2 + "," + array12[1] + ",Power factor";
							outstringLandisData.AppendLine(text2);
							text2 = "";
						}
					}
					else if (array12[0] == "1.1.14.7.0.255")
					{
						outstringLandisData.AppendLine("," + array12[1] + "," + array12[1] + "," + array12[1] + ",,Frequency(Hz)");
					}
					else if (array12[0] == "1.1.81.7.4.255")
					{
						text2 = ((!(array12[1] == "400")) ? (text2 + "," + array12[1]) : (text2 + ",not available"));
					}
					else if (array12[0] == "1.1.81.7.5.255")
					{
						text2 = ((!(array12[1] == "400")) ? (text2 + "," + array12[1]) : (text2 + ",not available"));
					}
					else if (array12[0] == "1.1.81.7.15.255")
					{
						text2 = ((!(array12[1] == "400")) ? (text2 + "," + array12[1]) : (text2 + ",not available"));
					}
					else if (array12[0] == "1.1.81.7.6.255")
					{
						if (array12[1] == "400")
						{
							text2 += ",not available,,Phase Angle(deg)";
							outstringLandisData.AppendLine(text2);
							outstringLandisData.AppendLine(",,,,A->B->C,Phase Rotation");
							text2 = "";
						}
						else
						{
							text2 = text2 + "," + array12[1] + ",,Phase Angle(deg)";
							outstringLandisData.AppendLine(text2);
							outstringLandisData.AppendLine(",,,,A->B->C,Phase Rotation");
							text2 = "";
						}
					}
					else if (array12[0] == "1.1.81.7.26.255")
					{
						if (array12[1] == "400")
						{
							text2 += ",not available,,Phase Angle(deg)";
							outstringLandisData.AppendLine(text2);
							outstringLandisData.AppendLine(",,,,A->B->C,Phase Rotation");
							text2 = "";
						}
						else
						{
							text2 = text2 + "," + array12[1] + ",,Phase Angle(deg)";
							outstringLandisData.AppendLine(text2);
							outstringLandisData.AppendLine(",,,,A->B->C,Phase Rotation");
							text2 = "";
						}
					}
				}
			}
			if (nametask == "ReadInstrumentationProfile")
			{
				outstringLandisData.AppendLine("From unit," + meterid + "," + DateTime.Now.ToString());
				List<string> list7 = ReverseList(strArr);
				List<string> list8 = new List<string>();
				List<string> list9 = new List<string>();
				DateTime date7 = DateTime.Now.Date;
				DateTime dateTime5 = DateTime.Now.Date;
				list9.Clear();
				for (int num6 = 0; num6 < list7.Count; num6++)
				{
					string[] array13 = list7[num6].Split(',');
					string[] array14 = string.Join(",", array13, 2, array13.Length - 2).Split(',');
					string item4 = string.Join(",", array14, 1, array14.Length - 1) + "," + array14[0] + "," + array13[1];
					list8.Add(item4);
				}
				for (int num7 = 0; num7 < list8.Count; num7++)
				{
					string[] array15 = list8[num7].Split(',');
					if (num7 == 0)
					{
						date7 = DateTime.Parse(array15[array15.Length - 1]).Date;
						dateTime5 = DateTime.Parse(array15[array15.Length - 1]).Date;
						if (DateTime.Parse(array15[array15.Length - 1]).ToShortTimeString() == "12:00 AM")
						{
							continue;
						}
						list9.Add(string.Concat("E4,", date7, ",99"));
					}
					date7 = DateTime.Parse(array15[array15.Length - 1]).Date;
					if (dateTime5 != date7)
					{
						if (!(DateTime.Parse(array15[array15.Length - 1]).ToShortTimeString() != "12:00 AM"))
						{
							dateTime5 = date7;
							continue;
						}
						list9.Add(string.Concat("E4,", date7, ",99"));
						dateTime5 = date7;
					}
					if (DictEvent["E4"].Contains(array15[array15.Length - 2]))
					{
						list9.Add("E4," + array15[array15.Length - 1] + ",99");
					}
					else if (DictEvent["E6"].Contains(array15[array15.Length - 2]))
					{
						list9.Add("E6," + array15[array15.Length - 1]);
					}
					else if (DictEvent["E5"].Contains(array15[array15.Length - 2]))
					{
						list9.Add("E5," + array15[array15.Length - 1]);
					}
					string[] input = list8[num7].Split(',');
					double[] array16 = StringToDouble(8, input, 0, 8);
					List<string> list10 = new List<string>();
					for (int num8 = 0; num8 < array16.Length; num8++)
					{
						list10.Add(array16[num8].ToString());
					}
					string[] value5 = list10.ToArray();
					list9.Add(string.Join(",", value5));
				}
				foreach (string item8 in list9)
				{
					outstringLandisData.AppendLine(item8);
				}
			}
			if (nametask == "ReadEventLog")
			{
				outstringLandisData.AppendLine("From unit," + meterid + "," + DateTime.Now.ToString());
				outstringLandisData.AppendLine("Date Time Event,Event number,Status");
				List<string> souList = ReverseList(strArr);
				List<string> list11 = DesList(souList, nametask);
				for (int num9 = 0; num9 < list11.Count; num9++)
				{
					outstringLandisData.AppendLine(list11[num9]);
				}
			}
			WorkingStatus = EnumWorkingStatus.Completed;
			return outstringLandisData;
		}

		public List<string> ReverseList(List<string> inputlist)
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			for (int i = 0; i < inputlist.Count; i++)
			{
				string[] array = inputlist[i].Split(',');
				string text = array[0];
				string item = array[1];
				if (text == "0.0.1.0.0.255")
				{
					if (i == 0)
					{
						list2.Add(item);
						continue;
					}
					string text2 = string.Join(",", list2.ToArray());
					list.Add("," + text2);
					list2.Clear();
					list2.Add(item);
				}
				else
				{
					list2.Add(item);
				}
				if (i == inputlist.Count - 1)
				{
					string text3 = string.Join(",", list2.ToArray());
					list.Add("," + text3);
				}
			}
			return list;
		}

		public double[] StringToDouble(int length, string[] input, int startindex, int endindex)
		{
			double[] array = new double[length];
			for (int i = startindex; i < endindex; i++)
			{
                double val = 0;
                double.TryParse(input[i],out val);
				array[i] = val;
			}
			return array;
		}

		public string[] CalculatePower(double[] currentline, double[] nextline)
		{
			string[] array = new string[currentline.Length];
			for (int i = 0; i < currentline.Length; i++)
			{
				if (i < 4)
				{
					array[i] = Math.Abs((nextline[i] - currentline[i]) * 0.001 * 2.0).ToString();
				}
				else
				{
					array[i] = currentline[i].ToString();
				}
			}
			return array;
		}

		private void OnAfterRead(GXDLMSObject sender, int index, string nametask, int set, DateTime starttime)
		{
			string text = "";
			try
			{
				if (sender is GXDLMSRegister)
				{
					GXDLMSRegister gXDLMSRegister = sender as GXDLMSRegister;
					if (index == 2 && gXDLMSRegister.Scaler != 1.0)
					{
						gXDLMSRegister.Value = Convert.ToDouble(gXDLMSRegister.Value) * gXDLMSRegister.Scaler;
						string logicalName = gXDLMSRegister.LogicalName;
						object value = gXDLMSRegister.Value;
						text = logicalName + "," + ((value != null) ? value.ToString() : null);
						resultList.Add(text);
					}
				}
				else if (sender is GXDLMSDemandRegister)
				{
					GXDLMSDemandRegister gXDLMSDemandRegister = sender as GXDLMSDemandRegister;
					if (index == 2 && gXDLMSDemandRegister.Scaler != 1.0)
					{
						gXDLMSDemandRegister.CurrentAvarageValue = Convert.ToDouble(gXDLMSDemandRegister.CurrentAvarageValue) * gXDLMSDemandRegister.Scaler;
						string logicalName2 = gXDLMSDemandRegister.LogicalName;
						object currentAvarageValue = gXDLMSDemandRegister.CurrentAvarageValue;
						text = logicalName2 + "," + ((currentAvarageValue != null) ? currentAvarageValue.ToString() : null);
						resultList.Add(text);
					}
					if (index == 3 && gXDLMSDemandRegister.Scaler != 1.0)
					{
						gXDLMSDemandRegister.LastAvarageValue = Convert.ToDouble(gXDLMSDemandRegister.LastAvarageValue) * gXDLMSDemandRegister.Scaler;
						string logicalName3 = gXDLMSDemandRegister.LogicalName;
						object lastAvarageValue = gXDLMSDemandRegister.LastAvarageValue;
						text = logicalName3 + "," + ((lastAvarageValue != null) ? lastAvarageValue.ToString() : null);
						resultList.Add(text);
					}
				}
				else if (sender is GXDLMSClock)
				{
					GXDLMSClock gXDLMSClock = sender as GXDLMSClock;
					DateTime time = gXDLMSClock.Time;
					if (resultList.Count == 0)
					{
						text = gXDLMSClock.LogicalName + "," + gXDLMSClock.Time.ToString();
						resultList.Add(text);
					}
				}
			}
			catch (Exception ex)
			{
				AddLog(MediaLog.MessageType.Outgoing, ex.Message.ToString());
			}
		}

		private void OnBeforeRead(GXDLMSObject sender, int index, string nametask, int set, DateTime starttime)
		{
			try
			{
				if (!(sender is GXDLMSProfileGeneric))
				{
					return;
				}
				GXDLMSProfileGeneric gXDLMSProfileGeneric = sender as GXDLMSProfileGeneric;
				gXDLMSProfileGeneric.Data.Clear();
				DateTime now = DateTime.Now;
				string value = now.Month + "/1/" + now.Year;
				switch (nametask)
				{
				case "ReadEventLog":
				{
					DateTime dateTime4 = starttime;
					int days4 = (now - dateTime4).Days;
					if (TaskPara.IsFromDate)
					{
						if (set == 1 && days4 == 0)
						{
							gXDLMSProfileGeneric.From = dateTime4.Date;
							gXDLMSProfileGeneric.To = dateTime4;
						}
						else if (set == 1 && days4 > 0)
						{
							gXDLMSProfileGeneric.From = dateTime4.Date.AddDays(-1.0);
							gXDLMSProfileGeneric.To = dateTime4.Date;
						}
						else
						{
							gXDLMSProfileGeneric.From = dateTime4.Date.AddDays(-set);
							gXDLMSProfileGeneric.To = dateTime4.Date;
						}
					}
					else if (set == 1)
					{
						gXDLMSProfileGeneric.From = dateTime4.Date;
						gXDLMSProfileGeneric.To = dateTime4;
					}
					else
					{
						gXDLMSProfileGeneric.From = now.AddDays(-set).Date;
						gXDLMSProfileGeneric.To = now;
					}
					break;
				}
				case "ReadInstrumentationProfile":
				{
					DateTime dateTime2 = starttime;
					int days2 = (now - dateTime2).Days;
					if (TaskPara.IsFromDate)
					{
						if (set == 1 && days2 == 0)
						{
							gXDLMSProfileGeneric.From = dateTime2.Date;
							gXDLMSProfileGeneric.To = dateTime2;
						}
						else if (set == 1 && days2 > 0)
						{
							gXDLMSProfileGeneric.From = dateTime2.Date.AddDays(-1.0);
							gXDLMSProfileGeneric.To = dateTime2.Date;
						}
						else
						{
							gXDLMSProfileGeneric.From = dateTime2.Date;
							gXDLMSProfileGeneric.To = dateTime2.Date.AddDays(set - 1);
						}
					}
					else if (set == 1)
					{
						gXDLMSProfileGeneric.From = dateTime2.Date;
						gXDLMSProfileGeneric.To = dateTime2;
					}
					else
					{
						gXDLMSProfileGeneric.From = now.AddDays(-set).Date;
						gXDLMSProfileGeneric.To = now;
					}
					break;
				}
				case "ReadLoadProfile_A0":
				{
					DateTime dateTime3 = starttime;
					int days3 = (now - dateTime3).Days;
					if (TaskPara.IsFromDate)
					{
						if (set == 1 && days3 == 0)
						{
							gXDLMSProfileGeneric.From = dateTime3.Date;
							gXDLMSProfileGeneric.To = dateTime3;
						}
						else if (set == 1 && days3 > 0)
						{
							gXDLMSProfileGeneric.From = dateTime3.Date.AddDays(-1.0);
							gXDLMSProfileGeneric.To = dateTime3.Date;
						}
						else
						{
							gXDLMSProfileGeneric.To = dateTime3.Date.AddDays(set - 1);
							gXDLMSProfileGeneric.From = dateTime3.Date;
						}
					}
					else if (set == 1)
					{
						gXDLMSProfileGeneric.From = dateTime3.Date;
						gXDLMSProfileGeneric.To = dateTime3;
					}
					else
					{
						gXDLMSProfileGeneric.From = now.AddDays(-(set - 1)).Date;
						gXDLMSProfileGeneric.To = now;
					}
					break;
				}
				case "ReadLoadProfile":
				{
					DateTime dateTime = starttime;
					int days = (now - dateTime).Days;
					if (TaskPara.IsFromDate)
					{
						if (set == 1 && days == 0)
						{
							gXDLMSProfileGeneric.From = dateTime.Date;
							gXDLMSProfileGeneric.To = dateTime;
						}
						else if (set == 1 && days > 0)
						{
							gXDLMSProfileGeneric.From = dateTime.Date.AddDays(-1.0);
							gXDLMSProfileGeneric.To = dateTime.Date;
						}
						else
						{
							gXDLMSProfileGeneric.From = dateTime.Date;
							gXDLMSProfileGeneric.To = dateTime.Date.AddDays(set - 1);
						}
					}
					else if (set == 1)
					{
						gXDLMSProfileGeneric.From = dateTime.Date;
						gXDLMSProfileGeneric.To = dateTime;
					}
					else
					{
						gXDLMSProfileGeneric.From = now.AddDays(-set).Date;
						gXDLMSProfileGeneric.To = now;
					}
					break;
				}
				case "ReadHistoricalRegister":
					switch (set)
					{
					default:
						gXDLMSProfileGeneric.From = Convert.ToDateTime(value).AddMonths(-(set - 1));
						gXDLMSProfileGeneric.To = now;
						break;
					case 1:
						gXDLMSProfileGeneric.From = Convert.ToDateTime(value);
						gXDLMSProfileGeneric.To = now;
						break;
					case -1:
						gXDLMSProfileGeneric.AccessSelector = AccessRange.Entry;
						gXDLMSProfileGeneric.From = Convert.ToDateTime(value);
						gXDLMSProfileGeneric.To = Convert.ToDateTime(value).AddMonths(-100);
						break;
					}
					break;
				}
			}
			catch (Exception ex)
			{
				AddLog(MediaLog.MessageType.Outgoing, ex.Message.ToString());
			}
		}

		public override void Export2CSV(string pathSchedule, string pathmanual, string exportCompany, bool CheckpathManual, bool bOpen = false)
		{
			Export2CSV(pathSchedule, pathmanual, exportCompany, CheckpathManual, outstringLandisData.ToString(), bOpen);
		}

		private void Read(object[] parameters, string serial, string nametask, int set, DateTime starttime)
		{
			try
			{
				object obj = parameters[0];
				if (obj == null)
				{
					return;
				}
				if (obj is GXDLMSDeviceCollection)
				{
					ReadDevices(serial, nametask, set, starttime);
				}
				else if (obj is GXDLMSDevice)
				{
					ReadDevice(obj as GXDLMSDevice, serial, nametask, set, starttime);
				}
				else if (obj is GXDLMSObject)
				{
					GXDLMSObject gXDLMSObject = obj as GXDLMSObject;
					GXDLMSDevice gXDLMSDevice = gXDLMSObject.Parent.Parent as GXDLMSDevice;
					gXDLMSDevice.KeepAliveStop();
					try
					{
						GXDLMSCommunicator comm = gXDLMSDevice.Comm;
						comm.OnBeforeRead = (ReadEventHandler)Delegate.Combine(comm.OnBeforeRead, new ReadEventHandler(OnBeforeRead));
						GXDLMSCommunicator comm2 = gXDLMSDevice.Comm;
						comm2.OnAfterRead = (ReadEventHandler)Delegate.Combine(comm2.OnAfterRead, new ReadEventHandler(OnAfterRead));
						gXDLMSDevice.Comm.loadIDTask(serial, nametask);
						gXDLMSDevice.Comm.Read(this, gXDLMSObject, 0, set, starttime);
					}
					finally
					{
						GXDLMSCommunicator comm3 = gXDLMSDevice.Comm;
						comm3.OnBeforeRead = (ReadEventHandler)Delegate.Remove(comm3.OnBeforeRead, new ReadEventHandler(OnBeforeRead));
						GXDLMSCommunicator comm4 = gXDLMSDevice.Comm;
						comm4.OnAfterRead = (ReadEventHandler)Delegate.Remove(comm4.OnAfterRead, new ReadEventHandler(OnAfterRead));
					}
					gXDLMSDevice.KeepAliveStart();
				}
				else
				{
					if (!(obj is GXDLMSObjectCollection))
					{
						return;
					}
					GXDLMSObjectCollection gXDLMSObjectCollection = obj as GXDLMSObjectCollection;
					GXDLMSDevice gXDLMSDevice2 = gXDLMSObjectCollection[0].Parent.Parent as GXDLMSDevice;
					gXDLMSDevice2.KeepAliveStop();
					foreach (GXDLMSObject item in gXDLMSObjectCollection)
					{
						try
						{
							GXDLMSCommunicator comm5 = gXDLMSDevice2.Comm;
							comm5.OnBeforeRead = (ReadEventHandler)Delegate.Combine(comm5.OnBeforeRead, new ReadEventHandler(OnBeforeRead));
							GXDLMSCommunicator comm6 = gXDLMSDevice2.Comm;
							comm6.OnAfterRead = (ReadEventHandler)Delegate.Combine(comm6.OnAfterRead, new ReadEventHandler(OnAfterRead));
							gXDLMSDevice2.Comm.Read(this, item, 0, set, starttime);
						}
						finally
						{
							GXDLMSCommunicator comm7 = gXDLMSDevice2.Comm;
							comm7.OnBeforeRead = (ReadEventHandler)Delegate.Remove(comm7.OnBeforeRead, new ReadEventHandler(OnBeforeRead));
							GXDLMSCommunicator comm8 = gXDLMSDevice2.Comm;
							comm8.OnAfterRead = (ReadEventHandler)Delegate.Remove(comm8.OnAfterRead, new ReadEventHandler(OnAfterRead));
						}
					}
					gXDLMSDevice2.KeepAliveStart();
					return;
				}
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception ex2)
			{
				AddLog(MediaLog.MessageType.Error, ex2.ToString());
			}
			finally
			{
			}
		}

		private void AddLog(MediaLog.MessageType type, string msg)
		{
			if (m_UseESMedia)
			{
				base.Media.AddLog(type, msg);
			}
			else
			{
				MediaLog.DisplayData(type, msg);
			}
		}

		private void DoWork(TaskInfo para = null)
		{
			if (para == null)
			{
				para = TaskPara;
			}
			if (!m_UseESMedia)
			{
                LoadManufacturer(para.DeviceType);
				FileInfo fileInfo = new FileInfo(ObisProfilePath);
				string path = fileInfo.DirectoryName + "\\LANDISANDGYR\\" + para.MeterID + "_" + para.Taskname + ".gxc";
				if (File.Exists(path))
				{
					parameterdevice = LoadFile(path, para.MeterID);
				}
				else
				{
					parameterdevice = LoadFile(ObisProfile, para.MeterID);
				}
				if (Connect(parameterdevice))
				{
					AddLog(MediaLog.MessageType.Outgoing, "Connected!\n");
					Read(parameterdevice, para.MeterID, para.Taskname, para.SetNumber, para.StartDate);
					Disconnect(parameterdevice);
				}
				return;
			}
			LoadManufacturer(para.DeviceType);
			FileInfo fileInfo2 = new FileInfo(ObisProfilePath);
			string path2 = fileInfo2.DirectoryName + "\\LANDISANDGYR\\" + para.MeterID + "_" + para.Taskname + ".gxc";
			if (File.Exists(path2))
			{
				parameterdevice = LoadFile(path2, para.MeterID);
			}
			else
			{
				parameterdevice = LoadFile(ObisProfile, para.MeterID);
			}
			if (ConnectES(parameterdevice))
			{
				AddLog(MediaLog.MessageType.Outgoing, "Connected!");
				para.SetNumber++;
				Read(parameterdevice, para.MeterID, para.Taskname, para.SetNumber, para.StartDate);
				DisconnectES(parameterdevice);
			}
		}

		public override void ReadCurrentRegisterValues(TaskInfo para = null)
		{
			DoWork(para);
		}

		public override void ReadHistoricalRegister(TaskInfo para = null)
		{
			DoWork(para);
		}

		public override void ReadLoadProfile(TaskInfo para = null)
		{
			DoWork(para);
		}

		public override void ReadLoadProfile_A0(TaskInfo para = null)
		{
			DoWork(para);
		}

		public override void ReadInstrumentationProfile(TaskInfo para = null)
		{
			DoWork(para);
		}

		public override void ReadEventLog(TaskInfo para = null)
		{
			DoWork(para);
		}

		public override void WriteSyncDateTime(TaskInfo para = null)
		{
			throw new NotImplementedException();
		}
	}
}
