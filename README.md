# ScrapingSpider
> C#开发的支持多线程，关键字过滤，正文内容自解析的爬虫
> 爬虫核心使用业余时间开发

# 快速开始
## Spider主爬虫类介绍
爬虫的核心实现在ScrapingSpider.Core程序集中。爬虫类为Spider类，爬虫的爬取逻辑，与页面处理逻辑通过事件分离，两个关键事件为AddUrlEvent和DataReceivedEvent。

## 构造Spider类
```C#
// 构造爬虫，需要3个参数：爬虫设置，实现了ILogger的日志记录器，上次未执行完的爬取链接
Spider spider = new Spider(new Settings(), new EmptyLogger(), null);

spider.AddUrlEvent += addUrlArgs =>
{
	// Url即将添加到队列的事件处理
};

spider.DataReceivedEvent += receivedArgs =>
{
	// 页面已经被抓取下来的事件，可在此处理页面，例如页面保存添加到数据库
};

// 开始爬取
spider.Crawl();
```

## ScrapingSpider示例代码
* 具体的使用方法请参考ScrapingSpider项目的Program.cs类中的示例代码。
* 示例代码使用SqlServer数据库存储爬取信息，表结构与WebPage类对应，数据库连接字符串请参考App.config。
* 采用log4net作为日志记录组件。

## Settings说明
* Init Seeds: 初始Url地址，多个地址使用回车分开。
* Regex Filter: 通过正则表达式过滤Url，多个正则使用回车隔开。
* Keyword: 关键字，按照指定的关键字爬取，多个关键字使用回车分开。
* Crawl Depth: 爬取深度，小于0表示不限
* Escape Links: 要过滤的链接，如：.jpg|.rar|.exe
* Keep Cookie: 抓取过程中是否保留Cookie
* Lock Host: 是否锁定Host，锁定后，指抓取站点相关链接。
* Limit Speed: 是否智能限速。
* Threads: 线程数量，启用多个线程有利于提高爬取效率。
* Timeout: 超时时间，以毫秒为单位。
* User Agent: http协议UserAgent设置。


# 更新日志
* 2012/12/25	添加Url正则表达式过滤功能.
* 2012/12/23	完善说明文档和ScrapingSpider的Settings窗口的使用帮助；解决LockHost无效的问题；完善页面解码的BUG；添加自动限速功能。