//============================================================
//
//    文件名　：Log4netLogger.cs
//    功能描述：日志记录类
//    创建标识：StanZhai 2012/12/23
//    文件版本：1.0.0.0
//
//============================================================

using System;
using ScrapingSpider.Core.Logging;

namespace ScrapingSpider.Logging
{
    /// <summary>
    /// Controller日志辅助
    /// </summary>
    /// <author>inetfuture@qq.com</author>
    public class Log4netLogger : ILogger
    {
        private Log4netLogger()
        {
            //do nothing;
        }

        private readonly log4net.ILog _log = log4net.LogManager.GetLogger("Controller");
        private static ILogger _instance;

        // 单例模式，获取唯一示例
        public static ILogger Instance {
            get
            {
                if (_instance == null)
                {
                    // 初始化配置工作由网站或应用程序初始化
                    _instance = new Log4netLogger();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 添加Info级别日志
        /// </summary>
        /// <param name="msg">日志信息</param>
        public void Info(string msg)
        {
            _log.InfoFormat(msg);
        }

        /// <summary>
        /// 添加WARN级别日志
        /// </summary>
        /// <param name="msg">日志信息</param>
        public void Warn(string msg)
        {
            _log.WarnFormat(msg);
        }

        
        /// <summary>
        /// 添加ERROR级别日志
        /// </summary>
        /// <param name="msg">日志信息</param>
        public void Error(string msg)
        {
            _log.ErrorFormat(msg);
        }

        /// <summary>
        /// 添加ERROR级别日志，记录异常信息
        /// </summary>
        /// <param name="ex">异常实例</param>
        public void Error(Exception ex)
        {
            try
            {
                _log.Error(string.Format(ex.Message));
            }
            catch (Exception)
            {

            }
        }
    }
}