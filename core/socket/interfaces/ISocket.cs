using System.Collections.Generic;


namespace Core.Socket
{
	public interface ISocket
	{
		void Bind (int port);
		void UnBind (int port);
		void Send(Dictionary<string, object> parameters);
	}

}

