using System.Threading;
using System.Windows.Forms;

namespace GXDLMSDirector
{
    internal enum AsyncState
    {
        Start,
        Finish,
        Cancel
    }
    internal delegate void AsyncStateChangeEventHandler(Control sender, AsyncState state);
    internal delegate void AsyncTransaction(Control sender, object[] parameters);
    public enum DeviceState
    {
        None = 0,
        Initialized = 1,
        Connecting = 2,
        Disconnecting = 3,
        Reading = 4,
        Writing = 5,
        Connected = 0x10
    }
	internal class GXAsyncWork
	{
		private Control Sender;

		private AsyncTransaction Command;

		private object[] Parameters;

		private Thread Thread;

		private AsyncStateChangeEventHandler OnAsyncStateChangeEventHandler;

		public GXAsyncWork(Control sender, AsyncStateChangeEventHandler e, AsyncTransaction command, object[] parameters)
		{
			OnAsyncStateChangeEventHandler = e;
			Sender = sender;
			Command = command;
			Parameters = parameters;
		}

		private void Run()
		{
			Command(Sender, Parameters);
			OnAsyncStateChangeEventHandler(Sender, AsyncState.Finish);
		}

		public void Start()
		{
			OnAsyncStateChangeEventHandler(Sender, AsyncState.Start);
			Thread = new Thread(Run);
			Thread.IsBackground = true;
			Thread.Start();
		}

		public void Cancel()
		{
			OnAsyncStateChangeEventHandler(Sender, AsyncState.Cancel);
			Thread.Abort();
		}
	}
}
