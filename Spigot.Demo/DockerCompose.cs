using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Spigot.Demo
{
    public class DockerCompose
    {
        public DockerCompose(IBufferedLogger[] logger)
        {
            this.Logger = logger;
        }

        private string dockerComposeArgs, dockerComposeUpArgs, dockerComposeDownArgs;

        public void Init(string dockerComposeArgs, string dockerComposeUpArgs, string dockerComposeDownArgs)
        {
            this.dockerComposeArgs = dockerComposeArgs;
            this.dockerComposeUpArgs = dockerComposeUpArgs;
            this.dockerComposeDownArgs = dockerComposeDownArgs;
        }

        public Task Up()
        {
            var start = new ProcessStartInfo("docker-compose", $"{this.dockerComposeArgs} up {this.dockerComposeUpArgs}");
            return Task.Run(() => this.RunProcess(start));
        }

        private void RunProcess(ProcessStartInfo processStartInfo)
        {
            var runner = new ProcessRunner(processStartInfo);
            foreach (var logger in this.Logger)
            {
                runner.Subscribe(logger);
            }
            runner.Execute();
        }

        public int PauseMs => 1000;
        public IBufferedLogger[] Logger { get; }

        public void Down()
        {
            var down = new ProcessStartInfo("docker-compose", $"{this.dockerComposeArgs} down {this.dockerComposeDownArgs}");
            this.RunProcess(down);
        }

        public IEnumerable<string> Ps()
        {
            var ps = new ProcessStartInfo("docker-compose", $"{this.dockerComposeArgs} ps");
            var runner = new ProcessRunner(ps);
            var observerToQueue = new ObserverToQueue<string>();

            foreach (var logger in this.Logger)
            {
                runner.Subscribe(logger);
            }
            runner.Subscribe(observerToQueue);
            runner.Execute();
            return observerToQueue.Queue.ToArray();
        }
    }
}