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

namespace ScrapingSpider.Core
{
    /// <summary>
    /// 主爬虫类
    /// Author: 翟士丹 StanZhai，2012/12/21
    /// </summary>
    public class Spider
    {
        private Settings _settings;
        private readonly Queue<UrlInfo> _urlQueue;
        private readonly Thread[] _crawlThreads;
        private readonly bool[] _idleThreads;     // 记录空闲着的线程，当所有的线程都处于空闲状态，则表明爬取结束
        private string[] _escapeLinks;
        private string[] _keywords;
        private string[] _regexFilters;
        private readonly CookieContainer _cookieContainer;
        private readonly Random _random;
        private readonly ILogger _log;
        public Settings Settings 
        {
            get { return _settings; }
            set { _settings = value; }
        }

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
            _idleThreads = new bool[_settings.Threads];
            _urlQueue = new Queue<UrlInfo>();
            _random = new Random();
            _log = logger ?? new EmptyLogger();
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
                _crawlThreads[i] = new Thread(new ParameterizedThreadStart(CrawlProc));
            }
            // 设置排除的链接
            if (!String.IsNullOrEmpty(_settings.EscapeLinks))
                _escapeLinks = _settings.EscapeLinks.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            // 设置关键字
            if (!String.IsNullOrEmpty(_settings.Keywords))
                _keywords = _settings.Keywords.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            // 设置正则过滤
            if (!String.IsNullOrEmpty(_settings.RegexFilter))
                _regexFilters = _settings.RegexFilter.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // 设置多线程环境下默认链接数限制为256
            ServicePointManager.DefaultConnectionLimit = 256;
        }


        /// <summary>
        /// 开始抓取
        /// </summary>
        public void Crawl()
        {
            Init();
            for (int i = 0; i < _crawlThreads.Count(); i++)
            {
                _crawlThreads[i].Start(i);
                _idleThreads[i] = false;
            }
            _log.Info(String.Format("爬虫已启动，开启线程数：{0}", _crawlThreads.Length));
        }

        /// <summary>
        /// 停止爬取
        /// </summary>
        public void Stop()
        {
            foreach (var thread in _crawlThreads)
            {
                thread.Abort();
            }
        }

        // 爬取过程
        private void CrawlProc(object threadIndex)
        {
            int currentThreadIndex = (int) threadIndex;
            while (true)
            {
                // 根据队列中的Url数量和空闲线程的数量，判断线程是睡眠还是退出
                if (_urlQueue.Count == 0)
                {
                    _idleThreads[currentThreadIndex] = true;
                    if (!_idleThreads.Any(t => t == false))
                    {
                        _log.Info("爬取结束，第" + (currentThreadIndex + 1) + "个线程退出。");
                        break;
                    }
                    Thread.Sleep(2000);
                    continue;
                }
                _idleThreads[currentThreadIndex] = false;

                // 从队列中取url进行爬取
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
                    Uri cookieUrl = new Uri(string.Format("{0}://{1}:{2}/", 
                        response.ResponseUri.Scheme, 
                        response.ResponseUri.Host, 
                        response.ResponseUri.Port));
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

            // 获取页面所有链接
            Dictionary<string, string> urls = new Dictionary<string, string>();
            Match match = Regex.Match(html, "(?i)<a .*?href=\"([^\"]+)\"[^>]*>(.*?)</a>");
            while (match.Success)
            {
                // 以href作为key，已链接文本作为value
                urls[match.Groups[1].Value] = Regex.Replace(match.Groups[2].Value, "(?i)<.*?>", "");
                match = match.NextMatch();
            }

            foreach (var linknode in urls)
            {
                string href = linknode.Key;
                string linkText = linknode.Value;
                if (href != null)
                {
                    bool canBeAdd = true;
                    if (_escapeLinks != null)
                    {
                        foreach (var node in _escapeLinks)
                        {
                            if (href.EndsWith(node, StringComparison.OrdinalIgnoreCase))
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
                        string url = href
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

                        AddUrlEventArgs args = new AddUrlEventArgs { Title = linkText, Depth = currentUrl.Depth + 1, Url = url };
                        if (AddUrlEvent != null && !AddUrlEvent(args))
                            continue;

                        lock (this)
                        {
                            _urlQueue.Enqueue(new UrlInfo { Url = url, Depth = currentUrl.Depth + 1 });
                        }
                    }
                }
            }
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
