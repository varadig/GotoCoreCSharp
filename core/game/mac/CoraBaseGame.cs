
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Core.Context;
using Core.Service;
using Core.Base.Interfaces;
using Core.Base;
using Core.Notification;
using Microsoft.Xna.Framework.Graphics;

namespace Core.Game
{
	public class CoreGame:Microsoft.Xna.Framework.Game, ICoreBaseObject
	{
		protected GraphicsDeviceManager graphics;
		protected SpriteBatch spritePatch;
		private CoreServiceContainer _Sc;
		private CoreContext _Context;
		private Dictionary<string,List<IExecutable>> _Callbacks = new Dictionary<string, List<IExecutable>> ();
		protected static int NameIndex = 0;
		protected string Prefix = "object";
		private string _Name;

		public CoreGame ()
		{
			this._Name = this.GenerateName ();
			CoreBaseClassFactory.Construct (this);
		}

		#region ICoreBaseObject implementation

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
			return (string)(this.Prefix + CoreGame.NameIndex++);
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
		#endregion
	}
}

