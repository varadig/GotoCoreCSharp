using System;
using System.Collections.Generic;
using GotoCore.Messaging;

namespace GotoCore.Commands
{
    /// <summary>
    /// A unit of work triggered by a message. Commands are short-lived: a new instance is created
    /// for every message, so keep state in models or services, not in the command.
    /// </summary>
    public interface ICommand<in TMessage>
    {
        void Execute(TMessage message);
    }

    /// <summary>
    /// Maps message types to commands, the typed successor of GotoCore 1.x named services
    /// (<c>Sc.GetService("name").AddParam(...).Execute()</c>). Without a DI container, pass a factory
    /// that creates commands; with VContainer use <c>builder.RegisterCommand&lt;TMessage, TCommand&gt;()</c>.
    /// </summary>
    public sealed class CommandMap : IDisposable
    {
        private readonly ISubscriber _subscriber;
        private readonly Func<Type, object> _factory;
        private readonly DisposableBag _bindings = new DisposableBag();

        /// <param name="subscriber">The bus the commands listen on.</param>
        /// <param name="factory">Creates a command instance from its type (e.g. <c>type =&gt; container.Resolve(type)</c>).</param>
        public CommandMap(ISubscriber subscriber, Func<Type, object> factory)
        {
            _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>Runs a new <typeparamref name="TCommand"/> for every <typeparamref name="TMessage"/>.</summary>
        public CommandMap Map<TMessage, TCommand>() where TCommand : ICommand<TMessage>
        {
            _bindings.Add(_subscriber.Subscribe<TMessage>(message => ((TCommand)_factory(typeof(TCommand))).Execute(message)));
            return this;
        }

        /// <summary>Runs the command created by <paramref name="create"/> for every <typeparamref name="TMessage"/>.</summary>
        public CommandMap Map<TMessage>(Func<ICommand<TMessage>> create)
        {
            if (create == null) throw new ArgumentNullException(nameof(create));
            _bindings.Add(_subscriber.Subscribe<TMessage>(message => create().Execute(message)));
            return this;
        }

        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}
