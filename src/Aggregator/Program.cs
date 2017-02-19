using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KickStart.Net;
using KickStart.Net.Extensions;
using LinkAggregator;
using LinkAggregator.Git;
using LinkAggregator.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Aggregator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("app.config.json");
            builder.AddJsonFile("app.config.secret.json");
            var config = builder.Build();

            ApplicationLogging.LoggerFactory.AddProvider(
                new ConsoleLoggerProvider((text, logLevel) => logLevel >= LogLevel.Debug, true));

            var logger = ApplicationLogging.LoggerFactory.CreateLogger<Program>();

            var server = config["email:server"];
            var port = int.Parse(config["email:port"]);
            var username = config["email:username"];
            var workingDir = config["git:workingDir"];
            workingDir = workingDir ?? Path.GetTempPath();
            var repo = config["git:repository"];
            var frequency = int.Parse(config["email:pollingFrequencyInMins"]);

            var userWhiteListStr = config["email:userWhiteList"];

            var processor = new GitDocumentProcessor(workingDir, repo);
            var merger = new GitDocumentMerger(workingDir, repo);

            var task = Task.Factory.ScheduleAtFixedDelay(async () =>
            {
                using (logger.BeginScope(DateTime.Now))
                {
                    try
                    {
                        logger.LogInformation($"Start fetching emails from {server}:{port} with {username}");
                        var fetcher = new MailLinkFetcher(
                            server,
                            port,
                            username,
                            config["email:password"],
                            userWhiteListStr.Split(';').ToList(),
                            DateTime.UtcNow.Date.AddMonths(-1));

                        var docs = await fetcher.FetchAsync().ToListAsync();
                        logger.LogInformation($"Found {docs.Count} emails");
                        if (docs.Count > 0)
                            processor.Process(docs);
                        logger.LogInformation("Done adding");

                        logger.LogInformation("Start running merger ...");
                        merger.Process();
                        logger.LogInformation("Done merging");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Failed", ex);
                    }
                }
            }, 0, frequency, TimeUnits.Minutes);

            task.Result.Wait();
        }
    }
}
