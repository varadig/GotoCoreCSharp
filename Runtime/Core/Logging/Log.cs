using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GotoCore.Logging
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
    }

    public readonly struct LogEntry
    {
        public LogEntry(DateTime timestamp, LogLevel level, string category, string message, Exception exception)
        {
            Timestamp = timestamp;
            Level = level;
            Category = category;
            Message = message;
            Exception = exception;
        }

        public DateTime Timestamp { get; }
        public LogLevel Level { get; }
        public string Category { get; }
        public string Message { get; }
        public Exception Exception { get; }

        /// <summary>One-line text form: <c>2026-11-24 10:15:00.123 [Info] Category: message</c>.</summary>
        public string Format()
        {
            var sb = new StringBuilder(64);
            sb.Append(Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append(" [").Append(Level).Append("] ");
            if (!string.IsNullOrEmpty(Category)) sb.Append(Category).Append(": ");
            sb.Append(Message);
            if (Exception != null) sb.Append(Environment.NewLine).Append(Exception);
            return sb.ToString();
        }
    }

    /// <summary>Destination of log entries (Unity console, file, remote endpoint, test buffer…).</summary>
    public interface ILogSink
    {
        void Write(in LogEntry entry);
    }

    public interface ILog
    {
        LogLevel MinimumLevel { get; }
        bool IsEnabled(LogLevel level);
        void Write(LogLevel level, string message, Exception exception = null);
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception exception = null);

        /// <summary>Returns a logger that prefixes entries with <paramref name="category"/> (e.g. the class name).</summary>
        ILog ForCategory(string category);
    }

    /// <summary>
    /// Logger that fans entries out to sinks. Successor of the GotoCore 1.x CoreLogger and its
    /// Debug / File / Server loggers, which are now sinks. A failing sink never breaks the caller.
    /// Disposing the log disposes the sinks that implement <see cref="IDisposable"/>.
    /// </summary>
    public sealed class Log : ILog, IDisposable
    {
        private readonly ILogSink[] _sinks;
        private readonly string _category;
        private readonly bool _ownsSinks;

        public Log(IEnumerable<ILogSink> sinks, LogLevel minimumLevel = LogLevel.Debug)
            : this(ToArray(sinks), minimumLevel, null, true)
        {
        }

        private Log(ILogSink[] sinks, LogLevel minimumLevel, string category, bool ownsSinks)
        {
            _sinks = sinks;
            MinimumLevel = minimumLevel;
            _category = category;
            _ownsSinks = ownsSinks;
        }

        public LogLevel MinimumLevel { get; }

        public bool IsEnabled(LogLevel level) => level >= MinimumLevel && _sinks.Length > 0;

        public void Write(LogLevel level, string message, Exception exception = null)
        {
            if (!IsEnabled(level)) return;
            var entry = new LogEntry(DateTime.Now, level, _category, message, exception);
            foreach (var sink in _sinks)
            {
                try
                {
                    sink.Write(in entry);
                }
                catch
                {
                    // Logging must never crash the game.
                }
            }
        }

        public void Debug(string message) => Write(LogLevel.Debug, message);
        public void Info(string message) => Write(LogLevel.Info, message);
        public void Warning(string message) => Write(LogLevel.Warning, message);
        public void Error(string message, Exception exception = null) => Write(LogLevel.Error, message, exception);

        public ILog ForCategory(string category)
        {
            var full = string.IsNullOrEmpty(_category) ? category : _category + "." + category;
            return new Log(_sinks, MinimumLevel, full, false);
        }

        public void Dispose()
        {
            if (!_ownsSinks) return;
            foreach (var sink in _sinks)
                (sink as IDisposable)?.Dispose();
        }

        private static ILogSink[] ToArray(IEnumerable<ILogSink> sinks)
        {
            if (sinks == null) return Array.Empty<ILogSink>();
            var list = new List<ILogSink>();
            foreach (var sink in sinks)
                if (sink != null)
                    list.Add(sink);
            return list.ToArray();
        }
    }

    /// <summary>Writes to <see cref="Console"/>; handy outside Unity (tests, tools).</summary>
    public sealed class ConsoleLogSink : ILogSink
    {
        public void Write(in LogEntry entry) => Console.WriteLine(entry.Format());
    }

    /// <summary>Keeps entries in memory; useful in tests and for an in-game debug console.</summary>
    public sealed class MemoryLogSink : ILogSink
    {
        private readonly List<LogEntry> _entries = new List<LogEntry>();
        private readonly int _capacity;

        public MemoryLogSink(int capacity = 500)
        {
            _capacity = Math.Max(1, capacity);
        }

        public IReadOnlyList<LogEntry> Entries => _entries;

        public void Write(in LogEntry entry)
        {
            if (_entries.Count >= _capacity) _entries.RemoveAt(0);
            _entries.Add(entry);
        }

        public void Clear() => _entries.Clear();
    }

    /// <summary>Appends formatted entries to a text file (created with its directory if missing).</summary>
    public sealed class FileLogSink : ILogSink, IDisposable
    {
        private readonly object _gate = new object();
        private StreamWriter _writer;

        public FileLogSink(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Path is required.", nameof(path));
            var directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            _writer = new StreamWriter(path, true, new UTF8Encoding(false)) { AutoFlush = true };
            FilePath = path;
        }

        public string FilePath { get; }

        public void Write(in LogEntry entry)
        {
            lock (_gate)
            {
                _writer?.WriteLine(entry.Format());
            }
        }

        public void Dispose()
        {
            lock (_gate)
            {
                _writer?.Dispose();
                _writer = null;
            }
        }
    }
}
