namespace WatchTool.Client.NodeIntegration
{
    public class NodeController
    {
        private readonly GitIntegration git;

        public NodeController(GitIntegration git)
        {
            this.git = git;
        }

        /// <summary>Clones or updates the node in the background.</summary>
        public void StartUpdatingOrCloningTheNode()
        {
            // TODO check if already doing that. do nothing if in progress
        }
    }
}
