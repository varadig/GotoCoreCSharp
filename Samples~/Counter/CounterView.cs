using GotoCore.Messaging;
using GotoCore.Unity;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace GotoCore.Samples.Counter
{
    /// <summary>Renders the model and turns clicks into messages. No game logic lives here.</summary>
    public sealed class CounterView : ViewBase
    {
        [SerializeField] private Button incrementButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Text countLabel;
        [SerializeField] private Text messageLabel;

        private IPublisher _publisher;

        [Inject]
        public void Construct(IPublisher publisher, ISubscriber subscriber, CounterModel model)
        {
            _publisher = publisher;

            incrementButton.onClick.AddListener(OnIncrementClicked);
            resetButton.onClick.AddListener(OnResetClicked);
            Track(new ActionDisposable(() =>
            {
                if (incrementButton != null) incrementButton.onClick.RemoveListener(OnIncrementClicked);
                if (resetButton != null) resetButton.onClick.RemoveListener(OnResetClicked);
            }));

            Track(model.Count.Subscribe(value => countLabel.text = value.ToString()));
            Track(subscriber.Subscribe<MilestoneReached>(m => messageLabel.text = "Milestone: " + m.Value + "!"));
        }

        private void OnIncrementClicked() => _publisher.Publish(new IncrementRequested(1));

        private void OnResetClicked()
        {
            messageLabel.text = string.Empty;
            _publisher.Publish(new ResetRequested());
        }
    }
}
