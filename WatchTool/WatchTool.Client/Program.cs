using System;
using System.Threading.Tasks;

namespace WatchTool.Client
{
    internal class Program
    {
        private ClientApplication application;

        private static void Main(string[] args)
        {
            new Program().MainAsync(args).GetAwaiter().GetResult();
        }

        private async Task MainAsync(string[] args)
        {
            Console.CancelKeyPress += ShutdownHandler;

            application = new ClientApplication();
            await application.StartAsync();

            await Task.Delay(-1);
        }

        /// <summary>Shutdown the handler. Executed when user presses CTRL+C on console.</summary>
        private void ShutdownHandler(object sender, ConsoleCancelEventArgs args)
        {
            application.Dispose();

            args.Cancel = true;
            Environment.Exit(0);
        }
    }
}
