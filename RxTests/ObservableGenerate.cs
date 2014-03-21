using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using NUnit.Framework;

namespace RxTests
{
    [TestFixture]
    internal class ObservableGenerate
    {
        [Test]
        public void ObservableRangeImplementedUsingObservableGenerate()
        {
            // Arrange
            var manualResetEvent = new ManualResetEventSlim(false);
            var sequence = new List<int>();
            var observable = MakeObservableRangeOfInt(10, 15);

            // Act
            observable.Subscribe(sequence.Add, _ => manualResetEvent.Set(), manualResetEvent.Set);
            manualResetEvent.Wait();

            // Assert
            Assert.That(sequence, Is.EqualTo(new[] {10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24}));
        }

        private static IObservable<int> MakeObservableRangeOfInt(int start, int count)
        {
            return ObservableGenerateWithIdentityResultSelector(
                start,
                i => i < start + count,
                i => i + 1);
        }

        private static IObservable<T> ObservableGenerateWithIdentityResultSelector<T>(T initialState, Func<T, bool> condition, Func<T, T> iterate)
        {
            return Observable.Generate(initialState, condition, iterate, x => x);
        }
    }
}
