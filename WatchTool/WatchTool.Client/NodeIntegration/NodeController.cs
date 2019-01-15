using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using WatchTool.Common.P2P.Payloads;

namespace WatchTool.Client.NodeIntegration
{
    public class NodeController
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly GitIntegration git;

        private readonly APIIntegration api;

        public NodeController(ClientConfiguration config)
        {
            this.git = new GitIntegration(config);
            this.api = new APIIntegration(config);
        }

        private async Task<NodeRunningResul> IsNodeLaunchedAsync(CancellationToken token)
        {
            this.logger.Trace("()");

            try
            {
                string result = await this.api.GetConsoleOutput(token).ConfigureAwait(false);

                bool nodeRunning = !string.IsNullOrEmpty(result);

                this.logger.Trace("(-):{0}", nodeRunning);
                return new NodeRunningResul() {IsNodeRunning = nodeRunning, LastLog = result};
            }
            catch (Exception)
            {
            }

            this.logger.Trace("(-)[EX]:false");
            return new NodeRunningResul() {IsNodeRunning = false};
        }

        /// <summary>Clones or updates the node in the background.</summary>
        public async Task UpdateOrCloneTheNodeAsync()
        {
            this.logger.Trace("()");

            await this.git.UpdateAndBuildRepositoryAsync().ConfigureAwait(false);

            this.logger.Trace("(-)");
        }

        public async Task<NodeInfoPayload> GetNodeInfo(CancellationToken token)
        {
            this.logger.Trace("()");

            var info = new NodeInfoPayload();

            info.PayloadCreationUTCTime = DateTime.UtcNow;

            info.IsNodeCloned = this.git.IsNodeCloned();

            if (!info.IsNodeCloned)
                return info;

            info.NodeRepoInfo = git.GetRepoInfo();

            NodeRunningResul runningResult = await this.IsNodeLaunchedAsync(token).ConfigureAwait(false);

            info.IsNodeRunning = runningResult.IsNodeRunning;

            // Parse consensus height
            if (info.IsNodeRunning)
            {
                string data = runningResult.LastLog;

                string key = "Consensus.Height:    ";
                int keyIndex = data.IndexOf(key);

                if (keyIndex != -1)
                {
                    string consensusHeightString = data.Substring(keyIndex + key.Length);
                    consensusHeightString = consensusHeightString.Substring(0, consensusHeightString.IndexOf("   Consensus.Hash"));
                    int consensusHeight = int.Parse(consensusHeightString);

                    info.RunningNodeInfo = new RunningNodeInfo()
                    {
                        LastConsoleOutput = runningResult.LastLog,
                        ConsensusHeight = consensusHeight
                    };
                }
            }

            this.logger.Trace("(-)");
            return info;
        }

        public async Task RunNodeAsync(CancellationToken token)
        {
            this.logger.Trace("()");

            NodeRunningResul runningResult = await this.IsNodeLaunchedAsync(token).ConfigureAwait(false);

            if (runningResult.IsNodeRunning)
            {
                this.logger.Trace("(-)[ALREADY_LAUNCHED]");
                return;
            }

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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            while (true)
            {
                runningResult = await this.IsNodeLaunchedAsync(token).ConfigureAwait(false);

                if (runningResult.IsNodeRunning)
                    break;

                await Task.Delay(1500, token).ConfigureAwait(false);
            }

            this.logger.Trace("(-)");
        }

        public async Task StopNodeAsync(CancellationToken token)
        {
            this.logger.Trace("()");

            NodeRunningResul runningResult = await this.IsNodeLaunchedAsync(token).ConfigureAwait(false);

            if (!runningResult.IsNodeRunning)
            {
                this.logger.Trace("(-)[NODE_NOT_RUNNING]");
                return;
            }

            this.logger.Info("Stopping the node.");

            await this.api.StopNodeAsync(token).ConfigureAwait(false);

            // Give node some time to stop.
            await Task.Delay(1000, token).ConfigureAwait(false);

            this.logger.Trace("(-)");
        }

        private class NodeRunningResul
        {
            public bool IsNodeRunning;
            public string LastLog;
        }
    }
}
