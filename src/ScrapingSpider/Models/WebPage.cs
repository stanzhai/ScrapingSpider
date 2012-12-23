using System;

namespace ScrapingSpider.Models
{
    /// <summary>
    /// 网页结构化数据模型
    /// </summary>
    public class WebPage
    {
        private DateTime _publishDate = new DateTime(1990, 1, 1);

        public string Id { get; set; }

        public string Url { get; set; }

        /// <summary>
        /// Url相对于种子地址的深度
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Html原始内容
        /// </summary>
        public string Raw { get; set; }

        /// <summary>
        /// 解析后的正文内容
        /// </summary>
        public string Content { get; set; }

        public string Title { get; set; }

        public DateTime PublishDate
        {
            get { return _publishDate; }
            set { _publishDate = value; }
        }

        public DateTime InsertDate { get; set; }

        /// <summary>
        /// 状态：0，尚未下载，1，已下载
        /// </summary>
        public short Status { get; set; }
    }
}
