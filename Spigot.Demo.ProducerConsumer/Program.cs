using System;
using System.Threading;
using Archetypical.Software.Spigot;
using Archetypical.Software.Spigot.Extensions;
using Archetypical.Software.Spigot.Streams.KubeMQ;
using Archetypical.Software.Spigot.Streams.Redis;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spigot.Demo.Models;
using StackExchange.Redis;

namespace Spigot.Demo.ProducerConsumer
{
    internal class Program
    {
        public class Options
        {
            [Option('p', Required = true, HelpText = "The port to configure the spigot taps ")]
            public int Port { get; set; }

            [Option('n', Required = true, HelpText = "The friendly name of the producer/consumer")]
            public string Name { get; set; }

            [Option('b', Default = Backend.Redis, Required = true, HelpText = "The type of the backing for Spigot")]
            public Backend Backend { get; set; }
        }

        /// <summary>
        /// He needs the following:
        /// Backing (Redis/KubeMQ)
        /// Port
        /// Name
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            var random = new Random();
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    Console.WriteLine(
                        $"---------------------------------- PRODUCER/CONSUMER NAMED {o.Name} ----------------------------------");

                    var config = new ConfigurationBuilder().Build();
                    var collection = new ServiceCollection().AddLogging();
                    var spigot = collection.AddSpigot(config)
                        .WithFriendlyName(o.Name)
                        .AddKnob<ConsoleKnob, ComplexModelWithChildren>();
                    switch (o.Backend)
                    {
                        case Backend.Redis:
                            spigot.AddRedis(r =>
                            {
                                r.ConfigurationOptions = ConfigurationOptions.Parse($"localhost:{o.Port}");
                            });
                            break;

                        case Backend.KubeMQ:
                            spigot.AddKubeMq(r =>
                            {
                                r.ServerAddress = $"localhost:{o.Port}";
                            });
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    spigot.Build();

                    var provider = collection.BuildServiceProvider();
                    var sender = provider.GetService<MessageSender<ComplexModelWithChildren>>();
                    while (true)
                    {
                        sender.Send(ComplexModelWithChildren.Random());
                        Thread.Sleep(random.Next(1000, 5000));
                    }
                });
        }
    }
}