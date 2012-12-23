using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapingSpider.Core.Events
{
    /// <summary>
    /// 爬取错误
    /// </summary>
    public class CrawlErrorEventArgs : EventArgs
    {
        public string Url { get; set; }
        public Exception Exception { get; set; }
    }
}
