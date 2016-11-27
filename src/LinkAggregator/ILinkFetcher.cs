using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkAggregator
{
    public interface ILinkFetcher
    {
        Task<IEnumerable<Document>> FetchAsync();
    }
}
