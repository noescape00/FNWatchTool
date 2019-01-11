using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
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

        public string GetSolutionPath()
        {
            if (!this.WorkFolderExists())
                return null;

            var repoPath = this.GetRepoPath();

            if (repoPath == null)
                return null;

            foreach (string directory in Directory.EnumerateFiles(repoPath, "*.sln", SearchOption.AllDirectories))
            {
                return Path.GetDirectoryName(directory);
            }

            return null;
        }

        public async Task UpdateAndBuildRepositoryAsync()
        {
            this.logger.Trace("()");

            if (!this.WorkFolderExists() || this.GetRepoPath() == null)
            {
                // Clone
                Directory.CreateDirectory(ClientConfiguration.WorkFolder);

                await Task.Delay(500).ConfigureAwait(false);

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.AddScript($@"cd {ClientConfiguration.WorkFolder}");

                    ps.AddScript($@"git clone {ClientConfiguration.RepoPath}");

                    ps.Invoke();

                    this.LogPsExecutionResult(ps);
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

                this.LogPsExecutionResult(ps);
            }

            this.logger.Info("Finished cloning or updating FN repository.");

            // Build
            string solutionPath = this.GetSolutionPath();

            using (PowerShell ps = PowerShell.Create())
            {
                ps.AddScript($@"cd {solutionPath}");

                ps.AddScript($@"dotnet build");
                ps.Invoke();

                this.LogPsExecutionResult(ps);
            }

            this.logger.Info("Build completed.");

            this.logger.Trace("(-)");
        }

        private void LogPsExecutionResult(PowerShell powershell)
        {
            StringBuilder builder = new StringBuilder();

            this.LogPsStreamData(powershell.Streams.Debug, "Debug", builder);
            this.LogPsStreamData(powershell.Streams.Progress, "Progress", builder);
            this.LogPsStreamData(powershell.Streams.Verbose, "Verbose", builder);
            this.LogPsStreamData(powershell.Streams.Error, "Error", builder);
            this.LogPsStreamData(powershell.Streams.Information, "Information", builder);
            this.LogPsStreamData(powershell.Streams.Warning, "Warning", builder);

            string output = builder.ToString();

            if (string.IsNullOrEmpty(output))
                return;

            this.logger.Info(output);
        }

        private void LogPsStreamData<T>(PSDataCollection<T> data, string streamName, StringBuilder builder) where T : class
        {
            if (!data.Any())
                return;

            builder.AppendLine($"PowerShell data from {streamName} stream:");

            foreach (T item in data)
            {
                builder.AppendLine(item.ToString());
            }
        }
    }
}
