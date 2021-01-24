using System.Linq;

namespace Spigot.Demo
{
    public static class LoggingExtensionMethods
    {
        public static void Log(this IBufferedLogger[] loggers, string msg)
        {
            foreach (var logger in loggers)
            {
                logger.Log(msg);
            }
        }

        public static string[] GetLoggedLines(this IBufferedLogger[] loggers)
        {
            var listLogger = (ListLogger)loggers.Single(l => l is ListLogger);
            return listLogger.LoggedLines.ToArray();
        }
    }
}