using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using NUnit.Framework;

namespace RxTests
{
    [TestFixture]
    internal class ErrorHandlingTests
    {
        [Test]
        public void RetryWithColdObservable()
        {
            // Arrange
            var onErrorEvent = new ManualResetEventSlim(false);
            var onCompletedEvent = new ManualResetEventSlim(false);
            var sequence = new List<int>();
            var cold = CreateObservableThatThrowsDuringFirstSubscriptionButNotDuringSubsequentSubscriptions();

            // Act
            cold
                .Retry(2)
                .Subscribe(
                    sequence.Add,
                    _ => onErrorEvent.Set(),
                    onCompletedEvent.Set);
            var waitResultIndex = WaitHandle.WaitAny(new[] { onErrorEvent.WaitHandle, onCompletedEvent.WaitHandle });

            // Assert
            Assert.That(waitResultIndex, Is.EqualTo(1));
            Assert.That(sequence, Is.EqualTo(new[] { 1, 2, 3, 4, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }));
        }

        [Test]
        public void RetryWithHotRefCountObservable()
        {
            // Arrange
            var onErrorEvent = new ManualResetEventSlim(false);
            var onCompletedEvent = new ManualResetEventSlim(false);
            var sequence = new List<int>();
            var cold = CreateObservableThatThrowsDuringFirstSubscriptionButNotDuringSubsequentSubscriptions();
            var hot = cold.Publish().RefCount();

            // Act
            hot
                .Retry(2)
                .Subscribe(
                    sequence.Add,
                    _ => onErrorEvent.Set(),
                    onCompletedEvent.Set);
            var waitResultIndex = WaitHandle.WaitAny(new[] { onErrorEvent.WaitHandle, onCompletedEvent.WaitHandle });

            // Assert
            Assert.That(waitResultIndex, Is.EqualTo(0));
            Assert.That(sequence, Is.EqualTo(new[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void CatchWithHotRefCountObservableWhereCatchJustReturnsTheHotObservable()
        {
            // Arrange
            var onErrorEvent = new ManualResetEventSlim(false);
            var onCompletedEvent = new ManualResetEventSlim(false);
            var sequence = new List<int>();
            var cold = CreateObservableThatThrowsDuringFirstSubscriptionButNotDuringSubsequentSubscriptions();
            var hot = cold.Publish().RefCount();

            // Act
            hot
                .Catch<int, InvalidOperationException>(_ => hot)
                .Subscribe(
                    sequence.Add,
                    _ => onErrorEvent.Set(),
                    onCompletedEvent.Set);
            var waitResultIndex = WaitHandle.WaitAny(new[] { onErrorEvent.WaitHandle, onCompletedEvent.WaitHandle });

            // Assert
            Assert.That(waitResultIndex, Is.EqualTo(0));
            Assert.That(sequence, Is.EqualTo(new[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void CatchWithHotRefCountObservableWhereCatchRepublishes()
        {
            // Arrange
            var onErrorEvent = new ManualResetEventSlim(false);
            var onCompletedEvent = new ManualResetEventSlim(false);
            var sequence = new List<int>();
            var cold = CreateObservableThatThrowsDuringFirstSubscriptionButNotDuringSubsequentSubscriptions();
            var hot = cold.Publish().RefCount();

            // Act
            hot
                .Catch<int, InvalidOperationException>(_ => cold.Publish().RefCount())
                .Subscribe(
                    sequence.Add,
                    _ => onErrorEvent.Set(),
                    onCompletedEvent.Set);
            var waitResultIndex = WaitHandle.WaitAny(new[] {onErrorEvent.WaitHandle, onCompletedEvent.WaitHandle});

            // Assert
            Assert.That(waitResultIndex, Is.EqualTo(1));
            Assert.That(sequence, Is.EqualTo(new[] {1, 2, 3, 4, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10}));
        }

        private static bool _firstTimeThrough;

        private static IObservable<int> CreateObservableThatThrowsDuringFirstSubscriptionButNotDuringSubsequentSubscriptions()
        {
            _firstTimeThrough = true;

            return Observable.Create<int>(observer =>
                {
                    observer.OnNext(1);
                    observer.OnNext(2);
                    observer.OnNext(3);
                    observer.OnNext(4);

                    if (_firstTimeThrough)
                    {
                        _firstTimeThrough = false;
                        throw new InvalidOperationException("Something went wrong");
                    }

                    observer.OnNext(5);
                    observer.OnNext(6);
                    observer.OnNext(7);
                    observer.OnNext(8);
                    observer.OnNext(9);
                    observer.OnNext(10);

                    observer.OnCompleted();

                    return (() => { });
                });
        }
    }
}
