using Microsoft.AspNetCore.Mvc;
using NhomBroccoli.Services;

namespace NhomBroccoli.Controllers
{
    public class BlogController : Controller
    {
        private readonly IRssService _rssService;
        public BlogController(IRssService rssService)
        {
            _rssService = rssService;
        }
        public async Task<IActionResult> Index()
        {
            var feedUrl = "https://vnexpress.net/rss/suc-khoe.rss";
            var feedItems = await _rssService.GetRssFeedItems(feedUrl);
            return View(feedItems);
        }
    }
}
