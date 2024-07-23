using NhomBroccoli.Models;
using System.Xml.Linq;

namespace NhomBroccoli.Services
{
    public class RssService : IRssService
    {
        private readonly HttpClient _httpClient;
        public RssService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<List<RssFeedItem>> GetRssFeedItems(string feedUrl)
        {
            var response = await _httpClient.GetStringAsync(feedUrl);
            var xml = XDocument.Parse(response);

            return xml.Descendants("item").Select(item => new RssFeedItem
            {
                Title = item.Element("title")?.Value,
                Link = item.Element("link")?.Value,
                Description = item.Element("description")?.Value,
                PubDate = item.Element("pubDate")?.Value,
                ImageUrl = item.Element("enclosure")?.Attribute("url")?.Value
            }).ToList();
        }
    }
}
