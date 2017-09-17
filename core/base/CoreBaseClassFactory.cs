using System;
using System.Collections.Generic;
using Core.Base.Interfaces;
using Core.Service;
using Core.Context;
using Core.Logger;

namespace Core.Base
{
	public class CoreBaseClassFactory
	{
		public CoreBaseClassFactory ()
		{

		}

		public static void  Construct (ICoreBaseObject _instance)
		{
			_instance.Sc = CoreServiceContainer.GetInstance ();
			_instance.Context = CoreContext.GetInstance ();

			_instance.Sc.RegisterService (_instance.Name + ".add.callback", _instance.ServiceAddCallback);
			_instance.Sc.RegisterService (_instance.Name + ".add.callbacks", _instance.ServiceAddCallbacks);
		}

		public static object ServiceAddCallback (ICoreBaseObject _instance, Dictionary<string,object>parameters)
		{
			string group = (String)parameters [CoreCallback.GROUP];
			IExecutable callback = (IExecutable)parameters [CoreCallback.CALLBACK];

			CoreBaseClassFactory.AddCallback (_instance, group, callback);
			return null;
		}


		public static object ServiceAddCallbacks (ICoreBaseObject _instance, Dictionary<string,object>parameters)
		{
			string group = (string)parameters ["group"];
			List<IExecutable> callbacks = (List<IExecutable>)parameters ["callbacks"];

			foreach (IExecutable callback in callbacks)
				CoreBaseClassFactory.AddCallback (_instance, group, callback);
			return null;
		}

		public static CoreCallback CreateCallBack (ICoreBaseObject _instance, string group)
		{
			if (!_instance.Callbacks.ContainsKey (group))
				_instance.Callbacks.Add (group, new List<IExecutable> ());

			return new CoreCallback (group, (List<IExecutable>)_instance.Callbacks [group]);
		}

		public static void AddCallback (ICoreBaseObject _instance, string  group, IExecutable callback)
		{
			if (!_instance.Callbacks.ContainsKey (group))
				_instance.Callbacks.Add (group,new List<IExecutable>());

			_instance.Callbacks [group].Add (callback);
		}

		public static object Log (ICoreBaseObject _instance, object message)
		{
			if (_instance.Sc.HasService (CoreLogger.LOGGER_LOG))
				_instance.Sc.GetService (CoreLogger.LOGGER_LOG).AddParam (CoreLogger.MESSAGE, message).Execute ();
			return null;
		}
	}
}

