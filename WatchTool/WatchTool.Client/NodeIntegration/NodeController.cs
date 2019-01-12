using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;
using NLog;
using WatchTool.Common.P2P.Payloads;

namespace WatchTool.Client.NodeIntegration
{
    public class NodeController
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly GitIntegration git;

        private Task nodeUpdatingTask;

        public NodeController(GitIntegration git)
        {
            this.git = git;

            this.nodeUpdatingTask = Task.CompletedTask;
        }

        /// <summary>Clones or updates the node in the background.</summary>
        public void StartUpdatingOrCloningTheNode()
        {
            this.logger.Trace("()");

            if (this.nodeUpdatingTask != Task.CompletedTask)
            {
                this.logger.Debug("Node update is in process.");
                this.logger.Trace("(-)[ALREADY_IN_PROCESS]");
                return;
            }

            this.nodeUpdatingTask = this.git.UpdateAndBuildRepositoryAsync();

            this.logger.Trace("(-)");
        }

        public NodeInfoPayload GetNodeInfo()
        {
            this.logger.Trace("()");

            var info = new NodeInfoPayload();

            bool nodeCloned = this.git.GetSolutionPath() != null;
            info.IsNodeCloned = nodeCloned;

            if (!nodeCloned)
                return info;

            info.NodeRepoInfo = git.GetRepoInfo();

            // TODO ADD is running & running info

            this.logger.Trace("(-)");
            return info;
        }

        public void RunNode()
        {
            this.logger.Trace("()");

            try
            {
                var solutionPath = this.git.GetSolutionPath();

                var dllPath = solutionPath + @"\Stratis.StratisD\bin\Debug\netcoreapp2.1\Stratis.StratisD.dll";

                string strCmdText = $"/C dotnet exec \"{dllPath}\" -testnet";

                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = strCmdText;
                startInfo.UseShellExecute = true;
                process.StartInfo = startInfo;
                process.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            this.logger.Trace("(-)");
        }

        public void StopNode()
        {

        }
    }
}
