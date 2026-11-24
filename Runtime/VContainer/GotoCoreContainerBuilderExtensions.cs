using System;
using System.Collections.Generic;
using GotoCore.Commands;
using GotoCore.Context;
using GotoCore.Logging;
using GotoCore.Messaging;
using GotoCore.Mvc;
using VContainer;
using VContainer.Unity;

namespace GotoCore.DI
{
    /// <summary>Options for <see cref="GotoCoreContainerBuilderExtensions.RegisterGotoCore"/>.</summary>
    public sealed class GotoCoreOptions
    {
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Debug;
        public List<ILogSink> LogSinks { get; } = new List<ILogSink>();

        public GotoCoreOptions AddLogSink(ILogSink sink)
        {
            if (sink != null) LogSinks.Add(sink);
            return this;
        }
    }

    /// <summary>VContainer registration helpers for GotoCore.</summary>
    public static class GotoCoreContainerBuilderExtensions
    {
        /// <summary>
        /// Registers the GotoCore services as singletons: <see cref="IMessageBus"/> (also as
        /// <see cref="IPublisher"/> and <see cref="ISubscriber"/>), <see cref="IAppContext"/> and <see cref="ILog"/>.
        /// Call it once, in the root (project or scene) lifetime scope; child scopes inherit these.
        /// </summary>
        public static IContainerBuilder RegisterGotoCore(this IContainerBuilder builder, Action<GotoCoreOptions> configure = null)
        {
            var options = new GotoCoreOptions();
            configure?.Invoke(options);
            var sinks = options.LogSinks.ToArray();
            var level = options.MinimumLogLevel;

            builder.Register<MessageBus>(Lifetime.Singleton)
                .As<IMessageBus>()
                .As<IPublisher>()
                .As<ISubscriber>()
                .AsSelf();
            builder.Register<ApplicationContext>(Lifetime.Singleton).As<IAppContext>();
            builder.Register<ILog>(_ => new Log(sinks, level), Lifetime.Singleton);
            return builder;
        }

        /// <summary>
        /// Runs a new <typeparamref name="TCommand"/> (resolved from this scope, so it can take any
        /// registered dependency) for every published <typeparamref name="TMessage"/> while the scope lives.
        /// Use one command class per message type.
        /// </summary>
        public static IContainerBuilder RegisterCommand<TMessage, TCommand>(this IContainerBuilder builder)
            where TCommand : class, ICommand<TMessage>
        {
            builder.Register<TCommand>(Lifetime.Transient);
            builder.RegisterEntryPoint<CommandBinding<TMessage, TCommand>>();
            return builder;
        }

        /// <summary>
        /// Registers a controller as a singleton of this scope and calls <see cref="IController.Initialize"/>
        /// right after the container is built (before the first Unity Start). Controllers that implement
        /// <see cref="IDisposable"/> (e.g. <see cref="ControllerBase"/>) are disposed with the scope.
        /// </summary>
        public static RegistrationBuilder RegisterController<TController>(this IContainerBuilder builder)
            where TController : class, IController
        {
            builder.RegisterEntryPoint<ControllerInitializer<TController>>();
            return builder.Register<TController>(Lifetime.Singleton);
        }
    }

    /// <summary>Subscribes a command to its message for the lifetime of the scope.</summary>
    public sealed class CommandBinding<TMessage, TCommand> : IInitializable, IDisposable
        where TCommand : class, ICommand<TMessage>
    {
        private readonly ISubscriber _subscriber;
        private readonly IObjectResolver _resolver;
        private IDisposable _subscription;

        [UnityEngine.Scripting.Preserve]
        public CommandBinding(ISubscriber subscriber, IObjectResolver resolver)
        {
            _subscriber = subscriber;
            _resolver = resolver;
        }

        public void Initialize()
        {
            if (_subscription != null) return;
            _subscription = _subscriber.Subscribe<TMessage>(message => _resolver.Resolve<TCommand>().Execute(message));
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }

    /// <summary>Calls <see cref="IController.Initialize"/> on a registered controller.</summary>
    public sealed class ControllerInitializer<TController> : IInitializable where TController : class, IController
    {
        private readonly TController _controller;

        [UnityEngine.Scripting.Preserve]
        public ControllerInitializer(TController controller)
        {
            _controller = controller;
        }

        public void Initialize() => _controller.Initialize();
    }
}
