using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapingSpider.Core.Logging
{
    public class EmptyLogger : ILogger
    {
        public void Info(string msg)
        {
        }

        public void Warn(string msg)
        {
        }

        public void Error(string msg)
        {
        }

        public void Error(Exception ex)
        {
        }
    }
}
