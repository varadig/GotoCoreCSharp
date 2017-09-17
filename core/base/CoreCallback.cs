using System;
using System.Collections.Generic;
using Core.Base;
using Core.Base.Interfaces;
using Core.Service;

namespace Core.Base
{
	public class CoreCallback:CoreBaseSender
	{
		public static readonly string GROUP = "core.callback.group";
		public static readonly string CALLBACK = "core.callback.callback";
		public static readonly string CALLBACKS = "core.callback.callbacks";
		public static readonly string ADD_CALLBACK = ".add.callback";
		public static readonly string ADD_CALLBACKS = ".add.callbacks";


		public CoreCallback (string name, List<IExecutable> collection) : base (name, collection)
		{
		}

		override public void Send ()
		{
			foreach (IExecutable service in collection) 
				service.SetParams (this.parameters).Execute ();
		}

		public static void addCallback (ICoreBaseObject target,string group,IExecutable callback)
		{
			CoreServiceContainer.GetInstance ().GetService (target.Name + ADD_CALLBACK)
				.AddParam (GROUP, group)
				.AddParam (CALLBACK, callback)
				.Execute ();
		}

		public static void addCallbacks(ICoreBaseObject target,string group,List<IExecutable> callbacks)
		{
			CoreServiceContainer.GetInstance ().GetService (target.Name + ADD_CALLBACK)
				.AddParam (GROUP, group)
				.AddParam (CALLBACK, callbacks)
				.Execute ();
		}
	}
}

