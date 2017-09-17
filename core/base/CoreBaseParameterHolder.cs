using System;
using Core.Base.Interfaces;
using System.Collections.Generic;

namespace Core.Base
{
	abstract public class CoreBaseParameterHolder :IExecutable
	{
		protected Dictionary <string,object> parameters = new Dictionary<string, object> ();

		public CoreBaseParameterHolder ()
		{

		}

		public IExecutable AddParam (string name, object value)
		{
			if (!this.parameters.ContainsKey (name))
				this.parameters [name] = value;
			return this;
		}

		public IExecutable AddParams (Dictionary<string,object>parameters)
		{
			foreach (KeyValuePair<string,object> parameter in parameters)
				this.parameters [parameter.Key] = parameter.Value;
			return this;
		}

		public IExecutable SetParam (string name, object value)
		{
			this.parameters [name] = value;
			return this;
		}

		public IExecutable SetParams (Dictionary<string,object>parameters)
		{
			foreach (KeyValuePair<string,object> parameter in parameters)
				this.parameters [parameter.Key] = parameter.Value;
			return this;
		}

		abstract public object Execute ();

		abstract public void Notify ();

		abstract public void Send ();
	}
}
