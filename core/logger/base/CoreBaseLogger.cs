using System;
using Core.Base;
using System.Collections;
using Core.Utils;

namespace Core.Logger
{
	abstract public class CoreBaseLogger : CoreBaseClass
	{
		protected string br = "\n";

		public void AddLog (IEnumerable message)
		{
			this.AddLogEntry (Convert.ToString (message));	
		}

		public void AddLog (string message)
		{
			this.AddLogEntry (message);

		}

		abstract protected void AddLogEntry (string message);

		protected string CreateEntryFrom (string message)
		{
			return (CoreUtils.HostName +"\t-\t"+ CoreUtils.TimeStamp () + ":\t" + message + this.br);
		}
	}
}

