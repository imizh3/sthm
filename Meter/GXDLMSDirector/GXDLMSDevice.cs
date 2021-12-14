using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Timers;
using System.Xml.Serialization;
using Gurux.Common;
using Gurux.DLMS;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.DLMS.Objects;
using GXDLMS.Common;
using GXDLMS.ManufacturerSettings;
using STHM.Media;

namespace GXDLMSDirector
{
	[Serializable]
	public class GXDLMSDevice
	{
		private DeviceState m_Status;

		private DataSet m_DataSet;

		[Browsable(false)]
		[XmlIgnore]
		private GXDLMSObjectCollection m_Objects;

		[Browsable(false)]
		[XmlIgnore]
		public ProgressEventHandler OnProgress;

		[Browsable(false)]
		[XmlIgnore]
		public StatusEventHandler OnStatusChanged;

		[Browsable(false)]
		[XmlIgnore]
		private Timer KeepAlive;

		[Browsable(false)]
		[XmlIgnore]
		private GXDLMSCommunicator m_Communicator;

		[Browsable(false)]
		[XmlIgnore]
		public DeviceState Status
		{
			get
			{
				return m_Status;
			}
		}

		[XmlIgnore]
		public InactivityMode InactivityMode
		{
			get
			{
				return Manufacturers.FindByIdentification(Manufacturer).InactivityMode;
			}
		}

		[XmlIgnore]
		public bool ForceInactivity
		{
			get
			{
				return Manufacturers.FindByIdentification(Manufacturer).ForceInactivity;
			}
		}

		[DefaultValue(5)]
		public int WaitTime { get; set; }

		[DefaultValue(Authentication.None)]
		public Authentication Authentication { get; set; }

		[DefaultValue("")]
		public string Password { get; set; }

		[DefaultValue(null)]
		public object PhysicalAddress { get; set; }

		[DefaultValue(null)]
		public int LogicalAddress { get; set; }

		[DefaultValue(16)]
		public object ClientID { get; set; }

		[DefaultValue(StartProtocolType.IEC)]
		public StartProtocolType StartProtocol { get; set; }

		[DefaultValue(false)]
		public bool UseRemoteSerial { get; set; }

		public string Name { get; set; }

		public GXDLMSCommunicator Comm
		{
			get
			{
				return m_Communicator;
			}
		}

		[Browsable(false)]
		[XmlIgnore]
		public GXObisCodeCollection ObisCodes
		{
			get
			{
				return m_Communicator.m_Cosem.ObisCodes;
			}
			set
			{
				m_Communicator.m_Cosem.ObisCodes = value;
			}
		}

		[Browsable(false)]
		[XmlIgnore]
		public IGXManufacturerExtension Extension { get; internal set; }

		[Browsable(false)]
		[XmlIgnore]
		public IGXMedia Media
		{
			get
			{
				return m_Communicator.Media;
			}
			set
			{
				m_Communicator.Media = value;
			}
		}

		[Browsable(false)]
		[XmlIgnore]
		public ESMedia esMedia
		{
			get
			{
				return m_Communicator.m_ESMedia;
			}
			set
			{
				m_Communicator.m_ESMedia = value;
				m_Communicator.m_bESMediaType = true;
			}
		}

		[Browsable(false)]
		[XmlIgnore]
		public GXManufacturerCollection Manufacturers { get; set; }

		[Browsable(false)]
		public string Manufacturer { get; set; }

		[Browsable(false)]
		public HDLCAddressType HDLCAddressing { get; set; }

		[Browsable(false)]
		public bool UseIEC47
		{
			get
			{
				return Manufacturers.FindByIdentification(Manufacturer).UseIEC47;
			}
		}

