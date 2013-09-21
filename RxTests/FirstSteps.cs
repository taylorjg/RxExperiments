using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using NUnit.Framework;

namespace RxTests
{
    [TestFixture]
    internal class FirstSteps
    {
        [Test]
        public void SimpleTestUsingObservableRange()
        {
            var observable = Observable.Range(1, 5);
            var values = new List<int>();
            observable.Subscribe(values.Add);
            Assert.That(values, Has.Count.EqualTo(5));
        }

        [Test]
        public void SimpleTestUsingObservableTimer()
        {
            var dueTime = TimeSpan.FromMilliseconds(100);
            var period = TimeSpan.FromMilliseconds(100);
            var observable = Observable.Timer(dueTime, period).Timestamp();

            var cancellationTokenSource = new CancellationTokenSource();
            var values = new List<string>();

            observable.Subscribe(
                x =>
                    {
                        values.Add(x.ToString());
                        if (values.Count == 5)
                        {
                            cancellationTokenSource.Cancel();
                        }
                    },
                cancellationTokenSource.Token);

            cancellationTokenSource.Token.WaitHandle.WaitOne();

            Assert.That(values, Has.Count.EqualTo(5));
        }
    }
}
