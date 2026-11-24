using System;
using System.Collections.Generic;
using System.IO;
using GotoCore.Commands;
using GotoCore.Context;
using GotoCore.Logging;
using GotoCore.Messaging;
using GotoCore.Model;
using GotoCore.Mvc;
using GotoCore.Storage;
using NUnit.Framework;

namespace GotoCore.Tests
{
    public class MessageBusTests
    {
        private readonly struct Ping
        {
            public Ping(int value) => Value = value;
            public int Value { get; }
        }

        private readonly struct Pong
        {
        }

        [Test]
        public void Publish_ReachesSubscribersOfThatTypeOnly()
        {
            var bus = new MessageBus();
            var pings = new List<int>();
            var pongs = 0;
            bus.Subscribe<Ping>(p => pings.Add(p.Value));
            bus.Subscribe<Pong>(_ => pongs++);

            bus.Publish(new Ping(1));
            bus.Publish(new Ping(2));

            CollectionAssert.AreEqual(new[] { 1, 2 }, pings);
            Assert.AreEqual(0, pongs);
        }

        [Test]
        public void DisposingSubscription_StopsDelivery()
        {
            var bus = new MessageBus();
            var count = 0;
            var subscription = bus.Subscribe<Ping>(_ => count++);
            bus.Publish(new Ping());
            subscription.Dispose();
            subscription.Dispose(); // idempotent
            bus.Publish(new Ping());

            Assert.AreEqual(1, count);
            Assert.AreEqual(0, bus.SubscriberCount<Ping>());
        }

        [Test]
        public void UnsubscribingDuringPublish_SkipsTheRemovedHandler()
        {
            var bus = new MessageBus();
            var calls = new List<string>();
            IDisposable second = null;
            bus.Subscribe<Ping>(_ =>
            {
                calls.Add("first");
                second.Dispose();
            });
            second = bus.Subscribe<Ping>(_ => calls.Add("second"));

            bus.Publish(new Ping());

            CollectionAssert.AreEqual(new[] { "first" }, calls);
        }

        [Test]
        public void SubscribingDuringPublish_TakesEffectFromNextPublish()
        {
            var bus = new MessageBus();
            var late = 0;
            bus.Subscribe<Ping>(_ => bus.Subscribe<Ping>(__ => late++));

            bus.Publish(new Ping());
            Assert.AreEqual(0, late);
            bus.Publish(new Ping());
            Assert.AreEqual(1, late);
        }

        [Test]
        public void ThrowingHandler_DoesNotStopOtherHandlers()
        {
            var bus = new MessageBus();
            var reached = false;
            bus.Subscribe<Ping>(_ => throw new InvalidOperationException("boom"));
            bus.Subscribe<Ping>(_ => reached = true);

            Assert.Throws<InvalidOperationException>(() => bus.Publish(new Ping()));
            Assert.IsTrue(reached);
        }

        [Test]
        public void DisposedBus_RejectsUse()
        {
            var bus = new MessageBus();
            bus.Dispose();
            Assert.Throws<ObjectDisposedException>(() => bus.Publish(new Ping()));
        }
    }

    public class CommandMapTests
    {
        private readonly struct Add
        {
            public Add(int amount) => Amount = amount;
            public int Amount { get; }
        }

        private sealed class Total
        {
            public int Value;
        }

        private sealed class AddCommand : ICommand<Add>
        {
            private readonly Total _total;
            public AddCommand(Total total) => _total = total;
            public void Execute(Add message) => _total.Value += message.Amount;
        }

        [Test]
        public void MappedCommand_RunsForEveryMessage_UntilDisposed()
        {
            var bus = new MessageBus();
            var total = new Total();
            var created = 0;
            var map = new CommandMap(bus, type =>
            {
                created++;
                return new AddCommand(total);
            }).Map<Add, AddCommand>();

            bus.Publish(new Add(2));
            bus.Publish(new Add(3));
            map.Dispose();
            bus.Publish(new Add(100));

            Assert.AreEqual(5, total.Value);
            Assert.AreEqual(2, created, "a new command instance per message");
        }
    }

    public class ObservablePropertyTests
    {
        [Test]
        public void Subscribe_InvokesImmediately_ThenOnChangesOnly()
        {
            var property = new ObservableProperty<int>(1);
            var seen = new List<int>();
            property.Subscribe(seen.Add);

            property.Value = 1; // same value: no notification
            property.Value = 2;

            CollectionAssert.AreEqual(new[] { 1, 2 }, seen);
        }