		[Browsable(false)]
        [XmlAttribute]
		public string MediaType
		{
			get
			{
				return m_Communicator.Media.GetType().ToString();
			}
			set
			{
				Type type = Type.GetType(value);
				if (type == null)
				{
					throw new Exception("Invalid media type " + value);
				}
				m_Communicator.Media = (IGXMedia)Activator.CreateInstance(type);
			}
		}
        [XmlAttribute]
		public string MediaSettings
		{
			get
			{
				return m_Communicator.Media.Settings;
			}
			set
			{
				m_Communicator.Media.Settings = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[XmlArray("Objects2")]
		public GXDLMSObjectCollection Objects
		{
			get
			{
				return m_Objects;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[XmlArray("Objects")]
		[XmlArrayItem("Object", Type = typeof(GXDLMSObjectSerializer<GXDLMSObject>))]
		public GXDLMSObjectCollection ObsoleteObjects
		{
			get
			{
				return m_Objects;
			}
		}

		private void UpdateStatus(DeviceState state)
		{
			if (state == DeviceState.Connected)
			{
				state &= (DeviceState)(-3);
				state |= DeviceState.Initialized;
			}
			m_Status = state;
			if (OnStatusChanged != null)
			{
				OnStatusChanged(this, m_Status);
			}
		}

		internal static DataType GetAttributeType(GXDLMSObject component, int attributeIndex)
		{
			if (attributeIndex != 0)
			{
				if (attributeIndex > 16)
				{
					attributeIndex = 2;
				}
				GXDLMSAttributeSettings gXDLMSAttributeSettings = component.Attributes.Find(attributeIndex);
				if (gXDLMSAttributeSettings != null && gXDLMSAttributeSettings.Type != 0)
				{
					return gXDLMSAttributeSettings.Type;
				}
				PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
				foreach (PropertyDescriptor item in properties)
				{
					GXDLMSAttributeSettings gXDLMSAttributeSettings2 = (GXDLMSAttributeSettings)item.Attributes[typeof(GXDLMSAttributeSettings)];
					if (gXDLMSAttributeSettings2 != null && gXDLMSAttributeSettings2.Index == attributeIndex)
					{
						if (gXDLMSAttributeSettings2.UIType != 0)
						{
							return gXDLMSAttributeSettings2.UIType;
						}
						if (item.PropertyType == typeof(int))
						{
							return DataType.Int32;
						}
						if (item.PropertyType == typeof(uint))
						{
							return DataType.UInt32;
						}
						if (item.PropertyType == typeof(string))
						{
							return DataType.String;
						}
						if (item.PropertyType == typeof(byte))
						{
							return DataType.UInt8;
						}
						if (item.PropertyType == typeof(sbyte))
						{
							return DataType.Int8;
						}
						if (item.PropertyType == typeof(short))
						{
							return DataType.Int16;
						}
						if (item.PropertyType == typeof(ushort))
						{
							return DataType.UInt16;
						}
						if (item.PropertyType == typeof(long))
						{
							return DataType.Int64;
						}
						if (item.PropertyType == typeof(ulong))
						{
							return DataType.UInt64;
						}
						if (item.PropertyType == typeof(float))
						{
							return DataType.Float32;
						}
						if (item.PropertyType == typeof(double))
						{
							return DataType.Float64;
						}
						if (item.PropertyType == typeof(DateTime))
						{
							return DataType.DateTime;
						}
						if (item.PropertyType == typeof(bool) || item.PropertyType == typeof(bool))
						{
							return DataType.Boolean;
						}
						if (item.PropertyType == typeof(object))
						{
							return DataType.None;
						}
					}
				}
			}
			return DataType.None;
		}

		public void KeepAliveStart()
		{
			if (InactivityMode != 0)
			{
				KeepAlive.Interval = Manufacturers.FindByIdentification(Manufacturer).KeepAliveInterval;
				KeepAlive.Start();
			}
		}

		public void KeepAliveStop()
		{
			KeepAlive.Stop();
		}

		private void NotifyProgress(object sender, string description, int current, int maximium)
		{
			if (OnProgress != null)
			{
				OnProgress(sender, description, current, maximium);
			}
		}

		public void InitializeConnection()
		{
			try
			{
				UpdateStatus(DeviceState.Connecting);
				m_Communicator.InitializeConnection();
				UpdateStatus(DeviceState.Connected);
			}
			catch (Exception ex)
			{
				MediaLog.DisplayData(MediaLog.MessageType.Error, ex.ToString());
				UpdateStatus(DeviceState.Initialized);
				if (Media != null)
				{
					Media.Close();
				}
				throw ex;
			}
		}

		public void InitializeConnectionES(ESMedia media, string HDLCadress)
		{
			try
			{
				esMedia = media;
				m_Communicator.InitializeConnectionES(HDLCadress);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public GXDLMSDevice(IGXMedia media)
		{
			StartProtocol = StartProtocolType.IEC;
			ClientID = 16;
			PhysicalAddress = 1;
			Password = "";
			Authentication = Authentication.None;
			m_Objects = new GXDLMSObjectCollection();
			m_Objects.Parent = this;
			m_Communicator = new GXDLMSCommunicator(this, media);
			GXDLMSCommunicator communicator = m_Communicator;
			communicator.OnProgress = (ProgressEventHandler)Delegate.Combine(communicator.OnProgress, new ProgressEventHandler(NotifyProgress));
			KeepAlive = new Timer();
			KeepAlive.Interval = 40000;
			KeepAlive.Elapsed += KeepAlive_Elapsed;
			m_Status = DeviceState.Initialized;
			WaitTime = 5;
		}

		public GXDLMSDevice()
			: this(null)
		{
		}

		public void Disconnect()
		{
			try
			{
				UpdateStatus(DeviceState.Disconnecting);
				if (KeepAlive.Enabled)
				{
					KeepAlive.Stop();
				}
				if (m_Communicator.Media.IsOpen)
				{
					byte[] data = m_Communicator.DisconnectRequest();
					m_Communicator.ReadDLMSPacket(data);
				}
			}
			catch (Exception ex)
			{
				MediaLog.DisplayData(MediaLog.MessageType.Error, ex.Message);
			}
			finally
			{
				if (m_Communicator.Media != null)
				{
					m_Communicator.Media.Close();
				}
				UpdateStatus(DeviceState.Initialized);
				MediaLog.DisplayData(MediaLog.MessageType.Outgoing, "Disconnected!");
			}
		}

		public void DisconnectES()
		{
			try
			{
				if (KeepAlive.Enabled)
				{
					KeepAlive.Stop();
				}
				byte[] data = m_Communicator.DisconnectRequest();
				m_Communicator.ReadDLMSPacket(data);
			}
			catch (Exception ex)
			{
				m_Communicator.m_ESMedia.AddLog(MediaLog.MessageType.Error, ex.Message);
			}
			finally
			{
				m_Communicator.m_ESMedia.AddLog(MediaLog.MessageType.Outgoing, "Disconnected!");
			}
		}

		private GXDLMSObject FindObject(ObjectType type, string logicalName)
		{
			foreach (GXDLMSObject @object in m_Objects)
			{
				if (type == @object.ObjectType && @object.LogicalName == logicalName)
				{
					return @object;
				}
			}
			return null;
		}

		public static GXDLMSObject ConvertObject2Class(GXDLMSDevice device, ObjectType objectType, string logicalName)
		{
			GXDLMSObject gXDLMSObject = GXDLMSClient.CreateObject(objectType);
			if (gXDLMSObject != null)
			{
				GXManufacturer gXManufacturer = device.Manufacturers.FindByIdentification(device.Manufacturer);
				GXObisCode gXObisCode = gXManufacturer.ObisCodes.FindByLN(gXDLMSObject.ObjectType, logicalName, null);
				gXDLMSObject.LogicalName = logicalName;
				if (gXObisCode != null)
				{
					gXDLMSObject.Description = gXObisCode.Description;
				}
			}
			return gXDLMSObject;
		}

		private DataColumn AddColumn(GXDLMSProfileGeneric item, GXDLMSObject row, DataType type, string text, int pos)
		{
			item.Columns.Add(row);
			DataColumn dataColumn = new DataColumn("O" + pos);
			dataColumn.Caption = text;
			if (((IGXDLMSColumnObject)item).SelectedAttributeIndex > 0 && ((uint)((IGXDLMSColumnObject)item).SelectedAttributeIndex & 0x18u) != 0)
			{
				return dataColumn;
			}
			if (type != 0)
			{
				Type type2 = GXHelpers.FromDLMSDataType(type);
				if (type2 != null)
				{
					dataColumn.DataType = type2;
				}
			}
			return dataColumn;
		}

		private bool ShouldSkip(GXDLMSObject it, int index)
		{
			if (it is GXDLMSRegister && index == 3)
			{
				return true;
			}
			return false;
		}

		public void UpdateColumns(GXDLMSProfileGeneric item, GXManufacturer man, int set, DateTime dtime)
		{
			item.Data.Columns.Clear();
			item.Data.Rows.Clear();
			item.Columns.Clear();
			int num = 0;
			GXDLMSObjectCollection gXDLMSObjectCollection = null;
			List<DataColumn> list = new List<DataColumn>();
			if (Extension != null)
			{
				gXDLMSObjectCollection = Extension.Refresh(item, Comm, set, dtime);
			}
			if (gXDLMSObjectCollection == null)
			{
				gXDLMSObjectCollection = Comm.GetProfileGenericColumns(item.Name, set, dtime);
			}
			Comm.DeviceColumns[item] = gXDLMSObjectCollection;
			foreach (GXDLMSObject item2 in gXDLMSObjectCollection)
			{
				IGXDLMSColumnObject iGXDLMSColumnObject = item2;
				string text = item2.Description;
				DataType type = DataType.None;
				if (item2.ObjectType == ObjectType.ProfileGeneric)
				{
					GXDLMSObjectCollection gXDLMSObjectCollection2 = null;
					if (!Comm.DeviceColumns.ContainsKey(item2))
					{
						if (Extension != null)
						{
							gXDLMSObjectCollection2 = Extension.Refresh(item2 as GXDLMSProfileGeneric, Comm, set, dtime);
						}
						if (gXDLMSObjectCollection2 == null)
						{
							gXDLMSObjectCollection2 = Comm.GetProfileGenericColumns(item2, set, dtime);
						}
						Comm.DeviceColumns[item2] = gXDLMSObjectCollection2;
					}
					else
					{
						gXDLMSObjectCollection2 = Comm.DeviceColumns[item2];
					}
					int num2 = 0;
					foreach (GXDLMSObject item3 in gXDLMSObjectCollection2)
					{
						if (iGXDLMSColumnObject.SelectedAttributeIndex != num2)
						{
							num2++;
							continue;
						}
						IGXDLMSColumnObject iGXDLMSColumnObject2 = item3;
						if (iGXDLMSColumnObject2.SelectedAttributeIndex == 0)
						{
							Array values = item3.GetValues();
							if (values == null)
							{
								break;
							}
							for (int i = 1; i != values.Length; i++)
							{
								if (!ShouldSkip(item3, i + 1))
								{
									type = DataType.None;
									GXDLMSObject gXDLMSObject = MakeColumn(man, item3, ref text, ref type, i + 1);
									iGXDLMSColumnObject2 = gXDLMSObject;
									iGXDLMSColumnObject2.SourceLogicalName = item2.LogicalName;
									iGXDLMSColumnObject2.SourceObjectType = item2.ObjectType;
									list.Add(AddColumn(item, gXDLMSObject, type, text, num++));
									iGXDLMSColumnObject2.SelectedAttributeIndex = num2;
									iGXDLMSColumnObject2.SelectedDataIndex = i + 1;
								}
							}
						}
						else
						{
							type = DataType.None;
							GXDLMSObject gXDLMSObject2 = MakeColumn(man, item3, ref text, ref type, iGXDLMSColumnObject2.SelectedAttributeIndex);
							gXDLMSObject2.SelectedAttributeIndex = num2;
							iGXDLMSColumnObject2 = gXDLMSObject2;
							iGXDLMSColumnObject2.SourceLogicalName = item2.LogicalName;
							iGXDLMSColumnObject2.SourceObjectType = item2.ObjectType;
							list.Add(AddColumn(item, gXDLMSObject2, type, text, num++));
						}
						break;
					}
				}
				else if (iGXDLMSColumnObject.SelectedAttributeIndex == 0)
				{
					Array values2 = item2.GetValues();
					if (values2 == null)
					{
						continue;
					}
					for (int j = 1; j != values2.Length; j++)
					{
						if (!ShouldSkip(item2, j + 1))
						{
							type = DataType.None;
							GXDLMSObject gXDLMSObject3 = MakeColumn(man, item2, ref text, ref type, j + 1);
							iGXDLMSColumnObject = gXDLMSObject3;
							iGXDLMSColumnObject.SourceLogicalName = item2.LogicalName;
							iGXDLMSColumnObject.SourceObjectType = item2.ObjectType;
							list.Add(AddColumn(item, gXDLMSObject3, type, text, num++));
							iGXDLMSColumnObject.SelectedDataIndex = j + 1;
						}
					}
				}
				else
				{
					list.Add(AddColumn(item, MakeColumn(man, item2, ref text, ref type, iGXDLMSColumnObject.SelectedAttributeIndex), type, text, num++));
				}
			}
			item.Data.Columns.AddRange(list.ToArray());
		}

		private GXDLMSObject MakeColumn(GXManufacturer man, GXDLMSObject it, ref string text, ref DataType type, int index)
		{
			GXObisCode gXObisCode = man.ObisCodes.FindByLN(it.ObjectType, it.LogicalName, null);
			GXDLMSObject gXDLMSObject = m_Objects.FindByLN(it.ObjectType, it.LogicalName);
			if (gXDLMSObject != null)
			{
				gXDLMSObject = gXDLMSObject.Clone();
				IGXDLMSColumnObject iGXDLMSColumnObject = gXDLMSObject;
				iGXDLMSColumnObject.SelectedAttributeIndex = ((IGXDLMSColumnObject)it).SelectedAttributeIndex;
				iGXDLMSColumnObject.SelectedDataIndex = ((IGXDLMSColumnObject)it).SelectedDataIndex;
				type = gXDLMSObject.GetUIDataType(index);
			}
			else
			{
				gXDLMSObject = it;
				if (gXObisCode != null && type == DataType.None)
				{
					GXDLMSAttributeSettings gXDLMSAttributeSettings = gXObisCode.Attributes.Find(((IGXDLMSColumnObject)it).SelectedAttributeIndex);
					if (gXDLMSAttributeSettings != null)
					{
						type = gXDLMSAttributeSettings.Type;
					}
				}
				else
				{
					type = gXDLMSObject.GetUIDataType(index);
				}
			}
			if (gXObisCode == null)
			{
				text = it.LogicalName + " " + it.Description;
			}
			else
			{
				text = it.LogicalName + " " + gXObisCode.Description;
			}
			if (string.IsNullOrEmpty(text))
			{
				text = it.LogicalName;
			}
			return gXDLMSObject;
		}

		public void UpdateObjects(int set, DateTime dtime)
		{
			try
			{
				GXDLMSObjectCollection objects = Comm.GetObjects(set, dtime);
				int num = 0;
				foreach (GXDLMSObject item in objects)
				{
					GXObisCode gXObisCode;
					if ((gXObisCode = ObisCodes.FindByLN(item.ObjectType, item.LogicalName, null)) == null)
					{
						gXObisCode = new GXObisCode(item.LogicalName, item.ObjectType, "");
						ObisCodes.Add(gXObisCode);
					}
					if (item.ObjectType == ObjectType.ProfileGeneric || item.GetType() == typeof(GXDLMSObject))
					{
						continue;
					}
					num++;
					NotifyProgress(this, "Creating object " + item.LogicalName, num, objects.Count);
					if (item.GetDataType(2) == DataType.None && (item is GXDLMSData || item is GXDLMSRegister))
					{
						DataType type = DataType.None;
						try
						{
							Comm.ReadValue(item.ToString(), item.ObjectType, 2, ref type, set, dtime);
							item.SetDataType(2, type);
							GXDLMSAttributeSettings gXDLMSAttributeSettings = gXObisCode.Attributes.Find(2);
							if (gXDLMSAttributeSettings == null)
							{
								gXDLMSAttributeSettings = new GXDLMSAttributeSettings(2);
								gXObisCode.Attributes.Add(gXDLMSAttributeSettings);
							}
							gXDLMSAttributeSettings.Type = type;
							m_Objects.Add(item);
						}
						catch (GXDLMSException ex)
						{
							if (ex.ErrorCode != 1)
							{
								if (ex.ErrorCode == 3)
								{
									item.SetAccess(2, AccessMode.NoAccess);
								}
								else
								{
									Error.ShowError(null, ex);
								}
							}
						}
						catch (Exception ex2)
						{
							Error.ShowError(null, ex2);
							goto IL_01b7;
						}
					}
					else
					{
						m_Objects.Add(item);
					}
				}
				goto IL_01b7;
				IL_01b7:
				GXLogWriter.WriteLog("--- Created " + m_Objects.Count + " objects. ---");
				int num2 = 0;
				foreach (GXDLMSProfileGeneric @object in objects.GetObjects(ObjectType.ProfileGeneric))
				{
					num++;
					NotifyProgress(this, "Creating object " + @object.LogicalName, num, objects.Count);
					@object.Data.TableName = @object.LogicalName + " " + @object.Description;
					try
					{
						NotifyProgress(this, "Get profile generic columns", ++num2, objects.Count);
						UpdateColumns(@object, Manufacturers.FindByIdentification(Manufacturer), set, dtime);
						if (@object.Columns == null || @object.Columns.Count == 0)
						{
							continue;
						}
					}
					catch
					{
						GXLogWriter.WriteLog("Failed to read Profile Generic " + @object.LogicalName + " columns.");
						continue;
					}
					m_Objects.Add(@object);
				}
			}
			finally
			{
				NotifyProgress(this, "", 0, 0);
			}
		}

		public DataSet ToDataSet()
		{
			if (m_DataSet != null)
			{
				return m_DataSet;
			}
			DataTable dataTable = new DataTable("Data Objects");
			dataTable.Columns.Add("Name");
			dataTable.Columns.Add("Value");
			DataTable dataTable2 = new DataTable("Register Objects");
			dataTable2.Columns.Add("Name");
			dataTable2.Columns.Add("Value");
			m_DataSet = new DataSet();
			foreach (GXDLMSObject @object in Objects)
			{
				if (@object.ObjectType == ObjectType.ProfileGeneric)
				{
					GXDLMSProfileGeneric gXDLMSProfileGeneric = @object as GXDLMSProfileGeneric;
					DataTable data = gXDLMSProfileGeneric.Data;
					data.TableName = gXDLMSProfileGeneric.LogicalName + " " + gXDLMSProfileGeneric.Description;
					if (data.DataSet != null)
					{
						data.DataSet.Tables.Remove(data);
					}
					m_DataSet.Tables.Add(data);
				}
				else if (@object.ObjectType == ObjectType.Data)
				{
					DataRow dataRow = dataTable.NewRow();
					dataRow[0] = @object.LogicalName + @object.Description;
					dataRow[1] = ((GXDLMSData)@object).Value;
					dataTable.Rows.Add(dataRow);
				}
				else if (@object.ObjectType == ObjectType.Register)
				{
					DataRow dataRow2 = dataTable2.NewRow();
					dataRow2[0] = @object.LogicalName + @object.Description;
					dataRow2[1] = ((GXDLMSRegister)@object).Value;
					dataTable2.Rows.Add(dataRow2);
				}
			}
			if (dataTable.Rows.Count != 0)
			{
				m_DataSet.Tables.Add(dataTable);
			}
			if (dataTable2.Rows.Count != 0)
			{
				m_DataSet.Tables.Add(dataTable2);
			}
			return m_DataSet;
		}

		private void KeepAlive_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				if (InactivityMode == InactivityMode.Disconnect)
				{
					Disconnect();
				}
				else if (InactivityMode == InactivityMode.KeepAlive)
				{
					NotifyProgress(this, "Keep Alive", 0, 1);
					m_Communicator.KeepAlive();
				}
				else if ((InactivityMode == InactivityMode.Reopen || InactivityMode == InactivityMode.ReopenActive) && DateTime.Now.Subtract(m_Communicator.ConnectionStartTime).TotalSeconds > 40.0)
				{
					Disconnect();
					InitializeConnection();
					m_Communicator.ConnectionStartTime = DateTime.Now;
				}
			}
			catch (Exception ex)
			{
				Disconnect();
				Error.ShowError(null, ex);
			}
			finally
			{
				NotifyProgress(this, "Ready", 1, 1);
			}
		}
	}
}
