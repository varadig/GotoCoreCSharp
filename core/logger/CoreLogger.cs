using System;
using Core.Base;
using System.Collections.Generic;

namespace Core.Logger
{
	public class CoreLogger : CoreBaseClass
	{
		public static readonly string LOGGER_LOG = "logger.log";
		public static readonly string MESSAGE = "message";
		private static CoreLogger instance;
		private List<CoreBaseLogger> loggers;

		public static CoreLogger GetInstance (List<CoreBaseLogger> loggers)
		{
			if (instance == null)
				instance = new CoreLogger (loggers);
			return instance;

		}

		public CoreLogger (List<CoreBaseLogger> loggers)
		{
			this.loggers = loggers;

			this.Sc.RegisterService (LOGGER_LOG, this.ServiceLog);
		}

		private object ServiceLog (Dictionary<string, object> parameters)
		{
			foreach(CoreBaseLogger logger in  loggers)
				logger.AddLog(parameters [MESSAGE] as String);
			return null;
		}
	}
}

