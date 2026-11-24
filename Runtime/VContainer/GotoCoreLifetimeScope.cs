using GotoCore.Unity;
using VContainer;
using VContainer.Unity;

namespace GotoCore.DI
{
    /// <summary>
    /// Root lifetime scope that registers the GotoCore services (message bus, app context, log with a
    /// Unity console sink) and then calls <see cref="ConfigureScope"/>. Use it for the project or scene root;
    /// use a plain VContainer <c>LifetimeScope</c> for child scopes so they share the root message bus.
    /// </summary>
    public abstract class GotoCoreLifetimeScope : LifetimeScope
    {
        protected sealed override void Configure(IContainerBuilder builder)
        {
            builder.RegisterGotoCore(ConfigureGotoCore);
            ConfigureScope(builder);
        }

        /// <summary>Override to change logging (sinks, minimum level).</summary>
        protected virtual void ConfigureGotoCore(GotoCoreOptions options)
        {
            options.AddLogSink(new UnityLogSink());
        }

        /// <summary>Register models, services, commands, controllers and views here.</summary>
        protected abstract void ConfigureScope(IContainerBuilder builder);
    }
}
