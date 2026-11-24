using System.Globalization;
using GotoCore.Logging;
using GotoCore.Messaging;
using GotoCore.Mvc;
using GotoCore.Storage;

namespace GotoCore.Samples.Counter
{
    /// <summary>
    /// Loads and saves the counter, and announces every 10th step. Plain C#: it can be unit tested
    /// with an in-memory or temp-folder <see cref="IFileStorage"/> and a <see cref="MessageBus"/>.
    /// </summary>
    public sealed class CounterController : ControllerBase
    {
        private const string SaveFile = "counter.txt";

        private readonly CounterModel _model;
        private readonly IFileStorage _storage;
        private readonly IPublisher _publisher;
        private readonly ILog _log;

        public CounterController(CounterModel model, IFileStorage storage, IPublisher publisher, ILog log)
        {
            _model = model;
            _storage = storage;
            _publisher = publisher;
            _log = log.ForCategory(nameof(CounterController));
        }

        protected override void OnInitialize()
        {
            if (_storage.TryReadText(SaveFile, out var text) &&
                int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var saved))
            {
                _model.Set(saved);
                _log.Info("Loaded counter: " + saved);
            }

            Track(_model.Count.Subscribe(OnCountChanged, invokeImmediately: false));
        }

        private void OnCountChanged(int value)
        {
            _storage.WriteText(SaveFile, value.ToString(CultureInfo.InvariantCulture));
            if (value > 0 && value % 10 == 0) _publisher.Publish(new MilestoneReached(value));
        }
    }
}
