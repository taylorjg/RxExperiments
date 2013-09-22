using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using NUnit.Framework;

namespace RxTests
{
    [TestFixture]
    internal class FromAsyncPattern
    {
        [Test]
        public void FromAsyncPatternWithMemoryStream()
        {
            var stream = new MemoryStream(new byte[] {1, 2, 3, 4, 5});
            var read = Observable.FromAsyncPattern<byte[], int, int, int>(stream.BeginRead, stream.EndRead);
            var buffer = new byte[10];
            var observable = read(buffer, 0, 10);
            var actual = 0;
            var cancellationTokenSource = new CancellationTokenSource();
            observable.Subscribe(
                numBytesRead =>
                    {
                        actual = numBytesRead;
                        cancellationTokenSource.Cancel();
                    },
                cancellationTokenSource.Token);
            cancellationTokenSource.Token.WaitHandle.WaitOne();
            Assert.That(actual, Is.EqualTo(5));
            Assert.That(buffer, Is.EqualTo(new byte[] {1, 2, 3, 4, 5, 0, 0, 0, 0, 0}));
        }
    }
}
