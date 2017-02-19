using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using KickStart.Net.Extensions;

namespace LinkAggregator.Git
{
    public class GitDocumentMerger
    {
        private readonly ILogger<GitDocumentMerger> _logger = ApplicationLogging.LoggerFactory.CreateLogger<GitDocumentMerger>();
        private readonly string _projectFullDir;
        private readonly GitCommands _gitCommands;

        public GitDocumentMerger(string baseWorkingDir, string gitRepositoryString)
        {
            _projectFullDir = Path.Combine(baseWorkingDir, gitRepositoryString.Split('/')[1].Split('.')[0]);
            _gitCommands = GitCommands.Create(baseWorkingDir, gitRepositoryString, _projectFullDir);
        }

        public void Process()
        {
            _gitCommands.Clone();

            var directoryInfo = new DirectoryInfo(_projectFullDir);
            foreach (var userFolder in directoryInfo.GetDirectories())
            {
                var fileInfos = userFolder.GetFiles("*.md", SearchOption.TopDirectoryOnly);
                var results = fileInfos.Where(f => f.NameIsDate("yyyy-MM-dd") && f.IsNotFromThisMonth())
                    .GroupBy(f => f.GetYearAndMonthPart());
                foreach (var result in results)
                {
                    _logger.LogInformation($"Creating digest for {result.Key}");
                    _logger.LogInformation($"Containing files: {", ".Joins(result.Select(r => r.Name).ToArray())}");

                    var filename = result.Key + ".md";
                    var fullname = Path.Combine(userFolder.FullName, filename);
                    if (!File.Exists(fullname))
                    {
                        using (File.Create(fullname)) ;
                    }

                    using (var writer = new StreamWriter(new FileStream(fullname, FileMode.Append), Encoding.UTF8))
                    {
                        foreach (var fileInfo in result)
                        {
                            using (var reader = new StreamReader(new FileStream(fileInfo.FullName, FileMode.Open)))
                            {
                                var content = reader.ReadToEnd();
                                writer.WriteLine(content);
                                writer.Flush();
                            }
                            fileInfo.Delete();
                        }
                    }
                }
            }

            _gitCommands.CommitWithMessage("Creating monthly digest");
        }
    }

    public static partial class StringExtensions
    {
        public static bool NameIsDate(this FileInfo fileInfo, string pattern)
        {
            var filename = fileInfo.Name;
            filename = filename.Split('.')[0];
            DateTime dt;
            return DateTime.TryParseExact(filename, pattern,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out dt);
        }

        public static bool IsNotFromThisMonth(this FileInfo fileInfo)
        {
            var serverDateString = DateTime.Today.ToString("yyyy-MM");
            return !fileInfo.Name.StartsWith(serverDateString);
        }

        public static string GetYearAndMonthPart(this FileInfo fileInfo)
        {
            return fileInfo.Name.Substring(0, 7);
        }
    }
}
