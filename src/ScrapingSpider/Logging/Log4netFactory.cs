//============================================================
//
//    Copyright (C) 2012 翟士丹@CYOU●监控部●CYINC.MOMain 版权所有
//    文件名　：Log4netFactory.cs
//    功能描述：基于Log4net的Logger工厂
//    创建标识：StanZhai 2012/07/19
//    文件版本：1.0.0.0
//
//============================================================

namespace ScrapingSpider.Logging
{
    public class Log4netFactory
    {
        public static ScrapingSpider.Core.Logging.ILogger CreateLogger()
        {
            return Log4netLogger.Instance;
        }
    }
}
