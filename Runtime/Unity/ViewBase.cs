using System;
using UnityEngine;

namespace GotoCore.Unity
{
    /// <summary>
    /// Base class for views: MonoBehaviours that render models and turn user input into messages.
    /// Views should not contain game logic; they publish intents (e.g. <c>new BuyRequested(itemId)</c>)
    /// and bind to read-only model properties. Subscriptions passed to <see cref="Track{T}"/> are
    /// disposed when the GameObject is destroyed.
    /// </summary>
    public abstract class ViewBase : MonoBehaviour
    {
        private readonly DisposableBag _disposables = new DisposableBag();

        /// <summary>Disposes <paramref name="disposable"/> when this view is destroyed.</summary>
        protected T Track<T>(T disposable) where T : IDisposable => _disposables.Add(disposable);

        /// <summary>Disposes everything tracked so far (e.g. before rebinding to another model).</summary>
        protected void ClearTracked() => _disposables.Clear();

        protected virtual void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
