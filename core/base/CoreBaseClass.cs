using System;
using System.Collections.Generic;
using Core.Service;
using Core.Base.Interfaces;
using Core.Context;
using Core.Notification;

namespace Core.Base
{
	public class CoreBaseClass : ICoreBaseObject
	{

		private CoreServiceContainer _Sc;
		private CoreContext _Context;
		private Dictionary<string,List<IExecutable>> _Callbacks;
		protected static int NameIndex = 0;
		protected string Prefix = "object";
		private string _Name;

		public CoreBaseClass ()
		{
			this._Callbacks = new Dictionary<string, List<IExecutable>> ();
			this._Name = this.GenerateName ();
			CoreBaseClassFactory.Construct (this);
		}

		public object ServiceAddCallback (Dictionary<string,object>parameters)
		{
			CoreBaseClassFactory.ServiceAddCallback (this, parameters);
			return null;
		}

		public object ServiceAddCallbacks (Dictionary<string,object>parameters)
		{
			CoreBaseClassFactory.ServiceAddCallbacks (this, parameters);
			return null;
		}

		public CoreCallback CreateCallBack (string group)
		{
			return CoreBaseClassFactory.CreateCallBack (this, group);
		}

		public void Log (object message)
		{
			CoreBaseClassFactory.Log (this, message);
		}

		public CoreNotification CreateNotificationByName (String name)
		{
			return (CoreNotification)this.Sc.GetService (CoreNotification.CREATE_NOTIFICATION)
				.AddParam (CoreNotification.NAME, name)
				.Execute ();
		}

		private string GenerateName ()
		{
			return (string)(this.Prefix + CoreBaseClass.NameIndex++);
		}

		public string Name{ get { return this._Name; } }

		public CoreServiceContainer Sc {
			get { return this._Sc; }
			set { this._Sc = value; }
		}

		public Dictionary<string,List<IExecutable>> Callbacks {
			get{ return this._Callbacks; }
			set{ this._Callbacks = value; }
		}

		public CoreContext Context {
			get {
				return _Context;
			}
			set {
				_Context = value;
			}
		}
	}
}