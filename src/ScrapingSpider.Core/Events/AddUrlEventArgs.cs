using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapingSpider.Core.Events
{
    public class AddUrlEventArgs : EventArgs
    {
        public string Url { get; set; }
        public int Depth { get; set; }
        public string Title { get; set; }
    }
}
