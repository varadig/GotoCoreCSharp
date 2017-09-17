using System;
using Core.Service;
using Core.Base;

namespace Core.Context
{
	public class CoreContext : CoreBaseParameterHolder
	{
		public static CoreContext Instance;
		public CoreServiceContainer Sc;

		public CoreContext ()
		{
			this.Sc = CoreServiceContainer.GetInstance ();
		}

		public static CoreContext GetInstance ()
		{
			if (Instance == null)
				Instance = new CoreContext ();
			return Instance;
		}

		public object GetParam (string name)
		{
			return this.parameters [name];
		}

		override public object Execute ()
		{
			return null;
		}

		override public void Notify ()
		{
		}

		override public void Send ()
		{
		}
	}
}

