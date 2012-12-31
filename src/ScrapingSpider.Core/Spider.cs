using System.IO.Compression;
using ScrapingSpider.Core.Logging;
using ScrapingSpider.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ScrapingSpider.Core.Events;
using HtmlAgilityPack;

namespace ScrapingSpider.Core
{
    /// <summary>
    /// 主爬虫类
    /// Author: 翟士丹 StanZhai，2012/12/21
    /// </summary>
    public class Spider
    {
        private Settings _settings;
        private Queue<UrlInfo> _urlQueue;
        private Thread[] _crawlThreads;
        private string[] _escapeLinks;
        private string[] _keywords;
        private string[] _regexFilters;
        private CookieContainer _cookieContainer;
        private Random _random;
        private ILogger _log;

        #region 事件
        public event DataReceivedEventHandler DataReceivedEvent;
        public event CrawlErrorEventHandler CrawErrorEvent;
        /// <summary>
        /// 如果事件的执行结果为false，则不会将链接添加到队列
        /// </summary>
        public event AddUrlEventHandler AddUrlEvent;
        #endregion

        public Spider(Settings settings, ILogger logger, IEnumerable<UrlInfo> continueLinks = null)
        {
            _settings = settings;
            _cookieContainer = new CookieContainer();
            _crawlThreads = new Thread[_settings.Threads];
            _urlQueue = new Queue<UrlInfo>();
            _random = new Random();
            _log = logger ?? new EmptyLogger();
            Init();
            // 将待继续爬取的链接加到队列
            if (continueLinks != null)
            {
                foreach (var link in continueLinks)
                {
                    _urlQueue.Enqueue(link);
                }
                _log.Info("上次未处理链接数：" + continueLinks.Count());
            }
        }

