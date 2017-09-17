using System;
using System.Collections.Generic;
using Core.Base;

namespace Core.Base
{
	abstract public class CoreBaseFunctionWrapper: CoreBaseParameterHolder
	{
		protected string name;
		protected Func<Dictionary<string,object>,object> reference;

		public CoreBaseFunctionWrapper (string name, Func<Dictionary<string,object>,object> reference)
		{
			this.name = name;
			this.reference = reference;
		}

		protected object Call ()
		{
			return this.reference (this.parameters);
		}

		public bool Has (Func<Dictionary<string,object>,object> reference)
		{
			return (this.reference == reference);	
		}
		//Abstract function
		abstract public object Clone ();
	}
}