        [Test]
        public void DisposedListener_IsNotCalled()
        {
            var property = new ObservableProperty<string>("a");
            var calls = 0;
            var subscription = property.Subscribe(_ => calls++, invokeImmediately: false);
            subscription.Dispose();
            property.Value = "b";
            Assert.AreEqual(0, calls);
        }

        [Test]
        public void SetAndNotify_NotifiesEvenWithoutChange()
        {
            var property = new ObservableProperty<int>(5);
            var calls = 0;
            property.Subscribe(_ => calls++, invokeImmediately: false);
            property.SetAndNotify(5);
            Assert.AreEqual(1, calls);
        }
    }

    public class ApplicationContextTests
    {
        private static readonly ContextKey<string> Url = new ContextKey<string>("server.url");
        private static readonly ContextKey<int> Retries = new ContextKey<int>("server.retries");

        [Test]
        public void TypedValues_RoundTrip()
        {
            var context = new ApplicationContext();
            context.Set(Url, "https://example.com");
            context.Set(Retries, 3);

            Assert.AreEqual("https://example.com", context.Get(Url));
            Assert.AreEqual(3, context.Get(Retries));
            Assert.IsTrue(context.Has(Retries));
        }

        [Test]
        public void MissingValue_ThrowsOnGet_AndDefaultsOnGetOrDefault()
        {
            var context = new ApplicationContext();
            Assert.Throws<KeyNotFoundException>(() => context.Get(Retries));
            Assert.AreEqual(7, context.GetOrDefault(Retries, 7));
            Assert.IsFalse(context.TryGet(Url, out _));
        }
    }

    public class LogTests
    {
        [Test]
        public void EntriesBelowMinimumLevel_AreDropped_AndCategoryIsPrefixed()
        {
            var sink = new MemoryLogSink();
            var log = new Log(new ILogSink[] { sink }, LogLevel.Info);

            log.Debug("hidden");
            log.ForCategory("Shop").Warning("low stock");

            Assert.AreEqual(1, sink.Entries.Count);
            Assert.AreEqual("Shop", sink.Entries[0].Category);
            Assert.AreEqual(LogLevel.Warning, sink.Entries[0].Level);
        }

        [Test]
        public void FailingSink_DoesNotThrow()
        {
            var log = new Log(new ILogSink[] { new ThrowingSink() });
            Assert.DoesNotThrow(() => log.Error("x", new Exception("inner")));
        }

        private sealed class ThrowingSink : ILogSink
        {
            public void Write(in LogEntry entry) => throw new IOException("disk full");
        }
    }

    public class FileStorageTests
    {
        private string _root;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "gotocore-tests-" + Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        [Test]
        public void WriteReadListDelete()
        {
            var storage = new FileStorage(_root);
            storage.WriteText("saves/slot1.json", "{\"a\":1}");
            storage.AppendText("logs/game.log", "line1\n");
            storage.AppendText("logs/game.log", "line2\n");

            Assert.AreEqual("{\"a\":1}", storage.ReadText("saves/slot1.json"));
            Assert.AreEqual("line1\nline2\n", storage.ReadText("logs/game.log"));
            CollectionAssert.AreEqual(new[] { "saves/slot1.json" }, storage.List("saves"));
            Assert.IsTrue(storage.Delete("saves/slot1.json"));
            Assert.IsFalse(storage.Exists("saves/slot1.json"));
        }

        [Test]
        public void PathsOutsideRoot_AreRejected()
        {
            var storage = new FileStorage(_root);
            Assert.Throws<ArgumentException>(() => storage.ReadText("../secret.txt"));
            Assert.Throws<ArgumentException>(() => storage.WriteText(Path.GetTempPath(), "x"));
        }
    }

    public class ControllerBaseTests
    {
        private readonly struct Tick
        {
        }

        private sealed class TickController : ControllerBase
        {
            private readonly ISubscriber _subscriber;
            public int Ticks;
            public int InitializeCalls;

            public TickController(ISubscriber subscriber) => _subscriber = subscriber;

            protected override void OnInitialize()
            {
                InitializeCalls++;
                Listen<Tick>(_subscriber, _ => Ticks++);
            }
        }

        [Test]
        public void Initialize_RunsOnce_AndDisposeRemovesSubscriptions()
        {
            var bus = new MessageBus();
            var controller = new TickController(bus);
            controller.Initialize();
            controller.Initialize();

            bus.Publish(new Tick());
            controller.Dispose();
            bus.Publish(new Tick());

            Assert.AreEqual(1, controller.InitializeCalls);
            Assert.AreEqual(1, controller.Ticks);
            Assert.AreEqual(0, bus.SubscriberCount<Tick>());
        }
    }
}
