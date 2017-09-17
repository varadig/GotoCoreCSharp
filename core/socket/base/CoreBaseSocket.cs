using Core.Base;

namespace Core.Socket
{
	public class CoreBaseSocket:CoreBaseClass
	{

		protected System.Net.Sockets.Socket socket;
		protected System.Net.Sockets.UdpClient udpClient;
		public CoreBaseSocket():base()
		{
		}

		protected System.Net.Sockets.Socket UdpSocket ()
		{
			return new System.Net.Sockets.Socket (System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
		}

	}

}

