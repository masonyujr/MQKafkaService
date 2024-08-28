using Serilog;

namespace YourProject.Utils
{
    public static class LoggingSetup
    {
        public static ILogger ConfigureLogging()
        {
            return new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
    }
}
