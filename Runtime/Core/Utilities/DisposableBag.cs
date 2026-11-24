using System;
using System.Collections.Generic;

namespace GotoCore
{
    /// <summary>
    /// Collects <see cref="IDisposable"/> instances (subscriptions, handles) and disposes them together.
    /// Items added after the bag was disposed are disposed immediately.
    /// </summary>
    public sealed class DisposableBag : IDisposable
    {
        private readonly List<IDisposable> _items = new List<IDisposable>();
        private bool _disposed;

        public bool IsDisposed => _disposed;

        /// <summary>Adds an item and returns it, so it can be used inline.</summary>
        public T Add<T>(T item) where T : IDisposable
        {
            if (item == null) return item;
            if (_disposed)
            {
                item.Dispose();
                return item;
            }
            _items.Add(item);
            return item;
        }

        /// <summary>Disposes and forgets every item, but keeps the bag usable.</summary>
        public void Clear()
        {
            var snapshot = _items.ToArray();
            _items.Clear();
            DisposeAll(snapshot);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Clear();
        }

        private static void DisposeAll(IDisposable[] items)
        {
            List<Exception> errors = null;
            // Reverse order: later subscriptions usually depend on earlier ones.
            for (var i = items.Length - 1; i >= 0; i--)
            {
                try
                {
                    items[i].Dispose();
                }
                catch (Exception e)
                {
                    (errors ??= new List<Exception>()).Add(e);
                }
            }
            if (errors != null) throw new AggregateException(errors);
        }
    }

    /// <summary>An <see cref="IDisposable"/> that runs an action once.</summary>
    public sealed class ActionDisposable : IDisposable
    {
        private Action _action;

        public ActionDisposable(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            var action = _action;
            _action = null;
            action?.Invoke();
        }
    }
}
