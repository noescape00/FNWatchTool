using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;
using NLog;

namespace WatchTool.Client.NodeIntegration
{
    public class GitIntegration
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public bool WorkFolderExists()
        {
            return Directory.Exists(ClientConfiguration.WorkFolder);
        }

        public string GetRepoPath()
        {
            if (!this.WorkFolderExists())
                return null;

            foreach (string directory in Directory.EnumerateDirectories(ClientConfiguration.WorkFolder, ".git", SearchOption.AllDirectories))
            {
                return Path.GetDirectoryName(directory);
            }

            return null;
        }

        public async Task CloneOrUpdateRepositoryAsync()
        {
            if (!this.WorkFolderExists() || this.GetRepoPath() == null)
            {
                // Clone
                Directory.CreateDirectory(ClientConfiguration.WorkFolder);

                await Task.Delay(500).ConfigureAwait(false);

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.AddScript($@"cd {ClientConfiguration.WorkFolder}");

                    ps.AddScript($@"git clone {ClientConfiguration.RepoPath}");

                    Collection<PSObject> results = ps.Invoke();

                    if (ps.HadErrors)
                        this.LogPSErrors(ps);
                }
            }

            // Update
            string repoPath = this.GetRepoPath();

            using (PowerShell ps = PowerShell.Create())
            {
                ps.AddScript($@"cd {repoPath}");

                ps.AddScript($@"git checkout master");
                ps.AddScript($@"git pull");

                ps.Invoke();
            }
        }

        private void LogPSErrors(PowerShell powershell)
        {
            foreach (var error in powershell.Streams.Error)
            {
                this.logger.Warn(error.ToString());
            }
        }
    }
}
