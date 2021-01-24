using System;
using Microsoft.Extensions.Logging;

namespace Spigot.Demo
{
    public class BufferedLogger : IBufferedLogger
    {
        private readonly ILogger<BufferedLogger> _logger;

        public BufferedLogger()
        {
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
            Console.WriteLine(error.ToString());
        }

        public void OnNext(string value)
        {
            Console.WriteLine(value);
        }

        public void Log(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}