using System.IO;
using System.Windows.Forms;
using ScrapingSpider.DataAccess;
using ScrapingSpider.Extensions;
using ScrapingSpider.Forms;
using ScrapingSpider.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrapingSpider.Core;
using ScrapingSpider.Logging;

namespace ScrapingSpider
{
    class Program
    {
        static void Main(string[] args)
        {
            MD5Helper.GetMD5HashCode("zsd");
            // 初始化log4net
            log4net.Config.XmlConfigurator.Configure(new FileInfo(Path.Combine(Application.StartupPath, "Config", "log4net.config")));
            FrmSettings frmSettings = new FrmSettings();
            if (frmSettings.ShowDialog() == DialogResult.OK)
            {
                Spider spider = new Spider(frmSettings.Settings, Log4netFactory.CreateLogger());
                //spider.DataReceivedEvent += receivedArgs =>
                //{
                //    WebPage webPage = ArticleParse.GetArticleWebPage(receivedArgs.Html);
                //    webPage.Id = MD5Helper.GetMD5HashCode(receivedArgs.Url);
                //    webPage.Url = receivedArgs.Url;
                //    webPage.Depth = receivedArgs.Depth;
                //    webPage.InsertDate = DateTime.Now;
                //    webPage.Status = 1;
                //    WebPageDao.SaveOrUpdateWebPage(webPage);
                //};
                spider.Crawl();
            }
        }
    }
}
