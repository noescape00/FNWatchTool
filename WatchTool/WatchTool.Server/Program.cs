﻿using System;
using System.Threading.Tasks;

namespace WatchTool.Server
{
    internal class Program
    {
        private ServerApplication application;

        private static void Main(string[] args)
        {
            new Program().MainAsync(args).GetAwaiter().GetResult();
        }

        private async Task MainAsync(string[] args)
        {
            Console.CancelKeyPress += this.ShutdownHandler;

            this.application = new ServerApplication();
            await this.application.StartAsync(args);

            await Task.Delay(-1);
        }

        /// <summary>Shutdown the handler. Executed when user presses CTRL+C on console.</summary>
        private void ShutdownHandler(object sender, ConsoleCancelEventArgs args)
        {
            this.application.Dispose();

            args.Cancel = true;
            Environment.Exit(0);
        }
    }
}
