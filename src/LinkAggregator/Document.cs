using System;
using KickStart.Net;

namespace LinkAggregator
{
    public class Document
    {
        public string User { get; set; }
        public string Link { get; set; }
        public string Category { get; set; }
        public string Header { get; set; }
        public string Comment { get; set; }
        public DateTime TimeStamp { get; set; }

        public override string ToString()
        {
            return Objects.ToStringHelper(GetType())
                .Add(nameof(User), User)
                .Add(nameof(Link), Link)
                .Add(nameof(Category), Category)
                .Add(nameof(Header), Header)
                .Add(nameof(Comment), Comment)
                .Add(nameof(TimeStamp), TimeStamp)
                .ToString();
        }
    }
}
