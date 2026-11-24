using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace GotoCore.Messaging
{
    /// <summary>Sends messages. Inject this where something happens (views, services).</summary>
    public interface IPublisher
    {
        void Publish<TMessage>(TMessage message);
    }

    /// <summary>Receives messages. Inject this where something reacts (controllers, views).</summary>
    public interface ISubscriber
    {
        /// <summary>Subscribes to messages of exactly <typeparamref name="TMessage"/>. Dispose the result to unsubscribe.</summary>
        IDisposable Subscribe<TMessage>(Action<TMessage> handler);
    }

    public interface IMessageBus : IPublisher, ISubscriber
    {
    }

    /// <summary>
    /// Typed, synchronous publish/subscribe bus. This is the successor of the string-keyed
    /// CoreNotification / CoreListener pair of GotoCore 1.x: a message is a plain type
    /// (ideally a <c>readonly struct</c>) instead of a name plus a parameter dictionary.
    /// </summary>
    /// <remarks>
    /// Handlers run in subscription order on the publishing thread. Subscribing or unsubscribing
    /// from inside a handler is safe: it takes effect from the next publish (an unsubscribed handler
    /// is not called again even within the current publish). If handlers throw, every handler still
    /// runs and the exceptions are rethrown afterwards. Intended for Unity's main thread; it is not thread-safe.
    /// </remarks>
    public sealed class MessageBus : IMessageBus, IDisposable
    {
        private readonly Dictionary<Type, List<ISubscription>> _subscriptions = new Dictionary<Type, List<ISubscription>>();
        private bool _disposed;

        public void Publish<TMessage>(TMessage message)
        {
            ThrowIfDisposed();
            if (!_subscriptions.TryGetValue(typeof(TMessage), out var list) || list.Count == 0) return;

            var snapshot = list.ToArray();
            List<Exception> errors = null;
            foreach (var subscription in snapshot)
            {
                var typed = (Subscription<TMessage>)subscription;
                if (!typed.IsActive) continue;
                try
                {
                    typed.Handler(message);
                }
                catch (Exception e)
                {
                    (errors ??= new List<Exception>()).Add(e);
                }
            }

            if (errors == null) return;
            if (errors.Count == 1) ExceptionDispatchInfo.Capture(errors[0]).Throw();
            throw new AggregateException(errors);
        }

        public IDisposable Subscribe<TMessage>(Action<TMessage> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            ThrowIfDisposed();

            if (!_subscriptions.TryGetValue(typeof(TMessage), out var list))
            {
                list = new List<ISubscription>();
                _subscriptions.Add(typeof(TMessage), list);
            }

            var subscription = new Subscription<TMessage>(this, handler);
            list.Add(subscription);
            return subscription;
        }

        /// <summary>Number of active handlers for a message type (useful in tests and diagnostics).</summary>
        public int SubscriberCount<TMessage>()
        {
            return _subscriptions.TryGetValue(typeof(TMessage), out var list) ? list.Count : 0;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var list in _subscriptions.Values)
                foreach (var subscription in list)
                    subscription.Deactivate();
            _subscriptions.Clear();
        }

        private void Remove(Type type, ISubscription subscription)
        {
            if (_subscriptions.TryGetValue(type, out var list)) list.Remove(subscription);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MessageBus));
        }

        private interface ISubscription
        {
            void Deactivate();
        }

        private sealed class Subscription<TMessage> : ISubscription, IDisposable
        {
            private MessageBus _owner;

            public Subscription(MessageBus owner, Action<TMessage> handler)
            {
                _owner = owner;
                Handler = handler;
                IsActive = true;
            }

            public Action<TMessage> Handler { get; }
            public bool IsActive { get; private set; }

            public void Deactivate()
            {
                IsActive = false;
                _owner = null;
            }

            public void Dispose()
            {
                if (!IsActive) return;
                var owner = _owner;
                Deactivate();
                owner?.Remove(typeof(TMessage), this);
            }
        }
    }
}
