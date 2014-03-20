using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using NUnit.Framework;

namespace RxTests
{
    [TestFixture]
    internal class ObservableCreate
    {
        [Test]
        public void BasicUseOfObservableCreateWithASequenceOfThreeInts()
        {
            // Arrange
            var manualResetEvent = new ManualResetEventSlim(false);
            var sequence = new List<int>();
            var disposableDisposed = false;
            var observable = Observable.Create<int>(observer =>
                {
                    observer.OnNext(1);
                    observer.OnNext(2);
                    observer.OnNext(3);
                    observer.OnCompleted();
                    return Disposable.Create(() =>
                        {
                            disposableDisposed = true;
                            manualResetEvent.Set();
                        });
                });

            // Act
            observable.Subscribe(sequence.Add);
            manualResetEvent.Wait();

            // Assert
            Assert.That(sequence, Is.EqualTo(new[] {1, 2, 3}));
            Assert.That(disposableDisposed, Is.True);
        }

        [Test]
        public void ObservableCreateEmpty()
        {
            // Arrange
            var manualResetEvent = new ManualResetEventSlim(false);
            var sequence = new List<int>();
            var disposableDisposed = false;
            var observable = Observable.Create<int>(observer =>
            {
                observer.OnCompleted();
                return Disposable.Create(() =>
                {
                    disposableDisposed = true;
                    manualResetEvent.Set();
                });
            });

            // Act
            observable.Subscribe(sequence.Add);
            manualResetEvent.Wait();

            // Assert
            Assert.That(sequence, Is.Empty);
            Assert.That(disposableDisposed, Is.True);
        }

        [Test]
        public void ObservableCreateReturn()
        {
            // Arrange
            var manualResetEvent = new ManualResetEventSlim(false);
            var sequence = new List<int>();
            var disposableDisposed = false;
            var observable = Observable.Create<int>(observer =>
            {
                observer.OnNext(42);
                observer.OnCompleted();
                return Disposable.Create(() =>
                {
                    disposableDisposed = true;
                    manualResetEvent.Set();
                });
            });

            // Act
            observable.Subscribe(sequence.Add);
            manualResetEvent.Wait();

            // Assert
            Assert.That(sequence, Is.EqualTo(new[] {42}));
            Assert.That(disposableDisposed, Is.True);
        }

        [Test]
        public void ObservableCreateNever()
        {
            // Arrange
            var manualResetEvent = new ManualResetEventSlim(false);
            var sequence = new List<int>();
            var disposableDisposed = false;
            var observable = Observable.Create<int>(observer => Disposable.Create(() =>
                {
                    disposableDisposed = true;
                    manualResetEvent.Set();
                }));

            // Act
            observable.Subscribe(sequence.Add);
            manualResetEvent.Wait(millisecondsTimeout:1000);

            // Assert
            Assert.That(sequence, Is.Empty);
            Assert.That(disposableDisposed, Is.False);
        }

        [Test]
        public void ObservableCreateThrow()
        {
            // Arrange
            const string exceptionMessage = "My exception message";
            var manualResetEvent = new ManualResetEventSlim(false);
            var sequence = new List<int>();
            var disposableDisposed = false;
            Exception thrownException = null;
            var observable = Observable.Create<int>(observer =>
            {
                observer.OnError(new Exception(exceptionMessage));
                return Disposable.Create(() =>
                {
                    disposableDisposed = true;
                    manualResetEvent.Set();
                });
            });

            // Act
            observable.Subscribe(sequence.Add, ex =>
                {
                    thrownException = ex;
                });
            manualResetEvent.Wait();

            // Assert
            Assert.That(sequence, Is.Empty);
            Assert.That(disposableDisposed, Is.True);
            Assert.That(thrownException, Is.Not.Null);
            Assert.That(thrownException.Message, Is.EqualTo(exceptionMessage));
        }
    }
}
