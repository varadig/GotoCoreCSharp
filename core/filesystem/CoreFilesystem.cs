using System;
using Core.Base;
using System.IO;

namespace Core.Filesystem
{
	public class CoreFilesystem:CoreBaseClass
	{
		/*SERVICES*/
		/*READ*/
		public static readonly string READ_TEXT_FILE = "core.filesystem.read.text.file";
		public static readonly string READ_BINARY_FILE = "core.filesystem.read.binary.file";
		public static readonly string READ_XML_FILE = "core.filesystem.read.xml.file";

		/*CREATE*/
		public static readonly string CREATE_TEXT_FILE = "core.filesystem.create.text.file";
		public static readonly string CREATE_BINARY_FILE = "core.filesystem.create.binary.file";
		public static readonly string CREATE_FOLDER = "core.filesystem.create.folder";
		/*APPEND*/
		public static readonly string APPEND_TEXT_FILE = "core.filesystem.append.text.file";
		/*DELETE*/
		public static readonly string DELETE_FILE = "core.filesystem.delete.file";
		public static readonly string DELETE_FOLDER = "core.filesystem.delete.folder";

		/*PARAMETERS*/
		public static readonly string PATH = "core.filesystem.path";
		public static readonly string CONTENT = "core.filesystem.content";
		public static readonly string PATTERN = "core.filesystem.pattern";

		/*GET*/
		public static readonly string GET_FILES_NAME = "core.filesystem.get.files.name";
		public static readonly string GET_FILES_PATH = "core.filesystem.get.files.path";

		private static CoreFilesystem instance;

		public static CoreFilesystem GetInstance ()
		{
			if (instance == null)
				instance = new CoreFilesystem ();
			return instance;
		}

		public CoreFilesystem ()
		{
			/*READ*/
			this.Sc.RegisterService (READ_TEXT_FILE, this.ServiceReadTextFile);
			this.Sc.RegisterService (READ_BINARY_FILE, this.ServiceReadBinaryFile);
			/*CREATE*/
			this.Sc.RegisterService (CREATE_TEXT_FILE, this.ServiceWriteTextFile);
			this.Sc.RegisterService (CREATE_BINARY_FILE, this.ServiceWriteBinaryFile);
			this.Sc.RegisterService (CREATE_FOLDER, this.ServiceCreateFolder);
			/*APPEND*/
			this.Sc.RegisterService (APPEND_TEXT_FILE, this.ServiceAppendTextFile);
			/*DELETE*/
			this.Sc.RegisterService (DELETE_FILE, this.ServiceDeleteFile);
			this.Sc.RegisterService (DELETE_FOLDER, this.ServiceDeleteFolder);

			/*GET*/
			this.Sc.RegisterService (GET_FILES_PATH, this.ServiceGetFilesPath);
			this.Sc.RegisterService (GET_FILES_NAME, this.ServiceGetFilesName);
		}

		private object ServiceReadTextFile (System.Collections.Generic.Dictionary<string, object> parameters)
		{
			return File.ReadAllText ((string)parameters [PATH]);
		}

		private object ServiceReadBinaryFile (System.Collections.Generic.Dictionary<string, object> parameters)
		{
			return File.ReadAllBytes ((string)parameters [PATH]);

		}

		private object ServiceWriteTextFile (System.Collections.Generic.Dictionary<string, object> parameters)
		{
			File.WriteAllText ((string)parameters [PATH], (string)parameters [CONTENT]);
			return null;
		}

		private object ServiceWriteBinaryFile (System.Collections.Generic.Dictionary<string, object> parameters)
		{
			Log ("Create Binary File On Path:" + parameters [PATH]);
			FileInfo fileInfo = new FileInfo((string)parameters [PATH]);
			if (!fileInfo.Exists)
				Directory.CreateDirectory(fileInfo.Directory.FullName);
			
			File.WriteAllBytes ((string)parameters [PATH], (byte[])parameters [CONTENT]);
			return null;
		}

		private object ServiceCreateFolder (System.Collections.Generic.Dictionary<string, object> parameters)
		{
			Directory.CreateDirectory ((string)parameters [PATH]);
			return null;

		}

		private object ServiceAppendTextFile (System.Collections.Generic.Dictionary<string, object> parameters)
		{
			File.AppendAllText ((string)parameters [PATH], (string)parameters [CONTENT]);
			return null;

		}

		public object ServiceDeleteFile (System.Collections.Generic.Dictionary<string, object> parameters)
		{
			File.Delete ((string)parameters [PATH]);
			return null;
		}

		public object ServiceDeleteFolder (System.Collections.Generic.Dictionary<string, object> parameters)
		{
			Directory.Delete ((string)parameters [PATH]);
			return null;
		}

		private object ServiceGetFilesPath (System.Collections.Generic.Dictionary<string, object> parameters)
		{
			return Directory.GetFiles ((string)parameters [PATH], (string)parameters [PATTERN]);
		}

		private object ServiceGetFilesName (System.Collections.Generic.Dictionary<string, object> parameters)
		{
			string[] paths = (string[])this.Sc.GetService (GET_FILES_PATH)
				.AddParam (CoreFilesystem.PATH, (string)parameters [PATH])
				.AddParam (CoreFilesystem.PATTERN, (string)parameters [PATTERN])
				.Execute ();
			
			string[] names = new string[paths.Length];
			for (int i = 0; i < paths.Length; i++) {
				names [i] = Path.GetFileName (paths[i]);
			}
			return names;
		}

		public static string Documents {
			get {
				return Environment.GetFolderPath (Environment.SpecialFolder.CommonDocuments);
			}
		}

		public static string Desktop {
			get {
				return Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory);
			}
		}

		public static string ApplicationData {
			get {
				return Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
			}
		}

		public static string User {
			get {
				return Environment.GetFolderPath (Environment.SpecialFolder.UserProfile);
			}
		}

	}
}

