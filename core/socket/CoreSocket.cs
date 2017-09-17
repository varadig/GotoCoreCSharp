using System;
using System.Collections.Generic;
using Core.Base;
using System.Net;

namespace Core.Socket
{
	public class CoreSocket:CoreBaseClass
	{
		/*NOTIFICAIONTS*/
		public static readonly string SOCKET_CLOSED = "core.socket.socket.closed";

		/*SERVICES*/
		public static readonly string SEND_MESSAGE = "core.socket.send.message";
		public static readonly string UNBIND = "core.socket.unbind";
		public static readonly string BIND = "core.socket.bind";

		/*PARAMETERS*/
		public static readonly string PACKAGE = "core.socket.package";
		public static readonly string MESSAGE = "core.socket.message";
		public static readonly string IP_ADDRESS = "core.socket.ip.adress";
		public static readonly string PORT = "core.socket.port";

		private static CoreSocket instance;


		private List<ISocket> sockets;

		public static CoreSocket GetInstance (List<ISocket> sockets = null)
		{
			if (instance == null)
				instance = new CoreSocket (sockets);
			return instance;
		}

		public CoreSocket (List<ISocket> sockets)
		{
			
			this.sockets = sockets;

			if (this.sockets == null)
				this.sockets = new List<ISocket> ();
			
			
			this.Sc.RegisterService (SEND_MESSAGE, this.serviceSendMessage);
			this.Sc.RegisterService (BIND, this.serviceBind);
			this.Sc.RegisterService (UNBIND, this.serviceUnBind);
		}

		private object serviceSendMessage (Dictionary<string, object> parameters)
		{
			foreach (ISocket socket in this.sockets)
				socket.Send (parameters);
			return null;
		}


		private object serviceBind (Dictionary<string, object> parameters)
		{
			foreach (ISocket socket in this.sockets)
				socket.Bind ((int)parameters [PORT]);
			return null;
		}

		private object serviceUnBind (Dictionary<string, object> parameters)
		{
			foreach (ISocket socket in this.sockets)
				socket.UnBind ((int)parameters [PORT]);
			return null;
		}
	}
}

