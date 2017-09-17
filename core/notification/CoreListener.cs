using System;
using System.Collections.Generic;
using Core.Base;
using Core.Service;

namespace Core.Notification
{
	public class CoreListener : CoreBaseFunctionWrapper
	{
		public static readonly string REGISTER_LISTENER = "register.listener";
		public static readonly string REMOVE_LISTENER = "remove.listener";
		public static readonly string LISTENER = "listener";
		public static readonly string NAME = "name";

		public static readonly string REFERENCE = "reference";

		public CoreListener (string name, Func<Dictionary<string,object>,object> reference) : base (name, reference)
		{

		}

		public override void Notify ()
		{
			this.Call ();
		}

		public override object Clone ()
		{
			throw new NotImplementedException ();
		}

		public override object Execute ()
		{
			throw new NotImplementedException ();
		}

		public override void Send ()
		{
			throw new NotImplementedException ();
		}

		public static void Register(string name, Func<Dictionary<string,object>,object> reference) {
			CoreListener listener = new CoreListener (name, reference);
				CoreServiceContainer.GetInstance().GetService(CoreListener.REGISTER_LISTENER)
				.AddParam(CoreListener.LISTENER, listener)
				.AddParam(CoreListener.NAME, name)
				.Execute();
		}

		public static void Unregister(string name, Func<Dictionary<string,object>,object> reference) {
			CoreServiceContainer.GetInstance().GetService(CoreListener.REMOVE_LISTENER)
				.AddParam(CoreListener.NAME, name)
				.AddParam(CoreListener.REFERENCE, reference)
				.Execute();
		}
	}
}

