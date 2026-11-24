using GotoCore.Logging;
using GotoCore.Storage;
using UnityEngine;

namespace GotoCore.Unity
{
    /// <summary>Forwards log entries to the Unity console with the matching severity.</summary>
    public sealed class UnityLogSink : ILogSink
    {
        public void Write(in LogEntry entry)
        {
            var text = string.IsNullOrEmpty(entry.Category) ? entry.Message : "[" + entry.Category + "] " + entry.Message;
            switch (entry.Level)
            {
                case LogLevel.Warning:
                    Debug.LogWarning(text);
                    break;
                case LogLevel.Error:
                    Debug.LogError(text);
                    if (entry.Exception != null) Debug.LogException(entry.Exception);
                    break;
                default:
                    Debug.Log(text);
                    break;
            }
        }
    }

    /// <summary>Unity-specific factories.</summary>
    public static class UnityStorage
    {
        /// <summary>A <see cref="FileStorage"/> rooted at <c>Application.persistentDataPath</c> (optionally in a subfolder).</summary>
        public static FileStorage Persistent(string subfolder = null)
        {
            var root = string.IsNullOrEmpty(subfolder)
                ? Application.persistentDataPath
                : System.IO.Path.Combine(Application.persistentDataPath, subfolder);
            return new FileStorage(root);
        }
    }
}
