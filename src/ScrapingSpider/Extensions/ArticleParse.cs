using System;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ScrapingSpider.Models;

namespace ScrapingSpider.Extensions
{
    /// <summary>
    /// 自动分析文章正文内容
    /// </summary>
    public class ArticleParse
    {
        private static float _maxT;
        private static HtmlNode _articleNode;
        private static string _publishDate;
        private static readonly string[] EscapeNode = { "#comment", "#text", "script", "style", "title", "head", "compress" };
        private static Regex _dateTimeRegex = new Regex(@"((\d{4}|\d{2})(\-|\/|\.)\d{1,2}\3\d{1,2})|(\d{4}年\d{1,2}月\d{1,2}日)");

        public static WebPage GetArticleWebPage(string rawHtml)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(rawHtml);

            WebPage webPage = new WebPage();
            webPage.Raw = rawHtml.Replace("'", "''");
            var contentNode = GetArticleNode(doc.DocumentNode);
            if (contentNode != null)
                webPage.Content = contentNode.InnerText.Replace("'", "''");
            var titleNode = doc.DocumentNode.SelectSingleNode("//title");
            if (titleNode != null)
                webPage.Title = titleNode.InnerText.Replace("'", "''");
            // 提取发布日期
            if (!String.IsNullOrEmpty(_publishDate))
                webPage.PublishDate = Convert.ToDateTime(_publishDate);

            return webPage;
        }

        public static HtmlNode GetArticleNode(HtmlNode root)
        {
            _maxT = 0.0F;
            _articleNode = null;
            _publishDate = null;
            // 过滤节点内的属性值
            HtmlDocument doc = new HtmlDocument();
            string clearnHtml = Regex.Replace(root.OuterHtml, "(([^\\s=]+)=(\"|\')(.*?)(\"|\'))*>", ">");
            doc.LoadHtml(clearnHtml);
            root = doc.DocumentNode;
            ParseAllNodes(root);
            return _articleNode;
        }

        private static void ParseAllNodes(HtmlNode root)
        {
            if (EscapeNode.Any(t => t == root.Name))
                return;
            foreach (var childNode in root.ChildNodes)
            {
                if (EscapeNode.Any(t => t == childNode.Name))
                    continue;

                float currentT = childNode.InnerText.Length / (float)(childNode.OuterHtml.Length);
                // 分析正文节点
                if (currentT > _maxT && currentT > 0.7)
                {
                    _maxT = currentT;
                    {
                        _articleNode = childNode.ParentNode;
                    }
                }
                // 分析发布日期
                if (String.IsNullOrEmpty(_publishDate) && childNode.InnerText.Length < 30)
                {
                    var match = _dateTimeRegex.Match(childNode.InnerText);
                    if (match.Success)
                    {
                        _publishDate = match.Result("$1");
                    }
                }
                ParseAllNodes(childNode);
            }
        }

    }
}
