using System;
using System.Collections.Generic;

namespace GotoCore.Model
{
    /// <summary>Read-only view of an <see cref="ObservableProperty{T}"/>; expose this from models to views.</summary>
    public interface IReadOnlyObservableProperty<out T>
    {
        T Value { get; }

        /// <summary>
        /// Calls <paramref name="onChanged"/> whenever the value changes (and once right away when
        /// <paramref name="invokeImmediately"/> is true). Dispose the result to stop listening.
        /// </summary>
        IDisposable Subscribe(Action<T> onChanged, bool invokeImmediately = true);
    }

    /// <summary>A value that notifies subscribers when it changes. The building block of models.</summary>
    public sealed class ObservableProperty<T> : IReadOnlyObservableProperty<T>
    {
        private readonly IEqualityComparer<T> _comparer;
        private readonly List<Listener> _listeners = new List<Listener>();
        private T _value;

        public ObservableProperty(T initialValue = default, IEqualityComparer<T> comparer = null)
        {
            _value = initialValue;
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        /// <summary>Setting an equal value does nothing; a different value notifies every subscriber.</summary>
        public T Value
        {
            get => _value;
            set
            {
                if (_comparer.Equals(_value, value)) return;
                _value = value;
                Notify();
            }
        }

        /// <summary>Sets the value and notifies even if it did not change (e.g. after mutating a reference type in place).</summary>
        public void SetAndNotify(T value)
        {
            _value = value;
            Notify();
        }

        public IDisposable Subscribe(Action<T> onChanged, bool invokeImmediately = true)
        {
            if (onChanged == null) throw new ArgumentNullException(nameof(onChanged));
            var listener = new Listener(this, onChanged);
            _listeners.Add(listener);
            if (invokeImmediately) onChanged(_value);
            return listener;
        }

        public IReadOnlyObservableProperty<T> AsReadOnly() => this;

        public override string ToString() => _value?.ToString() ?? "null";

        private void Notify()
        {
            var value = _value;
            foreach (var listener in _listeners.ToArray())
                if (listener.IsActive)
                    listener.Callback(value);
        }

        private sealed class Listener : IDisposable
        {
            private ObservableProperty<T> _owner;

            public Listener(ObservableProperty<T> owner, Action<T> callback)
            {
                _owner = owner;
                Callback = callback;
            }

            public Action<T> Callback { get; }
            public bool IsActive => _owner != null;

            public void Dispose()
            {
                var owner = _owner;
                _owner = null;
                owner?._listeners.Remove(this);
            }
        }
    }
}
