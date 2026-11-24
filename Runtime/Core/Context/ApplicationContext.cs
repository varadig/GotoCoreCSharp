using System;
using System.Collections.Generic;

namespace GotoCore.Context
{
    /// <summary>
    /// Typed key for <see cref="IAppContext"/>. Declare keys once, e.g.
    /// <c>public static readonly ContextKey&lt;string&gt; ServerUrl = new ContextKey&lt;string&gt;("server.url");</c>
    /// </summary>
    public sealed class ContextKey<T>
    {
        public ContextKey(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Key name is required.", nameof(name));
            Name = name;
        }

        public string Name { get; }

        public override string ToString() => Name + " (" + typeof(T).Name + ")";
    }

    /// <summary>
    /// Application-wide settings and runtime values (build flavour, server URLs, feature flags).
    /// Successor of the GotoCore 1.x <c>CoreContext</c> singleton: typed keys instead of casts,
    /// and injected instead of reached through a static instance.
    /// </summary>
    public interface IAppContext
    {
        void Set<T>(ContextKey<T> key, T value);
        T Get<T>(ContextKey<T> key);
        T GetOrDefault<T>(ContextKey<T> key, T defaultValue = default);
        bool TryGet<T>(ContextKey<T> key, out T value);
        bool Has<T>(ContextKey<T> key);
        bool Remove<T>(ContextKey<T> key);
    }

    public sealed class ApplicationContext : IAppContext
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public void Set<T>(ContextKey<T> key, T value)
        {
            _values[Check(key).Name] = value;
        }

        public T Get<T>(ContextKey<T> key)
        {
            if (TryGet(key, out var value)) return value;
            throw new KeyNotFoundException("No value in the app context for " + key + ".");
        }

        public T GetOrDefault<T>(ContextKey<T> key, T defaultValue = default)
        {
            return TryGet(key, out var value) ? value : defaultValue;
        }

        public bool TryGet<T>(ContextKey<T> key, out T value)
        {
            if (_values.TryGetValue(Check(key).Name, out var raw) && raw is T typed)
            {
                value = typed;
                return true;
            }
            if (raw == null && _values.ContainsKey(key.Name) && default(T) == null)
            {
                value = default;
                return true;
            }
            value = default;
            return false;
        }

        public bool Has<T>(ContextKey<T> key) => TryGet(key, out _);

        public bool Remove<T>(ContextKey<T> key) => _values.Remove(Check(key).Name);

        private static ContextKey<T> Check<T>(ContextKey<T> key)
        {
            return key ?? throw new ArgumentNullException(nameof(key));
        }
    }
}
