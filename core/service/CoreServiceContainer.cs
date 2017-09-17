using System;
using Core.Service;
using System.Collections.Generic;

namespace Core.Service
{
	public class CoreServiceContainer
	{
		private Dictionary<string,Func<Dictionary<string,object>,object>> Mapping = new Dictionary<string, Func<Dictionary<string, object>, object>> ();
		private static CoreServiceContainer Instance;

		public CoreServiceContainer ()
		{
		}

		public static CoreServiceContainer GetInstance ()
		{
			if (Instance == null)
				Instance = new CoreServiceContainer ();
			return Instance;
		}

		public void RegisterService (string name, Func<Dictionary<string,object>,object> reference)
		{
			this.Mapping [name] = reference;
		}

		public void RemoveService (string Name)
		{
			this.Mapping.Remove (Name);
		}

		public CoreService GetService (string Name)
		{
			if (!this.Mapping.ContainsKey (Name)) {
			}
			return new CoreService (Name, this.Mapping [Name]);
		}

		public bool HasService (string name)
		{
			return this.Mapping.ContainsKey (name);
		}
	}
}

