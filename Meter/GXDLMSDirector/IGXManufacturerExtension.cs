using System;
using Gurux.DLMS.Objects;

namespace GXDLMSDirector
{
	public interface IGXManufacturerExtension
	{
		bool Read(object sender, GXDLMSObject item, GXDLMSObjectCollection columns, int attribute, GXDLMSCommunicator comm, string nametask, int set, DateTime starttime);

		GXDLMSObjectCollection Refresh(GXDLMSProfileGeneric item, GXDLMSCommunicator comm, int set, DateTime starttime);
	}
}
