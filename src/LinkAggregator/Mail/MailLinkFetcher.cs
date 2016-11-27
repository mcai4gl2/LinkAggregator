using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;

namespace LinkAggregator.Mail
{
    public class MailLinkFetcher : ILinkFetcher
    {
        private readonly string _server;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;

        public MailLinkFetcher(string server, int port, string username, string password)
        {
            _server = server;
            _port = port;
            _username = username;
            _password = password;
        }

        public async Task<IEnumerable<Document>> FetchAsync()
        {
            using (var client = new ImapClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect(_server, _port, true, CancellationToken.None);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_username, _password);

                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadOnly);

                var documents = new List<Document>();

                foreach (var summary in await inbox.FetchAsync(0, 10, MessageSummaryItems.Full | MessageSummaryItems.UniqueId))
                {
                    documents.Add(new Document
                    {
                        User = summary.Envelope.From.First().ToString(),
                        Link = summary.Envelope.Subject,
                        TimeStamp = DateTime.UtcNow
                    });
                }

                client.Disconnect(true);

                return documents;
            }
        }
    }
}
