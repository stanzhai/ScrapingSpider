//============================================================
//
//    文件名　：ILogger.cs
//    功能描述：日志记录接口
//    创建标识：StanZhai 2012/12/23
//    文件版本：1.0.0.0
//
//============================================================

using System;

namespace ScrapingSpider.Core.Logging
{
    public interface ILogger
    {
        /// <summary>
        /// 添加Info级别日志
        /// </summary>
        /// <param name="msg">日志信息</param>
        void Info(string msg);

        /// <summary>
        /// 添加WARN级别日志
        /// </summary>
        /// <param name="msg">日志信息</param>
        void Warn(string msg);

        /// <summary>
        /// 添加ERROR级别日志
        /// </summary>
        /// <param name="msg">日志信息</param>
        void Error(string msg);

        /// <summary>
        /// 添加ERROR级别日志，记录异常信息
        /// </summary>
        /// <param name="ex">异常实例</param>
        void Error(Exception ex);
    }
}