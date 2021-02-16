using System.Threading;

namespace Gelf.Extensions.Logging.Tests.Fixtures
{
    public static class TestContext
    {
        private static readonly AsyncLocal<string> LocalTestId = new();

        public static string TestId
        {
            get => LocalTestId.Value;
            set => LocalTestId.Value = value;
        }
    }
}
