using System;
using GotoCore.Messaging;

namespace GotoCore.Mvc
{
    /// <summary>
    /// A controller reacts to messages and model changes and updates models / calls services.
    /// It is a plain C# object created by the container (or by hand), never a MonoBehaviour.
    /// </summary>
    public interface IController
    {
        /// <summary>Called once after construction, when all dependencies are available.</summary>
        void Initialize();
    }

    /// <summary>
    /// Convenience base class: subscriptions added with <see cref="Track{T}"/> or <see cref="Listen{TMessage}"/>
    /// are disposed together with the controller.
    /// </summary>
    public abstract class ControllerBase : IController, IDisposable
    {
        private readonly DisposableBag _disposables = new DisposableBag();
        private bool _initialized;

        protected bool IsDisposed => _disposables.IsDisposed;

        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            OnInitialize();
        }

        public void Dispose()
        {
            if (_disposables.IsDisposed) return;
            try
            {
                OnDispose();
            }
            finally
            {
                _disposables.Dispose();
            }
        }

        /// <summary>Set up subscriptions here.</summary>
        protected abstract void OnInitialize();

        protected virtual void OnDispose()
        {
        }

        /// <summary>Disposes <paramref name="disposable"/> together with this controller.</summary>
        protected T Track<T>(T disposable) where T : IDisposable => _disposables.Add(disposable);

        /// <summary>Subscribes to a message for the lifetime of this controller.</summary>
        protected void Listen<TMessage>(ISubscriber subscriber, Action<TMessage> handler)
        {
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
            Track(subscriber.Subscribe(handler));
        }
    }
}
