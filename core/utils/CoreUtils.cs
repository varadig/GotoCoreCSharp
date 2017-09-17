using System;
using System.Net;

namespace Core.Utils
{
	public class CoreUtils
	{
		public static string TimeStamp ()
		{
			return Convert.ToString (DateTime.Now);
		}

		public static string HostName {
			get {
				return Dns.GetHostName ();
			}
		}

		public static bool IsMAcOSX {
			get { 
				OperatingSystem os = Environment.OSVersion;
				PlatformID pid = os.Platform;
				return pid == PlatformID.MacOSX;
			}
		}

		public static bool IsWindows {
			get { 
				OperatingSystem os = Environment.OSVersion;
				PlatformID pid = os.Platform;
				switch (pid) {
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
					return true;
					break;
				default:
					return false;
					break;
				}


			}
		}

		public static bool IsLinux {
			get { 
				OperatingSystem os = Environment.OSVersion;
				PlatformID pid = os.Platform;
				return pid == PlatformID.Unix;
			}
		}

	}
}

