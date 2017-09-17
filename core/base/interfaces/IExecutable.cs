using System;
using System.Collections.Generic;

namespace Core.Base.Interfaces
{
	public interface IExecutable
	{
		IExecutable AddParam (string name, object value);

		IExecutable AddParams (Dictionary<string,object>parameters);

		IExecutable SetParam (string name, object value);

		IExecutable SetParams (Dictionary<string,object>parameters);

		object Execute ();

		void Notify ();

		void Send ();
	}
}

