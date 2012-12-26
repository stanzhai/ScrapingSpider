using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrapingSpider.Core.Models
{
    /// <summary>
    /// 爬虫设置
    /// </summary>
    [Serializable]
    public class Settings
    {
        /// <summary>
        /// 初始化种子地址
        /// </summary>
        public string InitSeeds { get; set; }

        private int _threads = 1;
        /// <summary>
        /// 启用的线程数量, 默认为1
        /// </summary>
        public int Threads
        {
            get { return _threads; }
            set { _threads = value; }
        }

        /// <summary>
        /// 要排除的链接:.png|.jpg|.gif|.rar
        /// </summary>
        public string EscapeLinks { get; set; }

        /// <summary>
        /// 指获取包含指定关键字的链接，多个关键字用回车分割
        /// </summary>
        public string Keywords { get; set; }

        /// <summary>
        /// 正则表达式过滤
        /// </summary>
        public string RegexFilter { get; set; }

        private int _crawlDepth = 3;
        /// <summary>
        /// 抓取深度，小于0表示不限制深度, 默认为3
        /// </summary>
        public int CrawlDepth
        {
            get { return _crawlDepth; }
            set { _crawlDepth = value; }
        }

        private int _timeout = 15000;
        /// <summary>
        /// 抓取超时时间
        /// </summary>
        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        private string _userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.97 Safari/537.11";
        public string UserAgent
        {
            get { return _userAgent; }
            set { _userAgent = value; }
        }

        /// <summary>
        /// 是否保留Cookie
        /// </summary>
        public bool KeepCookie { get; set; }

        /// <summary>
        /// 抓取过程中是否锁定Host
        /// </summary>
        public bool LockHost { get; set; }

        /// <summary>
        /// 自动限制速度
        /// </summary>
        public bool LimitSpeed { get; set; }
    }
}
