using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using NUnit.Framework;

namespace RxTests
{
    [TestFixture]
    internal class HotObservable
    {
        [Test]
        public void HotObservableTest()
        {
            var period = TimeSpan.FromMilliseconds(100);
            var coldObservable = Observable.Interval(period);
            var hotObservable = coldObservable.Publish();
            hotObservable.Connect();

            var values = new List<long>();

            {
                var cancellationTokenSource1 = new CancellationTokenSource();
                hotObservable.Subscribe(
                    v =>
                    {
                        values.Add(v);
                        if (values.Count == 5)
                        {
                            cancellationTokenSource1.Cancel();
                        }
                    },
                    cancellationTokenSource1.Token);
                cancellationTokenSource1.Token.WaitHandle.WaitOne();
            }

            Thread.Sleep(4 * period.Milliseconds + period.Milliseconds / 2);

            {
                var cancellationTokenSource2 = new CancellationTokenSource();
                hotObservable.Subscribe(
                    v =>
                    {
                        values.Add(v);
                        if (values.Count == 10)
                        {
                            cancellationTokenSource2.Cancel();
                        }
                    },
                    cancellationTokenSource2.Token);
                cancellationTokenSource2.Token.WaitHandle.WaitOne();
            }

            Assert.That(values.ToArray(), Is.EqualTo(new[] {0, 1, 2, 3, 4, 9, 10, 11, 12, 13}));
        }
    }
}
