using System;
using System.Collections.Generic;
using System.Threading;

namespace Gelf.Extensions.Logging
{
    public class GelfLogScope
    {
        private static readonly AsyncLocal<GelfLogScope?> Value = new AsyncLocal<GelfLogScope?>();

        private GelfLogScope(IEnumerable<KeyValuePair<string, object>> additionalFields)
        {
            AdditionalFields = additionalFields;
        }

        public GelfLogScope? Parent { get; private set; }

        public IEnumerable<KeyValuePair<string, object>> AdditionalFields { get; }

        public static GelfLogScope? Current
        {
            get => Value.Value;
            set => Value.Value = value;
        }

        public static IDisposable Push(IEnumerable<KeyValuePair<string, object>> additionalFields)
        {
            var parent = Current;
            Current = new GelfLogScope(additionalFields) {Parent = parent};

            return new DisposableScope();
        }

        private class DisposableScope : IDisposable
        {
            public void Dispose()
            {
                Current = Current?.Parent;
            }
        }
    }
}
