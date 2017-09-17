using System;
using System.Collections.Generic;
using Core.Base.Interfaces;
using Core.Base;
using Core.Service;

namespace Core.Notification
{
	public class CoreNotification : CoreBaseSender
	{
		public static readonly string CREATE_NOTIFICATION = "create.notification";
		public static readonly string NAME = "name";

		public CoreNotification (string name, List<IExecutable> collection) : base (name, collection)
		{
		}

		public override void Send ()
		{
			foreach (IExecutable listener in this.collection)
				listener.SetParams (this.parameters).Notify ();
		}

		public static CoreNotification CreateNotification(string name){
			Console.WriteLine ("NOTIFICATION NAME:" + name);
			return (CoreNotification) CoreServiceContainer.GetInstance ().GetService (CoreNotification.CREATE_NOTIFICATION)
				.AddParam (CoreNotification.NAME, name)
				.Execute ();
		}
	}
}

