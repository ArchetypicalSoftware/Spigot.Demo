using System;

namespace Spigot.Demo
{
    public interface IBufferedLogger : IObserver<string>
    {
        void Log(string msg);
    }
}