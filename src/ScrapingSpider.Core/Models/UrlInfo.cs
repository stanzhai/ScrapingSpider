using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapingSpider.Core.Models
{
    /// <summary>
    /// Url信息
    /// </summary>
    public class UrlInfo
    {
        /// <summary>
        /// Url地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 相对于种子地址的深度
        /// </summary>
        public int Depth { get; set; }
    }
}
