using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KickStart.Net;
using KickStart.Net.Extensions;
using LinkAggregator;
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

            var task = Task.Factory.ScheduleAtFixedDelay(async () =>
            {
                using (logger.BeginScope(DateTime.Now))
                {
                    logger.LogInformation($"Start fetching emails from {server}:{port} with {username}");
                    var fetcher = new MailLinkFetcher(
                        server,
                        port,
                        username,
                        config["email:password"]);
                    var docs = await fetcher.FetchAsync();
                    foreach (var doc in docs)
                    {
                        Console.WriteLine(doc);
                    }
                    logger.LogInformation("Done");
                }
            }, 0, 5, TimeUnits.Seconds);

            Console.ReadKey();
        }
    }
}
