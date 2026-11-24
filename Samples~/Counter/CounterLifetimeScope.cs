using GotoCore.DI;
using GotoCore.Storage;
using GotoCore.Unity;
using VContainer;
using VContainer.Unity;

namespace GotoCore.Samples.Counter
{
    /// <summary>
    /// Composition root of the sample. Put it on a GameObject in the scene; the
    /// <see cref="CounterView"/> anywhere in the same scene gets injected automatically.
    /// </summary>
    public sealed class CounterLifetimeScope : GotoCoreLifetimeScope
    {
        protected override void ConfigureScope(IContainerBuilder builder)
        {
            builder.RegisterInstance<IFileStorage>(UnityStorage.Persistent("GotoCoreCounterSample"));

            // Model
            builder.Register<CounterModel>(Lifetime.Singleton);

            // Commands: message -> command
            builder.RegisterCommand<IncrementRequested, IncrementCommand>();
            builder.RegisterCommand<ResetRequested, ResetCommand>();

            // Controller
            builder.RegisterController<CounterController>();

            // View
            builder.RegisterComponentInHierarchy<CounterView>();
        }
    }
}
