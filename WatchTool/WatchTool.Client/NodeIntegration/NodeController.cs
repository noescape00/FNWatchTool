﻿using System.Threading.Tasks;
using NLog;

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
    }
}