using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WatchTool.Client
{
    internal class Program
    {
        private ClientApplication application;

        private static void Main(string[] args)
        {
            string argumants = "/C dotnet exec \"C:\\Users\\user\\AppData\\Roaming\\WatchTool_StratisFNRepo\\StratisBitcoinFullNode\\src\\Stratis.StratisD\\bin\\Debug\\netcoreapp2.1\\Stratis.StratisD.dll\"";

            Console.WriteLine("=================================---------------------");

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = argumants;
            startInfo.UseShellExecute = true;
            process.StartInfo = startInfo;
            process.Start();


            Console.ReadKey();
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
