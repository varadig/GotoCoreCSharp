using System;
using Core.Logger;
using Core.Filesystem;

namespace Core.Logger
{
	public class CoreLoggerFile:CoreBaseLogger
	{
		public static readonly string PATH = "core.logger.file.path";
		private string path;
		public CoreLoggerFile ()
		{
			try {
				path = (string)this.Context.GetParam (PATH);	
			} catch (Exception ex) {
				Log (ex);
				path = "data.log";
			}


				
				
		}

		#region implemented abstract members of CoreBaseLogger

		protected override void AddLogEntry (string message)
		{
			this.Sc.GetService (CoreFilesystem.APPEND_TEXT_FILE)
				.AddParam (CoreFilesystem.PATH, path)
				.AddParam (CoreFilesystem.CONTENT, this.CreateEntryFrom (message))
				.Execute ();
		}

		#endregion
	}
}

