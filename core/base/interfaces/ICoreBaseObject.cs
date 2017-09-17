using System;
using System.Collections.Generic;
using Core.Service;
using Core.Context;
using Core.Notification;

namespace Core.Base.Interfaces
{
	public interface ICoreBaseObject
	{
		CoreServiceContainer Sc{ get; set; }

		CoreContext Context{ get; set; }

		Dictionary<string,List<IExecutable>> Callbacks{ get; set; }

		string Name{ get; }

		object ServiceAddCallback (Dictionary<string,object>parameters);

		object ServiceAddCallbacks (Dictionary<string,object>parameters);

		CoreCallback CreateCallBack (string group);
		CoreNotification CreateNotificationByName (String name);
	}
}

