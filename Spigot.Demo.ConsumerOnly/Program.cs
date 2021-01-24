using System;
using System.Collections.Generic;
using System.Threading;
using Archetypical.Software.Spigot;
using Archetypical.Software.Spigot.Extensions;
using Archetypical.Software.Spigot.Streams.KubeMQ;
using Archetypical.Software.Spigot.Streams.Redis;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spigot.Demo.Models;
using StackExchange.Redis;

namespace Spigot.Demo.ConsumerOnly
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    Console.WriteLine(
                        $"---------------------------------- CONSUMER OF ALL ----------------------------------");
                    var config = new ConfigurationBuilder().Build();
                    var collection = new ServiceCollection().AddLogging(s =>
                    {
                        s.AddConsole();
                        //.SetMinimumLevel(LogLevel.Trace);
                    });
                    Console.WriteLine("Adding Spigot");
                    Console.WriteLine("|");
                    var spigot = collection.AddSpigot(config)
                        .WithFriendlyName("Consumer of all things")
                        .AddKnob<ConsoleKnob, ComplexModelWithChildren>();
                    foreach (var objPort in o.Ports)
                    {
                        switch (o.Backend)
                        {
                            case Backend.Redis:
                                Console.WriteLine($"|-->Adding Redis stream on port {objPort}");
                                spigot.AddRedis(r =>
                                {
                                    r.ConfigurationOptions = ConfigurationOptions.Parse($"localhost:{objPort}");
                                });
                                break;

                            case Backend.KubeMQ:
                                Console.WriteLine($"|-->Adding KubeMQ eventstore on port {objPort}");
                                spigot.AddKubeMq(r =>
                                {
                                    r.ServerAddress = $"localhost:{objPort}";
                                });
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    spigot.Build();
                    var provider = collection.BuildServiceProvider();
                    Console.WriteLine();
                    provider.GetService<MessageSender<ComplexModelWithChildren>>()
                        .Send(ComplexModelWithChildren.Random());
                });

            while (true)
            {
                Thread.Sleep(2000);
            }
        }

        public class Options
        {
            [Option('p', Required = true, HelpText = "The port to configure the spigot taps ")]
            public IEnumerable<int> Ports { get; set; }

            [Option('b', Default = Backend.Redis, Required = true, HelpText = "The type of the backing for Spigot")]
            public Backend Backend { get; set; }
        }
    }
}