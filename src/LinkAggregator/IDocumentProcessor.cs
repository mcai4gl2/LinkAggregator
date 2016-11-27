using System.Collections.Generic;

namespace LinkAggregator
{
    public interface IDocumentProcessor
    {
        IEnumerable<Document> Process(IEnumerable<Document> docs);
    }
}
