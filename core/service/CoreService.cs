using System;
using Core.Base;
using System.Collections.Generic;

namespace Core.Service
{
	public class CoreService :CoreBaseFunctionWrapper
	{
		public CoreService (string name, Func<Dictionary<string,object>,object> reference) : base (name, reference)
		{
		
		}

		public override object Execute ()
		{
			return this.Call ();
		}

		public override object Clone ()
		{
			CoreService service = new CoreService (this.name, this.reference);
			service.AddParams (parameters);
			return service;
		}

		public override void Notify ()
		{
			throw new NotImplementedException ();
		}

		public override void Send ()
		{
			throw new NotImplementedException ();
		}
	}
}

