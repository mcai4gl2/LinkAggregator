using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LinkAggregator.Git
{
    public class GitDocumentProcessor : IDocumentProcessor
    {
        private readonly string _baseWorkingDir;
        private readonly string _gitRepositoryString;
        private readonly string _projectFullDir;

        public GitDocumentProcessor(string baseWorkingDir, string gitRepositoryString)
        {
            _baseWorkingDir = baseWorkingDir;
            _gitRepositoryString = gitRepositoryString;
            _projectFullDir = Path.Combine(_baseWorkingDir, _gitRepositoryString.Split('/')[1].Split('.')[0]);
        }

        public IEnumerable<Document> Process(IEnumerable<Document> docs)
        {
            Clone();
            var documents = docs.ToList();
            foreach (var doc in documents)
            {
                CreateUserFolderIfNotExists(doc);
                WriteDocument(doc);
            }
            CommitAndPush();
            return documents;
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
            var process = System.Diagnostics.Process.Start(startInfo);
            process.WaitForExit();
        }

        public void CreateUserFolderIfNotExists(Document doc)
        {
            var userDir = Path.Combine(_projectFullDir, doc.User);
            if (!Directory.Exists(userDir))
                Directory.CreateDirectory(userDir);
        }

        public void WriteDocument(Document doc)
        {
            var userDir = Path.Combine(_projectFullDir, doc.User);
            var filename = doc.TimeStamp.Date.ToString("yyyy-MM-dd") + ".md";
            if (!File.Exists(Path.Combine(userDir, filename)))
            {
                using (File.Create(Path.Combine(userDir, filename))) ;
            }

            using (var writer = new StreamWriter(new FileStream(Path.Combine(userDir, filename), FileMode.Append), Encoding.UTF8))
            {
                writer.WriteLine(Environment.NewLine);
                writer.WriteLine($"### {doc.Subject}");
                writer.WriteLine($"@ {doc.TimeStamp}");
                writer.WriteLine();
                writer.WriteLine($"{doc.Body}");
                writer.Flush();
            }
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

        public void CommitAndPush()
        {
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = _projectFullDir,
                FileName = "git",
                Arguments = "add ."
            };
            var process = System.Diagnostics.Process.Start(startInfo);
            process.WaitForExit();
            startInfo = new ProcessStartInfo
            {
                WorkingDirectory = _projectFullDir,
                FileName = "git",
                Arguments = @"commit -m ""Adding new link"""
            };
            process = System.Diagnostics.Process.Start(startInfo);
            process.WaitForExit();
            startInfo = new ProcessStartInfo
            {
                WorkingDirectory = _projectFullDir,
                FileName = "git",
                Arguments = "push"
            };
            process = System.Diagnostics.Process.Start(startInfo);
            process.WaitForExit();
        }
    }
}
