using System;
using System.Collections.Generic;
using System.Threading;

namespace Gelf.Extensions.Logging
{
    public class GelfLogScope
    {
        internal GelfLogScope(IEnumerable<KeyValuePair<string,string>> additionalFields)
        {
            AdditionalFields = additionalFields;
        }

        public GelfLogScope Parent { get; private set; }

        public IEnumerable<KeyValuePair<string,string>> AdditionalFields { get; }

        private static readonly AsyncLocal<GelfLogScope> Value = new AsyncLocal<GelfLogScope>();

        public static GelfLogScope Current
        {
            get => Value.Value;
            set => Value.Value = value;
        }

        public static IDisposable Push(IEnumerable<KeyValuePair<string,string>> additionalFields)
        {
            var parent = Current;
            Current = new GelfLogScope(additionalFields) {Parent = parent};

            return new DisposableScope();
        }

        private class DisposableScope : IDisposable
        {
            public void Dispose()
            {
                Current = Current.Parent;
            }
        }
    }
}
