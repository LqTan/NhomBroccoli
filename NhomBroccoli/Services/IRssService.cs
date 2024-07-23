using NhomBroccoli.Models;

namespace NhomBroccoli.Services
{
    public interface IRssService
    {
        public Task<List<RssFeedItem>> GetRssFeedItems(string feedUrl);
    }
}
