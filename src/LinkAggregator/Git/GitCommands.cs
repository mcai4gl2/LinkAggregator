using System.Diagnostics;
using System.IO;

namespace LinkAggregator.Git
{
    public class GitCommands
    {
        private readonly string _baseWorkingDir;
        private readonly string _gitRepositoryString;
        private readonly string _projectFullDir;

        private GitCommands(string baseWorkingDir, string gitRepositoryString, string projectFullDir)
        {
            _baseWorkingDir = baseWorkingDir;
            _gitRepositoryString = gitRepositoryString;
            _projectFullDir = projectFullDir;
        }

        public static GitCommands Create(string baseWorkingDir, string gitRepositoryString, string projectFullDir)
        {
            return new GitCommands(baseWorkingDir, gitRepositoryString, projectFullDir);   
        }

        public void Clone()
        {
            if (Directory.Exists(_projectFullDir))
            {
                EmptyFolder(new DirectoryInfo(_projectFullDir));
                Directory.Delete(_projectFullDir, true);
            }
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = _baseWorkingDir,
                FileName = "git",
                Arguments = $"clone {_gitRepositoryString}"
            };
            var process = Process.Start(startInfo);
            process.WaitForExit();
        }

        private void EmptyFolder(DirectoryInfo directoryInfo)
        {
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                File.SetAttributes(file.FullName, FileAttributes.Normal);
                file.Delete();
            }

            foreach (DirectoryInfo subfolder in directoryInfo.GetDirectories())
            {
                EmptyFolder(subfolder);
            }
        }

        public void CommitWithMessage(string message)
        {
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = _projectFullDir,
                FileName = "git",
                Arguments = "add ."
            };
            var process = Process.Start(startInfo);
            process.WaitForExit();
            startInfo = new ProcessStartInfo
            {
                WorkingDirectory = _projectFullDir,
                FileName = "git",
                Arguments = "add -u ."
            };
            process = Process.Start(startInfo);
            process.WaitForExit();
            startInfo = new ProcessStartInfo
            {
                WorkingDirectory = _projectFullDir,
                FileName = "git",
                Arguments = $@"commit -m ""{message}"""
            };
            process = Process.Start(startInfo);
            process.WaitForExit();
        }

        public void Push()
        {
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = _projectFullDir,
                FileName = "git",
                Arguments = "push"
            };
            var process = Process.Start(startInfo);
            process.WaitForExit();
        }
    }
}
