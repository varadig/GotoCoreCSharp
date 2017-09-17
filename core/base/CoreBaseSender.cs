using System;
using System.Collections.Generic;
using Core.Base.Interfaces;

namespace Core.Base
{
	public class CoreBaseSender:CoreBaseParameterHolder
	{
		protected string name;
		protected List<IExecutable> collection;

		public CoreBaseSender (string name, List<IExecutable> collection)
		{
			this.name = name;
			this.collection = collection;
		}

		public override object Execute ()
		{
			return null;
		}

		public override void Notify ()
		{

		}

		public override void Send ()
		{

		}
	}
}

