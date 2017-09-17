using System;
using System.Net;
using System.Text;
using System.IO;

namespace Core.Logger
{
	public class CoreLoggerServer : CoreBaseLogger
	{
		public static readonly string URL = "server.log.url";
		public static readonly string URLS = "server.log.urls";
		public static readonly string FILENAME = "server.log.filename";
		private WebRequest Request;

		public CoreLoggerServer ()
		{
			this.Init ();
		}

		override protected void AddLogEntry (string message)
		{
			Console.WriteLine ("Server Add Log");
			Console.WriteLine ("..............");
			try {
			
				string filename = (string)this.Context.GetParam (FILENAME);
				byte[] byteArray = Encoding.UTF8.GetBytes ("log=" + message + "&filename=" + filename);

				this.Request.Method = "POST";
				this.Request.ContentType = "application/x-www-form-urlencoded";
				this.Request.ContentLength = byteArray.Length;

				Stream dataStream = this.Request.GetRequestStream ();
				dataStream.Write (byteArray, 0, byteArray.Length);
				dataStream.Close ();
			} catch (Exception e) {
				Console.WriteLine (e.ToString ());
			}
			

		}

		void Init ()
		{
			this.Request = WebRequest.Create ((string)this.Context.GetParam (URL));
		}
	}
}

