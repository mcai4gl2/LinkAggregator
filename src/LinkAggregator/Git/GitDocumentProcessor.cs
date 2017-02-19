using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LinkAggregator.Git
{
    public class GitDocumentProcessor : IDocumentProcessor
    {
        private readonly string _projectFullDir;
        private readonly GitCommands _gitCommands;

        public GitDocumentProcessor(string baseWorkingDir, string gitRepositoryString)
        {
            _projectFullDir = Path.Combine(baseWorkingDir, gitRepositoryString.Split('/')[1].Split('.')[0]);
            _gitCommands = GitCommands.Create(baseWorkingDir, gitRepositoryString, _projectFullDir);         
        }

        public IEnumerable<Document> Process(IEnumerable<Document> docs)
        {
            _gitCommands.Clone();
            var documents = docs.ToList();
            foreach (var doc in documents)
            {
                CreateUserFolderIfNotExists(doc);
                WriteDocument(doc);
            }
            _gitCommands.CommitWithMessage("Adding new links");
            _gitCommands.Push();
            return documents;
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
    }
}
