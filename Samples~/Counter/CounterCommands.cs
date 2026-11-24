using GotoCore.Commands;
using GotoCore.Logging;

namespace GotoCore.Samples.Counter
{
    /// <summary>Created for every <see cref="IncrementRequested"/>; dependencies come from the container.</summary>
    public sealed class IncrementCommand : ICommand<IncrementRequested>
    {
        private readonly CounterModel _model;

        public IncrementCommand(CounterModel model)
        {
            _model = model;
        }

        public void Execute(IncrementRequested message)
        {
            _model.Set(_model.Count.Value + message.Amount);
        }
    }

    public sealed class ResetCommand : ICommand<ResetRequested>
    {
        private readonly CounterModel _model;
        private readonly ILog _log;

        public ResetCommand(CounterModel model, ILog log)
        {
            _model = model;
            _log = log.ForCategory(nameof(ResetCommand));
        }

        public void Execute(ResetRequested message)
        {
            _log.Info("Counter reset from " + _model.Count.Value);
            _model.Set(0);
        }
    }
}
