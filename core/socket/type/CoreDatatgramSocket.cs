using System;
using Core.Base;
using Core.Base.Interfaces;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Collections.Generic;

namespace Core.Socket
{
	public class CoreDatatgramSocket:CoreBaseSocket,ISocket
	{

		/*CALLBACKS*/
		public static readonly string PACKAGE_RECEIVED = "core.datagram.socket.received";
		
		private int listeningPort;

		private int triggeringPort;

		private IPEndPoint clientEndPoint;
		private IPEndPoint serverEndPoint;

		private byte[] incomingBytes;
		private byte[] outgoingBytes;

		private IPAddress serverIP;

		private List<Thread> listeningThreads;


		public CoreDatatgramSocket () : base ()
		{
			this.listeningThreads = new List<Thread> ();
		}


		public void Bind (int port)
		{

			this.listeningPort = port;
			this.udpClient = new UdpClient (this.listeningPort);
			this.clientEndPoint = new IPEndPoint (IPAddress.Any, this.listeningPort);
			Thread thread = new Thread (new ThreadStart (this.StartListening));
			thread.Name = port.ToString ();
			thread.Start ();
			this.listeningThreads.Add (thread);
			while (!thread.IsAlive)
				;

		}

		public void UnBind (int port)
		{
			foreach (Thread thread in this.listeningThreads) {
				if (thread.Name == port.ToString ()) {
					thread.Abort ();
					this.listeningThreads.Remove (thread);
				}
			}
		}

		public void StartListening ()
		{
			Console.WriteLine ("Thread start");

			while (true) {

				try {
					Console.WriteLine ("Waiting for broadcast");

					incomingBytes = this.udpClient.Receive (ref this.clientEndPoint);
					this.serverIP = this.clientEndPoint.Address;
					String incoming = Encoding.ASCII.GetString (incomingBytes, 0, incomingBytes.Length);

					this.CreateCallBack (PACKAGE_RECEIVED)
						.AddParam (CoreSocket.PACKAGE, incoming)
						.Send ();
					

				} catch (ThreadInterruptedException) {
					break; // exit the while loop
				} catch (SocketException) { 
					break; // exit the while loop
				}
			}
			this.udpClient.Close ();
			this.CreateCallBack (CoreSocket.SOCKET_CLOSED).Send ();

		}



		public void Send (Dictionary<string, object> parameters)
		{
			this.socket = this.UdpSocket ();
			this.serverEndPoint = new IPEndPoint (this.serverIP, (int)parameters [CoreSocket.PORT]);
			this.outgoingBytes = Encoding.ASCII.GetBytes ((string)parameters [CoreSocket.MESSAGE]);
			this.socket.SendTo (this.outgoingBytes, this.serverEndPoint);
		}
	}
}

