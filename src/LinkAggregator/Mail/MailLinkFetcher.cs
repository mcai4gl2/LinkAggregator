using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;

namespace LinkAggregator.Mail
{
    public class MailLinkFetcher : ILinkFetcher
    {
        private readonly string _server;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly List<string> _userWhiteList;
        private DateTime _fromDateTime;

        public MailLinkFetcher(string server, int port, string username, string password, List<string> userWhiteList, DateTime fromDateTime)
        {
            _server = server;
            _port = port;
            _username = username;
            _password = password;
            _userWhiteList = userWhiteList;
            _fromDateTime = fromDateTime;
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
                inbox.Open(FolderAccess.ReadWrite);

                var documents = new List<Document>();

                var query = SearchQuery.DeliveredAfter(_fromDateTime)
                    .And(SearchQuery.NotSeen);

                var uids = new List<UniqueId>();
                foreach (var uid in inbox.Search(query))
                {
                    var message = await inbox.GetMessageAsync(uid);
                    if (_userWhiteList.Contains(message.From.First().Name))
                    {
                        uids.Add(uid);
                        documents.Add(new Document
                        {
                            User = message.From.First().Name,
                            Subject = message.Subject,
                            Body = message.TextBody,
                            TimeStamp = message.Date.UtcDateTime
                        });
                    }
                }

                if (uids.Count > 0)
                    inbox.AddFlags(uids, MessageFlags.Seen, true);

                client.Disconnect(true);

                return documents;
            }
        }
    }
}
