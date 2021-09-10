using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gelf.Extensions.Logging
{
    internal static class Debouncer
    {
        public static Action<T> Debounce<T>(Action<T> action, TimeSpan delay)
        {
            CancellationTokenSource? cts = null;

            return parameter =>
            {
                try
                {
                    cts?.Cancel();
                }
                catch (ObjectDisposedException)
                {
                }

                var newCts = cts = new CancellationTokenSource();
                Task.Delay(delay, newCts.Token)
                    .ContinueWith(_ => action(parameter), newCts.Token)
                    .ContinueWith(_ => newCts.Dispose(), newCts.Token);
            };
        }
    }
}
