using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Spigot.Demo.Models;

namespace Spigot.Demo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var producer = FindProducers();
            var consumer = FindConsumer();
            if (string.IsNullOrWhiteSpace(producer) || string.IsNullOrWhiteSpace(consumer))
            {
                Console.WriteLine(
                    "You need to build the entire solution so the consumers and producers are built as well");
                return;
            }

            var config = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", true)
                .Build();
            var settings = new SampleSettings();
            config.Bind(settings);
            var services = new ServiceCollection();

            var items = string.Join("", Enumerable.Range(0, 150).Select(x => '*'));
            Console.WriteLine(items);
            Console.WriteLine(items);
            Console.WriteLine("                   PRESS Q or q TO EXIT CLEANLY");
            Console.WriteLine(items);
            Console.WriteLine(items);
            var processes = new List<ProcessRunner>();
            var ports = new List<int>();
            var fixtures = new List<DockerFixture>();
            var cleanup = new List<string>();
            try
            {
                for (int i = 0; i < settings.NumberOfInstances; i++)
                {
                    var port = FreeTcpPort();
                    var filename = $"{Path.GetFileNameWithoutExtension(Path.GetTempFileName())}.yaml";
                    cleanup.Add(filename);
                    File.WriteAllText(filename,
                        string.Format(File.ReadAllText($"{settings.Backend}.yaml"), port, i));
                    var fixture = new DockerFixture();
                    fixtures.Add(fixture);
                    var options = new DockerFixtureOptions();
                    options.DockerComposeFiles.Add(filename);
                    fixture.Init(() => options);

                    processes.Add(new ProcessRunner(
                        new System.Diagnostics.ProcessStartInfo("dotnet",
                            $"{producer} -n PRODUCER_1_ON_{port} -p {port} -b {settings.Backend}")));
                    processes.Add(new ProcessRunner(
                        new System.Diagnostics.ProcessStartInfo("dotnet",
                            $"{producer} -n PRODUCER_2_ON_{port} -p {port} -b {settings.Backend}")));
                    ports.Add(port);
                }

                services.BuildServiceProvider();

                processes.Add(new ProcessRunner(new System.Diagnostics.ProcessStartInfo("dotnet",
                    $"{consumer} -b {settings.Backend} -p {string.Join(" ", ports.Select(x => $"{x}"))}")));

                processes.ForEach(p => p.Execute(true));

                var quit = false;
                do
                {
                    var info = Console.ReadKey();
                    quit = (info.KeyChar == 'q' || info.KeyChar == 'Q');
                } while (!quit);
            }
            finally
            {
                processes.ForEach(p => p.Dispose());
                fixtures.ForEach(f => f.Dispose());
                cleanup.ForEach(File.Delete);
            }
        }

        private static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        private static string FindProducers()
        {
            //usually /bin/debug/netcoreapp/..dll
            var directory = GetProjectRoot();
            var files = directory.GetFiles("Spigot.Demo.ProducerConsumer.dll", SearchOption.AllDirectories);
            return files.First().FullName;
        }

        private static string FindConsumer()
        {
            //usually /bin/debug/netcoreapp/..dll
            var directory = GetProjectRoot();
            var files = directory.GetFiles("Spigot.Demo.ConsumerOnly.dll", SearchOption.AllDirectories);
            return files.First().FullName;
        }

        private static DirectoryInfo GetProjectRoot()
        {
            return new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.Parent.Parent;
        }
    }

    public class SampleSettings
    {
        public int NumberOfInstances { get; set; }

        public Backend Backend { get; set; }
    }
}