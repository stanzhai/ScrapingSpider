# ScrapingSpider
> C#开发的支持多线程，关键字过滤，正文内容自解析的爬虫
> 爬虫核心使用业余时间开发

# 快速开始
## 构造Spider类

## Settings说明
* Init Seeds: 初始Url地址，多个地址使用回车分开。
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
* 2012/12/23	完善说明文档和ScrapingSpider的Settings窗口的使用帮助；解决LockHost无效的问题；完善页面解码的BUG；添加自动限速功能。