        // 按照系统设置初始化Spider
        private void Init()
        {
            // 将初始种子加入队列
            if (!String.IsNullOrEmpty(_settings.InitSeeds))
            {
                foreach (var seed in _settings.InitSeeds.Split(Environment.NewLine.ToCharArray()))
                {
                    if (Regex.IsMatch(seed, @"^(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.IgnoreCase))
                        _urlQueue.Enqueue(new UrlInfo { Url = seed, Depth = 1 });
                }
            }
            // 初始化每个爬取线程
            for (int i = 0; i < _settings.Threads; i++)
            {
                _crawlThreads[i] = new Thread(new ThreadStart(CrawlProc));
            }
            // 设置排除的链接
            if (!String.IsNullOrEmpty(_settings.EscapeLinks))
                _escapeLinks = _settings.EscapeLinks.Split('|');
            // 设置关键字
            if (!String.IsNullOrEmpty(_settings.Keywords))
                _keywords = _settings.Keywords.Replace("\r", "").Split('\n');
            // 设置正则过滤
            if (!String.IsNullOrEmpty(_settings.RegexFilter))
                _regexFilters = _settings.RegexFilter.Replace("\r", "").Split('\n');

            // 设置多线程环境下默认链接数限制为256
            ServicePointManager.DefaultConnectionLimit = 256;
        }

        /// <summary>
        /// 开始抓取
        /// </summary>
        public void Crawl()
        {
            foreach (var thread in _crawlThreads)
            {
                thread.Start();
            }
            _log.Info(String.Format("爬虫已启动，开启线程数：{0}", _crawlThreads.Length));
        }

        // 爬取过程
        private void CrawlProc()
        {
            while (true)
            {
                if (_urlQueue.Count == 0)
                {
                    Thread.Sleep(2000);
                    continue;
                }

                UrlInfo urlInfo = null;
                lock (this)
                {
                    if (_urlQueue.Count == 0)
                        continue;
                    urlInfo = _urlQueue.Dequeue();
                }

                HttpWebRequest request = null;
                HttpWebResponse response = null;
                try
                {
                    if (urlInfo == null)
                        continue;
                    if (_settings.LimitSpeed)       // 1-5秒随机间隔的自动限速
                    {
                        int span = _random.Next(1000, 5000);
                        Thread.Sleep(span);
                    }

                    request = WebRequest.Create(urlInfo.Url) as HttpWebRequest;
                    ConfigRequest(request);
                    response = request.GetResponse() as HttpWebResponse;
                    ParseCookie(response);
                    // 获取网页数据量，如果页面压缩，则解压数据流
                    using (Stream stream = response.ContentEncoding == "gzip" ? 
                        new GZipStream(response.GetResponseStream(), CompressionMode.Decompress) : 
                        response.GetResponseStream())
                    {
                        _log.Info(String.Format("{0}-{1}-{2}-{3}", 
                            (int)response.StatusCode, 
                            response.StatusDescription, 
                            urlInfo.Depth,
                            urlInfo.Url));

                        string html = ParseContent(stream, response.CharacterSet);
                        ParseLinks(urlInfo, html);
                        if (DataReceivedEvent != null)
                            DataReceivedEvent(new DataReceivedEventArgs { Url = urlInfo.Url, Depth = urlInfo.Depth, Html = html});
                        stream.Close();
                    }                   
                }
                catch (Exception ex)
                {
                    _log.Error(String.Format("{0}：{1}", urlInfo.Url, ex.Message));
                    if (CrawErrorEvent != null)
                        CrawErrorEvent(new CrawlErrorEventArgs { Url = urlInfo.Url, Exception = ex });
                }
                finally
                {
                    if (request != null)
                        request.Abort();
                    if (response != null)
                        response.Close();
                }
            }
        }

        /// <summary>
        /// 配置请求设置
        /// </summary>
        /// <param name="request"></param>
        private void ConfigRequest(HttpWebRequest request)
        {
            request.UserAgent = _settings.UserAgent;
            request.CookieContainer = _cookieContainer;
            request.AllowAutoRedirect = true;
            request.MediaType = "text/html";
            request.Headers["Accept-Language"] = "zh-CN,zh;q=0.8";

            if (_settings.Timeout > 0)
                request.Timeout = _settings.Timeout;
        }

        /// <summary>
        /// 解析Cookie
        /// </summary>
        /// <param name="response"></param>
        private void ParseCookie(HttpWebResponse response)
        {
            if (_settings.KeepCookie)
            {
                string cookiesExpression = response.Headers["Set-Cookie"];
                if (!string.IsNullOrEmpty(cookiesExpression))
                {
                    Uri cookieUrl = new Uri(string.Format("{0}://{1}:{2}/", response.ResponseUri.Scheme, response.ResponseUri.Host, response.ResponseUri.Port));
                    _cookieContainer.SetCookies(cookieUrl, cookiesExpression);
                }
            }
        }

        /// <summary>
        /// 解析页面
        /// </summary>
        /// <param name="response"></param>
        private string ParseContent(Stream stream, string responseEncode)
        {
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            byte[] buffer = ms.ToArray();

            Encoding encode = Encoding.ASCII;
            string html = encode.GetString(buffer);
            string cs = responseEncode;
            Match match = Regex.Match(html, "<meta([^<]*)charset=([^<]*)\"", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                cs = match.Groups[2].Value;
                StringBuilder sb = new StringBuilder();
                foreach (var ch in cs.ToCharArray())
                {
                    if (ch == ' ')
                        break;
                    if (ch != '\"')
                        sb.Append(ch);
                }
                cs = sb.ToString();
            }

            if (String.IsNullOrEmpty(cs))
                cs = responseEncode;
            if (!String.IsNullOrEmpty(cs))
                encode = Encoding.GetEncoding(cs);

            ms.Close();

            return encode.GetString(buffer);
        }

        /// <summary>
        /// 解析页面链接
        /// </summary>
        /// <param name="html"></param>
        private void ParseLinks(UrlInfo currentUrl, string html)
        {
            if (_settings.CrawlDepth > 0 && currentUrl.Depth >= _settings.CrawlDepth)
                return;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//a");
            if (linkNodes == null)
                return;
            foreach (var linkNode in linkNodes)
            {
                HtmlAttribute href = linkNode.Attributes["href"];
                string linkText = linkNode.InnerText;
                if (href != null)
                {
                    bool canBeAdd = true;
                    if (_escapeLinks != null)
                    {
                        foreach (var node in _escapeLinks)
                        {
                            if (href.Value.EndsWith(node, StringComparison.OrdinalIgnoreCase))
                            {
                                canBeAdd = false;
                                break;
                            }
                        }                       
                    }
                    if (_keywords != null)
                    {
                        if (!_keywords.Any(linkText.Contains))
                            canBeAdd = false;
                    }

                    if (canBeAdd)
                    {
                        string url = href.Value
                            .Replace("%3f", "?")
                            .Replace("%3d", "=")
                            .Replace("%2f", "/")
                            .Replace("&amp;", "&");

                        if (String.IsNullOrEmpty(url) || 
                            url.StartsWith("#") || 
                            url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) || 
                            url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        Uri uri = new Uri(currentUrl.Url); 
                        Uri thisUri = url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? new Uri(url) : new Uri(uri, url);
                        url = thisUri.AbsoluteUri;
                        if (_settings.LockHost)
                        {
                            // 对于new.baidu.com和www.baidu.com
                            // 如果去除二级域名后相等，则认为是同一个网站
                            if (uri.Host.Split('.').Skip(1).Aggregate((a, b) => a + "." + b) != 
                                thisUri.Host.Split('.').Skip(1).Aggregate((a, b) => a + "." + b))
                                continue;
                        }
                        if (!IsUrlMatchRegex(url))
                            continue;

                        if (AddUrlEvent != null && !AddUrlEvent(new AddUrlEventArgs { Title = linkText, Depth = currentUrl.Depth + 1, Url = url }))
                            continue;

                        lock (this)
                        {
                            _urlQueue.Enqueue(new UrlInfo { Url = url, Depth = currentUrl.Depth + 1 });
                        }
                    }
                }
            }
        }

        private string GetUrlAddess(Uri url)
        {
            return url.Scheme + "://" + url.Authority + "/" + url.LocalPath.Trim('/');
        }

        // 判断Url是否符合正则表达式过滤规则
        private bool IsUrlMatchRegex(string url)
        {
            bool result = false;
            if (_regexFilters != null)
            {
                foreach (var filter in _regexFilters)
                {
                    if (Regex.IsMatch(url, filter, RegexOptions.IgnoreCase))
                    {
                        result = true;
                        break;
                    }
                }
            }
            else
            {
                result = true;
            }
            return  result;
        }

    }
}
