using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GotoCore.Storage
{
    /// <summary>
    /// File access relative to a root folder (in Unity usually <c>Application.persistentDataPath</c>).
    /// Successor of the GotoCore 1.x CoreFilesystem services.
    /// </summary>
    public interface IFileStorage
    {
        string Root { get; }
        bool Exists(string relativePath);
        string ReadText(string relativePath);
        bool TryReadText(string relativePath, out string text);
        void WriteText(string relativePath, string text);
        void AppendText(string relativePath, string text);
        byte[] ReadBytes(string relativePath);
        void WriteBytes(string relativePath, byte[] bytes);
        bool Delete(string relativePath);
        void DeleteFolder(string relativePath);

        /// <summary>Relative paths of the files in <paramref name="folder"/> that match <paramref name="pattern"/>.</summary>
        IReadOnlyList<string> List(string folder = "", string pattern = "*", bool recursive = false);
    }

    public sealed class FileStorage : IFileStorage
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(false);

        public FileStorage(string root)
        {
            if (string.IsNullOrEmpty(root)) throw new ArgumentException("Root folder is required.", nameof(root));
            Root = Path.GetFullPath(root);
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public bool Exists(string relativePath) => File.Exists(Resolve(relativePath));

        public string ReadText(string relativePath) => File.ReadAllText(Resolve(relativePath), Utf8);

        public bool TryReadText(string relativePath, out string text)
        {
            var path = Resolve(relativePath);
            if (!File.Exists(path))
            {
                text = null;
                return false;
            }
            text = File.ReadAllText(path, Utf8);
            return true;
        }

        public void WriteText(string relativePath, string text)
        {
            var path = PrepareWrite(relativePath);
            // Write to a temp file first so a crash cannot leave a half-written save.
            var temp = path + ".tmp";
            File.WriteAllText(temp, text ?? string.Empty, Utf8);
            ReplaceFile(temp, path);
        }

        public void AppendText(string relativePath, string text)
        {
            File.AppendAllText(PrepareWrite(relativePath), text ?? string.Empty, Utf8);
        }

        public byte[] ReadBytes(string relativePath) => File.ReadAllBytes(Resolve(relativePath));

        public void WriteBytes(string relativePath, byte[] bytes)
        {
            var path = PrepareWrite(relativePath);
            var temp = path + ".tmp";
            File.WriteAllBytes(temp, bytes ?? Array.Empty<byte>());
            ReplaceFile(temp, path);
        }

        public bool Delete(string relativePath)
        {
            var path = Resolve(relativePath);
            if (!File.Exists(path)) return false;
            File.Delete(path);
            return true;
        }

        public void DeleteFolder(string relativePath)
        {
            var path = Resolve(relativePath);
            if (string.Equals(path, Root, StringComparison.Ordinal))
                throw new InvalidOperationException("Refusing to delete the storage root.");
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }

        public IReadOnlyList<string> List(string folder = "", string pattern = "*", bool recursive = false)
        {
            var path = Resolve(folder ?? string.Empty);
            if (!Directory.Exists(path)) return Array.Empty<string>();
            var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(path, string.IsNullOrEmpty(pattern) ? "*" : pattern, option);
            var result = new List<string>(files.Length);
            foreach (var file in files)
                result.Add(file.Substring(Root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Replace(Path.DirectorySeparatorChar, '/'));
            result.Sort(StringComparer.Ordinal);
            return result;
        }

        /// <summary>Absolute path for a relative one; paths escaping the root (e.g. "../x") are rejected.</summary>
        public string Resolve(string relativePath)
        {
            if (relativePath == null) throw new ArgumentNullException(nameof(relativePath));
            if (Path.IsPathRooted(relativePath))
                throw new ArgumentException("Use a path relative to the storage root.", nameof(relativePath));
            var full = Path.GetFullPath(Path.Combine(Root, relativePath));
            var rootWithSeparator = Root.EndsWith(Path.DirectorySeparatorChar.ToString()) ? Root : Root + Path.DirectorySeparatorChar;
            if (!string.Equals(full, Root, StringComparison.Ordinal) && !full.StartsWith(rootWithSeparator, StringComparison.Ordinal))
                throw new ArgumentException("Path escapes the storage root: " + relativePath, nameof(relativePath));
            return full;
        }

        private string PrepareWrite(string relativePath)
        {
            var path = Resolve(relativePath);
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            return path;
        }

        private static void ReplaceFile(string source, string destination)
        {
            if (File.Exists(destination)) File.Delete(destination);
            File.Move(source, destination);
        }
    }
}
