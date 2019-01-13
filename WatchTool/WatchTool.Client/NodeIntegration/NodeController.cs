using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using WatchTool.Common.P2P.Payloads;

namespace WatchTool.Client.NodeIntegration
{
    public class NodeController : IDisposable
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly GitIntegration git;

        private readonly APIIntegration api;

        private Task nodeUpdatingTask;

        private bool NodeLaunched = false;

        private Task nodeStatusUpdateTask;

        private CancellationTokenSource cancellation;

        private string lastConsoleOutput;


        public NodeController()
        {
            this.git = new GitIntegration();
            this.api = new APIIntegration();
            this.lastConsoleOutput = null;

            this.nodeUpdatingTask = Task.CompletedTask;
        }

        /// <summary>Clones or updates the node in the background.</summary>
        public void StartUpdatingOrCloningTheNode(Func<Task> onNodeUpdated)
        {
            this.logger.Trace("()");

            if (this.nodeUpdatingTask != Task.CompletedTask)
            {
                this.logger.Debug("Node update is in process.");
                this.logger.Trace("(-)[ALREADY_IN_PROCESS]");
                return;
            }

            if (this.NodeLaunched)
            {
                this.logger.Trace("(-)[NODE_IS_RUNNING]");
                return;
            }

            this.nodeUpdatingTask = this.git.UpdateAndBuildRepositoryAsync(onNodeUpdated);

            this.logger.Trace("(-)");
        }

        public NodeInfoPayload GetNodeInfo()
        {
            this.logger.Trace("()");

            var info = new NodeInfoPayload();

            info.IsNodeCloned = this.git.IsNodeCloned();

            if (!info.IsNodeCloned)
                return info;

            info.NodeRepoInfo = git.GetRepoInfo();

            info.IsNodeRunning = this.NodeLaunched;

            // Parse consensus height
            if (lastConsoleOutput != null)
            {
                string data = lastConsoleOutput;

                string key = "Consensus.Height:    ";
                int keyIndex = data.IndexOf(key);

                if (keyIndex != -1)
                {
                    string consensusHeightString = data.Substring(keyIndex + key.Length);
                    consensusHeightString = consensusHeightString.Substring(0, consensusHeightString.IndexOf("   Consensus.Hash"));
                    int consensusHeight = int.Parse(consensusHeightString);

                    info.RunningNodeInfo = new RunningNodeInfo()
                    {
                        LastConsoleOutput = lastConsoleOutput,
                        ConsensusHeight = consensusHeight
                    };
                }
            }

            this.logger.Trace("(-)");
            return info;
        }

        public void RunNode()
        {
            this.logger.Trace("()");

            if (this.NodeLaunched)
            {
                this.logger.Trace("(-)[ALREADY_LAUNCHED]");
                return;
            }

            this.NodeLaunched = true;

            this.logger.Info("Running the node.");

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

                this.cancellation = new CancellationTokenSource();
                this.nodeStatusUpdateTask = this.UpdateNodeStatusContinuouslyAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            this.logger.Trace("(-)");
        }

        public async Task StopNodeAsync()
        {
            this.logger.Trace("()");

            if (!this.NodeLaunched)
            {
                this.logger.Trace("(-)[NOT_LAUNCHED]");
                return;
            }

            this.NodeLaunched = false;
            this.lastConsoleOutput = null;

            this.logger.Info("Stopping the node.");

            this.cancellation.Cancel();
            await this.nodeUpdatingTask.ConfigureAwait(false);

            await this.api.StopNode().ConfigureAwait(false);

            this.logger.Trace("(-)");
        }

        private async Task UpdateNodeStatusContinuouslyAsync()
        {
            this.logger.Trace("()");

            try
            {
                while (!this.cancellation.IsCancellationRequested)
                {
                    try
                    {
                        this.lastConsoleOutput = await this.api.GetConsoleOutput(this.cancellation.Token).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                    }

                    await Task.Delay(5_000, this.cancellation.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException e)
            {
            }

            this.logger.Trace("(-)");
        }


        public void Dispose()
        {
            this.logger.Trace("()");

            this.nodeStatusUpdateTask?.GetAwaiter().GetResult();

            this.logger.Trace("(-)");
        }
    }
}
