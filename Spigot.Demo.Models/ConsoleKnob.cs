using System;
using System.Linq;
using Archetypical.Software.Spigot;
using Microsoft.Extensions.Logging;

namespace Spigot.Demo.Models
{
    public class ConsoleKnob : Knob<ComplexModelWithChildren>
    {
        private ConsoleColor[] colors;

        public ConsoleKnob(Archetypical.Software.Spigot.Spigot spigot, ILogger<Knob<ComplexModelWithChildren>> logger) : base(spigot, logger)
        {
            Console.WriteLine("|-----> Adding New instance of a Knob");
            colors = Enum.GetValues(typeof(ConsoleColor)).Cast<ConsoleColor>().ToArray();
        }

        protected override void HandleMessage(EventArrived<ComplexModelWithChildren> message)
        {
            Console.ForegroundColor = colors[Math.Abs(message.Context.Sender.Name.GetHashCode()) % 15];
            Console.WriteLine($"Received a message from {message.Context.Sender.Name}[{message.Context.Sender.InstanceIdentifier}] || {message.EventData}");
            Console.ResetColor();
        }
    }
}