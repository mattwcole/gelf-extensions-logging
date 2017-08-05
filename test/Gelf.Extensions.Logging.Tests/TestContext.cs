using System.Threading;

namespace Gelf.Extensions.Logging.Tests
{
    public static class TestContext
    {
        private static readonly AsyncLocal<string> LocalTestId = new AsyncLocal<string>();

        public static string TestId
        {
            get => LocalTestId.Value;
            set => LocalTestId.Value = value;
        }
    }
}
