using System;
using Core.Logger;

namespace Core.Logger
{
	public class CoreLoggerDebug:CoreBaseLogger
	{
		override protected void AddLogEntry (string message)
		{
			Console.Write (CreateEntryFrom(message));
		}
	}
}

