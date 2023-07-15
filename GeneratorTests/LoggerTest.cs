using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorTests
{
    public static partial class Log
    {
        [LoggerMessage(
             EventId = 0,
             Level = LogLevel.Critical,
             Message = "Could not open socket to `{hostName}`")]
        public static partial void CouldNotOpenSocket(ILogger logger, string hostName);
    }
    public class FakeLogger : ILogger
    {
        public StringBuilder Logged { get; set; } = new StringBuilder();
        public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
                Logged.Append(formatter(state, exception));
        }
    }

    public class LoggerTest
    {
        FakeLogger logger = new FakeLogger();

        [Fact]
        public void Test()
        {
            Log.CouldNotOpenSocket(logger, "localhost");
            Assert.Equal("Could not open socket to `localhost`", logger.Logged.ToString());
        }

    }
}
