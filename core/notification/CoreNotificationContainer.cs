using System;
using Core.Base;
using System.Collections.Generic;
using Core.Base.Interfaces;

namespace Core.Notification
{
	public class CoreNotificationContainer : CoreBaseClass
	{
		private static CoreNotificationContainer instance;
		private Dictionary<string,List<IExecutable>> mapping = new Dictionary<string, List<IExecutable>> ();

		public CoreNotificationContainer () : base ()
		{
			this.Sc.RegisterService (CoreListener.REGISTER_LISTENER, this.RegisterListener);
			this.Sc.RegisterService (CoreListener.REMOVE_LISTENER, this.RemoveListener);
			this.Sc.RegisterService (CoreNotification.CREATE_NOTIFICATION, this.CreateNotification);
		}

		public static CoreNotificationContainer getInstance ()
		{
			if (instance == null)
				instance = new CoreNotificationContainer ();
			return instance;

		}

		private object RegisterListener (Dictionary<string,object> parameters)
		{
			string name = (string)parameters [CoreListener.NAME];
			CoreListener listener = (CoreListener)parameters [CoreListener.LISTENER];
			if (!this.HasNotification (name))
				this.mapping [name] = new List<IExecutable> ();
			this.GetListenersOf (name).Add (listener);
			return null;
		}

		private object RemoveListener (Dictionary<string, object> parameters)
		{
			string name = (string)parameters [CoreListener.NAME];
			IExecutable reference = (IExecutable)parameters [CoreListener.REFERENCE];

			List<IExecutable> listeners = (List<IExecutable>)this.GetListenersOf (name);
			if (listeners.Contains (reference))
				listeners.Remove (reference);
			return null;
		}

		private CoreNotification CreateNotification (Dictionary<string, object> parameters)
		{
			string name = (string)parameters [CoreNotification.NAME];

			return new CoreNotification (name, this.GetListenersOf (name));
		}

		private bool HasNotification (string name)
		{
			return this.mapping.ContainsKey (name);
		}

		private List<IExecutable> GetListenersOf (string name)
		{

			return (this.HasNotification (name) ? this.mapping [name] : new List<IExecutable> ());
		}
	}
}

