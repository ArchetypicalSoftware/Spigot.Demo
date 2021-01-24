using System;

namespace Spigot.Demo.Models
{
    public class ComplexChild
    {
        private static Random rando = new Random(DateTime.Now.Millisecond);

        public static ComplexChild Random() =>
            new ComplexChild
            {
                Name = $"Random_{rando.Next()}",
                S = (short)rando.Next(short.MaxValue - 1),
                TimeSpan = new TimeSpan(rando.Next(0, int.MaxValue - 1))
            };

        public TimeSpan TimeSpan { get; set; }
        public short S { get; set; }

        public string Name { get; set; }
    }
}