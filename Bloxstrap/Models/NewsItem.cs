using SimpleFeedReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bloxstrap.Models
{
    internal class NewsItem
    {
        public string? Title { get; set; }

        public DateTimeOffset? Date { get; set; }

        public string? ThumbnailURL { get; set; }

        public bool ThumbnailExpanded { get; set; } = false;

        public string? PostURL { get; set; }

        public string? DateString => Date?.ToString("d MMMM yyyy");

        public NewsItem(string? title, DateTimeOffset? date, string? thumbnailURL, string? postURL)
        {
            Title = title;
            Date = date;
            ThumbnailURL = thumbnailURL;
            PostURL = postURL;
        }

        // from RSS feed
        public NewsItem(FeedItem item)
        {
            Title = item.Title;
            Date = item.PublishDate;
            ThumbnailURL = item.Images?.First().AbsoluteUri;
            PostURL = item.Uri?.AbsoluteUri;
        }

        public static List<NewsItem> MakeTestSet()
        {
            const string testImage = "https://images.wondershare.com/filmora/article-images/2021/importance-with-video-walls-1.jpg";
            return new()
            {
                //new NewsItem("1 Test", DateTimeOffset.Now, testImage, "https://google.com"),
                //new NewsItem("2 Test", DateTimeOffset.Now, testImage, "https://google.com"),
                new NewsItem("3 Teeeeeeeeeeeeeeeeessssssssssttttttttttttt", DateTimeOffset.Now, testImage, "https://google.com"),
                new NewsItem("4 Test Test Test Test Test Test Test Test Test Test Test", DateTimeOffset.Now, testImage, "https://google.com"),
                new NewsItem("5 Test", DateTimeOffset.Now, testImage, "https://google.com"),
            };
        }
    }
}

