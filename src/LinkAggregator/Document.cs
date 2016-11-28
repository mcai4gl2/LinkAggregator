using System;
using KickStart.Net;

namespace LinkAggregator
{
    public class Document
    {
        public string User { get; set; }
        public string Category { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime TimeStamp { get; set; }

        public override string ToString()
        {
            return Objects.ToStringHelper(GetType())
                .Add(nameof(User), User)
                .Add(nameof(Category), Category)
                .Add(nameof(Subject), Subject)
                .Add(nameof(Body), Body)
                .Add(nameof(TimeStamp), TimeStamp)
                .ToString();
        }
    }
}
