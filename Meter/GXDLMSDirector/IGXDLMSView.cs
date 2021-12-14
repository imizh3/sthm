using System.Windows.Forms;
using Gurux.DLMS;
using Gurux.DLMS.Objects;

namespace GXDLMSDirector
{
	public interface IGXDLMSView
	{
		GXDLMSObject Target { get; set; }

		ErrorProvider ErrorProvider { get; }

		string Description { get; set; }

		void OnValueChanged(int attributeID, object value);

		void OnAccessRightsChange(int attributeID, AccessMode access);

		void OnDirtyChange(int attributeID, bool Dirty);
	}
}
